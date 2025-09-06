using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Main player controller handling movement, combat, health, and item management
/// </summary>
public class PlayerMotor : MonoBehaviour
{
    // Components and references
    CharacterController controller;
    PlayerAnimator playerAnimator;
    public GameObject bayonet;
    Vector3 playerVelocity;
    Vector2 movementVector;
    public Transform cameraTransform;

    // Health system
    public float maxHealth = 1500;
    public float health = 1500;
    [SerializeField] bool doHealthDrain = true;
    [SerializeField] float healthDrain = 3;
    float healthDrainTimer = 0, timeToNextDrain = 1f;

    // Ammo system
    public TextMeshProUGUI ammoTypeText, newAmmoSelectorText;
    public int ammoType;
    public GameObject AmmoPicMulty, AmmoPicSmite, AmmoPicNormal;
    public bool hasKey = false;
    public bool hasMultipleAmmo = false, hasSmiteAmmo = false, hasKnockbackAmmo = false;

    // Movement system
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

    // Combat system
    [SerializeField] private Bayonet bayonetScript;
    bool bayonetActive = false;
    public Vector3 knockbackVelocity;
    public float knockbackTimer;

    // Ammo types
    public enum AmmoTypes { Default, Multiple, Smite, Knockback };
    public AmmoTypes currentAmmoType = AmmoTypes.Default;

    // Input system
    public InputActionAsset inputActions;
    private InputAction anyKeyAction;
    private bool isWaitingForNextReloadStep = false;

    // Audio
    public AudioSource ramrodSound, gunpowderSound, bulletSound;
    private Coroutine speedBoostCoroutine;

