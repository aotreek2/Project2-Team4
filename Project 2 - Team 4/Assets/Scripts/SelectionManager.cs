using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    public Camera mainCamera;
    public LayerMask crewLayer; // Layer for crew members
    public LayerMask systemLayer; // Layer for systems (Engines, Life Support, Hull)

    private CrewMember selectedCrewMember; // Currently selected crew member

    void Update()
    {
        HandleSelection();
        HandleAssignment();
    }

    void HandleSelection()
    {
        if (Input.GetMouseButtonDown(0)) // Left click to select
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f, crewLayer))
            {
                CrewMember crew = hit.collider.GetComponent<CrewMember>();
                if (crew != null)
                {
                    if (selectedCrewMember != null)
                    {
                        selectedCrewMember.Deselect(); // Deselect the previous crew member
                    }

                    selectedCrewMember = crew; // Select the clicked crew member
                    selectedCrewMember.Select();
                }
            }
        }
    }

    void HandleAssignment()
    {
        if (Input.GetMouseButtonDown(1) && selectedCrewMember != null) // Right click to assign task
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f, systemLayer))
            {
                CubeInteraction cubeInteraction = hit.collider.GetComponent<CubeInteraction>();
                if (cubeInteraction != null)
                {
                    // Assign the selected crew member to the CubeInteraction
                    cubeInteraction.SetSelectedCrewMember(selectedCrewMember);

                    // Deselect after assigning task
                    selectedCrewMember.Deselect();
                    selectedCrewMember = null;
                }
            }
        }
    }
}
