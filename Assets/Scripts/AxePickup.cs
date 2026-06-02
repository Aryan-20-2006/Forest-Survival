using UnityEngine;

[RequireComponent(typeof(Collider))]
public class AxePickup : MonoBehaviour, IInteractable
{
    [Header("Setup")]
    [Tooltip("The handaxe object that should appear in the player's view after pickup.")]
    [SerializeField] private GameObject handaxe;
    [SerializeField] private bool hidePickupObjectOnCollect = true;

    private Collider pickupCollider;
    private bool collected;

    private void Awake()
    {
        pickupCollider = GetComponent<Collider>();

        if (handaxe != null)
        {
            handaxe.SetActive(false);
        }
    }

    public void Interact()
    {
        if (collected)
        {
            return;
        }

        collected = true;

        if (handaxe != null)
        {
            ConfigureHeldAxe(handaxe);
            handaxe.SetActive(true);
        }

        if (hidePickupObjectOnCollect)
        {
            gameObject.SetActive(false);
        }
        else if (pickupCollider != null)
        {
            pickupCollider.enabled = false;
        }
    }

    private static void ConfigureHeldAxe(GameObject heldAxe)
    {
        Rigidbody heldRigidbody = heldAxe.GetComponent<Rigidbody>();
        if (heldRigidbody != null)
        {
            heldRigidbody.isKinematic = true;
            heldRigidbody.useGravity = false;
            heldRigidbody.detectCollisions = true;
        }

        Collider[] heldColliders = heldAxe.GetComponentsInChildren<Collider>(true);
        foreach (Collider collider in heldColliders)
        {
            collider.enabled = true;
            collider.isTrigger = true;
        }
    }
}