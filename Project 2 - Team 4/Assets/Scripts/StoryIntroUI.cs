using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI; // For handling UI elements like the skip button

public class StoryIntroUI : MonoBehaviour
{
    public CanvasGroup textCanvasGroup; // CanvasGroup for the text
    public CanvasGroup panelCanvasGroup; // CanvasGroup for the panel (static, no fade)
    public TMP_Text storyText; // Reference to the TMP text for displaying story
    public float fadeDuration = 2f; // How long it takes to fade in/out
    public float displayDuration = 3f; // How long each text stays on screen
    public Button skipButton; // Button to skip the intro

    private string[] storyLines = new string[]
    {
        "A Broken Ship.",
        "A Dying Crew.",
        "Your Last Hope.",
        "Fix the Ship...",
        "Or Perish in Space."
    };

    private Coroutine introCoroutine; // Holds reference to the running intro coroutine

    void Start()
    {
        // Ensure the panel stays fully visible
        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = 1f; // Make sure the panel's alpha is always 1 (visible)
        }

        // Pause the game when the story begins
        Time.timeScale = 0f;

        // Add listener to the skip button
        if (skipButton != null)
        {
            skipButton.onClick.AddListener(SkipIntro);
        }

        // Start the story intro sequence
        introCoroutine = StartCoroutine(PlayStoryIntro());
    }

    // Plays the intro sequence
    private IEnumerator PlayStoryIntro()
    {
        textCanvasGroup.alpha = 0; // Set text alpha to 0 initially

        foreach (string line in storyLines)
        {
            yield return StartCoroutine(DisplayText(line));
        }

        // Once done with the intro, resume the game
        textCanvasGroup.alpha = 0; // Ensure the text is hidden at the end
        gameObject.SetActive(false); // Disable the UI when done

        Time.timeScale = 1f; // Resume the game
    }

    // Displays each line of text with fade in and out
    private IEnumerator DisplayText(string line)
    {
        storyText.text = line;
        yield return StartCoroutine(FadeInText());

        // Wait for the text to be visible for the specified duration
        yield return new WaitForSecondsRealtime(displayDuration);

        yield return StartCoroutine(FadeOutText());
    }

    // Fades the text in
    private IEnumerator FadeInText()
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime; // Use unscaled time to fade while the game is paused
            textCanvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
            yield return null;
        }

        textCanvasGroup.alpha = 1f;
    }

    // Fades the text out
    private IEnumerator FadeOutText()
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime; // Use unscaled time for fading out
            textCanvasGroup.alpha = Mathf.Clamp01(1f - (elapsedTime / fadeDuration));
            yield return null;
        }

        textCanvasGroup.alpha = 0f;
    }

    // Method to skip the intro sequence
    public void SkipIntro()
    {
        // If the intro coroutine is running, stop it
        if (introCoroutine != null)
        {
            StopCoroutine(introCoroutine);
        }

        // Reset the text and UI states
        textCanvasGroup.alpha = 0;
        panelCanvasGroup.alpha = 1f;

        // Resume the game
        Time.timeScale = 1f;

        // Disable the intro UI
        gameObject.SetActive(false);
    }
}
