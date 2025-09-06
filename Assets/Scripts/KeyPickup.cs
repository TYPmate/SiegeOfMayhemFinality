using UnityEngine;

/// <summary>
/// Handles key pickup functionality including visual effects and player interaction
/// </summary>
public class KeyPickup : MonoBehaviour
{
    public float rotationSpeed = 50f;
    public float floatStrength = 0.5f;
    public float floatSpeed = 2f;

    private Vector3 startPos;

    public AudioClip pickupSound;

    /// <summary>
    /// Initializes the starting position for floating animation
    /// </summary>
    void Start()
    {
        startPos = transform.position;
    }

    /// <summary>
    /// Updates key rotation and floating animation
    /// </summary>
    void Update()
    {
        // Rotate the key
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

        // Float up and down
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatStrength;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    /// <summary>
    /// Handles trigger entry to detect player and grant key possession
    /// </summary>
    /// <param name="other">The collider entering the trigger zone</param>
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMotor playerMotor = other.GetComponent<PlayerMotor>();
            if (playerMotor != null)
            {
                playerMotor.hasKey = true;
            }

            if (pickupSound)
                SoundManager.Instance.PlaySound(SoundManager.Instance.effects[8]);

            Destroy(gameObject);
        }
    }
}