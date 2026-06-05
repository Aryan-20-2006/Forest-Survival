using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FireSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CampfireDeposit campfireDeposit;
    [SerializeField] private GameObject fireVFX;
    [SerializeField] private Image fireFill;
    [SerializeField] private Light campfirePointLight;
    [SerializeField] private TMP_Text fireWarningText;

    [Header("Fire Bar")]
    [SerializeField, Min(0f)] private float maxFire = 20f;
    [SerializeField, Min(0f)] private float currentFire;
    [SerializeField, Range(0f, 1f)] private float fireWarningThreshold = 0.25f;
    [TextArea]
    [SerializeField] private string fireWarningMessage = "🔥 Campfire is fading. Add more wood!";
    [SerializeField, Min(0f)] private float warningFadeDuration = 0.25f;

    [Header("Fire Light")]
    [SerializeField] private Gradient fireColorGradient;
    [SerializeField, Min(0f)] private float lowFireIntensity = 0f;
    [SerializeField, Min(0f)] private float highFireIntensity = 2f;

    private bool hasBeenFueled;
    private bool fireWarningTargetVisible;
    private Coroutine fireWarningFadeRoutine;

    private void Awake()
    {
        if (campfireDeposit == null)
        {
            campfireDeposit = FindFirstObjectByType<CampfireDeposit>();
        }

        if (campfireDeposit != null)
        {
            maxFire = Mathf.Max(1f, campfireDeposit.MaxCapacity);
            currentFire = Mathf.Clamp(campfireDeposit.StoredWood, 0f, maxFire);
            campfireDeposit.OnStoredWoodChanged += HandleStoredWoodChanged;

            if (campfirePointLight == null)
            {
                campfirePointLight = campfireDeposit.GetComponentInChildren<Light>(true);
            }
        }
        else
        {
            currentFire = Mathf.Clamp(currentFire, 0f, maxFire);
        }

        hasBeenFueled = currentFire > 0f;

        if (fireVFX != null)
            fireVFX.SetActive(currentFire > 0f);

        EnsureDefaultFireGradient();

        UpdateFireVisuals();
        ApplyFireWarningImmediate(IsFireWarningActive());
    }

    private void OnDestroy()
    {
        if (campfireDeposit != null)
        {
            campfireDeposit.OnStoredWoodChanged -= HandleStoredWoodChanged;
        }
    }

    private void Update()
    {
        UpdateFireWarningText(IsFireWarningActive());
    }

    private void OnValidate()
    {
        ApplyFireWarningImmediate(IsFireWarningActive());
    }

    private void HandleStoredWoodChanged(int storedWood)
    {
        currentFire = Mathf.Clamp(storedWood, 0f, maxFire);
        if (storedWood > 0)
        {
            hasBeenFueled = true;
            if (fireVFX != null) fireVFX.SetActive(true);
        }
        else
        {
            if (fireVFX != null) fireVFX.SetActive(false);
        }

        UpdateFireVisuals();
    }

    private void UpdateFireVisuals()
    {
        if (maxFire <= 0f)
        {
            return;
        }

        float firePercent = Mathf.Clamp01(currentFire / maxFire);
        Color fireColor = fireColorGradient != null ? fireColorGradient.Evaluate(firePercent) : Color.white;

        if (fireFill != null)
        {
            fireFill.fillAmount = firePercent;
            fireFill.color = fireColor;
        }

        if (campfirePointLight != null)
        {
            campfirePointLight.intensity = Mathf.Lerp(lowFireIntensity, highFireIntensity, firePercent);
            campfirePointLight.color = fireColor;
        }

        UpdateFireWarningText(firePercent <= fireWarningThreshold);
    }

    private bool IsFireWarningActive()
    {
        if (maxFire <= 0f)
        {
            return false;
        }

        float firePercent = Mathf.Clamp01(currentFire / maxFire);
        return hasBeenFueled && firePercent <= fireWarningThreshold;
    }

    private void EnsureDefaultFireGradient()
    {
        fireColorGradient ??= new Gradient();

        if (fireColorGradient.colorKeys != null && fireColorGradient.colorKeys.Length >= 3)
        {
            return;
        }

        fireColorGradient.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(0.22f, 0.06f, 0.02f, 1f), 0f),
                new GradientColorKey(new Color(0.45f, 0.35f, 0.10f, 1f), 0.5f),
                new GradientColorKey(new Color(1f, 0.72f, 0.35f, 1f), 1f)
            },
            new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            }
        );
    }

    public void AddFuel(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        currentFire = Mathf.Clamp(currentFire + amount, 0f, maxFire);
        hasBeenFueled = true;
        UpdateFireVisuals();
    }

    private void UpdateFireWarningText(bool shouldShow)
    {
        if (fireWarningText == null)
        {
            return;
        }

        if (fireWarningTargetVisible == shouldShow)
        {
            if (shouldShow)
            {
                fireWarningText.text = fireWarningMessage;
                if (!fireWarningText.gameObject.activeSelf)
                {
                    fireWarningText.gameObject.SetActive(true);
                }
            }

            return;
        }

        fireWarningTargetVisible = shouldShow;

        if (fireWarningFadeRoutine != null)
        {
            StopCoroutine(fireWarningFadeRoutine);
        }

        fireWarningFadeRoutine = StartCoroutine(FadeFireWarning(shouldShow));
    }

    private void ApplyFireWarningImmediate(bool shouldShow)
    {
        if (fireWarningText == null)
        {
            return;
        }

        if (fireWarningFadeRoutine != null)
        {
            StopCoroutine(fireWarningFadeRoutine);
            fireWarningFadeRoutine = null;
        }

        fireWarningTargetVisible = shouldShow;

        if (shouldShow)
        {
            if (!fireWarningText.gameObject.activeSelf)
            {
                fireWarningText.gameObject.SetActive(true);
            }

            fireWarningText.text = fireWarningMessage;
            SetWarningAlpha(fireWarningText, 1f);
        }
        else
        {
            SetWarningAlpha(fireWarningText, 0f);
            fireWarningText.text = string.Empty;
        }
    }

    private IEnumerator FadeFireWarning(bool shouldShow)
    {
        if (fireWarningText == null)
        {
            yield break;
        }

        if (shouldShow)
        {
            if (!fireWarningText.gameObject.activeSelf)
            {
                fireWarningText.gameObject.SetActive(true);
            }

            fireWarningText.text = fireWarningMessage;
        }

        float duration = Mathf.Max(0.01f, warningFadeDuration);
        float startAlpha = fireWarningText.color.a;
        float endAlpha = shouldShow ? 1f : 0f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            SetWarningAlpha(fireWarningText, Mathf.Lerp(startAlpha, endAlpha, t));
            yield return null;
        }

        SetWarningAlpha(fireWarningText, endAlpha);

        if (!shouldShow)
        {
            fireWarningText.text = string.Empty;
        }

        fireWarningFadeRoutine = null;
    }

    private static void SetWarningAlpha(TMP_Text text, float alpha)
    {
        if (text == null)
        {
            return;
        }

        Color color = text.color;
        color.a = alpha;
        text.color = color;
    }
}