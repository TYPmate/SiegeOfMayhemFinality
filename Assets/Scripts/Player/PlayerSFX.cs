using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles player sound effects for movement and environmental interactions
/// </summary>
public class PlayerSFX : MonoBehaviour
{
    // Audio sources for different player sounds
    public AudioSource clankingSound;
    public AudioSource runningOnStone;

    /// <summary>
    /// Initialization method called when the script instance is being loaded
    /// </summary>
    void Start()
    {

    }

    /// <summary>
    /// Update is called once per frame to handle sound playback
    /// </summary>
    void Update()
    {
        // Check if any movement keys are being pressed
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
        {
            // Play clanking sound if not already playing
            if (!clankingSound.isPlaying)
            {
                clankingSound.Play();
            }

            // Play running on stone sound if not already playing
            if (!runningOnStone.isPlaying)
            {
                runningOnStone.Play();
            }
        }
    }
}