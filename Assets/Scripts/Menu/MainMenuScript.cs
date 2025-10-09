using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    public Animator settingsAnim;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Time.timeScale = 1f; // Asegura que el tiempo esté corriendo al iniciar el menú principal
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void StartGame()
    {
        // Ensure time is running (in case it was paused by a pause menu)
        SceneManager.LoadScene(1);
    }

    public void QuitGame()
    {
        Application.Quit();
        print("Game Closed");
    }

    public void GoToMainMenu()
    {
        // Reset time scale when going back to main menu
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    public void ShowSettings()
    {
        settingsAnim.SetBool("ShowSettings", true);
    }
    
    public void HideSettings()
    {
        settingsAnim.SetBool("ShowSettings", false);
    }
}