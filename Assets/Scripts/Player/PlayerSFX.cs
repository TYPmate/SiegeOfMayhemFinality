using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSFX : MonoBehaviour
{
    public AudioSource clankingSound;
    public AudioSource runningOnStone;

    void Start()
    {

    }

    void Update()
    {
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
        {
            if (!clankingSound.isPlaying)
            {
                clankingSound.Play();
            }

            if (!runningOnStone.isPlaying)
            {
                runningOnStone.Play();
            }
        }
    }
}
