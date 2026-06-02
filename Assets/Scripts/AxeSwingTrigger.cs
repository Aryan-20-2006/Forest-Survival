using UnityEngine;
using UnityEngine.InputSystem;

public class AxeSwingTrigger : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator animator;
    [SerializeField] private string swingStateName = "AxeSwing";
    [SerializeField] private int layerIndex = 0;

    [Header("Hit Timing")]
    [SerializeField, Min(0.01f)] private float swingHitWindow = 0.2f;

    private float swingTimer;

    public bool IsSwinging => swingTimer > 0f;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    private void Update()
    {
        if (swingTimer > 0f)
        {
            swingTimer -= Time.deltaTime;
        }

        if (animator == null)
        {
            return;
        }

        Mouse mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            animator.Play(swingStateName, layerIndex, 0f);
            swingTimer = swingHitWindow;
        }
    }
}
