using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoseManager : MonoBehaviour
{
    private static GameObject gameOverScreen;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject parent;
    private PlayerMovement _playerMovement;
    private TimerManager _timerManager;

    public delegate void RespawnAction();
    public static event RespawnAction OnRespawn;

    public delegate void DeathAction();
    public static event DeathAction OnDeath;
    // Start is called before the first frame update
    void Start()
    {
        gameOverScreen = GameObject.FindGameObjectWithTag("GameOver");
        gameOverScreen.SetActive(false);   
        _playerMovement = player.GetComponent<PlayerMovement>();
        _timerManager = parent.GetComponent<TimerManager>();
        if (_playerMovement == null)
        {
            Debug.LogError("No player movement found");
        }

        if (_timerManager == null)
        {
            Debug.LogError("No timer manager found");
        }
    }

    public static void TriggerRespawn()
    {
        OnRespawn?.Invoke();
    }

    public static void TriggerDeath()
    {
        OnDeath?.Invoke();
    }
    public static void LoseGame() 
    {
        gameOverScreen.SetActive(true);
        PauseScreen.IsPaused = true;
        TriggerDeath();
        //Time.timeScale = 0f;
    }

    public void MainMenu() 
    {
        Time.timeScale = 1f;
        gameOverScreen.SetActive(false);
        PauseScreen.IsPaused = false;
        SceneManager.LoadScene("MainMenu");
    }
    
    public void Retry() 
    {
        //Time.timeScale = 1f;
        _timerManager.RegainOxygen();
        _playerMovement.Respawn();
        TriggerRespawn();
        PauseScreen.IsPaused = false;
        gameOverScreen.SetActive(false);
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
