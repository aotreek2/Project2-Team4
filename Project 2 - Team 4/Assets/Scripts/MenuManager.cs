using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject mainPanel, fadePanel;

    Animator camAnimator;
    Scene scene;

    private void Start()
    {
        scene = SceneManager.GetActiveScene();

        if(scene.name == "MainMenu")
        {
           mainPanel.SetActive(true);
           camAnimator = gameObject.GetComponent<Animator>();

           camAnimator.ResetTrigger("PlayHit");
           camAnimator.ResetTrigger("HelpHit");
           camAnimator.ResetTrigger("CreditsHit");
           camAnimator.ResetTrigger("BackHit");
        }
    }

    public void OnPlayButtonClicked()
    {
        camAnimator.SetTrigger("PlayHit");
        camAnimator.ResetTrigger("BackHit");
    }
    public void OnHelpButtonClicked()
    {
        camAnimator.SetTrigger("HelpHit");
        camAnimator.ResetTrigger("BackHit");
    }
    public void OnCreditsButtonClicked()
    {
        camAnimator.SetTrigger("CreditsHit");
        camAnimator.ResetTrigger("BackHit");
    }
    public void OnBackButtonClicked()
    {
        camAnimator.SetTrigger("BackHit");

        camAnimator.ResetTrigger("PlayHit");
        camAnimator.ResetTrigger("HelpHit");
        camAnimator.ResetTrigger("CreditsHit");
    }
    public void OnQuitButtonClicked()
    {
        Application.Quit();
    }

    public void OnMainMenuButtonClicked()
    {
        SceneManager.LoadScene(0);
    }

    public void MainMenuFlipper()
    {
        mainPanel.SetActive(!mainPanel.activeSelf);
    }

    public void PlayFadeOut()
    {
        fadePanel.SetActive(true);
    }

    public void NextScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
