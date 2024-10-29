using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject helpPanel, creditsPanel, mainPanel;

    Animator camAnimator;

    private void Start()
    {
        helpPanel.SetActive(false);
        creditsPanel.SetActive(false);
        mainPanel.SetActive(true);

        camAnimator = gameObject.GetComponent<Animator>();

        camAnimator.ResetTrigger("PlayHit");
        camAnimator.ResetTrigger("HelpHit");
        camAnimator.ResetTrigger("CreditsHit");
        camAnimator.ResetTrigger("BackHit");
    }

    public void OnPlayButtonClicked()
    {
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        camAnimator.SetTrigger("PlayHit");
        camAnimator.ResetTrigger("BackHit");

        //mainPanel.SetActive(false);
    }
    public void OnHelpButtonClicked()
    {
        //helpPanel.SetActive(true);
        camAnimator.SetTrigger("HelpHit");
        camAnimator.ResetTrigger("BackHit");

        //mainPanel.SetActive(false);
    }
    public void OnCreditsButtonClicked()
    {
        //creditsPanel.SetActive(true);
        camAnimator.SetTrigger("CreditsHit");
        camAnimator.ResetTrigger("BackHit");

        //mainPanel.SetActive(false);
    }
    public void OnBackButtonClicked()
    {
        /*
        if(helpPanel.activeSelf == true || creditsPanel.activeSelf == true)
        {
            helpPanel.SetActive(false);
            creditsPanel.SetActive(false);
        }
        */
        camAnimator.SetTrigger("BackHit");

        camAnimator.ResetTrigger("PlayHit");
        camAnimator.ResetTrigger("HelpHit");
        camAnimator.ResetTrigger("CreditsHit");
        

        //mainPanel.SetActive(true);
    }
    public void OnQuitButtonClicked()
    {
        Application.Quit();
    }

    public void MainMenuFlipper()
    {
        mainPanel.SetActive(!mainPanel.activeSelf);
    }
}
