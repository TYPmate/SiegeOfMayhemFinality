using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BossEnemy : MonoBehaviour
{
    public Ability[] abilities;
    public GameObject rockPrefab;
    public float UseAbility()
    {
        int abilityIndex = 0;
        if (abilities != null)
        {
            abilityIndex = Random.Range(0, abilities.Length);
        }
        return abilities[abilityIndex].lifetime;
    }

}
