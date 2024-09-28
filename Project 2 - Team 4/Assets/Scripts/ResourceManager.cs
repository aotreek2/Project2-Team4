using UnityEngine;
using TMPro; // Include this if using TextMeshPro
using UnityEngine.UI; // Include this for Slider UI elements

public class ResourceManager : MonoBehaviour
{
    // Resource Variables
    public float oxygenLevel = 100f;           // Oxygen Level (%)
    public float fuelAmount = 100f;            // Fuel Amount (%)
    public float distanceToLighthouse = 1000f; // Distance in units

    // Efficiency variables (controlled by ShipController)
    public float engineEfficiency = 1f;      // Ranges from 0 to 1
    public float lifeSupportEfficiency = 1f; // Ranges from 0 to 1

    // UI Elements
    public TextMeshProUGUI oxygenText;
    public TextMeshProUGUI fuelText;
    public TextMeshProUGUI distanceText;

    // Reference to ShipController
    public ShipController shipController;

    void Start()
    {
        // Ensure shipController is assigned
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

        // Check for game over conditions
        CheckGameOver();
    }

    void ConsumeResources()
    {
        // Adjust resource consumption based on system efficiencies

        // Oxygen consumption increases if life support is damaged
        float oxygenConsumptionRate = 1f / lifeSupportEfficiency;
        oxygenLevel -= Time.deltaTime * oxygenConsumptionRate;

        // Fuel consumption increases if engines are damaged (less efficient)
        float fuelConsumptionRate = 0.5f / engineEfficiency;
        fuelAmount -= Time.deltaTime * fuelConsumptionRate;

        // Distance decreases based on engine efficiency
        float distanceReductionRate = 10f * engineEfficiency;
        distanceToLighthouse -= Time.deltaTime * distanceReductionRate;

        // Clamp values
        oxygenLevel = Mathf.Clamp(oxygenLevel, 0f, 100f);
        fuelAmount = Mathf.Clamp(fuelAmount, 0f, 100f);
        distanceToLighthouse = Mathf.Clamp(distanceToLighthouse, 0f, 1000f);
    }

    void UpdateResourceUI()
    {
        if (oxygenText != null)
            oxygenText.text = "Oxygen Level: " + oxygenLevel.ToString("F1") + "%";

        if (fuelText != null)
            fuelText.text = "Fuel Amount: " + fuelAmount.ToString("F1") + "%";

        if (distanceText != null)
            distanceText.text = "Distance to Lighthouse: " + distanceToLighthouse.ToString("F1") + " units";
    }

    void CheckGameOver()
    {
        if (oxygenLevel <= 0f)
        {
            Debug.Log("Game Over! Oxygen depleted.");
            // Implement game over logic here
        }

        if (fuelAmount <= 0f)
        {
            Debug.Log("Game Over! Fuel exhausted.");
            // Implement game over logic here
        }

        if (distanceToLighthouse <= 0f)
        {
            Debug.Log("Congratulations! You've reached the Lighthouse.");
            // Implement victory logic here
        }
    }

    // Methods to modify resources (can be called from other scripts)
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
