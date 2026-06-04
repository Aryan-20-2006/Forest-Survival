using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("End Prompts")]
    [SerializeField] private TMP_Text winPrompt;
    [SerializeField] private TMP_Text losePrompt;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (winPrompt != null)
            winPrompt.gameObject.SetActive(false);

        if (losePrompt != null)
            losePrompt.gameObject.SetActive(false);
    }

    public void ShowWin()
    {
        if (losePrompt != null)
            losePrompt.gameObject.SetActive(false);

        if (winPrompt != null)
            winPrompt.gameObject.SetActive(true);
    }

    public void ShowLose()
    {
        if (winPrompt != null)
            winPrompt.gameObject.SetActive(false);

        if (losePrompt != null)
            losePrompt.gameObject.SetActive(true);
    }
}
