using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class TimerManager : MonoBehaviour
{
    public float timeAllowed = 30f;
    public float MultiplyBy = 2f;

    public static bool multiply = false;

    public Image pressureGaugeNeedle;
    public Image pressureGauge;
    private float _timerGaugeStart = 140f;
    private float _timerGaugeEnd = -140f;
    public bool oxygenCanDeplete;
    [SerializeField] Animator animator;

    private Color tintNormal = new(1f, 1f, 1f, 1f);
    private Color tintRedPG = new(255f / 255f, 129f / 255f, 129f / 255f, 255f / 255f);

    private float percentOfCurrentTime;

    private Timer timer;

    [SerializeField] private AudioSource audioSource;
    
    void Start()
    {
        timer = gameObject.AddComponent<Timer>();
        if (!oxygenCanDeplete)
        {
            timer.Init(timeAllowed, MultiplyBy, true);
        }
        else
        {
            timer.Init(timeAllowed, MultiplyBy);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!oxygenCanDeplete)
        {
            if (!timer.paused) timer.paused = true;
        }
        else
        {
            timer.paused = false;
        }
        UpdateTimer();
        AudioChecks();
    }

    void AudioChecks()
    {
        if (Math.Abs(percentOfCurrentTime - 0.25) < 0.0001f || Math.Abs(percentOfCurrentTime - 0.5) < 0.0001f || Math.Abs(percentOfCurrentTime - 0.75) < 0.0001f || Math.Abs(percentOfCurrentTime - 0.9) < 0.0001f)
        {
            audioSource.Play();
        }
    }

    private void FixedUpdate()
    {
        UpdatePressureGauge();
        animator.SetFloat("animPercentOfCurrentTime", percentOfCurrentTime);
    }

    void UpdateTimer() 
    {
        timer.multiply = multiply;

        if (timer.Passed()) 
        {
            LoseManager.LoseGame();
            timer.StopTimer();
            //enabled = false;
        }
    }

    public void RegainOxygen()
    {
        timer.t = 0;
        oxygenCanDeplete = false;
        UpdatePressureGauge();
    }
    void UpdatePressureGauge() 
    {
        float max = timer.Max();
        float current = timer.CurrentTime();
        float jiggleAmount;
        float shouldJiggle = Random.Range(0, 30); // Jiggle randomly about every 16 frames

        if (timer.multiply) 
        {
            pressureGauge.color = tintRedPG;
            jiggleAmount = Random.Range(0, 20); // Making jiggle more visceral
            shouldJiggle = Random.Range(12,16); // Increase jiggle occurrence rate
        }
        else
        {
            pressureGauge.color = tintNormal;
            jiggleAmount = !oxygenCanDeplete ? 0 : Random.Range(0, 7);
        }

        if(shouldJiggle != 15) 
        {
            jiggleAmount = 0;
        }

        percentOfCurrentTime = current / max;
        float rotationOfNeedle =  _timerGaugeStart - (Mathf.Abs(_timerGaugeEnd - _timerGaugeStart) * percentOfCurrentTime + jiggleAmount);

        pressureGaugeNeedle.transform.localEulerAngles = new Vector3(0, 0, rotationOfNeedle);
    }
}
