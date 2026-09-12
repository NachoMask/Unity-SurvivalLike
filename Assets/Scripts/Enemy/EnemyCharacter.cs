using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

[RequireComponent(
    typeof(SpriteRenderer),
    typeof(Rigidbody2D))]
public class EnemyCharacter : MonoBehaviour
{
    private IObjectPool<EnemyCharacter> ownerPool;
    private DamageTextSpawner damageTextSpawner;
    private Action<int> onDefeated;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D body;
    private Collider2D enemyCollider;

    private Rigidbody2D moveTarget;

    private int currentHp;
    private float moveSpeed;
    private int expReward;

    private bool isSpawned = false;
    private bool isInKnockback = false;

    private MaterialPropertyBlock materialPropertyBlock;
    private static readonly int HitFlashFactorId = Shader.PropertyToID("_HitFlashFactor");
    private static readonly int DissolveAmountId = Shader.PropertyToID("_DissolveAmount");
    private const float HitFlashTime = 0.1f;
    private const float DissolveTime = 0.75f;

    private Coroutine hitFlashCoroutine = null;
    private WaitForSeconds hitFlashWait;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        body = GetComponent<Rigidbody2D>();

        if (!TryGetComponent(out Collider2D collider))
        {
            throw new MissingComponentException(
                $"{nameof(EnemyCharacter)} {name} : {nameof(Collider2D)} component is missing.");
        }
        enemyCollider = collider;

        materialPropertyBlock = new MaterialPropertyBlock();
        spriteRenderer.GetPropertyBlock(materialPropertyBlock);

        hitFlashWait = new WaitForSeconds(HitFlashTime);
    }

    public void Init(IObjectPool<EnemyCharacter> pool, DamageTextSpawner damageTextSpawner, Action<int> onDefeated)
    {
        if (pool == null)
        {
            throw new ArgumentNullException(nameof(pool));
        }
        if (damageTextSpawner == null)
        {
            throw new ArgumentNullException(nameof(damageTextSpawner));
        }
        if (onDefeated == null)
        {
            throw new ArgumentNullException(nameof(onDefeated));
        }

        ownerPool = pool;
        this.damageTextSpawner = damageTextSpawner;
        this.onDefeated = onDefeated;
    }

    public void Spawn(EnemyData data, Rigidbody2D target, Vector2 position)
    {
        isSpawned = true;

        transform.position = position;
        body.simulated = true;
        enemyCollider.enabled = true;

        currentHp = data.MaxHp;
        moveSpeed = data.MoveSpeed;
        expReward = data.Exp;
        moveTarget = target;

        materialPropertyBlock.SetFloat(HitFlashFactorId, 0f);
        materialPropertyBlock.SetFloat(DissolveAmountId, 0f);
        spriteRenderer.SetPropertyBlock(materialPropertyBlock);

        gameObject.SetActive(true);
    }

    private void FixedUpdate()
    {
        if (!isSpawned) return;

        if (isInKnockback)
        {
            isInKnockback = false;
            return;
        }

        Vector2 nextPosition =
            Vector2.MoveTowards(body.position, moveTarget.position, moveSpeed * Time.fixedDeltaTime);

        body.MovePosition(nextPosition);
    }

    private void LateUpdate()
    {
        if (!isSpawned) return;

        if (moveTarget.position.x != body.position.x)
            spriteRenderer.flipX = moveTarget.position.x > body.position.x;
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0)
        {
            throw new System.ArgumentOutOfRangeException(nameof(damage));
        }

        damageTextSpawner.Spawn(damage, transform.position);

        HitFlash();
        currentHp = Mathf.Max(0, currentHp - damage);

        if (currentHp == 0)
        {
            Die();
        }
    }

    public void ApplyKnockback(Vector2 direction, int knockbackForce)
    {
        if (knockbackForce < 0)
        {
            throw new System.ArgumentOutOfRangeException(nameof(knockbackForce));
        }
        if (knockbackForce == 0 || direction == Vector2.zero) return;

        isInKnockback = true;
        body.linearVelocity = Vector2.zero;
        body.AddForce(direction.normalized * knockbackForce, ForceMode2D.Impulse);
    }

    private void HitFlash()
    {
        if (hitFlashCoroutine != null)
            StopCoroutine(hitFlashCoroutine);

        materialPropertyBlock.SetFloat(HitFlashFactorId, 1f);
        spriteRenderer.SetPropertyBlock(materialPropertyBlock);

        hitFlashCoroutine = StartCoroutine(RecoveryHitFlash());
    }

    private IEnumerator RecoveryHitFlash()
    {
        yield return hitFlashWait;

        materialPropertyBlock.SetFloat(HitFlashFactorId, 0f);
        spriteRenderer.SetPropertyBlock(materialPropertyBlock);

        hitFlashCoroutine = null;
    }

    private void Die()
    {
        if (!isSpawned) return;

        isSpawned = false;
        enemyCollider.enabled = false;
        body.simulated = false;

        onDefeated.Invoke(expReward);

        StartCoroutine(DieEffect());
    }

    private IEnumerator DieEffect()
    {
        float elapsedTime = 0f;

        while (elapsedTime < DissolveTime)
        {
            elapsedTime += Time.deltaTime;

            float lerpedDissolve = Mathf.Lerp(0, 1f, (elapsedTime / DissolveTime));

            materialPropertyBlock.SetFloat(DissolveAmountId, lerpedDissolve);
            spriteRenderer.SetPropertyBlock(materialPropertyBlock);

            yield return null;
        }

        ownerPool.Release(this);
    }

    public void ResetForPool()
    {
        isSpawned = false;
        isInKnockback = false;

        moveTarget = null;
        body.linearVelocity = Vector2.zero;
        body.simulated = false;
        enemyCollider.enabled = false;

        hitFlashCoroutine = null;
        StopAllCoroutines();
        gameObject.SetActive(false);
    }
}
