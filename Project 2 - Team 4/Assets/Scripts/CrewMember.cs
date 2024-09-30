using UnityEngine;
using UnityEngine.AI;

public class CrewMember : MonoBehaviour
{
    public enum CrewType { Worker, Engineer }
    public CrewType crewType;

    public string crewName;
    public float taskEfficiency = 1f; // Task efficiency affects repair speed
    public float health = 100f;       // Crew health

    public enum Task { Idle, RepairEngines, RepairLifeSupport, RepairHull, Wander }
    public Task currentTask = Task.Idle;

    private NavMeshAgent navAgent;
    private bool isPerformingTask = false; // Flag to check if task is being performed
    private float repairProgress = 0f; // Repair progress
    public float repairDuration = 5f; // Duration to repair (adjustable based on damage)

    public Renderer crewRenderer; // Reference to the crew member's Renderer to indicate selection

    // References
    private ShipController shipController;
    private SystemPanelManager systemPanelManager;
    private CubeInteraction currentCubeInteraction; // Reference to the cube (system part)

    private Vector3 systemDestination; // Destination of the system to repair

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
        if (!isPerformingTask && currentTask != Task.Idle)
        {
            HandleMovement();
        }
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("System"))  // Assuming your system cubes have the "System" tag
        {
            EnterRepairZone();
        }
    }

    


    public void Select()
    {
        HighlightSelection(true); // Highlight the crew member when selected
        Debug.Log(crewName + " is selected.");
    }

    public void Deselect()
    {
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
    public void AssignToTask(Task newTask, Vector3 destination, ShipController controller, CubeInteraction cubeInteraction)
    {
        currentTask = newTask;
        isPerformingTask = false; // Reset the task performance flag
        repairProgress = 0f; // Reset repair progress
        shipController = controller; // Assign the ShipController
        currentCubeInteraction = cubeInteraction; // Store the reference to the system cube
        systemDestination = destination; // Assign the system destination

        if (navAgent != null)
        {
            navAgent.SetDestination(destination); // Move to the destination
        }
        else
        {
            Debug.LogError($"{crewName} has no NavMeshAgent assigned!");
        }

        Deselect(); // Deselect the crew member to allow movement
    }


    void HandleMovement()
    {
        if (!navAgent.pathPending && navAgent.remainingDistance <= navAgent.stoppingDistance)
        {
            Debug.Log($"{crewName} has arrived at destination.");
            // Wait until entering the repair zone
        }
    }

    // This method will be called when the crew enters the box collider around the system
    public void EnterRepairZone()
    {
        Debug.Log($"{crewName} entered repair zone for {currentCubeInteraction.systemType}.");
        systemPanelManager.StartRepair();  // Trigger repair now that the crew is in the repair zone
        PerformTask();
    }


    void PerformTask()
    {
        isPerformingTask = true;

        // Add debug to confirm task execution
        Debug.Log($"{crewName} started performing task: {currentTask}");

        // Check if shipController and systemPanelManager are assigned
        if (shipController == null || systemPanelManager == null)
        {
            Debug.LogError("ShipController or SystemPanelManager is not assigned!");
            return; // Prevent further execution if either is null
        }

        // Start the repair process in the system cube
        currentCubeInteraction.StartRepair(this);
    }

    // **Updated to public**: Notify the cube interaction script when the task is complete
    public void CompleteTask()
    {
        Debug.Log(crewName + " has completed the repair task: " + currentTask);

        isPerformingTask = false;
        currentTask = Task.Idle; // Reset the task
        repairProgress = 0f; // Reset repair progress

        // Keep crew member selectable after task completion
        HighlightSelection(false);
    }

    public void Die()
    {
        Debug.Log(crewName + " has died.");
        Destroy(gameObject);
    }
}
