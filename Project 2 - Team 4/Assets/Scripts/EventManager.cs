using UnityEngine;
using System.Collections;

public class EventManager : MonoBehaviour
{
    public ShipController shipController;
    public LifeSupportController lifeSupportController;
    public HullSystemController hullSystemController;
    public EngineSystemController engineSystemController;
    public GeneratorController generatorController; // Reference to GeneratorController
    public DecisionPanelManager decisionManager;
    public ChapterManager chapterManager;

    public AudioSource eventAudio;
    public AudioClip asteroidHit;
    public Animator[] asteroidEventAnim;
    public Animator derelictShipAnim;
    public ResourceManager resourceManager; 

    private bool isEventActive = false;

    void Start()
    {
        shipController = shipController ?? FindObjectOfType<ShipController>();
        lifeSupportController = lifeSupportController ?? FindObjectOfType<LifeSupportController>();
        hullSystemController = hullSystemController ?? FindObjectOfType<HullSystemController>();
        engineSystemController = engineSystemController ?? FindObjectOfType<EngineSystemController>();
        generatorController = generatorController ?? FindObjectOfType<GeneratorController>();
        decisionManager = decisionManager ?? FindObjectOfType<DecisionPanelManager>();
        chapterManager = chapterManager ?? FindObjectOfType<ChapterManager>();
        resourceManager = resourceManager ?? FindObjectOfType<ResourceManager>();

        StartCoroutine(ManageChapterEvents());
    }

    void Update()
    {
        HandleHotkeys();
    }

    private void HandleHotkeys()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("Triggering Fire Event");
            StartCoroutine(HandleFireEvent());
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("Triggering Asteroid Event");
            StartCoroutine(HandleAsteroidEvent());
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Debug.Log("Triggering System Failure Event");
            StartCoroutine(HandleSystemFailureEvent());
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            Debug.Log("Triggering Derelict Ship Event");
            StartCoroutine(HandleDerelictEvent());
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            Debug.Log("Triggering Generator Failure Event");
            StartCoroutine(HandleGeneratorFailureEvent());
        }
    }

    private IEnumerator ManageChapterEvents()
    {
        yield return new WaitUntil(() => chapterManager.currentChapter != ChapterManager.Chapter.Chapter1);
        StartCoroutine(RandomEvents());
    }

    IEnumerator RandomEvents()
    {
        while (true)
        {
            float waitTime = Random.Range(10f, 20f);
            yield return new WaitForSeconds(waitTime);
            if (isEventActive || decisionManager.isDecisionPanelOpen) continue;

            TriggerRandomEvent();
        }
    }

    void TriggerRandomEvent()
    {
        int eventType = Random.Range(0, 5); 

        switch (eventType)
        {
            case 0: StartCoroutine(HandleFireEvent()); break;
            case 1: StartCoroutine(HandleAsteroidEvent()); break;
            case 2: StartCoroutine(HandleSystemFailureEvent()); break;
            case 3: StartCoroutine(HandleDerelictEvent()); break;
            case 4: StartCoroutine(HandleGeneratorFailureEvent()); break;
        }
    }

    private IEnumerator HandleFireEvent()
    {
        isEventActive = true;
        lifeSupportController?.DamageLifeSupport(20f);

        yield return new WaitForSeconds(1.0f);

        decisionManager?.OpenDecisionPanel(
            "A fire has damaged the Life Support system. What will you do?",
            "Option 1: Sacrifice 5 crew to repair Life Support.",
            "Option 2: Save crew but reduce Life Support efficiency by 50%.",
            shipController
        );

        yield return new WaitUntil(() => !decisionManager.isDecisionPanelOpen);
        isEventActive = false;
    }

    private IEnumerator HandleGeneratorFailureEvent()
    {
        isEventActive = true;
        shipController.DamageGenerator(30f);

        yield return new WaitForSeconds(1.0f);

        decisionManager?.OpenDecisionPanel(
            "The generator has failed! What will you do?",
            "Option 1: Sacrifice 10 crew to repair the generator.",
            "Option 2: Accept reduced efficiency for other systems.",
            shipController
        );

        yield return new WaitUntil(() => !decisionManager.isDecisionPanelOpen);
        isEventActive = false;
    }

    private IEnumerator HandleAsteroidEvent()
    {
        isEventActive = true;
        hullSystemController?.StartDamageOverTime(5f);

        int asteroidSpawn = Random.Range(0, asteroidEventAnim.Length);
        asteroidEventAnim[asteroidSpawn].SetTrigger("DoEvent");

        yield return new WaitForSeconds(1.0f);

        decisionManager?.OpenDecisionPanel(
            "An asteroid collision has occurred. The hull is taking damage over time. What will you do?",
            "Option 1: Sacrifice 10 crew to repair the hull immediately.",
            "Option 2: Let the hull continue to take damage.",
            shipController
        );

        yield return new WaitUntil(() => !decisionManager.isDecisionPanelOpen);
        isEventActive = false;
    }

    private IEnumerator HandleSystemFailureEvent()
    {
        isEventActive = true;
        engineSystemController?.DamageEngine(25f);

        yield return new WaitForSeconds(1.0f);

        decisionManager?.OpenDecisionPanel(
            "A system failure has damaged the Engines. What will you do?",
            "Option 1: Sacrifice 10 crew to repair the engines.",
            "Option 2: Reduce engine efficiency by 50% to save the crew.",
            shipController
        );

        yield return new WaitUntil(() => !decisionManager.isDecisionPanelOpen);
        isEventActive = false;
    }

    private IEnumerator HandleDerelictEvent()
    {
        isEventActive = true;

        derelictShipAnim.SetTrigger("DoEvent");

        decisionManager?.OpenDecisionPanel(
            "You encounter a derelict ship. Do you want to explore it?",
            "Option 1: Explore the ship for potential resources.",
            "Option 2: Ignore it and continue your journey.",
            shipController
        );

        yield return new WaitUntil(() => !decisionManager.isDecisionPanelOpen);
        isEventActive = false;
    }
}
