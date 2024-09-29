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
    private float repairDuration = 5f; // Duration to repair (adjustable based on damage)

    public Renderer crewRenderer; // Reference to the crew member's Renderer to indicate selection

    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        if (crewRenderer == null)
        {
            crewRenderer = GetComponent<Renderer>();
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
        if (isHighlighted)
        {
            crewRenderer.material.color = Color.yellow; // Example of highlight color
        }
        else
        {
            crewRenderer.material.color = Color.white; // Reset color when deselected
        }
    }

    public void AssignToTask(Task newTask, Vector3 destination, float damage)
    {
        currentTask = newTask;
        isPerformingTask = false; // Reset the task performance flag
        repairProgress = 0f; // Reset repair progress

        // Calculate repair duration based on system damage (more damage = longer repair)
        repairDuration = 5f + (damage * 5f); // Damage level between 0-1, repair time will range between 5-10 seconds

        navAgent.SetDestination(destination); // Move to the destination
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

        // Simulate repair task over time
        repairProgress += Time.deltaTime;

        // Update progress bar via SystemPanelManager
        shipController.systemPanelManager.UpdateRepairProgress(repairProgress);

        if (repairProgress >= repairDuration) // Repair time based on system damage
        {
            CompleteTask();
        }
    }


    void CompleteTask()
    {
        Debug.Log(crewName + " has completed the repair task: " + currentTask);
        isPerformingTask = false;
        currentTask = Task.Idle; // Reset the task
        repairProgress = 0f; // Reset repair progress
    }

    void Die()
    {
        Debug.Log(crewName + " has died.");
        Destroy(gameObject);
    }
}
