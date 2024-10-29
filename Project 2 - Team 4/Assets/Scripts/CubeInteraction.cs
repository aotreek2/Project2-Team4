using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeInteraction : MonoBehaviour
{
    public enum SystemType { LifeSupport, Engines, Hull, Generator }
    public SystemType systemType;

    public RepairProgressBar repairProgressBar;
    public Transform repairPoint; // Assigned in the Inspector
    public float baseRepairDuration = 10f; // Base duration for repairs

    [Range(0f, 1f)]
    public float baseDeathChanceMultiplier = 0.05f; // Base death chance multiplier

    private List<CrewMember> assignedCrewMembers = new List<CrewMember>();
    private List<CrewMember> crewMembersInRepairZone = new List<CrewMember>();
    private bool isRepairing = false;
    private float repairProgress = 0f;
    private float repairDuration;

    private LifeSupportController lifeSupportController;
    private EngineSystemController engineSystemController;
    private GeneratorController generatorController;
    private HullSystemController hullSystemController;

    public DialogueManager dialogueManager;

    void Start()
    {
        switch (systemType)
        {
            case SystemType.LifeSupport:
                lifeSupportController = GetComponent<LifeSupportController>();
                if (lifeSupportController != null)
                {
                    lifeSupportController.DamageLifeSupport(50f);
                }
                break;
            case SystemType.Engines:
                engineSystemController = GetComponent<EngineSystemController>();
                if (engineSystemController != null)
                {
                    engineSystemController.DamageEngine(50f);
                }
                break;
            case SystemType.Generator:
                generatorController = GetComponent<GeneratorController>();
                if (generatorController != null)
                {
                    generatorController.DamageGenerator(50f);
                }
                break;
            case SystemType.Hull:
                hullSystemController = GetComponent<HullSystemController>();
                if (hullSystemController != null)
                {
                    hullSystemController.DamageHull(50f);
                }
                break;
        }

        if (repairProgressBar == null)
        {
            repairProgressBar = FindObjectOfType<RepairProgressBar>();
        }

        if (repairPoint == null)
        {
            Transform foundRepairPoint = transform.Find("RepairPoint");
            if (foundRepairPoint != null)
            {
                repairPoint = foundRepairPoint;
            }
        }

        if (repairProgressBar != null)
        {
            repairProgressBar.gameObject.SetActive(false);
        }

        GameObject repairZoneObj = transform.Find("RepairZone")?.gameObject;
        if (repairZoneObj != null)
        {
            Collider collider = repairZoneObj.GetComponent<Collider>();
            if (collider != null && !collider.isTrigger)
            {
                collider.isTrigger = true;
            }
        }

        if (dialogueManager == null)
        {
            dialogueManager = FindObjectOfType<DialogueManager>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        CrewMember crew = other.GetComponent<CrewMember>();
        if (crew != null && assignedCrewMembers.Contains(crew) && !crewMembersInRepairZone.Contains(crew))
        {
            crewMembersInRepairZone.Add(crew);

            if (!isRepairing)
            {
                StartRepair();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        CrewMember crew = other.GetComponent<CrewMember>();
        if (crew != null && crewMembersInRepairZone.Contains(crew))
        {
            crewMembersInRepairZone.Remove(crew);

            if (crewMembersInRepairZone.Count == 0)
            {
                StopRepair();
            }
        }
    }

    public void SetSelectedCrewMember(CrewMember crewMember)
    {
        if (crewMember == null)
        {
            return;
        }

        if (!assignedCrewMembers.Contains(crewMember))
        {
            assignedCrewMembers.Add(crewMember);
            crewMember.AssignToRepairPoint(this);

            if (dialogueManager != null)
            {
                string assignmentMessage = $"Crew member {crewMember.crewName} has been assigned to repair {systemType}.";
                string[] assignmentDialogue = new string[] { assignmentMessage };
                dialogueManager.StartDialogue(assignmentDialogue, systemType);
            }
        }
    }

    public void StartRepair(CrewMember crewMember, float efficiency)
    {
        SetSelectedCrewMember(crewMember);
    }

    public void RemoveCrewMember(CrewMember crewMember)
    {
        if (assignedCrewMembers.Contains(crewMember))
        {
            assignedCrewMembers.Remove(crewMember);

            if (crewMembersInRepairZone.Contains(crewMember))
            {
                crewMembersInRepairZone.Remove(crewMember);
            }

            if (assignedCrewMembers.Count == 0)
            {
                StopRepair();
            }
        }
    }

    private void StartRepair()
    {
        if (IsSystemFullyRepaired())
        {
            return;
        }

        if (!isRepairing && crewMembersInRepairZone.Count > 0)
        {
            isRepairing = true;
            repairProgress = 0f;

            float currentHealth = GetSystemHealth();
            float maxHealth = GetMaxSystemHealth();
            float damageProportion = (maxHealth - currentHealth) / maxHealth;
            repairDuration = baseRepairDuration * damageProportion / Mathf.Max(GetAverageEfficiency(), 0.1f);

            if (repairDuration <= 0f)
            {
                repairDuration = 0.1f;
            }

            if (repairProgressBar != null)
            {
                repairProgressBar.gameObject.SetActive(true);
                repairProgressBar.ResetProgress();
            }

            StartCoroutine(RepairSystem());
        }
    }

    private float GetAverageEfficiency()
    {
        if (crewMembersInRepairZone.Count == 0)
            return 1f;

        float totalEfficiency = 0f;
        foreach (var crew in crewMembersInRepairZone)
        {
            totalEfficiency += crew.efficiency;
        }

        return totalEfficiency / crewMembersInRepairZone.Count;
    }

    private IEnumerator RepairSystem()
    {
        float elapsedTime = 0f;

        while (elapsedTime < repairDuration)
        {
            if (crewMembersInRepairZone.Count == 0)
            {
                StopRepair();
                yield break;
            }

            elapsedTime += Time.deltaTime;
            repairProgress = Mathf.Clamp01(elapsedTime / repairDuration);

            if (repairProgressBar != null)
            {
                repairProgressBar.UpdateRepairProgress(repairProgress);
            }

            float currentHealth = GetSystemHealth();
            float deathChance = Mathf.Clamp((1 - (currentHealth / 100f)) * baseDeathChanceMultiplier / Mathf.Max(crewMembersInRepairZone.Count, 1), 0.01f, baseDeathChanceMultiplier);

            for (int i = crewMembersInRepairZone.Count - 1; i >= 0; i--)
            {
                CrewMember crew = crewMembersInRepairZone[i];
                if (crew == null)
                {
                    crewMembersInRepairZone.RemoveAt(i);
                    continue;
                }

                if (Random.value < deathChance * Time.deltaTime)
                {
                    crew.Die();
                    RemoveCrewMember(crew);
                }
            }

            yield return null;
        }

        CompleteRepair();
    }

    public void StopRepair()
    {
        if (isRepairing)
        {
            StopAllCoroutines();
            isRepairing = false;
            repairProgress = 0f;

            if (repairProgressBar != null)
            {
                repairProgressBar.gameObject.SetActive(false);
            }
        }
    }

    public void CompleteRepair()
    {
        isRepairing = false;
        SetSystemHealthToMax();

        if (repairProgressBar != null)
        {
            repairProgressBar.gameObject.SetActive(false);
        }

        repairProgress = 0f;

        foreach (var crew in crewMembersInRepairZone)
        {
            if (crew != null)
            {
                crew.CompleteTask();
            }
        }

        assignedCrewMembers.Clear();
        crewMembersInRepairZone.Clear();
    }

    private bool IsSystemFullyRepaired()
    {
        float currentHealth = GetSystemHealth();
        float maxHealth = GetMaxSystemHealth();

        return Mathf.Approximately(currentHealth, maxHealth);
    }

    private float GetSystemHealth()
    {
        switch (systemType)
        {
            case SystemType.LifeSupport:
                return lifeSupportController != null ? lifeSupportController.lifeSupportHealth : 0f;
            case SystemType.Engines:
                return engineSystemController != null ? engineSystemController.engineHealth : 0f;
            case SystemType.Generator:
                return generatorController != null ? generatorController.generatorHealth : 0f;
            case SystemType.Hull:
                return hullSystemController != null ? hullSystemController.hullHealth : 0f;
            default:
                return 0f;
        }
    }

    private float GetMaxSystemHealth()
    {
        switch (systemType)
        {
            case SystemType.LifeSupport:
                return lifeSupportController != null ? lifeSupportController.lifeSupportMaxHealth : 100f;
            case SystemType.Engines:
                return engineSystemController != null ? engineSystemController.engineMaxHealth : 100f;
            case SystemType.Generator:
                return generatorController != null ? generatorController.generatorMaxHealth : 100f;
            case SystemType.Hull:
                return hullSystemController != null ? hullSystemController.hullMaxHealth : 100f;
            default:
                return 100f;
        }
    }

    private void SetSystemHealthToMax()
    {
        switch (systemType)
        {
            case SystemType.LifeSupport:
                if (lifeSupportController != null)
                {
                    lifeSupportController.RepairLifeSupport(GetMaxSystemHealth());
                }
                break;
            case SystemType.Engines:
                if (engineSystemController != null)
                {
                    engineSystemController.RepairEngine(GetMaxSystemHealth());
                }
                break;
            case SystemType.Generator:
                if (generatorController != null)
                {
                    generatorController.RepairGenerator(GetMaxSystemHealth());
                }
                break;
            case SystemType.Hull:
                if (hullSystemController != null)
                {
                    hullSystemController.RepairHull(GetMaxSystemHealth());
                }
                break;
        }
    }

    public void DamageSystem(float damageAmount)
    {
        switch (systemType)
        {
            case SystemType.LifeSupport:
                if (lifeSupportController != null)
                {
                    lifeSupportController.DamageLifeSupport(damageAmount);
                }
                break;
            case SystemType.Engines:
                if (engineSystemController != null)
                {
                    engineSystemController.DamageEngine(damageAmount);
                }
                break;
            case SystemType.Generator:
                if (generatorController != null)
                {
                    generatorController.DamageGenerator(damageAmount);
                }
                break;
            case SystemType.Hull:
                if (hullSystemController != null)
                {
                    hullSystemController.DamageHull(damageAmount);
                }
                break;
        }

        CheckCriticalState();
    }

    private void CheckCriticalState()
    {
        switch (systemType)
        {
            case SystemType.Generator:
                if (generatorController != null && generatorController.generatorHealth <= 0)
                {
                    // Handle critical failure logic
                }
                break;
        }
    }

    public void OnSystemClicked()
    {
        if (dialogueManager == null)
        {
            return;
        }

        float systemHealth = GetSystemHealth();
        float deathChance = CalculateDeathChance(systemHealth);
        string systemName = systemType.ToString();
        dialogueManager.DisplaySystemInfo(systemName, systemHealth, deathChance);
    }

    private float CalculateDeathChance(float systemHealth)
    {
        float minDeathChance = 0.01f; // 1%
        float maxDeathChance = baseDeathChanceMultiplier; // 5%
        float damageProportion = Mathf.Clamp01((100f - systemHealth) / 100f);
        return Mathf.Lerp(minDeathChance, maxDeathChance, damageProportion);
    }

    public void TriggerInitialDialogue()
    {
        if (dialogueManager != null)
        {
            string[] introLines = new string[]
            {
                "Welcome to the ship.",
                "Your mission is to repair the critical systems."
            };
            dialogueManager.StartDialogue(introLines, systemType);
        }
    }
}
