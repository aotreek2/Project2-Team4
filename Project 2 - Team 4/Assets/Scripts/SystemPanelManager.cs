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

    public void StartRepair()
    {
        isRepairing = true;
        repairProgress = 0f; // Reset repair progress when starting new repair
        repairProgressBar.gameObject.SetActive(true); // Show progress bar during repair
        Debug.Log("Repair started. Waiting for repair to complete.");
    }


    void Update()
    {
        if (isRepairing)
        {
            PerformRepair();
        }
    }

    // Fade-in method for showing the panel and pausing the game at the end of the fade
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

        // Start the fade-in animation and pause the game at the end of the fade
        StartCoroutine(FadeInAndPauseGame(0f, 1f, fadeDuration));

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

        // Do not start the repair here anymore; 
        // Wait until the crew member enters the repair zone.
        Debug.Log("Repair task assigned to crew member. Waiting for crew to reach the repair zone.");
    }


    void OnCloseButtonClicked()
    {
        // Play the close sound
        if (panelAudioSource != null && closeSound != null)
        {
            panelAudioSource.PlayOneShot(closeSound);
        }

        // Start the fade-out animation and resume the game after the fade-out
        StartCoroutine(FadeOutAndResumeGame(1f, 0f, fadeDuration));

        // Hide the fog of war overlay after the panel fades out
        if (fogOfWarOverlay != null)
        {
            fogOfWarOverlay.SetActive(false);
        }

        // Disable the panel's interaction
        panelCanvasGroup.interactable = false;
        panelCanvasGroup.blocksRaycasts = false;
    }

    // Coroutine to fade in the panel and pause the game after fade-in completes
    private IEnumerator FadeInAndPauseGame(float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; // Use unscaled time for fade animation
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            panelCanvasGroup.alpha = alpha;
            yield return null;
        }

        panelCanvasGroup.alpha = endAlpha;

        // Now pause the game after fade-in is complete
        Time.timeScale = 0f;
    }

    // Coroutine to fade out the panel and resume the game after fade-out completes
    private IEnumerator FadeOutAndResumeGame(float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; // Use unscaled time for fade animation
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            panelCanvasGroup.alpha = alpha;
            yield return null;
        }

        panelCanvasGroup.alpha = endAlpha;

        // Resume the game after the fade-out is complete
        Time.timeScale = 1f;
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

    // Set selected crew member method
    public void SetSelectedCrewMember(CrewMember crewMember)
    {
        selectedCrewMember = crewMember;
        Debug.Log("Crew member " + crewMember.crewName + " is set for repair tasks.");
    }
}
