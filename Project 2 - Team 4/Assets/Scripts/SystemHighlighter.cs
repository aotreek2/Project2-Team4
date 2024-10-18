using UnityEngine;
using System.Collections.Generic;

public class SystemHighlighter : MonoBehaviour
{
    public Color outlineColor = Color.yellow; // The color for the outline effect
    public float outlineWidth = 0.005f;       // The width of the outline effect
    private Dictionary<Renderer, MaterialPropertyBlock> originalProperties = new Dictionary<Renderer, MaterialPropertyBlock>();

    private GameObject currentHighlightedObject = null;

    void Update()
    {
        // Cast a ray from the camera to detect which object is under the mouse
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            GameObject hoveredObject = hit.collider.gameObject;

            // Only highlight the object if it has the SystemHighlighter script
            SystemHighlighter systemHighlighter = hoveredObject.GetComponent<SystemHighlighter>();
            if (systemHighlighter != null && hoveredObject != currentHighlightedObject)
            {
                HighlightSystem(hoveredObject);
            }
        }
        else if (currentHighlightedObject != null)
        {
            // If no object is under the mouse, stop highlighting the previous object
            StopHighlighting(currentHighlightedObject);
        }
    }

    // Method to highlight the system with outline
    public void HighlightSystem(GameObject system)
    {
        if (currentHighlightedObject != null && currentHighlightedObject != system)
        {
            StopHighlighting(currentHighlightedObject);
        }

        Renderer[] renderers = system.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            foreach (Renderer renderer in renderers)
            {
                if (!originalProperties.ContainsKey(renderer))
                {
                    // Store the original MaterialPropertyBlock
                    MaterialPropertyBlock mpb = new MaterialPropertyBlock();
                    renderer.GetPropertyBlock(mpb);
                    originalProperties[renderer] = mpb;
                }

                // Create a new MaterialPropertyBlock for the outline
                MaterialPropertyBlock outlineMPB = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(outlineMPB);

                // Set the outline color and width
                outlineMPB.SetColor("_OutlineColor", outlineColor);
                outlineMPB.SetFloat("_Outline", outlineWidth);

                // Apply the property block to the renderer
                renderer.SetPropertyBlock(outlineMPB);

                Debug.Log("Applied outline to " + renderer.gameObject.name);
            }
            currentHighlightedObject = system;
            Debug.Log($"{system.name} is now highlighted.");
        }
        else
        {
            Debug.LogError("No renderers found for " + system.name);
        }
    }

    // Method to stop highlighting and return to the default properties
    public void StopHighlighting(GameObject system)
    {
        Renderer[] renderers = system.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (originalProperties.ContainsKey(renderer))
            {
                // Restore the original MaterialPropertyBlock
                renderer.SetPropertyBlock(originalProperties[renderer]);
                originalProperties.Remove(renderer);

                Debug.Log("Restored original properties to " + renderer.gameObject.name);
            }
            else
            {
                Debug.LogWarning("Original properties not found for " + renderer.gameObject.name);
            }
        }
        Debug.Log($"{system.name} has stopped highlighting.");
        currentHighlightedObject = null;
    }
}
