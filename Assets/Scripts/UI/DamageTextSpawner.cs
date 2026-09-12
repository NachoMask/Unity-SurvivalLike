using UnityEngine;
using UnityEngine.Pool;

public class DamageTextSpawner : MonoBehaviour
{
    [SerializeField] private DamageText damageTextPrefab;

    private ObjectPool<DamageText> pool;

    [Header("Pool")]
    [SerializeField, Min(0)] private int defaultCapacity = 10;
    [SerializeField, Min(1)] private int maxPoolSize = 20;

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
        if (damage <= 0)
            throw new System.ArgumentOutOfRangeException(nameof(damage));

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
        if (!damageTextPrefab.TryValidateSettings(out string prefabError))
        {
            error = $"{nameof(damageTextPrefab)} {damageTextPrefab.name} {prefabError}";
            return false;
        }

        if (defaultCapacity < 0)
        {
            error = $"{nameof(defaultCapacity)} is Invalid";
            return false;
        }
        if (maxPoolSize < 1)
        {
            error = $"{nameof(maxPoolSize)} is Invalid";
            return false;
        }

        error = null;
        return true;
    }
}
