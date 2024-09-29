using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SystemPanelManager : MonoBehaviour
{
    // UI Elements
    public TextMeshProUGUI systemNameText;
    public TextMeshProUGUI systemDescriptionText;
    public Button repairButton;
    public Button closeButton;
    public Slider repairProgressBar;

    // References
    private ShipController shipController;
    private CubeInteraction.SystemType currentSystemType;
    private CrewMember selectedCrewMember;
    private CubeInteraction currentSystemInteraction; // Reference to the system cube

    private bool isRepairing = false;
    private float repairProgress = 0f;
    private float repairDuration = 10f; // You can adjust the repair time here

    void Start()
    {
        // Ensure all necessary UI components are assigned
        if (repairButton == null || closeButton == null || repairProgressBar == null)
        {
            Debug.LogError("UI components not assigned! Check the Inspector.");
            return;
        }

        // Assign button listeners
        repairButton.onClick.AddListener(OnRepairButtonClicked);
        closeButton.onClick.AddListener(OnCloseButtonClicked);

        // Hide the panel and progress bar at the start
        gameObject.SetActive(false);
        repairProgressBar.gameObject.SetActive(false);
    }

    void Update()
    {
        if (isRepairing)
        {
            PerformRepair();
        }
    }

    // Method to set the selected crew member
    public void SetSelectedCrewMember(CrewMember crewMember)
    {
        selectedCrewMember = crewMember;
        Debug.Log("Crew member " + crewMember.crewName + " selected.");
    }

    // Method to update the repair progress bar
    public void UpdateRepairProgress(float progress)
    {
        // Ensure the progress bar is visible
        repairProgressBar.gameObject.SetActive(true);

        // Update the progress bar value
        repairProgressBar.value = progress;

        // If the task is completed (progress = 1), hide the progress bar
        if (progress >= 1f)
        {
            isRepairing = false;
            repairProgressBar.gameObject.SetActive(false);
            Debug.Log("Repair completed!");
        }
    }

    // Repair system method
    private void PerformRepair()
    {
        // Simulate repair task over time
        repairProgress += Time.deltaTime / repairDuration;

        // Update the progress bar in the UI
        UpdateRepairProgress(repairProgress);

        // If repair is complete
        if (repairProgress >= 1f)
        {
            float repairAmount = 20f; // You can adjust this value based on system health
            shipController.RepairSystem(currentSystemType, repairAmount);
            repairProgress = 0f; // Reset progress for future repairs
        }
    }

    // Updated method to accept CubeInteraction reference
    public void OpenSystemPanel(CubeInteraction.SystemType systemType, ShipController controller, CrewMember crewMember, CubeInteraction cubeInteraction)
    {
        currentSystemType = systemType;
        shipController = controller;
        selectedCrewMember = crewMember;
        currentSystemInteraction = cubeInteraction; // Store the system cube reference

        // Debug log to ensure crew member is correctly assigned
        if (selectedCrewMember != null)
        {
            Debug.Log("Crew member " + selectedCrewMember.crewName + " selected for system panel.");
        }
        else
        {
            Debug.LogError("No crew member selected!");
        }

        // Update UI based on the system type
        switch (systemType)
        {
            case CubeInteraction.SystemType.LifeSupport:
                systemNameText.text = "Life Support";
                systemDescriptionText.text = "Maintains oxygen levels for the crew.";
                break;
            case CubeInteraction.SystemType.Engines:
                systemNameText.text = "Engines";
                systemDescriptionText.text = "Propels the ship towards the Lighthouse.";
                break;
            case CubeInteraction.SystemType.Hull:
                systemNameText.text = "Hull";
                systemDescriptionText.text = "Protects the ship from external threats.";
                break;
        }

        // Show the panel
        gameObject.SetActive(true);
    }

    void OnRepairButtonClicked()
    {
        if (selectedCrewMember == null)
        {
            Debug.LogError("No crew member selected for repair!");
            return;
        }

        // Determine the task type based on the system
        CrewMember.Task taskType = CrewMember.Task.Idle;
        switch (currentSystemType)
        {
            case CubeInteraction.SystemType.Engines:
                taskType = CrewMember.Task.RepairEngines;
                break;
            case CubeInteraction.SystemType.LifeSupport:
                taskType = CrewMember.Task.RepairLifeSupport;
                break;
            case CubeInteraction.SystemType.Hull:
                taskType = CrewMember.Task.RepairHull;
                break;
        }

        // Assign the crew member to the task
        Vector3 destination = currentSystemInteraction.transform.position;

        // Assuming you have some way to determine the system's damage level
        float damageLevel = 0.5f; // Placeholder value

        selectedCrewMember.AssignToTask(taskType, destination, damageLevel, shipController, currentSystemType);

        // Start repair process
        isRepairing = true;
        repairProgress = 0f; // Reset repair progress when starting new repair
        repairProgressBar.gameObject.SetActive(true); // Show progress bar during repair
    }

    void OnCloseButtonClicked()
    {
        // Close the panel
        gameObject.SetActive(false);
    }
}
