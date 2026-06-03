using UnityEngine;
using System;

public class DayNightCycle : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Light directionalLight;

    [Header("Cycle")]
    [Tooltip("How long a full day-night cycle lasts in real time, in minutes.")]
    [SerializeField, Min(0.1f)] private float dayLengthMinutes = 10f;
    [Range(0f, 24f)]
    [SerializeField] private float startTimeOfDay = 8f;
    [SerializeField] private bool startPaused;

    [Header("Day / Night Thresholds")]
    [Range(0f, 24f)]
    [SerializeField] private float dayStartsAt = 6f;
    [Range(0f, 24f)]
    [SerializeField] private float nightStartsAt = 18f;
    [Tooltip("How many game-hours before nightStartsAt the OnNightApproaching event fires.")]
    [SerializeField, Min(0f)] private float nightWarningHoursBefore = 1f;

    [Header("Lighting")]
    [SerializeField] private Gradient lightColorOverDay;
    [SerializeField] private Gradient ambientColorOverDay;
    [SerializeField, Min(0f)] private float maxLightIntensity = 1.2f;
    [SerializeField, Min(0f)] private float nightLightIntensity = 0f;
    [SerializeField, Min(0.01f)] private float lightSmoothing = 2f;

    [Header("Fog")]
    [SerializeField] private bool driveFog = true;
    [SerializeField, Min(0f)] private float dayFogDensity = 0.002f;
    [SerializeField, Min(0f)] private float nightFogDensity = 0.04f;
    [SerializeField, Min(0.01f)] private float fogSmoothing = 1.5f;

    [Header("Skybox")]
    [SerializeField] private bool driveSkyboxExposure = true;
    [SerializeField, Min(0f)] private float daySkyboxExposure = 1.1f;
    [SerializeField, Min(0f)] private float nightSkyboxExposure = 0.2f;
    [SerializeField, Min(0.01f)] private float skyboxSmoothing = 1.5f;

    [Header("Sun Movement")]
    [SerializeField, Range(0f, 360f)] private float sunriseAngle = -90f;
    [SerializeField, Range(0f, 360f)] private float sunsetAngle = 270f;

    private float timeOfDay;
    private bool paused;
    private float currentSkyboxExposure;
    private bool isNight;
    private bool isNightApproaching;

    public event Action OnNightStarted;
    public event Action OnNightEnded;
    public event Action OnNightApproaching;

    private void Awake()
    {
        if (directionalLight == null)
        {
            directionalLight = RenderSettings.sun;
        }

        timeOfDay = Mathf.Repeat(startTimeOfDay, 24f);
        paused = startPaused;
        isNight = IsNightTime(timeOfDay);
        isNightApproaching = IsNightApproachingTime(timeOfDay);
        currentSkyboxExposure = GetSkyboxExposure();
        ApplyLightingImmediate();
    }

    private void Update()
    {
        if (!paused)
        {
            AdvanceTime();
        }

        ApplyLighting(Time.deltaTime);
    }

    public void SetPaused(bool value)
    {
        paused = value;
    }

    public void SetTimeOfDay(float hours)
    {
        timeOfDay = Mathf.Repeat(hours, 24f);
        isNight = IsNightTime(timeOfDay);
        ApplyLightingImmediate();
    }

    private void AdvanceTime()
    {
        bool wasNight = isNight;
        bool wasApproaching = isNightApproaching;
        float hoursPerSecond = 24f / Mathf.Max(0.1f, dayLengthMinutes * 60f);
        timeOfDay = Mathf.Repeat(timeOfDay + hoursPerSecond * Time.deltaTime, 24f);

        isNight = IsNightTime(timeOfDay);
        isNightApproaching = IsNightApproachingTime(timeOfDay);

        if (!wasApproaching && isNightApproaching)
            OnNightApproaching?.Invoke();

        if (!wasNight && isNight)
            OnNightStarted?.Invoke();

        if (wasNight && !isNight)
        {
            isNightApproaching = false;
            OnNightEnded?.Invoke();
        }
    }

    private void ApplyLighting(float deltaTime)
    {
        float dayT = GetDayFactor(timeOfDay);

        if (directionalLight != null)
        {
            directionalLight.transform.rotation = Quaternion.Euler(GetSunPitch(timeOfDay), 170f, 0f);

            Color targetColor = lightColorOverDay != null && lightColorOverDay.colorKeys.Length > 0
                ? lightColorOverDay.Evaluate(dayT)
                : Color.white;

            float targetIntensity = Mathf.Lerp(nightLightIntensity, maxLightIntensity, dayT);

            directionalLight.color = Color.Lerp(directionalLight.color, targetColor, 1f - Mathf.Exp(-lightSmoothing * deltaTime));
            directionalLight.intensity = Mathf.Lerp(directionalLight.intensity, targetIntensity, 1f - Mathf.Exp(-lightSmoothing * deltaTime));
        }

        RenderSettings.ambientLight = ambientColorOverDay != null && ambientColorOverDay.colorKeys.Length > 0
            ? ambientColorOverDay.Evaluate(dayT)
            : RenderSettings.ambientLight;

        if (driveSkyboxExposure)
        {
            float targetExposure = Mathf.Lerp(nightSkyboxExposure, daySkyboxExposure, dayT);
            currentSkyboxExposure = Mathf.Lerp(currentSkyboxExposure, targetExposure, 1f - Mathf.Exp(-skyboxSmoothing * deltaTime));
            SetSkyboxExposure(currentSkyboxExposure);
        }

        if (driveFog)
        {
            RenderSettings.fog = true;
            float targetDensity = Mathf.Lerp(nightFogDensity, dayFogDensity, dayT);
            RenderSettings.fogDensity = Mathf.Lerp(RenderSettings.fogDensity, targetDensity, 1f - Mathf.Exp(-fogSmoothing * deltaTime));
        }
    }

    private void ApplyLightingImmediate()
    {
        float dayT = GetDayFactor(timeOfDay);

        if (directionalLight != null)
        {
            directionalLight.transform.rotation = Quaternion.Euler(GetSunPitch(timeOfDay), 170f, 0f);
            directionalLight.color = lightColorOverDay != null && lightColorOverDay.colorKeys.Length > 0
                ? lightColorOverDay.Evaluate(dayT)
                : Color.white;
            directionalLight.intensity = Mathf.Lerp(nightLightIntensity, maxLightIntensity, dayT);
        }

        if (ambientColorOverDay != null && ambientColorOverDay.colorKeys.Length > 0)
        {
            RenderSettings.ambientLight = ambientColorOverDay.Evaluate(dayT);
        }

        if (driveSkyboxExposure)
        {
            currentSkyboxExposure = Mathf.Lerp(nightSkyboxExposure, daySkyboxExposure, dayT);
            SetSkyboxExposure(currentSkyboxExposure);
        }

        if (driveFog)
        {
            RenderSettings.fog = true;
            RenderSettings.fogDensity = Mathf.Lerp(nightFogDensity, dayFogDensity, dayT);
        }
    }

    private float GetDayFactor(float hour)
    {
        // 0 at midnight, 1 at midday, 0 at next midnight
        float angle = (hour / 24f) * Mathf.PI * 2f;
        return Mathf.Clamp01(Mathf.Sin(angle - Mathf.PI * 0.5f) * 0.5f + 0.5f);
    }

    public bool IsNightTime()
    {
        return IsNightTime(timeOfDay);
    }

    private bool IsNightTime(float hour)
    {
        if (dayStartsAt <= nightStartsAt)
        {
            return hour < dayStartsAt || hour >= nightStartsAt;
        }

        return hour >= nightStartsAt && hour < dayStartsAt;
    }

    private bool IsNightApproachingTime(float hour)
    {
        if (IsNightTime(hour) || nightWarningHoursBefore <= 0f) return false;
        float threshold = Mathf.Repeat(nightStartsAt - nightWarningHoursBefore, 24f);
        float distToThreshold = Mathf.Repeat(hour - threshold, 24f);
        float windowSize = Mathf.Repeat(nightStartsAt - threshold, 24f);
        return distToThreshold < windowSize;
    }

    private float GetSunPitch(float hour)
    {
        float t = hour / 24f;
        return Mathf.Lerp(sunriseAngle, sunsetAngle, t);
    }

    private float GetSkyboxExposure()
    {
        Material skybox = RenderSettings.skybox;
        if (skybox != null && skybox.HasProperty("_Exposure"))
        {
            return skybox.GetFloat("_Exposure");
        }

        return daySkyboxExposure;
    }

    private void SetSkyboxExposure(float value)
    {
        Material skybox = RenderSettings.skybox;
        if (skybox == null)
        {
            return;
        }

        if (skybox.HasProperty("_Exposure"))
        {
            skybox.SetFloat("_Exposure", value);
            DynamicGI.UpdateEnvironment();
        }
    }
}
