using UnityEngine;

[RequireComponent(
    typeof(SpriteRenderer),
    typeof(Rigidbody2D))]
public class EnemyCharacter : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D body;

    [SerializeField] private Rigidbody2D target;
    [SerializeField] private float moveSpeed = 1f;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        body = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        Vector2 nextPosition =
            Vector2.MoveTowards(body.position, target.position, moveSpeed * Time.fixedDeltaTime);

        body.MovePosition(nextPosition);
    }

    private void LateUpdate()
    {
        if (target.position.x != body.position.x)
            spriteRenderer.flipX = target.position.x > body.position.x;
    }
}
