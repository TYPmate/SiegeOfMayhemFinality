using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    public float floatStrength = 0.5f;
    public float floatSpeed = 2f;

    private Vector3 startPos;

    public AudioClip pickupSound;

    public string ammoType;
    void Start()
    {
        startPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {

        //Float up and down
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

            if (pickupSound)
                SoundManager.Instance.PlaySound(SoundManager.Instance.effects[7]);

            Destroy(gameObject);
        }
    }
}
