using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PlayerLastMoveDirectionAttack : MonoBehaviour, IPlayerAttackUpgradeable
{
    [Header("Projectile")]
    [SerializeField] private PlayerAttack projectilePrefab;

    [Header("Attack Stat")]
    [SerializeField, Min(PlayerAttack.MinimumDamage)] private int damage;
    [SerializeField, Min(PlayerAttack.MinimumCooldownDuration)] private float cooldownDuration;
    [SerializeField, Min(PlayerAttack.MinimumProjectileCount)] private int projectileCount;
    [SerializeField, Min(PlayerAttack.MinimumFireInterval)] private float fireInterval;
    [SerializeField, Min(PlayerAttack.MinimumProjectileSpeed)] private float projectileSpeed;
    [SerializeField, Min(PlayerAttack.MinimumAttackRange)] private float attackRange;
    [SerializeField, Min(PlayerAttack.MinimumKnockbackForce)] private int knockbackForce;
    [SerializeField, Min(PlayerAttack.MinimumActiveDuration)] private float activeDuration;
    [SerializeField, Min(PlayerAttack.MinimumRehitInterval)] private float rehitInterval;

    private int damageBonus;
    private float cooldownReduction;
    private int projectileCountBonus;
    private float fireIntervalReduction;
    private float projectileSpeedMultiplier;
    private float attackRangeBonus;
    private int knockbackForceBonus;
    private float activeDurationBonus;
    private float rehitIntervalReduction;

    private PlayerCharacter playerCharacter;
    private PlayerStats playerStats;

    private ObjectPool<PlayerAttack> pool;
    private readonly List<PlayerAttack> activeProjectiles = new();

    [Header("Pool")]
    [SerializeField, Min(0)] private int defaultCapacity = 10;
    [SerializeField, Min(1)] private int maxPoolSize = 20;

    public int FinalDamage => Mathf.Max(
        PlayerAttack.MinimumDamage,
        Mathf.RoundToInt((damage + damageBonus) * playerStats.AttackMultiplier));
    public float FinalCooldown => Mathf.Max(
        PlayerAttack.MinimumCooldownDuration,
        (cooldownDuration - cooldownReduction) * playerStats.CooldownMultiplier);
    public int FinalProjectileCount => Mathf.Clamp(
        projectileCount + projectileCountBonus + playerStats.ProjectileCountBonus,
        PlayerAttack.MinimumProjectileCount,
        PlayerAttack.MaximumProjectileCount);
    public float FinalFireInterval => Mathf.Max(
        PlayerAttack.MinimumFireInterval,
        fireInterval - fireIntervalReduction);
    public float FinalProjectileSpeed => Mathf.Max(
        PlayerAttack.MinimumProjectileSpeed,
        projectileSpeed * (projectileSpeedMultiplier + playerStats.ProjectileSpeedMultiplier));
    public float FinalAttackRange => Mathf.Max(
        PlayerAttack.MinimumAttackRange,
        attackRange * (attackRangeBonus + playerStats.AttackRangeMultiplier));
    public int FinalKnockbackForce => Mathf.Max(
        PlayerAttack.MinimumKnockbackForce,
        knockbackForce + knockbackForceBonus);
    public float FinalActiveDuration => Mathf.Max(
        PlayerAttack.MinimumActiveDuration,
        (activeDuration + activeDurationBonus) * playerStats.ActiveDurationMultiplier);
    public float FinalRehitDuration => Mathf.Max(
        PlayerAttack.MinimumRehitInterval,
        rehitInterval - rehitIntervalReduction);

    [Serializable]
    private struct UpgradeLevel
    {
        [TextArea] public string description;

        [Min(0)] public int damageBonus;
        [Min(0f)] public float cooldownReduction;
        [Min(0)] public int projectileCountBonus;
        [Min(0f)] public float fireIntervalReduction;
        [Min(0f)] public float projectileSpeedMultiplier;
        [Min(0f)] public float attackRangeBonus;
        [Min(0)] public int knockbackForceBonus;
        [Min(0f)] public float activeDurationBonus;
        [Min(0f)] public float rehitIntervalReduction;
    }

    [Header("Upgrade")]
    [SerializeField] private UpgradeLevel[] upgradeLevels;

    private int level = 1;

    public int Level => level;
    public int MaxLevel => upgradeLevels.Length;
    public bool CanUpgrade => Level < MaxLevel;
    public string NextUpgradeDescription => CanUpgrade ? upgradeLevels[level].description : string.Empty;

    private void Awake()
    {
        playerCharacter = GetComponentInParent<PlayerCharacter>();
        playerStats = GetComponentInParent<PlayerStats>();

        if (!TryValidateSettings(out string error))
        {
            Debug.LogError($"{nameof(PlayerRangedAttack)} {name}: {error}", this);
            enabled = false;
            return;
        }

        CreatePool();
    }

    private void CreatePool()
    {
        pool = new ObjectPool<PlayerAttack>(
            CreateProjectile,
            OnProjectileTake,
            OnProjectileReturned,
            OnProjectileDestroy,
            true, defaultCapacity, maxPoolSize);
    }

    private PlayerAttack CreateProjectile()
    {
        PlayerAttack projectile = Instantiate(projectilePrefab, playerStats.transform);

        return projectile;
    }

    private void OnProjectileTake(PlayerAttack projectile)
    {
        projectile.InitRehit(FinalDamage, FinalKnockbackForce, FinalRehitDuration);

        projectile.transform.SetParent(null);
        projectile.transform.position = transform.position;
        projectile.gameObject.SetActive(true);

        activeProjectiles.Add(projectile);
    }

    private void OnProjectileReturned(PlayerAttack projectile)
    {
        activeProjectiles.Remove(projectile);
        projectile.gameObject.SetActive(false);
    }

    private void OnProjectileDestroy(PlayerAttack projectile)
    {
        Destroy(projectile.gameObject);
    }

    private void OnEnable()
    {
        StartCoroutine(FireProjectiles());
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        for (int i = activeProjectiles.Count - 1; i >= 0; --i)
        {
            PlayerAttack attack = activeProjectiles[i];

            if (attack != null)
                pool.Release(activeProjectiles[i]);
            else
                activeProjectiles.RemoveAt(i);
        }
    }

    private IEnumerator FireProjectiles()
    {
        while (true)
        {
            int currentProjectileCount = FinalProjectileCount;

            while (currentProjectileCount > 0)
            {
                Vector2 direction = playerCharacter.LastMoveDirection;

                StartCoroutine(FireProjectile(direction));

                --currentProjectileCount;
                if (currentProjectileCount <= 0)
                    yield return new WaitForSeconds(FinalCooldown);
                else
                    yield return new WaitForSeconds(FinalFireInterval);
            }
        }
    }

    private IEnumerator FireProjectile(Vector2 direction)
    {
        direction = direction.normalized;

        PlayerAttack projectile = pool.Get();
        Rigidbody2D body = projectile.GetComponent<Rigidbody2D>();
        projectile.transform.rotation = Quaternion.FromToRotation(Vector2.up, direction);

        Vector2 start = body.position;
        Vector2 outwardEnd = start + direction * FinalAttackRange;
        var fixedUpdate =  new WaitForFixedUpdate();

        while (true)
        {
            float step = FinalProjectileSpeed * Time.fixedDeltaTime;

            if (Vector2.Distance(body.position, outwardEnd) <= step)
            {
                body.MovePosition(outwardEnd);
                yield return fixedUpdate;
                break;
            }

            body.MovePosition(Vector2.MoveTowards(body.position, outwardEnd, step));
            yield return fixedUpdate;
        }

        yield return new WaitForSeconds(FinalActiveDuration);
        pool.Release(projectile);

        //while (true)
        //{
        //    float step = FinalProjectileSpeed * Time.fixedDeltaTime;

        //    if (Vector2.Distance(body.position, end) <= step)
        //    {
        //        pool.Release(projectile);
        //        yield break;
        //    }

        //    body.MovePosition(Vector2.MoveTowards(body.position, end, step));
        //    yield return fixedUpdate;
        //}
    }

    public bool TryUpgrade()
    {
        if (!CanUpgrade) return false;

        UpgradeLevel upgrade = upgradeLevels[level];

        damageBonus += upgrade.damageBonus;
        cooldownReduction += upgrade.cooldownReduction;
        projectileCountBonus += upgrade.projectileCountBonus;
        fireIntervalReduction += upgrade.fireIntervalReduction;
        projectileSpeedMultiplier += upgrade.projectileSpeedMultiplier;
        attackRangeBonus += upgrade.attackRangeBonus;
        knockbackForceBonus += upgrade.knockbackForceBonus;
        activeDurationBonus += upgrade.activeDurationBonus;
        rehitIntervalReduction += upgrade.rehitIntervalReduction;

        ++level;
        return true;
    }

    private bool TryValidateSettings(out string error)
    {
        if (projectilePrefab == null)
        {
            error = $"{nameof(projectilePrefab)} is invalid";
            return false;
        }
        if (!projectilePrefab.TryGetComponent(out Rigidbody2D _))
        {
            error = $"{nameof(projectilePrefab)} must have {nameof(Rigidbody2D)}";
            return false;
        }

        if (damage < PlayerAttack.MinimumDamage)
        {
            error = $"{nameof(damage)} must be at least {PlayerAttack.MinimumDamage}";
            return false;
        }
        if (cooldownDuration < PlayerAttack.MinimumCooldownDuration)
        {
            error = $"{nameof(cooldownDuration)} must be at least {PlayerAttack.MinimumCooldownDuration}";
            return false;
        }
        if (projectileCount < PlayerAttack.MinimumProjectileCount || projectileCount > PlayerAttack.MaximumProjectileCount)
        {
            error = $"{nameof(projectileCount)} must be between " +
                    $"{PlayerAttack.MinimumProjectileCount} and {PlayerAttack.MaximumProjectileCount}";
            return false;
        }
        if (fireInterval < PlayerAttack.MinimumFireInterval)
        {
            error = $"{nameof(fireInterval)} must be at least {PlayerAttack.MinimumFireInterval}";
            return false;
        }
        if (projectileSpeed < PlayerAttack.MinimumProjectileSpeed)
        {
            error = $"{nameof(projectileSpeed)} must be at least {PlayerAttack.MinimumProjectileSpeed}";
            return false;
        }
        if (attackRange < PlayerAttack.MinimumAttackRange)
        {
            error = $"{nameof(attackRange)} must be at least {PlayerAttack.MinimumAttackRange}";
            return false;
        }
        if (knockbackForce < PlayerAttack.MinimumKnockbackForce)
        {
            error = $"{nameof(knockbackForce)} must be at least {PlayerAttack.MinimumKnockbackForce}";
            return false;
        }
        if (rehitInterval < PlayerAttack.MinimumRehitInterval)
        {
            error = $"{nameof(rehitInterval)} must be at least {PlayerAttack.MinimumRehitInterval}";
            return false;
        }

        if (playerCharacter == null)
        {
            error = $"{nameof(playerCharacter)} was not found in parents";
            return false;
        }
        if (playerStats == null)
        {
            error = $"{nameof(playerStats)} was not found in parents";
            return false;
        }

        if (defaultCapacity < 0)
        {
            error = $"{nameof(defaultCapacity)} must be at least zero";
            return false;
        }
        if (maxPoolSize < 1)
        {
            error = $"{nameof(maxPoolSize)} must be at least 1";
            return false;
        }

        if (upgradeLevels == null || upgradeLevels.Length == 0)
        {
            error = $"{nameof(upgradeLevels)} must contain at least one level.";
            return false;
        }

        for (int i = 1; i < upgradeLevels.Length; ++i)
        {
            if (string.IsNullOrWhiteSpace(upgradeLevels[i].description))
            {
                error = $"{nameof(upgradeLevels)}[{i}].description is invalid.";
                return false;
            }
        }

        error = null;
        return true;
    }
}
