using UnityEngine;
using System.Collections;

public class CubeInteraction : MonoBehaviour
{
    public enum SystemType { LifeSupport, Engines, Hull }
    public SystemType systemType;

    public SystemPanelManager systemPanelManager; // Manually assign this in the Inspector
    private CrewMember selectedCrewMember;

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
        selectedCrewMember = crewMember;

        if (selectedCrewMember == null)
        {
            Debug.LogError("No crew member selected!");
            return;
        }

        Debug.Log("Crew member " + selectedCrewMember.crewName + " assigned to system: " + systemType);

        // Pass the correct cube's position as the destination
        Vector3 destination = Vector3.zero;
        switch (systemType)
        {
            case SystemType.LifeSupport:
                destination = FindObjectOfType<ShipController>().lifeSupportCube.transform.position;
                break;
            case SystemType.Engines:
                destination = FindObjectOfType<ShipController>().enginesCube.transform.position;
                break;
            case SystemType.Hull:
                destination = FindObjectOfType<ShipController>().hullCube.transform.position;
                break;
        }

        // Assign the crew to the task with the correct destination
        selectedCrewMember.AssignToTask(CrewMember.Task.RepairHull, destination, FindObjectOfType<ShipController>(), this);

        if (systemPanelManager != null)
        {
            systemPanelManager.OpenSystemPanel(systemType, FindObjectOfType<ShipController>(), selectedCrewMember, this);
            Debug.Log("SystemPanelManager opened for system: " + systemType);
        }
        else
        {
            Debug.LogError("SystemPanelManager not found or not assigned!");
        }
    }


    // Method to start the repair when the crew enters the system trigger
    public void StartRepair(CrewMember crewMember)
    {
        if (!isRepairing)
        {
            isRepairing = true;
            ChangeSystemColor(repairingColor); // Change color to indicate repair
            Debug.Log("Repair started on system: " + systemType);
            StartCoroutine(RepairSystem(crewMember));
        }
    }

    // Coroutine to handle repair process
    private IEnumerator RepairSystem(CrewMember crewMember)
    {
        float repairProgress = 0f;
        float repairDuration = 5f; // Example duration of repair, you can adjust it

        while (repairProgress < 1f)
        {
            repairProgress += Time.deltaTime / repairDuration;
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
    }

    // **Trigger detection for the crew entering the repair zone**
    private void OnTriggerEnter(Collider other)
    {
        CrewMember crewMember = other.GetComponent<CrewMember>();
        if (crewMember != null && crewMember == selectedCrewMember)
        {
            Debug.Log("Crew member " + crewMember.crewName + " has entered the repair zone.");
            crewMember.EnterRepairZone(); // Let the crew member know they can start the repair
        }
    }

}
