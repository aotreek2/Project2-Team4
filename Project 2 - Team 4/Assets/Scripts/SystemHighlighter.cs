using UnityEngine;

public class SystemHighlighter : MonoBehaviour
{
    public Material highlightMaterial; // The material for pulsing effect
    private Material defaultMaterial;  // The system's original material
    private Renderer systemRenderer;   // Reference to the Renderer component

    void Start()
    {
        // Get the Renderer component from the system object
        systemRenderer = GetComponent<Renderer>();

        // Check if the systemRenderer exists and has a material
        if (systemRenderer != null)
        {
            // Store the system's current material as the default
            defaultMaterial = systemRenderer.material;
        }
        else
        {
            Debug.LogError("Renderer not found on system object.");
        }
    }

    // Method to highlight the system (apply pulsing material)
    public void HighlightSystem(GameObject system)
    {
        Renderer renderer = system.GetComponent<Renderer>();
        if (renderer != null && highlightMaterial != null)
        {
            // Apply the highlight material
            renderer.material = highlightMaterial;
        }
    }

    // Method to stop highlighting and return to the default material
    public void StopHighlighting(GameObject system)
    {
        Renderer renderer = system.GetComponent<Renderer>();
        if (renderer != null && defaultMaterial != null)
        {
            // Revert to the default material
            renderer.material = defaultMaterial;
        }
    }
}
