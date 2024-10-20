using UnityEngine;

public class EngineSystemController : MonoBehaviour
{
    public float engineHealth = 100f;
    public float engineMaxHealth = 100f;
    public float engineEfficiency = 1f;
    public GameObject enginesCube;

    public ParticleSystem engineFireParticles;
    private bool isEngineOnFire = false;
    public MonoBehaviour[] adjacentSystems;

    public float fireIntensity = 0f;
    public float fireSpreadChance = 0.2f;

    public ResourceManager resourceManager;

    void Start()
    {
        // Ensure engine starts with maximum health
        engineHealth = engineMaxHealth;

        if (engineFireParticles != null)
        {
            engineFireParticles.Stop();
        }

        if (resourceManager == null)
        {
            resourceManager = FindObjectOfType<ResourceManager>();
        }

        Debug.Log($"Engine system initialized. Engine health: {engineHealth}/{engineMaxHealth}");
    }

    void Update()
    {
        UpdateEngineEfficiency();
        
        if (isEngineOnFire)
        {
            UpdateFire();
        }

        // Log current engine health
        Debug.Log($"Engine health: {engineHealth}");

        // Add hotkey "L" to lower the engine health by 10 units for testing
        if (Input.GetKeyDown(KeyCode.L))
        {
            DamageEngine(10f); // Damage the engine by 10 units
            Debug.Log("Engine health reduced by 10 for testing purposes.");
        }
    }

    void UpdateEngineEfficiency()
    {
        engineEfficiency = Mathf.Clamp(engineHealth / engineMaxHealth, 0.1f, 1.0f);
        Debug.Log($"Engine efficiency updated. Current efficiency: {engineEfficiency}");

        if (resourceManager != null)
        {
            resourceManager.engineEfficiency = engineEfficiency;
        }
    }

    public void DamageEngine(float damage)
    {
        if (engineHealth > 0)
        {
            engineHealth -= damage;
            engineHealth = Mathf.Clamp(engineHealth, 0f, engineMaxHealth);  // Ensure it never goes below 0

            Debug.Log($"Engine damaged by {damage}. New health: {engineHealth}");

            // Only start fire if engine health is below 50% and no fire has started
            if (engineHealth <= engineMaxHealth * 0.5f && !isEngineOnFire)
            {
                StartEngineFire();
            }
        }
        else
        {
            Debug.Log("Engine is already at 0 health. Cannot reduce further.");
        }
    }

    public void RepairEngine(float amount)
    {
        engineHealth += amount;
        engineHealth = Mathf.Clamp(engineHealth, 0f, engineMaxHealth);

        Debug.Log($"Engine repaired by {amount}. New health: {engineHealth}");

        if (engineHealth == engineMaxHealth && isEngineOnFire)
        {
            StopEngineFire();
        }
    }

    public void StartEngineFire()
    {
        if (engineFireParticles != null)
        {
            engineFireParticles.Play();
            isEngineOnFire = true;
            fireIntensity = 1.0f;
            Debug.Log("Engine fire started!");
            InvokeRepeating("PropagateFire", 5f, 10f);
        }
    }

    public void StopEngineFire()
    {
        if (engineFireParticles != null)
        {
            engineFireParticles.Stop();
            isEngineOnFire = false;
            fireIntensity = 0f;
            Debug.Log("Engine fire stopped.");
            CancelInvoke("PropagateFire");
        }
    }

    public void AddFuel(float fuelAmount)
    {
        // Adding fuel increases engine health and boosts efficiency
        engineHealth += fuelAmount;
        engineHealth = Mathf.Clamp(engineHealth, 0f, engineMaxHealth);

        Debug.Log($"Fuel added: {fuelAmount}. New engine health: {engineHealth}");

        if (engineHealth == engineMaxHealth)
        {
            Debug.Log("Engine fully fueled! Maximum health reached.");
            
            if (isEngineOnFire)
            {
                StopEngineFire(); // Stop the fire if the engine is fully repaired
            }
        }

        UpdateEngineEfficiency(); // Recalculate efficiency based on the new health value
    }

    public void ReduceEngineEfficiency(float percentage)
    {
        float reduction = engineMaxHealth * (percentage / 100f);
        engineHealth -= reduction;
        engineHealth = Mathf.Clamp(engineHealth, 0f, engineMaxHealth);

        Debug.Log($"Engine efficiency reduced by {percentage}%. New health: {engineHealth}");
        
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

                Debug.Log($"Engine cube color updated based on health: {healthPercentage * 100}%");
            }
        }
    }

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
        }
    }

    void TrySpreadFire(MonoBehaviour system)
    {
        if (Random.value < fireSpreadChance)
        {
            if (system is EngineSystemController engineController)
            {
                engineController.StartEngineFire();
                Debug.Log("Fire spread to adjacent engine system.");
            }
            else if (system is HullSystemController hullController)
            {
                hullController.StartHullFire();
                Debug.Log("Fire spread to adjacent hull system.");
            }
        }
    }

    void UpdateFire()
    {
        fireIntensity += Time.deltaTime * 0.1f;
        fireIntensity = Mathf.Clamp(fireIntensity, 0f, 10f);

        DealFireDamage(fireIntensity * Time.deltaTime);

        fireSpreadChance = Mathf.Clamp(fireIntensity * 0.05f, 0.1f, 0.8f);
    }

    void DealFireDamage(float damageAmount)
    {
        engineHealth -= damageAmount;
        engineHealth = Mathf.Clamp(engineHealth, 0f, engineMaxHealth);

        Debug.Log($"Engine took {damageAmount} fire damage. New health: {engineHealth}");

        if (engineHealth <= 0)
        {
            engineHealth = 0;
            StopEngineFire();
            Debug.Log("Engine health has reached 0. Fire stopped.");
        }
    }
}
