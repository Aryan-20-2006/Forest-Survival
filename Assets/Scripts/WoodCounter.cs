using TMPro;
using UnityEngine;

public class WoodCounter : MonoBehaviour
{
    public static WoodCounter Instance { get; private set; }

    [Header("Counter")]
    [SerializeField, Min(0)] private int startingWood;
    [SerializeField] private string labelPrefix = "Wood: ";
    [SerializeField] private TMP_Text counterLabel;

    public int CurrentWood { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        CurrentWood = startingWood;
        RefreshLabel();
    }

    public void AddWood(int amount)
    {
        ModifyWood(amount);
    }

    public void ModifyWood(int delta)
    {
        if (delta == 0)
        {
            return;
        }

        int next = CurrentWood + delta;
        CurrentWood = Mathf.Max(0, next);
        RefreshLabel();
    }

    private void RefreshLabel()
    {
        if (counterLabel != null)
        {
            counterLabel.text = labelPrefix + CurrentWood;
        }
    }
}