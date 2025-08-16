using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelLoader : MonoBehaviour
{
    
    public GameObject loadingScreen;
    public Slider slider;
    public GameObject mainMenu;
    
    public void LevelLoad(int sceneIndex)
    {
        StartCoroutine(LoadAsync(sceneIndex));
    }

    IEnumerator LoadAsync(int sceneIndex)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
        mainMenu.SetActive(false);
        loadingScreen.SetActive(true);
        
        while (operation is { isDone: false })
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            slider.value = progress;
            yield return null; //waits one frame
        }
    }
}
