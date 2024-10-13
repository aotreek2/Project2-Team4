using UnityEngine;
using System.Collections;

public class ChapterManager : MonoBehaviour
{
    public enum Chapter { Chapter1, Chapter2, Chapter3, Chapter4 }
    public Chapter currentChapter = Chapter.Chapter1;

    public ShipController shipController;
    public DecisionPanelManager decisionPanelManager;

    void Start()
    {
        if (shipController == null)
            shipController = FindObjectOfType<ShipController>();

        if (decisionPanelManager == null)
            decisionPanelManager = FindObjectOfType<DecisionPanelManager>();

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

        // Trigger initial ship damage
        shipController.DamageShipAtStart();

        // Wait for player to repair critical systems
        yield return new WaitUntil(() => shipController.AreCriticalSystemsRepaired());

        // Proceed to the next chapter
        yield return new WaitForSeconds(2f);
    }

    private IEnumerator ChapterTwo()
    {
        currentChapter = Chapter.Chapter2;
        Debug.Log("Chapter Two started.");

        // Trigger asteroid event
        decisionPanelManager.OpenDecisionPanel(
            "An asteroid field is ahead! How do you proceed?",
            "Divert power to shields (Sacrifice 5 crew)",
            "Navigate through carefully (Risk hull damage)",
            shipController
        );

        // Wait for the decision to be made
        yield return new WaitUntil(() => decisionPanelManager.IsDecisionMade);

        // Proceed to the next chapter
        yield return new WaitForSeconds(2f);
    }

    private IEnumerator ChapterThree()
    {
        currentChapter = Chapter.Chapter3;
        Debug.Log("Chapter Three started.");

        // Trigger black hole event
        decisionPanelManager.OpenDecisionPanel(
            "A black hole is pulling the ship! What's your choice?",
            "Boost engines to escape (Use extra fuel)",
            "Ride the gravitational pull (Risk engine failure)",
            shipController
        );

        // Wait for the decision to be made
        yield return new WaitUntil(() => decisionPanelManager.IsDecisionMade);

        // Proceed to the next chapter
        yield return new WaitForSeconds(2f);
    }

    private IEnumerator ChapterFour()
    {
        currentChapter = Chapter.Chapter4;
        Debug.Log("Chapter Four started.");

        // Trigger the final event
        decisionPanelManager.OpenDecisionPanel(
            "You've reached the Lighthouse! Final decision?",
            "Dock safely (End the game)",
            "Broadcast a message and leave (New journey)",
            shipController
        );

        // Wait for the decision to be made
        yield return new WaitUntil(() => decisionPanelManager.IsDecisionMade);

        // End the game or start a new journey based on the decision
        // Implement your end-game logic here
        Debug.Log("Game Over. Thank you for playing!");
    }
}
