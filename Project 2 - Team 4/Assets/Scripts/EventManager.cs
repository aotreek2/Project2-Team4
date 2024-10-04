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
        int eventType = Random.Range(0, 5); // 0: Fire, 1: Asteroid, 2: System Failure, 3: Derelict, 4: Generator Failure

        switch (eventType)
        {
            case 0:
                // Fire outbreak damages life support
                StartCoroutine(HandleFireEvent());
                break;

            case 1:
                // Asteroid collision damages hull
                StartCoroutine(HandleAsteroidEvent());
                break;

            case 2:
                // System failure damages engines
                StartCoroutine(HandleSystemFailureEvent());
                break;

            case 3:
                // Derelict ship encounter
                StartCoroutine(HandleDerelictEvent());
                break;

            case 4:
                // Generator failure event
                StartCoroutine(HandleGeneratorFailureEvent());
                break;
        }
    }


// Coroutine for Fire Event
    private IEnumerator HandleFireEvent()
    {
        shipController.DamageLifeSupport(20f);
        Debug.Log("Event: Fire outbreak! Life Support damaged.");

        // Wait for effects to finish (adjust duration as needed)
        yield return new WaitForSeconds(1.0f);

        // Open the decision panel
        if (decisionManager != null)
        {
            decisionManager.OpenDecisionPanel(
                "A fire has damaged the Life Support system. What will you do?",
                "Option 1: Sacrifice 5 crew to repair Life Support.",
                "Option 2: Save crew but reduce Life Support efficiency by 50%.",
                shipController
            );
        }
    }

    // Coroutine for Generator Failure Event
    private IEnumerator HandleGeneratorFailureEvent()
    {
        shipController.DamageGenerator(30f); // Example damage value
        Debug.Log("Event: Generator failure! Generator health reduced.");

        // Wait for effects to finish (adjust duration as needed)
        yield return new WaitForSeconds(1.0f);

        if (decisionManager != null)
        {
            decisionManager.OpenDecisionPanel(
                "The generator has failed! What will you do?",
                "Option 1: Sacrifice 10 crew to repair the generator.",
                "Option 2: Accept reduced efficiency for other systems.",
                shipController
            );
        }
    }


    // Coroutine for Asteroid Event
    private IEnumerator HandleAsteroidEvent()
    {
        shipController.DamageHull(30f);
        Debug.Log("Event: Asteroid collision! Hull integrity reduced.");

        // Wait for effects to finish
        yield return new WaitForSeconds(1.0f);

        if (decisionManager != null)
        {
            decisionManager.OpenDecisionPanel(
                "An asteroid collision has occurred. What will you do?",
                "Option 1: Sacrifice 10 crew to repair the hull.",
                "Option 2: Reduce hull integrity by 50% but save the crew.",
                shipController
            );
        }
    }

    // Coroutine for System Failure Event
    private IEnumerator HandleSystemFailureEvent()
    {
        shipController.DamageEngine(25f);
        Debug.Log("Event: System failure! Engines damaged.");

        // Wait for effects to finish
        yield return new WaitForSeconds(1.0f);

        if (decisionManager != null)
        {
            decisionManager.OpenDecisionPanel(
                "A system failure has damaged the Engines. What will you do?",
                "Option 1: Sacrifice 10 crew to repair the engines.",
                "Option 2: Reduce engine efficiency by 50% to save the crew.",
                shipController
            );
        }
    }

    // Coroutine for Derelict Event
    private IEnumerator HandleDerelictEvent()
    {
        Debug.Log("Event: Derelict ship encountered!");

        // No damage effects, so no need to wait
        if (decisionManager != null)
        {
            decisionManager.OpenDecisionPanel(
                "You encounter a derelict ship. Do you want to explore it?",
                "Option 1: Explore the ship for potential resources.",
                "Option 2: Ignore it and continue your journey.",
                shipController
            );
        }

        yield return null;
    }
}
