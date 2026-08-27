using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerCharacter : MonoBehaviour
{
    private Rigidbody2D body;

    [SerializeField] private float moveSpeed = 5f;
    private Vector2 moveDirection;

    public Vector2 LastMoveDirection { get; private set; } = Vector2.right;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        Vector2 nextPosition = body.position + moveDirection * moveSpeed * Time.fixedDeltaTime;

        body.MovePosition(nextPosition);
    }

    public void SetMoveDirection(Vector2 direction)
    {
        moveDirection = direction;

        if (direction != Vector2.zero)
        {
            LastMoveDirection = direction;
        }
    }
}
