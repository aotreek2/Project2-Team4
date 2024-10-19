using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LightFlickerController : MonoBehaviour
{
    public GameObject[] lights;
    public Light pointLight;

    [Header("Flicker Settings")]
    public AnimationCurve flickerFrequencyCurve;
    public AnimationCurve flickerIntensityCurve;
    public bool randomizeFlickerPattern = true;

    [Header("Light Intensity Settings")]
    public float minLightIntensity = 0.5f;
    public float maxLightIntensity = 3.0f;

    [Header("Lighting Style Settings")]
    public Color damagedLightColor = Color.red;
    public Color stableLightColor = Color.white;
    public float transitionSpeed = 2f;

    [Header("Emergency Light Settings")]
    public List<Light> emergencyLights = new List<Light>();
    public AudioSource alarmAudioSource;
    public AudioClip alarmClip;
    public float pulsateSpeed = 2f;
    public float emergencyMinIntensity = 1f;
    public float emergencyMaxIntensity = 5f;
    public Color emergencyLightColor = Color.red; // Set the emergency light color to red
    public float criticalHealthThreshold = 20f;
    private bool emergencyActive = false;
    private bool isAlarmPlaying = false;

    private Light[] lightComponents;
    private float generatorHealth;
    private float generatorMaxHealth = 100f;
    private bool isFlickering = false;

    void Awake()
    {
        // Cache Light components
        lightComponents = new Light[lights.Length];
        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i] != null)
            {
                lightComponents[i] = lights[i].GetComponent<Light>();
                if (lightComponents[i] == null)
                {
                    Debug.LogError("No Light component on " + lights[i].name);
                }
            }
        }
    }

    void Update()
    {
        // Handle emergency lights pulsating effect if active
        if (emergencyActive)
        {
            foreach (var light in emergencyLights)
            {
                if (light != null)
                {
                    float intensity = Mathf.Lerp(emergencyMinIntensity, emergencyMaxIntensity, Mathf.PingPong(Time.time * pulsateSpeed, 1));
                    light.intensity = intensity;
                    light.color = emergencyLightColor; // Ensure emergency lights are red
                }
            }
        }
    }

    public void Initialize(float maxHealth)
    {
        generatorMaxHealth = maxHealth;
        generatorHealth = maxHealth;
    }

    // Update the generator health and trigger flicker and emergency effects based on health
    public void UpdateGeneratorHealth(float currentHealth)
    {
        generatorHealth = currentHealth;
        UpdateLightingEffects();

        if (!isFlickering && generatorHealth < generatorMaxHealth)
        {
            StartCoroutine(FlickerLoop());
        }

        // Check if emergency lights should be triggered
        if (generatorHealth <= criticalHealthThreshold && !emergencyActive)
        {
            StartEmergency();
        }
        else if (generatorHealth > criticalHealthThreshold && emergencyActive)
        {
            StopEmergency();
        }
    }

    // Triggers a major flicker effect based on damage percentage
    public void TriggerMajorFlicker()
    {
        float damagePercentage = 1 - (generatorHealth / generatorMaxHealth);
        StartCoroutine(DramaticFlickerLightsCoroutine(damagePercentage));
    }

    // Smoothly transition between damaged and stable light states
    private void UpdateLightingEffects()
    {
        float healthPercentage = generatorHealth / generatorMaxHealth;
        Color targetColor = Color.Lerp(damagedLightColor, stableLightColor, healthPercentage);
        float targetIntensity = Mathf.Clamp(Mathf.Lerp(minLightIntensity, maxLightIntensity, healthPercentage), minLightIntensity, maxLightIntensity);

        foreach (var light in lightComponents)
        {
            if (light != null)
            {
                light.color = Color.Lerp(light.color, targetColor, Time.deltaTime * transitionSpeed);
                light.intensity = Mathf.Lerp(light.intensity, targetIntensity, Time.deltaTime * transitionSpeed);
            }
        }

        if (pointLight != null)
        {
            pointLight.color = Color.Lerp(pointLight.color, targetColor, Time.deltaTime * transitionSpeed);
            pointLight.intensity = Mathf.Lerp(pointLight.intensity, targetIntensity, Time.deltaTime * transitionSpeed);
        }
    }

    private IEnumerator FlickerLoop()
    {
        isFlickering = true;

        while (generatorHealth < generatorMaxHealth && generatorHealth > 0)
        {
            float healthPercentage = generatorHealth / generatorMaxHealth;
            float flickerDuration = Mathf.Lerp(2f, 0.1f, 1 - healthPercentage);
            float intensity = Mathf.Clamp(Mathf.Lerp(maxLightIntensity, minLightIntensity, 1 - healthPercentage), minLightIntensity, maxLightIntensity);

            float flickerInterval = Random.Range(0.05f, flickerDuration);

            // Toggle lights off
            SetLightsEnabled(false);
            yield return new WaitForSeconds(flickerInterval);

            // Toggle lights on with adjusted intensity
            SetLightsEnabled(true, intensity);
            yield return new WaitForSeconds(flickerInterval);
        }

        StabilizeLights();
        isFlickering = false;
    }

    // More dynamic flicker patterns with randomization
    private IEnumerator DramaticFlickerLightsCoroutine(float damagePercentage)
    {
        if (lightComponents == null || lightComponents.Length == 0)
        {
            Debug.LogError("Lights array is null or empty. Cannot flicker lights.");
            yield break;
        }

        isFlickering = true;
        float flickerOffTime = Mathf.Lerp(0.05f, 0.5f, damagePercentage);
        float flickerOnTime = Mathf.Lerp(0.05f, 0.2f, damagePercentage);
        float elapsed = 0f;
        float maxFlickerDuration = 8f;

        while (elapsed < maxFlickerDuration)
        {
            // Lights OFF
            SetLightsEnabled(false);
            float offDuration = Random.Range(flickerOffTime, flickerOffTime * 1.5f);
            elapsed += offDuration;
            yield return new WaitForSeconds(offDuration);

            // Lights ON with random intensity to mimic unstable power
            float intensity = Mathf.Clamp(Random.Range(minLightIntensity, maxLightIntensity), minLightIntensity, maxLightIntensity);
            SetLightsEnabled(true, intensity);
            float onDuration = Random.Range(flickerOnTime, flickerOnTime * 1.2f);
            elapsed += onDuration;
            yield return new WaitForSeconds(onDuration);
        }

        StabilizeLights();
        isFlickering = false;
    }

    // Set lights enabled or disabled
    private void SetLightsEnabled(bool enabled, float intensity = 1f)
    {
        foreach (var light in lightComponents)
        {
            if (light != null)
            {
                light.enabled = enabled;
                if (enabled)
                {
                    light.intensity = intensity;
                }
            }
        }
    }

    // Stabilize the lights after flickering
    public void StabilizeLights()
    {
        SetLightsEnabled(true, maxLightIntensity);
        UpdateLightingEffects();
    }

    // Start emergency lights and alarm
    private void StartEmergency()
    {
        emergencyActive = true;

        foreach (var light in emergencyLights)
        {
            if (light != null)
            {
                light.color = emergencyLightColor; // Set emergency light color to red
                light.enabled = true; // Ensure emergency lights are turned on
            }
        }

        if (!isAlarmPlaying && alarmAudioSource != null && alarmClip != null)
        {
            alarmAudioSource.clip = alarmClip;
            alarmAudioSource.loop = true;
            alarmAudioSource.Play();
            isAlarmPlaying = true;
        }
    }

    // Stop emergency lights and alarm
    private void StopEmergency()
    {
        emergencyActive = false;

        if (isAlarmPlaying && alarmAudioSource != null)
        {
            alarmAudioSource.Stop();
            isAlarmPlaying = false;
        }

        foreach (var light in emergencyLights)
        {
            if (light != null)
            {
                light.intensity = emergencyMinIntensity; // Reset emergency light intensity
                light.enabled = false; // Turn off emergency lights
            }
        }
    }
}
