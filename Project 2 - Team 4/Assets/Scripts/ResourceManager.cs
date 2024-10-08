using UnityEngine;
using TMPro;

public class ResourceManager : MonoBehaviour
{
    // Resource Variables
    public float oxygenLevel = 100f; // Oxygen Level (%)
    public float fuelAmount = 100f;  // Fuel Amount (%)
    public float distanceToLighthouse = 1000f; // Distance in units

    // Efficiency variables (controlled by the different systems)
    public float engineEfficiency = 1f; // Controlled by EngineSystemController (Ranges from 0 to 1)
    public float lifeSupportEfficiency = 1f; // This will be updated from LifeSupportController
    public float generatorEfficiency = 1f; // Controlled by ShipController (Ranges from 0 to 1)

    // Fuel consumption variables
    public float baseFuelConsumptionRate = 0.2f; // The base rate at which fuel depletes (this can be adjusted)
    public float fuelDepletionMultiplier = 0.5f; // Adjusted multiplier to slow down the rate of fuel depletion

    // Oxygen consumption variables
    public float baseOxygenConsumptionRate = 1f; // Oxygen consumption rate when life support is fully damaged
    public float oxygenRecoveryRate = 5f; // Oxygen recovery rate when life support is fully functional

    // Scrap Resource
    public float scrapAmount = 50f; // Starting amount of Scrap

    // UI Elements
    public TextMeshProUGUI oxygenText;
    public TextMeshProUGUI fuelText;
    public TextMeshProUGUI distanceText;
    public TextMeshProUGUI scrapText;
    public TextMeshProUGUI moraleText;

    // References to Controllers
    public ShipController shipController;
    public LifeSupportController lifeSupportController;

    void Start()
    {
        if (shipController == null)
        {
            shipController = FindObjectOfType<ShipController>();
        }

        if (lifeSupportController == null)
        {
            lifeSupportController = FindObjectOfType<LifeSupportController>();
        }

        UpdateResourceUI();
    }

    void Update()
    {
        // Update efficiencies from controllers
        if (shipController != null)
        {
            generatorEfficiency = shipController.generatorHealth / shipController.generatorMaxHealth;
        }

        if (lifeSupportController != null)
        {
            lifeSupportEfficiency = lifeSupportController.LifeSupportEfficiency;
        }

        ConsumeResources();
        UpdateResourceUI();
        CheckGameOver();
    }

    void ConsumeResources()
    {
        // Oxygen management
        if (lifeSupportEfficiency < 1f)
        {
            // Life support is damaged, oxygen level decreases
            float oxygenConsumptionRate = (1f - lifeSupportEfficiency) * baseOxygenConsumptionRate;
            oxygenLevel -= oxygenConsumptionRate * Time.deltaTime;
        }
        else if (oxygenLevel < 100f)
        {
            // Life support is fully functional, oxygen level increases back up
            oxygenLevel += oxygenRecoveryRate * Time.deltaTime;
        }

        // Clamp oxygenLevel between 0 and 100
        oxygenLevel = Mathf.Clamp(oxygenLevel, 0f, 100f);

        // Fuel consumption
        float fuelConsumptionRate = baseFuelConsumptionRate / engineEfficiency * fuelDepletionMultiplier;
        fuelAmount -= Time.deltaTime * fuelConsumptionRate;

        // Distance decreases based on engine efficiency
        float distanceReductionRate = 10f * engineEfficiency;
        distanceToLighthouse -= Time.deltaTime * distanceReductionRate;

        // Clamp values to avoid going out of bounds
        fuelAmount = Mathf.Clamp(fuelAmount, 0f, 100f);
        distanceToLighthouse = Mathf.Clamp(distanceToLighthouse, 0f, 1000f);
    }

    public void UpdateResourceUI()
    {
        if (oxygenText != null)
            oxygenText.text = "Oxygen Level: " + oxygenLevel.ToString("F1") + "%";

        if (fuelText != null)
            fuelText.text = "Fuel Amount: " + fuelAmount.ToString("F1") + "%";

        if (distanceText != null)
            distanceText.text = "Distance to Lighthouse: " + distanceToLighthouse.ToString("F1") + " units";

        if (scrapText != null)
            scrapText.text = "Scrap: " + scrapAmount.ToString("F0");

        if (moraleText != null && shipController != null)
            moraleText.text = "Crew Morale: " + shipController.crewMorale.ToString("F0") + "%";
    }

    void CheckGameOver()
    {
        if (oxygenLevel <= 0f)
        {
            Debug.Log("Game Over! Oxygen depleted.");
        }

        if (fuelAmount <= 0f)
        {
            Debug.Log("Game Over! Fuel exhausted.");
        }

        if (distanceToLighthouse <= 0f)
        {
            Debug.Log("Congratulations! You've reached the Lighthouse.");
        }
    }

    // Add scrap
    public void AddScrap(float amount)
    {
        scrapAmount += amount;
        scrapAmount = Mathf.Clamp(scrapAmount, 0f, 100f);
        UpdateResourceUI();
    }

    // Add fuel
    public void AddFuel(float amount)
    {
        fuelAmount += amount;
        fuelAmount = Mathf.Clamp(fuelAmount, 0f, 100f);
        UpdateResourceUI();
    }

    // Add oxygen
    public void AddOxygen(float amount)
    {
        oxygenLevel += amount;
        oxygenLevel = Mathf.Clamp(oxygenLevel, 0f, 100f);
        UpdateResourceUI();
    }
}
