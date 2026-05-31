using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonPlayerController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionAsset controlsAsset;

    [Header("View")]
    [SerializeField] private Transform cameraRoot;
    [SerializeField] private float cameraHeight = 1.6f;
    [SerializeField] private bool snapCameraToPlayerOnStart = true;
    [SerializeField] private float mouseSensitivity = 0.75f;
    [SerializeField] private float maxLookAngle = 85f;
    [SerializeField] private float breathingAmplitude = 0.03f;
    [SerializeField] private float breathingFrequency = 1.5f;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float sprintSpeed = 7f;
    [SerializeField] private float jumpHeight = 1.4f;
    [SerializeField] private float gravity = -20f;

    private CharacterController characterController;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private Transform cameraPivot;
    private Vector3 cameraBaseLocalPosition;
    private Vector3 verticalVelocity;
    private float pitch;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        CacheActions();

        if (cameraRoot == null)
        {
            Camera playerCamera = GetComponentInChildren<Camera>();
            if (playerCamera == null)
            {
                playerCamera = Camera.main;
            }

            if (playerCamera != null)
            {
                cameraRoot = playerCamera.transform;
            }
        }

        ApplyCameraSetup();
    }

    private void OnEnable()
    {
        EnableAction(moveAction);
        EnableAction(lookAction);
        EnableAction(jumpAction);
        EnableAction(sprintAction);

        ApplyCameraSetup();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        DisableAction(moveAction);
        DisableAction(lookAction);
        DisableAction(jumpAction);
        DisableAction(sprintAction);
    }

    private void CacheActions()
    {
        if (controlsAsset == null)
        {
            return;
        }

        InputActionMap playerMap = controlsAsset.FindActionMap("Player", true);
        moveAction = playerMap.FindAction("Move", true);
        lookAction = playerMap.FindAction("Look", true);
        jumpAction = playerMap.FindAction("Jump", true);
        sprintAction = playerMap.FindAction("Sprint", true);
    }

    private static void EnableAction(InputAction action)
    {
        action?.Enable();
    }

    private static void DisableAction(InputAction action)
    {
        action?.Disable();
    }

    private void Update()
    {
        HandleLook();
        HandleMovement();
        UpdateBreathingEffect();
    }

    private void HandleLook()
    {
        if (cameraRoot == null || lookAction == null)
        {
            return;
        }

        Vector2 lookInput = lookAction.ReadValue<Vector2>() * mouseSensitivity;

        transform.Rotate(Vector3.up * lookInput.x);

        pitch -= lookInput.y;
        pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);
        cameraRoot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private void ApplyCameraSetup()
    {
        if (cameraRoot == null)
        {
            return;
        }
        // Preserve current world position/rotation of the camera so it doesn't snap on start
        Vector3 savedWorldPos = cameraRoot.position;
        Quaternion savedWorldRot = cameraRoot.rotation;

        if (cameraPivot == null)
        {
            GameObject pivotObject = new GameObject("CameraPivot");
            cameraPivot = pivotObject.transform;
            cameraPivot.SetParent(transform, false);
        }

        // Place pivot at desired eye height relative to player
        cameraPivot.localPosition = new Vector3(0f, cameraHeight, 0f);
        cameraPivot.localRotation = Quaternion.identity;

        if (snapCameraToPlayerOnStart)
        {
            // Parent camera to pivot and snap its local transform so game view matches configured height
            cameraRoot.SetParent(cameraPivot, false);
            cameraRoot.localPosition = Vector3.zero;
            cameraRoot.localRotation = Quaternion.identity;
            cameraBaseLocalPosition = cameraRoot.localPosition;
            pitch = 0f;
        }
        else
        {
            // Parent camera but preserve world transform (don't snap)
            if (cameraRoot.parent != cameraPivot)
            {
                cameraRoot.SetParent(cameraPivot, true);
            }

            // Restore original world transform (avoids snapping)
            cameraRoot.position = savedWorldPos;
            cameraRoot.rotation = savedWorldRot;

            // Cache the camera's local position relative to the pivot for breathing offsets
            cameraBaseLocalPosition = cameraRoot.localPosition;

            // Initialize pitch from current local rotation
            float localX = cameraRoot.localEulerAngles.x;
            if (localX > 180f) localX -= 360f;
            pitch = localX;
        }
    }

    private void UpdateBreathingEffect()
    {
        if (cameraRoot == null)
        {
            return;
        }

        float breathingOffset = Mathf.Sin(Time.time * breathingFrequency) * breathingAmplitude;
        cameraRoot.localPosition = cameraBaseLocalPosition + Vector3.up * breathingOffset;
    }

    private void HandleMovement()
    {
        if (characterController == null || moveAction == null)
        {
            return;
        }

        bool isGrounded = characterController.isGrounded;

        if (isGrounded && verticalVelocity.y < 0f)
        {
            verticalVelocity.y = -2f;
        }

        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;

        float currentSpeed = sprintAction != null && sprintAction.IsPressed() ? sprintSpeed : walkSpeed;

        characterController.Move(move * (currentSpeed * Time.deltaTime));

        if (jumpAction != null && jumpAction.WasPressedThisFrame() && isGrounded)
        {
            verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        verticalVelocity.y += gravity * Time.deltaTime;
        characterController.Move(verticalVelocity * Time.deltaTime);
    }
}
