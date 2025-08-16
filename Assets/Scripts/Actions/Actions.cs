using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class Actions
{
    //Pause Actions
    public static Action OnPause;
    public static Action OnUnpause;
    public static Action<bool> TurnPlayer;

    //Enemy Actions
    public static Action<GameObject, GameObject> OnPlayerSpotted;
    public static Action<GameObject> OnPlayerNotSpotted;
}
