using UnityEngine;
using TMPro;

public class ResourceManager : MonoBehaviour
{
    // Resource Variables
    public float oxygenLevel = 100f; // Oxygen Level (%)
    public float fuelAmount = 100f;  // Fuel Amount (%)
    public float distanceToLighthouse = 1000f; // Distance in units

    // Efficiency variables
    public float engineEfficiency = 1f;
    public float lifeSupportEfficiency = 1f;
    public float generatorEfficiency = 1f;

    // Fuel consumption variables
    public float baseFuelConsumptionRate = 0.2f;
    public float fuelDepletionMultiplier = 0.5f;

    // Oxygen consumption variables
    public float baseOxygenConsumptionRate = 1f;
    public float oxygenRecoveryRate = 5f;

    // Scrap Resource
    public float scrapAmount = 50f;

    // UI Elements
    public TextMeshProUGUI oxygenText;
    public TextMeshProUGUI fuelText;
    public TextMeshProUGUI distanceText;
    public TextMeshProUGUI scrapText;
    public TextMeshProUGUI moraleText;

    // References to Controllers
    public ShipController shipController;
    public LifeSupportController lifeSupportController;

    // New variable to control if systems are active
    public bool systemsActive = true;

    void Start()
    {
        if (shipController == null)
            shipController = FindObjectOfType<ShipController>();

        if (lifeSupportController == null)
            lifeSupportController = FindObjectOfType<LifeSupportController>();

        UpdateResourceUI();
    }

    void Update()
    {
        if (systemsActive)
        {
            // Only consume resources if the systems are active
            ConsumeResources();
        }
        UpdateResourceUI();
        CheckGameOver();
    }

    void ConsumeResources()
    {
        // Oxygen management
        if (lifeSupportEfficiency < 1f)
        {
            float oxygenConsumptionRate = (1f - lifeSupportEfficiency) * baseOxygenConsumptionRate;
            oxygenLevel -= oxygenConsumptionRate * Time.deltaTime;
        }
        else if (oxygenLevel < 100f)
        {
            oxygenLevel += oxygenRecoveryRate * Time.deltaTime;
        }
        oxygenLevel = Mathf.Clamp(oxygenLevel, 0f, 100f);

        // Fuel consumption
        float fuelConsumptionRate = baseFuelConsumptionRate / engineEfficiency * fuelDepletionMultiplier;
        fuelAmount -= Time.deltaTime * fuelConsumptionRate;

        // Distance decreases based on engine efficiency
        float distanceReductionRate = 10f * engineEfficiency;
        distanceToLighthouse -= Time.deltaTime * distanceReductionRate;

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

    public void ToggleSystems(bool state)
    {
        systemsActive = state;
    }

    // Methods to add resources if needed
    public void AddScrap(float amount) { /*...*/ }
    public void AddFuel(float amount) { /*...*/ }
    public void AddOxygen(float amount) { /*...*/ }
}
