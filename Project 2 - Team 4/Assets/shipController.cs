using UnityEngine;
using UnityEngine.UI; // Include this for Slider UI elements

public class ShipController : MonoBehaviour
{
    // System Health Variables (0 to 100%)
    public float engineHealth = 100f;
    public float lifeSupportHealth = 100f;
    public float hullIntegrity = 100f;

    // UI Elements
    public Slider engineHealthBar;
    public Slider lifeSupportHealthBar;
    public Slider hullIntegrityBar;

    // Reference to the ResourceManager
    private ResourceManager resourceManager;

    void Start()
    {
        // Find and reference the ResourceManager
        resourceManager = FindObjectOfType<ResourceManager>();

        UpdateSystemUI();
    }

    void Update()
    {
        UpdateSystems();
        UpdateSystemUI();
    }

    void UpdateSystems()
    {
        // Update engine efficiency based on health
        if (resourceManager != null)
        {
            resourceManager.engineEfficiency = Mathf.Clamp(engineHealth / 100f, 0.1f, 1f);
            resourceManager.lifeSupportEfficiency = Mathf.Clamp(lifeSupportHealth / 100f, 0.1f, 1f);
        }

        // Additional logic for hull integrity can be added here
    }

    void UpdateSystemUI()
    {
        if (engineHealthBar != null)
            engineHealthBar.value = engineHealth;

        if (lifeSupportHealthBar != null)
            lifeSupportHealthBar.value = lifeSupportHealth;

        if (hullIntegrityBar != null)
            hullIntegrityBar.value = hullIntegrity;
    }

    // Methods to apply damage or repair systems
    public void DamageEngine(float damage)
    {
        engineHealth -= damage;
        engineHealth = Mathf.Clamp(engineHealth, 0f, 100f);
    }

    public void RepairEngine(float amount)
    {
        engineHealth += amount;
        engineHealth = Mathf.Clamp(engineHealth, 0f, 100f);
    }

    public void DamageLifeSupport(float damage)
    {
        lifeSupportHealth -= damage;
        lifeSupportHealth = Mathf.Clamp(lifeSupportHealth, 0f, 100f);
    }

    public void RepairLifeSupport(float amount)
    {
        lifeSupportHealth += amount;
        lifeSupportHealth = Mathf.Clamp(lifeSupportHealth, 0f, 100f);
    }

    public void DamageHull(float damage)
    {
        hullIntegrity -= damage;
        hullIntegrity = Mathf.Clamp(hullIntegrity, 0f, 100f);
    }

    public void RepairHull(float amount)
    {
        hullIntegrity += amount;
        hullIntegrity = Mathf.Clamp(hullIntegrity, 0f, 100f);
    }
}
