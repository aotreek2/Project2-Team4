using UnityEngine;
using System.Collections;

public class EventManager : MonoBehaviour
{
    public ShipController shipController;
    public DecisionManager decisionManager;

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
            decisionManager = FindObjectOfType<DecisionManager>();
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

                // Optionally, trigger a decision
                if (decisionManager != null)
                {
                    decisionManager.ShowDecision("A fire has damaged the Life Support system. Choose an action:");
                }
                break;

            case 1:
                // Asteroid collision damages hull
                shipController.DamageHull(30f);
                Debug.Log("Event: Asteroid collision! Hull integrity reduced.");

                // Optionally, trigger a decision
                if (decisionManager != null)
                {
                    decisionManager.ShowDecision("An asteroid collision has occurred. Choose an action:");
                }
                break;

            case 2:
                // System failure damages engines
                shipController.DamageEngine(25f);
                Debug.Log("Event: System failure! Engines damaged.");

                // Optionally, trigger a decision
                if (decisionManager != null)
                {
                    decisionManager.ShowDecision("A system failure has damaged the Engines. Choose an action:");
                }
                break;
        }
    }
}
