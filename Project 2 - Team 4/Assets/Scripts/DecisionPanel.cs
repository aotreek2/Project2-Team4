using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DecisionPanel : MonoBehaviour
{
    public TextMeshProUGUI eventDescriptionText; // The text that shows the event description
    public Button sacrificeCrewButton;           // Button for sacrificing crew
    public Button saveCrewButton;                // Button for saving crew
    public GameObject decisionPanel;             // The UI panel for decision-making
    private ShipController shipController;       // Reference to the ship controller

    void Start()
    {
        shipController = FindObjectOfType<ShipController>();

        // Add listeners to the buttons
        sacrificeCrewButton.onClick.AddListener(OnSacrificeCrew);
        saveCrewButton.onClick.AddListener(OnSaveCrew);
        
        decisionPanel.SetActive(false); // Hide the decision panel initially
    }

    // Trigger the event when the engines catch fire
    public void TriggerEngineFireEvent()
    {
        decisionPanel.SetActive(true); // Show the decision panel
        eventDescriptionText.text = "The engines are on fire! Cut off the oxygen to stop the fire, or save the crew and risk engine power."; // Display event details

        // Update button text
        sacrificeCrewButton.GetComponentInChildren<TextMeshProUGUI>().text = "Cut off oxygen (-5 crew, 90% efficiency)";
        saveCrewButton.GetComponentInChildren<TextMeshProUGUI>().text = "Save crew (no loss, 50% efficiency)";
    }

    // Handle the consequence of sacrificing crew
    void OnSacrificeCrew()
    {
        shipController.crewCount -= 5; // Lose 5 crew members
        shipController.engineHealth *= 0.9f; // Keep engine efficiency at 90%

        decisionPanel.SetActive(false); // Close the decision panel
    }

    // Handle the consequence of saving the crew
    void OnSaveCrew()
    {
        shipController.engineHealth *= 0.5f; // Reduce engine efficiency to 50%

        decisionPanel.SetActive(false); // Close the decision panel
    }
}
