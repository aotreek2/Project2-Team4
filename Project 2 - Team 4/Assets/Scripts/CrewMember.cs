// CrewMember.cs
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

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

    private Animator crewAnim;
    public Image crewSelectedDot;
    private NavMeshAgent navAgent;
    private bool isPerformingTask = false;
    private CubeInteraction currentCubeInteraction; // Reference to the system being repaired
    private Transform currentRepairPoint; // Current target repair point

    public Renderer crewRenderer;
    public AudioSource walkingSFX, selectedSFX, assignedSFX, deathSFX;

    public Transform fpsCameraPos;
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
    public float repairStartThreshold = 1.0f; // Increased from 0.5f for better reliability

    void Start()
    {
        // Initialize NavMeshAgent
        navAgent = GetComponent<NavMeshAgent>();
        if (navAgent == null)
        {
            Debug.LogError($"[Start] NavMeshAgent component missing from crew member: {crewName}");
        }
        else
        {
            navAgent.speed = normalSpeed;
            navAgent.acceleration = normalAcceleration;
            navAgent.stoppingDistance = repairStartThreshold; // Set the stopping distance
            Debug.Log($"[Start] NavMeshAgent initialized for {crewName} with speed {navAgent.speed} and stopping distance {navAgent.stoppingDistance}");
        }

        // Initialize Rigidbody
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            Debug.LogWarning($"[Start] Rigidbody was missing and has been added as kinematic for {crewName}");
        }
        else
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            Debug.Log($"[Start] Rigidbody found for {crewName} set to kinematic and gravity disabled");
        }

        crewAnim = GetComponent<Animator>();
        if (crewAnim == null)
        {
            Debug.LogError($"[Start] Animator component missing from crew member: {crewName}");
        }
        else
        {
            // Ensure Root Motion is disabled to prevent conflicts with NavMeshAgent
            crewAnim.applyRootMotion = false;
            Debug.Log($"[Start] Animator initialized for {crewName} with Root Motion disabled");
        }

        if (crewSelectedDot != null)
        {
            crewSelectedDot.gameObject.SetActive(false);
        }

        // Find ShipController in the scene
        shipController = FindObjectOfType<ShipController>();
        if (shipController == null)
        {
            Debug.LogError("[Start] ShipController not found in the scene.");
        }

        // Initialize AudioSources
        if (walkingSFX == null || selectedSFX == null || assignedSFX == null || deathSFX == null)
        {
            Debug.LogWarning($"[Start] One or more AudioSources are not assigned on {crewName}");
        }

        // Initialize Renderer
        if (crewRenderer == null)
        {
            crewRenderer = GetComponent<Renderer>();
            if (crewRenderer == null)
            {
                Debug.LogError($"[Start] Renderer component missing from crew member: {crewName}");
            }
            else
            {
                Debug.Log($"[Start] Renderer initialized for {crewName}");
            }
        }
    }

    void Update()
    {
        if (isDead) return; // Skip update logic if dead

        // Check for ship's critical condition and adjust behavior
        if (shipController != null && shipController.IsCriticalCondition())
        {
            if (navAgent.speed != panicSpeed || navAgent.acceleration != panicAcceleration)
            {
                navAgent.speed = panicSpeed;
                navAgent.acceleration = panicAcceleration;
                Debug.Log($"[Update] {crewName} speed and acceleration set to panic values due to critical condition");
            }
        }
        else
        {
            if (navAgent.speed != normalSpeed || navAgent.acceleration != normalAcceleration)
            {
                navAgent.speed = normalSpeed;
                navAgent.acceleration = normalAcceleration;
                Debug.Log($"[Update] {crewName} speed and acceleration set to normal values");
            }
        }

        // Fatigue logic
        if (isPerformingTask)
        {
            fatigue += Time.deltaTime * 5f;
            fatigue = Mathf.Clamp(fatigue, 0, 100);
        }
        else
        {
            fatigue -= Time.deltaTime * 2f;
            fatigue = Mathf.Clamp(fatigue, 0, 100);
        }

        // Movement logic
        if (currentTask != Task.Idle && !isPerformingTask)
        {
            MoveTowardsRepairPoint();
        }
        else if (currentTask == Task.Idle && !isPerformingTask)
        {
            Wander();
        }

        // Check if the crew member has reached the repair point to start repair
        if (currentTask != Task.Idle && currentRepairPoint != null && !isPerformingTask)
        {
            // Added additional check for path status
            if (!navAgent.pathPending && navAgent.remainingDistance <= navAgent.stoppingDistance && navAgent.pathStatus == NavMeshPathStatus.PathComplete)
            {
                Debug.Log($"[Update] {crewName} has reached the repair point and is starting repair");
                StartRepair();
            }
        }

        // Update Animator Parameters
        if (crewAnim != null)
        {
            crewAnim.SetBool("isDead", isDead);
            crewAnim.SetBool("isFixing", isPerformingTask);

            // Set Speed for blending between Idle, Walking, and Running
            if (navAgent != null)
            {
                float speed = navAgent.velocity.magnitude;
                crewAnim.SetFloat("Speed", speed);
                // Optionally, set 'isMoving' based on speed
                // crewAnim.SetBool("isMoving", speed > 0.1f);
            }
            else
            {
                crewAnim.SetFloat("Speed", 0f);
                // crewAnim.SetBool("isMoving", false);
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

    private void LateUpdate()
    {
        if (crewSelectedDot != null && Camera.main != null)
        {
            crewSelectedDot.gameObject.transform.LookAt(
                Camera.main.transform.position + Camera.main.transform.rotation * Vector3.forward,
                Camera.main.transform.rotation * Vector3.up
            );
        }
    }

    // Assign the crew member to a repair system
    public void AssignToRepairPoint(CubeInteraction cubeInteraction)
    {
        if (currentCubeInteraction != null)
        {
            Debug.LogWarning($"[AssignToRepairPoint] Crew member {crewName} is already assigned to a repair task.");
            return;
        }

        // Stop wandering
        if (navAgent != null && navAgent.hasPath)
        {
            navAgent.ResetPath();
            Debug.Log($"[AssignToRepairPoint] {crewName}'s current path has been reset.");
        }

        currentCubeInteraction = cubeInteraction;
        currentRepairPoint = cubeInteraction.repairPoint;

        currentTask = DetermineTaskType(cubeInteraction.systemType);
        Debug.Log($"[AssignToRepairPoint] {crewName} assigned to task {currentTask}");

        if (navAgent != null && currentRepairPoint != null)
        {
            bool destinationSet = navAgent.SetDestination(currentRepairPoint.position);
            if (destinationSet)
            {
                navAgent.stoppingDistance = repairStartThreshold;
                // Optionally, set 'isMoving' to true
                // crewAnim?.SetBool("isMoving", true);
                navAgent.isStopped = false;
                Debug.Log($"[AssignToRepairPoint] {crewName} is moving towards {currentRepairPoint.position}");

                if (walkingSFX != null && !walkingSFX.isPlaying)
                {
                    walkingSFX.Play();
                    Debug.Log($"[AssignToRepairPoint] Walking sound played for {crewName}");
                }

                // Change the crew member's color to indicate assignment
                if (crewRenderer != null)
                {
                    crewRenderer.material.color = Color.yellow;
                    Debug.Log($"[AssignToRepairPoint] {crewName}'s color changed to yellow");
                }

                // Optional: Add a debug log to confirm assignment
                Debug.Log($"[AssignToRepairPoint] Crew member {crewName} assigned to system: {cubeInteraction.systemType}");
            }
            else
            {
                Debug.LogError($"[AssignToRepairPoint] Crew member {crewName} failed to set destination to {currentRepairPoint.position}. Ensure the repair point is on the NavMesh.");
                // Optionally, reset the task to Idle if destination setting fails
                currentTask = Task.Idle;
                currentCubeInteraction = null;
                currentRepairPoint = null;
            }
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
        crewAnim?.SetBool("isFixing", true);
        navAgent.isStopped = true;
        Debug.Log($"[StartRepair] {crewName} started repairing {currentTask}");

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
            Debug.Log($"[CompleteTask] {crewName}'s color reset to white");
        }

        // Update Animator Parameters
        if (crewAnim != null)
        {
            crewAnim.SetBool("isFixing", false);
            // crewAnim.SetBool("isMoving", false); // Optional, based on movement
            // crewAnim.SetFloat("Speed", 0f); // Optionally reset speed
        }

        Debug.Log($"[CompleteTask] {crewName} has completed the task and is now idle");
    }

    void Wander()
    {
        if (navAgent == null)
        {
            Debug.LogWarning($"[Wander] NavMeshAgent is null for {crewName}");
            return;
        }

        if (!navAgent.pathPending && navAgent.remainingDistance <= navAgent.stoppingDistance)
        {
            if (waitTimeCounter <= 0f)
            {
                waitTimeCounter = Random.Range(minWaitTime, maxWaitTime);
                Debug.Log($"[Wander] {crewName} is waiting for {waitTimeCounter} seconds");
            }
            else
            {
                waitTimeCounter -= Time.deltaTime;
                if (waitTimeCounter <= 0f)
                {
                    Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, NavMesh.AllAreas);
                    bool destinationSet = navAgent.SetDestination(newPos);
                    if (destinationSet)
                    {
                        // Set 'Speed' to walking speed
                        crewAnim?.SetFloat("Speed", navAgent.velocity.magnitude);
                        Debug.Log($"[Wander] {crewName} is wandering to {newPos}");

                        if (walkingSFX != null && !walkingSFX.isPlaying)
                        {
                            walkingSFX.Play();
                            Debug.Log($"[Wander] Walking sound played for {crewName}");
                        }

                        // Change the crew member's color to indicate wandering
                        if (crewRenderer != null)
                        {
                            crewRenderer.material.color = Color.white;
                            Debug.Log($"[Wander] {crewName}'s color set to white for wandering");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[Wander] {crewName} failed to set wander destination to {newPos}. Ensure the position is on the NavMesh.");
                        // Optionally, reset the wait counter to attempt wandering again
                        waitTimeCounter = Random.Range(minWaitTime, maxWaitTime);
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
        Debug.Log($"[AdjustMorale] {crewName} morale adjusted by {amount}. Current morale: {morale}");
    }

    public void EnableAI()
    {
        if (navAgent != null)
        {
            navAgent.enabled = true;
            Debug.Log($"[EnableAI] NavMeshAgent enabled for {crewName}");
        }
        this.enabled = true;

        if (rb != null)
        {
            rb.isKinematic = true; // Ensure Rigidbody remains kinematic
            rb.useGravity = false;
            Debug.Log($"[EnableAI] Rigidbody set to kinematic and gravity disabled for {crewName}");
        }
    }

    public void DisableAI()
    {
        if (navAgent != null)
        {
            navAgent.enabled = false;
            Debug.Log($"[DisableAI] NavMeshAgent disabled for {crewName}");
        }
        this.enabled = false;

        if (rb != null)
        {
            rb.isKinematic = false; // Allow physics if AI is disabled
            rb.useGravity = true;
            Debug.Log($"[DisableAI] Rigidbody set to non-kinematic and gravity enabled for {crewName}");
        }
    }

    public void Rest()
    {
        currentTask = Task.Idle;
        fatigue = Mathf.Clamp(fatigue - (Time.deltaTime * 10f), 0, 100);
        Debug.Log($"[Rest] {crewName} is resting. Fatigue level: {fatigue}");
    }

    public void Select()
    {
        // Highlight the crew member when selected
        if (crewSelectedDot != null)
        {
            crewSelectedDot.gameObject.SetActive(true);
            Debug.Log($"[Select] {crewName} has been selected");
        }
    }

    public void Deselect()
    {
        // Reset the crew member's appearance when deselected
        if (crewSelectedDot != null)
        {
            crewSelectedDot.gameObject.SetActive(false);
            Debug.Log($"[Deselect] {crewName} has been deselected");
        }
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;
        currentTask = Task.Dead;  // Set task to Dead state
        if (navAgent != null)
        {
            navAgent.isStopped = true;  // Stop all movement
            navAgent.enabled = false;   // Disable the NavMeshAgent to prevent sliding
            Debug.Log($"[Die] NavMeshAgent disabled for {crewName}");
        }

        // Play death sound if available
        if (deathSFX != null)
        {
            deathSFX.Play();
            Debug.Log($"[Die] Death sound played for {crewName}");
        }

        // Play a death animation if available
        if (crewAnim != null)
        {
            crewAnim.SetBool("isDead", true);
            Debug.Log($"[Die] Death animation triggered for {crewName}");
        }
        Deselect();

        // Notify ShipController about the death
        if (shipController != null)
        {
            shipController.SacrificeCrew(1); // Adjust the number as needed
        }

        // Optionally, destroy the crew member after a delay
        Destroy(gameObject, 5f); // Adjust the delay as needed
        Debug.Log($"[Die] {crewName} will be destroyed in 5 seconds");
    }

    private void MoveTowardsRepairPoint()
    {
        if (navAgent == null || currentRepairPoint == null)
        {
            Debug.LogWarning($"[MoveTowardsRepairPoint] {crewName} cannot move towards repair point because NavMeshAgent or repair point is null.");
            return;
        }

        // Ensure the agent is active
        if (navAgent.isStopped)
        {
            navAgent.isStopped = false;
            Debug.Log($"[MoveTowardsRepairPoint] NavMeshAgent resumed for {crewName}");
        }

        // Set destination if not already set or if the path is invalid
        if (!navAgent.hasPath || navAgent.remainingDistance <= navAgent.stoppingDistance)
        {
            bool destinationSet = navAgent.SetDestination(currentRepairPoint.position);
            if (destinationSet)
            {
                crewAnim?.SetFloat("Speed", navAgent.velocity.magnitude);
                Debug.Log($"[MoveTowardsRepairPoint] {crewName} is moving towards {currentRepairPoint.position}");

                // Play walking sound if not already playing
                if (walkingSFX != null && !walkingSFX.isPlaying)
                {
                    walkingSFX.Play();
                    Debug.Log($"[MoveTowardsRepairPoint] Walking sound played for {crewName}");
                }
            }
            else
            {
                Debug.LogWarning($"[MoveTowardsRepairPoint] {crewName} failed to set destination to {currentRepairPoint.position}. Ensure the repair point is on the NavMesh.");
            }
        }
    }
}
