using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.VisualScripting;
using UnityEngine;

public class MoveArmWithCursor : MonoBehaviour
{
    private GameObject _player;

    private bool _canMove = true;
    private Camera _camera;

    [SerializeField] private int minRotation;
    [SerializeField] private int maxRotation;

    // Start is called before the first frame update
    void Start()
    {
        _camera = Camera.main;
        _player = GameObject.FindWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        if (_canMove)
        {
            MoveToCursor();
        }
    }

    private void OnEnable()
    {
        Actions.OnPause += ChangeMovement;
        Actions.OnUnpause += ChangeMovement;
    }

    private void OnDisable()
    {
        Actions.OnPause -= ChangeMovement;
        Actions.OnUnpause -= ChangeMovement;
    }

    private void ChangeMovement()
    {
        _canMove = !_canMove;
    }

    private void MoveToCursor()
    {
        Vector2 mousePos = InputManager.MousePos;
        if (!_camera) return;
        
        Vector3 mouseWorldPos = _camera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, _camera.nearClipPlane));
        Vector2 direction = (mouseWorldPos - transform.position).normalized;

        float playerRotationY;

        if (_player) 
        {
            playerRotationY = _player.transform.rotation.eulerAngles.y;
        }
        else 
        {
            playerRotationY = 0;
        }

        float angle;

        if(playerRotationY == 0) 
        {   
            angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0, angle);
        }
        else 
        {
            angle = 180 - Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 180, angle);
        }

        if (InRange(angle, -90, 90))
        {
            Actions.TurnPlayer?.Invoke(false);
        }
        else 
        {
            Actions.TurnPlayer?.Invoke(true);
        }
    }

    bool InRange(float value, float min, float max, float buffer = 5f)
    {
        value = (value % 360 + 360) % 360;
        min = (min % 360 + 360) % 360;
        max = (max % 360 + 360) % 360;

        min = (min - buffer + 360) % 360;
        max = (max + buffer + 360) % 360;

        if (min < max)
        {
            return value >= min && value <= max;
        }
        else
        {
            return value >= min || value <= max;
        }
    }

}
