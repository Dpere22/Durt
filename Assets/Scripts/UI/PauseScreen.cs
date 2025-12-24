using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class PauseScreen : MonoBehaviour
    {
        public static bool IsPaused;

        private GameObject _pauseMenu;


        // Start is called before the first frame update
        void Start()
        {
            _pauseMenu = GameObject.FindGameObjectsWithTag("PauseMenu")[0];
            _pauseMenu.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
            if (InputManager.PauseWasPressed) 
            {
                if (!IsPaused) 
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
            _pauseMenu.SetActive(true);
            Time.timeScale = 0f;
            IsPaused = true;
            Actions.OnPause?.Invoke();
        }

        public void ResumeGame()
        {
            _pauseMenu.SetActive(false);
            Time.timeScale = 1f;
            IsPaused = false;   
            Actions.OnUnpause?.Invoke();
        }

        public void MainMenu()
        {
            ResumeGame();
            SceneManager.LoadScene(0);
        }
    }
}
