using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles enemy attack hitbox detection and damage application
/// </summary>
public class EnemyAttackHitbox : MonoBehaviour
{
    private bool hasHit = false;  // Prevents multiple hits per attack

    private void OnTriggerEnter(Collider other)
    {
        if (!hasHit && other.CompareTag("Player"))
        {
            PlayerMotor player = other.GetComponent<PlayerMotor>();
            if (player != null)
            {
                player.TakeDamage(100);  // Apply damage to player
                hasHit = true;           // Prevent additional hits
            }
        }
    }

    /// <summary>
    /// Forces a hit check on specific collider (used for immediate overlap detection)
    /// </summary>
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

    /// <summary>
    /// Resets the hit flag to allow damage on next attack
    /// </summary>
    public void ResetHit()
    {
        hasHit = false;
    }
}