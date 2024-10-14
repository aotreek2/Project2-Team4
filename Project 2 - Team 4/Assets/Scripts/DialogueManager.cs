using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    public CanvasGroup dialogueCanvasGroup;
    public TextMeshProUGUI dialogueText;
    public float typingSpeed = 0.05f;

    private Queue<string> sentences;
    public bool isDialogueActive = false; // Make sure this is public

    void Start()
    {
        sentences = new Queue<string>();
        dialogueCanvasGroup.alpha = 0f;
    }

    public void StartDialogue(string[] dialogueLines)
    {
        if (isDialogueActive)
            return;

        isDialogueActive = true;
        dialogueCanvasGroup.alpha = 1f;
        sentences.Clear();

        foreach (string line in dialogueLines)
        {
            sentences.Enqueue(line);
        }

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        string sentence = sentences.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));
    }

    IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        // Wait for player input or auto-advance after a delay
        yield return new WaitForSeconds(2f); // Adjust delay as needed
        DisplayNextSentence();
    }

    public void EndDialogue()
    {
        isDialogueActive = false;
        dialogueCanvasGroup.alpha = 0f;
    }
}
