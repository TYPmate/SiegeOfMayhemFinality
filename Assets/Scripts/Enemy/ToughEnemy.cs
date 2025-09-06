using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Controls tough enemy behavior including combat, abilities, and boss functionality
/// </summary>
public class ToughEnemy : MonoBehaviour
{
    // References and components
    public Transform player;
    private NavMeshAgent agent;
    public GameObject ragdoll;
    public Animator animator;
    private PlayerAnimator playerAnimator;
    public GameObject attackHitboxObject;
    private Collider attackHitbox;
    private EnemyAttackHitbox hitboxScript;
    private Renderer enemyRenderer;
    BossEnemy bossComponent;

    // Stats and configuration
    public float currentHealth = 100, maxHealth = 100;
    public float attackDuration = 2.6f;
    private float attackCooldown = 1.4f;
    public float dropChance = 0.5f;
    public float destroyDelay = 2f;
    public float distanceBeforeStop = 2f;
    public float darknessIntensity = 1f;
    [SerializeField] float abilityTimer, timeToNextAbility = 5f;

    // Audio/visual effects
    public AudioSource walkingSound;
    public ParticleSystem hitEffect;
    public AudioSource groanSound;
    public AudioSource gruntSound;
    public GameObject dropItemPrefab;

    // State management
    private enum EnemyState { Pathing, Attacking, UsingAbility, Dead }
    private EnemyState currentState = EnemyState.Pathing;
    private bool attackToggle = false;
    private float groanTimer = 0f;
    private float nextGroanTime = 0f;
    private string currentAnimTrigger = "";
    private Color originalColor;

    void Start()
    {
        InitializeComponents();
        SetupInitialState();
    }

    void Update()
    {
        if (currentState == EnemyState.Dead) return;

        abilityTimer += Time.deltaTime;
        HandleGroans();

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        HandleStateBehavior();
    }

    /// <summary>
    /// Initializes all required components and references
    /// </summary>
    void InitializeComponents()
    {
        // Check for boss component
        try
        {
            if (GetComponent<BossEnemy>() != null)
            {
                bossComponent = (BossEnemy)GetComponent<BossEnemy>();
                timeToNextAbility = 10f;
            }
        }
        catch { }

        agent = GetComponent<NavMeshAgent>();
        enemyRenderer = GetComponentInChildren<Renderer>();
        PlayerMotor playerMotor = (PlayerMotor)FindObjectOfType(typeof(PlayerMotor));
        playerAnimator = GetComponent<PlayerAnimator>();
        player = playerMotor.transform;

        // Initialize hitbox
        attackHitbox = attackHitboxObject.GetComponent<Collider>();
        hitboxScript = attackHitboxObject.GetComponent<EnemyAttackHitbox>();
        attackHitbox.enabled = false;

        // Setup audio
        if (walkingSound != null)
        {
            walkingSound.loop = true;
            SoundManager.Instance.PlaySound(SoundManager.Instance.effects[11]);
        }

        // Setup visual appearance
        if (enemyRenderer != null)
        {
            originalColor = enemyRenderer.material.color;
            DarkenEnemyColor(darknessIntensity);
        }
    }

    /// <summary>
    /// Sets up initial enemy state and values
    /// </summary>
    void SetupInitialState()
    {
        currentHealth = maxHealth;
        ragdoll.SetActive(false);
        nextGroanTime = Random.Range(5f, 15f);
    }

    /// <summary>
    /// Darkens the enemy's material color
    /// </summary>
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

