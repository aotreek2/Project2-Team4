// ChapterManager.cs
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

    // Damage levels for starting state
    public float initialLifeSupportDamage = 50f; // Life Support starts at 50% health
    public float initialEngineDamage = 60f; // Engine starts at 60% health
    public float initialHullDamage = 70f; // Hull starts at 70% health

    private Scene scene;
    public Animator crossFade;

    void Start()
    {
        scene = SceneManager.GetActiveScene();
        DetermineCurrentChapter();
        InitializeComponents();
        StartCoroutine(StartChapterSequence());
    }

    /// <summary>
    /// Determines the current chapter based on the active scene's name.
    /// Ensure that your scene names correspond to "Chapter1Scene", "Chapter2Scene", etc.
    /// </summary>
    private void DetermineCurrentChapter()
    {
        // Map scene names to chapters
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

    /// <summary>
    /// Initializes component references, ensuring they are assigned either via the Inspector or found in the scene.
    /// </summary>
    private void InitializeComponents()
    {
        if (shipController == null)
            shipController = FindObjectOfType<ShipController>();
        else
            Debug.Log("[InitializeComponents] ShipController assigned via Inspector.");

        if (decisionPanelManager == null)
            decisionPanelManager = FindObjectOfType<DecisionPanelManager>();
        else
            Debug.Log("[InitializeComponents] DecisionPanelManager assigned via Inspector.");

        if (dialogueManager == null)
            dialogueManager = FindObjectOfType<DialogueManager>();
        else
            Debug.Log("[InitializeComponents] DialogueManager assigned via Inspector.");

        if (chapterIntroUI == null)
            chapterIntroUI = FindObjectOfType<ChapterIntroUI>();
        else
            Debug.Log("[InitializeComponents] ChapterIntroUI assigned via Inspector.");

        if (crossFade == null)
            crossFade = GetComponent<Animator>(); // Assuming crossFade is part of the same GameObject
        else
            Debug.Log("[InitializeComponents] CrossFade Animator assigned via Inspector.");

        // Verify all components are assigned
        if (shipController == null)
            Debug.LogError("[InitializeComponents] ShipController is not assigned or found in the scene.");

        if (decisionPanelManager == null)
            Debug.LogError("[InitializeComponents] DecisionPanelManager is not assigned or found in the scene.");

        if (dialogueManager == null)
            Debug.LogError("[InitializeComponents] DialogueManager is not assigned or found in the scene.");

        if (chapterIntroUI == null)
            Debug.LogError("[InitializeComponents] ChapterIntroUI is not assigned or found in the scene.");

        if (crossFade == null)
            Debug.LogError("[InitializeComponents] CrossFade Animator is not assigned.");
    }

    /// <summary>
    /// Starts the chapter sequence based on the current chapter.
    /// </summary>
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

    /// <summary>
    /// Coroutine for Chapter 1 sequence.
    /// </summary>
    private IEnumerator ChapterOne()
    {
        currentChapter = Chapter.Chapter1;

        // Damage the ship systems at the start of Chapter 1
        ApplyInitialDamageToSystems();

        // Display Chapter 1 title and subtext with cinematic intro
        yield return StartCoroutine(chapterIntroUI.DisplayChapterIntro("CHAPTER 1", "Repair Critical Systems"));

        // Show additional guidance dialogue after Chapter UI is done
        string[] dialogueLines = new string[]
        {
            "Alert! Critical systems are damaged.",
            "You must repair the Life Support, Engines, and Hull to proceed.",
            "Select a crew member and assign them to a system to begin repairs."
        };

        // Start the dialogue
        dialogueManager.StartDialogue(dialogueLines, CubeInteraction.SystemType.LifeSupport);

        // Wait for the dialogue to finish
        yield return new WaitUntil(() => !dialogueManager.isDialogueActive);
    }

    /// <summary>
    /// Applies initial damage to the ship systems to simulate a damaged state.
    /// </summary>
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

    /// <summary>
    /// Coroutine that waits until all critical systems are fully repaired.
    /// </summary>
    private IEnumerator WaitUntilSystemsFullyRepaired()
    {
        // Wait for all critical systems to be repaired to 100%
        yield return new WaitUntil(() =>
            shipController.lifeSupportController.lifeSupportHealth >= shipController.lifeSupportController.lifeSupportMaxHealth &&
            shipController.engineSystemController.engineHealth >= shipController.engineSystemController.engineMaxHealth &&
            shipController.hullSystemController.hullHealth >= shipController.hullSystemController.hullMaxHealth
        );

        Debug.Log("[WaitUntilSystemsFullyRepaired] All critical systems have been repaired.");

        // Proceed to Chapter 2
        yield return StartCoroutine(ChapterTwo());

        // Proceed to load the next level after Chapter 2
        LoadNextLevel();
    }

    /// <summary>
    /// Coroutine for Chapter 2 sequence.
    /// </summary>
    private IEnumerator ChapterTwo()
    {
        currentChapter = Chapter.Chapter2;

        // Display Chapter 2 intro with cinematic text
        yield return StartCoroutine(chapterIntroUI.DisplayChapterIntro("CHAPTER 2", "Asteroid Field Ahead"));

        // Trigger asteroid event decision after intro
        decisionPanelManager.OpenDecisionPanel(
            "An asteroid field is ahead! How do you proceed?",
            "Divert power to shields (Sacrifice 5 crew)",
            "Navigate through carefully (Risk hull damage)",
            shipController
        );

        // Wait for the decision to be made
        yield return new WaitUntil(() => decisionPanelManager.IsDecisionMade);

        // Handle the outcome of the decision here
        HandleAsteroidDecision();

        // Proceed to the next chapter after a short delay
        yield return new WaitForSeconds(2f);
    }

    /// <summary>
    /// Coroutine for Chapter 3 sequence.
    /// </summary>
    private IEnumerator ChapterThree()
    {
        currentChapter = Chapter.Chapter3;

        // Implement Chapter 3 sequence here
        yield break;
    }

    /// <summary>
    /// Coroutine for Chapter 4 sequence.
    /// </summary>
    private IEnumerator ChapterFour()
    {
        currentChapter = Chapter.Chapter4;

        // Implement Chapter 4 sequence here
        yield break;
    }

    /// <summary>
    /// Handles the outcome of the asteroid field decision made in Chapter 2.
    /// </summary>
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
                    // Divert power to shields: Sacrifice 5 crew
                    shipController.SacrificeCrew(5);
                    // Optionally, provide feedback to the player
                    Debug.Log("[HandleAsteroidDecision] Diverted power to shields. Sacrificed 5 crew members.");
                }
                else if (selectedOption == DecisionPanelManager.DecisionOption.Option2)
                {
                    // Navigate through carefully: Risk hull damage
                    shipController.ApplyHullDamage(20f); // Now defined in ShipController
                    Debug.Log("[HandleAsteroidDecision] Navigated carefully. Applied 20% hull damage risk.");
                }
                break;

            // Implement cases for Chapter3 and Chapter4 if needed
            default:
                Debug.LogWarning("[HandleAsteroidDecision] Decision made in an unsupported chapter.");
                break;
        }
    }

    /// <summary>
    /// Loads the next level with a crossfade animation.
    /// </summary>
    public void LoadNextLevel()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        StartCoroutine(LevelAnimation(nextSceneIndex));
    }

    /// <summary>
    /// Coroutine to handle crossfade animation before loading the next scene.
    /// </summary>
    /// <param name="index">Build index of the next scene.</param>
    private IEnumerator LevelAnimation(int index)
    {
        if (crossFade != null)
        {
            crossFade.SetTrigger("start");
            Debug.Log("[LevelAnimation] Crossfade animation triggered.");
        }
        else
        {
            Debug.LogWarning("[LevelAnimation] CrossFade Animator is not assigned.");
        }

        yield return new WaitForSeconds(2.0f); // Wait for the crossfade animation to complete

        if (index < SceneManager.sceneCountInBuildSettings)
        {
            Debug.Log($"[LevelAnimation] Loading next scene at index {index}.");
            SceneManager.LoadScene(index);
        }
        else
        {
            Debug.LogError("[LevelAnimation] Next scene index is out of range.");
        }
    }
}
