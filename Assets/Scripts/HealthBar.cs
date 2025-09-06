using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles health bar UI display and updates based on player health changes
/// </summary>
public class HealthBar : MonoBehaviour
{
    public Slider armorSlider;
    public PlayerMotor player;

    /// <summary>
    /// Initializes health bar values and subscribes to health change events
    /// </summary>
    void Start()
    {
        if (player == null)
        {
            player = FindObjectOfType<PlayerMotor>();
        }

        armorSlider.maxValue = player.maxHealth;
        armorSlider.value = player.health;

        player.OnHealthChanged += UpdateArmorBar;
    }

    /// <summary>
    /// Unsubscribes from health change events when the object is destroyed
    /// </summary>
    private void OnDestroy()
    {
        if (player != null)
            player.OnHealthChanged -= UpdateArmorBar;
    }

    /// <summary>
    /// Updates the health bar display with current health values
    /// </summary>
    /// <param name="currentHealth">The player's current health value</param>
    /// <param name="maxHealth">The player's maximum health value</param>
    private void UpdateArmorBar(float currentHealth, float maxHealth)
    {
        armorSlider.value = currentHealth;
    }
}