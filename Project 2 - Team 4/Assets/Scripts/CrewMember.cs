using UnityEngine;
using UnityEngine.AI;

public class CrewMember : MonoBehaviour
{
    public enum CrewType { Worker, Engineer }
    public CrewType crewType;

    public string crewName;
    public float morale = 100f; // Crew morale
    public float taskEfficiency = 1f; // Task efficiency affects repair speed
    public float health = 100f; // Crew health
    public float fatigue = 0f; // Fatigue level

    public enum Task { Idle, RepairEngines, RepairLifeSupport, RepairHull, RepairGenerator, Wander }
    public Task currentTask = Task.Idle;

    private NavMeshAgent navAgent;
    private bool isPerformingTask = false;
    private bool isInsideRepairZone = false; // Added flag for checking if crew member is inside repair zone
    private CubeInteraction currentCubeInteraction; // Reference to the system being repaired

    public Renderer crewRenderer;
    public Animator crewAnimator;
    public AudioSource walkingSFX, selectedSFX, assignedSFX;

    // Variables for wandering behavior
    public float wanderRadius = 10f; // Radius within which the crewmember can wander
    public float minWaitTime = 0f;   // Minimum time to wait before moving again
    public float maxWaitTime = 2f;   // Maximum time to wait before moving again
    private float waitTimeCounter = 0f;

    // Variables for speed adjustment
    public float normalSpeed = 3.5f;      // Normal movement speed
    public float panicSpeed = 6f;         // Speed when panicking
    public float normalAcceleration = 8f; // Normal acceleration
    public float panicAcceleration = 12f; // Acceleration when panicking

    // Reference to ShipController to check ship status
    private ShipController shipController;

    // Rigidbody reference for first-person control mode
    private Rigidbody rb;

    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();

        // Initialize navAgent speed and acceleration
        navAgent.speed = normalSpeed;
        navAgent.acceleration = normalAcceleration;

        // Find the ShipController in the scene
        shipController = FindObjectOfType<ShipController>();
        if (shipController == null)
        {
            Debug.LogError("ShipController not found in the scene. Please ensure a ShipController script is present.");
        }

        // Randomize initial wait time counter to prevent synchronization
        waitTimeCounter = Random.Range(0f, maxWaitTime);

        // Get the Rigidbody component
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Check if the ship is in critical condition
        if (shipController != null && shipController.IsCriticalCondition())
        {
            // Increase speed and acceleration
            navAgent.speed = panicSpeed;
            navAgent.acceleration = panicAcceleration;

            // Optional: Trigger panic animations or behaviors
            if (crewAnimator != null)
            {
                crewAnimator.SetBool("isPanicking", true);
            }
        }
        else
        {
            // Reset to normal speed and acceleration
            navAgent.speed = normalSpeed;
            navAgent.acceleration = normalAcceleration;

            // Reset panic animations
            if (crewAnimator != null)
            {
                crewAnimator.SetBool("isPanicking", false);
            }
        }

        // If performing a task, increase fatigue over time
        if (isPerformingTask)
        {
            fatigue += Time.deltaTime * 5f;
            fatigue = Mathf.Clamp(fatigue, 0, 100);
        }
        else
        {
            // If not performing a task, decrease fatigue over time (resting)
            fatigue -= Time.deltaTime * 2f;
            fatigue = Mathf.Clamp(fatigue, 0, 100);

            // If idle, start wandering
            if (currentTask == Task.Idle && !isPerformingTask)
            {
                Wander();
            }
        }

