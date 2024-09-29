using UnityEngine;
using UnityEngine.UI;

public class ShipController : MonoBehaviour
{
    public SystemPanelManager systemPanelManager; // Reference for system panel manager

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

        // Find and reference the SystemPanelManager if not assigned
        if (systemPanelManager == null)
        {
            systemPanelManager = FindObjectOfType<SystemPanelManager>();
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

    // Repair system method for CrewMember interaction
    public void RepairSystem(CubeInteraction.SystemType systemType, float repairAmount)
    {
        switch (systemType)
        {
            case CubeInteraction.SystemType.Engines:
                engineHealth += repairAmount;
                engineHealth = Mathf.Clamp(engineHealth, 0f, engineMaxHealth); // Ensure health stays within bounds
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
    }
}
