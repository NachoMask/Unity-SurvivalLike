using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public const int MinimumDamage = 1;
    public const float MinimumCooldownDuration = 0.1f;
    public const int MinimumProjectileCount = 1;
    public const float MinimumProjectileSpeed = 0.1f;
    public const float MinimumAttackRange = 0.1f;
    public const float MinimumActiveDuration = 0.1f;
    public const float MinimumRehitInterval = 0.1f;

    public const int MaximumProjectileCount = 20;

    private const string RequiredLayerName = "PlayerAttack";

    private Collider2D attackCollider;

    private int damage;
    private float rehitInterval;

    private Dictionary<EnemyCharacter, float> lastHitTimes = new();

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
    }

    public void Init(int damage, float rehitInterval)
    {
        this.damage = Mathf.Max(damage, MinimumDamage);
        this.rehitInterval = Mathf.Max(rehitInterval, MinimumRehitInterval);
    }

    private void OnDisable()
    {
        lastHitTimes.Clear();
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        ProcessContact(collider);
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        ProcessContact(collider);
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (!collider.TryGetComponent(out EnemyCharacter enemy)) return;

        if (lastHitTimes.ContainsKey(enemy))
            lastHitTimes.Remove(enemy);
    }

    private void ProcessContact(Collider2D collider)
    {
        if (!collider.TryGetComponent(out EnemyCharacter enemy)) return;

        float currentTime = Time.time;

        if (lastHitTimes.TryGetValue(enemy, out float lastHitTime) &&
            currentTime - lastHitTime < rehitInterval) return;

        lastHitTimes[enemy] = currentTime;
        enemy.TakeDamage(damage);
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

        if (gameObject.layer != requiredLayer)
        {
            error = $"{name} must use Layer {RequiredLayerName}";
            return false;
        }

        error = null;
        return true;
    }
}
