using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class InputManager : MonoBehaviour
{
    public static PlayerInput PlayerInput;

    public static Vector2 Movement;
    public static Vector2 MousePos;
    public static bool JumpWasPressed;
    public static bool JumpWasReleased;
    public static bool JumpIsHeld;
    public static bool RunIsHeld;
    public static bool PauseWasPressed;

    //MouseStuff
    public static bool RightClickHeld;
    public static bool LeftClick;
    public static bool RightClickReleased;


    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction runAction;
    private InputAction mouseAction;
    private InputAction pauseAction;
    private InputAction rightClickAction;
    private InputAction leftClickAction;


    private void Awake()
    {
        PlayerInput = GetComponent<PlayerInput>();
        moveAction = PlayerInput.actions["Move"];
        jumpAction = PlayerInput.actions["Jump"];
        runAction = PlayerInput.actions["Run"];
        mouseAction = PlayerInput.actions["Mouse"];
        pauseAction = PlayerInput.actions["Pause"];
        rightClickAction = PlayerInput.actions["MouseRightClick"];
        leftClickAction = PlayerInput.actions["MouseLeftClick"];
    }

    private void Update()
    {
        Movement = moveAction.ReadValue<Vector2>();
        MousePos = mouseAction.ReadValue<Vector2>();

        JumpWasPressed = jumpAction.WasPressedThisFrame();
        JumpIsHeld = jumpAction.WasPerformedThisFrame();
        JumpWasReleased = jumpAction.WasReleasedThisFrame();

        PauseWasPressed = pauseAction.WasPressedThisFrame();

        RunIsHeld = runAction.IsPressed();

        //Mouse Stuff
        RightClickHeld = rightClickAction.IsPressed();
        RightClickReleased = rightClickAction.WasReleasedThisFrame();
        LeftClick = leftClickAction.WasPressedThisFrame();
    }
}
