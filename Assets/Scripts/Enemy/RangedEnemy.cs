using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class RangedEnemy : MonoBehaviour
{
    public Transform player;
    private NavMeshAgent agent;
    public GameObject ragdoll;
    public Animator animator;
    public float currentHealth = 100, maxHealth = 100;

    private enum EnemyState
    {
        Pathing,
        Attacking,
        Dead
    }

    private EnemyState currentState = EnemyState.Pathing;

    public GameObject dropItemPrefab;
    private PlayerAnimator playerAnimator;

    public AudioSource walkingSound;
    public ParticleSystem hitEffect;
    public AudioSource groanSound;
    private float groanTimer = 0f;
    private float nextGroanTime = 0f;
    private string currentAnimTrigger = "";

    public float destroyDelay = 2f;
    public float attackRange = 15f; // Optimal shooting distance
    public float minDistance = 8f;  // Minimum distance to maintain from player

    private Gun currentGun;
    private float fireRateDelta;
    private bool isShooting = false;

    [Header("Color Settings")]
    [Range(0f, 1f)]
    public float darknessIntensity = 0.5f;
    private Color originalColor;
    private Renderer enemyRenderer;

    void Start()
    {
        currentHealth = maxHealth;
        ragdoll.SetActive(false);
        agent = GetComponent<NavMeshAgent>();
        currentGun = GetComponentInChildren<Gun>();

        enemyRenderer = GetComponentInChildren<Renderer>();
        if (enemyRenderer != null)
        {
            originalColor = enemyRenderer.material.color;
            DarkenEnemyColor(darknessIntensity);
        }

        PlayerMotor playerMotor = FindObjectOfType<PlayerMotor>();
        playerAnimator = GetComponent<PlayerAnimator>();
        player = playerMotor.transform;

        if (walkingSound != null)
        {
            walkingSound.loop = true;
            SoundManager.Instance.PlaySound(SoundManager.Instance.effects[11]);
        }

        nextGroanTime = Random.Range(5f, 15f);
    }

    private void DarkenEnemyColor(float intensity)
    {
        if (enemyRenderer == null) return;

        Color darkenedColor = new Color(
            originalColor.r * (1f - intensity),
            originalColor.g * (1f - intensity),
            originalColor.b * (1f - intensity),
            originalColor.a
        );

        enemyRenderer.material.color = darkenedColor;
    }

    private void OnValidate()
    {
        if (enemyRenderer != null && !Application.isPlaying)
        {
            DarkenEnemyColor(darknessIntensity);
        }
    }

    void Update()
    {
        if (currentState == EnemyState.Dead) return;

        HandleGroans();

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        Vector3 lookAtPosition = new Vector3(player.position.x, transform.position.y, player.position.z);
        transform.LookAt(lookAtPosition);

        float distance = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case EnemyState.Pathing:
                HandlePathing(distance);
                break;

            case EnemyState.Attacking:
                HandleShooting(distance);
                break;
        }
    }

    public void TakeDamage(int damage, bool knockback)
    {
        currentHealth -= damage;
        SoundManager.Instance.PlaySound(SoundManager.Instance.effects[6]);

        if (knockback)
        {
            ragdoll.GetComponent<Rigidbody>().AddForce(new Vector3(0, 20, 20));
        }

        if (hitEffect != null)
        {
            hitEffect.Play();
        }
    }

    private void Die()
    {
        currentState = EnemyState.Dead;
        animator.enabled = false;
        ragdoll.SetActive(true);
        ragdoll.GetComponent<Rigidbody>().AddForce(new Vector3(0, 0, 20));

        if (walkingSound != null && walkingSound.isPlaying)
            walkingSound.Stop();

        if (dropItemPrefab != null)
            Instantiate(dropItemPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject, destroyDelay);
    }

    private void HandleGroans()
    {
        groanTimer += Time.deltaTime;
        if (groanTimer >= nextGroanTime)
        {
            SoundManager.Instance.PlaySound(SoundManager.Instance.effects[5]);
            groanTimer = 0f;
            nextGroanTime = Random.Range(10f, 20f);
        }
    }

    private void HandlePathing(float distance)
    {
        if (distance <= attackRange && distance >= minDistance)
        {
            currentState = EnemyState.Attacking;
            agent.isStopped = true;
            return;
        }

        agent.isStopped = false;

        if (distance > attackRange)
        {
            agent.destination = player.position;
        }
        else if (distance < minDistance)
        {
            Vector3 directionAway = transform.position - player.position;
            agent.destination = transform.position + directionAway.normalized * minDistance;
        }

        SetAnimationTrigger("trWalk");
    }

    private void SetAnimationTrigger(string triggerName)
    {
        if (currentAnimTrigger == triggerName) return;

        animator.ResetTrigger(currentAnimTrigger);

        currentAnimTrigger = triggerName;
        playerAnimator?.PlayAnimation(triggerName);
    }

    private void HandleShooting(float distance)
    {
        if (distance > attackRange || distance < minDistance)
        {
            currentState = EnemyState.Pathing;
            return;
        }

        fireRateDelta -= Time.deltaTime;
        if (fireRateDelta <= 0)
        {
            SetAnimationTrigger("trIdle");
            currentGun.Fire();
            fireRateDelta = currentGun.GetRateOfFire();
        }
    }
}