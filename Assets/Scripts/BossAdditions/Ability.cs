using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//<summary>
//Stores values for boss ability
//<summary>

[CreateAssetMenu(fileName = "New Ability", menuName = "Ability/Ability")]
public class Ability : ScriptableObject
{

    public int damage;
    public new string name;
    public float lifetime;
    public bool knockback;
}