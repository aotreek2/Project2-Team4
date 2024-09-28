using UnityEngine;
using UnityEngine.UI;
using TMPro; // Include if using TextMeshPro

public class DecisionManager : MonoBehaviour
{
    public GameObject decisionPanel; // The panel containing decision UI
    public TextMeshProUGUI decisionText;
    public Button option1Button;
    public Button option2Button;

    // References to other managers
    private ResourceManager resourceManager;
    private ShipController shipController;

    void Start()
    {
        // Assign button listeners
        option1Button.onClick.AddListener(Option1Selected);
        option2Button.onClick.AddListener(Option2Selected);

        // Find other managers
        resourceManager = FindObjectOfType<ResourceManager>();
        shipController = FindObjectOfType<ShipController>();

        // Hide the panel at start
        decisionPanel.SetActive(false);
    }

    public void ShowDecision(string prompt)
    {
        decisionText.text = prompt;
        decisionPanel.SetActive(true);
    }

    void Option1Selected()
    {
        // Implement the radical solution effect
        // Example: Sacrifice crew to gain fuel
        resourceManager.AddFuel(20f);
        Debug.Log("Radical solution chosen: Gained fuel at moral cost.");

        // Close the panel
        decisionPanel.SetActive(false);
    }

    void Option2Selected()
    {
        // Implement the ethical solution effect
        // Example: Use extra scrap to repair systems
        // Implement logic for scrap if available

        Debug.Log("Ethical solution chosen: Maintained morale.");

        // Close the panel
        decisionPanel.SetActive(false);
    }
}
