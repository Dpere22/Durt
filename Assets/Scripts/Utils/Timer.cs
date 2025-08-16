using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Class used to time items in the game.
/// </summary>
public class Timer : MonoBehaviour
{
    [SerializeField]
    public float t = 0;
    private float howLong;
    private float multiplyAmt;
    private bool start;

    public bool paused;
    public bool multiply;

    void FixedUpdate()
    {
        if (start && !paused)
        {
            AddTime(multiply);
        }
    }

    public void Init(float howLong, float multiplyAmt = 1f, bool isPaused = false)
    {
        this.howLong = howLong;
        this.multiplyAmt = multiplyAmt;
        paused = isPaused;
        start = true;
    }

    void AddTime(bool multiply, bool reverse = false)
    {
        if (multiply) t += Time.deltaTime * multiplyAmt;
        else if(!reverse) t += Time.deltaTime;
        else 
        {
            if(t != 0) t -= Time.deltaTime; //can't reverse time if time is 0
        }
    }

    /// <summary>
    /// Returns true if the timer has reached completion.
    /// </summary>
    /// <returns></returns>
    public bool Passed()
    {
        if (t > howLong)
        {
            StopTimer();
            return true;
        }
        else
        {
            return false;
        }
    }

    public float CurrentTime() 
    {
        return t;
    }

    public float Max() 
    {
        return howLong;
    }

    public void ResetTimer()
    {
        enabled = true;
        t = 0;
    }

    public void StopTimer()
    {
        paused = true;
    }
}