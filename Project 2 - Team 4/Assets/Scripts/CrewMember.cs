using UnityEngine;
using UnityEngine.AI;

public class CrewMember : MonoBehaviour
{
    public enum CrewType { Worker, Engineer }
    public CrewType crewType;

    public string crewName;
    public float taskEfficiency = 1f; // Task efficiency affects repair speed
    public float health = 100f;       // Crew health

    public enum Task { Idle, RepairEngines, RepairLifeSupport, RepairHull }
    public Task currentTask = Task.Idle;

    private NavMeshAgent navAgent;
    private bool isPerformingTask = false; // Flag to check if task is being performed
    private bool isSelected = false; // Flag to check if the crew member is selected
    private float repairProgress = 0f; // Repair progress
    public float repairDuration = 5f; // Duration to repair (adjustable based on damage)

    public Renderer crewRenderer; // Reference to the crew member's Renderer to indicate selection

    // References
    private ShipController shipController;
    private SystemPanelManager systemPanelManager;
    private CubeInteraction.SystemType currentSystemType; // Store the system type

    void Start()
    {
        // Ensure the NavMeshAgent is assigned
        navAgent = GetComponent<NavMeshAgent>();
        if (navAgent == null)
        {
            Debug.LogError("NavMeshAgent not found on " + gameObject.name);
        }

        // Find ShipController if not assigned
        if (shipController == null)
        {
            shipController = FindObjectOfType<ShipController>();
        }

        // Find SystemPanelManager if not assigned
        if (systemPanelManager == null)
        {
            systemPanelManager = FindObjectOfType<SystemPanelManager>();
        }

        if (shipController == null || systemPanelManager == null)
        {
            Debug.LogError("ShipController or SystemPanelManager is not assigned!");
        }
    }

    void Update()
    {
        if (!isSelected) // Only handle movement and tasks when the crew member is not selected
        {
            HandleMovement();
        }
    }

    public void Select()
    {
        isSelected = true;
        HighlightSelection(true); // Highlight the crew member when selected
        Debug.Log(crewName + " is selected.");
    }

    public void Deselect()
    {
        isSelected = false;
        HighlightSelection(false); // Remove highlight when deselected
        Debug.Log(crewName + " is deselected.");
    }

    void HighlightSelection(bool isHighlighted)
    {
        // Change color or material to indicate selection
        if (crewRenderer != null)
        {
            crewRenderer.material.color = isHighlighted ? Color.yellow : Color.white;
        }
    }

    // Updated AssignToTask method
    public void AssignToTask(Task newTask, Vector3 destination, float damage, ShipController controller, CubeInteraction.SystemType systemType)
    {
        currentTask = newTask;
        isPerformingTask = false; // Reset the task performance flag
        repairProgress = 0f; // Reset repair progress
        shipController = controller; // Assign the ShipController
        currentSystemType = systemType; // Store the system type

        // Calculate repair duration based on system damage
        repairDuration = 5f + (damage * 5f); // Adjust as needed

        if (navAgent != null)
        {
            navAgent.SetDestination(destination); // Move to the destination
        }
    }

    void HandleMovement()
    {
        if (!navAgent.pathPending && navAgent.remainingDistance <= navAgent.stoppingDistance && !isPerformingTask)
        {
            PerformTask(); // Perform the task once the crew reaches the destination
        }
    }

    void PerformTask()
    {
        isPerformingTask = true;

        // Check if shipController and systemPanelManager are assigned
        if (shipController == null || systemPanelManager == null)
        {
            Debug.LogError("ShipController or SystemPanelManager is not assigned!");
            return; // Prevent further execution if either is null
        }

        // Simulate repair task over time
        repairProgress += Time.deltaTime;

        // Update progress bar via SystemPanelManager
        systemPanelManager.UpdateRepairProgress(repairProgress / repairDuration);

        if (repairProgress >= repairDuration)
        {
            CompleteTask();
        }
    }

    void CompleteTask()
    {
        Debug.Log(crewName + " has completed the repair task: " + currentTask);

        // Perform the actual repair
        float repairAmount = 20f; // Adjust as needed
        shipController.RepairSystem(currentSystemType, repairAmount);

        isPerformingTask = false;
        currentTask = Task.Idle; // Reset the task
        repairProgress = 0f; // Reset repair progress

        // Keep crew member selectable after task completion
        isSelected = false;
        HighlightSelection(false);
    }

    public void Die()
    {
        Debug.Log(crewName + " has died.");
        Destroy(gameObject);
    }
}
