using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ability", menuName = "Ability/Ability")]
public class Ability : ScriptableObject
{

    public int damage;
    public new string name;
    public float lifetime;
    public bool knockback;
}