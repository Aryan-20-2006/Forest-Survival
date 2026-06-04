using TMPro;
using UnityEngine;

public class DayCounter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DayNightCycle dayNightCycle;
    [SerializeField] private TMP_Text dayCounterLabel;

    [Header("Counter")]
    [SerializeField, Min(1)] private int startingDay = 1;

    public int CurrentDay { get; private set; }

    private void Awake()
    {
        CurrentDay = startingDay;
        RefreshLabel();

        if (dayNightCycle == null)
        {
            dayNightCycle = FindFirstObjectByType<DayNightCycle>();
        }
    }

    private void OnEnable()
    {
        if (dayNightCycle == null)
        {
            dayNightCycle = FindFirstObjectByType<DayNightCycle>();
        }

        if (dayNightCycle != null)
        {
            dayNightCycle.OnNightEnded += HandleNightEnded;
        }
    }

    private void OnDisable()
    {
        if (dayNightCycle != null)
        {
            dayNightCycle.OnNightEnded -= HandleNightEnded;
        }
    }

    private void HandleNightEnded()
    {
        if (CurrentDay == 1)
        {
            Time.timeScale = 0f;
            Debug.Log("One day-night cycle complete. Game over.");

            if (GameManager.Instance != null)
                GameManager.Instance.ShowWin();

            return;
        }

        CurrentDay++;
        RefreshLabel();
    }

    private void RefreshLabel()
    {
        if (dayCounterLabel != null)
        {
            dayCounterLabel.text = "Day " + CurrentDay;
        }
    }
}