    /// <summary>
    /// Handles behavior based on current enemy state
    /// </summary>
    void HandleStateBehavior()
    {
        switch (currentState)
        {
            case EnemyState.Pathing:
                HandlePathing();
                break;
            case EnemyState.Attacking:
                HandleAttackingState();
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

        hitEffect?.Play();
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

        walkingSound?.Stop();

        if (dropItemPrefab != null && Random.value <= dropChance)
        {
            Instantiate(dropItemPrefab, transform.position, Quaternion.identity);
        }

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

        if (distance <= distanceBeforeStop)
        {
            currentState = EnemyState.Attacking;
            agent.isStopped = true;
            SetAnimationTrigger("trAttack");
        }
        else
        {
            agent.isStopped = false;
            agent.destination = player.position;

            Vector3 lookAtPosition = new Vector3(player.position.x, transform.position.y, player.position.z);
            transform.LookAt(lookAtPosition);

            SetAnimationTrigger(agent.velocity.magnitude > 0.1f ? "trWalk" : "trIdle");
        }
    }

    /// <summary>
    /// Handles behavior when in attacking state
    /// </summary>
    void HandleAttackingState()
    {
        if (bossComponent != null && abilityTimer > timeToNextAbility)
        {
            HandleBossAbility();
        }
        else
        {
            ResetToPath();
        }
    }

    /// <summary>
    /// Handles boss-specific ability usage
    /// </summary>
    void HandleBossAbility()
    {
        switch (Random.Range(0, 10))
        {
            case 0:
            case 1:
                currentState = EnemyState.UsingAbility;
                agent.isStopped = false;
                SetAnimationTrigger("trIdle");
                float abilityTime = bossComponent.UseAbility();
                abilityTimer = 0;
                if (abilityTime == 3f)
                {
                    ThrowAbility(abilityTime);
                }
                break;
            default:
                ResetToPath();
                break;
        }
    }

    /// <summary>
    /// Transitions back to pathing state if player moves out of range
    /// </summary>
    void ResetToPath()
    {
        if (Vector3.Distance(transform.position, player.position) > distanceBeforeStop)
        {
            currentState = EnemyState.Pathing;
            agent.isStopped = false;
            SetAnimationTrigger("trIdle");
        }
    }

    /// <summary>
    /// Initiates ability throwing sequence
    /// </summary>
    void ThrowAbility(float cooldown)
    {
        StartCoroutine(AbilityCastTime(cooldown));
        StartCoroutine(ThrowUntilRockTime(1.5f));
    }

    // Animation event methods
    public void activateHurtBox() => EnableAttackHitbox(true);
    public void deActivateHurtBox() => EnableAttackHitbox(false);
    public void endAttack() => StartCoroutine(AttackCooldown());

    /// <summary>
    /// Enables/disables attack hitbox and checks for immediate collisions
    /// </summary>
    void EnableAttackHitbox(bool enable)
    {
        attackHitbox.enabled = enable;
        if (enable)
        {
            hitboxScript.ResetHit();
            CheckImmediateHits();
        }
    }

    /// <summary>
    /// Checks for immediate collisions when hitbox is activated
    /// </summary>
    void CheckImmediateHits()
    {
        Collider[] hits = Physics.OverlapBox(
            attackHitbox.transform.position,
            attackHitbox.bounds.extents,
            attackHitbox.transform.rotation
        );

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                hitboxScript.ForceHit(hit);
            }
        }
    }

    /// <summary>
    /// Handles attack cooldown and state transition
    /// </summary>
    private IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);

        if (Vector3.Distance(transform.position, player.position) < distanceBeforeStop)
        {
            attackToggle = !attackToggle;
            SetAnimationTrigger(attackToggle ? "trReAttack" : "trAttack");
        }
        else
        {
            currentState = EnemyState.Pathing;
            agent.isStopped = false;
            SetAnimationTrigger("trIdle");
        }
    }

    // Coroutines for ability timing
    private IEnumerator AbilityCastTime(float cooldown)
    {
        yield return new WaitForSeconds(cooldown);
        currentState = EnemyState.Pathing;
        agent.isStopped = false;
        SetAnimationTrigger("trIdle");
    }

    private IEnumerator ThrowUntilRockTime(float timeToWait)
    {
        yield return new WaitForSeconds(timeToWait);
        Instantiate(bossComponent.rockPrefab, transform, false);
    }

    // Editor validation for color preview
    private void OnValidate()
    {
        if (enemyRenderer != null && !Application.isPlaying)
        {
            DarkenEnemyColor(darknessIntensity);
        }
    }
}