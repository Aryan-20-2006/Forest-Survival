using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FireAudioController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CampfireDeposit campfireDeposit;
    [SerializeField] private GameObject fireVFX;

    [Header("Audio")]
    [SerializeField] private AudioClip fireClip;
    [SerializeField, Range(0f, 1f)] private float maxVolume = 1f;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = fireClip;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
        audioSource.volume = 0f;

        if (campfireDeposit == null)
            campfireDeposit = FindFirstObjectByType<CampfireDeposit>();

        if (fireVFX != null)
            fireVFX.SetActive(false);
    }

    private void Start()
    {
        if (campfireDeposit != null)
            campfireDeposit.OnStoredWoodChanged += OnStoredWoodChanged;
    }

    private void OnDestroy()
    {
        if (campfireDeposit != null)
            campfireDeposit.OnStoredWoodChanged -= OnStoredWoodChanged;
    }

    private void Update()
    {
        if (!audioSource.isPlaying) return;
        audioSource.volume = GetFirePercent() * maxVolume;
    }

    private void OnStoredWoodChanged(int storedWood)
    {
        if (storedWood > 0)
        {
            if (fireVFX != null) fireVFX.SetActive(true);
            if (!audioSource.isPlaying) audioSource.Play();
        }
        else
        {
            if (fireVFX != null) fireVFX.SetActive(false);
            audioSource.Stop();
        }
    }

    private float GetFirePercent()
    {
        if (campfireDeposit == null || campfireDeposit.MaxCapacity <= 0) return 0f;
        return Mathf.Clamp01(campfireDeposit.BurnFuelRemaining / campfireDeposit.MaxCapacity);
    }
}
