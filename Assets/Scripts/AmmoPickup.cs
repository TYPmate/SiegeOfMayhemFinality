using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles ammo pickup behavior including floating animation and player collection
/// </summary>
public class AmmoPickup : MonoBehaviour
{
    public float floatStrength = 0.5f;    // Height of floating animation
    public float floatSpeed = 2f;         // Speed of floating animation

    private Vector3 startPos;             // Original position reference

    public AudioClip pickupSound;         // Sound played when collected
    public string ammoType;               // Type of ammo (Multiple, Smite, Knockback)

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Floating up/down animation using sine wave
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatStrength;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMotor playerMotor = other.GetComponent<PlayerMotor>();
            if (playerMotor != null)
            {
                // Grant appropriate ammo type based on pickup
                switch (ammoType)
                {
                    case "Multiple":
                        playerMotor.hasMultipleAmmo = true;
                        break;
                    case "Smite":
                        playerMotor.hasSmiteAmmo = true;
                        break;
                    case "Knockback":
                        playerMotor.hasKnockbackAmmo = true;
                        break;
                }
            }

            // Play pickup sound if available
            if (pickupSound)
                SoundManager.Instance.PlaySound(SoundManager.Instance.effects[7]);

            Destroy(gameObject);
        }
    }
}