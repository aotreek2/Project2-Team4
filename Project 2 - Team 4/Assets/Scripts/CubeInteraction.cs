using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeInteraction : MonoBehaviour
{
    public enum SystemType { LifeSupport, Engines, Hull, Generator }
    public SystemType systemType;

    public RepairProgressBar repairProgressBar;
    public Transform repairPoint; // Ensure this is assigned in the Inspector
    public float baseRepairDuration = 10f; // Base duration to repair from 0% to 100% health without efficiency modifiers

    [Range(0f, 1f)]
    public float baseDeathChanceMultiplier = 0.05f; // Base death chance multiplier

    private List<CrewMember> assignedCrewMembers = new List<CrewMember>();
    private List<CrewMember> crewMembersInRepairZone = new List<CrewMember>();
    private bool isRepairing = false;
    private float repairProgress = 0f;
    private float repairDuration;

    // References to system controllers
    private LifeSupportController lifeSupportController;
    private EngineSystemController engineSystemController;
    private GeneratorController generatorController;
    private HullSystemController hullSystemController;

    // Reference to the DialogueManager
    public DialogueManager dialogueManager;

    void Start()
    {
        Debug.Log($"CubeInteraction: Start method called for {systemType}.");

        // Assign system controllers based on system type
        switch (systemType)
        {
            case SystemType.LifeSupport:
                lifeSupportController = GetComponent<LifeSupportController>();
                if (lifeSupportController != null)
                {
                    lifeSupportController.DamageLifeSupport(50f);
                    Debug.Log("CubeInteraction: LifeSupportController found and damaged by 50.");
                }
                else
                {
                    Debug.LogError($"CubeInteraction: LifeSupportController not found on {gameObject.name}. Please ensure it is attached.");
                }
                break;
            case SystemType.Engines:
                engineSystemController = GetComponent<EngineSystemController>();
                if (engineSystemController != null)
                {
                    engineSystemController.DamageEngine(50f);
                    Debug.Log("CubeInteraction: EngineSystemController found and damaged by 50.");
                }
                else
                {
                    Debug.LogError($"CubeInteraction: EngineSystemController not found on {gameObject.name}. Please ensure it is attached.");
                }
                break;
            case SystemType.Generator:
                generatorController = GetComponent<GeneratorController>();
                if (generatorController != null)
                {
                    generatorController.DamageGenerator(50f);
                    Debug.Log("CubeInteraction: GeneratorController found and damaged by 50.");
                }
                else
                {
                    Debug.LogError($"CubeInteraction: GeneratorController not found on {gameObject.name}. Please ensure it is attached.");
                }
                break;
            case SystemType.Hull:
                hullSystemController = GetComponent<HullSystemController>();
                if (hullSystemController != null)
                {
                    hullSystemController.DamageHull(50f);
                    Debug.Log("CubeInteraction: HullSystemController found and damaged by 50.");
                }
                else
                {
                    Debug.LogError($"CubeInteraction: HullSystemController not found on {gameObject.name}. Please ensure it is attached.");
                }
                break;
        }

        // Ensure repairProgressBar is assigned
        if (repairProgressBar == null)
        {
            repairProgressBar = FindObjectOfType<RepairProgressBar>();
            if (repairProgressBar == null)
            {
                Debug.LogError("CubeInteraction: RepairProgressBar not found in the scene.");
            }
            else
            {
                Debug.Log("CubeInteraction: RepairProgressBar found via FindObjectOfType.");
            }
        }

        // Ensure repairPoint is assigned
        if (repairPoint == null)
        {
            Transform foundRepairPoint = transform.Find("RepairPoint");
            if (foundRepairPoint != null)
            {
                repairPoint = foundRepairPoint;
                Debug.Log("CubeInteraction: RepairPoint found as child and assigned.");
            }
            else
            {
                Debug.LogError($"CubeInteraction: RepairPoint not found as a child of {gameObject.name}. Please assign it in the Inspector.");
            }
        }

        // Hide the repair progress bar initially
        if (repairProgressBar != null)
        {
            repairProgressBar.gameObject.SetActive(false);
            Debug.Log("CubeInteraction: RepairProgressBar hidden initially.");
        }

        // Ensure this GameObject has a Collider set as Trigger for repair zone detection
        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
            Debug.LogError($"CubeInteraction: {gameObject.name} requires a Collider component to detect crew members in the repair zone.");
        }
        else if (!collider.isTrigger)
        {
            collider.isTrigger = true;
            Debug.LogWarning($"CubeInteraction: Collider on {gameObject.name} was not set as Trigger. It has been set to Trigger.");
        }

        // Assign DialogueManager if not assigned
        if (dialogueManager == null)
        {
            dialogueManager = FindObjectOfType<DialogueManager>();
            if (dialogueManager == null)
            {
                Debug.LogError("CubeInteraction: DialogueManager not found in the scene. Please ensure it is present.");
            }
            else
            {
                Debug.Log("CubeInteraction: DialogueManager found via FindObjectOfType.");
            }
        }

        // Optionally, trigger initial dialogue
        // TriggerInitialDialogue();
    }

    // Detect when a crew member enters the repair zone
    private void OnTriggerEnter(Collider other)
    {
        CrewMember crew = other.GetComponent<CrewMember>();
        if (crew != null && assignedCrewMembers.Contains(crew) && !crewMembersInRepairZone.Contains(crew))
        {
            crewMembersInRepairZone.Add(crew);
            Debug.Log($"CubeInteraction: Crew member {crew.crewName} entered the repair zone of {systemType}.");

            if (!isRepairing)
            {
                StartRepair();
            }
        }
    }

    // Detect when a crew member exits the repair zone
    private void OnTriggerExit(Collider other)
    {
        CrewMember crew = other.GetComponent<CrewMember>();
        if (crew != null && crewMembersInRepairZone.Contains(crew))
        {
            crewMembersInRepairZone.Remove(crew);
            Debug.Log($"CubeInteraction: Crew member {crew.crewName} exited the repair zone of {systemType}.");

            if (crewMembersInRepairZone.Count == 0)
            {
                StopRepair();
            }
        }
    }

    // Assign a crew member to repair this system
    public void SetSelectedCrewMember(CrewMember crewMember)
    {
        if (crewMember == null)
        {
            Debug.LogWarning("CubeInteraction: Attempted to assign a null CrewMember.");
            return;
        }

        if (!assignedCrewMembers.Contains(crewMember))
        {
            assignedCrewMembers.Add(crewMember);
            Debug.Log($"CubeInteraction: Crew member {crewMember.crewName} assigned to repair {systemType}.");

            // Move the crew member to the repair point
            crewMember.AssignToRepairPoint(this);
            Debug.Log($"CubeInteraction: Crew member {crewMember.crewName} moved to repair point.");

            // Trigger dialogue when assigning a crewmate
            if (dialogueManager != null)
            {
                string assignmentMessage = $"Crew member {crewMember.crewName} has been assigned to repair {systemType}.";
                // You can choose to use StartDialogue or DisplaySystemInfo based on your design
                // Here, we'll use StartDialogue with a single line
                string[] assignmentDialogue = new string[] { assignmentMessage };
                dialogueManager.StartDialogue(assignmentDialogue, systemType);
                Debug.Log($"CubeInteraction: Assignment dialogue triggered with message: {assignmentMessage}");
            }
            else
            {
                Debug.LogError("CubeInteraction: DialogueManager reference is null. Cannot display assignment dialogue.");
            }
        }
        else
        {
            Debug.LogWarning($"CubeInteraction: Crew member {crewMember.crewName} is already assigned to repair {systemType}.");
        }
    }

    // Public method to start repair with two arguments
    public void StartRepair(CrewMember crewMember, float efficiency)
    {
        Debug.Log($"CubeInteraction: StartRepair called for {systemType} with CrewMember {crewMember.crewName} and efficiency {efficiency}.");
        SetSelectedCrewMember(crewMember);
        // Repair will start when the crew member enters the repair zone
    }

    // Remove a crew member from repairing this system
    public void RemoveCrewMember(CrewMember crewMember)
    {
        if (assignedCrewMembers.Contains(crewMember))
        {
            assignedCrewMembers.Remove(crewMember);
            Debug.Log($"CubeInteraction: Crew member {crewMember.crewName} removed from repairing {systemType}.");

            if (crewMembersInRepairZone.Contains(crewMember))
            {
                crewMembersInRepairZone.Remove(crewMember);
                Debug.Log($"CubeInteraction: Crew member {crewMember.crewName} removed from repair zone of {systemType}.");
            }

            if (assignedCrewMembers.Count == 0)
            {
                StopRepair();
            }
        }
    }

    // Start the repair process
    private void StartRepair()
    {
        Debug.Log($"CubeInteraction: StartRepair initiated for {systemType}.");
        if (IsSystemFullyRepaired())
        {
            Debug.Log($"CubeInteraction: {systemType} is already fully repaired.");
            return;
        }

        if (!isRepairing && crewMembersInRepairZone.Count > 0)
        {
            isRepairing = true;
            repairProgress = 0f; // Reset progress

            // Get the current health and max health from the system controller
            float currentHealth = GetSystemHealth();
            float maxHealth = GetMaxSystemHealth();

            // Adjust the repair duration dynamically based on the system's current damage
            float damageProportion = (maxHealth - currentHealth) / maxHealth; // 0 (no damage) to 1 (fully damaged)
            repairDuration = baseRepairDuration * damageProportion / Mathf.Max(GetAverageEfficiency(), 0.1f); // Avoid division by zero

            // Ensure repairDuration is not zero or too short
            if (repairDuration <= 0f)
            {
                repairDuration = 0.1f;
            }

            Debug.Log($"CubeInteraction: Repair started for {systemType} with duration {repairDuration} seconds.");

            // Show the repair progress bar when repair starts
            if (repairProgressBar != null)
            {
                repairProgressBar.gameObject.SetActive(true);
                repairProgressBar.ResetProgress();
                Debug.Log("CubeInteraction: RepairProgressBar activated and reset.");
            }

            StartCoroutine(RepairSystem());
        }
    }

    // Calculate the average efficiency of all crew members in the repair zone
    private float GetAverageEfficiency()
    {
        if (crewMembersInRepairZone.Count == 0)
            return 1f;

        float totalEfficiency = 0f;
        foreach (var crew in crewMembersInRepairZone)
        {
            totalEfficiency += crew.efficiency; // Assuming CrewMember has an 'efficiency' property
        }

        float average = totalEfficiency / crewMembersInRepairZone.Count;
        Debug.Log($"CubeInteraction: Calculated average efficiency: {average}");
        return average;
    }

    private IEnumerator RepairSystem()
    {
        Debug.Log("CubeInteraction: RepairSystem coroutine started.");
        float elapsedTime = 0f;

        while (elapsedTime < repairDuration)
        {
            if (crewMembersInRepairZone.Count == 0)
            {
                Debug.Log("CubeInteraction: No crew members in repair zone. Stopping repair.");
                StopRepair();
                yield break;
            }

            elapsedTime += Time.deltaTime;
            repairProgress = Mathf.Clamp01(elapsedTime / repairDuration);

            if (repairProgressBar != null)
            {
                repairProgressBar.UpdateRepairProgress(repairProgress); // Update the slider
                Debug.Log($"CubeInteraction: Repair progress updated to {repairProgress * 100f}%.");
            }

            // Calculate death chance based on system health and number of crew members in repair zone
            float currentHealth = GetSystemHealth();
            float deathChance = Mathf.Clamp((1 - (currentHealth / 100f)) * baseDeathChanceMultiplier / Mathf.Max(crewMembersInRepairZone.Count, 1), 0.01f, baseDeathChanceMultiplier);

            Debug.Log($"CubeInteraction: Death chance per crew member during repair: {deathChance * 100f}%");

            // Check for each crew member in the repair zone
            for (int i = crewMembersInRepairZone.Count - 1; i >= 0; i--)
            {
                CrewMember crew = crewMembersInRepairZone[i];
                if (crew == null)
                {
                    crewMembersInRepairZone.RemoveAt(i);
                    Debug.Log("CubeInteraction: Null CrewMember removed from repair zone.");
                    continue;
                }

                if (Random.value < deathChance * Time.deltaTime)
                {
                    Debug.Log($"CubeInteraction: Crew member {crew.crewName} has died during repair of {systemType}.");
                    crew.Die();
                    RemoveCrewMember(crew);
                }
            }

            yield return null;
        }

        Debug.Log($"CubeInteraction: Repair completed for {systemType}.");
        CompleteRepair();
    }

    public void StopRepair()
    {
        Debug.Log($"CubeInteraction: StopRepair called for {systemType}.");
        if (isRepairing)
        {
            StopAllCoroutines();
            isRepairing = false;
            repairProgress = 0f;

            // Hide the repair progress bar
            if (repairProgressBar != null)
            {
                repairProgressBar.gameObject.SetActive(false);
                Debug.Log("CubeInteraction: RepairProgressBar deactivated.");
            }

            Debug.Log($"CubeInteraction: Repair stopped for {systemType}.");
        }
    }

    public void CompleteRepair()
    {
        Debug.Log($"CubeInteraction: CompleteRepair called for {systemType}.");
        isRepairing = false;

        // Set the system's health to maximum after repair is complete
        SetSystemHealthToMax();

        // Hide the repair progress bar when the repair is done
        if (repairProgressBar != null)
        {
            repairProgressBar.gameObject.SetActive(false);
            Debug.Log("CubeInteraction: RepairProgressBar deactivated after completion.");
        }

        repairProgress = 0f;

        Debug.Log($"CubeInteraction: Repair completed for {systemType}.");

        // Notify all crew members that the task is complete
        foreach (var crew in crewMembersInRepairZone)
        {
            if (crew != null)
            {
                crew.CompleteTask();
                Debug.Log($"CubeInteraction: Crew member {crew.crewName} notified of task completion.");
            }
        }

        assignedCrewMembers.Clear();
        crewMembersInRepairZone.Clear();
    }

    // Method to check if the system is fully repaired
    private bool IsSystemFullyRepaired()
    {
        float currentHealth = GetSystemHealth();
        float maxHealth = GetMaxSystemHealth();

        bool isFull = Mathf.Approximately(currentHealth, maxHealth);
        Debug.Log($"CubeInteraction: IsSystemFullyRepaired for {systemType}: {isFull}");
        return isFull;
    }

    /// <summary>
    /// Retrieves the current health of the system based on its type.
    /// </summary>
    private float GetSystemHealth()
    {
        switch (systemType)
        {
            case SystemType.LifeSupport:
                return lifeSupportController != null ? lifeSupportController.lifeSupportHealth : 0f;
            case SystemType.Engines:
                return engineSystemController != null ? engineSystemController.engineHealth : 0f;
            case SystemType.Generator:
                return generatorController != null ? generatorController.generatorHealth : 0f;
            case SystemType.Hull:
                return hullSystemController != null ? hullSystemController.hullHealth : 0f;
            default:
                return 0f;
        }
    }

    /// <summary>
    /// Retrieves the maximum health of the system based on its type.
    /// </summary>
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
            case SystemType.Hull:
                return hullSystemController != null ? hullSystemController.hullMaxHealth : 100f;
            default:
                return 100f;
        }
    }

    /// <summary>
    /// Sets the system's health to its maximum value.
    /// </summary>
    private void SetSystemHealthToMax()
    {
        switch (systemType)
        {
            case SystemType.LifeSupport:
                if (lifeSupportController != null)
                {
                    lifeSupportController.RepairLifeSupport(GetMaxSystemHealth());
                    Debug.Log($"CubeInteraction: LifeSupport repaired to max health {GetMaxSystemHealth()}.");
                }
                break;
            case SystemType.Engines:
                if (engineSystemController != null)
                {
                    engineSystemController.RepairEngine(GetMaxSystemHealth());
                    Debug.Log($"CubeInteraction: Engines repaired to max health {GetMaxSystemHealth()}.");
                }
                break;
            case SystemType.Generator:
                if (generatorController != null)
                {
                    generatorController.RepairGenerator(GetMaxSystemHealth());
                    Debug.Log($"CubeInteraction: Generator repaired to max health {GetMaxSystemHealth()}.");
                }
                break;
            case SystemType.Hull:
                if (hullSystemController != null)
                {
                    hullSystemController.RepairHull(GetMaxSystemHealth());
                    Debug.Log($"CubeInteraction: Hull repaired to max health {GetMaxSystemHealth()}.");
                }
                break;
        }
    }

    // Method to damage the system (through the controller)
    public void DamageSystem(float damageAmount)
    {
        Debug.Log($"CubeInteraction: DamageSystem called for {systemType} with damage amount {damageAmount}.");

        switch (systemType)
        {
            case SystemType.LifeSupport:
                if (lifeSupportController != null)
                {
                    lifeSupportController.DamageLifeSupport(damageAmount);
                    Debug.Log($"CubeInteraction: LifeSupport damaged by {damageAmount}. Current Health: {lifeSupportController.lifeSupportHealth}");
                }
                break;
            case SystemType.Engines:
                if (engineSystemController != null)
                {
                    engineSystemController.DamageEngine(damageAmount);
                    Debug.Log($"CubeInteraction: Engines damaged by {damageAmount}. Current Health: {engineSystemController.engineHealth}");
                }
                break;
            case SystemType.Generator:
                if (generatorController != null)
                {
                    generatorController.DamageGenerator(damageAmount);
                    Debug.Log($"CubeInteraction: Generator damaged by {damageAmount}. Current Health: {generatorController.generatorHealth}");
                }
                break;
            case SystemType.Hull:
                if (hullSystemController != null)
                {
                    hullSystemController.DamageHull(damageAmount);
                    Debug.Log($"CubeInteraction: Hull damaged by {damageAmount}. Current Health: {hullSystemController.hullHealth}");
                }
                break;
        }

        CheckCriticalState();
    }

    private void CheckCriticalState()
    {
        switch (systemType)
        {
            case SystemType.Generator:
                if (generatorController != null && generatorController.generatorHealth <= 0)
                {
                    Debug.Log("CubeInteraction: Generator is down! Systems are failing.");
                    // Implement further logic for critical failure
                }
                break;
            // Add similar checks for other systems if needed
        }
    }

    /// <summary>
    /// Method to handle system clicks and display system info via DialogueManager
    /// </summary>
    public void OnSystemClicked()
    {
        Debug.Log($"CubeInteraction: OnSystemClicked called for {systemType}.");
        if (dialogueManager == null)
        {
            Debug.LogError("CubeInteraction: DialogueManager is not assigned.");
            return;
        }

        // Retrieve system health
        float systemHealth = GetSystemHealth();
        Debug.Log($"CubeInteraction: Retrieved system health for {systemType}: {systemHealth}%.");

        // Calculate death chance based on system health
        float deathChance = CalculateDeathChance(systemHealth);
        Debug.Log($"CubeInteraction: Calculated death chance for {systemType}: {deathChance * 100f}%.");

        // Get system name as string
        string systemName = systemType.ToString();

        // Display system info via DialogueManager
        dialogueManager.DisplaySystemInfo(systemName, systemHealth, deathChance);
        Debug.Log($"CubeInteraction: DisplaySystemInfo called on DialogueManager for {systemName}.");
    }

    /// <summary>
    /// Calculate the chance of a crew member dying during repair based on system health
    /// </summary>
    private float CalculateDeathChance(float systemHealth)
    {
        // Example logic: Higher damage increases death chance
        // Ensure that death chance is between 1% and baseDeathChanceMultiplier (e.g., 5%)
        float minDeathChance = 0.01f; // 1%
        float maxDeathChance = baseDeathChanceMultiplier; // 5%
        float damageProportion = Mathf.Clamp01((100f - systemHealth) / 100f); // 0 to 1
        float deathChance = Mathf.Lerp(minDeathChance, maxDeathChance, damageProportion);
        Debug.Log($"CubeInteraction: Death chance calculated as {deathChance * 100f}% for {systemType}.");
        return deathChance;
    }

    /// <summary>
    /// Example method to trigger initial dialogue (e.g., on game start)
    /// </summary>
    public void TriggerInitialDialogue()
    {
        Debug.Log("CubeInteraction: TriggerInitialDialogue called.");
        if (dialogueManager != null)
        {
            string[] introLines = new string[]
            {
                "Welcome to the ship.",
                "Your mission is to repair the critical systems."
            };
            dialogueManager.StartDialogue(introLines, systemType);
            Debug.Log("CubeInteraction: Initial dialogue started.");
        }
        else
        {
            Debug.LogError("CubeInteraction: DialogueManager is not assigned. Cannot trigger initial dialogue.");
        }
    }
}
