using UnityEngine;
using System.Collections;

public class LightFlickerController : MonoBehaviour
{
    public GameObject[] lights; // The lights to flicker
    public Light pointLight; // The main point light over the ship

    // Configurable options
    [Header("Flicker Settings")]
    public float maxFlickerDuration = 8f; // Maximum flicker duration for dramatic flicker
    public float minFlickerOffTime = 0.05f; // Minimum time lights stay off
    public float maxFlickerOffTime = 1.5f; // Maximum time lights stay off based on damage
    public float minFlickerOnTime = 0.05f; // Minimum time lights stay on
    public float maxFlickerOnTime = 0.5f; // Maximum time lights stay on based on damage
    public bool randomizeFlickerTiming = true; // Whether to randomize flicker times per light

    [Header("Light Intensity Settings")]
    public float minLightIntensity = 1.0f; // Minimum intensity when lights are on
    public float maxLightIntensity = 2.5f; // Maximum intensity when lights are on

    private float generatorHealth;
    private float generatorMaxHealth = 100f;
    private bool isFlickering = false;

    public void Initialize(float maxHealth)
    {
        generatorMaxHealth = maxHealth;
        generatorHealth = maxHealth;
    }

    public void UpdateGeneratorHealth(float currentHealth)
    {
        generatorHealth = currentHealth;
        if (!isFlickering && generatorHealth < generatorMaxHealth)
        {
            StartCoroutine(FlickerLoop());
        }
    }

    public void TriggerMinorFlicker()
    {
        StartCoroutine(MinorFlickerCoroutine());
    }

    public void TriggerMajorFlicker()
    {
        float damagePercentage = 1 - (generatorHealth / generatorMaxHealth);
        StartCoroutine(DramaticFlickerLightsCoroutine(damagePercentage));
    }

    private IEnumerator MinorFlickerCoroutine()
    {
        FlickerLights(false);
        yield return new WaitForSeconds(0.1f);
        FlickerLights(true, maxLightIntensity);
    }

    private IEnumerator DramaticFlickerLightsCoroutine(float damagePercentage)
    {
        if (lights == null || lights.Length == 0)
        {
            Debug.LogError("Lights array is null or empty. Cannot flicker lights.");
            yield break;
        }

        isFlickering = true;
        float flickerOffTime = Mathf.Lerp(minFlickerOffTime, maxFlickerOffTime, damagePercentage); // Off time scales with damage
        float flickerOnTime = Mathf.Lerp(minFlickerOnTime, maxFlickerOnTime, damagePercentage); // On time scales with damage

        float elapsed = 0f;

        while (elapsed < maxFlickerDuration)
        {
            // Lights OFF
            if (randomizeFlickerTiming)
            {
                foreach (GameObject lightObject in lights)
                {
                    StartCoroutine(RandomFlickerLight(lightObject, false, Random.Range(0f, 0.2f))); // Random delay before turning off
                }
            }
            else
            {
                FlickerLights(false); // Turn off all lights at once
            }

            float offDuration = Random.Range(flickerOffTime, flickerOffTime * 1.5f);
            elapsed += offDuration;
            yield return new WaitForSeconds(offDuration);

            // Lights ON with random intensity to mimic unstable power
            if (randomizeFlickerTiming)
            {
                foreach (GameObject lightObject in lights)
                {
                    StartCoroutine(RandomFlickerLight(lightObject, true, Random.Range(0f, 0.2f)));
                }
            }
            else
            {
                float intensity = Random.Range(minLightIntensity, maxLightIntensity);
                FlickerLights(true, intensity);
            }

            float onDuration = Random.Range(flickerOnTime, flickerOnTime * 1.2f);
            elapsed += onDuration;
            yield return new WaitForSeconds(onDuration);
        }

        StabilizeLights();
        isFlickering = false;
    }

    private IEnumerator FlickerLoop()
    {
        isFlickering = true;

        while (generatorHealth < generatorMaxHealth && generatorHealth > 0)
        {
            float flickerDuration = Mathf.Lerp(0.5f, 2f, 1 - (generatorHealth / generatorMaxHealth)); // Flicker more often as generator health decreases

            if (randomizeFlickerTiming)
            {
                foreach (GameObject lightObject in lights)
                {
                    StartCoroutine(RandomFlickerLight(lightObject, false, Random.Range(0f, 0.2f)));
                }
            }
            else
            {
                FlickerLights(false);
            }

            yield return new WaitForSeconds(Random.Range(0.05f, flickerDuration));

            if (randomizeFlickerTiming)
            {
                foreach (GameObject lightObject in lights)
                {
                    StartCoroutine(RandomFlickerLight(lightObject, true, Random.Range(0f, 0.2f)));
                }
            }
            else
            {
                float intensity = Random.Range(minLightIntensity, maxLightIntensity);
                FlickerLights(true, intensity);
            }

            yield return new WaitForSeconds(Random.Range(0.05f, flickerDuration));
        }

        StabilizeLights();
        isFlickering = false;
    }

    private IEnumerator RandomFlickerLight(GameObject lightObject, bool turnOn, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (lightObject != null)
        {
            Light lightComponent = lightObject.GetComponent<Light>();
            if (lightComponent != null)
            {
                lightComponent.enabled = turnOn;
                if (turnOn)
                {
                    lightComponent.intensity = Random.Range(minLightIntensity, maxLightIntensity);
                }
            }
        }
    }

    private void FlickerLights(bool turnOn, float intensity = 1.0f)
    {
        foreach (GameObject lightObject in lights)
        {
            if (lightObject != null)
            {
                Light lightComponent = lightObject.GetComponent<Light>();
                if (lightComponent != null)
                {
                    lightComponent.enabled = turnOn;
                    if (turnOn)
                    {
                        lightComponent.intensity = intensity;
                    }
                }
            }
        }
    }

    public void StabilizeLights()
    {
        foreach (GameObject lightObject in lights)
        {
            if (lightObject != null)
            {
                Light lightComponent = lightObject.GetComponent<Light>();
                if (lightComponent != null)
                {
                    lightComponent.enabled = true;
                    lightComponent.intensity = Mathf.Lerp(minLightIntensity, maxLightIntensity, generatorHealth / generatorMaxHealth);
                }
            }
        }

        // Adjust point light intensity and color based on generator health
        if (pointLight != null)
        {
            float intensityPercentage = generatorHealth / generatorMaxHealth;
            pointLight.intensity = Mathf.Lerp(minLightIntensity, maxLightIntensity, intensityPercentage);
            pointLight.color = Color.Lerp(Color.red, Color.white, intensityPercentage);
        }
    }
}
