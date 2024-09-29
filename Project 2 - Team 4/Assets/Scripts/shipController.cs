using UnityEngine;
using UnityEngine.UI; // For UI elements

public class ShipController : MonoBehaviour
{
    // System Health Variables
    public float engineHealth = 100f;
    public float lifeSupportHealth = 100f;
    public float hullIntegrity = 100f;

    // Maximum health values (can be increased through upgrades)
    public float engineMaxHealth = 100f;
    public float lifeSupportMaxHealth = 100f;
    public float hullMaxIntegrity = 100f;

    // Crew management
    public int crewCount = 20; // Initial crew count

    // UI Elements
    public Slider engineHealthBar;
    public Slider lifeSupportHealthBar;
    public Slider hullIntegrityBar;

    // System Cubes
    public GameObject lifeSupportCube;
    public GameObject enginesCube;
    public GameObject hullCube;

    // Reference to the ResourceManager
    public ResourceManager resourceManager;

    void Start()
    {
        // Find and reference the ResourceManager if not assigned
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

    void UpdateSystems()
    {
        // Update efficiencies based on health
        float engineHealthPercentage = engineHealth / engineMaxHealth;
        resourceManager.engineEfficiency = Mathf.Clamp(engineHealthPercentage, 0.1f, 1.2f);

        float lifeSupportHealthPercentage = lifeSupportHealth / lifeSupportMaxHealth;
        resourceManager.lifeSupportEfficiency = Mathf.Clamp(lifeSupportHealthPercentage, 0.1f, 1.2f);

        // Additional logic for hull integrity can be added here
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
        // Update Life Support Cube Color
        UpdateCubeColor(lifeSupportCube, lifeSupportHealth / lifeSupportMaxHealth);

        // Update Engines Cube Color
        UpdateCubeColor(enginesCube, engineHealth / engineMaxHealth);

        // Update Hull Cube Color
        UpdateCubeColor(hullCube, hullIntegrity / hullMaxIntegrity);
    }

    void UpdateCubeColor(GameObject cube, float healthPercentage)
    {
        if (cube != null)
        {
            Renderer renderer = cube.GetComponent<Renderer>();
            if (renderer != null)
            {
                // Calculate color based on health (Green to Red)
                Color healthColor = Color.Lerp(Color.red, Color.green, healthPercentage);
                renderer.material.color = healthColor;
            }
        }
    }

    // Damage and Repair Methods
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

    // Decision-Based Event Integration
    public void TriggerEngineFireEvent()
    {
        // This method can be called by the event manager to handle crew decisions
        // Example decision event: Crew sacrifice or efficiency reduction

        // Display decision panel (handled by UI manager)
        Debug.Log("Engine Fire Event triggered! Make a decision...");
    }

    public void OnSacrificeCrewDecision()
    {
        if (crewCount >= 5)
        {
            crewCount -= 5; // Lose 5 crew members
            engineHealth *= 0.9f; // Reduce engine health to 90%
            Debug.Log("5 Crew sacrificed. Engine efficiency maintained.");
        }
        else
        {
            Debug.Log("Not enough crew members to sacrifice!");
        }
    }

    public void OnSaveCrewDecision()
    {
        engineHealth *= 0.5f; // Reduce engine health to 50%
        Debug.Log("Crew saved. Engine efficiency reduced.");
    }

    // Upgrade Methods
    public void UpgradeEngine()
    {
        engineMaxHealth += 20f; // Increase max health
        engineHealth = engineMaxHealth; // Fully repair upon upgrade
    }

    public void UpgradeLifeSupport()
    {
        lifeSupportMaxHealth += 20f;
        lifeSupportHealth = lifeSupportMaxHealth;
    }

    public void UpgradeHull()
    {
        hullMaxIntegrity += 20f;
        hullIntegrity = hullMaxIntegrity;
    }
}
