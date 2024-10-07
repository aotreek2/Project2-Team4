using UnityEngine;

public class EngineSystemController : MonoBehaviour
{
    // Engine health and efficiency variables
    public float engineHealth = 100f;
    public float engineMaxHealth = 100f;
    public float engineEfficiency = 1f; // Efficiency will decrease as health decreases
    public GameObject enginesCube; // Add this in EngineSystemController

    // Fire effects
    public ParticleSystem engineFireParticles; // Reference to the fire particle system for engine
    private bool isEngineOnFire = false;

    // Reference to ResourceManager
    public ResourceManager resourceManager;

    void Start()
    {
        // Set fire particle effects inactive at start
        if (engineFireParticles != null)
        {
            engineFireParticles.Stop();
        }

        // Initialize ResourceManager
        if (resourceManager == null)
        {
            resourceManager = FindObjectOfType<ResourceManager>();
            if (resourceManager == null)
            {
                Debug.LogError("ResourceManager not found. Ensure it's properly set in the scene.");
            }
        }
    }

    void Update()
    {
        UpdateEngineEfficiency();
    }

    // Update the engine efficiency based on current health
    void UpdateEngineEfficiency()
    {
        engineEfficiency = Mathf.Clamp(engineHealth / engineMaxHealth, 0.1f, 1.0f); // Efficiency decreases with health

        if (resourceManager != null)
        {
            resourceManager.engineEfficiency = engineEfficiency;
        }
    }

    // Method to handle engine damage
    public void DamageEngine(float damage)
    {
        engineHealth -= damage;
        engineHealth = Mathf.Clamp(engineHealth, 0f, engineMaxHealth);
        Debug.Log("Engine damaged by " + damage + " points. Current health: " + engineHealth);

        // Trigger fire breakout when engine health falls below a threshold
        if (engineHealth <= engineMaxHealth * 0.5f && !isEngineOnFire)
        {
            StartEngineFire();
        }
    }

    // Method to repair the engine
    public void RepairEngine(float amount)
    {
        engineHealth += amount;
        engineHealth = Mathf.Clamp(engineHealth, 0f, engineMaxHealth);
        Debug.Log("Engine repaired by " + amount + " points. Current health: " + engineHealth);

        // Stop fire if engine health is fully restored
        if (engineHealth == engineMaxHealth && isEngineOnFire)
        {
            StopEngineFire();
        }
    }

    // Method to start engine fire
    private void StartEngineFire()
    {
        if (engineFireParticles != null)
        {
            engineFireParticles.Play();
            isEngineOnFire = true;
            Debug.Log("Engine fire started!");
        }
    }

    // Method to stop engine fire
    private void StopEngineFire()
    {
        if (engineFireParticles != null)
        {
            engineFireParticles.Stop();
            isEngineOnFire = false;
            Debug.Log("Engine fire stopped!");
        }
    }

    // Method to reduce engine efficiency
    public void ReduceEngineEfficiency(float percentage)
    {
        float reduction = engineMaxHealth * (percentage / 100f);
        engineHealth -= reduction;
        engineHealth = Mathf.Clamp(engineHealth, 0f, engineMaxHealth);
        Debug.Log($"Engine efficiency reduced by {percentage}%. Current engine health: {engineHealth}");

        UpdateEngineEfficiency();
    }

    public void UpdateEngineCubeColor()
    {
        if (enginesCube != null)
        {
            Renderer renderer = enginesCube.GetComponent<Renderer>();
            if (renderer != null)
            {
                float healthPercentage = engineHealth / engineMaxHealth;
                Color healthColor = Color.Lerp(Color.red, Color.green, healthPercentage);
                renderer.material.color = healthColor;
            }
        }
    }
}
