using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New Enemy Attack", menuName = "Enemy Attack")]
public class MovesData : ScriptableObject
{

    public string moveName;
    public string moveMessage;
    public int moveDamage;
    public float probability;

}