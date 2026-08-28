using UnityEngine;

[RequireComponent(
    typeof(SpriteRenderer),
    typeof(Rigidbody2D),
    typeof(Animator))]
public class PlayerCharacter : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D body;
    private Animator animator;

    [SerializeField] private float moveSpeed = 5f;
    private Vector2 moveDirection;

    public Vector2 LastMoveDirection { get; private set; } = Vector2.right;

    private static readonly int IsMovingHash
        = Animator.StringToHash("IsMoving");

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        Vector2 nextPosition = body.position + moveDirection * moveSpeed * Time.fixedDeltaTime;

        body.MovePosition(nextPosition);
    }

    public void SetMoveDirection(Vector2 direction)
    {
        moveDirection = direction;
        animator.SetBool(IsMovingHash, direction.sqrMagnitude > 0f);

        if (direction != Vector2.zero)
        {
            LastMoveDirection = direction;

            if (direction.x != 0f)
                spriteRenderer.flipX = direction.x < 0f;
        }
    }
}
