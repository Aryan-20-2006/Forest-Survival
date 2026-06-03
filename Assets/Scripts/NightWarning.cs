using UnityEngine;
using System.Collections;

public class NightWarning : MonoBehaviour
{
    public DayNightCycle dayNightCycle;
    public GameObject warningPanel;

    private void Start()
    {
        warningPanel.SetActive(false);

        dayNightCycle.OnNightApproaching += ShowWarning;
    }

    private void OnDestroy()
    {
        dayNightCycle.OnNightApproaching -= ShowWarning;
    }

    private void ShowWarning()
    {
        StopAllCoroutines();
        StartCoroutine(ShowWarningRoutine());
    }

    IEnumerator ShowWarningRoutine()
    {
        warningPanel.SetActive(true);

        yield return new WaitForSeconds(5f);

        warningPanel.SetActive(false);
    }
}