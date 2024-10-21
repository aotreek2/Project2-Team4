using UnityEngine;
using UnityEngine.AI;

public class CrewMember : MonoBehaviour
{
    public enum CrewType { Worker, Engineer }
    public CrewType crewType;

    public string crewName;
    public float morale = 100f; // Crew morale
    public float efficiency = 1f; // Task efficiency affects repair speed
    public float health = 100f; // Crew health
    public float fatigue = 0f; // Fatigue level

    public bool isDead = false; // Track if the crew member is dead

    public enum Task { Idle, RepairEngines, RepairLifeSupport, RepairHull, RepairGenerator, Wander, Dead }
    public Task currentTask = Task.Idle;

    private NavMeshAgent navAgent;
    private bool isPerformingTask = false;
    private CubeInteraction currentCubeInteraction; // Reference to the system being repaired
    private Transform currentRepairPoint; // Current target repair point

    public Renderer crewRenderer;
    public AudioSource walkingSFX, selectedSFX, assignedSFX, deathSFX;

    // Variables for wandering behavior
    public float wanderRadius = 10f; // Radius within which the crewmember can wander
    public float minWaitTime = 0f;   // Minimum time to wait before moving again
    public float maxWaitTime = 2f;   // Maximum time to wait before moving again
    private float waitTimeCounter = 0f;

    // Variables for speed adjustment
    public float normalSpeed = 3.5f;
    public float panicSpeed = 6f;
    public float normalAcceleration = 8f;
    public float panicAcceleration = 12f;

    // Reference to ShipController for any ship-wide mechanics
    private ShipController shipController;

    // Rigidbody reference for physics interactions
    private Rigidbody rb;

    // Threshold distance to start repair
    public float repairStartThreshold = 0.5f; // Adjust as needed

    void Start()
    {
        // Initialize NavMeshAgent
        navAgent = GetComponent<NavMeshAgent>();
        if (navAgent == null)
        {
            Debug.LogError("NavMeshAgent component missing from crew member: " + crewName);
        }
        else
        {
            navAgent.speed = normalSpeed;
            navAgent.acceleration = normalAcceleration;
        }

        // Initialize Rigidbody
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
        }

        // Find ShipController in the scene
        shipController = FindObjectOfType<ShipController>();
        if (shipController == null)
        {
            Debug.LogError("ShipController not found in the scene.");
        }

        // Initialize AudioSources
        if (walkingSFX == null || selectedSFX == null || assignedSFX == null || deathSFX == null)
        {
            Debug.LogWarning("One or more AudioSources are not assigned on " + crewName);
        }

