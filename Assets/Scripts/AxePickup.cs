using TMPro;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class AxePickup : MonoBehaviour, IInteractable, IKeyOnlyInteractable
{
    [Header("Setup")]
    [Tooltip("The handaxe object that should appear in the player's view after pickup.")]
    [SerializeField] private GameObject handaxe;
    [SerializeField] private bool hidePickupObjectOnCollect = true;

    [Header("Audio")]
    [SerializeField] private AudioClip pickupClip;

    [Header("Prompt UI")]
    [SerializeField] private TMP_Text promptLabel;

    private Collider pickupCollider;
    private bool collected;
    private ReticleInteractor reticleInteractor;

    private void Awake()
    {
        pickupCollider = GetComponent<Collider>();

        if (handaxe != null)
            handaxe.SetActive(false);
    }

    private void Start()
    {
        if (promptLabel != null)
        {
            promptLabel.gameObject.SetActive(false);
            reticleInteractor = FindFirstObjectByType<ReticleInteractor>();
            if (reticleInteractor != null)
            {
                reticleInteractor.OnHoverEnter += OnHoverEnter;
                reticleInteractor.OnHoverExit += OnHoverExit;
            }
        }
    }

    private void OnDestroy()
    {
        if (reticleInteractor != null)
        {
            reticleInteractor.OnHoverEnter -= OnHoverEnter;
            reticleInteractor.OnHoverExit -= OnHoverExit;
        }
    }

    private void OnHoverEnter(string text)
    {
        if (reticleInteractor.CurrentInteractable == (IInteractable)this)
            promptLabel.gameObject.SetActive(true);
    }

    private void OnHoverExit()
    {
        promptLabel.gameObject.SetActive(false);
    }

    public void Interact()
    {
        if (collected)
        {
            return;
        }

        collected = true;

        if (pickupClip != null)
            AudioSource.PlayClipAtPoint(pickupClip, transform.position);

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