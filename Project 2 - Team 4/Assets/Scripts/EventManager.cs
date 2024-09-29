using UnityEngine;
using System.Collections;

public class EventManager : MonoBehaviour
{
    public ShipController shipController;
    public DecisionPanelManager decisionPanelManager; // Updated to use DecisionPanelManager

    void Start()
    {
        // Assign the ShipController if not set
        if (shipController == null)
        {
            shipController = FindObjectOfType<ShipController>();
        }

        // Assign the DecisionPanelManager if not set
        if (decisionPanelManager == null)
        {
            decisionPanelManager = FindObjectOfType<DecisionPanelManager>();
        }

        // Start event coroutine
        StartCoroutine(RandomEvents());
    }

    IEnumerator RandomEvents()
    {
        while (true)
        {
            // Wait for a random interval between events
            float waitTime = Random.Range(10f, 20f);
            yield return new WaitForSeconds(waitTime);

            // Trigger a random event
            TriggerRandomEvent();
        }
    }

    void TriggerRandomEvent()
    {
        int eventType = Random.Range(0, 3); // 0: Fire, 1: Asteroid, 2: System Failure

        switch (eventType)
        {
            case 0:
                // Fire outbreak damages life support
                shipController.DamageLifeSupport(20f);
                Debug.Log("Event: Fire outbreak! Life Support damaged.");

                // Trigger the decision panel
                decisionPanelManager.OpenDecisionPanel("A fire has damaged the Life Support system. Choose an action:", shipController);
                break;

            case 1:
                // Asteroid collision damages hull
                shipController.DamageHull(30f);
                Debug.Log("Event: Asteroid collision! Hull integrity reduced.");

                // Trigger the decision panel
                decisionPanelManager.OpenDecisionPanel("An asteroid collision has damaged the hull. Choose an action:", shipController);
                break;

            case 2:
                // System failure damages engines
                shipController.DamageEngine(25f);
                Debug.Log("Event: System failure! Engines damaged.");

                // Trigger the decision panel
                decisionPanelManager.OpenDecisionPanel("A system failure has damaged the Engines. Choose an action:", shipController);
                break;
        }
    }
}
