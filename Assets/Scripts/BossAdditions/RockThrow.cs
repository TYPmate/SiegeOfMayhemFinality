using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static PlayerMotor;

/// <summary>
/// Controls a rock projectile that floats up, then homes in and damages the player
/// </summary>
public class RockThrow : MonoBehaviour
{
    [SerializeField] Ability abilityInfo;     // Contains damage and lifetime values
    public float floatAtPlayer = 1f;          // Unused in current implementation
    public float floatUp = 1f;                // Duration of upward float animation
    public float moveSpeed = 1000f;           // Speed toward player
    float LifeTimer = 0;                      // Tracks existence time
    private bool isMoving = false;            // True when homing toward player
    Vector3 targetPosition;                   // Player's position to target
    private Transform playerTransform;        // Reference to player's transform

    void Start()
    {
        // Find player and begin floating sequence
        playerTransform = GameObject.FindObjectOfType<PlayerMotor>().transform;
        StartCoroutine(FloatSequence());
    }

    IEnumerator FloatSequence()
    {
        // Float upward for specified duration
        float timer = 0;
        while (timer < floatUp)
        {
            transform.position += Vector3.up * (Time.deltaTime * 7f);
            timer += Time.deltaTime;
            yield return null;
        }

        // Start moving toward player's position
        isMoving = true;
        targetPosition = playerTransform.position;
    }

    private void Update()
    {
        if (isMoving)
        {
            LifeTimer += Time.deltaTime;
        }

        // Destroy when lifetime expires
        if (LifeTimer > abilityInfo.lifetime)
        {
            Destroy(this.gameObject);
        }

        // Move toward target position
        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (isMoving && other.CompareTag("Player"))
        {
            // Damage player and destroy rock on collision
            PlayerMotor playerMotor = other.GetComponent<PlayerMotor>();
            if (playerMotor != null)
            {
                playerMotor.TakeDamage(abilityInfo.damage);
            }
            Destroy(gameObject);
        }
    }
}