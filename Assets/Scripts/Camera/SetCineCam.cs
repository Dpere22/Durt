using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetCineCam : MonoBehaviour
{
    CinemachineVirtualCamera cam;
    GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<CinemachineVirtualCamera>();
        player = GameObject.FindWithTag("Player");

        if(player && cam) 
        {
            cam.Follow = player.transform;
            cam.LookAt = player.transform;
        }
    }

}
