using UnityEngine;

public class CubeInteraction : MonoBehaviour
{
    // Existing variables
    private ShipController shipController;

    // System type represented by this cube
    public enum SystemType { LifeSupport, Engines, Hull }
    public SystemType systemType;

    // Reference to the SystemPanelManager
    private SystemPanelManager systemPanelManager;

    void Start()
    {
        // Find the ShipController in the parent hierarchy
        shipController = GetComponentInParent<ShipController>();

        // Find the SystemPanelManager in the scene
        systemPanelManager = FindObjectOfType<SystemPanelManager>();
    }

    void OnMouseDown()
    {
        if (systemPanelManager != null)
        {
            // Open the System Panel and pass system information
            systemPanelManager.OpenSystemPanel(systemType, shipController);
        }
    }
}
