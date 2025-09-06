using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Handles arquebus weapon functionality including shooting, reloading, and ammo management
/// </summary>
public class Arquebus : MonoBehaviour
{
    /// <summary>
    /// Represents a single slot in the reload mini-game interface
    /// </summary>
    [System.Serializable]
    public class ReloadSlot
    {
        public Image arrowImage;          // shows up/left/right arrow
        public Image correctHighlight;    // green behind it
        public Image incorrectHighlight;  // red behind it
        public Image timedHighlight;      // yellow behind it
    }

    // Reload mini-game UI elements
    public List<ReloadSlot> reloadSlots;      // exactly 5 elements in the inspector
    public Sprite upArrowSprite, leftArrowSprite, rightArrowSprite;

    // References and components
    public Camera cam;
    public PlayerAnimator playerAnimator;
    public float nextTimeToFire = 0f, timeBetweenShots = 500f;
    public bool isReloading = false;
    public int currentAmmo = 3, maxAmmo = 3, damage = 25;

    // UI elements
    public TextMeshProUGUI ammoText, maxAmmoText;

    // Audio components
    public AudioSource arquebusShot;
    public AudioSource dryFire;

    // Visual effects
    public Light areaLight;
    public ParticleSystem muzzleFlash;
    public ParticleSystem muzzleSmoke1;
    public ParticleSystem muzzleSmoke2;

    // Ammo type system
    public string currentType = "Default";

    // Reload mini-game sequences for different ammo types
    public string[] comboSequence = { "uparrow", "leftarrow", "rightarrow", "leftarrow", "uparrow" };
    public string[] comboRegular = { "uparrow", "leftarrow", "rightarrow", "leftarrow", "uparrow" };
    public string[] comboMultiple = { "leftarrow", "uparrow", "rightarrow", "leftarrow", "uparrow" };
    public string[] comboSmite = { "rightarrow", "leftarrow", "rightarrow", "leftarrow", "uparrow" };
    public string[] comboKnockback = { "uparrow", "leftarrow", "uparrow", "leftarrow", "uparrow" };

    // The current step in the combo sequence
    public int currentComboStep = 0;

    // Camera shake parameters for different ammo types
    public float defaultShakeMag = 0.10f;
    public float multipleShakeMag = 0.5f;
    public float smiteShakeMag = 0.15f;
    public float knockbackShakeMag = 0.10f;
    public float shakeDuration = 0.2f;

    // Reload prompt UI
    public GameObject reloadPromptContainer;
    public Image reloadMarker;
    public RectTransform markerTransform;
    public Image timingBar;

    // Reload timing variables
    private float markerStartX, markerEndX;
    private bool isMarkerMoving = false;
    private float reloadTimer;
    public float markerMoveDuration = 6.3f;

    public float timingWindowDuration = 0.5f;
    private float timingWindowStartTime;
    private bool isInTimingWindow = false;

    // Visual effects for reload prompt
    public ParticleSystem reloadPromptEffect;
    public float speedBoostAmount = 2f;
    public float speedBoostDuration = 5f;

    private bool allStepsTimed;

    /// <summary>
    /// Initialization method called when the script instance is being loaded
    /// </summary>
    void Start()
    {

    }

    /// <summary>
    /// Update is called once per frame to handle weapon state and UI updates
    /// </summary>
    void Update()
    {
        nextTimeToFire += nextTimeToFire + Time.deltaTime / 500;
        Debug.DrawRay(cam.transform.position, cam.transform.forward);

        if (ammoText != null)
        {
            ammoText.text = currentAmmo.ToString();
            maxAmmoText.text = maxAmmo.ToString();
        }

        if (isMarkerMoving)
        {
            reloadTimer += Time.deltaTime;
            float t = reloadTimer / markerMoveDuration;

            if (t >= 1f)
            {
                t = 1f;
                isMarkerMoving = false;
            }

            float newX = Mathf.Lerp(markerStartX, markerEndX, t);
            markerTransform.anchoredPosition = new Vector2(newX, markerTransform.anchoredPosition.y);
        }

        if (isInTimingWindow && Time.time - timingWindowStartTime > timingWindowDuration)
        {
            EndTimingWindow();
        }
    }

    /// <summary>
    /// Selects the current ammo type and configures weapon parameters accordingly
    /// </summary>
    public void SelectAmmoType(string type)
    {
        currentType = type;
        switch (type)
        {
            case "Default":
                comboSequence = comboRegular;
                maxAmmo = 3;
                damage = 75;
                break;
            case "Multiple":
                comboSequence = comboMultiple;
                maxAmmo = 5;
                damage = 20;
                break;
            case "Smite":
                comboSequence = comboSmite;
                maxAmmo = 1;
                damage = 150;
                break;
            case "Knockback":
                comboSequence = comboKnockback;
                maxAmmo = 1;
                damage = 0;
                break;
        }
    }

