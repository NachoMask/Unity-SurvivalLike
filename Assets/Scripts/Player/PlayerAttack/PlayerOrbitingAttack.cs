using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerOrbitingAttack : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] private PlayerAttack projectilePrefab;
    [SerializeField] private float projectileRotationOffset;

    [Header("Attack Stat")]
    [SerializeField, Min(PlayerAttack.MinimumDamage)] private int damage;
    [SerializeField, Min(PlayerAttack.MinimumCooldownDuration)] private float cooldownDuration;
    [SerializeField, Min(PlayerAttack.MinimumProjectileCount)] private int projectileCount;
    [SerializeField, Min(PlayerAttack.MinimumProjectileSpeed)] private float orbitAngularSpeed;
    [SerializeField, Min(PlayerAttack.MinimumAttackRange)] private float attackRange;
    [SerializeField, Min(PlayerAttack.MinimumKnockbackForce)] private int knockbackForce;
    [SerializeField, Min(PlayerAttack.MinimumActiveDuration)] private float activeDuration;
    [SerializeField, Min(PlayerAttack.MinimumRehitInterval)] private float rehitInterval;

    private readonly List<PlayerAttack> projectiles = new();

    private WaitForSeconds activeWait;
    private WaitForSeconds cooldownWait;

    private bool isAttacking = false;
    private float orbitAngle;

    private void Awake()
    {
        if (!TryValidateSettings(out string error))
        {
            Debug.LogError($"{nameof(PlayerOrbitingAttack)} {name}: {error}", this);
            enabled = false;
            return;
        }

        activeWait = new WaitForSeconds(activeDuration);
        cooldownWait = new WaitForSeconds(cooldownDuration);

        InitProjectiles();
    }

    private void InitProjectiles()
    {
        while (projectiles.Count < projectileCount)
        {
            CreateProjectile();
        }
    }

    private void OnEnable()
    {
        StartCoroutine(AttackCycle());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        isAttacking = false;

        HideProjectiles();
    }

    private void FixedUpdate()
    {
        if (!isAttacking) return;

        orbitAngle = Mathf.Repeat(
            orbitAngle - orbitAngularSpeed * Time.fixedDeltaTime,
            360f);

        PlaceProjectiles();
    }

    private IEnumerator AttackCycle()
    {
        while (true)
        {
            BeginAttack();
            yield return activeWait;

            EndAttack();
            yield return cooldownWait;
        }
    }

    private void HideProjectiles()
    {
        foreach (var projectile in projectiles)
        {
            projectile.gameObject.SetActive(false);
        }
    }

    private void BeginAttack()
    {
        orbitAngle = 0f;
        isAttacking = true;

        RefreshProjectiles();
    }

    private void EndAttack()
    {
        isAttacking = false;

        HideProjectiles();
    }

    private void RefreshProjectiles()
    {
        for (int i = 0; i < projectiles.Count; ++i)
        {
            bool activate = isAttacking && i < projectileCount;
            projectiles[i].gameObject.SetActive(activate);
        }

        PlaceProjectiles();
    }

    private void PlaceProjectiles()
    {
        float angleInterval = 360f / projectileCount;

        for (int i = 0; i < projectileCount; ++i)
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
            Mathf.Sin(radians)) * attackRange;
    }

    public void SetProjectileCount(int count)
    {
        int clampedCount = Mathf.Clamp(
            count,
            PlayerAttack.MinimumProjectileCount,
            PlayerAttack.MaximumProjectileCount);

        while (projectiles.Count < clampedCount)
        {
            CreateProjectile();
        }

        if (projectileCount == clampedCount) return;

        projectileCount = clampedCount;
        RefreshProjectiles();
    }

    private void CreateProjectile()
    {
        PlayerAttack projectile = Instantiate(projectilePrefab, transform);

        projectile.gameObject.SetActive(false);
        projectile.InitRehit(damage, knockbackForce, rehitInterval);

        projectiles.Add(projectile);
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

        error = null;
        return true;
    }
}