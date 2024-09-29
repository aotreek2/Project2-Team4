using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    public Camera mainCamera;
    public LayerMask crewLayer;   // Layer for crew members
    public LayerMask systemLayer; // Layer for systems (Engines, Life Support, Hull)

    private CrewMember selectedCrewMember; // Currently selected crew member
    public SystemPanelManager systemPanelManager; // Reference to SystemPanelManager

    void Update()
    {
        HandleSelection();
        HandleAssignment();
    }

    // Handle selection of crew members with left click
    void HandleSelection()
    {
        if (Input.GetMouseButtonDown(0)) // Left-click to select or re-select
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f, crewLayer))
            {
                CrewMember crew = hit.collider.GetComponent<CrewMember>();
                if (crew != null)
                {
                    // Deselect previous crew member, if any
                    if (selectedCrewMember != null && selectedCrewMember != crew)
                    {
                        selectedCrewMember.Deselect(); 
                    }

                    // Select the clicked crew member (or re-select)
                    selectedCrewMember = crew;
                    selectedCrewMember.Select();

                    // Set selected crew in the SystemPanelManager for future use
                    systemPanelManager.SetSelectedCrewMember(selectedCrewMember);

                    Debug.Log("Selected crew member: " + selectedCrewMember.crewName);
                }
            }
        }
    }

    // Handle right-click to assign selected crew members to a system (e.g., Engines, Life Support, Hull)
    void HandleAssignment()
    {
        if (Input.GetMouseButtonDown(1) && selectedCrewMember != null) // Right-click to assign task
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f, systemLayer))
            {
                CubeInteraction cubeInteraction = hit.collider.GetComponent<CubeInteraction>();
                if (cubeInteraction != null)
                {
                    // Assign the selected crew member to the system
                    cubeInteraction.SetSelectedCrewMember(selectedCrewMember);

                    Debug.Log("Crew member " + selectedCrewMember.crewName + " assigned to system: " + cubeInteraction.systemType);

                    // Do not deselect the crew member after assignment, so they can be moved again
                    // selectedCrewMember.Deselect(); // Remove this line to keep them selected
                }
            }
        }
    }
}
