using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{

    public List<ItemData> items;

    public List<ItemData> getItems()
    {
        return items;
    }

    public int getItemsCount()
    {
        return items.Count;
    }

}
