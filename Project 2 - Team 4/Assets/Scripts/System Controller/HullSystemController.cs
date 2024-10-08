using UnityEngine;

public class HullSystemController : MonoBehaviour
{
    // Hull health variables
    public float hullHealth = 100f;
    public float hullMaxHealth = 100f;
    public GameObject hullCube; // Reference to the visual representation of the hull

    // Fire and smoke effects
    public ParticleSystem hullFireParticles; // Reference to the fire particle system for the hull
    public ParticleSystem[] hullSmokeParticlesArray; // Array of smoke particle systems for the hull
    public bool isHullOnFire = false; // Flag to check if hull is on fire

    // Damage over time variables
    private bool isTakingDamageOverTime = false;
    private float damageOverTimeRate = 0f; // Damage per second

    // Reference to ResourceManager
    public ResourceManager resourceManager;

    void Start()
    {
        // Set fire particle effects inactive at start
        if (hullFireParticles != null)
        {
            hullFireParticles.Stop();
        }

        // Set all smoke particle effects inactive at start
        foreach (ParticleSystem smoke in hullSmokeParticlesArray)
        {
            if (smoke != null)
            {
                smoke.Stop();
            }
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
        // Reduce health over time if the hull is on fire
        if (isHullOnFire)
        {
            UpdateFire();
        }

        // Damage over time due to asteroid or other events
        if (isTakingDamageOverTime)
        {
            float damage = damageOverTimeRate * Time.deltaTime;
            DamageHull(damage);

            // Optionally, stop damage over time when hull health reaches zero
            if (hullHealth <= 0f)
            {
                isTakingDamageOverTime = false;
            }
        }
    }

    // Method to start damage over time
    public void StartDamageOverTime(float damageRate)
    {
        isTakingDamageOverTime = true;
        damageOverTimeRate = damageRate;
        Debug.Log("Hull is now taking damage over time at a rate of " + damageRate + " per second.");

        // Start fire and smoke effects if not already active
        if (!isHullOnFire)
        {
            StartHullFire();
        }
    }

    // Method to stop damage over time
    public void StopDamageOverTime()
    {
        isTakingDamageOverTime = false;
        Debug.Log("Damage over time stopped.");
    }

    // Method to handle hull damage
    public void DamageHull(float damage)
    {
        hullHealth -= damage;
        hullHealth = Mathf.Clamp(hullHealth, 0f, hullMaxHealth);
        Debug.Log("Hull damaged by " + damage + " points. Current health: " + hullHealth);

        // Update smoke particle effects based on hull health
        UpdateSmokeEffects();

        UpdateHullCubeColor();
    }

    // Method to repair the hull
    public void RepairHull(float amount)
    {
        hullHealth += amount;
        hullHealth = Mathf.Clamp(hullHealth, 0f, hullMaxHealth);
        Debug.Log("Hull repaired by " + amount + " points. Current health: " + hullHealth);

        // Stop fire and smoke if hull health is fully restored
        if (hullHealth >= hullMaxHealth)
        {
            StopHullFire();
        }

        // Update smoke particle effects based on hull health
        UpdateSmokeEffects();

        UpdateHullCubeColor();
    }

    // **Added Method to Reduce Hull Integrity**
    // Method to reduce hull integrity by a percentage
    public void ReduceHullIntegrity(float percentage)
    {
        float reductionAmount = hullMaxHealth * (percentage / 100f);
        DamageHull(reductionAmount);
        Debug.Log($"Hull integrity reduced by {percentage}%. Current hull health: {hullHealth}");
    }

    // Method to start hull fire
    public void StartHullFire()
    {
        if (hullFireParticles != null)
        {
            hullFireParticles.Play();
            isHullOnFire = true;
            Debug.Log("Hull fire started!");
        }

        // Start all smoke effects
        foreach (ParticleSystem smoke in hullSmokeParticlesArray)
        {
            if (smoke != null && !smoke.isPlaying)
            {
                smoke.Play();
            }
        }
    }

    // Method to stop hull fire
    public void StopHullFire()
    {
        if (hullFireParticles != null)
        {
            hullFireParticles.Stop();
            isHullOnFire = false;
            Debug.Log("Hull fire stopped!");
        }

        // Stop all smoke effects
        foreach (ParticleSystem smoke in hullSmokeParticlesArray)
        {
            if (smoke != null && smoke.isPlaying)
            {
                smoke.Stop();
            }
        }
    }

    // Update the color of the hull cube based on its health
    public void UpdateHullCubeColor()
    {
        if (hullCube != null)
        {
            Renderer renderer = hullCube.GetComponent<Renderer>();
            if (renderer != null)
            {
                float healthPercentage = hullHealth / hullMaxHealth;
                Color healthColor = Color.Lerp(Color.red, Color.green, healthPercentage);
                renderer.material.color = healthColor;
            }
        }
    }

    // Update fire intensity and damage the hull over time
    private void UpdateFire()
    {
        float fireDamage = Time.deltaTime * 5f; // Damage per second due to fire
        DamageHull(fireDamage);

        // Adjust fire particle system emission rate based on hull health
        if (hullFireParticles != null)
        {
            float healthPercentage = hullHealth / hullMaxHealth;
            var emission = hullFireParticles.emission;
            emission.rateOverTime = Mathf.Lerp(10f, 50f, 1f - healthPercentage); // Increase emission rate as health decreases
        }
    }

    // Update the smoke particle effects based on hull health
    private void UpdateSmokeEffects()
    {
        if (hullSmokeParticlesArray.Length == 0) return;

        float healthPercentage = hullHealth / hullMaxHealth;

        // Keep smoke effects active until hull is fully repaired
        bool hullFullyRepaired = Mathf.Approximately(healthPercentage, 1f);

        if (hullFullyRepaired)
        {
            // Turn off all smoke particles when hull is fully repaired
            foreach (ParticleSystem smoke in hullSmokeParticlesArray)
            {
                if (smoke != null && smoke.isPlaying)
                {
                    smoke.Stop();
                }
            }
            return;
        }

        // Activate and adjust the smoke effects based on damage
        foreach (ParticleSystem smoke in hullSmokeParticlesArray)
        {
            if (smoke != null)
            {
                if (!smoke.isPlaying)
                {
                    smoke.Play();
                }

                // Adjust emission rate based on health percentage
                var emission = smoke.emission;
                emission.rateOverTime = Mathf.Lerp(5f, 30f, 1f - healthPercentage);

                // Adjust smoke color based on health percentage
                var main = smoke.main;
                main.startColor = Color.Lerp(new Color(0.5f, 0.5f, 0.5f, 0.5f), Color.black, 1f - healthPercentage);

                // Adjust smoke lifetime
                main.startLifetime = 10f; // Keep startLifetime at 10 seconds
            }
        }
    }
}
