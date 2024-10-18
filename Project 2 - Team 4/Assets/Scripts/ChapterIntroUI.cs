using System.Collections;
using UnityEngine;
using TMPro; // For TextMeshProUGUI
using UnityEngine.UI; // For CanvasGroup

public class ChapterIntroUI : MonoBehaviour
{
    public CanvasGroup[] borderPanels; // Array of CanvasGroups for the borders
    public TextMeshProUGUI chapterTitleText;
    public TextMeshProUGUI chapterSubtext;
    public float displayDuration = 5f; // Display duration increased to 5 seconds
    public float titleDelay = 1f; // Title text appears 1 second after the story UI
    public float subtextDelay = 1f; // Subtext appears 1 second after the title

    public AudioClip chapterAudio; // Audio clip for chapter "bang" sound
    public AudioSource audioSource; // AudioSource for playing the sound
    public float fadeDuration = 1f; // Duration for fading out

    void Start()
    {
        // Make sure all borders are visible at the start
        foreach (var panel in borderPanels)
        {
            panel.alpha = 1f;
        }
    }

    // Display chapter intro and wait for it to finish
    public IEnumerator DisplayChapterIntro(string chapterTitle, string subtext)
    {
        // Wait for the delay before the title text appears (sync with the end of story UI)
        yield return new WaitForSeconds(titleDelay);

        // Set the chapter title text and immediately pop it on screen
        chapterTitleText.text = chapterTitle;
        chapterTitleText.alpha = 1f;

        // Play audio for chapter title intro
        if (chapterAudio != null && audioSource != null)
        {
            audioSource.PlayOneShot(chapterAudio);
        }

        // Wait for 1 second before showing the subtext
        yield return new WaitForSeconds(subtextDelay);

        // Set the subtext and pop it on screen
        chapterSubtext.text = subtext;
        chapterSubtext.alpha = 1f;

        // Play audio again for subtext intro
        if (chapterAudio != null && audioSource != null)
        {
            audioSource.PlayOneShot(chapterAudio);
        }

        // Wait for the display duration (keeping text on screen for 5 seconds)
        yield return new WaitForSeconds(displayDuration);

        // Fade out text and borders at the same time
        yield return StartCoroutine(FadeOutTextAndBorders());
        
        // Reset text alpha after it's done (optional)
        chapterTitleText.alpha = 0;
        chapterSubtext.alpha = 0;
    }

    private IEnumerator FadeOutTextAndBorders()
    {
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(1f - (elapsedTime / fadeDuration));

            // Fade out text
            chapterTitleText.alpha = alpha;
            chapterSubtext.alpha = alpha;

            // Fade out borders
            foreach (var panel in borderPanels)
            {
                panel.alpha = alpha;
            }
            yield return null;
        }
    }
}
