using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Controls boss enemy behavior including ability selection and usage
/// </summary>
public class BossEnemy : MonoBehaviour
{
    public Ability[] abilities;       // Array of available abilities
    public GameObject rockPrefab;     // Reference to rock projectile prefab

    /// <summary>
    /// Selects a random ability and returns its lifetime
    /// </summary>
    /// <returns>Lifetime duration of the selected ability</returns>
    public float UseAbility()
    {
        int abilityIndex = 0;
        if (abilities != null && abilities.Length > 0)
        {
            abilityIndex = Random.Range(0, abilities.Length);
        }
        return abilities[abilityIndex].lifetime;
    }
}