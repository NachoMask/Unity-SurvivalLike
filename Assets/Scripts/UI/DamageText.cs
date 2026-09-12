using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;

[RequireComponent(
    typeof(TextMeshProUGUI))]
public class DamageText : MonoBehaviour
{
    private IObjectPool<DamageText> ownerPool;

    private TextMeshProUGUI damageText;
    private Vector3 defaultScale;

    [SerializeField, Min(0.01f)] private float growDuration = 0.3f;
    [SerializeField, Min(0.01f)] private float fadeDuration = 0.3f;
    [SerializeField, Min(1f)] private float maxScaleMultiplier = 1.3f;
    [SerializeField, Min(1f)] private float moveSpeed = 100f;

    private void Awake()
    {
        damageText = GetComponent<TextMeshProUGUI>();
        defaultScale = transform.localScale;
    }

    public void Init(IObjectPool<DamageText> ownerPool)
    {
        this.ownerPool = ownerPool;
    }

    public void Play(int damage, Vector2 position)
    {
        StopAllCoroutines();

        damageText.text = $"{damage}";
        transform.position = position;
        transform.localScale = defaultScale;
        damageText.alpha = 1f;

        gameObject.SetActive(true);
        StartCoroutine(PlayEffect());
    }

    private IEnumerator PlayEffect()
    {
        Vector3 maxScale = defaultScale * maxScaleMultiplier;
        Vector3 moveDirection = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        moveDirection.Normalize();

        float elapsedTime = 0f;

        while (elapsedTime < growDuration)
        {
            elapsedTime += Time.deltaTime;

            float progress = Mathf.Clamp01(elapsedTime / growDuration);

            transform.localScale =
                Vector3.Lerp(defaultScale, maxScale, progress);

            transform.localPosition += moveDirection * moveSpeed * Time.deltaTime;

            yield return null;
        }

        elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / fadeDuration);

            transform.localScale =
                Vector3.Lerp(maxScale, defaultScale, progress);

            damageText.alpha = 1f - progress;

            yield return null;
        }

        ownerPool.Release(this);
    }

    public void ResetForPool()
    {
        StopAllCoroutines();

        transform.localScale = defaultScale;
        damageText.alpha = 1f;
        gameObject.SetActive(false);
    }

    internal bool TryValidateSettings(out string error)
    {
        if (growDuration < 0.01f)
        {
            error = $"{nameof(growDuration)} must be at least 0.01";
            return false;
        }
        if (fadeDuration < 0.01f)
        {
            error = $"{nameof(fadeDuration)} must be at least 0.01";
            return false;
        }
        if (maxScaleMultiplier < 1f)
        {
            error = $"{nameof(maxScaleMultiplier)} must be at least 1.0";
            return false;
        }
        if (moveSpeed < 1f)
        {
            error = $"{nameof(moveSpeed)} must be at least 1.0";
            return false;
        }

        error = null;
        return true;
    }
}
