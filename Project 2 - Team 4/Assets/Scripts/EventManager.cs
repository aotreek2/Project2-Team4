using UnityEngine;
using System.Collections;

public class EventManager : MonoBehaviour
{
    public ShipController shipController;
    public DecisionPanelManager decisionManager;

    void Start()
    {
        // Assign the ShipController if not set
        if (shipController == null)
        {
            shipController = FindObjectOfType<ShipController>();
        }

        // Assign the DecisionManager if not set
        if (decisionManager == null)
        {
            decisionManager = FindObjectOfType<DecisionPanelManager>();
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
        int eventType = Random.Range(0, 4); // 0: Fire, 1: Asteroid, 2: System Failure

        switch (eventType)
        {
            case 0:
                // Fire outbreak damages life support
                shipController.DamageLifeSupport(20f);
                Debug.Log("Event: Fire outbreak! Life Support damaged.");

                // Trigger a decision with descriptions for both options
                if (decisionManager != null)
                {
                    decisionManager.OpenDecisionPanel(
                        "A fire has damaged the Life Support system. What will you do?",
                        "Option 1: Sacrifice 5 crew to repair Life Support.",
                        "Option 2: Save crew but reduce Life Support efficiency by 50%.",
                        shipController
                    );
                }
                break;

            case 1:
                // Asteroid collision damages hull
                shipController.DamageHull(30f);
                Debug.Log("Event: Asteroid collision! Hull integrity reduced.");

                // Trigger a decision with descriptions for both options
                if (decisionManager != null)
                {
                    decisionManager.OpenDecisionPanel(
                        "An asteroid collision has occurred. What will you do?",
                        "Option 1: Sacrifice 10 crew to repair the hull.",
                        "Option 2: Reduce hull integrity by 50% but save the crew.",
                        shipController
                    );
                }
                break;

            case 2:
                // System failure damages engines
                shipController.DamageEngine(25f);
                Debug.Log("Event: System failure! Engines damaged.");

                // Trigger a decision with descriptions for both options
                if (decisionManager != null)
                {
                    decisionManager.OpenDecisionPanel(
                        "A system failure has damaged the Engines. What will you do?",
                        "Option 1: Sacrifice 10 crew to repair the engines.",
                        "Option 2: Reduce engine efficiency by 50% to save the crew.",
                        shipController
                    );
                }
                break;

                case 3:
            // Derelict ship encounter
                Debug.Log("Event: Derelict ship encountered!");

                if (decisionManager != null)
                {
                    decisionManager.OpenDecisionPanel(
                        "You encounter a derelict ship. Do you want to explore it?",
                        "Option 1: Explore the ship for potential resources.",
                        "Option 2: Ignore it and continue your journey.",
                        shipController
                    );
                }
                break;
        }
    }
}
