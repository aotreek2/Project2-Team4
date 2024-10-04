using UnityEngine;
using TMPro;
using System.Collections;

public class ResourceManager : MonoBehaviour
{
    // Resource Variables
    public float oxygenLevel = 100f;           // Oxygen Level (%)
    public float fuelAmount = 100f;            // Fuel Amount (%)
    public float distanceToLighthouse = 1000f; // Distance in units

    // Efficiency variables (controlled by ShipController)
    public float engineEfficiency = 1f;      // Ranges from 0 to 1
    public float lifeSupportEfficiency = 1f; // Ranges from 0 to 1
    public float generatorEfficiency = 1f;   // Ranges from 0 to 1

    // Scrap Resource
    public float scrapAmount = 50f; // Starting amount of Scrap

    // UI Elements
    public TextMeshProUGUI oxygenText;
    public TextMeshProUGUI fuelText;
    public TextMeshProUGUI distanceText;
    public TextMeshProUGUI scrapText;
    public TextMeshProUGUI moraleText;

    // Reference to ShipController
    public ShipController shipController;

    void Start()
    {
        if (shipController == null)
        {
            shipController = FindObjectOfType<ShipController>();
        }

        UpdateResourceUI();
    }

    void Update()
    {
        ConsumeResources();
        UpdateResourceUI();
        CheckGameOver();
    }

    void ConsumeResources()
    {
        // Check if life support or generator is damaged before consuming oxygen
        if (shipController.generatorHealth < shipController.generatorMaxHealth || lifeSupportEfficiency < 1f)
        {
            float oxygenConsumptionRate = 1f / lifeSupportEfficiency;
            oxygenLevel -= Time.deltaTime * oxygenConsumptionRate;
        }
        else
        {
            Debug.Log("Oxygen is not being consumed because life support and generator are fully functional.");
        }

        // Fuel consumption remains as it is, based on engine efficiency
        float fuelConsumptionRate = 0.5f / engineEfficiency;
        fuelAmount -= Time.deltaTime * fuelConsumptionRate;

        // Reduce distance based on engine efficiency
        float distanceReductionRate = 10f * engineEfficiency;
        distanceToLighthouse -= Time.deltaTime * distanceReductionRate;

        // Clamp the values to prevent going out of bounds
        oxygenLevel = Mathf.Clamp(oxygenLevel, 0f, 100f);
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

        if (moraleText != null)
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

    public void AddFuel(float amount)
    {
        fuelAmount += amount;
        fuelAmount = Mathf.Clamp(fuelAmount, 0f, 100f);
    }

    public void AddOxygen(float amount)
    {
        oxygenLevel += amount;
        oxygenLevel = Mathf.Clamp(oxygenLevel, 0f, 100f);
    }
}
