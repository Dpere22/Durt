using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightTrailFadeOut : MonoBehaviour
{
    public bool beginFade = false;
    public float timeToFade;
    private Light2D _light;

    private float _timer = 0;
    private float _maxIntensity;
   
    void Start()
    {
        _light = GetComponent<Light2D>();
        _maxIntensity = _light.intensity;
    }

    // Update is called once per frame
    void Update()
    {
        if(beginFade) 
        {
            StartFade();
        }
    }

    public void StartFade()
    {
        _timer += Time.deltaTime;
        float percentComplete = _timer / timeToFade;
        float intensity = Mathf.Lerp(_maxIntensity, 0, percentComplete);
        _light.intensity = intensity;

        if (_timer > timeToFade)
        {
            Destroy(gameObject); 
        }
    }
}
