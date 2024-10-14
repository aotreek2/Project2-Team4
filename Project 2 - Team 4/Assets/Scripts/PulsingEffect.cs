using System.Collections;
using UnityEngine;

public class PulsingEffect : MonoBehaviour
{
    public Renderer targetRenderer;
    public Color pulseColor = Color.red; // Color to pulse
    public float pulseSpeed = 2f; // Speed of pulsing effect

    private Color originalColor; // The original emission color of the material
    private Material material;

    void Start()
    {
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>(); // Try to get the renderer if not assigned
        }

        // Get the material and store the original emission color
        material = targetRenderer.material;
        originalColor = material.GetColor("_EmissionColor");
    }

    void Update()
    {
        // Create a pulsing effect by interpolating between the original and pulse color
        float emission = Mathf.PingPong(Time.time * pulseSpeed, 1.0f);
        Color finalColor = originalColor + pulseColor * emission;

        // Set the emission color of the material
        material.SetColor("_EmissionColor", finalColor);
    }
}
