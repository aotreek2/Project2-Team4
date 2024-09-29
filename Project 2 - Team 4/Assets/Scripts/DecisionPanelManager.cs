using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections; // This is required for coroutines

public class DecisionPanelManager : MonoBehaviour
{
    public CanvasGroup decisionPanelCanvasGroup; // For fade animation
    public AudioSource decisionAudioSource; // For playing sounds
    public AudioClip decisionOpenSound; // Sound when panel opens
    public AudioClip decisionCloseSound; // Sound when panel closes
    public Image darkOverlay; // Fog of war effect
    public TextMeshProUGUI decisionDescriptionText; // Description of the decision
    public Button option1Button; // First decision button
    public Button option2Button; // Second decision button

    private ShipController shipController;
    private string currentEvent;

    void Start()
    {
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

        // Initially hide the decision panel
        CloseDecisionPanel();
    }

    public void OpenDecisionPanel(string eventDescription, ShipController controller)
    {
        shipController = controller;
        currentEvent = eventDescription;

        // Set the description text based on the event
        decisionDescriptionText.text = eventDescription;

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

        // Start fade-in animation
        StartCoroutine(FadePanel(0f, 1f, 0.5f));

        decisionPanelCanvasGroup.interactable = true;
        decisionPanelCanvasGroup.blocksRaycasts = true;
    }

    public void CloseDecisionPanel()
    {
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

        // Start fade-out animation
        StartCoroutine(FadePanel(1f, 0f, 0.5f));

        decisionPanelCanvasGroup.interactable = false;
        decisionPanelCanvasGroup.blocksRaycasts = false;
    }

    private void OnOption1Selected()
    {
        // Implement the gameplay logic for Option 1 (e.g., sacrificing crew members)
        if (currentEvent.Contains("fire")) // Example: Fire event
        {
            // Lose 5 crew members but stabilize the system
            shipController.SacrificeCrew(5);
            shipController.RepairLifeSupport(30f);
            Debug.Log("Option 1: Sacrificed 5 crew to repair Life Support.");
        }
        else if (currentEvent.Contains("asteroid")) // Example: Asteroid event
        {
            // Reduce hull integrity by 50% but save the crew
            shipController.ReduceHullIntegrity(50f);
            Debug.Log("Option 1: Reduced hull integrity to save crew.");
        }

        // Close the decision panel after the choice
        CloseDecisionPanel();
    }

    private void OnOption2Selected()
    {
        // Implement the gameplay logic for Option 2 (e.g., avoiding crew loss but with system penalties)
        if (currentEvent.Contains("fire")) // Example: Fire event
        {
            // Save the crew but lose system efficiency
            shipController.ReduceLifeSupportEfficiency(50f);
            Debug.Log("Option 2: Saved the crew but reduced Life Support efficiency.");
        }
        else if (currentEvent.Contains("asteroid")) // Example: Asteroid event
        {
            // Save the hull but lose 10 crew members
            shipController.SacrificeCrew(10);
            Debug.Log("Option 2: Saved the hull but lost 10 crew members.");
        }

        // Close the decision panel after the choice
        CloseDecisionPanel();
    }

    private IEnumerator FadePanel(float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            decisionPanelCanvasGroup.alpha = alpha;
            yield return null;
        }

        decisionPanelCanvasGroup.alpha = endAlpha;
    }
}
