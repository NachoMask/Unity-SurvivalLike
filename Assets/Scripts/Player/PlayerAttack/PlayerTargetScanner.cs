using System.Collections.Generic;
using UnityEngine;

public class PlayerTargetScanner : MonoBehaviour
{
    private const float MinimumScanRange = 0.1f;
    private const int InitialTargetCapacity = 32;

    [SerializeField, Min(MinimumScanRange)] private float scanRange;
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private Transform nearestTarget;

    private readonly List<Collider2D> targets = new(InitialTargetCapacity);
    private ContactFilter2D targetFilter;

    public Transform NearestTarget => nearestTarget;

    private void Awake()
    {
        if (!TryValidateSettings(out string error))
        {
            Debug.LogError($"{nameof(PlayerTargetScanner)} {name}: {error}", this);
            enabled = false;
            return;
        }

        targetFilter = new ContactFilter2D();
        targetFilter.SetLayerMask(targetLayer);
    }

    private void FixedUpdate()
    {
        int targetCount = Physics2D.OverlapCircle(transform.position, scanRange, targetFilter, targets);
        nearestTarget = GetNearestTarget(targetCount);
    }

    private Transform GetNearestTarget(int targetCount)
    {
        Vector2 origin = transform.position;

        Transform result = null;
        float nearestSqrDistance = float.PositiveInfinity;

        for (int i = 0; i < targetCount; ++i)
        {
            Transform target = targets[i].transform;
            Vector2 offset = (Vector2)target.position - origin;

            float sqrDistance = offset.sqrMagnitude;

            if (sqrDistance < nearestSqrDistance)
            {
                nearestSqrDistance = sqrDistance;
                result = target;
            }
        }

        return result;
    }

    private bool TryValidateSettings(out string error)
    {
        if (scanRange < MinimumScanRange)
        {
            error = $"{nameof(scanRange)} must be at least {MinimumScanRange}";
            return false;
        }

        if (targetLayer.value == 0)
        {
            error = $"{nameof(targetLayer)} must not be empty";
            return false;
        }

        error = null;
        return true;
    }
}
