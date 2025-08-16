using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WolfAnimationController : MonoBehaviour
{
    public GameObject body;
    private Animator _anim;
    private int _currentState;
    private float _timeTillSwitch;
    private float _switchTimer = 0.1f;
    private DogController _controller;
    private static readonly int _wolfJump = Animator.StringToHash("wolfJump");
    private static readonly int _wolfTrot = Animator.StringToHash("wolfTrot");
    private static readonly int _wolfIdle = Animator.StringToHash("wolfIdle");
    void Start()
    {
        _anim = body.GetComponent<Animator>();
        _controller = GetComponent<DogController>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAnim();
    }

    private void UpdateAnim() 
    {
        int state = GetState();
        if (state == _currentState) return;
        _anim.CrossFade(state, 0f, 0);
        _currentState = state;
    }

    private int GetState() 
    {
        if (_controller.isJumping) 
        {
            return _wolfJump;
        }
        else if (_controller.moving)
        {
            _switchTimer = 0f;
            return _wolfTrot;
        }
        else if (_controller.moving || _switchTimer <= _timeTillSwitch) 
        {
            _switchTimer += Time.deltaTime;
            return _wolfTrot;
        }
        else
        {
            return _wolfIdle;
        }
    }
}
