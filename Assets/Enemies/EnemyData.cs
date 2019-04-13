using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[CreateAssetMenu(fileName = "New Enemy", menuName = "Enemy")]
public class EnemyData : ScriptableObject
{

    public string enemyName;
    public int maxHealth;
    public string description;
    public Sprite sprite;
    public List<MovesData> moves;




}