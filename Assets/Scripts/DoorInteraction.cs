using UnityEngine;
using System;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class DoorInteraction : MonoBehaviour
{
    [Header("Input")]
    [Tooltip("If true, uses the Input System action. If false, uses a keyboard key fallback via the Input System API.")]
    [SerializeField] private bool useInputAction = true;
    [SerializeField] private InputActionReference interactAction;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("Setup")]
    [SerializeField] private string playerTag = "Player";
    [Tooltip("If the door has no trigger collider, enable proximity-based interaction.")]
    [SerializeField] private bool useProximityIfNoTrigger = true;
    [Tooltip("Max distance (meters) from door center for interaction when using proximity.")]
    [SerializeField] private float interactionRange = 2f;
    [Tooltip("Max angle (degrees) between door forward and direction to player to allow interaction.")]
    [SerializeField] private float interactionAngle = 120f;
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
    private Transform playerTransform;
    private Collider doorCollider;
    private bool hasTrigger;
    
    private bool warnedMissingAction = false;
    private bool prevPlayerInRange = false;
    private Vector3 referenceForward;

    private void Awake()
    {
        closedRotation = Quaternion.Euler(closedLocalEulerAngles);
        openRotation = Quaternion.Euler(openLocalEulerAngles);
        transform.localRotation = closedRotation;
        isOpen = false;

        doorCollider = GetComponent<Collider>();
        hasTrigger = doorCollider != null && doorCollider.isTrigger;

        // Cache the door's initial forward so opening the door doesn't change interaction facing checks
        referenceForward = transform.forward;

        // Find player transform by tag for proximity checks
        GameObject p = GameObject.FindGameObjectWithTag(playerTag);
        if (p != null)
            playerTransform = p.transform;

        

        // no UI prompt assigned (handled externally)
    }

    private void OnEnable()
    {
        if (useInputAction)
            interactAction?.action?.Enable();
    }

    private void OnDisable()
    {
        if (useInputAction)
            interactAction?.action?.Disable();
    }

    private void Update()
    {
        // Update playerInRange either from trigger events or via proximity checks
        if (!hasTrigger && useProximityIfNoTrigger)
        {
            if (playerTransform != null)
            {
                Vector3 toPlayer = playerTransform.position - transform.position;
                float horizDist = new Vector3(toPlayer.x, 0f, toPlayer.z).magnitude;
                Vector3 toPlayerFlat = new Vector3(toPlayer.x, 0f, toPlayer.z);
                float angle = Vector3.Angle(referenceForward, toPlayerFlat);
                playerInRange = horizDist <= interactionRange && angle <= (interactionAngle * 0.5f);
            }
            else
            {
                playerInRange = false;
            }
        }

        if (!playerInRange)
        {
            prevPlayerInRange = false;
            return;
        }

        bool pressed = false;

        bool actionAssigned = interactAction != null && interactAction.action != null;

        if (useInputAction && actionAssigned)
        {
            if (interactAction.action.WasPressedThisFrame())
                pressed = true;
        }
        else
        {
            if (useInputAction && !actionAssigned && !warnedMissingAction)
            {
                Debug.LogWarning("DoorInteraction: 'Use Input Action' is enabled but no 'Interact Action' is assigned. Falling back to keyboard key. Assign an Input Action or disable Use Input Action to avoid this warning.");
                warnedMissingAction = true;
            }

            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (Enum.TryParse<Key>(interactKey.ToString(), out var key))
                {
                    var keyControl = keyboard[key];
                    if (keyControl != null && keyControl.wasPressedThisFrame)
                        pressed = true;
                }
            }
        }

        if (pressed)
            isOpen = !isOpen;

        prevPlayerInRange = true;

        Quaternion targetRotation = isOpen ? openRotation : closedRotation;
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = false;
        }
    }
}