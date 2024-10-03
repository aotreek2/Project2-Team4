using UnityEngine;
using System.Collections;

public class CubeInteraction : MonoBehaviour
{
    public enum SystemType { LifeSupport, Engines, Hull }
    public SystemType systemType;

    public SystemPanelManager systemPanelManager; // Manually assign this in the Inspector
    private CrewMember assignedCrewMember;

    public Renderer cubeRenderer; // Reference to the system part's renderer
    private Color defaultColor;
    public Color repairingColor = Color.green; // Color when the system is being repaired

    private bool isRepairing = false;

    void Start()
    {
        if (systemPanelManager == null)
        {
            Debug.LogError("SystemPanelManager not assigned in the Inspector!");
        }

        if (cubeRenderer != null)
        {
            defaultColor = cubeRenderer.material.color; // Store the original color
        }
    }

    // This method is called when the cube is clicked to interact with the system
    public void SetSelectedCrewMember(CrewMember crewMember)
    {
        assignedCrewMember = crewMember;

        if (assignedCrewMember == null)
        {
            Debug.LogError("No crew member selected!");
            return;
        }

        Debug.Log("Crew member " + assignedCrewMember.crewName + " assigned to system: " + systemType);

        // Assign the crew to the task with the cube's position as the destination
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

    // Method to start the repair when the crew enters the system trigger
    public void StartRepair(CrewMember crewMember, float efficiency)
    {
        if (!isRepairing)
        {
            isRepairing = true;
            ChangeSystemColor(repairingColor); // Change color to indicate repair
            Debug.Log("Repair started on system: " + systemType);
            StartCoroutine(RepairSystem(crewMember, efficiency));
        }
    }

    // Coroutine to handle repair process
    private IEnumerator RepairSystem(CrewMember crewMember, float efficiency)
    {
        float repairProgress = 0f;
        float adjustedRepairDuration = 5f / efficiency; // Adjust repair duration based on efficiency

        while (repairProgress < 1f)
        {
            repairProgress += Time.deltaTime / adjustedRepairDuration;
            systemPanelManager.UpdateRepairProgress(repairProgress); // Update repair progress bar
            yield return null;
        }

        // Once repair is completed
        CompleteRepair();
        crewMember.CompleteTask(); // Notify the crew member that the task is complete
    }

    // Method to change the system's color
    public void ChangeSystemColor(Color newColor)
    {
        if (cubeRenderer != null)
        {
            cubeRenderer.material.color = newColor;
        }
    }

    // Reset system's color after repair completion
    public void CompleteRepair()
    {
        isRepairing = false;
        ChangeSystemColor(defaultColor); // Restore original color
        Debug.Log("Repair completed on system: " + systemType);

        // Repair the system in ShipController
        FindObjectOfType<ShipController>().RepairSystem(systemType, 20f); // Adjust repair amount as needed

        // Reset repair progress bar
        systemPanelManager.UpdateRepairProgress(0f);
    }

    // Trigger detection for the crew entering the repair zone
    private void OnTriggerEnter(Collider other)
    {
        CrewMember crewMember = other.GetComponent<CrewMember>();
        if (crewMember != null && crewMember == assignedCrewMember)
        {
            Debug.Log("Crew member " + crewMember.crewName + " has entered the repair zone.");
            crewMember.EnterRepairZone(); // Let the crew member know they can start the repair
        }
    }
}
