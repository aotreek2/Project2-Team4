using UnityEngine;
using UnityEngine.UI;

public class RepairProgressBar : MonoBehaviour
{
    public Slider repairSlider; // The UI slider for the progress bar
    public Vector3 offset = new Vector3(0, 2, 0); // Offset above the system
    public Transform cameraTransform; // Reference to the main camera transform

    private CubeInteraction systemBeingRepaired; // Reference to the system being repaired

    void Start()
    {
        // Find the system object this repair bar is attached to
        systemBeingRepaired = GetComponentInParent<CubeInteraction>();

        if (systemBeingRepaired == null)
        {
            Debug.LogError("CubeInteraction not found on parent. Please make sure this RepairProgressBar is attached to the right object.");
            return; // Exit if systemBeingRepaired is not found
        }

        if (repairSlider == null)
        {
            Debug.LogError("Repair slider not assigned.");
        }

        // If the camera transform is not assigned, default to the main camera
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main?.transform;
            if (cameraTransform == null)
            {
                Debug.LogError("Main Camera not found in the scene. Make sure you have a camera assigned.");
            }
        }

        // Hide the progress bar initially, it should only appear when repairs are ongoing
        gameObject.SetActive(false);
    }

    void Update()
    {
        // Keep the progress bar above the system and make it face the camera
        if (cameraTransform != null && systemBeingRepaired != null)
        {
            AlignWithSystem();
        }
    }

    public void UpdateRepairProgress(float progress)
    {
        // Update the slider value based on repair progress (value between 0 and 1)
        repairSlider.value = progress;
    }

    public void ResetProgress()
    {
        // Reset the slider value to 0
        if (repairSlider != null)
        {
            repairSlider.value = 0f;
        }
    }

    void AlignWithSystem()
    {
        // Adjust the position of the repair bar to hover above the system
        transform.position = systemBeingRepaired.transform.position + offset;

        // Make the repair bar face the camera
        transform.LookAt(transform.position + cameraTransform.forward);
    }
}
