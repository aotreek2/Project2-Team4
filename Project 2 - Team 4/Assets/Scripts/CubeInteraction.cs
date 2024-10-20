using System.Collections;
using UnityEngine;

public class CubeInteraction : MonoBehaviour
{
    public enum SystemType { LifeSupport, Engines, Hull, Generator }
    public SystemType systemType;

    public RepairProgressBar repairProgressBar;
    public Collider repairZone;
    public float baseRepairDuration = 10f; // Base duration to repair from 0% to 100% health without efficiency modifiers

    private CrewMember assignedCrewMember;
    private bool isRepairing = false;
    private float repairProgress = 0f;
    private float repairDuration;

    // References to system controllers
    private LifeSupportController lifeSupportController;
    private EngineSystemController engineSystemController;
    private GeneratorController generatorController;

    void Start()
    {
        if (repairProgressBar == null)
        {
            Debug.LogError("Repair progress bar not found. Please assign it in the Inspector.");
        }

        if (repairZone == null)
        {
            Debug.LogError("Repair zone not found. Please assign it in the Inspector.");
        }

        // Hide the repair progress bar initially
        if (repairProgressBar != null)
        {
            repairProgressBar.gameObject.SetActive(false);
        }

        // Get reference to the appropriate system controller based on systemType
        switch (systemType)
        {
            case SystemType.LifeSupport:
                lifeSupportController = GetComponent<LifeSupportController>();
                if (lifeSupportController == null)
                {
                    Debug.LogError("LifeSupportController not found on " + gameObject.name);
                }
                else
                {
                    lifeSupportController.DamageLifeSupport(50f);
                }
                break;
            case SystemType.Engines:
                engineSystemController = GetComponent<EngineSystemController>();
                if (engineSystemController == null)
                {
                    Debug.LogError("EngineSystemController not found on " + gameObject.name);
                }
                else
                {
                    engineSystemController.DamageEngine(50f);
                }
                break;
            case SystemType.Generator:
                generatorController = GetComponent<GeneratorController>();
                if (generatorController == null)
                {
                    Debug.LogError("GeneratorController not found on " + gameObject.name);
                }
                else
                {
                    generatorController.DamageGenerator(50f);
                }
                break;
        }
    }

    // Assign a crew member to repair this system
    public void SetSelectedCrewMember(CrewMember crewMember)
    {
        assignedCrewMember = crewMember;

        if (assignedCrewMember == null)
        {
            Debug.LogError("No crew member selected!");
            return;
        }

        Debug.Log("Crew member " + assignedCrewMember.crewName + " assigned to system: " + systemType);

        // Move the crew member to the system
        assignedCrewMember.AssignToSystem(this);
    }

    void OnTriggerEnter(Collider other)
    {
        CrewMember crew = other.GetComponent<CrewMember>();
        if (crew != null && crew == assignedCrewMember)
        {
            Debug.Log("Assigned crewmate entered the repair zone.");

            // Start the repair process
            StartRepair(crew, crew.taskEfficiency);
        }
    }

    void OnTriggerExit(Collider other)
    {
        CrewMember crew = other.GetComponent<CrewMember>();
        if (crew != null && crew == assignedCrewMember)
        {
            Debug.Log("Assigned crewmate exited the repair zone.");

            // Stop the repair process
            StopRepair();
        }
    }

    // Start the repair process
    public void StartRepair(CrewMember crewMember, float efficiency)
    {
        // Check if the system is already fully repaired
        if (IsSystemFullyRepaired())
        {
            Debug.Log("System " + systemType + " is already fully repaired.");
            return;
        }

        if (!isRepairing)
        {
            isRepairing = true;
            repairProgress = 0f; // Reset progress

            // Get the current health and max health from the system controller
            float currentHealth = GetCurrentSystemHealth();
            float maxHealth = GetMaxSystemHealth();

            // Adjust the repair duration dynamically based on the system's current damage
            float damageProportion = (maxHealth - currentHealth) / maxHealth;
            repairDuration = baseRepairDuration * damageProportion / efficiency;

            // Ensure repairDuration is not zero
            if (repairDuration <= 0f)
            {
                repairDuration = 0.1f;
            }

            Debug.Log("Repair started on system: " + systemType + " with repair duration: " + repairDuration);

            // Show the repair progress bar when repair starts
            if (repairProgressBar != null)
            {
                repairProgressBar.gameObject.SetActive(true);
            }

            StartCoroutine(RepairSystem(crewMember, repairDuration));
        }
    }

