using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
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
        UsingAbility,
        Dead
    }

    private EnemyState currentState = EnemyState.Pathing;
    private bool attackToggle = false;

    public float attackDuration = 2.6f;
    private float attackCooldown = 1.4f;

    public float dropChance = 0.5f;
    public GameObject dropItemPrefab;

    private PlayerAnimator playerAnimator;

    public AudioSource walkingSound;

    public ParticleSystem hitEffect;
    public AudioSource groanSound;
    public AudioSource gruntSound;
    private float groanTimer = 0f;
    private float nextGroanTime = 0f;
    private string currentAnimTrigger = "";

    public float destroyDelay = 2f;
    public float distanceBeforeStop = 2f;

    public GameObject attackHitboxObject; // assign in Inspector
    private Collider attackHitbox;
    private EnemyAttackHitbox hitboxScript;

    BossEnemy bossComponent;
    [SerializeField] float abilityTimer, timeToNextAbility = 5f;


    void Start()
    {
        try
        {
            if (GetComponent<BossEnemy>() != null)
            {
                Debug.Log("Has boss component");
                bossComponent = (BossEnemy)GetComponent<BossEnemy>();
                timeToNextAbility = 10f;
            }
        }
        catch { }
        currentHealth = maxHealth;
        ragdoll.SetActive(false);
        agent = GetComponent<NavMeshAgent>();
        PlayerMotor playerMotor = (PlayerMotor)FindObjectOfType(typeof(PlayerMotor));
        playerAnimator = GetComponent<PlayerAnimator>();
        player = playerMotor.transform;

        if (walkingSound != null)
        {
            walkingSound.loop = true;
            SoundManager.Instance.PlaySound(SoundManager.Instance.effects[11]);
            //walkingSound.Play();
        }

        nextGroanTime = Random.Range(5f, 15f);

        attackHitbox = attackHitboxObject.GetComponent<Collider>();
        hitboxScript = attackHitboxObject.GetComponent<EnemyAttackHitbox>();
        attackHitbox.enabled = false;
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

        switch (currentState)
        {
            case EnemyState.Pathing:
                HandlePathing();
                break;

            case EnemyState.Attacking:
                if (bossComponent != null)
                {
                    if (abilityTimer > timeToNextAbility)
                    {
                        switch (Random.Range(0, 10))
                        {
                            case 0:
                            case 1:
                                currentState = EnemyState.UsingAbility;
                                agent.isStopped = false;
                                SetAnimationTrigger("trIdle");
                                float abilityTime = bossComponent.UseAbility();
                                Debug.Log("Used Ability with time: " + abilityTime);
                                abilityTimer = 0;
                                if (abilityTime == 3f)
                                {

                                    ThrowAbility(abilityTime);
                                }
                                break;
                            default:
                                float attackDist = Vector3.Distance(transform.position, player.position);
                                if (attackDist > distanceBeforeStop)
                                {
                                    ResetToPath();
                                    Debug.Log("Kept attacking");
                                }
                                break;
                        }
                        break;
                    }
                    else
                    {
                        ResetToPath();
                        break;
                    }
                }
                else
                {
                    ResetToPath();
                    break;
                }


        }
    }

    void ThrowAbility(float cooldown)
    {
        StartCoroutine(AbilityCastTime(cooldown));
        StartCoroutine(ThrowUntilRockTime(1.5f));
    }
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
        Instantiate(GetComponent<BossEnemy>().rockPrefab, this.transform, false);
    }

    void ResetToPath()
    {
        float attackDist = Vector3.Distance(transform.position, player.position);
        if (attackDist > distanceBeforeStop)
        {
            currentState = EnemyState.Pathing;
            agent.isStopped = false;
            SetAnimationTrigger("trIdle");
        }
    }





    public void TakeDamage(int damage, bool knockback)
    {
        currentHealth -= damage;

        //gruntSound.Play();
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

    private void SetAnimationTrigger(string triggerName)
    {
        if (currentAnimTrigger == triggerName) return;

        // Optional: Reset old trigger
        animator.ResetTrigger(currentAnimTrigger);

        currentAnimTrigger = triggerName;
        playerAnimator?.PlayAnimation(triggerName);
    }



    private void Die()
    {
        currentState = EnemyState.Dead;
        animator.enabled = false;
        ragdoll.SetActive(true);
        ragdoll.GetComponent<Rigidbody>().AddForce(new Vector3(0, 0, 20));

        if (walkingSound != null && walkingSound.isPlaying)
            walkingSound.Stop();

        if (dropItemPrefab != null && Random.value <= dropChance)
        {
            Instantiate(dropItemPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject, destroyDelay);
    }

    private void HandleGroans()
    {
        // Groan logic
        groanTimer += Time.deltaTime;
        if (groanTimer >= nextGroanTime)
        {
            //groanSound.Play();
            SoundManager.Instance.PlaySound(SoundManager.Instance.effects[5]);
            groanTimer = 0f;
            nextGroanTime = Random.Range(10f, 20f);
        }
    }
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

            if (agent.velocity.magnitude > 0.1f)
                SetAnimationTrigger("trWalk");
            else
                SetAnimationTrigger("trIdle");
        }
    }

    // Called by Animation Event
    public void activateHurtBox()
    {
        attackHitbox.enabled = true;
        hitboxScript.ResetHit();
        Debug.Log($"activateHurtBox");
        // Immediately check for overlap in case the player is already in range
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


    // Called by Animation Event
    public void deActivateHurtBox()
    {
        attackHitbox.enabled = false;
        Debug.Log($"DeActivateHurtbox");
    }

    // Called by Animation Event
    public void endAttack()
    {
        StartCoroutine(AttackCooldown());
        Debug.Log($"EndAttack");
    }

    private IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance < distanceBeforeStop)
        {
            // Continue attacking
            attackToggle = !attackToggle;
            string nextTrigger = attackToggle ? "trReAttack" : "trAttack";

            Debug.Log($"Triggering next attack animation: {nextTrigger}");

            // Force animation reset
            //animator.Play("idle", 0);
            yield return null;

            SetAnimationTrigger(nextTrigger);

            // Stay in Attacking state — wait for animation event to trigger again
        }
        else
        {
            // Player moved out of range — return to pathing
            currentState = EnemyState.Pathing;
            agent.isStopped = false;
            SetAnimationTrigger("trIdle");
        }
    }
}