using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public CanvasGroup dialogueCanvasGroup; // For fade-in and fade-out
    public TMP_Text dialogueText; // Dialogue text (using TextMeshPro)
    public TMP_Text continueText; // "Click to Continue" or "Press Space to Continue" text
    public float typingSpeed = 0.05f; // Speed at which characters are typed out
    public float fadeDuration = 1f; // Duration for fade-in and fade-out
    public AudioSource typingAudioSource; // Optional: Audio for typing sound
    public AudioClip typingSound; // Sound for each character being typed

    private Coroutine typingCoroutine;
    public bool isDialogueActive { get; private set; }
    private bool isSkipping = false;

    void Start()
    {
        if (dialogueCanvasGroup == null)
        {
            dialogueCanvasGroup = GetComponent<CanvasGroup>();
        }

        dialogueCanvasGroup.alpha = 0; // Make sure the dialogue is hidden at the start
        continueText.alpha = 0; // Hide "Click to Continue" text at the start
        isDialogueActive = false;
    }

    // Starts the dialogue with multiple lines of text
    public void StartDialogue(string[] lines)
    {
        if (isDialogueActive) return; // Prevent starting if dialogue is already active
        StartCoroutine(DisplayDialogueSequence(lines));
    }

    // Displays a sequence of dialogue lines
    private IEnumerator DisplayDialogueSequence(string[] lines)
    {
        isDialogueActive = true;

        // Fade in the dialogue box
        yield return StartCoroutine(FadeInDialogue());

        foreach (var line in lines)
        {
            yield return StartCoroutine(TypeSentence(line));

            // Show "Click to Continue" text after sentence is fully typed
            yield return StartCoroutine(ShowContinueText());

            // Wait for player input to continue
            yield return StartCoroutine(WaitForPlayerInput());

            // Hide "Click to Continue" text when the player clicks to continue
            yield return StartCoroutine(HideContinueText());
        }

        // Fade out the dialogue box when finished
        yield return StartCoroutine(FadeOutDialogue());

        isDialogueActive = false;
    }

    // Typing effect for displaying one sentence
    private IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";
        isSkipping = false;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine); // Stop any ongoing typing coroutine
        }

        typingCoroutine = StartCoroutine(TypingEffect(sentence));
        yield return typingCoroutine;
    }

    // Coroutine for typing effect
    private IEnumerator TypingEffect(string sentence)
    {
        foreach (char letter in sentence.ToCharArray())
        {
            if (isSkipping)
            {
                dialogueText.text = sentence; // Skip to full sentence if player skips
                yield break;
            }

            dialogueText.text += letter;

            // Play typing sound for each character
            if (typingAudioSource != null && typingSound != null)
            {
                typingAudioSource.PlayOneShot(typingSound);
            }

            yield return new WaitForSeconds(typingSpeed);
        }
    }

    // Waits for player input (like clicking a button) to continue to the next sentence
    private IEnumerator WaitForPlayerInput()
    {
        while (!Input.GetKeyDown(KeyCode.Space) && !Input.GetMouseButtonDown(0))
        {
            yield return null;
        }

        isSkipping = true; // Set skip flag to true when player presses input
    }

    // Fade in the dialogue box
    private IEnumerator FadeInDialogue()
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            dialogueCanvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
            yield return null;
        }

        dialogueCanvasGroup.alpha = 1f; // Ensure it's fully visible
    }

    // Fade out the dialogue box
    private IEnumerator FadeOutDialogue()
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            dialogueCanvasGroup.alpha = Mathf.Clamp01(1f - (elapsedTime / fadeDuration));
            yield return null;
        }

        dialogueCanvasGroup.alpha = 0f; // Ensure it's fully hidden
    }

    // Show "Click to Continue" text with fade in
    private IEnumerator ShowContinueText()
    {
        float elapsedTime = 0f;
        continueText.alpha = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            continueText.alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
            yield return null;
        }

        continueText.alpha = 1f; // Ensure it's fully visible
    }

    // Hide "Click to Continue" text with fade out
    private IEnumerator HideContinueText()
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            continueText.alpha = Mathf.Clamp01(1f - (elapsedTime / fadeDuration));
            yield return null;
        }

        continueText.alpha = 0f; // Ensure it's fully hidden
    }

    // Optionally, allow skipping the entire dialogue sequence
    public void SkipDialogue()
    {
        if (isDialogueActive)
        {
            isSkipping = true; // If dialogue is active, set the skip flag
        }
    }
}
