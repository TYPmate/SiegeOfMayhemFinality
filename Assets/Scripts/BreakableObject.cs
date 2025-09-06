using UnityEngine;
using System.Collections;

/// <summary>
/// Handles breakable object functionality including fracture effects and damage propagation
/// </summary>
public class BreakableObject : MonoBehaviour
{
    [SerializeField] public GameObject fracturedModel;
    public float explosionForce = 500f;
    public float explosionRadius = 500f;
    public int objectDamage = 100;
    public float destroyDelay = 0.1f;

    private bool isBroken = false;

    // Visual and audio effects
    public ParticleSystem hitEffect;
    public AudioSource objectDestoyed;

    /// <summary>
    /// Breaks the object, activating fracture effects and applying damage to nearby enemies
    /// </summary>
    /// <param name="impactPoint">The point where the break impact occurred</param>
    public void Break(Vector3 impactPoint)
    {
        if (isBroken) return;

        // Play break sound effect
        SoundManager.Instance.PlaySound(SoundManager.Instance.effects[12]);

        isBroken = true;

        // Play visual hit effect if available
        if (hitEffect != null)
        {
            hitEffect.Play();
        }

        // Activate fractured model and deactivate original object
        fracturedModel.SetActive(true);
        gameObject.SetActive(false);

        // Apply explosion force to all fractured pieces
        foreach (Rigidbody rb in fracturedModel.GetComponentsInChildren<Rigidbody>())
        {
            rb.AddExplosionForce(explosionForce, impactPoint, explosionRadius);
        }

        // Find and damage nearby enemies
        Collider[] hitColliders = Physics.OverlapSphere(impactPoint, explosionRadius);
        foreach (Collider hit in hitColliders)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(objectDamage, false);
                Rigidbody enemyRb = enemy.GetComponent<Rigidbody>();
                if (enemyRb != null)
                {
                    enemyRb.AddExplosionForce(explosionForce, impactPoint, explosionRadius);
                }
            }
        }

        // Clean up fractured model after delay
        Destroy(fracturedModel, destroyDelay);
    }
}