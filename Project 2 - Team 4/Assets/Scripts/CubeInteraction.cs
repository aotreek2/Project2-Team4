using UnityEngine;
using System.Collections;

public class CubeInteraction : MonoBehaviour
{
    public enum SystemType { LifeSupport, Engines, Hull, Generator }
    public SystemType systemType;

    public SystemPanelManager systemPanelManager;
    private CrewMember assignedCrewMember;

    public Renderer cubeRenderer;
    private Color defaultColor;
    public Color repairingColor = Color.green;

    private bool isRepairing = false;

    // **Added ChapterManager Reference**
    private ChapterManager chapterManager;

    void Start()
    {
        if (systemPanelManager == null)
        {
            Debug.LogError("SystemPanelManager not assigned in the Inspector!");
        }

        if (cubeRenderer != null)
        {
            defaultColor = cubeRenderer.material.color;
        }

        // Initialize ChapterManager
        chapterManager = FindObjectOfType<ChapterManager>();
    }

    public void SetSelectedCrewMember(CrewMember crewMember)
    {
        assignedCrewMember = crewMember;

        if (assignedCrewMember == null)
        {
            Debug.LogError("No crew member selected!");
            return;
        }

        Debug.Log("Crew member " + assignedCrewMember.crewName + " assigned to system: " + systemType);

        assignedCrewMember.AssignToTask(CrewMember.Task.RepairHull, transform.position, FindObjectOfType<ShipController>(), this);

        if (systemPanelManager != null)
        {
            systemPanelManager.OpenSystemPanel(systemType, FindObjectOfType<ShipController>());
            Debug.Log("SystemPanelManager opened for system: " + systemType);
        }
        else
        {
            Debug.LogError("SystemPanelManager not found or not assigned!");
        }
    }

    // **Add the StartRepair method back into the script**
    public void StartRepair(CrewMember crewMember, float efficiency)
    {
        if (!isRepairing)
        {
            isRepairing = true;
            ChangeSystemColor(repairingColor);
            Debug.Log("Repair started on system: " + systemType);
            StartCoroutine(RepairSystem(crewMember, efficiency));
        }
    }

    private IEnumerator RepairSystem(CrewMember crewMember, float efficiency)
    {
        float repairProgress = 0f;
        float adjustedRepairDuration = 5f / efficiency;

        while (repairProgress < 1f)
        {
            repairProgress += Time.deltaTime / adjustedRepairDuration;
            systemPanelManager.UpdateRepairProgress(repairProgress);
            yield return null;
        }

        CompleteRepair();
        crewMember.CompleteTask();
    }

    public void ChangeSystemColor(Color newColor)
    {
        if (cubeRenderer != null)
        {
            cubeRenderer.material.color = newColor;
        }
    }

    public void CompleteRepair()
    {
        isRepairing = false;
        ChangeSystemColor(defaultColor);
        Debug.Log("Repair completed on system: " + systemType);

         TriggerRepairDialogue();

        switch (systemType)
        {
            case SystemType.LifeSupport:
                FindObjectOfType<LifeSupportController>().RepairLifeSupport(20f); // Use LifeSupportController for LifeSupport repairs
                break;

            case SystemType.Engines:
                FindObjectOfType<ShipController>().RepairEngine(20f); // Use ShipController for Engine repairs
                break;

            case SystemType.Hull:
                FindObjectOfType<HullSystemController>().RepairHull(20f); // Use HullSystemController for Hull repairs
                break;

            case SystemType.Generator:
                FindObjectOfType<ShipController>().RepairGenerator(20f); // Use ShipController for Generator repairs
                break;
        }

        systemPanelManager.UpdateRepairProgress(0f);
    }

    private void TriggerRepairDialogue()
    {
        DialogueManager dialogueManager = FindObjectOfType<DialogueManager>();
        if (dialogueManager != null)
        {
            string[] dialogueLines = new string[]
            {
                $"{systemType} has been successfully repaired!",
                "Good job! Keep it up."
            };
            dialogueManager.StartDialogue(dialogueLines);
        }
    }

    public void SetPulsingEffect(bool isActive)
    {
        PulsingEffect pulsingEffect = GetComponent<PulsingEffect>();
        if (pulsingEffect == null && isActive)
        {
            pulsingEffect = gameObject.AddComponent<PulsingEffect>();
        }
        else if (pulsingEffect != null && !isActive)
        {
            Destroy(pulsingEffect);
        }
    }

}
