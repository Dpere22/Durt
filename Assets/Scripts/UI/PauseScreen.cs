using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseScreen : MonoBehaviour
{
    public static bool isPaused;

    private GameObject pauseMenu;


    // Start is called before the first frame update
    void Start()
    {
        pauseMenu = GameObject.FindGameObjectsWithTag("PauseMenu")[0];
        pauseMenu.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (InputManager.PauseWasPressed) 
        {
            if (!isPaused) 
            {
                PauseGame();
            }
            else 
            {
                ResumeGame();
            }
        }
    }

    void PauseGame()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
        Actions.OnPause?.Invoke();
    }

    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;   
        Actions.OnUnpause?.Invoke();
    }

    public void MainMenu()
    {
        ResumeGame();
        SceneManager.LoadScene("MainMenu");
    }
}
