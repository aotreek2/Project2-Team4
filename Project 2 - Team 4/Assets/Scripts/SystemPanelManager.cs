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
    public Button sacrificeButton;

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
        sacrificeButton.onClick.AddListener(OnSacrificeButtonClicked); // New listener
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

    public void OpenSystemPanel(CubeInteraction.SystemType systemType, ShipController controller)
    {
        currentSystemType = systemType;
        shipController = controller;

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
            case CubeInteraction.SystemType.Generator:
                systemNameText.text = "Generator";
                systemDescriptionText.text = "Provides power to ship systems.";
                break;
            default:
                systemNameText.text = "Unknown System";
                systemDescriptionText.text = "No description available.";
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

        // Hide the repair progress bar initially
        repairProgressBar.gameObject.SetActive(false);
    }

    void OnRepairButtonClicked()
    {
        // The repair process is now handled when the crew member reaches the system
        Debug.Log("Repair task assigned. Assign a crew member to perform the repair.");
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

    // Method to update the repair progress bar
    public void UpdateRepairProgress(float progress)
    {
        repairProgressBar.value = progress;

        if (progress >= 1f || progress <= 0f)
        {
            repairProgressBar.gameObject.SetActive(false); // Hide progress bar when repair is done
        }
        else
        {
            repairProgressBar.gameObject.SetActive(true); // Show progress bar during repair
        }
    }

    void OnSacrificeButtonClicked()
    {
        // Sacrifice crew to instantly repair the system
        shipController.SacrificeCrewForRepair(5, currentSystemType);
        Debug.Log("Crew sacrificed to repair the system.");

        ClosePanelWithoutFade();
    }

    void ClosePanelWithoutFade()
    {
        // Close the panel immediately without fade
        panelCanvasGroup.alpha = 0f;
        panelCanvasGroup.interactable = false;
        panelCanvasGroup.blocksRaycasts = false;
        Time.timeScale = 1f;

        // Hide the fog of war overlay
        if (fogOfWarOverlay != null)
        {
            fogOfWarOverlay.SetActive(false);
        }
    }
}
