using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

public class PlayerRangedAttack : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] private PlayerAttack projectilePrefab;

    [Header("Attack Stat")]
    [SerializeField, Min(PlayerAttack.MinimumDamage)] private int damage;
    [SerializeField, Min(PlayerAttack.MinimumCooldownDuration)] private float cooldownDuration;
    [SerializeField, Min(PlayerAttack.MinimumProjectileCount)] private int projectileCount;
    [SerializeField, Min(PlayerAttack.MinimumFireInterval)] private float fireInterval;
    [SerializeField, Min(PlayerAttack.MinimumProjectileSpeed)] private float projectileSpeed;
    [SerializeField, Min(PlayerAttack.MinimumAttackRange)] private float projectileSizeMultiplier;
    [SerializeField, Min(PlayerAttack.MinimumKnockbackForce)] private int knockbackForce;
    [SerializeField, Min(PlayerAttack.MinimumPenetrationCount)] private int penetrationCount;

    private PlayerStats playerStats;
    private PlayerTargetScanner targetScanner;

    private WaitForSeconds fireWait;

    private ObjectPool<PlayerAttack> pool;

    [Header("Pool")]
    [SerializeField, Min(0)] private int defaultCapacity = 10;
    [SerializeField, Min(1)] private int maxPoolSize = 20;

    public int FinalDamage => Mathf.Max(
        PlayerAttack.MinimumDamage,
        Mathf.RoundToInt(damage * playerStats.AttackMultiplier));
    public float FinalCooldown => Mathf.Max(
        PlayerAttack.MinimumCooldownDuration,
        cooldownDuration * playerStats.CooldownMultiplier);
    public int FinalProjectileCount => Mathf.Clamp(
        projectileCount + playerStats.ProjectileCountBonus,
        PlayerAttack.MinimumProjectileCount,
        PlayerAttack.MaximumProjectileCount);
    public float FinalProjectileSpeed => Mathf.Max(
        PlayerAttack.MinimumProjectileSpeed,
        projectileSpeed * playerStats.ProjectileSpeedMultiplier);
    public float FinalProjectileSizeMultiplier => Mathf.Max(
        PlayerAttack.MinimumAttackRange,
        projectileSizeMultiplier * playerStats.AttackRangeMultiplier);

    private void Awake()
    {
        playerStats = GetComponentInParent<PlayerStats>();
        targetScanner = GetComponentInParent<PlayerTargetScanner>();

        if (!TryValidateSettings(out string error))
        {
            Debug.LogError($"{nameof(PlayerRangedAttack)} {name}: {error}", this);
            enabled = false;
            return;
        }

        fireWait = new WaitForSeconds(fireInterval);

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
        projectile.InitPenetration(pool, FinalDamage, knockbackForce, penetrationCount);

        projectile.transform.SetParent(null);
        projectile.transform.position = transform.position;
        projectile.transform.localScale =
            projectilePrefab.transform.localScale * FinalProjectileSizeMultiplier;

        projectile.gameObject.SetActive(true);
    }

    private void OnProjectileReturned(PlayerAttack projectile)
    {
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
    }

    private IEnumerator FireProjectiles()
    {
        while (true)
        {
            int currentProjectileCount = FinalProjectileCount;

            Vector3? targetPosition = null;

            while (currentProjectileCount > 0)
            {
                if (targetScanner.NearestTarget != null)
                {
                    targetPosition = targetScanner.NearestTarget.position;
                }

                if (targetPosition == null)
                {
                    yield return null;
                    continue;
                }

                FireProjectile(targetPosition.Value - transform.position);

                --currentProjectileCount;
                if (currentProjectileCount <= 0)
                    yield return new WaitForSeconds(FinalCooldown);
                else
                    yield return fireWait;
            }
        }
    }

    private void FireProjectile(Vector2 direction)
    {
        if (direction == Vector2.zero)
            direction = Vector2.up;
        else
            direction = direction.normalized;

        PlayerAttack projectile = pool.Get();
        projectile.transform.rotation = Quaternion.FromToRotation(Vector2.up, direction);
        projectile.GetComponent<Rigidbody2D>().linearVelocity = direction * FinalProjectileSpeed;
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
        if (projectileSizeMultiplier < PlayerAttack.MinimumAttackRange)
        {
            error = $"{nameof(projectileSizeMultiplier)} must be at least {PlayerAttack.MinimumAttackRange}";
            return false;
        }
        if (knockbackForce < PlayerAttack.MinimumKnockbackForce)
        {
            error = $"{nameof(knockbackForce)} must be at least {PlayerAttack.MinimumKnockbackForce}";
            return false;
        }
        if (penetrationCount < PlayerAttack.MinimumPenetrationCount)
        {
            error = $"{nameof(penetrationCount)} must be at least {PlayerAttack.MinimumPenetrationCount}";
            return false;
        }

        if (playerStats == null)
        {
            error = $"{nameof(playerStats)} was not found in parents";
            return false;
        }
        if (targetScanner == null)
        {
            error = $"{nameof(targetScanner)} was not found in parents";
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

        error = null;
        return true;
    }
}
