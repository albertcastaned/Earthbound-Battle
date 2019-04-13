using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{

    public List<itemsData> items;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public List<itemsData> getItems()
    {
        return items;
    }

    public int getItemsCount()
    {
        return items.Count;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
