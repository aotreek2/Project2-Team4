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
    public TextMeshProUGUI crewCountText;
    public TextMeshProUGUI fuelAmountText;
    public TextMeshProUGUI oxygenText;
    public TextMeshProUGUI distanceText;
    public TextMeshProUGUI scrapText;
    public TextMeshProUGUI moraleText;

    [Header("References")]
    public ShipController shipController;
    public LifeSupportController lifeSupportController;
    public ChapterManager chapterManager;

    public bool systemsActive = true;

    [Header("Crew Management")]
    public List<CrewMember> crewMembers = new List<CrewMember>();

    void Start()
    {
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
            ConsumeResources();
        }

        UpdateResourceUI();
        CheckGameOver();
    }

    void ConsumeResources()
    {
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

        float fuelConsumptionRate = baseFuelConsumptionRate / engineEfficiency * fuelDepletionMultiplier;
        fuelAmount -= Time.deltaTime * fuelConsumptionRate;

        float distanceReductionRate = 10f * engineEfficiency;
        distanceToLighthouse -= Time.deltaTime * distanceReductionRate;

        fuelAmount = Mathf.Clamp(fuelAmount, 0f, 100f);
        distanceToLighthouse = Mathf.Clamp(distanceToLighthouse, 0f, 1000f);
    }

    public void UpdateResourceUI()
    {
        if (crewCountText != null)
            crewCountText.text = $"Crew: {crewCount}";

        if (fuelAmountText != null)
            fuelAmountText.text = $"Fuel: {fuelAmount:F1}";

        if (oxygenText != null)
            oxygenText.text = $"Oxygen Level: {oxygenLevel:F1}%";

        if (distanceText != null)
            distanceText.text = $"Distance to Lighthouse: {distanceToLighthouse:F1} units";

        if (scrapText != null)
            scrapText.text = $"Scrap: {scrapAmount:F0}";

        if (moraleText != null && shipController != null)
            moraleText.text = $"Crew Morale: {shipController.crewMorale:F0}%";
    }

    void CheckGameOver()
    {
        if (oxygenLevel <= 0f || fuelAmount <= 0f)
        {
            SceneManager.LoadScene("DeathScene");
        }

        if (distanceToLighthouse <= 0f)
        {
            chapterManager.LoadNextLevel("Chapter2Scene");
        }
    }

    public void ToggleSystems(bool state)
    {
        systemsActive = state;
    }

    public void AddScrap(float amount)
    {
        scrapAmount += amount;
        scrapAmount = Mathf.Clamp(scrapAmount, 0f, 100f);
        UpdateResourceUI();
    }

    public void AdjustFuel(float amount)
    {
        fuelAmount += amount;
        fuelAmount = Mathf.Clamp(fuelAmount, 0f, 100f);
        UpdateResourceUI();
    }

    public void AddOxygen(float amount)
    {
        oxygenLevel += amount;
        oxygenLevel = Mathf.Clamp(oxygenLevel, 0f, 100f);
        UpdateResourceUI();
    }

    public bool SacrificeCrew(int amount)
    {
        if (crewCount >= amount)
        {
            crewCount -= amount;
            UpdateResourceUI();
            return true;
        }
        else
        {
            AlertManager.Instance?.ShowAlert("Not enough crew members to sacrifice.");
            return false;
        }
    }

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
                    bool sacrificed = SacrificeCrew(1);
                    if (sacrificed)
                    {
                        member.Die();
                    }
                }
            }
            UpdateResourceUI();
            return true;
        }
        else
        {
            AlertManager.Instance?.ShowAlert("Not enough crew members to assign and sacrifice.");
            return false;
        }
    }

    public void AdjustMorale(float amount)
    {
        if (shipController != null)
        {
            shipController.AdjustCrewMorale(amount);
        }
    }

    public void AddCrew(int amount)
    {
        crewCount += amount;
        UpdateResourceUI();
    }
}
