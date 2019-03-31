using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SelectPSI : MonoBehaviour {
    public GameObject categoryBox;
    private Vector3 categoryP1 = new Vector3(-80f, 76f, 0f);
    private Vector3 categoryP2 = new Vector3(-250f, 76f, 0f);
    private Vector3 catDest;
    public GameObject psiMenuBox;
    private Vector3 psiP1 = new Vector3(430f, 76f, 0f);
    private Vector3 psiP2 = new Vector3(87f, 76f, 0f);
    private Vector3 psiDest;
    public GameObject descriptionBox;
    private Vector3 descriptionP1 = new Vector3(25f, -100f, 0f);
    private Vector3 descriptionP2 = new Vector3(25f, -130f, 0f);
    private Vector3 descDest;


    public GameObject categorySelector;
    public GameObject psiSelector;
    private Vector3 categoryDestination;
    private Vector3 psiDestination;
    private int categoryIndex;
    private Vector3 catVelocity = Vector3.zero;
    private Vector3 psiVelocity = Vector3.zero;
    private Vector3 desVelocity = Vector3.zero;

    private Vector3 velocity;
    private Vector3 velocity2;
    public bool selectingCategory;
    public List<PsiData> moves;
    public Battle battle;
    public List<TMP_Text> slot;
    public List<TMP_Text> categorySlot;
    public TMP_Text description;

    private int categoryHorizontalIndex;
    private int categoryVerticalIndex;
    private bool inPosition;
    private bool selecting;
    // Use this for initialization
    void Start() {
        selecting = false;
        inPosition = false;

        categoryBox.transform.localPosition = categoryP2;
        psiMenuBox.transform.localPosition = psiP1;
        descriptionBox.transform.localPosition = descriptionP2;
        catDest = categoryBox.transform.localPosition;
        psiDest = psiMenuBox.transform.localPosition;
        descDest = descriptionBox.transform.localPosition;

        psiSelector.SetActive(false);
        selectingCategory = true;
        categoryVerticalIndex = 0;
        categoryHorizontalIndex = 0;
        description.text = "Offensive psi moves focused on damage";
        psiSelector.transform.localPosition = new Vector3(-16f, 65f, transform.localPosition.z);

        categoryIndex = 0;
        moves = battle.GetMovesByCategory("Offense");
        UpdateText();
        categoryDestination = categorySelector.transform.localPosition;
        psiDestination = psiSelector.transform.localPosition;

    }
    public string getGreek()
    {
        string ret = "";
        switch(categoryHorizontalIndex)
        {
            case 0:
                ret = " [";
                break;
            case 1:
                ret = " ]";
                break;
            case 2:
                ret = " _";
                break;
        }
        return ret;
    }
    public string getPSIName()
    {
        return moves[categoryVerticalIndex].moveName + getGreek();
    }
    public int getPSIDamage()
    {
        return moves[categoryVerticalIndex].moveDamage[categoryHorizontalIndex];
    }
    public int getPSICost()
    {
        return moves[categoryVerticalIndex].cost[categoryHorizontalIndex];
    }
    public void Deactivate()
    {
        selectingCategory = true;
        categoryVerticalIndex = 0;
        categoryHorizontalIndex = 0;
        psiSelector.SetActive(false);

    }
    public void MoveMenu()
    {
        if (!selecting)
        {
            catDest = categoryP1;
            psiDest = psiP2;
            descDest = descriptionP1;
            selecting = true;
        }
        else
        {
            catDest = categoryP2;
            psiDest = psiP1;
            descDest = descriptionP2;
            selecting = false;
        }

    }
    public bool getInPosition()
    {
        return inPosition;
    }
    public bool getChoosingCat()
    {
        return selectingCategory;
    }
    void ClearSlotText()
    {
        for (int i = 0; i < slot.Count; i++)
        {
            slot[i].text = "";
            categorySlot[i].text = "";
        }
    }
    void UpdateText()
    {
        ClearSlotText();
        for (int i = 0; i < moves.Count; i++)
        {
            switch (moves[i].upgrade)
            {
                case 0:
                    categorySlot[i].text = "[";
                    break;
                case 1:
                    categorySlot[i].text = "[           ]";
                    break;

                case 2:
                    categorySlot[i].text = "[           ]           _";
                    break;
            }
            slot[i].text = moves[i].moveName;
        }
    }
    // Update is called once per frame
    void Update() {
        if(categoryBox.transform.localPosition.x >= categoryP1.x - 10f)
        {
            inPosition = true;
        }
        else
        {
            inPosition = false;
        }
        if (categoryBox.transform.localPosition!=catDest)
        {
            categoryBox.transform.localPosition = Vector3.SmoothDamp(categoryBox.transform.localPosition, catDest, ref catVelocity, 0.1f);
        }

        if (psiMenuBox.transform.localPosition != psiDest)
        {
            psiMenuBox.transform.localPosition = Vector3.SmoothDamp(psiMenuBox.transform.localPosition, psiDest, ref psiVelocity, 0.1f);
        }
        if (descriptionBox.transform.localPosition != descDest)
        {
            descriptionBox.transform.localPosition = Vector3.SmoothDamp(descriptionBox.transform.localPosition, descDest, ref desVelocity, 0.1f);
        }

        if (inPosition)
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                if (selectingCategory)
                {
                    if (categoryIndex < 2)
                    {
                        categoryIndex += 1;
                        switch (categoryIndex)
                        {
                            case 0:
                                moves = battle.GetMovesByCategory("Offense");
                                description.text = "Offensive psi moves focused on damage";
                                break;

                            case 1:
                                moves = battle.GetMovesByCategory("Recovery");
                                description.text = "Recovery psi moves focused on recovery";

                                break;


                            case 2:
                                moves = battle.GetMovesByCategory("Assist");
                                description.text = "Assist psi moves focused on assist";

                                break;
                        }
                        UpdateText();
                        categoryDestination = new Vector3(categorySelector.transform.localPosition.x, 65f - (60f * categoryIndex), categorySelector.transform.localPosition.z);
                    }
                }
                else
                {
                    if (categoryVerticalIndex < moves.Count - 1)
                    {
                        categoryVerticalIndex++;
                        categoryHorizontalIndex = 0;
                        description.text = moves[categoryVerticalIndex].description[categoryHorizontalIndex];
                        psiDestination = new Vector3(-16f, 65f - (65f * categoryVerticalIndex), psiSelector.transform.localPosition.z);
                    }

                }
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                if (selectingCategory)
                {
                    if (categoryIndex > 0)
                    {
                        categoryIndex -= 1;
                        switch (categoryIndex)
                        {
                            case 0:
                                moves = battle.GetMovesByCategory("Offense");
                                description.text = "Offensive psi moves focused on damage";
                                break;

                            case 1:
                                moves = battle.GetMovesByCategory("Recovery");
                                description.text = "Recovery psi moves focused on recovery";

                                break;


                            case 2:
                                moves = battle.GetMovesByCategory("Assist");
                                description.text = "Assist psi moves focused on assist";

                                break;
                        }
                        UpdateText();
                        categoryDestination = new Vector3(categorySelector.transform.localPosition.x, 65f - (60f * categoryIndex), categorySelector.transform.localPosition.z);

                    }
                }
                else
                {
                    if (categoryVerticalIndex > 0)
                    {
                        categoryVerticalIndex--;
                        categoryHorizontalIndex = 0;
                        description.text = moves[categoryVerticalIndex].description[categoryHorizontalIndex];
                        psiDestination = new Vector3(-16f, 65f - (65f * categoryVerticalIndex), psiSelector.transform.localPosition.z);
                    }

                }
            }
            if (Input.GetKeyDown(KeyCode.D) && !selectingCategory)
            {
                if (categoryHorizontalIndex < moves[categoryVerticalIndex].upgrade)
                {
                    categoryHorizontalIndex++;
                    description.text = moves[categoryVerticalIndex].description[categoryHorizontalIndex];
                    psiDestination = new Vector3(-16f + (155f * categoryHorizontalIndex), psiDestination.y, psiSelector.transform.localPosition.z);

                }
            }
            if (Input.GetKeyDown(KeyCode.A) && !selectingCategory)
            {
                if (categoryHorizontalIndex > 0)
                {
                    categoryHorizontalIndex--;
                    description.text = moves[categoryVerticalIndex].description[categoryHorizontalIndex];
                    psiDestination = new Vector3(-16f + (155f * categoryHorizontalIndex), psiDestination.y, psiSelector.transform.localPosition.z);

                }
            }
            if (Input.GetKeyDown(KeyCode.Space) && moves.Count != 0)
            {
                if (selectingCategory)
                {
                    description.text = moves[categoryVerticalIndex].description[categoryHorizontalIndex];
                    selectingCategory = false;
                    psiSelector.transform.localPosition = new Vector3(-16f, 65f, transform.localPosition.z);
                    psiDestination = psiSelector.transform.localPosition;
                    psiSelector.SetActive(true);
                }
            }

            categorySelector.transform.localPosition = Vector3.SmoothDamp(categorySelector.transform.localPosition, categoryDestination, ref velocity, 0.1f);
            if (!selectingCategory)
                psiSelector.transform.localPosition = Vector3.SmoothDamp(psiSelector.transform.localPosition, psiDestination, ref velocity2, 0.1f);

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (!selectingCategory)
                {
                    psiSelector.SetActive(false);
                    selectingCategory = true;
                    description.text = "Offensive psi moves focused on damage";
                    categoryHorizontalIndex = 0;
                    categoryVerticalIndex = 0;
                    psiSelector.transform.localPosition = new Vector3(-16f, 65f, transform.localPosition.z);
                    psiDestination = psiSelector.transform.localPosition;
                }

            }
        }
    }
}
