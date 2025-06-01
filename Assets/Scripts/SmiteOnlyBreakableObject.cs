using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class SmiteOnlyBreakableObject : MonoBehaviour
{
    [SerializeField] private GameObject fracturedModel;
    [SerializeField] private float explosionForce = 500f;
    [SerializeField] private float explosionRadius = 500f;
    [SerializeField] private float destroyDelay = 0.1f;

    [SerializeField] private int objectDamage = 100;

    [SerializeField] private ParticleSystem hitEffect;
    [SerializeField] private AudioSource objectDestroyed;

    private bool isBroken = false;
    private Arquebus playerArquebus;

    private void Awake()
    {
        playerArquebus = FindObjectOfType<Arquebus>();
        if (playerArquebus == null)
            Debug.LogWarning("No Arquebus component found in scene; this object will never break.");
    }

    public void Break(Vector3 impactPoint)
    {
        if (isBroken) return;

        if (playerArquebus == null || playerArquebus.currentType != "Smite")
            return;

        isBroken = true;

        if (objectDestroyed != null)
            objectDestroyed.Play();

        if (hitEffect != null)
            hitEffect.Play();

        fracturedModel.SetActive(true);
        gameObject.SetActive(false);

        foreach (Rigidbody rb in fracturedModel.GetComponentsInChildren<Rigidbody>())
            rb.AddExplosionForce(explosionForce, impactPoint, explosionRadius);

        Collider[] hits = Physics.OverlapSphere(impactPoint, explosionRadius);
        foreach (var col in hits)
        {
            Enemy enemy = col.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(objectDamage, false);
                Rigidbody enemyRb = enemy.GetComponent<Rigidbody>();
                if (enemyRb != null)
                    enemyRb.AddExplosionForce(explosionForce, impactPoint, explosionRadius);
            }
        }

        Destroy(fracturedModel, destroyDelay);
    }
}