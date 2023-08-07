using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    int menuIndex;
    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject settingsMenu;
    [SerializeField] GameObject creditsMenu;

    public void Start()
    {
        ChangeMenu(0);
    }
    public void LoadScene(int sceneToLoad)
    {
        SceneManager.LoadScene(sceneToLoad);
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void ChangeMenu(int menuIndex)
    {
        mainMenu.SetActive(false);
        settingsMenu.SetActive(false);
        creditsMenu.SetActive(false);
        if (menuIndex == 0) mainMenu.SetActive(true);
        if (menuIndex == 1) settingsMenu.SetActive(true);
        if (menuIndex == 2) creditsMenu.SetActive(true);
    }

}
