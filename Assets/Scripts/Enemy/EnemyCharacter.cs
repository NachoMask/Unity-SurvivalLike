using UnityEngine;
using UnityEngine.Pool;

[RequireComponent(
    typeof(SpriteRenderer),
    typeof(Rigidbody2D))]
public class EnemyCharacter : MonoBehaviour
{
    private IObjectPool<EnemyCharacter> ownerPool;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D body;

    private Rigidbody2D moveTarget;

    private int currentHp;
    private float moveSpeed;

    private bool isSpawned = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        body = GetComponent<Rigidbody2D>();
    }

    public void Init(IObjectPool<EnemyCharacter> pool)
    {
        ownerPool = pool;
    }

    public void Spawn(EnemyData data, Rigidbody2D target, Vector2 position)
    {
        isSpawned = true;

        transform.position = position;

        currentHp = data.MaxHp;
        moveSpeed = data.MoveSpeed;
        moveTarget = target;

        gameObject.SetActive(true);
    }

    private void FixedUpdate()
    {
        Vector2 nextPosition =
            Vector2.MoveTowards(body.position, moveTarget.position, moveSpeed * Time.fixedDeltaTime);

        body.MovePosition(nextPosition);
    }

    private void LateUpdate()
    {
        if (moveTarget.position.x != body.position.x)
            spriteRenderer.flipX = moveTarget.position.x > body.position.x;
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0)
        {
            throw new System.ArgumentOutOfRangeException(nameof(damage));
        }

        currentHp = Mathf.Max(0, currentHp - damage);

        if (currentHp == 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (!isSpawned) return;

        isSpawned = false;
        ownerPool.Release(this);
    }

    public void ResetForPool()
    {
        isSpawned = false;

        moveTarget = null;
        body.linearVelocity = Vector2.zero;

        gameObject.SetActive(false);
    }
}
