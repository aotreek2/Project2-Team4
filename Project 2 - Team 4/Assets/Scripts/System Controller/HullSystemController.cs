using UnityEngine;

public class HullSystemController : MonoBehaviour
{
    public GameObject hullCube; // Reference to the hull cube
    public float hullIntegrity = 100f; // Current hull integrity
    public float hullMaxIntegrity = 100f; // Maximum hull integrity

    // Array of particle systems for random damage effects throughout the ship
    public ParticleSystem[] hullDamageParticles;

    void Start()
    {
        // Initialize particle systems as inactive at the start
        DisableAllParticleEffects();

        // Initialize the hull cube color
        UpdateHullCubeColor();
    }

    // Method to disable all particle effects at the start of the game
    private void DisableAllParticleEffects()
    {
        if (hullDamageParticles != null && hullDamageParticles.Length > 0)
        {
            foreach (ParticleSystem ps in hullDamageParticles)
            {
                if (ps != null)
                {
                    ps.Stop(); // Ensure that the particle system is stopped
                    ps.Clear(); // Clear any existing particles
                }
            }
        }
    }

    // Method to update the hull cube's color based on its integrity
    public void UpdateHullCubeColor()
    {
        if (hullCube != null)
        {
            Renderer renderer = hullCube.GetComponent<Renderer>();
            if (renderer != null)
            {
                float healthPercentage = hullIntegrity / hullMaxIntegrity;
                Color healthColor = Color.Lerp(Color.red, Color.green, healthPercentage);
                renderer.material.color = healthColor;
            }
        }
    }

    // Method to reduce hull integrity by a percentage
    public void ReduceHullIntegrity(float percentage)
    {
        float reduction = hullMaxIntegrity * (percentage / 100f);
        hullIntegrity -= reduction;
        hullIntegrity = Mathf.Clamp(hullIntegrity, 0f, hullMaxIntegrity);
        Debug.Log($"Hull integrity reduced by {percentage}%. Current Hull Integrity: {hullIntegrity}");

        // Trigger particle effects based on hull damage
        TriggerRandomHullDamageParticles();
    }

    // Method to repair the hull by a given amount
    public void RepairHull(float amount)
    {
        hullIntegrity += amount;
        hullIntegrity = Mathf.Clamp(hullIntegrity, 0f, hullMaxIntegrity);
        Debug.Log($"Hull repaired by {amount} points. Current Hull Integrity: {hullIntegrity}");

        // Update hull color after repair
        UpdateHullCubeColor();
    }

    // Method to apply damage to the hull
    public void DamageHull(float damage)
    {
        hullIntegrity -= damage;
        hullIntegrity = Mathf.Clamp(hullIntegrity, 0f, hullMaxIntegrity);
        Debug.Log($"Hull damaged by {damage} points. Current Hull Integrity: {hullIntegrity}");

        // Trigger particle effects based on hull damage
        TriggerRandomHullDamageParticles();

        // Update hull color after damage
        UpdateHullCubeColor();
    }

    // Method to randomly trigger particle effects based on damage severity
    private void TriggerRandomHullDamageParticles()
    {
        if (hullDamageParticles == null || hullDamageParticles.Length == 0)
        {
            Debug.LogWarning("No hull damage particle systems assigned!");
            return;
        }

        // Calculate how many particle systems to trigger based on damage (the lower the integrity, the more particles)
        int particleCount = Mathf.FloorToInt((1 - (hullIntegrity / hullMaxIntegrity)) * hullDamageParticles.Length);
        particleCount = Mathf.Clamp(particleCount, 1, hullDamageParticles.Length);

        // Shuffle the particle system array to randomize selection
        ShuffleParticleArray();

        // Trigger the first `particleCount` particle systems
        for (int i = 0; i < particleCount; i++)
        {
            if (hullDamageParticles[i] != null)
            {
                hullDamageParticles[i].Play(); // Start the particle effect
            }
        }
    }

    // Utility method to shuffle the particle systems array for randomness
    private void ShuffleParticleArray()
    {
        for (int i = 0; i < hullDamageParticles.Length; i++)
        {
            int randIndex = Random.Range(0, hullDamageParticles.Length);
            ParticleSystem temp = hullDamageParticles[i];
            hullDamageParticles[i] = hullDamageParticles[randIndex];
            hullDamageParticles[randIndex] = temp;
        }
    }
}
