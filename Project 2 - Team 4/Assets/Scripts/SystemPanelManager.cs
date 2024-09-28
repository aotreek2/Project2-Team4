using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SystemPanelManager : MonoBehaviour
{
    // UI Elements
    public TextMeshProUGUI systemNameText;
    public TextMeshProUGUI systemDescriptionText;
    public Button repairButton;
    public Button upgradeButton;
    public Button closeButton;

    // References
    private ShipController shipController;
    private CubeInteraction.SystemType currentSystemType;

    void Start()
    {
        // Assign button listeners
        repairButton.onClick.AddListener(OnRepairButtonClicked);
        upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
        closeButton.onClick.AddListener(OnCloseButtonClicked);

        // Hide the panel at start
        gameObject.SetActive(false);
    }

    public void OpenSystemPanel(CubeInteraction.SystemType systemType, ShipController controller)
    {
        shipController = controller;
        currentSystemType = systemType;

        // Update UI elements based on the system
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

        // Show the panel
        gameObject.SetActive(true);
    }

    void OnRepairButtonClicked()
    {
        // Implement repair logic
        float repairAmount = 20f; // Amount to repair
        float repairCost = 10f;   // Scrap cost

        if (shipController.resourceManager.scrapAmount >= repairCost)
        {
            shipController.resourceManager.scrapAmount -= repairCost;

            switch (currentSystemType)
            {
                case CubeInteraction.SystemType.LifeSupport:
                    shipController.RepairLifeSupport(repairAmount);
                    break;
                case CubeInteraction.SystemType.Engines:
                    shipController.RepairEngine(repairAmount);
                    break;
                case CubeInteraction.SystemType.Hull:
                    shipController.RepairHull(repairAmount);
                    break;
            }

            Debug.Log($"{currentSystemType} repaired by {repairAmount}%");
        }
        else
        {
            Debug.Log("Not enough Scrap to repair.");
        }
    }

    void OnUpgradeButtonClicked()
    {
        // Implement upgrade logic
        float upgradeCost = 30f; // Scrap cost

        if (shipController.resourceManager.scrapAmount >= upgradeCost)
        {
            shipController.resourceManager.scrapAmount -= upgradeCost;

            switch (currentSystemType)
            {
                case CubeInteraction.SystemType.LifeSupport:
                    // Upgrade Life Support (e.g., increase efficiency)
                    shipController.UpgradeLifeSupport();
                    break;
                case CubeInteraction.SystemType.Engines:
                    // Upgrade Engines
                    shipController.UpgradeEngine();
                    break;
                case CubeInteraction.SystemType.Hull:
                    // Upgrade Hull
                    shipController.UpgradeHull();
                    break;
            }

            Debug.Log($"{currentSystemType} upgraded.");
        }
        else
        {
            Debug.Log("Not enough Scrap to upgrade.");
        }
    }

    void OnCloseButtonClicked()
    {
        // Close the panel
        gameObject.SetActive(false);
    }
}
