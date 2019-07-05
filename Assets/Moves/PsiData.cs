using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New PSI Attack", menuName = "PSI Move")]
public class PsiData : ScriptableObject {

    public string moveName;
    public string GetPSIMoveName => moveName;
    
    
    public DamageRange moveDamage;

    public int GetPSIMoveDamage()
    {
        return Random.Range(moveDamage.DamageMin, moveDamage.DamageMax);
    }

    [System.Serializable]
    public struct DamageRange
    {
        public int DamageMin;
        public int DamageMax;
    }
    public int cost = 1;

    public int GetPSIMoveCost => cost;
    
    public string description = "Description";

    public string GetPSIMoveDescription => description;
    [StringInList("Offense","Recovery","Assist")] public string type = "Offense";

    public string GetPSIType => type;
    [StringInList("Damage","Status","Heal","Revive")] public string effect = "Damage";

    public string GetPSIEffect => effect;
    [StringInList("None","Sleep","Paralysis")] public string statusEffect = "None";

    public string GetPSIStatusEffect => statusEffect;
    public enum Direction
    {
        Self,
        Target,
        AllyTarget,
        Allies,
        Opponents,
        All
    }
    
    
    public Direction MoveTarget = Direction.Target;

    public Direction GetMoveTarget => MoveTarget;
    
    public GameObject psiAnimation;

    public GameObject GetPSIAnimation => psiAnimation;
    
    public string ApplyEffect(BattleEntity target,int value = -1)
    {
        string message = "";
        if(value == -1)
            value = GetPSIMoveDamage();
        switch (effect)
        {
            case "Damage":
                message = "@ " + target.GetName + " takes " +
                          value + " of damage.";
                target.ReceiveDamage(-value);
                break;
            case "Status":
                message = "@ " + target.GetName + " status is changed.";
                target.SetStatus(statusEffect);
                break;
            case "Heal":
                message = "@ " + target.GetName + " recovers " + value + " of health.";
                target.Heal(value);
                break;
            case "Revive":
                message = "@ " + target.GetName + " comes back to life. ";
                target.Revive(value);
                break;
            
            default:
                message = "@ " + target.GetName + " takes " +
                          -value + " of damage.";
                target.ReceiveDamage(-value);
                break;
        }

        return message;
    }
    
}
