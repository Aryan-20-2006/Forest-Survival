using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class HungerSystem : MonoBehaviour
{
    public float maxHunger = 100f;
    public float currentHunger = 100f;
    public float hungerDrainRate = 2f;

    public Image hungerFill;
    [SerializeField] private TMP_Text hungerWarningText;

    [Header("Hunger Bar Colors")]
    [SerializeField] private Color fullHungerColor = new Color(0.2f, 0.9f, 0.2f, 1f);
    [SerializeField] private Color muddyHungerColor = new Color(0.45f, 0.55f, 0.18f, 1f);
    [SerializeField] private Color lowHungerColor = new Color(0.9f, 0.2f, 0.2f, 1f);
    [SerializeField, Range(0f, 1f)] private float hungerWarningThreshold = 0.1f;
    [TextArea]
    [SerializeField] private string hungerWarningMessage = "🍖 Need food soon!";
    [SerializeField, Min(0f)] private float warningFadeDuration = 0.25f;

    [Header("Food Consumption")]
    [SerializeField, Min(0f)] private float foodHungerRestoreAmount = 25f;
    [SerializeField] private Key consumeKey = Key.F;
    [SerializeField] private string consumeLogMessage = "Press F to consume food";

    [Header("Lose Settings")]
    [SerializeField] private bool allowRestartOnLoss = true;
    [SerializeField] private Key restartKey = Key.R;
    [SerializeField] private string restartLogMessage = "Press R to restart";

    private bool hasStarved;
    private bool hungerWarningTargetVisible;
    private Coroutine hungerWarningFadeRoutine;

    private void Awake()
    {
        ApplyHungerWarningImmediate(IsHungerWarningActive());
    }

    void Update()
    {
        if (hasStarved)
        {
            if (allowRestartOnLoss && IsRestartPressed())
            {
                RestartGame();
            }

            return;
        }

        if (Keyboard.current != null && Keyboard.current[(consumeKey == Key.None ? Key.F : consumeKey)].wasPressedThisFrame)
        {
            TryConsumeFood();
        }

        currentHunger -= hungerDrainRate * Time.deltaTime;

        currentHunger = Mathf.Clamp(currentHunger, 0f, maxHunger);
        UpdateHungerBarVisuals();
        UpdateHungerWarningText(IsHungerWarningActive());

        if(currentHunger <= 0)
        {
            hasStarved = true;
            Time.timeScale = 0f;
            Debug.Log("Player Starved");

            UpdateHungerWarningText(false);

            if (GameManager.Instance != null)
                GameManager.Instance.ShowLose();

            if (allowRestartOnLoss)
            {
                Debug.Log(restartLogMessage);
            }
        }
    }

    public void AddHunger(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        currentHunger = Mathf.Clamp(currentHunger + amount, 0f, maxHunger);

        UpdateHungerBarVisuals();
    }

    private void OnValidate()
    {
        ApplyHungerWarningImmediate(IsHungerWarningActive());
    }

    private void TryConsumeFood()
    {
        if (FoodCounter.Instance == null || FoodCounter.Instance.CurrentFood <= 0)
        {
            Debug.Log(consumeLogMessage);
            return;
        }

        if (currentHunger >= maxHunger)
        {
            return;
        }

        FoodCounter.Instance.ModifyFood(-1);
        AddHunger(foodHungerRestoreAmount);
    }

    private void UpdateHungerBarVisuals()
    {
        if (hungerFill == null || maxHunger <= 0f)
            return;

        float hungerT = Mathf.Clamp01(currentHunger / maxHunger);
        hungerFill.fillAmount = hungerT;

        if (hungerT >= 0.5f)
        {
            float t = Mathf.InverseLerp(0.5f, 1f, hungerT);
            hungerFill.color = Color.Lerp(muddyHungerColor, fullHungerColor, t);
        }
        else
        {
            float t = Mathf.InverseLerp(0f, 0.5f, hungerT);
            hungerFill.color = Color.Lerp(lowHungerColor, muddyHungerColor, t);
        }
    }

    private bool IsHungerWarningActive()
    {
        if (maxHunger <= 0f)
        {
            return false;
        }

        return Mathf.Clamp01(currentHunger / maxHunger) <= hungerWarningThreshold && !hasStarved;
    }

    private void UpdateHungerWarningText(bool shouldShow)
    {
        if (hungerWarningText == null)
        {
            return;
        }

        if (hungerWarningTargetVisible == shouldShow)
        {
            if (shouldShow)
            {
                hungerWarningText.text = hungerWarningMessage;
                if (!hungerWarningText.gameObject.activeSelf)
                {
                    hungerWarningText.gameObject.SetActive(true);
                }
            }

            return;
        }

        hungerWarningTargetVisible = shouldShow;

        if (hungerWarningFadeRoutine != null)
        {
            StopCoroutine(hungerWarningFadeRoutine);
        }

        hungerWarningFadeRoutine = StartCoroutine(FadeHungerWarning(shouldShow));
    }

    private void ApplyHungerWarningImmediate(bool shouldShow)
    {
        if (hungerWarningText == null)
        {
            return;
        }

        if (hungerWarningFadeRoutine != null)
        {
            StopCoroutine(hungerWarningFadeRoutine);
            hungerWarningFadeRoutine = null;
        }

        hungerWarningTargetVisible = shouldShow;

        if (shouldShow)
        {
            if (!hungerWarningText.gameObject.activeSelf)
            {
                hungerWarningText.gameObject.SetActive(true);
            }

            hungerWarningText.text = hungerWarningMessage;
            SetWarningAlpha(hungerWarningText, 1f);
        }
        else
        {
            SetWarningAlpha(hungerWarningText, 0f);
            hungerWarningText.text = string.Empty;
        }
    }

    private IEnumerator FadeHungerWarning(bool shouldShow)
    {
        if (hungerWarningText == null)
        {
            yield break;
        }

        if (shouldShow)
        {
            if (!hungerWarningText.gameObject.activeSelf)
            {
                hungerWarningText.gameObject.SetActive(true);
            }

            hungerWarningText.text = hungerWarningMessage;
        }

        float duration = Mathf.Max(0.01f, warningFadeDuration);
        float startAlpha = hungerWarningText.color.a;
        float endAlpha = shouldShow ? 1f : 0f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            SetWarningAlpha(hungerWarningText, Mathf.Lerp(startAlpha, endAlpha, t));
            yield return null;
        }

        SetWarningAlpha(hungerWarningText, endAlpha);

        if (!shouldShow)
        {
            hungerWarningText.text = string.Empty;
        }

        hungerWarningFadeRoutine = null;
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

    private bool IsRestartPressed()
    {
        if (Keyboard.current == null)
        {
            return false;
        }

        // Fallback to R if the serialized key is unset (Key.None).
        Key keyToCheck = restartKey == Key.None ? Key.R : restartKey;
        return Keyboard.current[keyToCheck].wasPressedThisFrame;
    }

    private static void RestartGame()
    {
        Time.timeScale = 1f;
        Scene activeScene = SceneManager.GetActiveScene();

        if (!string.IsNullOrEmpty(activeScene.name))
        {
            SceneManager.LoadScene(activeScene.name);
            return;
        }

        if (activeScene.buildIndex >= 0)
        {
            SceneManager.LoadScene(activeScene.buildIndex);
        }
    }
}