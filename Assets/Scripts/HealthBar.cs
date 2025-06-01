using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider armorSlider;
    public PlayerMotor player;

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

    private void OnDestroy()
    {
        if (player != null)
            player.OnHealthChanged -= UpdateArmorBar;
    }

    private void UpdateArmorBar(float currentHealth, float maxHealth)
    {
        armorSlider.value = currentHealth;
    }
}