using UnityEngine;
using UnityEngine.Pool;

public class DamageTextSpawner : MonoBehaviour
{
    [SerializeField] private DamageText damageTextPrefab;

    private ObjectPool<DamageText> pool;

    [Header("Pool")]
    [SerializeField, Min(0)] private int defaultCapacity = 20;
    [SerializeField, Min(1)] private int maxPoolSize = 50;

    private void Awake()
    {
        if (!TryValidateSettings(out string error))
        {
            Debug.LogError($"{nameof(DamageTextSpawner)} {name}: {error}", this);
            enabled = false;
            return;
        }

        CreatePool();
    }

    private void CreatePool()
    {
        pool = new ObjectPool<DamageText>(
            CreateDamageText,
            null,
            OnDamageTextReturned,
            OnDamageTextDestroy,
            true, defaultCapacity, maxPoolSize);
    }

    public DamageText Spawn(int damage, Vector2 position)
    {
        DamageText damageText = pool.Get();
        damageText.Play(damage, position);

        return damageText;
    }

    private DamageText CreateDamageText()
    {
        DamageText damageText = Instantiate(damageTextPrefab, transform);
        damageText.Init(pool);
        damageText.gameObject.SetActive(false);

        return damageText;
    }

    private void OnDamageTextReturned(DamageText damageText)
    {
        damageText.ResetForPool();
        damageText.gameObject.SetActive(false);
    }

    private void OnDamageTextDestroy(DamageText damageText)
    {
        Destroy(damageText.gameObject);
    }

    private bool TryValidateSettings(out string error)
    {
        if (damageTextPrefab == null)
        {
            error = $"{nameof(damageTextPrefab)} is Invalid";
            return false;
        }

        error = null;
        return true;
    }
}
