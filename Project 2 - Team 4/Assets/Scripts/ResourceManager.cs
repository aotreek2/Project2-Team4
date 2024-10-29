using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class ResourceManager : MonoBehaviour
{
    [Header("Crew and Fuel")]
    public int crewCount = 20; // Starting crew count
    public float fuelAmount = 100f; // Starting fuel

    [Header("Other Resources")]
    public float oxygenLevel = 100f; // Oxygen Level (%)
    public float distanceToLighthouse = 1000f; // Distance in units
    public float scrapAmount = 50f; // Scrap Resource

    [Header("Efficiency Variables")]
    public float engineEfficiency = 1f;
    public float lifeSupportEfficiency = 1f;
    public float generatorEfficiency = 1f;

    [Header("Consumption Variables")]
    public float baseFuelConsumptionRate = 0.2f;
    public float fuelDepletionMultiplier = 0.5f;
    public float baseOxygenConsumptionRate = 1f;
    public float oxygenRecoveryRate = 5f;

    [Header("UI Components")]
    public TextMeshProUGUI crewCountText; // UI Text to display crew count
    public TextMeshProUGUI fuelAmountText; // UI Text to display fuel amount
    public TextMeshProUGUI oxygenText;
    public TextMeshProUGUI distanceText;
    public TextMeshProUGUI scrapText;
    public TextMeshProUGUI moraleText;

    [Header("References")]
    public ShipController shipController;
    public LifeSupportController lifeSupportController;
    public ChapterManager chapterManager;

    // Variable to control if systems are active
    public bool systemsActive = true;

    [Header("Crew Management")]
    public List<CrewMember> crewMembers = new List<CrewMember>();

    void Start()
    {
        // Initialize references if not assigned via Inspector
        if (shipController == null)
            shipController = FindObjectOfType<ShipController>();

        if (lifeSupportController == null)
            lifeSupportController = FindObjectOfType<LifeSupportController>();

        if (chapterManager == null)
            chapterManager = FindObjectOfType<ChapterManager>();

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

    /// <summary>
    /// Handles the consumption of resources like oxygen and fuel.
    /// </summary>
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

    /// <summary>
    /// Updates all resource-related UI elements.
    /// </summary>
    public void UpdateResourceUI()
    {
        if (crewCountText != null)
            crewCountText.text = $"Crew: {crewCount}";
        else
            Debug.LogError("[ResourceManager] crewCountText is not assigned.");

        if (fuelAmountText != null)
            fuelAmountText.text = $"Fuel: {fuelAmount:F1}";
        else
            Debug.LogError("[ResourceManager] fuelAmountText is not assigned.");

        if (oxygenText != null)
            oxygenText.text = $"Oxygen Level: {oxygenLevel:F1}%";
        else
            Debug.LogError("[ResourceManager] oxygenText is not assigned.");

        if (distanceText != null)
            distanceText.text = $"Distance to Lighthouse: {distanceToLighthouse:F1} units";
        else
            Debug.LogError("[ResourceManager] distanceText is not assigned.");

        if (scrapText != null)
            scrapText.text = $"Scrap: {scrapAmount:F0}";
        else
            Debug.LogError("[ResourceManager] scrapText is not assigned.");

        if (moraleText != null && shipController != null)
            moraleText.text = $"Crew Morale: {shipController.crewMorale:F0}%";
        else
            Debug.LogError("[ResourceManager] moraleText or shipController is not assigned.");
    }

    /// <summary>
    /// Checks for game over conditions based on resource levels.
    /// </summary>
    void CheckGameOver()
    {
        if (oxygenLevel <= 0f)
        {
            SceneManager.LoadScene("DeathScene");
        }

        if (fuelAmount <= 0f)
        {
            SceneManager.LoadScene("DeathScene");
        }

        if (distanceToLighthouse <= 0f)
        {
            chapterManager.LoadNextLevel();
        }
    }

    /// <summary>
    /// Toggles the active state of resource systems.
    /// </summary>
    /// <param name="state">True to activate, False to deactivate.</param>
    public void ToggleSystems(bool state)
    {
        systemsActive = state;
    }

    /// <summary>
    /// Adds scrap resources.
    /// </summary>
    /// <param name="amount">Amount of scrap to add.</param>
    public void AddScrap(float amount)
    {
        scrapAmount += amount;
        scrapAmount = Mathf.Clamp(scrapAmount, 0f, 100f); // Adjust max as needed
        UpdateResourceUI();
        Debug.Log($"[ResourceManager] Added {amount} scrap. Current scrap: {scrapAmount}");
    }

    /// <summary>
    /// Adjusts fuel resources by a specified amount.
    /// </summary>
    /// <param name="amount">Amount of fuel to adjust.</param>
    public void AdjustFuel(float amount)
    {
        fuelAmount += amount;
        fuelAmount = Mathf.Clamp(fuelAmount, 0f, 100f); // Adjust max as needed
        UpdateResourceUI();
        Debug.Log($"[ResourceManager] Fuel adjusted by {amount}. Current fuel: {fuelAmount}");
    }

    /// <summary>
    /// Adds oxygen resources.
    /// </summary>
    /// <param name="amount">Amount of oxygen to add.</param>
    public void AddOxygen(float amount)
    {
        oxygenLevel += amount;
        oxygenLevel = Mathf.Clamp(oxygenLevel, 0f, 100f); // Adjust max as needed
        UpdateResourceUI();
        Debug.Log($"[ResourceManager] Added {amount} oxygen. Current oxygen level: {oxygenLevel}");
    }

    /// <summary>
    /// Sacrifices a specified number of crew members.
    /// </summary>
    /// <param name="amount">Number of crew members to sacrifice.</param>
    public bool SacrificeCrew(int amount)
    {
        if (crewCount >= amount)
        {
            crewCount -= amount;
            UpdateResourceUI();
            Debug.Log($"[ResourceManager] Sacrificed {amount} crew members. Remaining crew: {crewCount}");
            return true;
        }
        else
        {
            Debug.LogWarning("[ResourceManager] Not enough crew members to sacrifice.");
            // Optionally, trigger an alert
            AlertManager.Instance?.ShowAlert("Not enough crew members to sacrifice.");
            return false;
        }
    }

    /// <summary>
    /// Assigns and sacrifices a specified number of crew members to the generator.
    /// </summary>
    /// <param name="amount">Number of crew members to assign and sacrifice.</param>
    /// <param name="generator">The generator to which crew are assigned.</param>
    public bool AssignAndSacrificeCrew(int amount, GeneratorController generator)
    {
        if (crewCount >= amount && crewMembers.Count >= amount)
        {
            for (int i = 0; i < amount; i++)
            {
                if (crewMembers.Count > 0)
                {
                    CrewMember member = crewMembers[0];
                    crewMembers.RemoveAt(0);
                    generator.AssignCrew(member);
                    bool sacrificed = SacrificeCrew(1); // Sacrifice one crew member
                    if (sacrificed)
                    {
                        member.Die(); // Handle crew death animation
                    }
                }
            }
            UpdateResourceUI();
            Debug.Log($"[ResourceManager] Assigned and sacrificed {amount} crew members to Generator.");
            return true;
        }
        else
        {
            Debug.LogWarning("[ResourceManager] Not enough crew members to assign and sacrifice.");
            AlertManager.Instance?.ShowAlert("Not enough crew members to assign and sacrifice.");
            return false;
        }
    }

    /// <summary>
    /// Adjusts crew morale by a specified amount.
    /// </summary>
    /// <param name="amount">Amount to adjust morale by.</param>
    public void AdjustMorale(float amount)
    {
        if (shipController != null)
        {
            shipController.AdjustCrewMorale(amount);
        }
        else
        {
            Debug.LogError("[ResourceManager] ShipController is not assigned.");
        }
    }

    /// <summary>
    /// Adds a specified number of crew members.
    /// </summary>
    /// <param name="amount">Number of crew members to add.</param>
    public void AddCrew(int amount)
    {
        crewCount += amount;
        // Optionally, instantiate new CrewMember GameObjects and add to crewMembers list
        UpdateResourceUI();
        Debug.Log($"[ResourceManager] Added {amount} crew members. Total crew: {crewCount}");
    }

    // Add other resource management methods as needed
}
