using UnityEngine;
using UnityEngine.EventSystems; // This is needed to detect UI elements

public class SelectionManager : MonoBehaviour
{
    public Camera mainCamera;
    public LayerMask crewLayer;   // Layer for crew members
    public LayerMask systemLayer; // Layer for systems (Engines, Life Support, Hull)

    private CrewMember selectedCrewMember; // Currently selected crew member

    void Update()
    {
        HandleSelection();
        HandleAssignment();
    }

    // Handle selection of crew members with left click
    void HandleSelection()
    {
        // Check if the mouse is over a UI element, if it is, don't select anything
        if (IsPointerOverUIElement())
        {
            return;
        }

        if (Input.GetMouseButtonDown(0)) // Left-click to select or re-select
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Debug.DrawRay(ray.origin, ray.direction * 100, Color.green);

            // Raycast to detect crew members based on their colliders
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

                    Debug.Log("Selected crew member: " + selectedCrewMember.crewName);
                    Debug.Log("Selected crew member: " + selectedCrewMember.gameObject.name);
                }
            }
            else
            {
                // If clicked elsewhere, deselect the current crew member
                if (selectedCrewMember != null)
                {
                    selectedCrewMember.Deselect();
                    selectedCrewMember = null;
                }
            }
        }
    }

    // Handle right-click to assign selected crew members to a system (e.g., Engines, Life Support, Hull)
    void HandleAssignment()
    {
        if (Input.GetMouseButtonDown(1) && selectedCrewMember != null) // Right-click to assign task
        {
            // Ignore clicks on UI elements
            if (IsPointerOverUIElement())
            {
                return;
            }

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f))
            {
                if (((1 << hit.collider.gameObject.layer) & systemLayer) != 0)
                {
                    CubeInteraction cubeInteraction = hit.collider.GetComponent<CubeInteraction>();
                    if (cubeInteraction != null)
                    {
                        // Assign the selected crew member to the system
                        cubeInteraction.SetSelectedCrewMember(selectedCrewMember);

                        Debug.Log("Crew member " + selectedCrewMember.crewName + " assigned to system: " + cubeInteraction.systemType);

                        // Deselect the crew member after assignment
                        selectedCrewMember.Deselect();
                        selectedCrewMember = null;
                    }
                }
                else
                {
                    // If right-clicked elsewhere, set crew member to rest
                    selectedCrewMember.Rest();
                    Debug.Log("Crew member " + selectedCrewMember.crewName + " is resting.");

                    selectedCrewMember.Deselect();
                    selectedCrewMember = null;
                }
            }
        }
    }

    // Helper method to check if the mouse is over any UI element
    private bool IsPointerOverUIElement()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }
}
