using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Bayonet : MonoBehaviour
{
    public int damage = 100;
    public Vector3 halfExtents = new Vector3(0.05f, 0.05f, 0.3f);
    public LayerMask enemyLayer;

    public float detachDelay = 3f; // seconds before launch
    public float launchForce = 500f; // how fast it shoots forward

    private float timer;
    private bool launched = false;

    private Rigidbody rb;
    private List<Collider> damagedColliders = new List<Collider>();

    public float cooldownDuration = 3f; // cooldown between uses
    private float cooldownTimer = 0f;

    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;
    private Transform originalParent;
    private bool activeVisual;
    private bool isArmed = false; // controls whether bayonet should run countdown
    public GameObject readyIndicator;

    private MeshRenderer meshRenderer;

    // Enemy tracking lists
    private List<Enemy> enemiesHit = new List<Enemy>();
    private List<RangedEnemy> rangedEnemiesHit = new List<RangedEnemy>();

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    private void Start()
    {
        timer = detachDelay;
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
        }

        // Save original position & parent for reset later
        originalLocalPosition = transform.localPosition;
        originalLocalRotation = transform.localRotation;
        originalParent = transform.parent;
    }

    void Update()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;

            // Still cooling down, keep the indicator hidden
            if (readyIndicator != null)
                readyIndicator.SetActive(false);
        }
        else if (!launched && readyIndicator != null)
        {
            // Cooldown complete and bayonet is not launched
            readyIndicator.SetActive(true);
        }

        if (cooldownTimer > 0f)
            return;

        // Launch countdown logic...
        if (!launched && isArmed)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                LaunchBayonet();
                Invoke(nameof(ResetBayonet), 10f);
                return;
            }
        }

        if (isArmed || launched)
        {
            CheckForHits();
        }
    }

    private void CheckForHits()
    {
        Collider[] hits = Physics.OverlapBox(
            transform.position,
            halfExtents,
            transform.rotation,
            enemyLayer);

        // Clear previous frame's hits
        enemiesHit.Clear();
        rangedEnemiesHit.Clear();

        foreach (Collider hit in hits)
        {
            if (!damagedColliders.Contains(hit))
            {
                Enemy enemy = hit.GetComponent<Enemy>();
                RangedEnemy rangedEnemy = hit.GetComponent<RangedEnemy>();

                if (enemy != null && !enemiesHit.Contains(enemy))
                {
                    enemiesHit.Add(enemy);
                    damagedColliders.Add(hit);
                }
                else if (rangedEnemy != null && !rangedEnemiesHit.Contains(rangedEnemy))
                {
                    rangedEnemiesHit.Add(rangedEnemy);
                    damagedColliders.Add(hit);
                }
            }
        }

        // Apply damage to all hit enemies
        foreach (var enemy in enemiesHit)
        {
            enemy.TakeDamage(damage, false);
            Debug.Log("Bayonet hit regular enemy: " + enemy.name);
        }

        foreach (var rangedEnemy in rangedEnemiesHit)
        {
            rangedEnemy.TakeDamage(damage, false);
            Debug.Log("Bayonet hit ranged enemy: " + rangedEnemy.name);
        }

        damagedColliders.RemoveAll(col => col == null);
    }

    private void LaunchBayonet()
    {
        launched = true;
        transform.parent = null; // Detach from player

        rb.useGravity = true;
        rb.isKinematic = false; // ensure it's set *after* we detach
        rb.AddForce(transform.forward * launchForce, ForceMode.Impulse);

        if (readyIndicator != null)
        {
            readyIndicator.SetActive(false);
        }

        Debug.Log("Bayonet launched!");
    }

    private void ResetBayonet()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.useGravity = false;
        rb.isKinematic = true;

        cooldownTimer = cooldownDuration;
        launched = false;
        isArmed = false;
        damagedColliders.Clear();
        enemiesHit.Clear();
        rangedEnemiesHit.Clear();

        if (readyIndicator != null)
        {
            readyIndicator.SetActive(false); // Hide until ready
        }

        Debug.Log("Bayonet reset — press B to rearm");
    }

    private void OnTriggerExit(Collider other)
    {
        if (damagedColliders.Contains(other))
        {
            damagedColliders.Remove(other);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, halfExtents * 2);
    }

    public bool IsReady()
    {
        Debug.Log($"IsReady called — cooldownTimer: {cooldownTimer}, launched: {launched}");
        return cooldownTimer <= 0f && !launched;
    }

    public void Rearm()
    {
        transform.SetParent(originalParent);
        transform.localPosition = originalLocalPosition;
        transform.localRotation = originalLocalRotation;

        timer = detachDelay;
        isArmed = true;
        Debug.Log("Bayonet rearmed and ready to launch");
        if (readyIndicator != null)
        {
            readyIndicator.SetActive(false);
        }
    }

    public void SetVisible(bool visible)
    {
        if (meshRenderer != null)
            meshRenderer.enabled = visible;
    }
}