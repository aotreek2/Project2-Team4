using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    public Light flickerLight; // Reference to the light component
    public float minIntensity = 0.5f; // Minimum light intensity
    public float maxIntensity = 1.5f; // Maximum light intensity
    public float flickerSpeed = 0.1f; // Speed at which the light flickers
    public bool useRandomizedSpeed = true; // Option to randomize flicker speed

    private float baseIntensity;
    private float timeCounter = 0f;

    void Start()
    {
        // Set base intensity to the light's current intensity
        baseIntensity = flickerLight.intensity;
    }

    void Update()
    {
        timeCounter += Time.deltaTime;

        if (useRandomizedSpeed)
        {
            // Randomize flicker speed to avoid predictable flickers
            flickerSpeed = Random.Range(0.05f, 0.2f);
        }

        // Add a smooth flicker effect using Perlin noise
        flickerLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, Mathf.PerlinNoise(timeCounter * flickerSpeed, 0.0f));

        // Optional: reset timeCounter to avoid overflow over long periods
        if (timeCounter > 100f)
        {
            timeCounter = 0f;
        }
    }
}
