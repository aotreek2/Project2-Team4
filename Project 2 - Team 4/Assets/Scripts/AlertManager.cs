// AlertManager.cs
using UnityEngine;
using TMPro;
using System.Collections;

public class AlertManager : MonoBehaviour
{
    public static AlertManager Instance { get; private set; }

    [Header("UI Components")]
    public CanvasGroup alertCanvasGroup;
    public TextMeshProUGUI alertText;

    [Header("Alert Settings")]
    public float fadeInDuration = 0.5f;
    public float displayDuration = 2f;
    public float fadeOutDuration = 0.5f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        if (alertCanvasGroup != null)
        {
            alertCanvasGroup.alpha = 0f;
            alertCanvasGroup.interactable = false;
            alertCanvasGroup.blocksRaycasts = false;
        }
        else
        {
            Debug.LogError("[AlertManager] alertCanvasGroup is not assigned.");
        }

        if (alertText == null)
        {
            Debug.LogError("[AlertManager] alertText is not assigned.");
        }
    }

    /// <summary>
    /// Displays an alert with the specified message.
    /// </summary>
    /// <param name="message">The alert message.</param>
    public void ShowAlert(string message)
    {
        if (alertCanvasGroup == null || alertText == null)
        {
            Debug.LogError("[AlertManager] Alert UI components are not assigned.");
            return;
        }

        StopAllCoroutines();
        StartCoroutine(DisplayAlertCoroutine(message));
    }

    private IEnumerator DisplayAlertCoroutine(string message)
    {
        alertText.text = message;
        alertCanvasGroup.alpha = 0f;
        alertCanvasGroup.interactable = false;
        alertCanvasGroup.blocksRaycasts = false;

        // Fade in
        while (alertCanvasGroup.alpha < 1f)
        {
            alertCanvasGroup.alpha += Time.unscaledDeltaTime / fadeInDuration;
            yield return null;
        }
        alertCanvasGroup.alpha = 1f;
        alertCanvasGroup.interactable = true;
        alertCanvasGroup.blocksRaycasts = true;

        // Display duration
        yield return new WaitForSecondsRealtime(displayDuration);

        // Fade out
        while (alertCanvasGroup.alpha > 0f)
        {
            alertCanvasGroup.alpha -= Time.unscaledDeltaTime / fadeOutDuration;
            yield return null;
        }
        alertCanvasGroup.alpha = 0f;
        alertCanvasGroup.interactable = false;
        alertCanvasGroup.blocksRaycasts = false;
    }
}
