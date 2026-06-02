using TMPro;
using UnityEngine;

public class FoodCounter : MonoBehaviour
{
    public static FoodCounter Instance { get; private set; }

    [Header("Counter")]
    [SerializeField, Min(0)] private int startingFood;
    [SerializeField] private string labelPrefix = "Food: ";
    [SerializeField] private TMP_Text canvasLabel;

    public int CurrentFood { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        CurrentFood = startingFood;
        RefreshLabel();
    }

    public void ModifyFood(int delta)
    {
        if (delta == 0)
        {
            return;
        }

        int next = CurrentFood + delta;
        CurrentFood = Mathf.Max(0, next);
        RefreshLabel();
    }

    private void RefreshLabel()
    {
        if (canvasLabel != null)
        {
            canvasLabel.text = labelPrefix + CurrentFood;
        }
    }
}