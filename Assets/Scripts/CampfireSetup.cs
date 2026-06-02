using UnityEngine;

[RequireComponent(typeof(Transform))]
public class CampfireSetup : MonoBehaviour
{
    [Header("Setup Options")]
    [Tooltip("Ensure the root collider is non-trigger so the player cannot phase through.")]
    [SerializeField] private bool ensureBlockCollider = true;

    [Tooltip("Create or enable a child trigger used for interaction checks.")]
    [SerializeField] private bool ensureInteractionTrigger = true;

    [Tooltip("Extra padding added to the interaction trigger size (meters).")]
    [SerializeField, Min(0f)] private float interactionTriggerPadding = 0.5f;

    [Tooltip("Name for the interaction trigger child object.")]
    [SerializeField] private string interactionTriggerName = "InteractionTrigger";

    private void Awake()
    {
        if (ensureBlockCollider)
        {
            Collider rootCol = GetComponent<Collider>();
            if (rootCol == null)
            {
                // add a box collider if nothing exists
                BoxCollider bc = gameObject.AddComponent<BoxCollider>();
                bc.isTrigger = false;
            }
            else if (rootCol.isTrigger)
            {
                rootCol.isTrigger = false;
            }
        }

        if (ensureInteractionTrigger)
        {
            Transform child = transform.Find(interactionTriggerName);
            if (child == null)
            {
                GameObject go = new GameObject(interactionTriggerName);
                go.transform.SetParent(transform, false);
                go.layer = gameObject.layer;

                BoxCollider trg = go.AddComponent<BoxCollider>();
                trg.isTrigger = true;

                Bounds b = CalculateBounds();
                trg.center = transform.InverseTransformPoint(b.center);
                trg.size = b.size + Vector3.one * interactionTriggerPadding;
            }
            else
            {
                Collider trgCol = child.GetComponent<Collider>();
                if (trgCol == null)
                {
                    BoxCollider trg = child.gameObject.AddComponent<BoxCollider>();
                    trg.isTrigger = true;
                }
                else
                {
                    trgCol.isTrigger = true;
                }
            }
        }

        // Make any Rigidbody kinematic so static blocking works reliably
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
    }

    private Bounds CalculateBounds()
    {
        Renderer[] rends = GetComponentsInChildren<Renderer>();
        if (rends != null && rends.Length > 0)
        {
            Bounds b = rends[0].bounds;
            for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
            return b;
        }

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            return col.bounds;
        }

        return new Bounds(transform.position, Vector3.one);
    }
}
