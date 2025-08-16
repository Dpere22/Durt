using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ambience : MonoBehaviour
{
    [SerializeField] AudioSource ambientPlayer;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(PlaySoundEvery10Seconds());
    }
    private void Update()
    {
        if (PauseScreen.isPaused)
        {
            ambientPlayer.Stop(); //also ghetto
        }
    }
    IEnumerator PlaySoundEvery10Seconds()
    {
        while (true)
        {
            if (!PauseScreen.isPaused)
            {
                ambientPlayer.Play();
            }
            yield return new WaitForSeconds(20f);

        }
    }
}
