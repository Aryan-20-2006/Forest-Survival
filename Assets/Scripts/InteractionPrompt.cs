using TMPro;
using UnityEngine;

public class InteractionPrompt : MonoBehaviour
{
    public static InteractionPrompt Instance { get; private set; }

    [SerializeField] private TMP_Text promptText;
    [SerializeField] private GameObject promptRoot;
    [SerializeField] private ReticleInteractor reticleInteractor;

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

    private void Start()
    {
        if (reticleInteractor == null)
            reticleInteractor = FindFirstObjectByType<ReticleInteractor>();

        if (reticleInteractor != null)
        {
            reticleInteractor.OnHoverEnter += Show;
            reticleInteractor.OnHoverExit += Hide;
        }
    }

    private void OnDestroy()
    {
        if (reticleInteractor != null)
        {
            reticleInteractor.OnHoverEnter -= Show;
            reticleInteractor.OnHoverExit -= Hide;
        }
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
