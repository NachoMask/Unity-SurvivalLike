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
        if (damage <= 0)
            throw new System.ArgumentOutOfRangeException(nameof(damage));

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
        float maxScaleMultiplier = 1.3f;
        float growDuration = 0.3f;
        float fadeDuration = 0.5f;
        Vector3 maxScale = defaultScale * maxScaleMultiplier;

        float elapsedTime = 0f;

        while (elapsedTime < growDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / growDuration);
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);

            transform.localScale =
                Vector3.LerpUnclamped(defaultScale, maxScale, easedProgress);

            yield return null;
        }

        transform.localScale = maxScale;

        elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / fadeDuration);
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);

            transform.localScale =
                Vector3.LerpUnclamped(maxScale, defaultScale, easedProgress);

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
}
