using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegainOxygen : MonoBehaviour
{
    // Start is called before the first frame update
    private TimerManager _tm;
    private bool _used;
    [SerializeField] private GameObject player;
    private PlayerMovement _playerMovement;
    void Start()
    {
        _playerMovement = player.GetComponent<PlayerMovement>();
        var timerObject = GameObject.Find("GameManagerFinal");
        if (timerObject != null)
        {
            _tm = timerObject.GetComponent<TimerManager>();
        }
        else
        {
            Debug.LogError("GameManagerFinal not found");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Player") || _used) return;
        if(_playerMovement == null) Debug.LogError("PlayerMovement not found");
        _playerMovement.SetRespawnPoint();
        _tm.RegainOxygen();
        _used = true;
    }
}
