using UnityEngine;

[RequireComponent(
    typeof(Rigidbody2D))]
public class Reposition : MonoBehaviour
{
    private Rigidbody2D body;

    [SerializeField] private LayerMask repositionDetectLayer;
    [SerializeField] private Vector2 repositionSize;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        if ((repositionDetectLayer.value & (1 << collider.gameObject.layer)) == 0) return;

        Bounds detectorBounds = collider.bounds;
        Vector2 detectorCenter = detectorBounds.center;
        Vector2 thisCenter = transform.position;

        Vector2 centerOffset = detectorCenter - thisCenter;
        Vector2 repositionOffset = Vector2.zero;

        if (Mathf.Abs(centerOffset.x) >= repositionSize.x)
        {
            repositionOffset.x = Mathf.Sign(centerOffset.x) * repositionSize.x * 2;
        }

        if (Mathf.Abs(centerOffset.y) >= repositionSize.y)
        {
            repositionOffset.y = Mathf.Sign(centerOffset.y) * repositionSize.y * 2;
        }

        body.position += repositionOffset;
    }
}
