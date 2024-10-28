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

    // **Added Enum and Property for Selected Option**
    public enum DecisionOption { Option1, Option2 }
    public DecisionOption SelectedOption { get; private set; }

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
            if (shipController == null)
            {
                Debug.LogError("[DecisionPanelManager] ShipController not found in the scene.");
            }
        }

        if (lifeSupportController == null)
        {
            lifeSupportController = FindObjectOfType<LifeSupportController>();
            if (lifeSupportController == null)
            {
                Debug.LogError("[DecisionPanelManager] LifeSupportController not found in the scene.");
            }
        }

        if (hullSystemController == null)
        {
            hullSystemController = FindObjectOfType<HullSystemController>();
            if (hullSystemController == null)
            {
                Debug.LogError("[DecisionPanelManager] HullSystemController not found in the scene.");
            }
        }

        if (engineSystemController == null)
        {
            engineSystemController = FindObjectOfType<EngineSystemController>();
            if (engineSystemController == null)
            {
                Debug.LogError("[DecisionPanelManager] EngineSystemController not found in the scene.");
            }
        }

        // **Initialize ChapterManager**
        chapterManager = FindObjectOfType<ChapterManager>();
        if (chapterManager == null)
        {
            Debug.LogError("[DecisionPanelManager] ChapterManager not found in the scene.");
        }
    }

    // Method to open the decision panel with options and fade-in effect
    public void OpenDecisionPanel(string eventDescription, string option1, string option2, ShipController controller)
    {
        if (decisionMade)
        {
            Debug.LogWarning("[DecisionPanelManager] Decision panel is already open.");
            return;
        }

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
        if (decisionMade)
        {
            Debug.LogWarning("[DecisionPanelManager] Decision has already been made.");
            return;
        }

        decisionMade = true;
        SelectedOption = DecisionOption.Option1;

        switch (chapterManager.currentChapter)
        {
            case ChapterManager.Chapter.Chapter2:
                // Handle Chapter 2, Option 1
                shipController.SacrificeCrew(5);
                hullSystemController.RepairHull(20f);
                if (shipController.resourceManager != null)
                {
                    shipController.resourceManager.UpdateResourceUI();
                }
                Debug.Log("Diverted power to shields. Sacrificed 5 crew.");
                break;
            case ChapterManager.Chapter.Chapter3:
                // Handle Chapter 3, Option 1
                if (shipController.resourceManager != null)
                {
                    shipController.resourceManager.fuelAmount -= 20f;
                    shipController.resourceManager.UpdateResourceUI();
                }
                engineSystemController.RepairEngine(20f);
                Debug.Log("Boosted engines. Used extra fuel.");
                break;
            case ChapterManager.Chapter.Chapter4:
                // Handle Chapter 4, Option 1 (End game)
                SceneManager.LoadScene("MainMenu");
                Debug.Log("Docked safely. Game ends.");
                // Implement end-game logic here
                break;
            default:
                Debug.LogWarning("[DecisionPanelManager] Option 1 selected in an unsupported chapter.");
                break;
        }

        CloseDecisionPanel();
    }

    // Handles option 2 being selected
    private void OnOption2Selected()
    {
        if (decisionMade)
        {
            Debug.LogWarning("[DecisionPanelManager] Decision has already been made.");
            return;
        }

        decisionMade = true;
        SelectedOption = DecisionOption.Option2;

        switch (chapterManager.currentChapter)
        {
            case ChapterManager.Chapter.Chapter2:
                // Handle Chapter 2, Option 2
                shipController.ApplyHullDamage(20f); // Now defined in ShipController
                Debug.Log("Navigated carefully. Applied 20% hull damage risk.");
                break;
            case ChapterManager.Chapter.Chapter3:
                // Handle Chapter 3, Option 2
                engineSystemController.DamageEngine(30f);
                Debug.Log("Rode gravitational pull. Engine damaged.");
                break;
            case ChapterManager.Chapter.Chapter4:
                // Handle Chapter 4, Option 2 (Start new journey)
                SceneManager.LoadScene("Chapter1Scene"); // Ensure correct scene name
                Debug.Log("Broadcasted message and left. New journey begins.");
                // Implement new journey logic here
                break;
            default:
                Debug.LogWarning("[DecisionPanelManager] Option 2 selected in an unsupported chapter.");
                break;
        }

        if (shipController.resourceManager != null)
        {
            shipController.resourceManager.UpdateResourceUI();
        }

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
