using UnityEngine;
using System.Collections;

public class ChapterManager : MonoBehaviour
{
    public enum Chapter { Chapter1, Chapter2, Chapter3, Chapter4 }
    public Chapter currentChapter = Chapter.Chapter1;

    public ShipController shipController;
    public DecisionPanelManager decisionPanelManager;
    public DialogueManager dialogueManager; // Reference to DialogueManager for dialogue control
    public SystemHighlighter systemHighlighter; // Reference to a system highlighting manager
    public ChapterIntroUI chapterIntroUI; // Reference to ChapterIntroUI for cinematic intro

    void Start()
    {
        if (shipController == null)
            shipController = FindObjectOfType<ShipController>();

        if (decisionPanelManager == null)
            decisionPanelManager = FindObjectOfType<DecisionPanelManager>();

        if (dialogueManager == null)
            dialogueManager = FindObjectOfType<DialogueManager>();

        if (systemHighlighter == null)
            systemHighlighter = FindObjectOfType<SystemHighlighter>();

        if (chapterIntroUI == null)
            chapterIntroUI = FindObjectOfType<ChapterIntroUI>();

        StartCoroutine(StartChapterSequence());
    }

    private IEnumerator StartChapterSequence()
    {
        yield return StartCoroutine(ChapterOne());
        yield return StartCoroutine(ChapterTwo());
        yield return StartCoroutine(ChapterThree());
        yield return StartCoroutine(ChapterFour());
    }

    private IEnumerator ChapterOne()
    {
        currentChapter = Chapter.Chapter1;
        Debug.Log("Chapter One started.");

        // Cinematic display of Chapter 1 title and subtext with fog
        chapterIntroUI.DisplayChapterIntro("CHAPTER 1", "Repair Critical Systems");

        // Wait for a few seconds for the intro to finish
        yield return new WaitForSeconds(chapterIntroUI.fadeDuration + chapterIntroUI.displayDuration);

        // Show additional guidance dialogue
        string[] dialogueLines = new string[]
        {
            "Alert! Critical systems are damaged.",
            "You must repair the Life Support, Engines, and Hull to proceed.",
            "Select a crew member and assign them to a system to begin repairs."
        };
        dialogueManager.StartDialogue(dialogueLines);
        yield return new WaitUntil(() => !dialogueManager.isDialogueActive);

        // Highlight critical systems to be repaired
        systemHighlighter.HighlightSystem(shipController.lifeSupportController.gameObject);
        systemHighlighter.HighlightSystem(shipController.engineSystemController.gameObject);
        systemHighlighter.HighlightSystem(shipController.hullSystemController.gameObject);

        // Wait for player to repair critical systems
        yield return new WaitUntil(() => shipController.AreCriticalSystemsRepaired());

        // Stop highlighting once systems are repaired
        systemHighlighter.StopHighlighting(shipController.lifeSupportController.gameObject);
        systemHighlighter.StopHighlighting(shipController.engineSystemController.gameObject);
        systemHighlighter.StopHighlighting(shipController.hullSystemController.gameObject);

        // Proceed to the next chapter after a short delay
        yield return new WaitForSeconds(2f);
    }

    private IEnumerator ChapterTwo()
    {
        currentChapter = Chapter.Chapter2;
        Debug.Log("Chapter Two started.");

        // Cinematic display of Chapter 2 title and subtext with fog
        chapterIntroUI.DisplayChapterIntro("CHAPTER 2", "Asteroid Field Ahead");

        // Wait for a few seconds for the intro to finish
        yield return new WaitForSeconds(chapterIntroUI.fadeDuration + chapterIntroUI.displayDuration);

        // Trigger asteroid event decision
        decisionPanelManager.OpenDecisionPanel(
            "An asteroid field is ahead! How do you proceed?",
            "Divert power to shields (Sacrifice 5 crew)",
            "Navigate through carefully (Risk hull damage)",
            shipController
        );

        // Wait for the decision to be made
        yield return new WaitUntil(() => decisionPanelManager.IsDecisionMade);

        // Proceed to the next chapter after a short delay
        yield return new WaitForSeconds(2f);
    }

    private IEnumerator ChapterThree()
    {
        currentChapter = Chapter.Chapter3;
        Debug.Log("Chapter Three started.");

        // Cinematic display of Chapter 3 title and subtext with fog
        chapterIntroUI.DisplayChapterIntro("CHAPTER 3", "Black Hole Event");

        // Wait for a few seconds for the intro to finish
        yield return new WaitForSeconds(chapterIntroUI.fadeDuration + chapterIntroUI.displayDuration);

        // Trigger black hole event decision
        decisionPanelManager.OpenDecisionPanel(
            "A black hole is pulling the ship! What's your choice?",
            "Boost engines to escape (Use extra fuel)",
            "Ride the gravitational pull (Risk engine failure)",
            shipController
        );

        // Wait for the decision to be made
        yield return new WaitUntil(() => decisionPanelManager.IsDecisionMade);

        // Proceed to the next chapter after a short delay
        yield return new WaitForSeconds(2f);
    }

    private IEnumerator ChapterFour()
    {
        currentChapter = Chapter.Chapter4;
        Debug.Log("Chapter Four started.");

        // Cinematic display of Chapter 4 title and subtext with fog
        chapterIntroUI.DisplayChapterIntro("CHAPTER 4", "Reaching the Lighthouse");

        // Wait for a few seconds for the intro to finish
        yield return new WaitForSeconds(chapterIntroUI.fadeDuration + chapterIntroUI.displayDuration);

        // Trigger the final event decision
        decisionPanelManager.OpenDecisionPanel(
            "You've reached the Lighthouse! Final decision?",
            "Dock safely (End the game)",
            "Broadcast a message and leave (New journey)",
            shipController
        );

        // Wait for the decision to be made
        yield return new WaitUntil(() => decisionPanelManager.IsDecisionMade);

        // End the game or start a new journey based on the decision
        Debug.Log("Game Over. Thank you for playing!");
    }
}