    // Events
    public delegate void HealthChanged(float currentHealth, float maxHealth);
    public event HealthChanged OnHealthChanged;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerAnimator = GetComponent<PlayerAnimator>();
        currentJumpHeight = defaultJumpHeight;
        bayonetScript.SetVisible(false);
        bayonetActive = false;
        transform.rotation = Quaternion.Euler(0, 40f, 0);
    }

    private void OnEnable()
    {
        var playerMap = inputActions.FindActionMap("Player", true);
        anyKeyAction = playerMap.FindAction("AnyKey", true);
        anyKeyAction.performed += OnKeyPressed;
        anyKeyAction.Enable();
    }

    private void OnDisable()
    {
        anyKeyAction.performed -= OnKeyPressed;
        anyKeyAction.Disable();
    }

    void Update()
    {
        HandleHealthDrain();
        UpdateUI();
        HandleMovement();
        HandleCrouching();
        HandleSlowedState();
        HandleBayonetInput();
        HandleAnimations();
    }

    /// <summary>
    /// Handles continuous health drain if enabled
    /// </summary>
    void HandleHealthDrain()
    {
        healthDrainTimer += Time.deltaTime;
        if (doHealthDrain && healthDrainTimer > timeToNextDrain)
        {
            healthDrainTimer = 0;
            TakeDamage(healthDrain);
        }
    }

    /// <summary>
    /// Updates UI elements with current ammo information
    /// </summary>
    void UpdateUI()
    {
        ammoTypeText.text = currentAmmoType.ToString();
        newAmmoSelectorText.text = ammoType.ToString();
    }

    /// <summary>
    /// Handles player movement and knockback effects
    /// </summary>
    void HandleMovement()
    {
        if (knockbackTimer <= 0)
        {
            ProcessMove(new Vector2(movementVector.x, movementVector.y));
            isGrounded = controller.isGrounded;
        }
        else
        {
            HandleKnockback();
        }
    }

    /// <summary>
    /// Handles knockback movement when player is hit
    /// </summary>
    void HandleKnockback()
    {
        knockbackTimer -= Time.deltaTime;
        movementVector = knockbackVelocity;
        knockbackVelocity = Vector3.Lerp(knockbackVelocity, Vector3.zero, Time.deltaTime * 5f);
        playerVelocity.y += gravity * Time.deltaTime;
        controller.Move(movementVector * Time.deltaTime);
    }

    /// <summary>
    /// Handles crouching state and height adjustment
    /// </summary>
    void HandleCrouching()
    {
        if (lerpCrouch)
        {
            crouchTimer += Time.deltaTime;
            float p = crouchTimer / 1;
            p *= p;
            controller.height = Mathf.Lerp(controller.height, crouching ? 0.8f : 2, p);
        }
    }

    /// <summary>
    /// Handles slowed state effects on movement
    /// </summary>
    void HandleSlowedState()
    {
        if (isSlowed)
        {
            speed = 4;
            allowedToSprint = false;
        }
        else
        {
            allowedToSprint = true;
        }
    }

    /// <summary>
    /// Handles bayonet activation input
    /// </summary>
    void HandleBayonetInput()
    {
        if (Input.GetKeyUp(KeyCode.B))
        {
            activateBayonet();
        }
    }

    /// <summary>
    /// Handles player animations based on movement state
    /// </summary>
    void HandleAnimations()
    {
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

    // Input system callbacks
    private void OnMovement(InputValue input) => movementVector = input.Get<Vector2>();
    private void OnJump(InputValue input) => Jump();
    private void OnSprint(InputValue input) => Sprint();
    private void OnCrouch() => crouching = !crouching;

    /// <summary>
    /// Handles ammo type switching
    /// </summary>
    private void OnSwapAmmoUp(InputValue input)
    {
        ammoType = (ammoType + 1) % 8;
        UpdateAmmoType();
    }

    /// <summary>
    /// Handles ammo type switching
    /// </summary>
    private void OnSwapAmmoDown(InputValue input)
    {
        ammoType = (ammoType - 1 + 8) % 8;
        UpdateAmmoType();
    }

    /// <summary>
    /// Updates current ammo type based on selection and availability
    /// </summary>
    private void OnReload(InputValue input) => UpdateAmmoType();

    /// <summary>
    /// Handles reload mini-game key presses
    /// </summary>
    private void OnKeyPressed(InputAction.CallbackContext context)
    {
        var weapon = GetComponent<Arquebus>();
        if (!weapon.isReloading || isWaitingForNextReloadStep) return;

        string keyPressed = GetLastPressedKey();
        if (keyPressed != "leftarrow" && keyPressed != "rightarrow" && keyPressed != "uparrow") return;

        bool correct = keyPressed == weapon.comboSequence[weapon.currentComboStep];
        weapon.MarkReloadStep(correct);

        if (!correct)
        {
            StartCoroutine(FinishFailedReload(weapon));
            return;
        }

        if (weapon.currentComboStep >= weapon.comboSequence.Length)
        {
            weapon.FinalizeReload();
            playerAnimator.PlayAnimation("TrMove");
        }
        else
        {
            StartCoroutine(ReloadStepDelay(weapon, weapon.currentComboStep));
        }
    }

    /// <summary>
    /// Toggles sprint state and adjusts movement parameters
    /// </summary>
    public void Sprint()
    {
        sprinting = !sprinting;

        if (sprinting && allowedToSprint)
        {
            speed = Mathf.Lerp(speed, sprintSpeed, sprintTimeElapsed / sprintLerpDuration);
            currentJumpHeight = defaultJumpHeight + 12;
        }
        else
        {
            speed = maxSpeed;
            currentJumpHeight = defaultJumpHeight;
        }
    }

    /// <summary>
    /// Processes player movement based on input
    /// </summary>
    public void ProcessMove(Vector2 input)
    {
        if (cameraTransform == null) return;

        Vector3 moveDirection = (cameraTransform.forward * input.y + cameraTransform.right * input.x);
        moveDirection.y = 0f;
        moveDirection.Normalize();

        if (ShouldMove)
        {
            if (!isGrounded) moveDirection *= (sprinting ? 0.9f : 0.6f);
            controller.Move(moveDirection * speed * Time.deltaTime);
        }

        playerVelocity.y += gravity * Time.deltaTime;
        if (isGrounded && playerVelocity.y < 0) playerVelocity.y = -2f;
        controller.Move(playerVelocity * Time.deltaTime);
    }

    /// <summary>
    /// Makes the player jump if grounded
    /// </summary>
    public void Jump()
    {
        if (isGrounded && ShouldMove)
        {
            playerVelocity.y = Mathf.Sqrt(currentJumpHeight - 3f * gravity);
        }
    }

    /// <summary>
    /// Applies damage to the player and checks for death
    /// </summary>
    public void TakeDamage(float damage)
    {
        health -= damage;
        health = Mathf.Clamp(health, 0, maxHealth);
        OnHealthChanged?.Invoke(health, maxHealth);

        if (health <= 0) Die();
    }

    /// <summary>
    /// Handles player death and scene reset
    /// </summary>
    private void Die()
    {
        doHealthDrain = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Activates bayonet if ready and not reloading
    /// </summary>
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

    // Coroutines for reload system
    private IEnumerator FinishFailedReload(Arquebus weapon)
    {
        yield return new WaitForSeconds(0.5f);
        weapon.CancelReload();
    }

    private IEnumerator ReloadStepDelay(Arquebus weapon, int currentStep)
    {
        isWaitingForNextReloadStep = true;
        ReloadAnimation(currentStep);
        yield return new WaitForSeconds(0.2f);
        isWaitingForNextReloadStep = false;
    }

    /// <summary>
    /// Plays appropriate reload animation for current step
    /// </summary>
    private void ReloadAnimation(int currentReloadStep)
    {
        switch (currentReloadStep)
        {
            case 0: playerAnimator.PlayAnimation("TrBeginReload"); break;
            case 1: playerAnimator.PlayAnimation("TrCleanBarrel"); PlaySound(SoundManager.Instance.effects[9]); break;
            case 2: playerAnimator.PlayAnimation("TrGunPowder"); PlaySound(SoundManager.Instance.effects[7]); break;
            case 3: playerAnimator.PlayAnimation("TrProjectile"); PlaySound(SoundManager.Instance.effects[2]); break;
            case 4: playerAnimator.PlayAnimation("TrRamrod"); PlaySound(SoundManager.Instance.effects[9]); break;
            case 5: playerAnimator.PlayAnimation("TrEndReload"); break;
        }
    }

    /// <summary>
    /// Gets the last pressed key for reload mini-game
    /// </summary>
    private string GetLastPressedKey()
    {
        foreach (var keyControl in Keyboard.current.allKeys)
        {
            if (keyControl.wasPressedThisFrame) return keyControl.name.ToLower();
        }
        return "";
    }

    /// <summary>
    /// Updates current ammo type based on selection wheel
    /// </summary>
    private void UpdateAmmoType()
    {
        switch (ammoType)
        {
            case 0: case 1: SetAmmoType(AmmoTypes.Default, AmmoPicNormal); break;
            case 2: case 3: SetAmmoTypeIfAvailable(AmmoTypes.Multiple, hasMultipleAmmo, AmmoPicMulty); break;
            case 4: case 5: SetAmmoTypeIfAvailable(AmmoTypes.Smite, hasSmiteAmmo, AmmoPicSmite); break;
            case 6: case 7: SetAmmoTypeIfAvailable(AmmoTypes.Knockback, hasKnockbackAmmo, null); break;
        }

        GetComponent<Arquebus>().SelectAmmoType(currentAmmoType.ToString());
    }

    /// <summary>
    /// Sets ammo type if player has it available
    /// </summary>
    private void SetAmmoTypeIfAvailable(AmmoTypes type, bool hasAmmo, GameObject ammoPic)
    {
        if (hasAmmo)
        {
            SetAmmoType(type, ammoPic);
        }
        else
        {
            currentAmmoType = AmmoTypes.Default;
            ammoType = 0;
            ResetAmmoPics();
            AmmoPicNormal.SetActive(true);
        }
    }

    /// <summary>
    /// Sets ammo type and updates UI
    /// </summary>
    private void SetAmmoType(AmmoTypes type, GameObject activePic)
    {
        currentAmmoType = type;
        ResetAmmoPics();
        if (activePic != null) activePic.SetActive(true);
    }

    /// <summary>
    /// Resets all ammo type UI indicators
    /// </summary>
    private void ResetAmmoPics()
    {
        AmmoPicMulty.SetActive(false);
        AmmoPicSmite.SetActive(false);
        AmmoPicNormal.SetActive(false);
    }

    /// <summary>
    /// Applies speed boost from perfect reload
    /// </summary>
    void OnPerfectReload()
    {
        if (speedBoostCoroutine == null)
            speedBoostCoroutine = StartCoroutine(DoSpeedBoost());
    }

    /// <summary>
    /// Coroutine to handle temporary speed boost
    /// </summary>
    private IEnumerator DoSpeedBoost()
    {
        Arquebus weapon = GetComponent<Arquebus>();
        float originalMaxSpeed = maxSpeed;
        float originalSprintSpeed = sprintSpeed;
        float originalSpeed = speed;

        maxSpeed *= weapon.speedBoostAmount;
        sprintSpeed *= weapon.speedBoostAmount;
        speed *= weapon.speedBoostAmount;

        yield return new WaitForSeconds(weapon.speedBoostDuration);

        maxSpeed = originalMaxSpeed;
        sprintSpeed = originalSprintSpeed;
        speed = originalSpeed;
        speedBoostCoroutine = null;
    }

    /// <summary>
    /// Helper method to play sounds through SoundManager
    /// </summary>
    private void PlaySound(AudioClip clip)
    {
        SoundManager.Instance.PlaySound(clip);
    }
}