using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Item")]
public class ItemData : ScriptableObject
{
    public int ID;
    public string itemName;
    public string itemDescription;
    public int itemHealthGain;

    [StringInList("Heal", "Revive","PP Restore")]
    public string effect = "Heal";
    
    

    public string GetName()
    {
        return itemName;
    }

    public string GetEffect() => effect;

    public string GetDescription()
    {
        return itemDescription;
    }
    public int GetHPGain()
    {

        return itemHealthGain;
    }
    public string ApplyEffect(BattleEntity target)
    {
        string message = "";
        switch (effect)
        {
            case "Heal":
                target.Heal(itemHealthGain);
                message = "@ " + target.GetName + " recovered " + itemHealthGain + " of HP";
                break;
            case "Revive":
                target.Revive(itemHealthGain);
                message = "@ " + target.GetName + " came back to life.";

                break;
            case "PP Restore":
                target.ChangePP(itemHealthGain);
                message = "@ " + target.GetName + " recovered " + itemHealthGain + " of PP";
                break;
        }

        return message;
    }
    

}