    /// <summary>
    /// Input system callback for shoot input
    /// </summary>
    private void OnShoot(InputValue input)
    {
        Shoot();
    }

    /// <summary>
    /// Input system callback for reload input
    /// </summary>
    private void OnReload(InputValue input)
    {
        isReloading = true;
        currentComboStep = 0;
        InitializeReloadPrompt();
    }

    /// <summary>
    /// Completes the reload process and refills ammo
    /// </summary>
    public void Reload()
    {
        currentAmmo = maxAmmo;
        isReloading = false;
    }

    /// <summary>
    /// Fires the weapon if ammunition is available and not currently reloading
    /// </summary>
    public void Shoot()
    {
        if (Time.timeScale == 0f)
            return;

        if (!isReloading)
        {
            if (currentAmmo > 0)
            {
                SoundManager.Instance.PlaySound(SoundManager.Instance.effects[1]);

                if (muzzleFlash != null) muzzleFlash.Play();
                if (muzzleSmoke1 != null) muzzleSmoke1.Play();
                if (muzzleSmoke2 != null) muzzleSmoke2.Play();
                if (areaLight != null) StartCoroutine(PlayLightEffect());

                currentAmmo--;
                nextTimeToFire = 0;

                RaycastHit[] hits = Physics.RaycastAll(cam.transform.position, cam.transform.forward, 1000f);
                List<Enemy> enemiesHit = new List<Enemy>();
                List<RangedEnemy> rangedEnemiesHit = new List<RangedEnemy>();
                foreach (var hit in hits)
                {
                    Enemy enemy = hit.transform.GetComponent<Enemy>();
                    if (enemy != null && !enemiesHit.Contains(enemy))
                    {
                        enemiesHit.Add(enemy);
                    }

                    RangedEnemy rangedEnemy = hit.transform.GetComponent<RangedEnemy>();
                    if (rangedEnemy != null && !rangedEnemiesHit.Contains(rangedEnemy))
                    {
                        rangedEnemiesHit.Add(rangedEnemy);
                    }

                    BreakableObject breakable = hit.transform.GetComponent<BreakableObject>();
                    if (breakable != null)
                    {
                        breakable.Break(hit.point);
                    }

                    SmiteOnlyBreakableObject smiteBreakable = hit.transform.GetComponent<SmiteOnlyBreakableObject>();
                    if (smiteBreakable != null && currentType == "Smite")
                    {
                        smiteBreakable.Break(hit.point);
                    }
                }
                switch (currentType)
                {
                    case "Default":
                        foreach (var enemy in enemiesHit)
                        {
                            enemy.TakeDamage(damage, false);
                        }
                        foreach (var rangedEnemy in rangedEnemiesHit)
                        {
                            rangedEnemy.TakeDamage(damage, false);
                        }
                        break;
                    case "Multiple":
                    case "Smite":
                        if (enemiesHit.Count > 0) enemiesHit[0].TakeDamage(damage, false);
                        if (rangedEnemiesHit.Count > 0) rangedEnemiesHit[0].TakeDamage(damage, false);
                        break;
                    case "Knockback":
                        if (enemiesHit.Count > 0) enemiesHit[0].TakeDamage(damage, true);
                        if (rangedEnemiesHit.Count > 0) rangedEnemiesHit[0].TakeDamage(damage, true);
                        break;
                }

                float mag = defaultShakeMag;
                switch (currentType)
                {
                    case "Multiple": mag = multipleShakeMag; break;
                    case "Smite": mag = smiteShakeMag; break;
                    case "Knockback": mag = knockbackShakeMag; break;
                }

                if (CameraShake.Instance != null)
                    CameraShake.Instance.ShakeCamera(shakeDuration, mag);
            }
            else
            {
                SoundManager.Instance.PlaySound(SoundManager.Instance.effects[4]);
            }
        }
    }

    /// <summary>
    /// Coroutine to play muzzle flash light effect
    /// </summary>
    private IEnumerator PlayLightEffect()
    {
        areaLight.intensity = 50f;
        yield return new WaitForSeconds(0.1f);
        areaLight.intensity = 0f;
    }

