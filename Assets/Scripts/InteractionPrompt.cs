using TMPro;
using UnityEngine;

public class InteractionPrompt : MonoBehaviour
{
    public static InteractionPrompt Instance { get; private set; }

    [SerializeField] private TMP_Text promptText;
    [SerializeField] private GameObject promptRoot;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        if (promptRoot != null) promptRoot.SetActive(false);
    }

    private void OnDestroy()
    {
        // no automatic subscription; UI should subscribe explicitly if used
    }

    public void Show(string text)
    {
        if (promptRoot == null || promptText == null) return;
        promptText.text = text;
        promptRoot.SetActive(true);
    }

    public void Hide()
    {
        if (promptRoot == null) return;
        promptRoot.SetActive(false);
    }
}
