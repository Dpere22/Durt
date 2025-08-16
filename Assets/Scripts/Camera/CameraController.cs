using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Camera mainCam;
    public GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        mainCam = Camera.main;
        player = GameObject.FindWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        if(player != null)
        {
            WatchPlayer();
        }
    }

    public void WatchPlayer()
    {
        Vector3 playerXY = new Vector3(player.transform.position.x, player.transform.position.y, mainCam.transform.position.z);
        mainCam.transform.position = playerXY;
    }

}
