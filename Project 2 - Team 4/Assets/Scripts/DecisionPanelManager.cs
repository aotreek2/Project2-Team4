using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class DecisionPanelManager : MonoBehaviour
{
    [Header("UI Components")]
    public CanvasGroup decisionPanelCanvasGroup; // For fade animation
    public Image darkOverlay; // Fog of war effect
    public TextMeshProUGUI decisionDescriptionText; // Description of the decision
    public TextMeshProUGUI option1Text; // Text for Option 1
    public TextMeshProUGUI option2Text; // Text for Option 2
    public Button option1Button; // First decision button
    public Button option2Button; // Second decision button

    [Header("Audio Components")]
    public AudioSource decisionAudioSource; // For playing sounds
    public AudioClip decisionOpenSound; // Sound when panel opens
    public AudioClip decisionCloseSound; // Sound when panel closes

    [Header("Controllers")]
    public ShipController shipController;
    public CameraController cameraController; // Reference to CameraController

    private LifeSupportController lifeSupportController; // Reference to LifeSupportController
    private HullSystemController hullSystemController; // Reference to HullSystemController
    private EngineSystemController engineSystemController; // Reference to EngineSystemController
    public bool isDecisionPanelOpen => decisionPanelCanvasGroup != null && decisionPanelCanvasGroup.interactable;

    private ChapterManager chapterManager;

    // **Variables for Chapter System**
    private bool decisionMade = false;
    public bool IsDecisionMade => decisionMade;

    // **Enum and Property for Selected Option**
    public enum DecisionOption { Option1, Option2 }
    public DecisionOption SelectedOption { get; private set; }

    void Start()
    {
        // Initialize UI to be hidden
        HideDecisionPanelInstantly();

        // Assign button click listeners
        option1Button.onClick.AddListener(OnOption1Selected);
        option2Button.onClick.AddListener(OnOption2Selected);

        // Find Controllers
        if (shipController == null)
        {
            shipController = FindObjectOfType<ShipController>();
            if (shipController == null)
            {
                Debug.LogError("[DecisionPanelManager] ShipController not found in the scene.");
            }
        }

        lifeSupportController = FindObjectOfType<LifeSupportController>();
        if (lifeSupportController == null)
        {
            Debug.LogError("[DecisionPanelManager] LifeSupportController not found in the scene.");
        }

        hullSystemController = FindObjectOfType<HullSystemController>();
        if (hullSystemController == null)
        {
            Debug.LogError("[DecisionPanelManager] HullSystemController not found in the scene.");
        }

        engineSystemController = FindObjectOfType<EngineSystemController>();
        if (engineSystemController == null)
        {
            Debug.LogError("[DecisionPanelManager] EngineSystemController not found in the scene.");
        }

        // Initialize ChapterManager
        chapterManager = FindObjectOfType<ChapterManager>();
        if (chapterManager == null)
        {
            Debug.LogError("[DecisionPanelManager] ChapterManager not found in the scene.");
        }

        // Assign CameraController via Inspector or find it
        if (cameraController == null)
        {
            cameraController = FindObjectOfType<CameraController>();
            if (cameraController == null)
            {
                Debug.LogError("[DecisionPanelManager] CameraController not found in the scene.");
            }
        }
        else
        {
            Debug.Log("[DecisionPanelManager] CameraController assigned via Inspector.");
        }
    }

    /// <summary>
    /// Instantly hides the decision panel without any animation.
    /// </summary>
    private void HideDecisionPanelInstantly()
    {
        if (decisionPanelCanvasGroup != null)
        {
            decisionPanelCanvasGroup.alpha = 0f;
            decisionPanelCanvasGroup.interactable = false;
            decisionPanelCanvasGroup.blocksRaycasts = false;
        }
        else
        {
            Debug.LogError("[DecisionPanelManager] decisionPanelCanvasGroup is not assigned.");
        }

        if (darkOverlay != null)
        {
            darkOverlay.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("[DecisionPanelManager] darkOverlay is not assigned.");
        }
    }

    /// <summary>
    /// Opens the decision panel with specified options and pauses the game.
    /// </summary>
    /// <param name="eventDescription">Description of the event.</param>
    /// <param name="option1">Text for Option 1.</param>
    /// <param name="option2">Text for Option 2.</param>
    /// <param name="controller">Reference to ShipController.</param>
    public void OpenDecisionPanel(string eventDescription, string option1, string option2, ShipController controller)
    {
        if (decisionMade)
        {
            Debug.LogWarning("[DecisionPanelManager] Decision panel is already open.");
            return;
        }

        Debug.Log("[DecisionPanelManager] Opening Decision Panel.");

        decisionMade = false; // Reset decision flag
        SelectedOption = default; // Reset selected option
        shipController = controller;

        // Set the description and options texts
        if (decisionDescriptionText != null)
            decisionDescriptionText.text = eventDescription;
        else
            Debug.LogError("[DecisionPanelManager] decisionDescriptionText is not assigned.");

        if (option1Text != null)
            option1Text.text = option1;
        else
            Debug.LogError("[DecisionPanelManager] option1Text is not assigned.");

        if (option2Text != null)
            option2Text.text = option2;
        else
            Debug.LogError("[DecisionPanelManager] option2Text is not assigned.");

        // Play open sound
        if (decisionAudioSource != null && decisionOpenSound != null)
        {
            decisionAudioSource.PlayOneShot(decisionOpenSound);
        }

        // Show dark overlay
        if (darkOverlay != null)
        {
            darkOverlay.gameObject.SetActive(true);
        }

        // Start fade-in animation and pause the game
        StartCoroutine(FadeInAndPauseGame(0f, 1f, 0.5f)); // 0.5s fade duration

        // Make the panel interactable
        if (decisionPanelCanvasGroup != null)
        {
            decisionPanelCanvasGroup.interactable = true;
            decisionPanelCanvasGroup.blocksRaycasts = true;
        }
    }

    /// <summary>
    /// Fades in the decision panel and pauses the game.
    /// </summary>
    private IEnumerator FadeInAndPauseGame(float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; // Use unscaled time to allow fade during pause
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            decisionPanelCanvasGroup.alpha = alpha;
            yield return null;
        }
        decisionPanelCanvasGroup.alpha = endAlpha;
        Time.timeScale = 0f; // Pauses the game
    }

    /// <summary>
    /// Handles the selection of Option 1.
    /// </summary>
    private void OnOption1Selected()
    {
        HandleOptionSelection(DecisionOption.Option1);
    }

    /// <summary>
    /// Handles the selection of Option 2.
    /// </summary>
    private void OnOption2Selected()
    {
        HandleOptionSelection(DecisionOption.Option2);
    }

    /// <summary>
    /// Centralized method to handle option selections.
    /// </summary>
    /// <param name="option">The selected decision option.</param>
    private void HandleOptionSelection(DecisionOption option)
    {
        if (decisionMade)
        {
            Debug.LogWarning("[DecisionPanelManager] Decision has already been made.");
            return;
        }

        Debug.Log($"[DecisionPanelManager] Option {option} selected.");

        decisionMade = true;
        SelectedOption = option;

        // Handle decision based on current chapter and selected option
        switch (chapterManager.currentChapter)
        {
            case ChapterManager.Chapter.Chapter2:
                HandleChapter2Decision(option);
                break;
            case ChapterManager.Chapter.Chapter3:
                HandleChapter3Decision(option);
                break;
            case ChapterManager.Chapter.Chapter4:
                HandleChapter4Decision(option);
                break;
            default:
                Debug.LogWarning("[DecisionPanelManager] Decision made in an unsupported chapter.");
                break;
        }

        // Trigger camera shake based on the decision
        TriggerCameraShakeBasedOnDecision(option);

        // Close the decision panel
        CloseDecisionPanel();
    }

    /// <summary>
    /// Handles decisions specific to Chapter 2.
    /// </summary>
    /// <param name="option">Selected option.</param>
    private void HandleChapter2Decision(DecisionOption option)
    {
        if (option == DecisionOption.Option1)
        {
            Debug.Log("[DecisionPanelManager] Option1 selected: Sacrificing crew for shield repair.");
            shipController.SacrificeCrewForRepair(5, CubeInteraction.SystemType.Hull);
        }
        else if (option == DecisionOption.Option2)
        {
            Debug.Log("[DecisionPanelManager] Option2 selected: Navigate Carefully.");
            shipController.ApplyHullDamage(20f);
        }
    }

    /// <summary>
    /// Placeholder method for handling Chapter 3 decisions.
    /// </summary>
    private void HandleChapter3Decision(DecisionOption option)
    {
        Debug.Log("[DecisionPanelManager] Handling Chapter 3 decision.");
        // Add logic for Chapter 3 decisions here.
    }

    /// <summary>
    /// Placeholder method for handling Chapter 4 decisions.
    /// </summary>
    private void HandleChapter4Decision(DecisionOption option)
    {
        Debug.Log("[DecisionPanelManager] Handling Chapter 4 decision.");
        // Add logic for Chapter 4 decisions here.
    }

    /// <summary>
    /// Triggers camera shake based on the selected decision option.
    /// </summary>
    private void TriggerCameraShakeBasedOnDecision(DecisionOption option)
    {
        if (cameraController == null)
        {
            Debug.LogError("[DecisionPanelManager] CameraController reference is missing.");
            return;
        }

        float shakeDuration = 0.5f;
        float shakeMagnitude = 0.3f;

        if (option == DecisionOption.Option1)
        {
            cameraController.ShakeCamera(shakeDuration, shakeMagnitude * 0.8f);
        }
        else if (option == DecisionOption.Option2)
        {
            cameraController.ShakeCamera(shakeDuration, shakeMagnitude * 1.2f);
        }
    }

    /// <summary>
    /// Closes the decision panel and resumes the game.
    /// </summary>
    private void CloseDecisionPanel()
    {
        StartCoroutine(FadeOutAndResumeGame(1f, 0f, 0.5f)); // Fade out and resume game
        if (darkOverlay != null) darkOverlay.gameObject.SetActive(false);
        if (decisionPanelCanvasGroup != null)
        {
            decisionPanelCanvasGroup.interactable = false;
            decisionPanelCanvasGroup.blocksRaycasts = false;
        }
    }

    /// <summary>
    /// Coroutine to fade out the decision panel and resume the game.
    /// </summary>
    private IEnumerator FadeOutAndResumeGame(float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            decisionPanelCanvasGroup.alpha = alpha;
            yield return null;
        }
        decisionPanelCanvasGroup.alpha = endAlpha;
        Time.timeScale = 1f; // Resumes the game
    }
}