    /// <summary>
    /// Initializes the reload mini-game UI and starts the process
    /// </summary>
    public void InitializeReloadPrompt()
    {
        allStepsTimed = true;

        timingBar.gameObject.SetActive(true);
        reloadMarker.gameObject.SetActive(true);
        reloadPromptEffect.gameObject.SetActive(true);

        reloadPromptContainer?.SetActive(true);

        for (int i = 0; i < reloadSlots.Count; i++)
        {
            var slot = reloadSlots[i];

            switch (comboSequence[i])
            {
                case "uparrow": slot.arrowImage.sprite = upArrowSprite; break;
                case "leftarrow": slot.arrowImage.sprite = leftArrowSprite; break;
                case "rightarrow": slot.arrowImage.sprite = rightArrowSprite; break;
            }

            slot.arrowImage.gameObject.SetActive(true);
            slot.correctHighlight.gameObject.SetActive(false);
            slot.incorrectHighlight.gameObject.SetActive(false);
            slot.timedHighlight.gameObject.SetActive(false);
        }

        currentComboStep = 0;
        isReloading = true;

        StartReloadMarker();
        StartTimingWindow();
    }

    /// <summary>
    /// Marks a reload step as correct or incorrect and handles progression
    /// </summary>
    public void MarkReloadStep(bool isCorrect)
    {
        bool wasInTime = isInTimingWindow;
        EndTimingWindow();

        var slot = reloadSlots[currentComboStep];

        if (isCorrect && !wasInTime)
            allStepsTimed = false;

        if (wasInTime && isCorrect)
        {
            slot.timedHighlight.gameObject.SetActive(true);
        }

        slot.correctHighlight.gameObject.SetActive(isCorrect);
        slot.incorrectHighlight.gameObject.SetActive(!isCorrect);

        if (isCorrect)
        {
            currentComboStep++;
            if (currentComboStep < reloadSlots.Count)
            {
                reloadSlots[currentComboStep].arrowImage.gameObject.SetActive(true);
                StartCoroutine(WaitThenStartTimingWindow(1f));
            }
            else
            {
                FinalizeReload();
            }
        }
        else
        {
            StartCoroutine(FinishFailedReload());
        }
    }

    /// <summary>
    /// Starts the timing window for the current reload step
    /// </summary>
    private void StartTimingWindow()
    {
        timingWindowStartTime = Time.time;
        isInTimingWindow = true;

        reloadPromptEffect?.Play();
    }

    /// <summary>
    /// Ends the current timing window
    /// </summary>
    private void EndTimingWindow()
    {
        isInTimingWindow = false;
        reloadPromptEffect?.Stop();
    }

    /// <summary>
    /// Cancels the reload process and resets UI elements
    /// </summary>
    public void CancelReload()
    {
        isReloading = false;
        isMarkerMoving = false;

        timingBar.gameObject.SetActive(false);
        reloadMarker.gameObject.SetActive(false);
        reloadPromptEffect?.Stop();
        reloadPromptEffect.gameObject.SetActive(false);

        foreach (var slot in reloadSlots)
        {
            slot.arrowImage.gameObject.SetActive(false);
            slot.correctHighlight.gameObject.SetActive(false);
            slot.incorrectHighlight.gameObject.SetActive(false);
            slot.timedHighlight.gameObject.SetActive(false);
        }

        reloadPromptContainer?.SetActive(false);
        markerTransform.anchoredPosition = new Vector2(markerStartX, markerTransform.anchoredPosition.y);
    }

    /// <summary>
    /// Coroutine to handle failed reload sequence
    /// </summary>
    private IEnumerator FinishFailedReload()
    {
        yield return new WaitForSeconds(0.5f);
        CancelReload();
    }

    /// <summary>
    /// Finalizes the reload process after successful completion
    /// </summary>
    public void FinalizeReload()
    {
        isReloading = false;
        isMarkerMoving = false;

        timingBar.gameObject.SetActive(false);
        reloadMarker.gameObject.SetActive(false);
        reloadPromptEffect?.Stop();
        reloadPromptEffect.gameObject.SetActive(false);

        currentAmmo = maxAmmo;
        reloadPromptContainer?.SetActive(false);
        reloadPromptEffect?.Stop();
        markerTransform.anchoredPosition = new Vector2(markerStartX, markerTransform.anchoredPosition.y);

        if (allStepsTimed)
            SendMessage("OnPerfectReload", SendMessageOptions.DontRequireReceiver);
    }

    /// <summary>
    /// Starts the reload marker movement for timing mini-game
    /// </summary>
    public void StartReloadMarker()
    {
        if (reloadSlots.Count < 2) return;

        isMarkerMoving = true;
        reloadTimer = 0;

        RectTransform first = reloadSlots[0].arrowImage.rectTransform;
        RectTransform last = reloadSlots[reloadSlots.Count - 1].arrowImage.rectTransform;

        markerStartX = first.anchoredPosition.x + 30;
        markerEndX = last.anchoredPosition.x - 30;

        markerTransform.anchoredPosition = new Vector2(markerStartX, markerTransform.anchoredPosition.y);
        reloadMarker.gameObject.SetActive(true);
    }

    /// <summary>
    /// Coroutine to wait before starting the next timing window
    /// </summary>
    private IEnumerator WaitThenStartTimingWindow(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartTimingWindow();
    }
}