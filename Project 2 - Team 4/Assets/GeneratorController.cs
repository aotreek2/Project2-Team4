using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Added for List<T>

public class GeneratorController : MonoBehaviour
{
    [Header("Generator Settings")]
    public float generatorHealth = 100f;
    public float generatorMaxHealth = 100f;
    public float criticalHealthThreshold = 20f; // Below this, critical state triggers
    public LightFlickerController lightFlickerController;
    public DamageScreenEffects damageScreenEffects; // Reference to the DamageScreenEffects script
    public ShipController shipController;

    public float generatorEfficiency = 1f; // Added declaration

    private bool isCriticalState = false;

    [Header("Testing Hotkeys")]
    public KeyCode damageHotkey = KeyCode.Minus; // Key to damage the generator
    public KeyCode repairHotkey = KeyCode.Equals; // Key to repair the generator
    public float healthChangeAmount = 10f; // Amount to change when pressing the hotkeys

    // To keep track of assigned crew
    public List<CrewMember> assignedCrew = new List<CrewMember>();

    void Start()
    {
        // Ensure light flicker controller is assigned
        if (lightFlickerController == null)
        {
            lightFlickerController = FindObjectOfType<LightFlickerController>();
            if (lightFlickerController == null)
            {
                Debug.LogError("LightFlickerController not found. Please assign it.");
            }
        }

        // Ensure DamageScreenEffects is assigned
        if (damageScreenEffects == null)
        {
            damageScreenEffects = FindObjectOfType<DamageScreenEffects>();
            if (damageScreenEffects == null)
            {
                Debug.LogError("DamageScreenEffects not found. Please assign it.");
            }
        }

        // Initialize light flicker and effects based on the max generator health
        lightFlickerController?.Initialize(generatorMaxHealth);
        UpdateGeneratorHealth(generatorHealth); // Initial update
    }

    void Update()
    {
        // Hotkey to damage the generator
        if (Input.GetKeyDown(damageHotkey))
        {
            DamageGenerator(healthChangeAmount);
        }

        // Hotkey to repair the generator
        if (Input.GetKeyDown(repairHotkey))
        {
            RepairGenerator(healthChangeAmount);
        }
    }

    // Call this to damage the generator
    public void DamageGenerator(float damage)
    {
        generatorHealth -= damage;
        generatorHealth = Mathf.Clamp(generatorHealth, 0f, generatorMaxHealth);
        Debug.Log("Generator damaged by " + damage + " points. Current Health: " + generatorHealth);

        // Check if we enter critical state
        if (generatorHealth <= criticalHealthThreshold && !isCriticalState)
        {
            TriggerCriticalState();
        }

        // Notify the light flicker controller and damage effects
        lightFlickerController?.UpdateGeneratorHealth(generatorHealth);
        damageScreenEffects?.UpdateGeneratorHealth(generatorHealth);

        // Check if generator is down
        if (generatorHealth <= 0f)
        {
            Debug.Log("Generator is down! Systems are failing.");
            // You can trigger additional system failures here
        }
    }

    // Call this to repair the generator
    public void RepairGenerator(float amount)
    {
        generatorHealth += amount;
        generatorHealth = Mathf.Clamp(generatorHealth, 0f, generatorMaxHealth);
        Debug.Log("Generator repaired by " + amount + " points. Current Health: " + generatorHealth);

        // Exit critical state if health goes above the threshold
        if (generatorHealth > criticalHealthThreshold && isCriticalState)
        {
            ExitCriticalState();
        }

        // Notify light flicker controller and damage effects
        lightFlickerController?.UpdateGeneratorHealth(generatorHealth);
        damageScreenEffects?.UpdateGeneratorHealth(generatorHealth);
    }

    // Trigger the critical state (e.g., flicker faster, more intense effects)
    private void TriggerCriticalState()
    {
        Debug.Log("Generator in critical state!");
        isCriticalState = true;
        lightFlickerController?.TriggerMajorFlicker();
    }

    // Exit the critical state when the generator is repaired above the threshold
    private void ExitCriticalState()
    {
        Debug.Log("Generator has exited critical state.");
        isCriticalState = false;
        lightFlickerController?.StabilizeLights();
    }

    // Method to update generator health based on current status
    private void UpdateGeneratorHealth(float currentHealth)
    {
        generatorHealth = currentHealth;
        lightFlickerController?.UpdateGeneratorHealth(generatorHealth);
        damageScreenEffects?.UpdateGeneratorHealth(generatorHealth);
    }

    // Method to assign a crew member to the generator
    public void AssignCrew(CrewMember crewMember)
    {
        if (!assignedCrew.Contains(crewMember))
        {
            assignedCrew.Add(crewMember);
            // Implement logic such as increasing generator efficiency
            generatorEfficiency += crewMember.efficiency;
            generatorEfficiency = Mathf.Clamp(generatorEfficiency, 0f, 2f); // Adjust max as needed
            Debug.Log($"{crewMember.crewName} has been assigned to the Generator. Current Generator Efficiency: {generatorEfficiency}");

            // Optionally, initiate repair
            StartRepair(crewMember, crewMember.efficiency);
        }
    }

    // Method to remove a crew member from the generator
    public void RemoveCrew(CrewMember crewMember)
    {
        if (assignedCrew.Contains(crewMember))
        {
            assignedCrew.Remove(crewMember);
            // Implement logic such as decreasing generator efficiency
            generatorEfficiency -= crewMember.efficiency;
            generatorEfficiency = Mathf.Clamp(generatorEfficiency, 0f, 2f); // Adjust min as needed
            Debug.Log($"{crewMember.crewName} has been removed from the Generator. Current Generator Efficiency: {generatorEfficiency}");
        }
    }

    // Start the repair process for a crew member
    public void StartRepair(CrewMember crewMember, float efficiency)
    {
        StartCoroutine(RepairProcess(crewMember, efficiency));
    }

    // Coroutine to handle the repair process with a chance of crew death
    private IEnumerator RepairProcess(CrewMember crewMember, float efficiency)
    {
        Debug.Log($"{crewMember.crewName} has started repairing the generator.");
        crewMember.isPerformingTask = true;

        // Simulate repair time
        float repairDuration = 5f; // 5 seconds for example
        float elapsedTime = 0f;

        while (elapsedTime < repairDuration)
        {
            elapsedTime += Time.deltaTime;

            // Implement high chance of death
            // For example, 20% chance per second
            if (Random.value < 0.2f * Time.deltaTime) // Adjust the probability as needed
            {
                crewMember.Die();
                yield break; // Exit the coroutine if crew member dies
            }

            yield return null;
        }

        // After repair duration
        generatorHealth += efficiency * 10f; // Example repair logic
        generatorHealth = Mathf.Clamp(generatorHealth, 0f, generatorMaxHealth);
        Debug.Log($"Generator repaired by {efficiency * 10f} points by {crewMember.crewName}.");

        crewMember.CompleteTask();
    }
}
