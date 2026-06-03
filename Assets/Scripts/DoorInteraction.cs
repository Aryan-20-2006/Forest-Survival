using TMPro;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DoorInteraction : MonoBehaviour, IInteractable, IKeyOnlyInteractable
{
    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    [Header("Rotation")]
    [SerializeField] private Vector3 closedLocalEulerAngles = Vector3.zero;
    [SerializeField] private Vector3 openLocalEulerAngles = new Vector3(0f, 90f, 0f);
    [SerializeField] private float rotationSpeed = 6f;

    [Header("Prompt UI")]
    [SerializeField] private TMP_Text promptLabel;

    private bool isOpen;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private ReticleInteractor reticleInteractor;

    private void Awake()
    {
        closedRotation = Quaternion.Euler(closedLocalEulerAngles);
        openRotation = Quaternion.Euler(openLocalEulerAngles);
        transform.localRotation = closedRotation;
        isOpen = false;
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

    private void Update()
    {
        Quaternion targetRotation = isOpen ? openRotation : closedRotation;
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    public void Interact()
    {
        isOpen = !isOpen;
    }
}