using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour

{
    public GameObject pauseMenu;
    bool isPaused;

    // Called when the script instance is being loaded
    void Awake()
    {
        // Ensure time is running when the scene starts (in case it was paused previously)
        Time.timeScale = 1f;
        if (pauseMenu != null)
            pauseMenu.SetActive(false);
        isPaused = false;
    }

    // Update is called once per frame
    void Update()
    {
        Pause();
    }

    public void Pause()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !isPaused)
        {
            Time.timeScale = 0f;
            if (pauseMenu != null)
                pauseMenu.SetActive(true);
            isPaused = true;
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && isPaused)
        {
            Time.timeScale = 1f;
            if (pauseMenu != null)
                pauseMenu.SetActive(false);
            isPaused = false;
        }
    }
}
