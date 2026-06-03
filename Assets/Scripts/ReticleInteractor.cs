using UnityEngine;
using UnityEngine.InputSystem;

public class ReticleInteractor : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera interactionCamera;

    [Header("Raycast")]
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private LayerMask interactableLayers = ~0;
    [Header("Interaction")]
    [SerializeField] private UnityEngine.InputSystem.Key interactionKey = UnityEngine.InputSystem.Key.E;
    [SerializeField] private bool useLayerMask = false;

    private Outline currentOutline;
    private IInteractable currentInteractable;
    private string currentInteractText;

    public IInteractable CurrentInteractable => currentInteractable;
    public string CurrentInteractText => currentInteractText;

    public event System.Action<string> OnHoverEnter;
    public event System.Action OnHoverExit;

    private void Awake()
    {
        if (interactionCamera == null)
        {
            interactionCamera = GetComponentInChildren<Camera>();
        }

        if (interactionCamera == null)
        {
            interactionCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (interactionCamera == null)
        {
            SetCurrentOutline(null);
            ClearHover();
            return;
        }

        Mouse mouse = Mouse.current;
        UpdateHoveredOutline();

        if (mouse != null && mouse.leftButton.wasPressedThisFrame && currentInteractable is not IKeyOnlyInteractable)
        {
            TryInteractCurrentHover();
        }

        if (Keyboard.current != null && Keyboard.current[interactionKey].wasPressedThisFrame && currentInteractable is not IMouseOnlyInteractable)
        {
            TryInteractCurrentHover();
        }
    }

    private void UpdateHoveredOutline()
    {
        Outline hoveredOutline = null;
        IInteractable hoveredInteractable = null;
        string hoveredText = null;

        if (interactionCamera != null)
        {
            Ray ray = interactionCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            RaycastHit hit;
            bool rayHit = useLayerMask
                ? Physics.Raycast(ray, out hit, interactionDistance, interactableLayers, QueryTriggerInteraction.Collide)
                : Physics.Raycast(ray, out hit, interactionDistance);
            if (rayHit)
            {
                Outline candidateOutline = hit.collider.GetComponentInParent<Outline>(true);
                if (candidateOutline != null && TryGetInteractable(hit.collider, out IInteractable interactable))
                {
                    hoveredOutline = candidateOutline;
                    hoveredInteractable = interactable;
                    if (interactable is IInteractionInfo info)
                    {
                        hoveredText = info.GetInteractionText();
                    }
                }
            }
        }

        SetCurrentOutline(hoveredOutline);
        UpdateHover(hoveredInteractable, hoveredText);
    }

    private void UpdateHover(IInteractable interactable, string promptText)
    {
        if (interactable != currentInteractable)
        {
            currentInteractable = interactable;
            currentInteractText = promptText;

            if (interactable != null)
            {
                OnHoverEnter?.Invoke(promptText);
            }
            else
            {
                OnHoverExit?.Invoke();
            }
        }
    }

    private void ClearHover()
    {
        if (currentInteractable != null)
        {
            currentInteractable = null;
            currentInteractText = null;
            OnHoverExit?.Invoke();
        }
    }

    private void TryInteractCurrentHover()
    {
        Ray ray = interactionCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (useLayerMask)
        {
            if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactableLayers, QueryTriggerInteraction.Collide))
            {
                if (TryGetInteractable(hit.collider, out IInteractable interactable))
                {
                    interactable.Interact();
                    return;
                }
            }
        }
        else
        {
            if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
            {
                if (TryGetInteractable(hit.collider, out IInteractable interactable))
                {
                    interactable.Interact();
                    return;
                }
            }
        }

        // If raycast didn't find an interactable, fall back to a proximity check around the camera
        Collider[] overlaps = useLayerMask
            ? Physics.OverlapSphere(interactionCamera.transform.position, interactionDistance, interactableLayers, QueryTriggerInteraction.Collide)
            : Physics.OverlapSphere(interactionCamera.transform.position, interactionDistance);
        if (overlaps == null || overlaps.Length == 0) return;

        IInteractable nearest = null;
        float nearestSqr = float.MaxValue;
        Vector3 camPos = interactionCamera.transform.position;

        foreach (Collider col in overlaps)
        {
            if (TryGetInteractable(col, out IInteractable candidate))
            {
                Vector3 closest = col.ClosestPoint(camPos);
                float sqr = (closest - camPos).sqrMagnitude;
                if (sqr < nearestSqr)
                {
                    nearestSqr = sqr;
                    nearest = candidate;
                }
            }
        }

        if (nearest != null)
        {
            nearest.Interact();
        }
    }

    private void SetCurrentOutline(Outline hoveredOutline)
    {
        if (currentOutline == hoveredOutline)
        {
            if (currentOutline != null)
            {
                currentOutline.enabled = true;
            }

            return;
        }

        if (currentOutline != null)
        {
            currentOutline.enabled = false;
        }

        currentOutline = hoveredOutline;

        if (currentOutline != null)
        {
            currentOutline.enabled = true;
        }
    }

    private static bool TryGetInteractable(Collider hitCollider, out IInteractable interactable)
    {
        interactable = null;

        MonoBehaviour[] behaviours = hitCollider.GetComponentsInParent<MonoBehaviour>(true);
        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is IInteractable candidate)
            {
                interactable = candidate;
                return true;
            }
        }

        return false;
    }
}