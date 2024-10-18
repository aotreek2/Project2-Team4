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
    public bool randomizeFlickerPattern = true; // Option to randomize flicker behavior

    [Header("Light Intensity Settings")]
    public float minLightIntensity = 1.0f; // Minimum intensity when lights are on
    public float maxLightIntensity = 2.5f; // Maximum intensity when lights are on

    [Header("Lighting Style Settings")]
    public Color damagedLightColor = Color.red;
    public Color stableLightColor = Color.white;
    public float transitionSpeed = 2f; // How fast the lights transition between damaged and stable

    [Header("Emergency Light Settings")]
    public List<Light> emergencyLights = new List<Light>(); // Supports multiple emergency lights
    public AudioSource alarmAudioSource;
    public AudioClip alarmClip;
    public float pulsateSpeed = 2f;
    public float emergencyMinIntensity = 0f;
    public float emergencyMaxIntensity = 5f;
    public KeyCode toggleEmergencyKey = KeyCode.E; // Default key to trigger emergency
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
        // Toggle emergency lights and alarms using the hotkey
        if (Input.GetKeyDown(toggleEmergencyKey))
        {
            emergencyActive = !emergencyActive;
            if (emergencyActive)
            {
                StartEmergency();
            }
            else
            {
                StopEmergency();
            }
        }

        // Handle emergency lights pulsating effect if active
        if (emergencyActive)
        {
            foreach (var light in emergencyLights)
            {
                if (light != null)
                {
                    float intensity = Mathf.Lerp(emergencyMinIntensity, emergencyMaxIntensity, Mathf.PingPong(Time.time * pulsateSpeed, 1));
                    light.intensity = intensity;
                }
            }
        }
    }

    public void Initialize(float maxHealth)
    {
        generatorMaxHealth = maxHealth;
        generatorHealth = maxHealth;
    }

    // Update the generator health and trigger flicker effects based on the health
    public void UpdateGeneratorHealth(float currentHealth)
    {
        generatorHealth = currentHealth;
        UpdateLightingEffects();

        if (!isFlickering && generatorHealth < generatorMaxHealth)
        {
            StartCoroutine(FlickerLoop());
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
        float targetIntensity = Mathf.Lerp(minLightIntensity, maxLightIntensity, healthPercentage);

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
            float intensity = Mathf.Lerp(maxLightIntensity, minLightIntensity, 1 - healthPercentage);

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
            float intensity = Random.Range(minLightIntensity, maxLightIntensity);
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
            }
        }
    }
}
