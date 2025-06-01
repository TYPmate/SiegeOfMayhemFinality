using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerMotor : MonoBehaviour
{
    CharacterController controller;
    PlayerAnimator playerAnimator;
    public GameObject bayonet;
    Vector3 playerVelocity;
    Vector2 movementVector;
    public Transform cameraTransform;
    public float maxHealth = 1500;
    public float health = 1500;
    [SerializeField] bool doHealthDrain = true;
    [SerializeField] float healthDrain = 3;

    float healthDrainTimer = 0, timeToNextDrain = 1f;


    public TextMeshProUGUI ammoTypeText, newAmmoSelectorText;
    public int ammoType;

    public GameObject AmmoPicMulty;
    public GameObject AmmoPicSmite;
    public GameObject AmmoPicNormal;

    public delegate void HealthChanged(float currentHealth, float maxHealth);
    public event HealthChanged OnHealthChanged;

    public bool ShouldMove = true;
    public float sprintTimeElapsed;
    public float sprintLerpDuration = 0.2f;
    public float maxSpeed = 8f;
    public float speed = 8f;
    public float sprintSpeed = 14f;
    public bool isGrounded;
    public bool isSlowed;
    public bool allowedToSprint = true;
    public float gravity = -9.82f;
    public float defaultJumpHeight = 3f;
    public float currentJumpHeight;
    public bool crouching = false;
    public float crouchTimer = 1;
    public bool lerpCrouch = false;
    public bool sprinting = false;
    [SerializeField] private Bayonet bayonetScript;
    bool bayonetActive = false;


    public bool hasKey = false;
    public bool hasMultipleAmmo = false, hasSmiteAmmo = false, hasKnockbackAmmo = false;
    private bool isWaitingForNextReloadStep = false;

    public enum AmmoTypes { Default, Multiple, Smite, Knockback };
    public AmmoTypes currentAmmoType = AmmoTypes.Default;

    public InputActionAsset inputActions; // Drag your .inputactions asset here in Inspector
    private InputAction anyKeyAction;

    public AudioSource ramrodSound, gunpowderSound;
    public AudioSource bulletSound;

    public Vector3 knockbackVelocity;
    public float knockbackTimer;

    private Coroutine speedBoostCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerAnimator = GetComponent<PlayerAnimator>();
        currentJumpHeight = defaultJumpHeight;
        bayonetScript.SetVisible(false);
        bayonetActive = false;
        transform.rotation = Quaternion.Euler(0, 40f, 0); // ← set player facing 40 degrees
    }


    private void OnEnable()
    {
        var playerMap = inputActions.FindActionMap("Player", true);
        anyKeyAction = playerMap.FindAction("AnyKey", true);

        anyKeyAction.performed += OnKeyPressed;
        anyKeyAction.Enable();

        Debug.Log("AnyKey action enabled.");
    }

    private void OnDisable()
    {
        anyKeyAction.performed -= OnKeyPressed;
        anyKeyAction.Disable();
    }

    private void OnKeyPressed(InputAction.CallbackContext context)
    {
        var weapon = GetComponent<Arquebus>();
        if (!weapon.isReloading || isWaitingForNextReloadStep)
            return;

        string keyPressed = GetLastPressedKey();
        // Only arrow‐keys count
        if (keyPressed != "leftarrow" && keyPressed != "rightarrow" && keyPressed != "uparrow")
            return;

        bool correct = keyPressed == weapon.comboSequence[weapon.currentComboStep];
        weapon.MarkReloadStep(correct);

        if (!correct)
        {
            // mark the slot red...
            weapon.MarkReloadStep(false);
            // then after a delay, cancel and hide
            StartCoroutine(FinishFailedReload(weapon));
            return;
        }

        // correct key → next step
        if (weapon.currentComboStep >= weapon.comboSequence.Length)
        {
            // all done
            weapon.FinalizeReload();
            playerAnimator.PlayAnimation("TrMove");
        }
        else
        {
            StartCoroutine(ReloadStepDelay(weapon, weapon.currentComboStep));
        }
    }

    private IEnumerator FinishFailedReload(Arquebus weapon)
    {
        // let the player see the red highlight for 0.5s
        yield return new WaitForSeconds(0.5f);
        weapon.CancelReload();
    }

    private void ReloadAnimation(int currentReloadStep)
    {
        switch (currentReloadStep)
        {
            case 0:

                playerAnimator.PlayAnimation("TrBeginReload");

                Debug.Log("beginReload");
                break;
            case 1:

                playerAnimator.PlayAnimation("TrCleanBarrel");

                Debug.Log("cleanBarrel");

                if (!ramrodSound.isPlaying)
                {
                    SoundManager.Instance.PlaySound(SoundManager.Instance.effects[9]);
                }
                break;
            case 2:

                playerAnimator.PlayAnimation("TrGunPowder");

                Debug.Log("gunpowder");

                if (!gunpowderSound.isPlaying)
                {
                    SoundManager.Instance.StopSound(SoundManager.Instance.effects[9]);
                    SoundManager.Instance.PlaySound(SoundManager.Instance.effects[7]);
                }
                break;
            case 3:

                playerAnimator.PlayAnimation("TrProjectile");

                Debug.Log("projectile");

                if (!gunpowderSound.isPlaying)
                {
                    SoundManager.Instance.StopSound(SoundManager.Instance.effects[7]);
                    SoundManager.Instance.PlaySound(SoundManager.Instance.effects[2]);
                }
                break;
            case 4:

                playerAnimator.PlayAnimation("TrRamrod");

                Debug.Log("ramrod");

                if (!ramrodSound.isPlaying)
                {
                    SoundManager.Instance.PlaySound(SoundManager.Instance.effects[2]);
                    SoundManager.Instance.PlaySound(SoundManager.Instance.effects[9]);
                }
                break;
            case 5:

                playerAnimator.PlayAnimation("TrEndReload");

                Debug.Log("reloaded");

                SoundManager.Instance.StopSound(SoundManager.Instance.effects[9]);

                break;
        }
    }

    private string GetLastPressedKey()
    {
        foreach (var keyControl in Keyboard.current.allKeys)
        {
            if (keyControl.wasPressedThisFrame)
            {
                return keyControl.name.ToLower();
            }
        }
        return "";
    }

    private void OnMovement(InputValue input)
    {
        movementVector = new Vector2(input.Get<Vector2>().x, input.Get<Vector2>().y);


    }

    private void OnJump(InputValue input)
    {
        Jump();
    }
    public void OnCrouch()
    {
        crouching = !crouching;
        crouchTimer = 0;
        lerpCrouch = true;
    }

    private void OnSprint(InputValue input)
    {
        Sprint();
    }

    private void OnSwapAmmoUp(InputValue input)
    {
        ammoType++;
        if (ammoType > 7)
        {
            ammoType = 0;
        }
    }

    private void OnSwapAmmoDown(InputValue input)
    {
        ammoType--;
        if (ammoType < 0)
        {
            ammoType = 6;
        }
    }

    private void OnReload(InputValue input)
    {
        switch (ammoType)
        {
            case 0:
            case 1:
                currentAmmoType = AmmoTypes.Default;
                AmmoPicMulty.SetActive(false);
                AmmoPicSmite.SetActive(false);
                AmmoPicNormal.SetActive(true);
                break;
            case 2:
            case 3:
                if (hasMultipleAmmo)
                {
                    currentAmmoType = AmmoTypes.Multiple;
                    AmmoPicNormal.SetActive(false);
                    AmmoPicSmite.SetActive(false);
                    AmmoPicMulty.SetActive(true);
                }
                else
                {
                    currentAmmoType = AmmoTypes.Default;
                    ammoType = 0;
                    AmmoPicSmite.SetActive(false);
                    AmmoPicMulty.SetActive(false);
                    AmmoPicNormal.SetActive(true);
                }
                break;
            case 4:
            case 5:
                if (hasSmiteAmmo)
                {
                    currentAmmoType = AmmoTypes.Smite;
                    AmmoPicMulty.SetActive(false);
                    AmmoPicNormal.SetActive(false);
                    AmmoPicSmite.SetActive(true);
                }
                else
                {
                    currentAmmoType = AmmoTypes.Default;
                    ammoType = 0;
                    AmmoPicSmite.SetActive(false);
                    AmmoPicMulty.SetActive(false);
                    AmmoPicNormal.SetActive(true);
                }
                break;
            case 6:
            case 7:
                if (hasKnockbackAmmo)
                {
                    currentAmmoType = AmmoTypes.Knockback;
                    AmmoPicSmite.SetActive(false);
                    AmmoPicMulty.SetActive(false);
                    AmmoPicNormal.SetActive(false);
                }
                else
                {
                    currentAmmoType = AmmoTypes.Default;
                    ammoType = 0;
                    AmmoPicSmite.SetActive(false);
                    AmmoPicMulty.SetActive(false);
                    AmmoPicNormal.SetActive(true);
                }
                break;
        }
        Arquebus weapon = GetComponent<Arquebus>();
        weapon.SelectAmmoType(currentAmmoType.ToString());
    }

    //private void OnShoot(InputValue input)
    //{
    //    Arquebus weapon = GetComponent<Arquebus>();
    //    if (!weapon.isReloading && weapon.currentAmmo > 0 )
    //    {

    //        Vector3 lookDir = Camera.main.transform.forward;
    //        Vector3 oppositeLook = new Vector3(-lookDir.x, -lookDir.y, -lookDir.z).normalized;
    //        ApplyKnockback(oppositeLook,20f,1f);   

    //    }
    //}

    //void ApplyKnockback(Vector3 direction, float force, float knockDuration)
    //{
    //    knockbackVelocity = direction.normalized * force;
    //    knockbackTimer = knockDuration;
    //}


    public void Sprint()
    {
        if (!sprinting)
        {
            sprinting = true;
            if (allowedToSprint)
            {
                speed = sprintSpeed;
                if (sprintTimeElapsed < sprintLerpDuration)
                {
                    speed = Mathf.Lerp(speed, sprintSpeed, sprintTimeElapsed / sprintLerpDuration);
                    sprintTimeElapsed += Time.deltaTime;
                }
                currentJumpHeight = defaultJumpHeight + 12;
            }
        }
        else
        {
            speed = maxSpeed;
            currentJumpHeight = defaultJumpHeight;
            sprinting = false;
        }
    }


    // Update is called once per frame
    void Update()
    {
        healthDrainTimer += Time.deltaTime;
        if (doHealthDrain)
        {
            if (healthDrainTimer > timeToNextDrain)
            {
                healthDrainTimer = 0;
                TakeDamage(healthDrain);
            }
        }
        ammoTypeText.text = currentAmmoType.ToString();
        newAmmoSelectorText.text = ammoType.ToString();
        if (knockbackTimer <= 0)
        {
            ProcessMove(new Vector2(movementVector.x, movementVector.y));
            isGrounded = controller.isGrounded;

        }
        else
        {
            knockbackTimer -= Time.deltaTime;
            movementVector = knockbackVelocity;
            knockbackVelocity = Vector3.Lerp(knockbackVelocity, Vector3.zero, Time.deltaTime * 5f);
            playerVelocity.y += gravity * Time.deltaTime;
            //if (isGrounded && playerVelocity.y < 0)
            //{
            //    playerVelocity.y = -2f;
            //}
            controller.Move(movementVector * Time.deltaTime);
        }

        if (lerpCrouch)
        {
            crouchTimer += Time.deltaTime;
            float p = crouchTimer / 1;
            p *= p;
            if (crouching)
            {
                controller.height = Mathf.Lerp(controller.height, 0.8f, p);
            }
            else
            {
                controller.height = Mathf.Lerp(controller.height, 2, p);
            }
        }
        if (isSlowed)
        {
            speed = 4;
            allowedToSprint = false;
        }
        else
        {
            allowedToSprint = true;
        }

        if (Input.GetKeyUp(KeyCode.B))
        {
            activateBayonet();
        }

        Arquebus weapon = GetComponent<Arquebus>();
        if (movementVector.magnitude > 0)
        {
            playerAnimator.PlayAnimation("TrMove");
        }
        else if (!weapon.isReloading)
        {
            playerAnimator.PlayAnimation("TrIdle");
        }
    }


    public void activateBayonet()
    {
        Arquebus weapon = GetComponent<Arquebus>();

        if (bayonetScript.IsReady() && !weapon.isReloading)
        {
            bayonetScript.Rearm();
            bayonetScript.SetVisible(true);
            bayonetActive = true;
        }

    }



    public void ProcessMove(Vector2 input)
    {
        if (cameraTransform == null)
        {
            Debug.LogWarning("Camera Transform is not assigned!");
            return;
        }

        Vector3 moveDirection = (cameraTransform.forward * input.y + cameraTransform.right * input.x);
        moveDirection.y = 0f; // Prevent moving vertically
        moveDirection.Normalize();

        if (ShouldMove)
        {
            if (!isGrounded)
            {
                if (!sprinting)
                    moveDirection *= 0.6f;
                else
                    moveDirection *= 0.9f;
            }
            controller.Move(moveDirection * speed * Time.deltaTime);
        }

        playerVelocity.y += gravity * Time.deltaTime;
        if (isGrounded && playerVelocity.y < 0)
            playerVelocity.y = -2f;

        controller.Move(playerVelocity * Time.deltaTime);
    }

    public void Jump()
    {
        if (isGrounded && ShouldMove)
        {
            playerVelocity.y = Mathf.Sqrt(currentJumpHeight - 3f * gravity);
        }
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        health = Mathf.Clamp(health, 0, maxHealth);
        Debug.Log("Player took damage! Current health: " + health);

        OnHealthChanged?.Invoke(health, maxHealth); // fire the event

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player died!");
        doHealthDrain = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private IEnumerator ReloadStepDelay(Arquebus weapon, int currentStep)
    {
        isWaitingForNextReloadStep = true;

        ReloadAnimation(currentStep);

        yield return new WaitForSeconds(0.2f);

        isWaitingForNextReloadStep = false;
    }

    void OnPerfectReload()
    {
        if (speedBoostCoroutine == null)
            speedBoostCoroutine = StartCoroutine(DoSpeedBoost());
    }

    private IEnumerator DoSpeedBoost()
    {
        float originalMaxSpeed = maxSpeed;
        float originalSprintSpeed = sprintSpeed;
        float originalSpeed = speed;

        maxSpeed *= GetComponent<Arquebus>().speedBoostAmount;
        sprintSpeed *= GetComponent<Arquebus>().speedBoostAmount;
        speed *= GetComponent<Arquebus>().speedBoostAmount;

        yield return new WaitForSeconds(GetComponent<Arquebus>().speedBoostDuration);

        maxSpeed = originalMaxSpeed;
        sprintSpeed = originalSprintSpeed;
        speed = originalSpeed;
        speedBoostCoroutine = null;
    }
}