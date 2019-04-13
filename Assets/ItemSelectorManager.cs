using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class ItemSelectorManager : MonoBehaviour {
    private Vector3 selectingPos = new Vector3(0f,50f, 0f);
    private Vector3 unselectedPos = new Vector3(0f,190f, 0f);
    private Vector3 dest;
    private Vector3 descriptionP1 = new Vector3(0f, -101f, 0f);
    private Vector3 descriptionP2 = new Vector3(0f,  -130f, 0f);
    private Vector3 descDest;
    public GameObject descriptionBox;
    public TMP_Text descriptionText;
    public GameObject itemBox;
    private bool selecting = false;
    private Vector3 velocity = Vector3.zero;
    private Vector3 descVelocity = Vector3.zero;
    private Vector3 selectorVelocity = Vector3.zero;

    public Inventory inventory;
    private List<itemsData> items;
    public List<TMP_Text> slot;

    public GameObject selector;
    private Vector3 selectorDestination = new Vector3(-440f, 150f, 0f);
    private int itemIndex = 0;

    private int verticalIndex = 0;
    public int horizontalIndex = 0;

    private int verticalMax;
    public int horizontalMax;

    
    // Use this for initialization
    void Start () {
        items = inventory.getItems();
        UpdateItemList();
        dest = unselectedPos;
        itemBox.transform.localPosition = unselectedPos;
        descriptionBox.transform.localPosition = descriptionP2;
        descDest = descriptionP2;

        horizontalMax = (items.Count == 1) ? 0:1;
        verticalMax = (items.Count-1) / 2;
	}

    public void UpdateItemList()
    {
        items = inventory.getItems();
        for (int i =0;i < items.Count;i++)
        {
            slot[i].text = items[i].itemName;
        }
        UpdateItemDescription();
    }

    void ClearItemNames()
    {
        for(int i=0;i<14;i++)
        {
            slot[i].text = "";
        }
    }
    public void UpdateItemDescription()
    {
        descriptionText.text = items[itemIndex].itemDescription;
    }

    public void ResetData()
    {
        horizontalIndex = 0;
        verticalIndex = 0;
        horizontalMax = (items.Count == 1) ? 0 : 1;
        verticalMax = (items.Count - 1) / 2;
        itemIndex = 0;
        selectorDestination = new Vector3(-440f, 150f, 0f);
        selector.transform.localPosition = selectorDestination;
        ClearItemNames();
        UpdateItemList();
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
    }
    public void Activate()
    {
        gameObject.SetActive(true);
    }
    public void MoveMenu()
    {
        if (!selecting)
        {
            ResetData();
            dest = selectingPos;
            descDest = descriptionP1;
            selecting = true;
        }
        else
        {
            dest = unselectedPos;
            descDest = descriptionP2;
            selecting = false;
        }
    }

    public int getItemIndex()
    {
        return itemIndex;
    }
    // Update is called once per frame
    void Update () {

        if (itemBox.transform.localPosition != dest)
        {
            itemBox.transform.localPosition = Vector3.SmoothDamp(itemBox.transform.localPosition, dest, ref velocity, 0.1f);
        }
        if (descriptionBox.transform.localPosition != descDest)
        {
            descriptionBox.transform.localPosition = Vector3.SmoothDamp(descriptionBox.transform.localPosition, descDest, ref descVelocity, 0.1f);
        }

        if (Input.GetKeyDown(KeyCode.S) && verticalIndex < verticalMax)
        {
            verticalIndex++;
            if (verticalIndex == verticalMax)
            {
                if (items.Count % 2 == 0)
                {
                    horizontalMax = 1;
                }
                else
                {
                    horizontalMax = 0;
                    if (horizontalIndex % 2 != 0)
                    {
                        selectorDestination.x -= 500f;
                        itemIndex -= 1;
                        horizontalIndex -= 1;
                    }
                }
            }
            else
            horizontalMax = 1;
            selectorDestination.y -= 50f;
            itemIndex += 2;
            UpdateItemDescription();

        }
        if (Input.GetKeyDown(KeyCode.W) && verticalIndex > 0)
        {
            verticalIndex--;
            if (verticalIndex == verticalMax)
                horizontalMax = (items.Count % 2 == 0) ? 1 : 0;
            else
                horizontalMax = 1;
            selectorDestination.y += 50f;
            itemIndex -= 2;
            UpdateItemDescription();

        }
        if (Input.GetKeyDown(KeyCode.D) && horizontalIndex < horizontalMax)
        {
            horizontalIndex++;
            selectorDestination.x += 500f;
            itemIndex += 1;
            UpdateItemDescription();

        }
        if (Input.GetKeyDown(KeyCode.A) && horizontalIndex > 0)
        {
            horizontalIndex--;
            selectorDestination.x -= 500f;
            itemIndex -= 1;
            UpdateItemDescription();
        }
        selector.transform.localPosition = Vector3.SmoothDamp(selector.transform.localPosition, selectorDestination, ref selectorVelocity, 0.05f);

    }
}
