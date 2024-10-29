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
    public float wanderRadius = 10f; // Radius within which the crewmember can wander
    public float minWaitTime = 0f;   // Minimum time to wait before moving again
    public float maxWaitTime = 2f;   // Maximum time to wait before moving again
    private float waitTimeCounter = 0f;

    public float normalSpeed = 3.5f;
    public float panicSpeed = 6f;
    public float normalAcceleration = 8f;
    public float panicAcceleration = 12f;

    private ShipController shipController;
    private Rigidbody rb;
    public float repairStartThreshold = 1.0f;

    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        if (navAgent != null)
        {
            navAgent.speed = normalSpeed;
            navAgent.acceleration = normalAcceleration;
            navAgent.stoppingDistance = repairStartThreshold;
        }

        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        else
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        crewAnim = GetComponent<Animator>();
        if (crewAnim != null)
        {
            crewAnim.applyRootMotion = false;
        }

        if (crewSelectedDot != null)
        {
            crewSelectedDot.gameObject.SetActive(false);
        }

        shipController = FindObjectOfType<ShipController>();
    }

    void Update()
    {
        if (isDead) return;

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

        if (currentTask != Task.Idle && !isPerformingTask)
        {
            MoveTowardsRepairPoint();
        }
        else if (currentTask == Task.Idle && !isPerformingTask)
        {
            Wander();
        }

        if (currentTask != Task.Idle && currentRepairPoint != null && !isPerformingTask)
        {
            if (!navAgent.pathPending && navAgent.remainingDistance <= navAgent.stoppingDistance && navAgent.pathStatus == NavMeshPathStatus.PathComplete)
            {
                StartRepair();
            }
        }

        if (crewAnim != null)
        {
            crewAnim.SetBool("isDead", isDead);
            crewAnim.SetBool("isFixing", isPerformingTask);

            if (navAgent != null)
            {
                float speed = navAgent.velocity.magnitude;
                crewAnim.SetFloat("Speed", speed);
            }
            else
            {
                crewAnim.SetFloat("Speed", 0f);
            }
        }

        if (health <= 0)
        {
            Die();
        }

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

    public void AssignToRepairPoint(CubeInteraction cubeInteraction)
    {
        if (currentCubeInteraction != null) return;

        if (navAgent != null && navAgent.hasPath)
        {
            navAgent.ResetPath();
        }

        currentCubeInteraction = cubeInteraction;
        currentRepairPoint = cubeInteraction.repairPoint;
        currentTask = DetermineTaskType(cubeInteraction.systemType);

        if (navAgent != null && currentRepairPoint != null)
        {
            navAgent.SetDestination(currentRepairPoint.position);
            navAgent.stoppingDistance = repairStartThreshold;
            navAgent.isStopped = false;
            if (walkingSFX != null && !walkingSFX.isPlaying)
            {
                walkingSFX.Play();
            }

            if (crewRenderer != null)
            {
                crewRenderer.material.color = Color.yellow;
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

        currentCubeInteraction.StartRepair(this, efficiency);
    }

    public void CompleteTask()
    {
        isPerformingTask = false;
        currentTask = Task.Idle;
        navAgent.isStopped = false;
        currentCubeInteraction = null;
        currentRepairPoint = null;

        if (crewRenderer != null)
        {
            crewRenderer.material.color = Color.white;
        }

        if (crewAnim != null)
        {
            crewAnim.SetBool("isFixing", false);
        }
    }

    void Wander()
    {
        if (navAgent == null) return;

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
                    if (walkingSFX != null && !walkingSFX.isPlaying)
                    {
                        walkingSFX.Play();
                    }

                    if (crewRenderer != null)
                    {
                        crewRenderer.material.color = Color.white;
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
            rb.isKinematic = true;
            rb.useGravity = false;
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
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }

    public void Rest()
    {
        currentTask = Task.Idle;
        fatigue = Mathf.Clamp(fatigue - (Time.deltaTime * 10f), 0, 100);
    }

    public void Select()
    {
        if (crewSelectedDot != null)
        {
            crewSelectedDot.gameObject.SetActive(true);
        }
    }

    public void Deselect()
    {
        if (crewSelectedDot != null)
        {
            crewSelectedDot.gameObject.SetActive(false);
        }
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;
        currentTask = Task.Dead;
        if (navAgent != null)
        {
            navAgent.isStopped = true;
            navAgent.enabled = false;
        }

        if (deathSFX != null)
        {
            deathSFX.Play();
        }

        if (crewAnim != null)
        {
            crewAnim.SetBool("isDead", true);
        }
        Deselect();

        if (shipController != null)
        {
            shipController.SacrificeCrew(1);
        }

        Destroy(gameObject, 5f);
    }

    private void MoveTowardsRepairPoint()
    {
        if (navAgent == null || currentRepairPoint == null) return;

        if (navAgent.isStopped)
        {
            navAgent.isStopped = false;
        }

        if (!navAgent.hasPath || navAgent.remainingDistance <= navAgent.stoppingDistance)
        {
            navAgent.SetDestination(currentRepairPoint.position);
            crewAnim?.SetFloat("Speed", navAgent.velocity.magnitude);

            if (walkingSFX != null && !walkingSFX.isPlaying)
            {
                walkingSFX.Play();
            }
        }
    }
}
