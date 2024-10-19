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

    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        // If performing a task, increase fatigue over time
        if (isPerformingTask)
        {
            fatigue += Time.deltaTime * 5f;
            fatigue = Mathf.Clamp(fatigue, 0, 100);
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
            navAgent.SetDestination(system.transform.position);
            navAgent.isStopped = false;
            if (walkingSFX != null) walkingSFX.Play();
        }

        Deselect();
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.name + " entered a trigger.");
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
}
