using UnityEngine;
using System.Collections;

public class BreakableObject : MonoBehaviour
{
    [SerializeField] public GameObject fracturedModel;
    public float explosionForce = 500f;
    public float explosionRadius = 500f;
    public int objectDamage = 100;
    public float destroyDelay = 0.1f;

    private bool isBroken = false;

    public ParticleSystem hitEffect;

    public AudioSource objectDestoyed;

    public void Break(Vector3 impactPoint)
    {
        if (isBroken) return;

        SoundManager.Instance.PlaySound(SoundManager.Instance.effects[12]);

        isBroken = true;

        if (hitEffect != null)
        {
            hitEffect.Play();
        }

        fracturedModel.SetActive(true);
        gameObject.SetActive(false);

        foreach (Rigidbody rb in fracturedModel.GetComponentsInChildren<Rigidbody>())
        {
            rb.AddExplosionForce(explosionForce, impactPoint, explosionRadius);
        }

        Collider[] hitColliders = Physics.OverlapSphere(impactPoint, explosionRadius);
        foreach (Collider hit in hitColliders)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(objectDamage,false);
                Rigidbody enemyRb = enemy.GetComponent<Rigidbody>();
                if (enemyRb != null)
                {
                    enemyRb.AddExplosionForce(explosionForce, impactPoint, explosionRadius);
                }
            }
        }

        Destroy(fracturedModel, destroyDelay);
    }
}