using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PlayerAttack : MonoBehaviour
{
    private enum HitMode
    {
        Uninitialized,
        Penetration,
        Rehit
    }

    public const int MinimumDamage = 1;
    public const float MinimumCooldownDuration = 0.1f;
    public const int MinimumProjectileCount = 1;
    public const float MinimumFireInterval = 0.1f;
    public const float MinimumProjectileSpeed = 0.1f;
    public const float MinimumAttackRange = 0.1f;
    public const int MinimumKnockbackForce = 0;
    public const int MinimumPenetrationCount = 1;
    public const float MinimumActiveDuration = 0.1f;
    public const float MinimumRehitInterval = 0.1f;

    public const int MaximumProjectileCount = 20;

    private const string RequiredLayerName = "PlayerAttack";
    private const string DetectorLayerName = "ChunkRepositionDetector";

    private Collider2D attackCollider;
    private int detectorLayer;

    private HitMode hitMode;
    private int damage;
    private int knockbackForce;
    private int remainingPenetration;
    private float rehitInterval;

    private HashSet<EnemyCharacter> hitEnemies = new();
    private Dictionary<EnemyCharacter, float> lastHitTimes = new();

    private IObjectPool<PlayerAttack> ownerPool;
    private bool canProcessHit = false;

    private void Awake()
    {
        attackCollider = GetComponent<Collider2D>();

        if (!TryValidateSettings(out string error))
        {
            Debug.LogError($"{nameof(PlayerAttack)} {name}: {error}");

            if (attackCollider != null)
                attackCollider.enabled = false;

            enabled = false;
        }

        detectorLayer = LayerMask.NameToLayer(DetectorLayerName);
    }

    public void InitPenetration(IObjectPool<PlayerAttack> pool, int damage, int knockbackForce, int penetration)
    {
        if (pool == null)
        {
            throw new System.ArgumentNullException(nameof(pool));
        }

        hitMode = HitMode.Penetration;

        ownerPool = pool;
        this.damage = Mathf.Max(damage, MinimumDamage);
        this.knockbackForce = Mathf.Max(knockbackForce, MinimumKnockbackForce);
        remainingPenetration = Mathf.Max(penetration, MinimumPenetrationCount);

        canProcessHit = true;
    }

    public void InitRehit(int damage, int knockbackForce, float rehitInterval)
    {
        hitMode = HitMode.Rehit;

        this.damage = Mathf.Max(damage, MinimumDamage);
        this.knockbackForce = Mathf.Max(knockbackForce, MinimumKnockbackForce);
        this.rehitInterval = Mathf.Max(rehitInterval, MinimumRehitInterval);

        canProcessHit = true;
    }

    private void OnDisable()
    {
        hitEnemies.Clear();
        lastHitTimes.Clear();
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (!canProcessHit || !Application.isPlaying) return;

        ProcessContact(collider);
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        if (!canProcessHit || !Application.isPlaying) return;

        if (hitMode == HitMode.Rehit)
        {
            ProcessContact(collider);
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (!canProcessHit || !Application.isPlaying) return;

        if (collider.TryGetComponent(out EnemyCharacter enemy))
        {
            if (lastHitTimes.ContainsKey(enemy))
                lastHitTimes.Remove(enemy);
        }

        if (collider.gameObject.layer == detectorLayer &&
            collider.isActiveAndEnabled)
        {
            ReleaseToPool();
        }
    }

    private void ProcessContact(Collider2D collider)
    {
        switch (hitMode)
        {
            case HitMode.Penetration:
                ProcessPenetrationContact(collider);
                break;
            case HitMode.Rehit:
                ProcessRehitContact(collider);
                break;
        }
    }

    private void ProcessPenetrationContact(Collider2D collider)
    {
        if (!collider.TryGetComponent(out EnemyCharacter enemy)) return;
        if (hitEnemies.Contains(enemy)) return;

        hitEnemies.Add(enemy);
        HitEnemy(enemy);

        --remainingPenetration;

        if (remainingPenetration <= 0)
        {
            ReleaseToPool();
        }
    }

    private void ProcessRehitContact(Collider2D collider)
    {
        if (!collider.TryGetComponent(out EnemyCharacter enemy)) return;

        float currentTime = Time.time;

        if (lastHitTimes.TryGetValue(enemy, out float lastHitTime) &&
            currentTime - lastHitTime < rehitInterval) return;

        lastHitTimes[enemy] = currentTime;
        HitEnemy(enemy);
    }

    private void HitEnemy(EnemyCharacter enemy)
    {
        Vector2 direction = enemy.transform.position - transform.position;

        enemy.ApplyKnockback(direction, knockbackForce);
        enemy.TakeDamage(damage);
    }

    private void ReleaseToPool()
    {
        if (!canProcessHit || ownerPool == null) return;

        canProcessHit = false;
        ownerPool.Release(this);
    }

    private bool TryValidateSettings(out string error)
    {
        if (attackCollider == null || !attackCollider.enabled)
        {
            error = $"{nameof(Collider2D)} is Invalid";
            return false;
        }

        if (!attackCollider.isTrigger)
        {
            error = $"{nameof(Collider2D)} must be configured as a trigger";
            return false;
        }

        int requiredLayer = LayerMask.NameToLayer(RequiredLayerName);

        if (requiredLayer < 0)
        {
            error = $"{RequiredLayerName} is not defined";
            return false;
        }

        int detectorLayer = LayerMask.NameToLayer(DetectorLayerName);

        if (detectorLayer < 0)
        {
            error = $"{DetectorLayerName} is not defined";
            return false;
        }

        if (gameObject.layer != requiredLayer)
        {
            error = $"{name} must use Layer {RequiredLayerName}";
            return false;
        }

        error = null;
        return true;
    }
}
