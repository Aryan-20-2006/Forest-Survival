using UnityEngine;

public class TreeChopShrink : MonoBehaviour
{
    [Header("Shrink Settings")]
    [SerializeField, Min(0.001f)] private float shrinkPerHit = 0.08f;
    [SerializeField, Min(0.01f)] private float minimumScaleFactor = 0.25f;
    [SerializeField] private bool disableWhenMinSizeReached;

    [Header("Destruction")]
    [SerializeField, Min(1)] private int hitsToDisappear = 8;
    [SerializeField] private bool disableOnHitLimit = true;

    [Header("Rewards")]
    [SerializeField, Min(1)] private int woodPerHit = 1;

    private Vector3 initialScale;
    private bool initialized;
    private int hitCount;

    private void Awake()
    {
        initialScale = transform.localScale;
        initialized = true;
    }

    public void RegisterAxeHit()
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        if (WoodCounter.Instance != null)
        {
            WoodCounter.Instance.AddWood(woodPerHit);
        }

        hitCount++;

        if (disableOnHitLimit && hitCount >= hitsToDisappear)
        {
            gameObject.SetActive(false);
            return;
        }

        if (!initialized)
        {
            initialScale = transform.localScale;
            initialized = true;
        }

        Vector3 minScale = initialScale * minimumScaleFactor;
        Vector3 shrinkStep = initialScale * shrinkPerHit;
        Vector3 nextScale = Vector3.Max(transform.localScale - shrinkStep, minScale);
        transform.localScale = nextScale;

        if (disableWhenMinSizeReached && IsAtOrBelowMinScale(nextScale, minScale))
        {
            gameObject.SetActive(false);
        }
    }

    private static bool IsAtOrBelowMinScale(Vector3 current, Vector3 min)
    {
        const float epsilon = 0.0001f;
        return current.x <= min.x + epsilon && current.y <= min.y + epsilon && current.z <= min.z + epsilon;
    }
}