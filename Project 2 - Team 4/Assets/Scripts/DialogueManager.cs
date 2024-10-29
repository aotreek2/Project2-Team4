// DialogueManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [Header("UI Components")]
    public CanvasGroup dialogueCanvasGroup; // For fade-in and fade-out
    public RectTransform dialoguePanel; // Reference to the RectTransform of the dialogue panel
    public Image dialogueBackgroundImage; // Background Image for the dialogue panel
    public TMP_Text dialogueText; // Dialogue text (using TextMeshPro)
    public TMP_Text continueText; // "Click to Continue" or "Press Space to Continue" text
    public TMP_Text systemHealthText; // Text for displaying system health (system name)
    public Slider systemHealthSlider; // Slider for visual system health
    public TMP_Text selectedSystemText; // Text to display the currently selected system name

    [Header("Settings")]
    public float typingSpeed = 0.05f; // Speed at which characters are typed out
    public float fadeDuration = 0.5f; // Duration for fade in/out

    [Header("Audio Components")]
    public AudioSource dialogueAudioSource; // AudioSource for dialogue sounds
    public AudioClip typingSound; // Sound played while typing

    public bool isDialogueActive { get; private set; }
    private bool isIntroCompleted = false; // Flag to indicate if the intro dialogue is finished
    private bool isTyping = false; // Flag to indicate if typing is in progress
    private Coroutine typingCoroutine; // Reference to the current typing coroutine

    // Dialogue data structured by chapter and event
    private Dictionary<string, List<string>> dialogues = new Dictionary<string, List<string>>();

    void Start()
    {
        Debug.Log("DialogueManager: Start method called.");

        // Validate essential components
        if (dialogueCanvasGroup == null)
        {
            Debug.LogError("DialogueManager: dialogueCanvasGroup is not assigned in the Inspector.");
            return;
        }

        if (dialoguePanel == null)
        {
            Debug.LogError("DialogueManager: dialoguePanel is not assigned in the Inspector.");
            return;
        }

        if (dialogueText == null)
        {
            Debug.LogError("DialogueManager: dialogueText is not assigned in the Inspector.");
            return;
        }

        if (continueText == null)
        {
            Debug.LogError("DialogueManager: continueText is not assigned in the Inspector.");
            return;
        }

        if (dialogueBackgroundImage == null)
        {
            Debug.LogError("DialogueManager: dialogueBackgroundImage is not assigned in the Inspector.");
            return;
        }

        // Initially hide the dialogue panel
        dialogueCanvasGroup.alpha = 0f;
        dialogueCanvasGroup.interactable = false;
        dialogueCanvasGroup.blocksRaycasts = false;
        Debug.Log("DialogueManager: Dialogue panel set to alpha 0, non-interactable, and raycasts disabled.");

        continueText.alpha = 0; // Hide "Click to Continue" text at the start
        isDialogueActive = false;

        // Hide system UI elements at the start
        if (systemHealthSlider != null)
            systemHealthSlider.gameObject.SetActive(false); // Hide the health bar slider
        if (systemHealthText != null)
            systemHealthText.gameObject.SetActive(false);   // Hide the system health text
        if (selectedSystemText != null)
            selectedSystemText.gameObject.SetActive(false); // Hide the selected system text

        // Set anchors to top-right, so the panel stays at the top-right
        dialoguePanel.anchorMin = new Vector2(1, 1); // Top-right
        dialoguePanel.anchorMax = new Vector2(1, 1); // Top-right

        // Set the starting position at top-right (adjust as needed)
        dialoguePanel.anchoredPosition = new Vector2(-500, -500);  // Start off-screen
        Debug.Log("DialogueManager: Dialogue panel anchored position set off-screen: X=-500, Y=-500");

        // Ensure the panel is properly sized and visible on screen
        dialoguePanel.gameObject.SetActive(true); // Ensure the panel itself is active
        Debug.Log("DialogueManager: Dialogue panel gameObject set to active.");

        // Ensure the background image has a semi-transparent color
        if (dialogueBackgroundImage != null)
        {
            dialogueBackgroundImage.color = new Color(0, 0, 0, 0.7f); // Semi-transparent black
            Debug.Log("DialogueManager: Dialogue background image color set to semi-transparent black.");
        }

        // Initialize all dialogues
        InitializeDialogues();
    }

    /// <summary>
    /// Initializes all dialogues for different chapters and events.
    /// </summary>
    private void InitializeDialogues()
    {
        Debug.Log("DialogueManager: Initializing dialogues.");

        // Chapter 1 Dialogues
        dialogues.Add("Chapter1_Event1", new List<string>
        {
            "Welcome aboard, Captain.",
            "Our mission is to reach the lighthouse safely.",
            "Ensure all systems are operational."
        });

        // Chapter 2 Dialogues - Asteroid Field Event
        dialogues.Add("Chapter2_AsteroidField", new List<string>
        {
            "Captain, sensors have detected an incoming asteroid field.",
            "Navigating through it poses significant risks.",
            "We need to decide our course of action swiftly."
        });

        // Chapter 3 Dialogues
        dialogues.Add("Chapter3_Event1", new List<string>
        {
            "A gravitational anomaly is affecting our trajectory.",
            "We must adjust the engines to compensate."
        });

        // Chapter 4 Dialogues
        dialogues.Add("Chapter4_Event1", new List<string>
        {
            "We've reached the final approach.",
            "Prepare for docking or set a new course for exploration."
        });

        // Add more dialogues as needed
    }

    /// <summary>
    /// Start a general dialogue
    /// </summary>
    public void StartDialogue(string[] lines, CubeInteraction.SystemType systemType)
    {
        Debug.Log("DialogueManager: StartDialogue called.");
        if (isDialogueActive)
        {
            Debug.LogWarning("DialogueManager: Dialogue is already active. StartDialogue call ignored.");
            return; // Prevent starting if dialogue is already active
        }

        string contextInfo = GetSystemDescription(systemType);
        List<string> fullDialogue = new List<string>(lines);
        fullDialogue.Insert(0, contextInfo); // Add the system description as the first line

        StartCoroutine(DisplayDialogueSequence(fullDialogue.ToArray()));
    }

    /// <summary>
    /// Display system information dialogue
    /// </summary>
    public void DisplaySystemInfo(string systemName, float systemHealth, float deathChance)
    {
        Debug.Log($"DialogueManager: DisplaySystemInfo called for {systemName} with Health: {systemHealth}, Death Chance: {deathChance * 100f}%");
        if (isDialogueActive)
        {
            Debug.LogWarning("DialogueManager: Dialogue is already active. DisplaySystemInfo call ignored.");
            return; // Prevent starting if dialogue is already active
        }

        string systemInfo = $"System: {systemName}\nHealth: {systemHealth}%\nCrew Death Chance: {deathChance * 100f}%";

        // Start the dialogue sequence with system info
        StartCoroutine(DisplayDialogueSequence(new string[] { systemInfo }));

        // Additionally, show the system health UI
        ShowSystemUI(systemName, systemHealth);
    }

    /// <summary>
    /// Coroutine to display dialogue sequence with typing effect
    /// </summary>
    private IEnumerator DisplayDialogueSequence(string[] lines)
    {
        Debug.Log("DialogueManager: DisplayDialogueSequence coroutine started.");
        isDialogueActive = true;

        // Fade in the dialogue panel to its target position
        yield return StartCoroutine(FadeInDialoguePanel());

        foreach (var line in lines)
        {
            Debug.Log($"DialogueManager: Displaying line - \"{line}\"");
            // Start typing the sentence
            typingCoroutine = StartCoroutine(TypeSentence(line));

            // Wait until typing is done
            while (isTyping)
            {
                yield return null;
            }

            // Show "Click to Continue" text after sentence is fully typed
            if (continueText != null)
            {
                continueText.alpha = 1f;
                Debug.Log("DialogueManager: Continue text shown.");
            }

            // Wait for player input to continue
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0));
            Debug.Log("DialogueManager: Player input detected to continue.");

            // Hide "Click to Continue" text
            if (continueText != null)
            {
                continueText.alpha = 0f;
                Debug.Log("DialogueManager: Continue text hidden.");
            }
        }

        // Fade out the dialogue panel
        yield return StartCoroutine(FadeOutDialoguePanel());

        isDialogueActive = false;
        Debug.Log("DialogueManager: Dialogue sequence completed. isDialogueActive set to false.");

        // Mark the intro as completed if it was an intro dialogue
        if (lines.Length > 1) // Assuming intro has multiple lines
        {
            isIntroCompleted = true;
            Debug.Log("DialogueManager: Intro dialogue marked as completed.");
        }
    }

    /// <summary>
    /// Coroutine to type out the sentence with typing effect
    /// </summary>
    private IEnumerator TypeSentence(string sentence)
    {
        Debug.Log("DialogueManager: TypeSentence coroutine started.");
        isTyping = true;
        dialogueText.text = "";  // Clear the text before typing starts

        foreach (char letter in sentence.ToCharArray())
        {
            // If player presses Space or clicks, skip the typing and show the full sentence
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                dialogueText.text = sentence;
                Debug.Log("DialogueManager: Typing skipped by player input.");
                break;
            }

            dialogueText.text += letter;  // Add one letter at a time
            if (typingSound != null && dialogueAudioSource != null)
            {
                dialogueAudioSource.PlayOneShot(typingSound);
            }
            yield return new WaitForSeconds(typingSpeed);  // Wait for typing speed interval
        }

        isTyping = false;
        Debug.Log("DialogueManager: Typing completed.");
    }

    /// <summary>
    /// Coroutine to fade in the dialogue panel
    /// </summary>
    private IEnumerator FadeInDialoguePanel()
    {
        Debug.Log("DialogueManager: FadeInDialoguePanel coroutine started.");

        // Ensure the panel is active before fading in
        if (!dialoguePanel.gameObject.activeSelf)
        {
            dialoguePanel.gameObject.SetActive(true);
            Debug.Log("DialogueManager: Dialogue panel set to active.");
        }

        // Reset the alpha and make it non-interactable before starting the fade
        dialogueCanvasGroup.alpha = 0f;
        dialogueCanvasGroup.interactable = false;
        dialogueCanvasGroup.blocksRaycasts = false;
        Debug.Log("DialogueManager: CanvasGroup alpha set to 0, interactable and blocksRaycasts set to false.");

        // Fade in
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            dialogueCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }

        dialogueCanvasGroup.alpha = 1f;
        dialogueCanvasGroup.interactable = true;
        dialogueCanvasGroup.blocksRaycasts = true;

        Debug.Log("DialogueManager: Dialogue panel faded in.");
    }

    /// <summary>
    /// Coroutine to fade out the dialogue panel
    /// </summary>
    private IEnumerator FadeOutDialoguePanel()
    {
        Debug.Log("DialogueManager: FadeOutDialoguePanel coroutine started.");
        float startAlpha = dialogueCanvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            dialogueCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeDuration);
            yield return null;
        }

        dialogueCanvasGroup.alpha = 0f;
        dialogueCanvasGroup.interactable = false;
        dialogueCanvasGroup.blocksRaycasts = false;

        Debug.Log("DialogueManager: Dialogue panel faded out.");

        // Do NOT deactivate the panel to allow future dialogues
        // dialoguePanel.gameObject.SetActive(false);
    }

    /// <summary>
    /// Shows the system health bar and selected system name
    /// </summary>
    public void ShowSystemUI(string systemName, float systemHealth)
    {
        Debug.Log($"DialogueManager: ShowSystemUI called for {systemName} with Health: {systemHealth}%.");
        if (!isIntroCompleted)
        {
            Debug.LogWarning("DialogueManager: Intro not completed. System UI will not be shown.");
            return; // Ensure the intro is completed before showing system UI
        }

        // Update the selected system name
        if (selectedSystemText != null)
        {
            selectedSystemText.gameObject.SetActive(true);
            selectedSystemText.text = $"Selected System: {systemName}";
            Debug.Log($"DialogueManager: Selected system text updated to {systemName}.");
        }

        // Show the health bar and update its value
        if (systemHealthSlider != null)
        {
            systemHealthSlider.gameObject.SetActive(true);
            systemHealthSlider.value = systemHealth / 100f;
            Debug.Log($"DialogueManager: System health slider shown and value set to {systemHealth}%.");
        }
        if (systemHealthText != null)
        {
            systemHealthText.gameObject.SetActive(true);
            systemHealthText.text = $"System Health: {systemHealth}%";
            Debug.Log($"DialogueManager: System health text updated to {systemHealth}%.");
        }
    }

    /// <summary>
    /// Hide system UI when no system is selected
    /// </summary>
    public void HideSystemUI()
    {
        Debug.Log("DialogueManager: HideSystemUI called.");
        if (selectedSystemText != null)
            selectedSystemText.gameObject.SetActive(false);
        if (systemHealthSlider != null)
            systemHealthSlider.gameObject.SetActive(false);
        if (systemHealthText != null)
            systemHealthText.gameObject.SetActive(false);

        Debug.Log("DialogueManager: System UI elements hidden.");
    }

    /// <summary>
    /// Get system description based on the selected system
    /// </summary>
    private string GetSystemDescription(CubeInteraction.SystemType systemType)
    {
        Debug.Log($"DialogueManager: GetSystemDescription called for {systemType}.");
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

    /// <summary>
    /// Simulate system health (in a real game, you'd pull this from your system's data)
    /// </summary>
    private float GetSystemHealth(CubeInteraction.SystemType systemType)
    {
        // Placeholder: Return simulated health percentage (replace with actual system health in your game)
        // Remove or replace this method if you are passing the actual health from CubeInteraction
        Debug.Log("DialogueManager: GetSystemHealth called with placeholder values.");
        switch(systemType)
        {
            case CubeInteraction.SystemType.Engines:
                // Replace with actual engines health
                return 75f; // Example value
            case CubeInteraction.SystemType.LifeSupport:
                // Replace with actual life support health
                return 60f; // Example value
            case CubeInteraction.SystemType.Hull:
                // Replace with actual hull health
                return 80f; // Example value
            case CubeInteraction.SystemType.Generator:
                // Replace with actual generator health
                return 90f; // Example value
            default:
                return 100f;
        }
    }

    /// <summary>
    /// Updates the system health UI in real-time
    /// </summary>
    public void UpdateSystemHealthUI(string systemName, float currentHealth)
    {
        Debug.Log($"DialogueManager: UpdateSystemHealthUI called for {systemName} with Health: {currentHealth}%.");

        // Check if the currently selected system matches
        if (selectedSystemText != null && selectedSystemText.text.Contains(systemName))
        {
            // Update the health slider and health text
            if (systemHealthSlider != null)
            {
                systemHealthSlider.value = currentHealth / 100f;
                Debug.Log($"DialogueManager: System health slider updated to {currentHealth}% for {systemName}.");
            }
            if (systemHealthText != null)
            {
                systemHealthText.text = $"System Health: {currentHealth}%";
                Debug.Log($"DialogueManager: System health text updated to {currentHealth}% for {systemName}.");
            }
        }
    }
}
