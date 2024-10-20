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

    public bool isDead = false; // Track if the crew member is dead
    private bool isSacrificed = false; // Track if crew member was sacrificed

    public enum Task { Idle, RepairEngines, RepairLifeSupport, RepairHull, RepairGenerator, Wander, Sacrificed, Dead } // Added Dead task
    public Task currentTask = Task.Idle;

    private NavMeshAgent navAgent;
    private bool isPerformingTask = false;
    private bool isInsideRepairZone = false;
    private CubeInteraction currentCubeInteraction; // Reference to the system being repaired

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

    // Reference to ShipController for sacrifice mechanics
    private ShipController shipController;

    // Rigidbody reference for first-person control mode
    private Rigidbody rb;

    // Amount of biomass fuel added when sacrificed
    public float biomassFuelAmount = 10f; // Set to a default value or adjust as necessary

    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();

        // Initialize navAgent speed and acceleration
        navAgent.speed = normalSpeed;
        navAgent.acceleration = normalAcceleration;

        // Find the ShipController in the scene
        shipController = FindObjectOfType<ShipController>();

        // Randomize initial wait time counter
        waitTimeCounter = Random.Range(0f, maxWaitTime);

        // Get the Rigidbody component
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (isDead || isSacrificed) return; // Skip update logic if dead or sacrificed

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

        if (isInsideRepairZone && !isPerformingTask)
        {
            PerformRepairTask();
        }

        // Check for death
        if (health <= 0)
        {
            Die();
        }

        // Press "D" to simulate death for testing purposes
        if (Input.GetKeyDown(KeyCode.K))
        {
            Die();
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

        // Lower crew member to the ground and rotate them to simulate falling over
        transform.position += new Vector3(0, -1f, 0);  // Lower the crew member by 2 units on Y-axis
        transform.Rotate(new Vector3(-90, 0, 0));  // Rotate to simulate falling on their side

        // Set Rigidbody settings for staying on the ground
        rb.isKinematic = false;
        rb.useGravity = true;

        // Make sure they stay on the ground without sliding
        rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;

        // Remove any code that destroys the object
        // (We do not want the crew member to disappear over time)
    }



    public void Sacrifice()
    {
        if (isSacrificed || isDead) return; // Skip if already sacrificed or dead

        isSacrificed = true;
        currentTask = Task.Sacrificed;

        // Add biomass fuel to the engine system directly
        shipController.AddBiomassFuel(biomassFuelAmount);

        // Animate or display visual feedback if needed
        navAgent.isStopped = true;

        Destroy(gameObject, 3f); // Destroy the crew member after sacrifice
    }

    // Trigger for the repair zone
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Generator") && !isSacrificed)
        {
            Sacrifice();
        }

        if (other.CompareTag("RepairZone"))
        {
            isInsideRepairZone = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("RepairZone"))
        {
            isInsideRepairZone = false;
        }
    }

    public void PerformRepairTask()
    {
        if (currentCubeInteraction != null && !isPerformingTask)
        {
            isPerformingTask = true;
            navAgent.isStopped = true;

            // Calculate the death chance based on system health
            float systemHealth = currentCubeInteraction.GetSystemHealth(); // Assume this method exists
            float deathChance = Mathf.Clamp(1 - (systemHealth / 100f), 0f, 0.5f); // e.g., at 50% system health, there's a 50% death chance

            // Check if the crew member dies during the task
            if (Random.value < deathChance)
            {
                Die();
            }
            else
            {
                // Continue the repair task
                currentCubeInteraction.StartRepair(this, taskEfficiency);
            }
        }
    }

    public void CompleteTask()
    {
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

    public void Select()
    {
        HighlightSelection(true);
        if (selectedSFX != null) selectedSFX.Play();
    }

    public void Deselect()
    {
        HighlightSelection(false);
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
}
