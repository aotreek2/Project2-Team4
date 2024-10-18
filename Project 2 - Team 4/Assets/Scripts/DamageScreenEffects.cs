using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DamageScreenEffects : MonoBehaviour
{
    public Volume postProcessVolume;  // Volume component to apply post-processing effects
    private Vignette vignette;
    private ChromaticAberration chromaticAberration;

    [Header("Health Settings")]
    public float generatorHealth;
    public float generatorMaxHealth = 100f;

    void Start()
    {
        // Ensure the postProcessVolume is assigned
        if (postProcessVolume == null)
        {
            Debug.LogError("PostProcessVolume is not assigned. Please assign it in the inspector.");
            return;
        }

        // Try to fetch the Vignette and Chromatic Aberration effects from the Volume profile
        if (postProcessVolume.profile.TryGet<Vignette>(out vignette))
        {
            Debug.Log("Vignette found and applied.");
        }
        else
        {
            Debug.LogError("Vignette effect is missing from the PostProcessVolume profile.");
        }

        if (postProcessVolume.profile.TryGet<ChromaticAberration>(out chromaticAberration))
        {
            Debug.Log("Chromatic Aberration found and applied.");
        }
        else
        {
            Debug.LogError("Chromatic Aberration effect is missing from the PostProcessVolume profile.");
        }
    }

    public void UpdateGeneratorHealth(float currentHealth)
    {
        generatorHealth = currentHealth;
        UpdatePostProcessingEffects();
    }

    // Method to dynamically update the intensity of the post-processing effects based on generator health
    private void UpdatePostProcessingEffects()
    {
        if (vignette != null)
        {
            // Intensity scales based on how low the generator health is
            float vignetteIntensity = Mathf.Lerp(0.2f, 0.6f, 1 - (generatorHealth / generatorMaxHealth));
            vignette.intensity.value = vignetteIntensity;
            vignette.color.value = Color.Lerp(Color.black, Color.red, 1 - (generatorHealth / generatorMaxHealth));
        }

        if (chromaticAberration != null)
        {
            // Chromatic aberration increases as health decreases
            chromaticAberration.intensity.value = Mathf.Lerp(0.0f, 1.0f, 1 - (generatorHealth / generatorMaxHealth));
        }
    }
}