        // If inside the repair zone and not performing a task, start the repair
        if (isInsideRepairZone && !isPerformingTask)
        {
            PerformRepairTask();
        }
    }

    public void Select()
    {
        Debug.Log(crewName + " selected.");
        HighlightSelection(true);
        if (selectedSFX != null) selectedSFX.Play();
    }

    public void Deselect()
    {
        HighlightSelection(false);
        Debug.Log(crewName + " deselected.");
    }

    public void AssignToSystem(CubeInteraction system)
    {
        currentCubeInteraction = system;

        switch (system.systemType)
        {
            case CubeInteraction.SystemType.Engines:
                currentTask = Task.RepairEngines;
                break;
            case CubeInteraction.SystemType.LifeSupport:
                currentTask = Task.RepairLifeSupport;
                break;
            case CubeInteraction.SystemType.Hull:
                currentTask = Task.RepairHull;
                break;
            case CubeInteraction.SystemType.Generator:
                currentTask = Task.RepairGenerator;
                break;
        }

        if (navAgent != null)
        {
            navAgent.SetDestination(system.repairZone.transform.position);
            navAgent.isStopped = false;
            if (walkingSFX != null) walkingSFX.Play();
        }

        Deselect();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("RepairZone"))
        {
            Debug.Log($"{crewName} entered the repair zone.");
            isInsideRepairZone = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Check if the crew member is exiting the repair zone
        if (other.CompareTag("RepairZone") && currentCubeInteraction != null)
        {
            isInsideRepairZone = false;
            Debug.Log($"{crewName} exited the repair zone for {currentCubeInteraction.systemType}");
        }
    }

    void PerformRepairTask()
    {
        if (currentCubeInteraction != null && !isPerformingTask)
        {
            isPerformingTask = true;
            navAgent.isStopped = true;
            if (walkingSFX != null) walkingSFX.Stop();

            Debug.Log($"{crewName} is repairing {currentCubeInteraction.systemType}");
            currentCubeInteraction.StartRepair(this, taskEfficiency);
        }
    }

    public void CompleteTask()
    {
        Debug.Log($"{crewName} has completed the task: {currentTask}");
        isPerformingTask = false;
        currentTask = Task.Idle;
        navAgent.isStopped = false;
    }

    void HighlightSelection(bool isHighlighted)
    {
        if (crewRenderer != null)
        {
            crewRenderer.material.color = isHighlighted ? Color.yellow : Color.white;
        }
    }

    public void Rest()
    {
        if (!isPerformingTask)
        {
            fatigue = Mathf.Clamp(fatigue - (Time.deltaTime * 5f), 0, 100);
        }
    }

    public void AdjustMorale(float amount)
    {
        morale += amount;
        morale = Mathf.Clamp(morale, 0f, 100f);
        Debug.Log($"{crewName}'s morale adjusted by {amount}. Current morale: {morale}%");
    }

    void Wander()
    {
        if (!navAgent.pathPending && navAgent.remainingDistance <= navAgent.stoppingDistance)
        {
            // Crewmate has reached destination
            if (waitTimeCounter <= 0f)
            {
                // Randomize wait time to prevent synchronization
                waitTimeCounter = Random.Range(minWaitTime, maxWaitTime);

                // Optionally, you can play an idle animation here
                if (crewAnimator != null)
                {
                    crewAnimator.SetBool("isWalking", false);
                }
            }
            else
            {
                waitTimeCounter -= Time.deltaTime;
                if (waitTimeCounter <= 0f)
                {
                    // Time to move again
                    Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, NavMesh.AllAreas);
                    navAgent.SetDestination(newPos);
                    navAgent.isStopped = false;

                    if (walkingSFX != null && !walkingSFX.isPlaying)
                    {
                        walkingSFX.Play();
                    }

                    // Set walking animation
                    if (crewAnimator != null)
                    {
                        crewAnimator.SetBool("isWalking", true);
                    }
                }
            }
        }
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layerMask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;

        randDirection += origin;

        NavMeshHit navHit;
        NavMesh.SamplePosition(randDirection, out navHit, dist, layerMask);

        return navHit.position;
    }

    // Methods to enable and disable AI control
    public void EnableAI()
    {
        if (navAgent != null)
        {
            navAgent.enabled = true;
        }
        this.enabled = true; // Enable the CrewMember script

        // Enable Rigidbody
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }

    public void DisableAI()
    {
        if (navAgent != null)
        {
            navAgent.enabled = false;
        }
        this.enabled = false; // Disable the CrewMember script

        // Disable Rigidbody
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }
}
