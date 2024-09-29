using UnityEngine;
using UnityEngine.UI;

public class ShipController : MonoBehaviour
{
    // Existing fields and variables
    public float engineHealth = 100f;
    public float lifeSupportHealth = 100f;
    public float hullIntegrity = 100f;
    
    public float engineMaxHealth = 100f;
    public float lifeSupportMaxHealth = 100f;
    public float hullMaxIntegrity = 100f;

    public int crewCount = 20; // Starting crew count

    public Slider engineHealthBar;
    public Slider lifeSupportHealthBar;
    public Slider hullIntegrityBar;

    public GameObject lifeSupportCube;
    public GameObject enginesCube;
    public GameObject hullCube;

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

    void UpdateSystemUI()
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

        // Update the UI after the repair
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
}
