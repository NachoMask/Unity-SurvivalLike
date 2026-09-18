using UnityEngine;

public class EnemyReposition : MonoBehaviour
{
    private Collider2D enemyCollider;
    private Rigidbody2D body;

    [SerializeField] private LayerMask detectLayer;

    private bool isInvalid;

    private void Awake()
    {
        enemyCollider = GetComponent<Collider2D>();
        body = GetComponent<Rigidbody2D>();

        if (!TryValidateSettings(out string error))
        {
            Debug.LogError($"{nameof(EnemyReposition)} {name} : {error}");
            enabled = false;
            isInvalid = true;
            return;
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (isInvalid) return;

        if ((detectLayer.value & (1 << collider.gameObject.layer)) == 0) return;

        if (!enemyCollider.isActiveAndEnabled ||
            !body.simulated ||
            !collider.isActiveAndEnabled) return;

        Bounds detectorBounds = collider.bounds;
        Vector2 enemyCenter = enemyCollider.bounds.center;

        Vector2 repositionOffset = Vector2.zero;

        if (enemyCenter.x <= detectorBounds.min.x)
        {
            repositionOffset.x = detectorBounds.size.x;     // 왼쪽 -> 오른쪽
        }
        else if (enemyCenter.x >= detectorBounds.max.x)
        {
            repositionOffset.x = -detectorBounds.size.x;    // 오른쪽 -> 왼쪽
        }

        if (enemyCenter.y <= detectorBounds.min.y)
        {
            repositionOffset.y = detectorBounds.size.y;     // 아래쪽 -> 위쪽
        }
        else if (enemyCenter.y >= detectorBounds.max.y)
        {
            repositionOffset.y = -detectorBounds.size.y;    // 위쪽 -> 아래쪽
        }

        body.position += repositionOffset;
    }

    private bool TryValidateSettings(out string error)
    {
        if (enemyCollider == null)
        {
            error = $"{nameof(enemyCollider)} is Invalid.";
            return false;
        }
        if (body == null)
        {
            error = $"{nameof(body)} is Invalid.";
            return false;
        }

        error = null;
        return true;
    }
}
