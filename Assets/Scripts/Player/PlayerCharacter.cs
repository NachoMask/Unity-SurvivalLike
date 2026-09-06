using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(
    typeof(SpriteRenderer),
    typeof(Rigidbody2D),
    typeof(Animator))]
[RequireComponent(
    typeof(PlayerStats))]
public class PlayerCharacter : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D body;
    private Animator animator;
    private PlayerStats playerStats;

    private float MoveSpeed => playerStats.MoveSpeed;
    private Vector2 moveDirection;

    public Vector2 LastMoveDirection { get; private set; } = Vector2.right;

    private static readonly int IsMovingHash
        = Animator.StringToHash("IsMoving");

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerStats = GetComponent<PlayerStats>();
    }

    private void OnEnable()
    {
        playerStats.Died += RestartGame;
    }

    private void OnDisable()
    {
        playerStats.Died -= RestartGame;
    }

    private void FixedUpdate()
    {
        Vector2 nextPosition = body.position + moveDirection * MoveSpeed * Time.fixedDeltaTime;

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

    private void RestartGame()
    {
        Time.timeScale = 1f;

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }
}
