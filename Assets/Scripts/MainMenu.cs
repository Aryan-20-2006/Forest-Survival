using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Canvas reticleCanvas;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private FirstPersonPlayerController playerController;

    private void Start()
    {
        if (playerController != null)
            playerController.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        if (optionsPanel != null)
            optionsPanel.SetActive(false);
    }

    public void Play()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        if (reticleCanvas != null)
            reticleCanvas.gameObject.SetActive(true);

        if (playerController != null)
            playerController.enabled = true;
    }

    public void Options()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        if (optionsPanel != null)
            optionsPanel.SetActive(true);
    }

    public void Back()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
    }

    public void Quit()
    {
        Application.Quit();
        Debug.Log("Quit");
    }
}
