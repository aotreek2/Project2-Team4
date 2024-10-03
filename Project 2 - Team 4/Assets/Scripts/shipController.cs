using UnityEngine;
using UnityEngine.UI;

public class ShipController : MonoBehaviour
{
    // Existing fields and variables
    public float engineHealth = 100f;
    public float lifeSupportHealth = 100f;
    public float hullIntegrity = 100f;
    public float crewMorale = 100f; // Overall crew moraless
    
    public float engineMaxHealth = 100f;
    public float lifeSupportMaxHealth = 100f;
    public float hullMaxIntegrity = 100f;


    public Slider engineHealthBar;
    public Slider lifeSupportHealthBar;
    public Slider hullIntegrityBar;

    public GameObject lifeSupportCube;
    public GameObject enginesCube;
    public GameObject hullCube;
    public int crewCount = 20; // Starting crew count

    public ResourceManager resourceManager;

    void Start()
    {
        if (resourceManager == null)
        {
            resourceManager = FindObjectOfType<ResourceManager>();
        }

        UpdateSystemUI();
    }

    void Update()
    {
        UpdateSystems();
        UpdateSystemUI();
        UpdateSystemCubes();
    }

    // Update efficiencies and system statuses
    void UpdateSystems()
    {
        float engineHealthPercentage = engineHealth / engineMaxHealth;
        resourceManager.engineEfficiency = Mathf.Clamp(engineHealthPercentage, 0.1f, 1.2f);

        float lifeSupportHealthPercentage = lifeSupportHealth / lifeSupportMaxHealth;
        resourceManager.lifeSupportEfficiency = Mathf.Clamp(lifeSupportHealthPercentage, 0.1f, 1.2f);
    }

    public void UpdateSystemUI()
    {
        if (engineHealthBar != null)
            engineHealthBar.value = engineHealth / engineMaxHealth;

        if (lifeSupportHealthBar != null)
            lifeSupportHealthBar.value = lifeSupportHealth / lifeSupportMaxHealth;

        if (hullIntegrityBar != null)
            hullIntegrityBar.value = hullIntegrity / hullMaxIntegrity;
    }

    void UpdateSystemCubes()
    {
        UpdateCubeColor(lifeSupportCube, lifeSupportHealth / lifeSupportMaxHealth);
        UpdateCubeColor(enginesCube, engineHealth / engineMaxHealth);
        UpdateCubeColor(hullCube, hullIntegrity / hullMaxIntegrity);
    }

    void UpdateCubeColor(GameObject cube, float healthPercentage)
    {
        if (cube != null)
        {
            Renderer renderer = cube.GetComponent<Renderer>();
            if (renderer != null)
            {
                Color healthColor = Color.Lerp(Color.red, Color.green, healthPercentage);
                renderer.material.color = healthColor;
            }
        }
    }

    // **New Method: RepairSystem**
    public void RepairSystem(CubeInteraction.SystemType systemType, float repairAmount)
    {
        switch (systemType)
        {
            case CubeInteraction.SystemType.Engines:
                engineHealth += repairAmount;
                engineHealth = Mathf.Clamp(engineHealth, 0f, engineMaxHealth);
                Debug.Log("Engine repaired by " + repairAmount + " points.");
                break;

            case CubeInteraction.SystemType.LifeSupport:
                lifeSupportHealth += repairAmount;
                lifeSupportHealth = Mathf.Clamp(lifeSupportHealth, 0f, lifeSupportMaxHealth);
                Debug.Log("Life Support repaired by " + repairAmount + " points.");
                break;

            case CubeInteraction.SystemType.Hull:
                hullIntegrity += repairAmount;
                hullIntegrity = Mathf.Clamp(hullIntegrity, 0f, hullMaxIntegrity);
                Debug.Log("Hull repaired by " + repairAmount + " points.");
                break;
        }
        UpdateSystemUI();
    }


    // Existing methods for damaging and repairing systems
    public void DamageEngine(float damage)
    {
        engineHealth -= damage;
        engineHealth = Mathf.Clamp(engineHealth, 0f, engineMaxHealth);
    }

    public void RepairEngine(float amount)
    {
        engineHealth += amount;
        engineHealth = Mathf.Clamp(engineHealth, 0f, engineMaxHealth);
    }

    public void DamageLifeSupport(float damage)
    {
        lifeSupportHealth -= damage;
        lifeSupportHealth = Mathf.Clamp(lifeSupportHealth, 0f, lifeSupportMaxHealth);
    }

    public void RepairLifeSupport(float amount)
    {
        lifeSupportHealth += amount;
        lifeSupportHealth = Mathf.Clamp(lifeSupportHealth, 0f, lifeSupportMaxHealth);
    }

    public void DamageHull(float damage)
    {
        hullIntegrity -= damage;
        hullIntegrity = Mathf.Clamp(hullIntegrity, 0f, hullMaxIntegrity);
    }

    public void RepairHull(float amount)
    {
        hullIntegrity += amount;
        hullIntegrity = Mathf.Clamp(hullIntegrity, 0f, hullMaxIntegrity);
    }

    // **New Methods to support DecisionPanelManager**

    // Sacrifice crew members to stabilize systems
    public void SacrificeCrew(int amount)
    {
        if (crewCount >= amount)
        {
            crewCount -= amount;
            Debug.Log($"{amount} crew members sacrificed.");
        }
        else
        {
            Debug.Log("Not enough crew members to sacrifice.");
        }
    }

    // Reduce the hull integrity by a percentage
    public void ReduceHullIntegrity(float percentage)
    {
        float reduction = hullMaxIntegrity * (percentage / 100f);
        hullIntegrity -= reduction;
        hullIntegrity = Mathf.Clamp(hullIntegrity, 0f, hullMaxIntegrity);
        Debug.Log($"Hull integrity reduced by {percentage}%.");
    }

    // Reduce life support system's efficiency
    public void ReduceLifeSupportEfficiency(float percentage)
    {
        float reduction = lifeSupportMaxHealth * (percentage / 100f);
        lifeSupportHealth -= reduction;
        lifeSupportHealth = Mathf.Clamp(lifeSupportHealth, 0f, lifeSupportMaxHealth);
        Debug.Log($"Life Support efficiency reduced by {percentage}%.");
    }

        public void AdjustCrewMorale(float amount)
    {
        crewMorale += amount;
        crewMorale = Mathf.Clamp(crewMorale, 0f, 100f);
        Debug.Log($"Crew morale adjusted by {amount}. Current morale: {crewMorale}%");

        // Update morale for each crew member
        CrewMember[] crewMembers = FindObjectsOfType<CrewMember>();
        foreach (CrewMember crew in crewMembers)
        {
            crew.AdjustMorale(amount);
        }
    }

     public void SacrificeCrewForRepair(int amount, CubeInteraction.SystemType systemType)
    {
        if (crewCount >= amount)
        {
            crewCount -= amount;
            Debug.Log($"{amount} crew members sacrificed to repair {systemType}.");

            // Instantly repair the system
            RepairSystem(systemType, 100f);
            AdjustCrewMorale(-20f); // Decrease morale due to sacrifice
        }
        else
        {
            Debug.Log("Not enough crew members to sacrifice.");
        }
    }

      public void AddCrew(int amount)
    {
        crewCount += amount;
        Debug.Log($"{amount} crew members added. Total crew: {crewCount}.");
    }
}
