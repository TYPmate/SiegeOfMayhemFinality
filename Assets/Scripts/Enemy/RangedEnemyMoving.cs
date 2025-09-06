using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Controls ranged enemy movement, shooting behavior, and combat states
/// </summary>
public class RangedEnemyMoving : MonoBehaviour
{
    // References and components
    public Transform player;
    private NavMeshAgent agent;
    public GameObject ragdoll;
    public Animator animator;
    private PlayerAnimator playerAnimator;
    private Gun currentGun;

    // Stats and configuration
    public float currentHealth = 25, maxHealth = 25;
    public float attackDuration = 2.6f;
    private float attackCooldown = 1.4f;
    public float destroyDelay = 2f;
    public float attackRange = 10f;  // Distance to stop and shoot
    public float minDistance = 5f;   // Minimum distance to maintain

    // Audio/visual effects
    public AudioSource walkingSound;
    public ParticleSystem hitEffect;
    public AudioSource groanSound;
    public AudioSource gruntSound;
    public GameObject dropItemPrefab;

    // State management
    private enum EnemyState { Pathing, Attacking, Dead }
    private EnemyState currentState = EnemyState.Pathing;
    private bool attackToggle = false;
    private float groanTimer = 0f;
    private float nextGroanTime = 0f;
    private string currentAnimTrigger = "";
    private float fireRateDelta;

    void Start()
    {
        currentHealth = maxHealth;
        ragdoll.SetActive(false);
        agent = GetComponent<NavMeshAgent>();
        currentGun = GetComponentInChildren<Gun>();

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

    void Update()
    {
        if (currentState == EnemyState.Dead) return;

        HandleGroans();

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        switch (currentState)
        {
            case EnemyState.Pathing:
                HandlePathing();
                break;

            case EnemyState.Attacking:
                HandleAttacking();
                break;
        }
    }

    /// <summary>
    /// Handles taking damage from player attacks
    /// </summary>
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

    /// <summary>
    /// Sets animation trigger with proper reset handling
    /// </summary>
    private void SetAnimationTrigger(string triggerName)
    {
        if (currentAnimTrigger == triggerName) return;

        animator.ResetTrigger(currentAnimTrigger);
        currentAnimTrigger = triggerName;
        playerAnimator?.PlayAnimation(triggerName);
    }

    /// <summary>
    /// Handles enemy death sequence
    /// </summary>
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

    /// <summary>
    /// Handles periodic groan sound playback
    /// </summary>
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

    /// <summary>
    /// Handles pathfinding and movement toward player
    /// </summary>
    private void HandlePathing()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= attackRange && distance >= minDistance)
        {
            currentState = EnemyState.Attacking;
            agent.isStopped = true;
            SetAnimationTrigger("trAttack");
        }
        else
        {
            // Move toward player if too far, or away if too close
            if (distance > attackRange)
            {
                agent.isStopped = false;
                agent.destination = player.position;
            }
            else if (distance < minDistance)
            {
                agent.isStopped = false;
                Vector3 directionAway = transform.position - player.position;
                agent.destination = transform.position + directionAway.normalized;
            }

            // Face the player
            Vector3 lookAtPosition = new Vector3(player.position.x, transform.position.y, player.position.z);
            transform.LookAt(lookAtPosition);

            if (agent.velocity.magnitude > 0.1f)
                SetAnimationTrigger("trWalk");
            else
                SetAnimationTrigger("trIdle");
        }
    }

    /// <summary>
    /// Handles shooting behavior when in attack range
    /// </summary>
    private void HandleAttacking()
    {
        // Face the player while attacking
        Vector3 lookAtPosition = new Vector3(player.position.x, transform.position.y, player.position.z);
        transform.LookAt(lookAtPosition);

        float distance = Vector3.Distance(transform.position, player.position);

        // Shoot at player
        fireRateDelta -= Time.deltaTime;
        if (fireRateDelta <= 0)
        {
            currentGun.Fire();
            fireRateDelta = currentGun.GetRateOfFire();
        }

        // Check if player moved out of range
        if (distance > attackRange || distance < minDistance)
        {
            currentState = EnemyState.Pathing;
            agent.isStopped = false;
            SetAnimationTrigger("trIdle");
        }
    }

    /// <summary>
    /// Animation event method for shooting projectile
    /// </summary>
    public void ShootProjectile()
    {
        currentGun.Fire();
    }

    /// <summary>
    /// Animation event method when attack ends
    /// </summary>
    public void EndAttack()
    {
        StartCoroutine(AttackCooldown());
    }

    /// <summary>
    /// Handles attack cooldown and state transition
    /// </summary>
    private IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= attackRange && distance >= minDistance)
        {
            // Continue attacking
            attackToggle = !attackToggle;
            string nextTrigger = attackToggle ? "trReAttack" : "trAttack";
            SetAnimationTrigger(nextTrigger);
        }
        else
        {
            // Player moved out of range - return to pathing
            currentState = EnemyState.Pathing;
            agent.isStopped = false;
            SetAnimationTrigger("trIdle");
        }
    }
}