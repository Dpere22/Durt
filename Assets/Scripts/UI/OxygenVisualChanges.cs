using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class OxygenVisualChanges : MonoBehaviour
{
    [SerializeField] private TimerManager tm;
    [SerializeField] private Volume volume;
    // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
        volume.enabled = tm.oxygenCanDeplete;
    }
}
