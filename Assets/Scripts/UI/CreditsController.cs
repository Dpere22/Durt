using UnityEngine.SceneManagement;
using UnityEngine;

public class CreditsController : MonoBehaviour
{
    private bool _creditsEnded;
    [SerializeField] private GameObject continueText;
    void Update()
    {
        // Allow the player to skip the credits with the Enter key
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SceneManager.LoadScene(0);
        }
        else if (Input.anyKeyDown)
        {
            continueText.SetActive(true);
        }
    }

    // Called via Animation Event at the end of the credits animation
    public void OnCreditsEnd()
    {
        _creditsEnded = true;
        LoadNextScene();
    }

    private void LoadNextScene()
    {
        if (_creditsEnded || Input.GetKeyDown(KeyCode.Return))
        {
            SceneManager.LoadScene(0);
        }
    }
}
