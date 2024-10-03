using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

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
    private string currentEvent;

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
    }

    // Method to open the decision panel with options and fade-in effect
    public void OpenDecisionPanel(string eventDescription, string option1, string option2, ShipController controller)
    {
        shipController = controller;
        currentEvent = eventDescription;

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

    // Method to close the decision panel and resume the game after fade-out
    public void CloseDecisionPanel()
    {
        // Resume the game after the panel is closed
        StartCoroutine(FadeOutAndResumeGame(1f, 0f, 0.5f)); // 0.5s fade duration

        // Play close sound
        if (decisionAudioSource != null && decisionCloseSound != null)
        {
            decisionAudioSource.PlayOneShot(decisionCloseSound);
        }

        // Hide fog of war
        if (darkOverlay != null)
        {
            darkOverlay.gameObject.SetActive(false);
        }

        decisionPanelCanvasGroup.interactable = false;
        decisionPanelCanvasGroup.blocksRaycasts = false;
    }

    // Handles option 1 being selected
    private void OnOption1Selected()
    {
        if (currentEvent.Contains("fire"))
        {
            shipController.SacrificeCrew(5);
            shipController.RepairLifeSupport(30f);
            Debug.Log("Option 1: Sacrificed 5 crew to repair Life Support.");
        }
        else if (currentEvent.Contains("asteroid"))
        {
            shipController.ReduceHullIntegrity(50f);
            Debug.Log("Option 1: Reduced hull integrity to save crew.");
        }
        else if (currentEvent.Contains("derelict"))
        {
            int outcome = Random.Range(0, 2); // Random success or failure outcome
            if (outcome == 0)
            {
                shipController.resourceManager.scrapAmount += 50;
                shipController.AddCrew(2);
                shipController.AdjustCrewMorale(10f);
                Debug.Log("You found valuable resources and rescued 2 crew members! Morale increased.");
            }
            else
            {
                shipController.DamageHull(30f);
                shipController.SacrificeCrew(5);
                shipController.AdjustCrewMorale(-20f);
                Debug.Log("Pirates ambushed you. Lost 5 crew and hull integrity reduced. Morale decreased.");
            }
        }

        shipController.resourceManager.UpdateResourceUI();
        CloseDecisionPanel();
    }

    private void OnOption2Selected()
    {
        // Adjust morale based on decisions
        if (currentEvent.Contains("fire"))
        {
            shipController.AdjustCrewMorale(-5f);
        }
        else if (currentEvent.Contains("asteroid"))
        {
            shipController.AdjustCrewMorale(-10f);
        }
        else if (currentEvent.Contains("derelict"))
        {
            shipController.resourceManager.fuelAmount -= 10f;
            shipController.AdjustCrewMorale(0f); // No change
            Debug.Log("You decide to play it safe and move on.");
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
        Time.timeScale = 1f; // Ensure game resumes during the fade-out process (so player action is smooth)

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
}
