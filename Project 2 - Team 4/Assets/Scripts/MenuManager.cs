using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject helpPanel, creditsPanel;

    private void Start()
    {
        helpPanel.SetActive(false);
        creditsPanel.SetActive(false);
    }

    public void OnPLayButtonClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    public void OnHelpButtonClicked()
    {
        helpPanel.SetActive(true);
    }
    public void OnCreditsButtonClicked()
    {
        creditsPanel.SetActive(true);
    }
    public void OnCloseButtonClicked()
    {
        if(helpPanel.activeSelf == true || creditsPanel.activeSelf == true)
        {
            helpPanel.SetActive(false);
            creditsPanel.SetActive(false);
        }
    }
    public void OnQuitButtonClicked()
    {
        Application.Quit();
    }
}
