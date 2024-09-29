using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;


public class SystemPanelManager : MonoBehaviour
{
    // UI Elements
    public TextMeshProUGUI systemNameText;
    public TextMeshProUGUI systemDescriptionText;
    public Button repairButton;
    public Button closeButton;
    public Slider repairProgressBar;

    // Animation and Sound
    public CanvasGroup panelCanvasGroup; // Reference to the CanvasGroup for fade animation
    public AudioSource panelAudioSource; // Reference to AudioSource for playing sounds
    public AudioClip openSound; // Audio clip for opening sound
    public AudioClip closeSound; // Audio clip for closing sound
    public float fadeDuration = 0.5f; // Duration of fade effect

    // Fog of War Effect
    public GameObject fogOfWarOverlay; // Semi-transparent overlay for fog effect

    // References
    private ShipController shipController;
    private CubeInteraction.SystemType currentSystemType;
    private CrewMember selectedCrewMember;

    private bool isRepairing = false;
    private float repairProgress = 0f;
    private float repairDuration = 10f; // You can adjust the repair time here

    void Start()
    {
        // Ensure all necessary UI components are assigned
        if (repairButton == null || closeButton == null || repairProgressBar == null || panelCanvasGroup == null)
        {
            Debug.LogError("UI components not assigned! Check the Inspector.");
            return;
        }

        // Assign button listeners
        repairButton.onClick.AddListener(OnRepairButtonClicked);
        closeButton.onClick.AddListener(OnCloseButtonClicked);

        // Ensure fog of war is hidden at start
        if (fogOfWarOverlay != null)
        {
            fogOfWarOverlay.SetActive(false);
        }

        // Hide the panel and progress bar at the start
        panelCanvasGroup.alpha = 0f;
        panelCanvasGroup.interactable = false;
        panelCanvasGroup.blocksRaycasts = false;
        repairProgressBar.gameObject.SetActive(false);
    }

    void Update()
    {
        if (isRepairing)
        {
            PerformRepair();
        }
    }

    // Fade-in method for showing the panel
   public void OpenSystemPanel(CubeInteraction.SystemType systemType, ShipController controller, CrewMember crewMember, CubeInteraction cubeInteraction)
    {
        currentSystemType = systemType;
        shipController = controller;
        selectedCrewMember = crewMember;

        // Update UI based on the system type
        switch (systemType)
        {
            case CubeInteraction.SystemType.LifeSupport:
                systemNameText.text = "Life Support";
                systemDescriptionText.text = "Maintains oxygen levels for the crew.";
                break;
            case CubeInteraction.SystemType.Engines:
                systemNameText.text = "Engines";
                systemDescriptionText.text = "Propels the ship towards the Lighthouse.";
                break;
            case CubeInteraction.SystemType.Hull:
                systemNameText.text = "Hull";
                systemDescriptionText.text = "Protects the ship from external threats.";
                break;
        }

        // Play the open sound
        if (panelAudioSource != null && openSound != null)
        {
            panelAudioSource.PlayOneShot(openSound);
        }

        // Show the fog of war overlay
        if (fogOfWarOverlay != null)
        {
            fogOfWarOverlay.SetActive(true);
        }

        // Start the fade-in animation
        StartCoroutine(FadePanel(0f, 1f, fadeDuration));

        // Enable the panel's interaction
        panelCanvasGroup.interactable = true;
        panelCanvasGroup.blocksRaycasts = true;
    }

    void OnRepairButtonClicked()
    {
        if (selectedCrewMember == null)
        {
            Debug.LogError("No crew member selected for repair!");
            return;
        }

        // Start repair process
        isRepairing = true;
        repairProgress = 0f; // Reset repair progress when starting new repair
        repairProgressBar.gameObject.SetActive(true); // Show progress bar during repair
    }

    void OnCloseButtonClicked()
    {
        // Play the close sound
        if (panelAudioSource != null && closeSound != null)
        {
            panelAudioSource.PlayOneShot(closeSound);
        }

        // Start the fade-out animation
        StartCoroutine(FadePanel(1f, 0f, fadeDuration));

        // Hide the fog of war overlay after the panel fades out
        if (fogOfWarOverlay != null)
        {
            fogOfWarOverlay.SetActive(false);
        }

        // Disable the panel's interaction
        panelCanvasGroup.interactable = false;
        panelCanvasGroup.blocksRaycasts = false;
    }

    // Coroutine to fade in/out the panel
    IEnumerator FadePanel(float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            panelCanvasGroup.alpha = alpha;
            yield return null;
        }

        panelCanvasGroup.alpha = endAlpha;
    }

    // Repair system method
    private void PerformRepair()
    {
        repairProgress += Time.deltaTime / repairDuration;

        // Update the progress bar
        UpdateRepairProgress(repairProgress);

        if (repairProgress >= 1f)
        {
            float repairAmount = 20f; // Adjust this value
            shipController.RepairSystem(currentSystemType, repairAmount);
            repairProgress = 0f; // Reset for future repairs
            repairProgressBar.gameObject.SetActive(false); // Hide progress bar when done
            isRepairing = false;
        }
    }

    // Method to update the repair progress bar
    public void UpdateRepairProgress(float progress)
    {
        repairProgressBar.value = progress;
    }

    // Add this method to SystemPanelManager.cs
    public void SetSelectedCrewMember(CrewMember crewMember)
    {
        selectedCrewMember = crewMember;
        Debug.Log("Crew member " + crewMember.crewName + " is set for repair tasks.");
    }

}
