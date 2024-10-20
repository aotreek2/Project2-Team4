using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    public CanvasGroup dialogueCanvasGroup; // For fade-in and fade-out
    public RectTransform dialoguePanel; // Reference to the RectTransform of the dialogue panel
    public TMP_Text dialogueText; // Dialogue text (using TextMeshPro)
    public TMP_Text continueText; // "Click to Continue" or "Press Space to Continue" text
    public TMP_Text systemHealthText; // Text for displaying system health
    public Slider systemHealthSlider; // Slider for visual system health
    public TMP_Text selectedSystemText; // Text to display the currently selected system name
    public GameObject historyPanel; // Panel to display dialogue history
    public TMP_Text historyText; // Text component for dialogue history
    public Button showHistoryButton; // Button to show history
    public Button closeHistoryButton; // Button to close history
    public float typingSpeed = 0.05f; // Speed at which characters are typed out
    public float moveDuration = 2f; // Duration to move the panel from the bottom-center to the top-right

    private List<string> dialogueHistory = new List<string>(); // Stores previous dialogue lines
    public bool isDialogueActive { get; private set; }
    private bool isIntroCompleted = false; // Flag to indicate if the intro dialogue is finished

    void Start()
    {
        if (dialogueCanvasGroup == null)
        {
            dialogueCanvasGroup = GetComponent<CanvasGroup>();
        }

        // Ensure the panel is fully visible (alpha = 1)
        dialogueCanvasGroup.alpha = 1f;
        dialogueCanvasGroup.interactable = true; 
        dialogueCanvasGroup.blocksRaycasts = true; 
        Debug.Log("Dialogue panel visibility set to alpha 1, interactable, and raycasts enabled");

        continueText.alpha = 0; // Hide "Click to Continue" text at the start
        isDialogueActive = false;

        // Hide the history panel and system UI elements at the start
        historyPanel.SetActive(false);
        systemHealthSlider.gameObject.SetActive(false); // Hide the health bar slider
        systemHealthText.gameObject.SetActive(false);   // Hide the system health text
        selectedSystemText.gameObject.SetActive(false); // Hide the selected system text

        // Assign listeners to the buttons
        showHistoryButton.onClick.AddListener(ShowHistory);
        closeHistoryButton.onClick.AddListener(HideHistory);

        // Set anchors to center-bottom, so the panel stays at the bottom center
        dialoguePanel.anchorMin = new Vector2(0.5f, 0); // Center-bottom
        dialoguePanel.anchorMax = new Vector2(0.5f, 0); // Center-bottom
        
        // Set the starting position at bottom-center (X = 0, Y = 350)
        dialoguePanel.anchoredPosition = new Vector2(0, 350);  // Start at bottom-center
        Debug.Log("Dialogue panel anchored position set to bottom-center: X=0, Y=350");

        // Ensure the panel is properly sized and visible on screen
        dialoguePanel.gameObject.SetActive(true); // Ensure the panel itself is active
    }


    // Display dialogue history
    public void ShowHistory()
    {
        // Only show history after the intro is completed
        if (isIntroCompleted)
        {
            historyPanel.SetActive(true);
            historyText.text = string.Join("\n", dialogueHistory.ToArray());
        }
    }

    // Hide dialogue history
    public void HideHistory()
    {
        historyPanel.SetActive(false);
    }

    // Add the dialogue to history after each sentence
    private void AddToHistory(string sentence)
    {
        dialogueHistory.Add(sentence);
        if (dialogueHistory.Count > 100)
        {
            dialogueHistory.RemoveAt(0); // Remove oldest dialogue when the list grows too long
        }
    }

    // Start a dialogue and add the lines to history
    public void StartDialogue(string[] lines, CubeInteraction.SystemType systemType)
    {
        if (isDialogueActive) return; // Prevent starting if dialogue is already active
        string contextInfo = GetSystemDescription(systemType);
        List<string> fullDialogue = new List<string>(lines);
        fullDialogue.Insert(0, contextInfo); // Add the system description as the first line

        StartCoroutine(DisplayDialogueSequence(fullDialogue.ToArray()));

        // Add lines to history
        foreach (var line in fullDialogue)
        {
            AddToHistory(line);
        }
    }

    // Shows the system health bar and selected system name, only after the intro is done
    public void ShowSystemUI(CubeInteraction.SystemType systemType)
    {
        if (!isIntroCompleted) return; // Ensure the intro is completed before showing system UI

        // Update the selected system name
        selectedSystemText.gameObject.SetActive(true);
        selectedSystemText.text = $"Selected System: {systemType.ToString()}";

        // Show the health bar and update its value
        systemHealthSlider.gameObject.SetActive(true);
        systemHealthText.gameObject.SetActive(true);

        // Set the system health for the selected system
        float systemHealth = GetSystemHealth(systemType);
        systemHealthText.text = $"System Health: {systemHealth}%";
        systemHealthSlider.value = systemHealth / 100f;
    }

    // Hide system UI when no system is selected
    public void HideSystemUI()
    {
        selectedSystemText.gameObject.SetActive(false);
        systemHealthSlider.gameObject.SetActive(false);
        systemHealthText.gameObject.SetActive(false);
    }

    // Get system description based on the selected system
    private string GetSystemDescription(CubeInteraction.SystemType systemType)
    {
        switch (systemType)
        {
            case CubeInteraction.SystemType.Engines:
                return "The Engines are critical for propulsion. Keep them in good condition to ensure travel.";
            case CubeInteraction.SystemType.LifeSupport:
                return "Life Support maintains the oxygen supply. A failure here could be catastrophic.";
            case CubeInteraction.SystemType.Hull:
                return "The Hull protects the ship from external threats. Make sure it's fully repaired.";
            case CubeInteraction.SystemType.Generator:
                return "The Generator supplies power. If it fails, many systems will stop functioning.";
            default:
                return "Unknown system.";
        }
    }

    // Simulate system health (in a real game, you'd pull this from your system's data)
    private float GetSystemHealth(CubeInteraction.SystemType systemType)
    {
        // Placeholder: Return simulated health percentage (replace with actual system health in your game)
        return Random.Range(10f, 100f);
    }

    // Typing effect for displaying one sentence
    private IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";  // Clear the text before typing starts
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;  // Add one letter at a time
            yield return new WaitForSeconds(typingSpeed);  // Wait for typing speed interval
        }
    }

    // Displays the dialogue sequence
    private IEnumerator DisplayDialogueSequence(string[] lines)
    {
        isDialogueActive = true;

        foreach (var line in lines)
        {
            yield return StartCoroutine(TypeSentence(line)); // Display each line with typing effect

            // Show "Click to Continue" text after sentence is fully typed
            continueText.alpha = 1f;

            // Wait for player input to continue
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0));

            // Hide "Click to Continue" text
            continueText.alpha = 0f;
        }

        isDialogueActive = false;
        isIntroCompleted = true; // Mark the intro as completed

        // Now move the dialogue panel to the top-right (950 left, -355 top, Z = 0)
        StartCoroutine(MoveDialoguePanelToTopRight());
    }

    // Coroutine to move the dialogue panel to the top-right (950 left, -355 top)
    private IEnumerator MoveDialoguePanelToTopRight()
    {
        // Set the initial anchor and position to the bottom-center
        dialoguePanel.anchorMin = new Vector2(0.5f, 0);  // Bottom-center
        dialoguePanel.anchorMax = new Vector2(0.5f, 0);  // Bottom-center

        Vector2 startPosition = dialoguePanel.anchoredPosition;  // Start at bottom-center (e.g., X=0, Y=350)
        Vector2 intermediatePosition = new Vector2(0, 800);  // Move upward to Y=800

        float elapsedTime = 0f;

        // Move to the intermediate position (rising up) to create smooth upward motion
        while (elapsedTime < moveDuration / 2)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / (moveDuration / 2);
            dialoguePanel.anchoredPosition = Vector2.Lerp(startPosition, intermediatePosition, t);
            yield return null;
        }

        // Update anchors to the top-right gradually
        Vector2 startAnchorMin = dialoguePanel.anchorMin;
        Vector2 startAnchorMax = dialoguePanel.anchorMax;
        Vector2 endAnchorMin = new Vector2(1, 1);  // Target top-right
        Vector2 endAnchorMax = new Vector2(1, 1);  // Target top-right

        startPosition = dialoguePanel.anchoredPosition;  // Set start position after intermediate position
        Vector2 endPosition = new Vector2(-450, -355);  // Final position at top-right (X = -450, Y = -355)

        elapsedTime = 0f;

        // Move to the final position at the top-right
        while (elapsedTime < moveDuration / 2)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / (moveDuration / 2);

            // Gradually adjust the anchors so it doesn’t snap from the center to top-right
            dialoguePanel.anchorMin = Vector2.Lerp(startAnchorMin, endAnchorMin, t);
            dialoguePanel.anchorMax = Vector2.Lerp(startAnchorMax, endAnchorMax, t);

            // Interpolate the position while the anchors are moving
            dialoguePanel.anchoredPosition = Vector2.Lerp(startPosition, endPosition, t);

            yield return null;
        }

        // Ensure the final position and anchors are set correctly
        dialoguePanel.anchorMin = endAnchorMin;
        dialoguePanel.anchorMax = endAnchorMax;
        dialoguePanel.anchoredPosition = endPosition;

        Debug.Log("Dialogue panel finished moving to top-right.");
    }

}
