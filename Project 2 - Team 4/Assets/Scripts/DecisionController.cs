using UnityEngine;
using TMPro; // Required for TextMeshPro
using UnityEngine.UI;
using System;

public class DecisionController : MonoBehaviour
{
    public GameObject decisionPanel; // The panel to display the decision
    public TMP_Text decisionText; // TextMeshPro element to display the decision description
    public Button confirmButton, cancelButton; // Buttons to confirm or cancel decisions
    public TMP_Text confirmButtonText, cancelButtonText; // TextMeshPro elements for button text

    private Action confirmAction; // Action to execute on confirm
    private Action cancelAction; // Action to execute on cancel

    void Start()
    {
        // Ensure the panel is hidden initially
        if (decisionPanel != null)
        {
            decisionPanel.SetActive(false);
            Debug.Log("Decision panel initialized and hidden.");
        }

        // Hook up the confirm and cancel buttons
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(OnConfirm);
            Debug.Log("Confirm button listener added.");
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(OnCancel);
            Debug.Log("Cancel button listener added.");
        }
    }

    // Method to show the decision panel and set up the actions
    public void ShowDecision(string message, Action onConfirm, Action onCancel, string confirmText = "Confirm", string cancelText = "Cancel")
    {
        // Set the decision message
        if (decisionText != null)
        {
            decisionText.text = message;
            Debug.Log($"Decision text set: {message}");
        }

        // Set the button text using TextMeshPro
        if (confirmButtonText != null)
        {
            confirmButtonText.text = confirmText;
            Debug.Log($"Confirm button text set: {confirmText}");
        }

        if (cancelButtonText != null)
        {
            cancelButtonText.text = cancelText;
            Debug.Log($"Cancel button text set: {cancelText}");
        }

        // Store the confirm and cancel actions
        confirmAction = onConfirm;
        cancelAction = onCancel;

        Debug.Log("Actions stored for confirm and cancel.");

        // Show the decision panel
        if (decisionPanel != null)
        {
            decisionPanel.SetActive(true);
            Debug.Log("Decision panel shown.");
        }
    }

    // Method to handle the confirm button press
    private void OnConfirm()
    {
        // Hide the panel
        if (decisionPanel != null)
        {
            decisionPanel.SetActive(false);
            Debug.Log("Decision panel hidden after confirm.");
        }

        // Invoke the confirm action
        if (confirmAction != null)
        {
            confirmAction.Invoke();
            Debug.Log("Confirm action invoked.");
        }
        else
        {
            Debug.LogWarning("No confirm action set.");
        }
    }

    // Method to handle the cancel button press
    private void OnCancel()
    {
        // Hide the panel
        if (decisionPanel != null)
        {
            decisionPanel.SetActive(false);
            Debug.Log("Decision panel hidden after cancel.");
        }

        // Invoke the cancel action
        if (cancelAction != null)
        {
            cancelAction.Invoke();
            Debug.Log("Cancel action invoked.");
        }
        else
        {
            Debug.LogWarning("No cancel action set.");
        }
    }

    // Method to hide the decision panel
    public void HideDecisionPanel()
    {
        if (decisionPanel != null)
        {
            decisionPanel.SetActive(false);
            Debug.Log("Decision panel manually hidden.");
        }
    }
}
