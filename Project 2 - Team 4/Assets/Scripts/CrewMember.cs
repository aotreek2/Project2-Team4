using UnityEngine;
using UnityEngine.AI;

public class CrewMember : MonoBehaviour
{
    public enum CrewType { Worker, Engineer }
    public CrewType crewType;

    public float morale = 100f; // Ranges from 0 to 100

    public string crewName;
    public float taskEfficiency = 1f; // Task efficiency affects repair speed
    public float health = 100f;       // Crew health
    public float fatigue = 0f; // Fatigue level, ranges from 0 to 100

    public enum Task { Idle, RepairEngines, RepairLifeSupport, RepairHull, Wander }
    public Task currentTask = Task.Idle;

    private NavMeshAgent navAgent;
    private bool isPerformingTask = false; // Flag to check if task is being performed
    public float repairDuration = 5f; // Duration to repair (adjustable based on damage)

    public Renderer crewRenderer; // Reference to the crew member's Renderer to indicate selection

    public Animator crewAnimator;

    public AudioSource walkingSFX, selectedSFX, assignedSFX;

    // References
    private ShipController shipController;
    private CubeInteraction currentCubeInteraction; // Reference to the cube (system part)
    private Vector3 systemDestination; // Destination of the system to repair

    // Wandering variables
    public float wanderRadius = 10f; // Radius within which the crew can wander
    public float wanderTimer = 5f; // Time to wait before picking a new destination
    private float wanderTimerCounter;

    void Start()
    {
        // Ensure the NavMeshAgent is assigned
        navAgent = GetComponent<NavMeshAgent>();
        if (navAgent == null)
        {
            Debug.LogError("NavMeshAgent not found on " + gameObject.name);
        }

        // Find ShipController if not assigned
        shipController = FindObjectOfType<ShipController>();
        crewAnimator = GetComponent<Animator>();

        // Initialize wander timer
        wanderTimerCounter = wanderTimer;
    }

    void Update()
    {
        if (isPerformingTask)
        {
            // Increase fatigue over time if performing a task
            IncreaseFatigue(Time.deltaTime * 5f); // Adjust rate as needed
        }
        else
        {
            // If not performing a task, handle wandering
            if (currentTask == Task.Idle || currentTask == Task.Wander)
            {
                Wander();
            }
        }
    }

    void Wander()
    {
        currentTask = Task.Wander;

        wanderTimerCounter += Time.deltaTime;

        if (wanderTimerCounter >= wanderTimer)
        {
            Vector3 newDestination = RandomNavSphere(transform.position, wanderRadius, -1);

            if (navAgent != null)
            {
                navAgent.SetDestination(newDestination);
                navAgent.isStopped = false;
                if (walkingSFX != null)
                {
                    walkingSFX.Play();
                }
            }

            wanderTimerCounter = 0;
        }

        // Check if the crew member has reached the destination
        if (!navAgent.pathPending && navAgent.remainingDistance <= navAgent.stoppingDistance)
        {
            navAgent.isStopped = true;
            navAgent.ResetPath();
            if (walkingSFX != null)
            {
                walkingSFX.Stop();
            }
        }
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float distance, int layermask)
    {
        Vector3 randomDirection = Random.insideUnitSphere * distance;
        randomDirection += origin;

        NavMeshHit navHit;
        if (NavMesh.SamplePosition(randomDirection, out navHit, distance, layermask))
        {
            return navHit.position;
        }
        else
        {
            return origin;
        }
    }

    public void Select()
    {
        HighlightSelection(true); // Highlight the crew member when selected
        if (selectedSFX != null)
        {
            selectedSFX.Play();
        }
        Debug.Log(crewName + " is selected.");
    }

    public void Deselect()
    {
        HighlightSelection(false); // Remove highlight when deselected
        Debug.Log(crewName + " is deselected.");
    }

    void HighlightSelection(bool isHighlighted)
    {
        if (crewRenderer != null)
        {
            crewRenderer.material.color = isHighlighted ? Color.yellow : Color.white;
        }
    }

    public void AssignToTask(Task newTask, Vector3 destination, ShipController controller, CubeInteraction cubeInteraction)
    {
        currentTask = newTask;
        isPerformingTask = false; // Reset the task performance flag
        shipController = controller;
        currentCubeInteraction = cubeInteraction;
        systemDestination = destination;

        if (navAgent != null)
        {
            if (assignedSFX != null)
            {
                assignedSFX.Play();
            }
            navAgent.isStopped = false; // Ensure the agent is not stopped
            navAgent.SetDestination(destination); // Move to the destination
            if (walkingSFX != null)
            {
                walkingSFX.Play();
            }
        }

        Deselect(); // Deselect the crew member to allow movement
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("System"))  // Ensure your system cubes have the "System" tag
        {
            EnterRepairZone();
        }
    }

    public void EnterRepairZone()
    {
        Debug.Log($"{crewName} entered repair zone for {currentCubeInteraction.systemType}.");
        PerformTask();
    }

    void PerformTask()
    {
        isPerformingTask = true;
        if (walkingSFX != null)
        {
            walkingSFX.Stop();
        }
        // Adjust task efficiency based on morale and fatigue
        float actualEfficiency = taskEfficiency * (morale / 100f) * ((100f - fatigue) / 100f);
        currentCubeInteraction.StartRepair(this, actualEfficiency);
    }

    public void CompleteTask()
    {
        Debug.Log(crewName + " has completed the repair task: " + currentTask);

        isPerformingTask = false;
        currentTask = Task.Idle; // Reset the task

        HighlightSelection(false);
    }

    public void IncreaseFatigue(float amount)
    {
        fatigue += amount;
        fatigue = Mathf.Clamp(fatigue, 0f, 100f);

        if (fatigue >= 100f)
        {
            Debug.Log($"{crewName} is exhausted and needs rest.");
            isPerformingTask = false;
            currentTask = Task.Idle;
        }
    }

    public void DecreaseFatigue(float amount)
    {
        fatigue -= amount;
        fatigue = Mathf.Clamp(fatigue, 0f, 100f);
    }

    public void Rest()
    {
        if (!isPerformingTask)
        {
            DecreaseFatigue(Time.deltaTime * 10f);
        }
    }

    public void AdjustMorale(float amount)
    {
        morale += amount;
        morale = Mathf.Clamp(morale, 0f, 100f);
        Debug.Log($"{crewName}'s morale adjusted by {amount}. Current morale: {morale}%");
    }

    public void Die()
    {
        Debug.Log(crewName + " has died.");
        Destroy(gameObject);
    }
}
