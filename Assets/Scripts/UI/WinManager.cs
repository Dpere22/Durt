using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WinManager : MonoBehaviour
{
    public static bool playerWon = false;
    private static GameObject winScreen;
    
    public GameObject loadingScreen;
    public Slider slider;

    // Start is called before the first frame update
    void Start()
    {
        winScreen = GameObject.FindGameObjectWithTag("WinScreen");
        winScreen.SetActive(false);
    }

    public static void WinGame()
    {
        playerWon = false;
        Time.timeScale = 0f;
        PauseScreen.isPaused = true;
        winScreen.SetActive(true);
    }

    public void Retry() 
    {
        PauseScreen.isPaused = false;
        winScreen.SetActive(false);
        Time.timeScale = 1f;
        var currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void MainMenu() 
    {
        PauseScreen.isPaused = false;
        winScreen.SetActive(false);
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    public void NextLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int totalScenes = SceneManager.sceneCountInBuildSettings;
        int nextSceneIndex = currentSceneIndex + 1;
        if (nextSceneIndex < totalScenes - 1 )
        {
            PauseScreen.isPaused = false;
            winScreen.SetActive(false);
            Time.timeScale = 1f;
            LevelLoad(nextSceneIndex);
        }
        else if (nextSceneIndex == totalScenes-1) //Credits should not need loading
        {
            PauseScreen.isPaused = false;
            winScreen.SetActive(false);
            Time.timeScale = 1f;
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            MainMenu();
        }
    }

    public void StartGame()
    {
        PauseScreen.isPaused = false;
        winScreen.SetActive(false);
        Time.timeScale = 1f;
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex + 1);
    }
    
    
    
    public void LevelLoad(int sceneIndex)
    {
        SceneManager.LoadScene("Credits");
        StartCoroutine(LoadAsync(sceneIndex));
    }

    IEnumerator LoadAsync(int sceneIndex)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
        loadingScreen.SetActive(true);
        
        while (operation is { isDone: false })
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            slider.value = progress;
            yield return null; //waits one frame
        }
    }
}
