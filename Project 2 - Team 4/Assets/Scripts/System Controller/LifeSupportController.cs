using UnityEngine;
using System.Collections;

public class LifeSupportController : MonoBehaviour
{
    // Life support health variables
    public float lifeSupportHealth = 100f;
    public float lifeSupportMaxHealth = 100f;

    // Fire and smoke effects
    public ParticleSystem lifeSupportFireParticles; // Reference to the fire particle system for life support
    public ParticleSystem[] lifeSupportSmokeParticlesArray; // Array of smoke particle systems for life support
    public bool isLifeSupportOnFire = false; // Flag to check if life support is on fire

    // Damage over time variables
    private bool isTakingDamageOverTime = false;
    private float damageOverTimeRate = 0f; // Damage per second due to fire

    // Reference to ShipController to access generator health
    private ShipController shipController;

    // Property to get life support efficiency
    public float LifeSupportEfficiency
    {
        get
        {
            return lifeSupportHealth / lifeSupportMaxHealth;
        }
    }

    void Start()
    {
        // Find and reference the ShipController to access generator health
        shipController = FindObjectOfType<ShipController>();

        if (shipController == null)
        {
            Debug.LogError("ShipController not found in the scene.");
        }

        // Set fire particle effects inactive at start
        if (lifeSupportFireParticles != null)
        {
            lifeSupportFireParticles.Stop();
        }

        // Set all smoke particle effects inactive at start
        foreach (ParticleSystem smoke in lifeSupportSmokeParticlesArray)
        {
            if (smoke != null)
            {
                smoke.Stop();
            }
        }
    }

    void Update()
    {
        // Reduce health over time if the life support is on fire
        if (isLifeSupportOnFire)
        {
            UpdateFire();
        }

        // Damage over time due to other events (if any)
        if (isTakingDamageOverTime)
        {
            float damage = damageOverTimeRate * Time.deltaTime;
            DamageLifeSupport(damage);

            // Optionally, stop damage over time when health reaches zero
            if (lifeSupportHealth <= 0f)
            {
                isTakingDamageOverTime = false;
            }
        }
    }

    // Method to handle life support damage
    public void DamageLifeSupport(float damage)
    {
        lifeSupportHealth -= damage;
        lifeSupportHealth = Mathf.Clamp(lifeSupportHealth, 0f, lifeSupportMaxHealth);
        Debug.Log("Life Support damaged by " + damage + " points. Current health: " + lifeSupportHealth);

        // Trigger fire breakout when life support health falls below a threshold
        if (lifeSupportHealth <= lifeSupportMaxHealth * 0.5f && !isLifeSupportOnFire)
        {
            StartLifeSupportFire();
        }

        // Update smoke particle effects based on life support health
        UpdateSmokeEffects();
    }

    // Method to repair the life support
    public void RepairLifeSupport(float amount)
    {
        lifeSupportHealth += amount;
        lifeSupportHealth = Mathf.Clamp(lifeSupportHealth, 0f, lifeSupportMaxHealth);
        Debug.Log("Life Support repaired by " + amount + " points. Current health: " + lifeSupportHealth);

        // Stop fire if life support health is fully restored
        if (lifeSupportHealth >= lifeSupportMaxHealth)
        {
            StopLifeSupportFire();
        }

        // Update smoke particle effects based on life support health
        UpdateSmokeEffects();
    }

    // Method to start life support fire
    public void StartLifeSupportFire()
    {
        if (lifeSupportFireParticles != null)
        {
            lifeSupportFireParticles.Play();
            isLifeSupportOnFire = true;
            Debug.Log("Life Support fire started!");
        }

        // Start all smoke effects
        foreach (ParticleSystem smoke in lifeSupportSmokeParticlesArray)
        {
            if (smoke != null && !smoke.isPlaying)
            {
                smoke.Play();
            }
        }
    }

    // Method to stop life support fire
    public void StopLifeSupportFire()
    {
        if (lifeSupportFireParticles != null)
        {
            lifeSupportFireParticles.Stop();
            isLifeSupportOnFire = false;
            Debug.Log("Life Support fire stopped!");
        }

        // Stop all smoke effects
        foreach (ParticleSystem smoke in lifeSupportSmokeParticlesArray)
        {
            if (smoke != null && smoke.isPlaying)
            {
                smoke.Stop();
            }
        }
    }

    // Update fire intensity and damage the life support over time
    private void UpdateFire()
    {
        float fireDamage = Time.deltaTime * 5f; // Damage per second due to fire
        DamageLifeSupport(fireDamage);

        // Adjust fire particle system emission rate based on life support health
        if (lifeSupportFireParticles != null)
        {
            float healthPercentage = lifeSupportHealth / lifeSupportMaxHealth;
            var emission = lifeSupportFireParticles.emission;
            emission.rateOverTime = Mathf.Lerp(10f, 50f, 1f - healthPercentage); // Increase emission rate as health decreases
        }
    }

    // Update the smoke particle effects based on life support health
    private void UpdateSmokeEffects()
    {
        if (lifeSupportSmokeParticlesArray.Length == 0) return;

        float healthPercentage = lifeSupportHealth / lifeSupportMaxHealth;

        // Keep smoke effects active until life support is fully repaired
        bool lifeSupportFullyRepaired = Mathf.Approximately(healthPercentage, 1f);

        if (lifeSupportFullyRepaired)
        {
            // Turn off all smoke particles when life support is fully repaired
            foreach (ParticleSystem smoke in lifeSupportSmokeParticlesArray)
            {
                if (smoke != null && smoke.isPlaying)
                {
                    smoke.Stop();
                }
            }
            return;
        }

        // Activate and adjust the smoke effects based on damage
        foreach (ParticleSystem smoke in lifeSupportSmokeParticlesArray)
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

    // Method to reduce life support efficiency by a percentage
    public void ReduceLifeSupportEfficiency(float percentage)
    {
        float reductionAmount = lifeSupportMaxHealth * (percentage / 100f);
        DamageLifeSupport(reductionAmount);
    }
}
