using UnityEngine;

public class CubeInteraction : MonoBehaviour
{
    public enum SystemType { LifeSupport, Engines, Hull }

    public SystemType systemType;
    public SystemPanelManager systemPanelManager;
    public ShipController shipController;
    private CrewMember selectedCrewMember; // Reference to the selected crew member

    void Start()
    {
        // Find the system panel manager in the scene (if not assigned in the Inspector)
        if (systemPanelManager == null)
        {
            systemPanelManager = FindObjectOfType<SystemPanelManager>();
        }

        if (shipController == null)
        {
            shipController = FindObjectOfType<ShipController>();
        }
    }

    void OnMouseDown()
    {
        // Open the system panel and pass the current system type, ship controller, and selected crew member
        systemPanelManager.OpenSystemPanel(systemType, shipController, selectedCrewMember);
    }

    // Method to set the selected crew member (called from the SelectionManager)
    public void SetSelectedCrewMember(CrewMember crew)
    {
        selectedCrewMember = crew;
        Debug.Log("Selected Crew Member: " + selectedCrewMember.crewName);
    }
}
