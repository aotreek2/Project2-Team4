using UnityEngine;

public class CubeInteraction : MonoBehaviour
{
    public enum SystemType { LifeSupport, Engines, Hull }
    public SystemType systemType;

    public SystemPanelManager systemPanelManager; // Manually assign this in the Inspector
    private CrewMember selectedCrewMember;

    void Start()
    {
        if (systemPanelManager == null)
        {
            Debug.LogError("SystemPanelManager not assigned in the Inspector!");
        }
    }

    // This method is called when the cube is clicked to interact with the system
// Updated SetSelectedCrewMember to pass CubeInteraction as the 4th argument
    public void SetSelectedCrewMember(CrewMember crewMember)
    {
        selectedCrewMember = crewMember;

        if (selectedCrewMember == null)
        {
            Debug.LogError("No crew member selected!");
            return;
        }

        Debug.Log("Crew member " + selectedCrewMember.crewName + " assigned to system: " + systemType);

        // Check if SystemPanelManager exists before opening the panel
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

}
