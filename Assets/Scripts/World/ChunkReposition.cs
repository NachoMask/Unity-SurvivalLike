using UnityEngine;

[RequireComponent(
    typeof(Rigidbody2D))]
public class ChunkReposition : MonoBehaviour
{
    private CompositeCollider2D chunkCollider;
    private Rigidbody2D body;

    [SerializeField] private LayerMask detectLayer;
    [SerializeField] private Vector2 chunkSize = new(20, 20);

    private bool isInvalid;

    private void Awake()
    {
        chunkCollider = GetComponent<CompositeCollider2D>();
        body = GetComponent<Rigidbody2D>();

        if (!TryValidateSettings(out string error))
        {
            Debug.LogError($"{nameof(ChunkReposition)} {name} : {error}");
            enabled = false;
            isInvalid = true;
            return;
        }
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        if (isInvalid) return;

        if ((detectLayer.value & (1 << collider.gameObject.layer)) == 0) return;

        Bounds detectorBounds = collider.bounds;
        Bounds chunkBounds = chunkCollider.bounds;

        Vector2 repositionOffset = Vector2.zero;

        if (chunkBounds.max.x <= detectorBounds.min.x)
        {
            repositionOffset.x = chunkSize.x * 2f;  // 왼쪽 -> 오른쪽
        }
        else if (chunkBounds.min.x >= detectorBounds.max.x)
        {
            repositionOffset.x = -chunkSize.x * 2f; // 오른쪽 -> 왼쪽
        }

        if (chunkBounds.max.y <= detectorBounds.min.y)
        {
            repositionOffset.y = chunkSize.y * 2f;  // 아래 -> 위
        }
        else if (chunkBounds.min.y >= detectorBounds.max.y)
        {
            repositionOffset.y = -chunkSize.y * 2f; // 위 -> 아래
        }

        body.position += repositionOffset;
    }

    private bool TryValidateSettings(out string error)
    {
        if (chunkCollider == null)
        {
            error = $"{nameof(chunkCollider)} is Invalid.";
            return false;
        }
        if (body == null)
        {
            error = $"{nameof(body)} is Invalid.";
            return false;
        }
        if (chunkSize.x <= 0f || chunkSize.y <= 0f)
        {
            error = $"{nameof(chunkSize)} must be positive.";
            return false;
        }

        error = null;
        return true;
    }
}
