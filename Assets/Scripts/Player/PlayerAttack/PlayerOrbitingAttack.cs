using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerOrbitingAttack : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] private PlayerAttack projectilePrefab;
    [SerializeField] private float projectileRotationOffset;

    private PlayerStats playerStats;

    [Header("Attack Stat")]
    [SerializeField, Min(PlayerAttack.MinimumDamage)] private int damage;
    [SerializeField, Min(PlayerAttack.MinimumCooldownDuration)] private float cooldownDuration;
    [SerializeField, Min(PlayerAttack.MinimumProjectileCount)] private int projectileCount;
    [SerializeField, Min(PlayerAttack.MinimumProjectileSpeed)] private float orbitAngularSpeed;
    [SerializeField, Min(PlayerAttack.MinimumAttackRange)] private float attackRange;
    [SerializeField, Min(PlayerAttack.MinimumKnockbackForce)] private int knockbackForce;
    [SerializeField, Min(PlayerAttack.MinimumActiveDuration)] private float activeDuration;
    [SerializeField, Min(PlayerAttack.MinimumRehitInterval)] private float rehitInterval;

    private int damageBonus;
    private float cooldownReduction;
    private int projectileCountBonus;
    private float orbitAngularSpeedMultiplier;
    private float attackRangeMultiplier;
    private int knockbackForceBonus;
    private float activeDurationBonus;
    private float rehitIntervalBonus;

    private readonly List<PlayerAttack> projectiles = new();
    private int activeProjectileCount;

    private bool isAttacking = false;
    private float orbitAngle;

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
    public float FinalOrbitAngularSpeed => Mathf.Max(
        PlayerAttack.MinimumProjectileSpeed,
        orbitAngularSpeed * (orbitAngularSpeedMultiplier + playerStats.ProjectileSpeedMultiplier));
    public float FinalAttackRange => Mathf.Max(
        PlayerAttack.MinimumAttackRange,
        attackRange * (attackRangeMultiplier + playerStats.AttackRangeMultiplier));
    public float FinalActiveDuration => Mathf.Max(
        PlayerAttack.MinimumActiveDuration,
        (activeDuration + activeDurationBonus) * playerStats.ActiveDurationMultiplier);

    [Serializable]
    private struct UpgradeLevel
    {
        [Min(0)] public int damageBonus;
        [Min(0f)] public float cooldownReduction;
        [Min(0)] public int projectileCountBonus;
        [Min(0f)] public float orbitAngularSpeedMultiplier;
        [Min(0f)] public float attackRangeMultiplier;
        [Min(0)] public int knockbackForceBonus;
        [Min(0f)] public float activeDurationBonus;
        [Min(0f)] public float rehitIntervalBonus;
    }

    [Header("Upgrade")]
    [SerializeField] private UpgradeLevel[] upgradeLevels;

    private int level = 1;

    private void Awake()
    {
        playerStats = GetComponentInParent<PlayerStats>();

        if (!TryValidateSettings(out string error))
        {
            Debug.LogError($"{nameof(PlayerOrbitingAttack)} {name}: {error}", this);
            enabled = false;
            return;
        }

        EnsureProjectileCapacity(FinalProjectileCount);
    }

    private void EnsureProjectileCapacity(int requireCount)
    {
        while (projectiles.Count < requireCount)
        {
            PlayerAttack projectile = Instantiate(projectilePrefab, transform);

            projectile.gameObject.SetActive(false);
            projectiles.Add(projectile);
        }
    }

    private void OnEnable()
    {
        playerStats.LevelChanged += BindUpgrade;

        StartCoroutine(AttackCycle());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        isAttacking = false;

        EndAttack();
    }

    private void FixedUpdate()
    {
        if (!isAttacking) return;

        orbitAngle = Mathf.Repeat(
            orbitAngle - FinalOrbitAngularSpeed * Time.fixedDeltaTime,
            360f);

        PlaceProjectiles();
    }

    private IEnumerator AttackCycle()
    {
        while (true)
        {
            BeginAttack();
            yield return new WaitForSeconds(FinalActiveDuration);

            EndAttack();
            yield return new WaitForSeconds(FinalCooldown);
        }
    }

    private void BeginAttack()
    {
        orbitAngle = 0f;
        isAttacking = true;

        activeProjectileCount = FinalProjectileCount;
        EnsureProjectileCapacity(activeProjectileCount);

        for (int i = 0; i < activeProjectileCount; ++i)
        {
            PlayerAttack projectile = projectiles[i];

            projectile.InitRehit(FinalDamage, knockbackForce, rehitInterval);
            projectile.transform.localScale =
                projectilePrefab.transform.localScale * (1 + FinalAttackRange/3);
            projectile.gameObject.SetActive(true);
        }

        PlaceProjectiles();
    }

    private void EndAttack()
    {
        isAttacking = false;

        for (int i = 0; i < activeProjectileCount; ++i)
        {
            projectiles[i].gameObject.SetActive(false);
        }
    }

    private void PlaceProjectiles()
    {
        float angleInterval = 360f / activeProjectileCount;

        for (int i = 0; i < activeProjectileCount; ++i)
        {
            float projectileAngle = orbitAngle + angleInterval * i;

            PlaceProjectile(projectiles[i].transform, projectileAngle);
        }
    }

    private void PlaceProjectile(Transform projectileTransform, float projectileAngle)
    {
        float tangentOffset = 90f;
        float radians = projectileAngle * Mathf.Deg2Rad;

        projectileTransform.localRotation = Quaternion.Euler(
            0f, 0f,
            projectileAngle + tangentOffset + projectileRotationOffset);

        projectileTransform.localPosition = new Vector2(
            Mathf.Cos(radians),
            Mathf.Sin(radians)) * FinalAttackRange;
    }

    private void BindUpgrade(int level)
    {
        TryUpgrade();
    }

    private bool TryUpgrade()
    {
        if (level >= upgradeLevels.Length) return false;

        UpgradeLevel upgrade = upgradeLevels[level];

        damageBonus += upgrade.damageBonus;
        cooldownReduction += upgrade.cooldownReduction;
        projectileCountBonus += upgrade.projectileCountBonus;
        orbitAngularSpeedMultiplier += upgrade.orbitAngularSpeedMultiplier;
        attackRangeMultiplier += upgrade.attackRangeMultiplier;
        knockbackForceBonus += upgrade.knockbackForceBonus;
        activeDurationBonus += upgrade.activeDurationBonus;
        rehitIntervalBonus += upgrade.rehitIntervalBonus;

        ++level;
        Debug.Log($"{name} is now LV.{level}.");
        return true;
    }

    private bool TryValidateSettings(out string error)
    {
        if (projectilePrefab == null)
        {
            error = $"{nameof(projectilePrefab)} is invalid";
            return false;
        }

        if (projectileCount < PlayerAttack.MinimumProjectileCount || projectileCount > PlayerAttack.MaximumProjectileCount)
        {
            error = $"{nameof(projectileCount)} must be between " +
                    $"{PlayerAttack.MinimumProjectileCount} and {PlayerAttack.MaximumProjectileCount}";
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
        if (activeDuration < PlayerAttack.MinimumActiveDuration)
        {
            error = $"{nameof(activeDuration)} must be at least {PlayerAttack.MinimumActiveDuration}";
            return false;
        }
        if (rehitInterval < PlayerAttack.MinimumRehitInterval)
        {
            error = $"{nameof(rehitInterval)} must be at least {PlayerAttack.MinimumRehitInterval}";
            return false;
        }
        if (orbitAngularSpeed < PlayerAttack.MinimumProjectileSpeed)
        {
            error = $"{nameof(orbitAngularSpeed)} must be at least {PlayerAttack.MinimumProjectileSpeed}";
            return false;
        }

        if (playerStats == null)
        {
            error = $"{nameof(playerStats)} was not found in parents";
            return false;
        }

        error = null;
        return true;
    }
}