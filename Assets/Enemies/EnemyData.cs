using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy", menuName = "Enemy/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Description")]
    
    
    public string EnemyName = "Enemy";
    public string GetEnemyName => EnemyName;
    
    public Sprite sprite;
    public Sprite GetEnemySprite => sprite;
    
    public string description = "Description";
    public string GetEnemyDescription => description;

    public string Behaviour = "DefaultAI";
    public string GetEnemyBehaviour => Behaviour;
    
    public string deathMessage = " has been defeated";

    public string GetEnemyDeadMSG => deathMessage;

    [Header("Stats")]
    public int HP = 1;
    public int GetEnemyMaxHP => HP;


    public enum AI{
        DefaultAI, Healer
    };

    public AI ThisEnemyAI = AI.DefaultAI;

    public AI GetEnemyAI => ThisEnemyAI;
    
    public int PP = 1;
    public int GetEnemyMaxPP => PP;

    public int Offense = 1;
    public int GetEnemyOffense => Offense;

    public int Defense = 1;

    public int GetEnemyDefense => Defense;
    
    public int Speed = 1;
    public int GetEnemySpeed => Speed;

    public int Guts = 1;

    public int GetEnemyGuts => Guts;

    
    public int HypnosisSuccess = 1;
    public int GetEnemyHypnosisSucc => HypnosisSuccess;
    
    public int ParalysisSuccess = 1;

    public int GetEnemyParalysisSucc => ParalysisSuccess;
    
    public int FlashSuccess = 1;

    public int GetEnemyFlashSucc => FlashSuccess;
    public int BrainShockSuccess = 1;

    public int GetEnemyBrainshockSucc => BrainShockSuccess;
    
    [Header("Moves & PSI")]
    public List<MovesData> moves;
    
    [Header("Drop")]
    public int EXP_Drop = 1;
    public int GetEnemyEXPDrop => EXP_Drop;

    public List<ItemDrops> itemDrop;

    [System.Serializable]
    public struct ItemDrops
    {
        public ItemData item;
        public ItemData percentageDrop;
    }

}