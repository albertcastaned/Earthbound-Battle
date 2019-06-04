using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[CreateAssetMenu(fileName = "New Enemy", menuName = "Enemy")]
public class EnemyData : ScriptableObject
{
    public string enemyName;

    public int HP;

    public int PP;

    public int Offense;

    public int Defense;

    public int Speed;

    public int Guts;

    public int EXP;

    public int HypnosisSuccess;

    public int ParalysisSuccess;

    public int FlashSuccess;

    public int BrainShockSuccess;

    public string description;

    public string deathMessage;

    public Sprite sprite;
    
    public List<MovesData> moves;


}