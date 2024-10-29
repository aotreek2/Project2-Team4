using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class ChapterManager : MonoBehaviour
{
    public enum Chapter { Chapter1, Chapter2, Chapter3, Chapter4 }
    public Chapter currentChapter = Chapter.Chapter1; // Default to Chapter1, will be overridden based on scene

    public ShipController shipController;
    public DecisionPanelManager decisionPanelManager;
    public DialogueManager dialogueManager; // Reference to DialogueManager for dialogue control
    public ChapterIntroUI chapterIntroUI; // Reference to ChapterIntroUI for cinematic intro
    public GameObject mainCanvas; // Reference to the main canvas to hide/show

    // Damage levels for starting state
    public float initialLifeSupportDamage = 50f; // Life Support starts at 50% health
    public float initialEngineDamage = 60f; // Engine starts at 60% health
    public float initialHullDamage = 70f; // Hull starts at 70% health

    private Scene scene;

    // Array of asteroid animations
    public Animator[] asteroidEventAnim;

    void Start()
    {
        scene = SceneManager.GetActiveScene();
        DetermineCurrentChapter();
        InitializeComponents();
        StartCoroutine(StartChapterSequence());
    }

    private void Update()
    {
        // Load Chapter 2 when F2 is pressed
        if (Input.GetKeyDown(KeyCode.F2))
        {
            LoadNextLevel("Chapter2Scene");
        }
        // Load Chapter 3 when F3 is pressed
        else if (Input.GetKeyDown(KeyCode.F3))
        {
            LoadNextLevel("Chapter3Scene");
        }
        // Load Chapter 4 when F4 is pressed
        else if (Input.GetKeyDown(KeyCode.F4))
        {
            LoadNextLevel("Chapter4Scene");
        }
    }

    private void DetermineCurrentChapter()
    {
        switch (scene.name)
        {
            case "Chapter1Scene":
                currentChapter = Chapter.Chapter1;
                break;
            case "Chapter2Scene":
                currentChapter = Chapter.Chapter2;
                break;
            case "Chapter3Scene":
                currentChapter = Chapter.Chapter3;
                break;
            case "Chapter4Scene":
                currentChapter = Chapter.Chapter4;
                break;
            default:
                Debug.LogWarning($"[DetermineCurrentChapter] Unknown scene name '{scene.name}'. Defaulting to Chapter1.");
                currentChapter = Chapter.Chapter1;
                break;
        }

        Debug.Log($"[DetermineCurrentChapter] Current Chapter set to: {currentChapter}");
    }

    private void InitializeComponents()
    {
        if (shipController == null)
            shipController = FindObjectOfType<ShipController>();

        if (decisionPanelManager == null)
            decisionPanelManager = FindObjectOfType<DecisionPanelManager>();

        if (dialogueManager == null)
            dialogueManager = FindObjectOfType<DialogueManager>();

        if (chapterIntroUI == null)
            chapterIntroUI = FindObjectOfType<ChapterIntroUI>();

        if (mainCanvas == null)
            mainCanvas = GameObject.Find("MainCanvas"); // Assuming the main canvas is named "MainCanvas"

        // asteroidEventAnim is assigned via the Unity Inspector, no need to initialize here
    }

    private IEnumerator StartChapterSequence()
    {
        switch (currentChapter)
        {
            case Chapter.Chapter1:
                yield return StartCoroutine(ChapterOne());
                yield return StartCoroutine(WaitUntilSystemsFullyRepaired());
                break;
            case Chapter.Chapter2:
                yield return StartCoroutine(ChapterTwo());
                break;
            case Chapter.Chapter3:
                yield return StartCoroutine(ChapterThree());
                break;
            case Chapter.Chapter4:
                yield return StartCoroutine(ChapterFour());
                break;
            default:
                Debug.LogError("[StartChapterSequence] Invalid chapter state.");
                break;
        }
    }

    private IEnumerator ChapterOne()
    {
        currentChapter = Chapter.Chapter1;

        ApplyInitialDamageToSystems();

        // Hide the main canvas during the chapter intro
        if (mainCanvas != null)
        {
            mainCanvas.SetActive(false);
        }

        yield return StartCoroutine(chapterIntroUI.DisplayChapterIntro("CHAPTER 1", "Repair Critical Systems"));

        // Re-enable the main canvas after the chapter intro
        if (mainCanvas != null)
        {
            mainCanvas.SetActive(true);
        }

        string[] dialogueLines = new string[]
        {
            "Alert! Critical systems are damaged.",
            "You must repair the Life Support, Engines, and Hull to proceed.",
            "Select a crew member and assign them to a system to begin repairs."
        };

        dialogueManager.StartDialogue(dialogueLines, CubeInteraction.SystemType.LifeSupport);

        yield return new WaitUntil(() => !dialogueManager.isDialogueActive);
    }

    private void ApplyInitialDamageToSystems()
    {
        if (shipController != null)
        {
            shipController.lifeSupportController.DamageLifeSupport(initialLifeSupportDamage);
            shipController.engineSystemController.DamageEngine(initialEngineDamage);
            shipController.hullSystemController.DamageHull(initialHullDamage);
            Debug.Log("[ApplyInitialDamageToSystems] Initial damage applied to Life Support, Engines, and Hull.");
        }
        else
        {
            Debug.LogError("[ApplyInitialDamageToSystems] ShipController is not assigned.");
        }
    }

    private IEnumerator WaitUntilSystemsFullyRepaired()
    {
        yield return new WaitUntil(() =>
            shipController.lifeSupportController.lifeSupportHealth >= shipController.lifeSupportController.lifeSupportMaxHealth &&
            shipController.engineSystemController.engineHealth >= shipController.engineSystemController.engineMaxHealth &&
            shipController.hullSystemController.hullHealth >= shipController.hullSystemController.hullMaxHealth
        );

        Debug.Log("[WaitUntilSystemsFullyRepaired] All critical systems have been repaired. Proceeding to the next chapter.");
    }

    private IEnumerator ChapterTwo()
    {
        currentChapter = Chapter.Chapter2;

        if (mainCanvas != null)
        {
            mainCanvas.SetActive(false);
        }

        yield return StartCoroutine(chapterIntroUI.DisplayChapterIntro("CHAPTER 2", "Asteroid Field Ahead"));

        if (mainCanvas != null)
        {
            mainCanvas.SetActive(true);
        }

        // Display decision panel for the asteroid field
        decisionPanelManager.OpenDecisionPanel(
            "An asteroid field is ahead! How do you proceed?",
            "Divert power to shields (Sacrifice 5 crew)",
            "Navigate through carefully (Risk hull damage)",
            shipController
        );

        // Wait until the player makes a decision
        yield return new WaitUntil(() => decisionPanelManager.IsDecisionMade);

        // Handle the decision outcome
        HandleAsteroidDecision();

        // Trigger the asteroid animation event
        yield return StartCoroutine(TriggerAsteroidAnimationEvent());

        // Wait until all critical systems are fully repaired before progressing to Chapter 3
        yield return StartCoroutine(WaitUntilSystemsFullyRepaired());

        // Now that both the asteroid decision and repairs are complete, load Chapter 3
        LoadNextLevel("Chapter3Scene");
    }

    private IEnumerator TriggerAsteroidAnimationEvent()
    {
        if (asteroidEventAnim != null && asteroidEventAnim.Length > 0)
        {
            int asteroidSpawn = Random.Range(0, asteroidEventAnim.Length);
            Animator selectedAnimator = asteroidEventAnim[asteroidSpawn];

            selectedAnimator.SetTrigger("DoEvent"); // Ensure your Animator has this trigger

            Debug.Log("[TriggerAsteroidAnimationEvent] Asteroid animation triggered.");

            // Optionally, wait for the animation to complete
            // If you have specific durations, you can use WaitForSeconds with known values
            yield return null; // Remove this if you add a wait
        }
        else
        {
            Debug.LogWarning("[TriggerAsteroidAnimationEvent] No asteroid animations assigned.");
        }
    }

    private IEnumerator ChapterThree()
    {
        currentChapter = Chapter.Chapter3;

        // Implement Chapter 3 sequence here
        yield break;
    }

    private IEnumerator ChapterFour()
    {
        currentChapter = Chapter.Chapter4;

        // Implement Chapter 4 sequence here
        yield break;
    }

    private void HandleAsteroidDecision()
    {
        if (decisionPanelManager == null)
        {
            Debug.LogError("[HandleAsteroidDecision] DecisionPanelManager is not assigned.");
            return;
        }

        DecisionPanelManager.DecisionOption selectedOption = decisionPanelManager.SelectedOption;

        switch (currentChapter)
        {
            case Chapter.Chapter2:
                if (selectedOption == DecisionPanelManager.DecisionOption.Option1)
                {
                    shipController.SacrificeCrew(5);
                    Debug.Log("[HandleAsteroidDecision] Diverted power to shields. Sacrificed 5 crew members.");
                }
                else if (selectedOption == DecisionPanelManager.DecisionOption.Option2)
                {
                    shipController.ApplyHullDamage(20f);
                    Debug.Log("[HandleAsteroidDecision] Navigated carefully. Applied 20% hull damage risk.");
                }
                break;

            default:
                Debug.LogWarning("[HandleAsteroidDecision] Decision made in an unsupported chapter.");
                break;
        }
    }

    /// <summary>
    /// Loads the next level/scene without a fade animation.
    /// </summary>
    /// <param name="sceneName">Name of the scene to load.</param>
    public void LoadNextLevel(string sceneName)
    {
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.Log($"[LoadNextLevel] Loading scene '{sceneName}' directly.");
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError($"[LoadNextLevel] Scene '{sceneName}' is not in the build settings.");
        }
    }
}
