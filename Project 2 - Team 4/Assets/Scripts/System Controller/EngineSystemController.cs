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
    public MonoBehaviour[] adjacentSystems; // Reference to other systems adjacent to this one

    // Fire propagation variables
    public float fireIntensity = 0f; // Fire intensity level
    public float fireSpreadChance = 0.2f; // Probability that fire spreads to adjacent systems per interval (20% in this example)

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
        if (isEngineOnFire)
        {
            UpdateFire();
        }
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
            fireIntensity = 1.0f; // Set initial intensity
            InvokeRepeating("PropagateFire", 5f, 10f); // Attempt to spread fire every 10 seconds
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
            fireIntensity = 0f;
            CancelInvoke("PropagateFire");
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

    // Fire propagation logic
    void PropagateFire()
    {
        if (!isEngineOnFire) return;

        foreach (MonoBehaviour adjacentSystem in adjacentSystems)
        {
            if (adjacentSystem is EngineSystemController engineController && !engineController.isEngineOnFire)
            {
                TrySpreadFire(engineController);
            }
            else if (adjacentSystem is HullSystemController hullController && !hullController.isHullOnFire)
            {
                TrySpreadFire(hullController);
            }
            // Add more conditions here for other types of systems if needed.
        }
    }

    private void TrySpreadFire(MonoBehaviour system)
    {
        // Roll for fire spread chance
        if (Random.value < fireSpreadChance)
        {
            if (system is EngineSystemController engineController)
            {
                engineController.StartEngineFire();
                Debug.Log($"{gameObject.name} fire spread to {engineController.gameObject.name}");
            }
            else if (system is HullSystemController hullController)
            {
                hullController.StartHullFire();
                Debug.Log($"{gameObject.name} fire spread to {hullController.gameObject.name}");
            }
        }
    }

    // Update fire intensity over time
    void UpdateFire()
    {
        // Gradually increase fire intensity
        fireIntensity += Time.deltaTime * 0.1f; // Fire grows over time
        fireIntensity = Mathf.Clamp(fireIntensity, 0f, 10f);

        // Deal damage to the engine based on fire intensity
        DealFireDamage(fireIntensity * Time.deltaTime);

        // Increase spread chance with higher intensity
        fireSpreadChance = Mathf.Clamp(fireIntensity * 0.05f, 0.1f, 0.8f); // Spread chance increases with intensity
    }

    // Deal damage to the engine based on fire intensity
    void DealFireDamage(float damageAmount)
    {
        engineHealth -= damageAmount;
        engineHealth = Mathf.Clamp(engineHealth, 0f, engineMaxHealth);
        Debug.Log($"Engine is taking fire damage: {damageAmount} points. Current health: {engineHealth}");

        if (engineHealth <= 0)
        {
            engineHealth = 0;
            Debug.Log($"{gameObject.name} system is critically damaged due to fire!");
            StopEngineFire(); // Stop the fire once the engine is completely destroyed
        }
    }
}