    private IEnumerator RepairSystem(CrewMember crewMember, float duration)
    {
        float elapsedTime = 0f;
        float currentHealth = GetCurrentSystemHealth();

        // Calculate death chance based on system health
        float deathChance = Mathf.Clamp(1 - (currentHealth / 100f), 0f, 0.5f); // Up to 50% chance of death

        // Smoothly repair over time
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            repairProgress = Mathf.Clamp01(elapsedTime / duration);

            if (repairProgressBar != null)
            {
                repairProgressBar.UpdateRepairProgress(repairProgress); // Update the slider
            }

            // Randomly check for crew member death during repair
            if (Random.value < deathChance)
            {
                crewMember.Die();
                StopRepair();
                yield break; // Stop the repair process if the crew member dies
            }

            yield return null;
        }

        CompleteRepair();
        crewMember.CompleteTask();
    }

    public void StopRepair()
    {
        if (isRepairing)
        {
            StopAllCoroutines();
            isRepairing = false;
            repairProgress = 0f;
            Debug.Log("Repair stopped on system: " + systemType);

            // Hide the repair progress bar
            if (repairProgressBar != null)
            {
                repairProgressBar.gameObject.SetActive(false);
            }
        }
    }

    public void CompleteRepair()
    {
        isRepairing = false;

        // Set the system's health to maximum after repair is complete
        SetSystemHealthToMax();
        Debug.Log("Repair completed on system: " + systemType);

        // Hide the repair progress bar when the repair is done
        if (repairProgressBar != null)
        {
            repairProgressBar.gameObject.SetActive(false);
        }

        repairProgress = 0f;
    }

    // Method to check if the system is fully repaired
    private bool IsSystemFullyRepaired()
    {
        float currentHealth = GetCurrentSystemHealth();
        float maxHealth = GetMaxSystemHealth();

        return Mathf.Approximately(currentHealth, maxHealth);
    }

    // Methods to get and set system health through the controllers
    private float GetCurrentSystemHealth()
    {
        switch (systemType)
        {
            case SystemType.LifeSupport:
                return lifeSupportController != null ? lifeSupportController.lifeSupportHealth : 0f;
            case SystemType.Engines:
                return engineSystemController != null ? engineSystemController.engineHealth : 0f;
            case SystemType.Generator:
                return generatorController != null ? generatorController.generatorHealth : 0f;
            default:
                return 0f;
        }
    }

    private float GetMaxSystemHealth()
    {
        switch (systemType)
        {
            case SystemType.LifeSupport:
                return lifeSupportController != null ? lifeSupportController.lifeSupportMaxHealth : 100f;
            case SystemType.Engines:
                return engineSystemController != null ? engineSystemController.engineMaxHealth : 100f;
            case SystemType.Generator:
                return generatorController != null ? generatorController.generatorMaxHealth : 100f;
            default:
                return 100f;
        }
    }

    private void SetSystemHealthToMax()
    {
        switch (systemType)
        {
            case SystemType.LifeSupport:
                if (lifeSupportController != null)
                {
                    lifeSupportController.RepairLifeSupport(GetMaxSystemHealth());
                }
                break;
            case SystemType.Engines:
                if (engineSystemController != null)
                {
                    engineSystemController.RepairEngine(GetMaxSystemHealth());
                }
                break;
            case SystemType.Generator:
                if (generatorController != null)
                {
                    generatorController.RepairGenerator(GetMaxSystemHealth());
                }
                break;
        }
    }

    // Example method to damage the system (through the controller)
    public void DamageSystem(float damageAmount)
    {
        switch (systemType)
        {
            case SystemType.LifeSupport:
                if (lifeSupportController != null)
                {
                    lifeSupportController.DamageLifeSupport(damageAmount);
                    Debug.Log("System " + systemType + " damaged. Current health: " + lifeSupportController.lifeSupportHealth);
                }
                break;
            case SystemType.Engines:
                if (engineSystemController != null)
                {
                    engineSystemController.DamageEngine(damageAmount);
                    Debug.Log("System " + systemType + " damaged. Current health: " + engineSystemController.engineHealth);
                }
                break;
            case SystemType.Generator:
                if (generatorController != null)
                {
                    generatorController.DamageGenerator(damageAmount);
                    Debug.Log("System " + systemType + " damaged. Current health: " + generatorController.generatorHealth);
                }
                break;
        }
    }

    public float GetSystemHealth()
    {
        switch (systemType)
        {
            case SystemType.LifeSupport:
                return lifeSupportController != null ? lifeSupportController.lifeSupportHealth : 0f;
            case SystemType.Engines:
                return engineSystemController != null ? engineSystemController.engineHealth : 0f;
            case SystemType.Generator:
                return generatorController != null ? generatorController.generatorHealth : 0f;
            default:
                return 0f;
        }
    }
}
