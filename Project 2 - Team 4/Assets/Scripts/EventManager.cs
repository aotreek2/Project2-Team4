using UnityEngine;
using System.Collections;

public class EventManager : MonoBehaviour
{
    public ShipController shipController;
    public LifeSupportController lifeSupportController;
    public HullSystemController hullSystemController;
    public EngineSystemController engineSystemController;
    public DecisionPanelManager decisionManager;
    public ChapterManager chapterManager; // Reference to ChapterManager

    // Audio (Ahmed)
    public AudioSource eventAudio;
    public AudioClip asteroidHit;

    public Animator[] asteroidEventAnim;
    public Animator derelictShipAnim;

    void Start()
    {
        // Assign the necessary controllers and managers
        shipController = shipController ?? FindObjectOfType<ShipController>();
        lifeSupportController = lifeSupportController ?? FindObjectOfType<LifeSupportController>();
        hullSystemController = hullSystemController ?? FindObjectOfType<HullSystemController>();
        engineSystemController = engineSystemController ?? FindObjectOfType<EngineSystemController>();
        decisionManager = decisionManager ?? FindObjectOfType<DecisionPanelManager>();
        chapterManager = chapterManager ?? FindObjectOfType<ChapterManager>();

        // Start event coroutine for random events after Chapter One
        StartCoroutine(ManageChapterEvents());
    }

    void Update()
    {
        HandleHotkeys(); // Check for hotkeys to manually trigger events for testing
    }

    // Handle hotkeys for testing events manually
    private void HandleHotkeys()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("Hotkey 1 pressed: Triggering Fire Event");
            StartCoroutine(HandleFireEvent());
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("Hotkey 2 pressed: Triggering Asteroid Event");
            StartCoroutine(HandleAsteroidEvent());
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Debug.Log("Hotkey 3 pressed: Triggering System Failure Event");
            StartCoroutine(HandleSystemFailureEvent());
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            Debug.Log("Hotkey 4 pressed: Triggering Derelict Ship Event");
            StartCoroutine(HandleDerelictEvent());
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            Debug.Log("Hotkey 5 pressed: Triggering Generator Failure Event");
            StartCoroutine(HandleGeneratorFailureEvent());
        }
    }

    // Manages random events based on the current chapter
    private IEnumerator ManageChapterEvents()
    {
        // Wait for Chapter One to be completed before starting random events
        yield return new WaitUntil(() => chapterManager.currentChapter != ChapterManager.Chapter.Chapter1);

        // Start random events after Chapter One
        StartCoroutine(RandomEvents());
    }

    // Coroutine for random event generation, only after Chapter One
    IEnumerator RandomEvents()
    {
        while (true)
        {
            float waitTime = Random.Range(10f, 20f);
            yield return new WaitForSeconds(waitTime);

            if (chapterManager.currentChapter == ChapterManager.Chapter.Chapter2 || 
                chapterManager.currentChapter == ChapterManager.Chapter.Chapter3 || 
                chapterManager.currentChapter == ChapterManager.Chapter.Chapter4)
            {
                TriggerRandomEvent();
            }
        }
    }

    // Method to trigger random events
    void TriggerRandomEvent()
    {
        int eventType = Random.Range(0, 5); // 0: Fire, 1: Asteroid, 2: System Failure, 3: Derelict, 4: Generator Failure

        switch (eventType)
        {
            case 0:
                StartCoroutine(HandleFireEvent());
                break;
            case 1:
                StartCoroutine(HandleAsteroidEvent());
                if (!eventAudio.isPlaying)
                {
                    eventAudio.PlayOneShot(asteroidHit);
                }
                break;
            case 2:
                StartCoroutine(HandleSystemFailureEvent());
                break;
            case 3:
                StartCoroutine(HandleDerelictEvent());
                break;
            case 4:
                StartCoroutine(HandleGeneratorFailureEvent());
                break;
        }
    }

    // Fire Event coroutine
    private IEnumerator HandleFireEvent()
    {
        lifeSupportController?.DamageLifeSupport(20f);
        Debug.Log("Event: Fire outbreak! Life Support damaged.");

        yield return new WaitForSeconds(1.0f);

        decisionManager?.OpenDecisionPanel(
            "A fire has damaged the Life Support system. What will you do?",
            "Option 1: Sacrifice 5 crew to repair Life Support.",
            "Option 2: Save crew but reduce Life Support efficiency by 50%.",
            shipController
        );
    }

    // Generator Failure Event coroutine
    private IEnumerator HandleGeneratorFailureEvent()
    {
        shipController.DamageGenerator(30f);
        Debug.Log("Event: Generator failure! Generator health reduced.");

        yield return new WaitForSeconds(1.0f);

        decisionManager?.OpenDecisionPanel(
            "The generator has failed! What will you do?",
            "Option 1: Sacrifice 10 crew to repair the generator.",
            "Option 2: Accept reduced efficiency for other systems.",
            shipController
        );
    }

    // Asteroid Event coroutine
    private IEnumerator HandleAsteroidEvent()
    {
        hullSystemController?.StartDamageOverTime(5f);
        Debug.Log("Event: Asteroid collision! Hull is taking damage over time.");

        int asteroidSpawn = Random.Range(0, asteroidEventAnim.Length);
        asteroidEventAnim[asteroidSpawn].SetTrigger("DoEvent");

        yield return new WaitForSeconds(1.0f);

        decisionManager?.OpenDecisionPanel(
            "An asteroid collision has occurred. The hull is taking damage over time. What will you do?",
            "Option 1: Sacrifice 10 crew to repair the hull immediately.",
            "Option 2: Let the hull continue to take damage.",
            shipController
        );
    }

    // System Failure Event coroutine
    private IEnumerator HandleSystemFailureEvent()
    {
        engineSystemController?.DamageEngine(25f);
        Debug.Log("Event: System failure! Engines damaged.");

        yield return new WaitForSeconds(1.0f);

        decisionManager?.OpenDecisionPanel(
            "A system failure has damaged the Engines. What will you do?",
            "Option 1: Sacrifice 10 crew to repair the engines.",
            "Option 2: Reduce engine efficiency by 50% to save the crew.",
            shipController
        );
    }

    // Derelict Event coroutine
    private IEnumerator HandleDerelictEvent()
    {
        Debug.Log("Event: Derelict ship encountered!");

        derelictShipAnim.SetTrigger("DoEvent");

        decisionManager?.OpenDecisionPanel(
            "You encounter a derelict ship. Do you want to explore it?",
            "Option 1: Explore the ship for potential resources.",
            "Option 2: Ignore it and continue your journey.",
            shipController
        );

        yield return null;
    }
}
