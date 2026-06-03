using TMPro;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CampfireDeposit : MonoBehaviour, IInteractable, IKeyOnlyInteractable
{
    [Header("Deposit Settings")]
    [SerializeField] private int storedWood;
    [SerializeField, Min(0)] private int maxCapacity = 20;

    [Header("Prompt UI")]
    [SerializeField] private TMP_Text promptLabel;

    [Header("Burn Settings")]
    [Tooltip("Seconds each wood contributes to the burn. Set 3-6 for a full campfire to last about 1-2 minutes.")]
    [SerializeField, Min(0.1f)] private float secondsPerWood = 5f;
    [SerializeField, Min(0f)] private float minIntensity = 0f;
    [SerializeField, Min(0f)] private float maxIntensity = 2f;
    [Tooltip("How quickly the light follows the burn target intensity.")]
    [SerializeField, Min(0.01f)] private float lightFollowSpeed = 1.5f;

    [Header("Light")]
    [SerializeField] private Light campfireLight;

    public int StoredWood => storedWood;
    public int MaxCapacity => maxCapacity;

    public event System.Action<int> OnStoredWoodChanged;

    private float burnFuelRemaining;
    private ReticleInteractor reticleInteractor;

    private void Awake()
    {
        if (campfireLight == null)
        {
            campfireLight = GetComponentInChildren<Light>(true);
        }

        storedWood = Mathf.Clamp(storedWood, 0, maxCapacity);
        burnFuelRemaining = storedWood;

        if (campfireLight != null)
        {
            campfireLight.enabled = true;
            campfireLight.intensity = GetTargetIntensity();
        }
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
        if (burnFuelRemaining > 0f)
        {
            burnFuelRemaining -= Time.deltaTime / Mathf.Max(0.01f, secondsPerWood);
            if (burnFuelRemaining < 0f)
            {
                burnFuelRemaining = 0f;
            }

            int nextStored = Mathf.Clamp(Mathf.CeilToInt(burnFuelRemaining), 0, maxCapacity);
            if (nextStored != storedWood)
            {
                storedWood = nextStored;
                OnStoredWoodChanged?.Invoke(storedWood);
            }
        }

        if (campfireLight != null)
        {
            campfireLight.enabled = true;
            float targetIntensity = GetTargetIntensity();
            campfireLight.intensity = Mathf.MoveTowards(
                campfireLight.intensity,
                targetIntensity,
                lightFollowSpeed * Time.deltaTime
            );
        }
    }

    public void Interact()
    {
        if (WoodCounter.Instance == null)
        {
            return;
        }

        int playerWood = WoodCounter.Instance.CurrentWood;
        if (playerWood <= 0)
        {
            return;
        }

        int spaceRemaining = Mathf.Max(0, maxCapacity - storedWood);
        if (spaceRemaining <= 0)
        {
            return;
        }

        int depositAmount = Mathf.Min(playerWood, spaceRemaining);
        if (depositAmount <= 0)
        {
            return;
        }

        storedWood += depositAmount;
        burnFuelRemaining += depositAmount;
        burnFuelRemaining = Mathf.Clamp(burnFuelRemaining, 0f, maxCapacity);

        WoodCounter.Instance.ModifyWood(-depositAmount);
        OnStoredWoodChanged?.Invoke(storedWood);
    }

    private float GetTargetIntensity()
    {
        if (maxCapacity <= 0)
        {
            return minIntensity;
        }

        float t = Mathf.Clamp01(burnFuelRemaining / maxCapacity);
        return Mathf.Lerp(minIntensity, maxIntensity, t);
    }
}
