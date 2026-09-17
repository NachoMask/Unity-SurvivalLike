using System.Collections.Generic;
using UnityEngine;

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
    private float lastHitTime = float.NegativeInfinity;


    private MaterialPropertyBlock materialPropertyBlock;
    private static readonly int IsHitFactorId = Shader.PropertyToID("_IsHitFactor");
    private readonly HashSet<Collider2D> contactingEnemies = new();
    [SerializeField] private ParticleSystem hitParticles;

    public Vector2 LastMoveDirection { get; private set; } = Vector2.right;

    private static readonly int IsMovingHash
        = Animator.StringToHash("IsMoving");

    private void Awake()
    {
        if (hitParticles == null)
        {
            Debug.LogError($"{nameof(hitParticles)} is Invalid.");
            enabled = false;
            return;
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerStats = GetComponent<PlayerStats>();

        materialPropertyBlock = new MaterialPropertyBlock();
        spriteRenderer.GetPropertyBlock(materialPropertyBlock);
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

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (!collider.TryGetComponent(out EnemyCharacter _)) return;

        if (contactingEnemies.Add(collider) && contactingEnemies.Count == 1)
            SetHitVisual(true);
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (contactingEnemies.Remove(collider) && contactingEnemies.Count == 0)
            SetHitVisual(false);
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        if (!collider.TryGetComponent(out EnemyCharacter enemy)) return;
        if (Time.time - lastHitTime < playerStats.InvulnerabilityTime) return;
        if (playerStats.IsDead) return;

        lastHitTime = Time.time;
        playerStats.TakeDamage(enemy.ContactDamage);

        hitParticles.Play();
        hitParticles.Emit(Mathf.RoundToInt(enemy.ContactDamage) * 10);
    }

    private void SetHitVisual(bool isHit)
    {
        materialPropertyBlock.SetFloat(IsHitFactorId, isHit ? 1f : 0f);
        spriteRenderer.SetPropertyBlock(materialPropertyBlock);
    }
}
