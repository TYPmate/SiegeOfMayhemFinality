using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float projectileSpeed = 5f;
    [SerializeField] private float lifetime = 4f; // Time before despawn in seconds
    [SerializeField] private int damageAmount = 100; // Damage to deal to player

    private void Start()
    {
        // Destroy the projectile after 'lifetime' seconds if it doesn't hit anything
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.Translate(Vector3.forward * projectileSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the projectile hit the player
        PlayerMotor player = other.GetComponent<PlayerMotor>();
        if (player != null)
        {
            // Deal damage to the player
            player.TakeDamage(damageAmount);

            // Destroy the projectile immediately
            Destroy(gameObject);
        }
    }
}