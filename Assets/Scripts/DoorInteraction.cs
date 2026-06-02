using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DoorInteraction : MonoBehaviour, IInteractable
{
    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    [Header("Rotation")]
    [SerializeField] private Vector3 closedLocalEulerAngles = Vector3.zero;
    [SerializeField] private Vector3 openLocalEulerAngles = new Vector3(0f, 90f, 0f);
    [SerializeField] private float rotationSpeed = 6f;

    private bool playerInRange;
    private bool isOpen;
    private Quaternion closedRotation;
    private Quaternion openRotation;

    private void Awake()
    {
        closedRotation = Quaternion.Euler(closedLocalEulerAngles);
        openRotation = Quaternion.Euler(openLocalEulerAngles);
        transform.localRotation = closedRotation;
        isOpen = false;
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