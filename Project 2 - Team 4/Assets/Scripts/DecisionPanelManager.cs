using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class DecisionPanelManager : MonoBehaviour
{
    public CanvasGroup decisionPanelCanvasGroup; // For fade animation
    public AudioSource decisionAudioSource; // For playing sounds
    public AudioClip decisionOpenSound; // Sound when panel opens
    public AudioClip decisionCloseSound; // Sound when panel closes
    public Image darkOverlay; // Fog of war effect
    public TextMeshProUGUI decisionDescriptionText; // Description of the decision
    public TextMeshProUGUI option1Text; // Text for Option 1
    public TextMeshProUGUI option2Text; // Text for Option 2
    public Button option1Button; // First decision button
    public Button option2Button; // Second decision button

    private ShipController shipController;
    private LifeSupportController lifeSupportController; // Reference to LifeSupportController
    private HullSystemController hullSystemController; // Reference to HullSystemController
    private EngineSystemController engineSystemController; // Reference to EngineSystemController
    private string currentEvent;

    // **Added Variables for Chapter System**
    private bool decisionMade = false;
    public bool IsDecisionMade => decisionMade;
    private ChapterManager chapterManager;

    void Start()
    {
        // Ensure the panel is inactive and hidden at the start
        decisionPanelCanvasGroup.alpha = 0f;
        decisionPanelCanvasGroup.interactable = false;
        decisionPanelCanvasGroup.blocksRaycasts = false;

        if (darkOverlay != null)
        {
            darkOverlay.gameObject.SetActive(false); // Hide overlay at start
        }

        // Assign button click listeners
        option1Button.onClick.AddListener(OnOption1Selected);
        option2Button.onClick.AddListener(OnOption2Selected);

        // Find controllers if not assigned
        if (shipController == null)
        {
            shipController = FindObjectOfType<ShipController>();
        }

        if (lifeSupportController == null)
        {
            lifeSupportController = FindObjectOfType<LifeSupportController>();
        }

        if (hullSystemController == null)
        {
            hullSystemController = FindObjectOfType<HullSystemController>();
        }

        if (engineSystemController == null)
        {
            engineSystemController = FindObjectOfType<EngineSystemController>();
        }

        // **Initialize ChapterManager**
        chapterManager = FindObjectOfType<ChapterManager>();
    }

    // Method to open the decision panel with options and fade-in effect
    public void OpenDecisionPanel(string eventDescription, string option1, string option2, ShipController controller)
    {
        decisionMade = false; // Reset decision flag
        shipController = controller;
        currentEvent = eventDescription.ToLower(); // Convert to lower case for easier comparison

        // Set the description text based on the event
        decisionDescriptionText.text = eventDescription;
        option1Text.text = option1; // Set Option 1 text
        option2Text.text = option2; // Set Option 2 text

        // Play open sound
        if (decisionAudioSource != null && decisionOpenSound != null)
        {
            decisionAudioSource.PlayOneShot(decisionOpenSound);
        }

        // Show fog of war
        if (darkOverlay != null)
        {
            darkOverlay.gameObject.SetActive(true);
        }

        // Start fade-in animation and pause the game at the end of the fade-in
        StartCoroutine(FadeInAndPauseGame(0f, 1f, 0.5f)); // 0.5s fade duration

        decisionPanelCanvasGroup.interactable = true;
        decisionPanelCanvasGroup.blocksRaycasts = true;
    }

    // Handles option 1 being selected
    private void OnOption1Selected()
    {
        decisionMade = true;

        switch (chapterManager.currentChapter)
        {
            case ChapterManager.Chapter.Chapter2:
                // Handle Chapter 2, Option 1
                shipController.SacrificeCrew(5);
                hullSystemController.RepairHull(20f);
                shipController.resourceManager.UpdateResourceUI();
                Debug.Log("Diverted power to shields. Sacrificed 5 crew.");
                break;
            case ChapterManager.Chapter.Chapter3:
                // Handle Chapter 3, Option 1
                shipController.resourceManager.fuelAmount -= 20f;
                engineSystemController.RepairEngine(20f);
                shipController.resourceManager.UpdateResourceUI();
                Debug.Log("Boosted engines. Used extra fuel.");
                break;
            case ChapterManager.Chapter.Chapter4:
                // Handle Chapter 4, Option 1 (End game)
                SceneManager.LoadScene("MainMenu");
                Debug.Log("Docked safely. Game ends.");
                // Implement end-game logic
                break;
        }

        CloseDecisionPanel();
    }

    private void OnOption2Selected()
    {
        decisionMade = true;

        switch (chapterManager.currentChapter)
        {
            case ChapterManager.Chapter.Chapter2:
                // Handle Chapter 2, Option 2
                hullSystemController.ReduceHullIntegrity(50f);
                Debug.Log("Navigated carefully. Hull damaged.");
                break;
            case ChapterManager.Chapter.Chapter3:
                // Handle Chapter 3, Option 2
                engineSystemController.DamageEngine(30f);
                Debug.Log("Rode gravitational pull. Engine damaged.");
                break;
            case ChapterManager.Chapter.Chapter4:
                // Handle Chapter 4, Option 2 (Start new journey)
                SceneManager.LoadScene("Chapter 1");
                Debug.Log("Broadcasted message and left. New journey begins.");
                // Implement new journey logic
                break;
        }

        shipController.resourceManager.UpdateResourceUI();
        CloseDecisionPanel();
    }

    // Coroutine to fade in the panel and pause the game when the fade-in completes
    private IEnumerator FadeInAndPauseGame(float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; // Use unscaled time for fade animation
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            decisionPanelCanvasGroup.alpha = alpha;
            yield return null;
        }

        decisionPanelCanvasGroup.alpha = endAlpha;

        // Now pause the game after fade-in is complete
        Time.timeScale = 0f;
    }

    // Coroutine to fade out the panel and resume the game when the fade-out completes
    private IEnumerator FadeOutAndResumeGame(float startAlpha, float endAlpha, float duration)
    {
        Time.timeScale = 1f; // Ensure game resumes during the fade-out process

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; // Use unscaled time for fade animation
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            decisionPanelCanvasGroup.alpha = alpha;
            yield return null;
        }

        decisionPanelCanvasGroup.alpha = endAlpha;
    }

    public void CloseDecisionPanel()
    {
        // Close the decision panel by fading it out and resuming the game
        StartCoroutine(FadeOutAndResumeGame(1f, 0f, 0.5f)); // 0.5s fade duration

        // Hide the dark overlay
        if (darkOverlay != null)
        {
            darkOverlay.gameObject.SetActive(false);
        }

        decisionPanelCanvasGroup.interactable = false;
        decisionPanelCanvasGroup.blocksRaycasts = false;
    }
}