        // Initialize Renderer
        if (crewRenderer == null)
        {
            crewRenderer = GetComponent<Renderer>();
            if (crewRenderer == null)
            {
                Debug.LogError("Renderer component missing from crew member: " + crewName);
            }
        }
    }
    void Update()
    {
        if (isDead) return; // Skip update logic if dead

        // Check for ship's critical condition and adjust behavior
        if (shipController != null && shipController.IsCriticalCondition())
        {
            navAgent.speed = panicSpeed;
            navAgent.acceleration = panicAcceleration;
        }
        else
        {
            navAgent.speed = normalSpeed;
            navAgent.acceleration = normalAcceleration;
        }

        // If performing a task, increase fatigue
        if (isPerformingTask)
        {
            fatigue += Time.deltaTime * 5f;
            fatigue = Mathf.Clamp(fatigue, 0, 100);
        }
        else
        {
            fatigue -= Time.deltaTime * 2f;
            fatigue = Mathf.Clamp(fatigue, 0, 100);
            if (currentTask == Task.Idle && !isPerformingTask)
            {
                Wander();
            }
        }

        // Check if the crew member has reached the repair point to start repair
        if (currentTask != Task.Idle && currentRepairPoint != null && !isPerformingTask)
        {
            float distance = Vector3.Distance(transform.position, currentRepairPoint.position);
            if (distance <= repairStartThreshold)
            {
                StartRepair();
            }
        }

        // Check for death
        if (health <= 0)
        {
            Die();
        }

        // Press "K" to simulate death for testing purposes
        if (Input.GetKeyDown(KeyCode.K))
        {
            Die();
        }
    }

    // Assign the crew member to a repair system
    public void AssignToRepairPoint(CubeInteraction cubeInteraction)
    {
        if (currentCubeInteraction != null)
        {
            Debug.LogWarning($"Crew member {crewName} is already assigned to a repair task.");
            return;
        }

        currentCubeInteraction = cubeInteraction;
        currentRepairPoint = cubeInteraction.repairPoint;

        currentTask = DetermineTaskType(cubeInteraction.systemType);

        if (navAgent != null && currentRepairPoint != null)
        {
            navAgent.SetDestination(currentRepairPoint.position);
            navAgent.isStopped = false;
            if (walkingSFX != null && !walkingSFX.isPlaying)
            {
                walkingSFX.Play();
            }

            // Change the crew member's color to indicate assignment
            if (crewRenderer != null)
            {
                crewRenderer.material.color = Color.yellow;
            }

            // Optional: Add a debug log to confirm assignment
            Debug.Log($"Crew member {crewName} assigned to system: {cubeInteraction.systemType}");
        }
    }

    private Task DetermineTaskType(CubeInteraction.SystemType systemType)
    {
        switch (systemType)
        {
            case CubeInteraction.SystemType.LifeSupport:
                return Task.RepairLifeSupport;
            case CubeInteraction.SystemType.Engines:
                return Task.RepairEngines;
            case CubeInteraction.SystemType.Hull:
                return Task.RepairHull;
            case CubeInteraction.SystemType.Generator:
                return Task.RepairGenerator;
            default:
                return Task.Idle;
        }
    }

    private void StartRepair()
    {
        if (isPerformingTask || currentCubeInteraction == null)
            return;

        isPerformingTask = true;
        navAgent.isStopped = true;

        // Start the repair process via CubeInteraction
        currentCubeInteraction.StartRepair(this, efficiency);
    }

    public void CompleteTask()
    {
        isPerformingTask = false;
        currentTask = Task.Idle;
        navAgent.isStopped = false;
        currentCubeInteraction = null;
        currentRepairPoint = null;

        // Reset the crew member's color after completing the task
        if (crewRenderer != null)
        {
            crewRenderer.material.color = Color.white;
        }
    }

    void Wander()
    {
        if (!navAgent.pathPending && navAgent.remainingDistance <= navAgent.stoppingDistance)
        {
            if (waitTimeCounter <= 0f)
            {
                waitTimeCounter = Random.Range(minWaitTime, maxWaitTime);
            }
            else
            {
                waitTimeCounter -= Time.deltaTime;
                if (waitTimeCounter <= 0f)
                {
                    Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, NavMesh.AllAreas);
                    navAgent.SetDestination(newPos);
                    navAgent.isStopped = false;
                    if (walkingSFX != null && !walkingSFX.isPlaying)
                    {
                        walkingSFX.Play();
                    }
                }
            }
        }
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layerMask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;
        NavMesh.SamplePosition(randDirection, out NavMeshHit navHit, dist, layerMask);
        return navHit.position;
    }

    public void AdjustMorale(float amount)
    {
        morale += amount;
        morale = Mathf.Clamp(morale, 0f, 100f);
    }

    public void EnableAI()
    {
        if (navAgent != null)
        {
            navAgent.enabled = true;
        }
        this.enabled = true;

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
        this.enabled = false;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    public void Rest()
    {
        currentTask = Task.Idle;
        fatigue = Mathf.Clamp(fatigue - (Time.deltaTime * 10f), 0, 100);
    }

    public void Select()
    {
        // Highlight the crew member when selected
        if (crewRenderer != null)
        {
            crewRenderer.material.color = Color.green;
        }
    }

    public void Deselect()
    {
        // Reset the crew member's appearance when deselected
        if (crewRenderer != null)
        {
            crewRenderer.material.color = Color.white;
        }
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;
        currentTask = Task.Dead;  // Set task to Dead state
        navAgent.isStopped = true;  // Stop all movement
        navAgent.enabled = false;   // Disable the NavMeshAgent to prevent sliding

        // Play death sound if available
        if (deathSFX != null) deathSFX.Play();

        // Play a death animation if available
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }
        Deselect();
        // Optionally, destroy the crew member after a delay
        Destroy(gameObject, 5f); // Adjust the delay as needed
    }
}
