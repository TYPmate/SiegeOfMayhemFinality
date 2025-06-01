using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackHitbox : MonoBehaviour
{
    private bool hasHit = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!hasHit && other.CompareTag("Player"))
        {
            PlayerMotor player = other.GetComponent<PlayerMotor>();
            if (player != null)
            {
                player.TakeDamage(100); // Adjust damage as needed
                hasHit = true;
            }
        }
    }
    public void ForceHit(Collider other)
    {
        if (!hasHit && other.CompareTag("Player"))
        {
            PlayerMotor player = other.GetComponent<PlayerMotor>();
            if (player != null)
            {
                player.TakeDamage(100);
                hasHit = true;
            }
        }
    }
    public void ResetHit()
    {
        hasHit = false;
    }
}
