using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Item")]
public class itemsData : ScriptableObject
{
    public string itemName;
    public string itemDescription;
    public int itemHealthGain;


    public string GetName()
    {
        return itemName;
    }
    public string GetDescription()
    {
        return itemDescription;
    }
    public int GetHPGain()
    {

        return itemHealthGain;
    }

}
