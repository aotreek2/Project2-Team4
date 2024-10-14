using System.Collections;
using UnityEngine;
using TMPro; // For TextMeshProUGUI
using UnityEngine.UI; // For CanvasGroup

public class ChapterIntroUI : MonoBehaviour
{
    public CanvasGroup[] borderPanels; // Array of CanvasGroups for the borders
    public TextMeshProUGUI chapterTitleText;
    public TextMeshProUGUI chapterSubtext;
    public float fadeDuration = 2f;
    public float displayDuration = 4f;

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
        // Set the text
        chapterTitleText.text = chapterTitle;
        chapterSubtext.text = subtext;
        chapterTitleText.alpha = 0;
        chapterSubtext.alpha = 0;

        // Fade in the title
        yield return StartCoroutine(FadeTextIn(chapterTitleText));

        // Wait, then fade in the subtext
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(FadeTextIn(chapterSubtext));

        // Wait for the display duration
        yield return new WaitForSeconds(displayDuration);

        // Fade out the text
        yield return StartCoroutine(FadeTextOut(chapterTitleText));
        yield return StartCoroutine(FadeTextOut(chapterSubtext));

        // Fade out the border panels after text fades
        yield return StartCoroutine(FadeOutBorders());
    }

    private IEnumerator FadeTextIn(TextMeshProUGUI textElement)
    {
        float elapsedTime = 0f;
        textElement.alpha = 0;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
            textElement.alpha = alpha;
            yield return null;
        }
    }

    private IEnumerator FadeTextOut(TextMeshProUGUI textElement)
    {
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(1f - (elapsedTime / fadeDuration));
            textElement.alpha = alpha;
            yield return null;
        }
    }

    private IEnumerator FadeOutBorders()
    {
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(1f - (elapsedTime / fadeDuration));
            foreach (var panel in borderPanels)
            {
                panel.alpha = alpha;
            }
            yield return null;
        }
    }
}
