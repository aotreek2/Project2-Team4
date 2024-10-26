using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class ChapterManager : MonoBehaviour
{
    public enum Chapter { Chapter1, Chapter2, Chapter3, Chapter4 }
    public Chapter currentChapter = Chapter.Chapter1;

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

        if (shipController == null)
            shipController = FindObjectOfType<ShipController>();

        if (decisionPanelManager == null)
            decisionPanelManager = FindObjectOfType<DecisionPanelManager>();

        if (dialogueManager == null)
            dialogueManager = FindObjectOfType<DialogueManager>();

        if (chapterIntroUI == null)
            chapterIntroUI = FindObjectOfType<ChapterIntroUI>();

        StartCoroutine(StartChapterSequence());
    }

    private IEnumerator StartChapterSequence()
    {
        yield return StartCoroutine(ChapterOne());
        yield return StartCoroutine(WaitUntilSystemsFullyRepaired());

        // No need to call ChapterTwo() here since it's called in WaitUntilSystemsFullyRepaired()
    }

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

    // Apply initial damage to the systems to simulate a damaged state
    private void ApplyInitialDamageToSystems()
    {
        shipController.lifeSupportController.DamageLifeSupport(initialLifeSupportDamage);
        shipController.engineSystemController.DamageEngine(initialEngineDamage);
        shipController.hullSystemController.DamageHull(initialHullDamage);
    }

    private IEnumerator WaitUntilSystemsFullyRepaired()
    {
        // Wait for all critical systems to be repaired to 100%
        yield return new WaitUntil(() =>
            shipController.lifeSupportController.lifeSupportHealth >= shipController.lifeSupportController.lifeSupportMaxHealth &&
            shipController.engineSystemController.engineHealth >= shipController.engineSystemController.engineMaxHealth &&
            shipController.hullSystemController.hullHealth >= shipController.hullSystemController.hullMaxHealth
        );

        // Now, display Chapter 2 dialogue
        yield return StartCoroutine(ChapterTwo());

        // Now, proceed to load the next level
        LoadNextLevel();
    }

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

        // Optionally, handle the outcome of the decision here

        // Proceed to the next chapter after a short delay
        yield return new WaitForSeconds(2f);
    }

    public void LoadNextLevel()
    {
        StartCoroutine(LevelAnimation(SceneManager.GetActiveScene().buildIndex + 1));
    }

    private IEnumerator LevelAnimation(int index)
    {
        crossFade.SetTrigger("start");
        yield return new WaitForSeconds(2.0f);

        SceneManager.LoadScene(index);
    }
}
