using System.Collections;
using UnityEngine;
using TMPro; // TextMeshPro for text
using UnityEngine.UI; // For handling the UI elements like Panel (Fog)

public class ChapterIntroUI : MonoBehaviour
{
    public CanvasGroup canvasGroup; // Reference to the CanvasGroup for fade effects
    public TextMeshProUGUI chapterTitleText; // TextMeshPro for Chapter Title
    public TextMeshProUGUI chapterSubtext; // TextMeshPro for Subtext
    public Image fogPanel; // Panel for Fog overlay (this will serve as the dark background)
    public float fadeDuration = 2f; // Duration for fade in/out
    public float displayDuration = 4f; // Time the text stays on screen after fade in

    void Start()
    {
        // Ensure the CanvasGroup is assigned
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
        
        // Ensure the fogPanel is assigned
        if (fogPanel == null)
        {
            Debug.LogError("Fog Panel is not assigned!");
        }
        
        // Initially hide the UI and fog panel
        canvasGroup.alpha = 0;
        fogPanel.color = new Color(fogPanel.color.r, fogPanel.color.g, fogPanel.color.b, 0f);
    }

    public void DisplayChapterIntro(string chapterTitle, string subtext)
    {
        StartCoroutine(ShowChapterIntro(chapterTitle, subtext));
    }

    private IEnumerator ShowChapterIntro(string chapterTitle, string subtext)
    {
        // Set the chapter title and subtext using TextMeshPro
        chapterTitleText.text = chapterTitle;
        chapterSubtext.text = subtext;

        // Fade in the chapter intro (text and fog)
        yield return StartCoroutine(FadeIn());

        // Hold the chapter intro for the specified display duration
        yield return new WaitForSeconds(displayDuration);

        // Fade out the chapter intro (text and fog)
        yield return StartCoroutine(FadeOut());

        // Reset the canvas and fog alpha
        canvasGroup.alpha = 0;
        fogPanel.color = new Color(fogPanel.color.r, fogPanel.color.g, fogPanel.color.b, 0f);
    }

    private IEnumerator FadeIn()
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeDuration);

            // Update both TextMeshPro text and panel transparency
            canvasGroup.alpha = alpha;
            fogPanel.color = new Color(fogPanel.color.r, fogPanel.color.g, fogPanel.color.b, alpha * 0.6f); // 60% opacity for fog

            yield return null;
        }

        // Ensure full visibility at the end of fade-in
        canvasGroup.alpha = 1f;
        fogPanel.color = new Color(fogPanel.color.r, fogPanel.color.g, fogPanel.color.b, 0.6f);
    }

    private IEnumerator FadeOut()
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(1f - (elapsedTime / fadeDuration));

            // Update both TextMeshPro text and panel transparency
            canvasGroup.alpha = alpha;
            fogPanel.color = new Color(fogPanel.color.r, fogPanel.color.g, fogPanel.color.b, alpha * 0.6f);

            yield return null;
        }

        // Ensure complete invisibility at the end of fade-out
        canvasGroup.alpha = 0f;
        fogPanel.color = new Color(fogPanel.color.r, fogPanel.color.g, fogPanel.color.b, 0f);
    }
}
