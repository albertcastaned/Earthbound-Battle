using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class SelectPSI : MonoBehaviour {

    private Player player;
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
    private Vector3 contVelocity = Vector3.zero;

    private Vector3 velocity;
    private Vector3 velocity2;
    public bool selectingCategory;
    public List<PsiData> moves;
    public Battle battle;
    private List<GameObject> slots;
    private List<GameObject> costSlots;

    public TMP_Text description;
    
    private int MoveVerticalIndex;
    private bool inPosition;
    private bool selecting;

    public int MaximumSlotsPerPage = 3;
    public GameObject movesUIContainer;
    public GameObject slotPrefab;
    public GameObject slotCostPrefab;

    private Vector3 contentDest;


    [SerializeField]
    private float timeBetweenSteps = 0.5f;
    private float lastStep = 0.5f;

    void Awake()
    {
        slots = new List<GameObject>();
        costSlots = new List<GameObject>();

        selecting = false;
        inPosition = false;
        
        categoryBox.transform.localPosition = categoryP2;
        psiMenuBox.transform.localPosition = psiP1;
        descriptionBox.transform.localPosition = descriptionP2;
        catDest = categoryBox.transform.localPosition;
        psiDest = psiMenuBox.transform.localPosition;
        descDest = descriptionBox.transform.localPosition;
        contentDest = movesUIContainer.transform.localPosition;
        
        psiSelector.SetActive(false);
        selectingCategory = true;
        MoveVerticalIndex = 0;
        description.text = "Offensive psi moves focused on damage";

        categoryIndex = 0;
        moves = battle.GetMovesByCategory("Offense");
        player = battle.CurrentPlayerSelecting();

        UpdateText();


        categoryDestination = categorySelector.transform.localPosition;
    }


    public PsiData GetPSIMove => moves[MoveVerticalIndex];
    

    public void Deactivate()
    {
        selectingCategory = true;
        MoveVerticalIndex = 0;
        psiSelector.SetActive(false);

    }

    private void UpdateMoves()
    {
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
        moves = moves.OrderBy(move=>move.moveName).ToList();
    }
    public void MoveMenu()
    {
        player = battle.CurrentPlayerSelecting();
        
        if (!selecting)
        {
            MoveVerticalIndex = 0;
            UpdateMoves();
            UpdateText();
            
            catDest = categoryP1;
            psiDest = psiP2;
            descDest = descriptionP1;
            selecting = true;
        }
        else
        {
            UpdateText();

            catDest = categoryP2;
            psiDest = psiP1;
            descDest = descriptionP2;

            selecting = false;
        }
        if (!selectingCategory)
        {
            slots[MoveVerticalIndex].GetComponent<SlotMove>().MoveSelect(true);
            costSlots[MoveVerticalIndex].GetComponent<SlotMove>().MoveSelect(true);

            StartCoroutine(nameof(MoveCursor));
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
    void ClearSlots()
    {
        slots.Clear();
        costSlots.Clear();
        contentDest = new Vector3(contentDest.x, 0f,contentDest.z);

        foreach (Transform child in movesUIContainer.transform) {
            if(child.name != "PsiNameSelector")
                Destroy(child.gameObject);
        }
    }


    void ExpandContainer()
    {
        movesUIContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(12.8f, 200f + 70f * (moves.Count - MaximumSlotsPerPage));
}
    void UpdateText()
    {
        ClearSlots();
        ExpandContainer();

        for (int i = 0; i < moves.Count; i++)
        {
            GameObject newSlot = Instantiate(slotPrefab, movesUIContainer.transform);
            newSlot.transform.localPosition = new Vector3(160f,-70f * i, movesUIContainer.transform.position.z);
            slots.Add(newSlot);
            
            GameObject costSlot = Instantiate(slotCostPrefab, movesUIContainer.transform);
            costSlot.transform.localPosition = new Vector3(800f,-70f * i, movesUIContainer.transform.position.z);
            costSlots.Add(costSlot);
            if (player.getPPMeter.Value >= moves[i].GetPSIMoveCost)
            {
                newSlot.GetComponent<TextMeshProUGUI>().text = moves[i].GetPSIMoveName;
                costSlot.GetComponent<TextMeshProUGUI>().text = moves[i].GetPSIMoveCost.ToString();

            }
            else
            {
                newSlot.GetComponent<TextMeshProUGUI>().text =
                    "<color=#808080ff>" + moves[i].GetPSIMoveName + "</color>";
                costSlot.GetComponent<TextMeshProUGUI>().text =
                    "<color=#808080ff>" + moves[i].GetPSIMoveCost.ToString() + "</color>";
                
            }


        }
    }
    public bool CanAfford()
    {
        return player.getPPMeter.Value >= moves[MoveVerticalIndex].GetPSIMoveCost;

    }
    


    // Update is called once per frame
    void Update()
    {
        
        if (categoryBox.transform.localPosition.x >= categoryP1.x - 10f)
        {
            inPosition = true;

        }
        else
        {
            inPosition = false;
        }

        if (categoryBox.transform.localPosition != catDest)
        {
            categoryBox.transform.localPosition =
                Vector3.SmoothDamp(categoryBox.transform.localPosition, catDest, ref catVelocity, 0.1f);
        }

        if (psiMenuBox.transform.localPosition != psiDest)
        {
            psiMenuBox.transform.localPosition =
                Vector3.SmoothDamp(psiMenuBox.transform.localPosition, psiDest, ref psiVelocity, 0.1f);
        }

        if (descriptionBox.transform.localPosition != descDest)
        {
            descriptionBox.transform.localPosition = Vector3.SmoothDamp(descriptionBox.transform.localPosition,
                descDest, ref desVelocity, 0.1f);
        }


        categorySelector.transform.localPosition = Vector3.SmoothDamp(categorySelector.transform.localPosition,
            categoryDestination, ref velocity, 0.2f);


        if (inPosition)
        {
            if (Input.GetKey(KeyCode.S))
            {
                if (Time.time - lastStep > timeBetweenSteps)
                {
                    AudioManager.instance.Play("RightMenu");

                    lastStep = Time.time;
                    {
                        if (selectingCategory)
                        {
                            MoveVerticalIndex = 0;
                            if (categoryIndex < 2)
                            {
                                categoryIndex += 1;
                                UpdateMoves();


                                var localPosition = categorySelector.transform.localPosition;
                                categoryDestination = new Vector3(localPosition.x, 65f - (60f * categoryIndex),
                                    localPosition.z);
                            }

                            UpdateText();
                            StartCoroutine(nameof(MoveCursor));


                        }
                        else
                        {
                            if (MoveVerticalIndex < moves.Count - 1)
                            {
                                slots[MoveVerticalIndex].GetComponent<SlotMove>().MoveSelect(false);
                                costSlots[MoveVerticalIndex].GetComponent<SlotMove>().MoveSelect(false);

                                MoveVerticalIndex++;
                                slots[MoveVerticalIndex].GetComponent<SlotMove>().MoveSelect(true);
                                costSlots[MoveVerticalIndex].GetComponent<SlotMove>().MoveSelect(true);

                                description.text = moves[MoveVerticalIndex].GetPSIMoveDescription;

                                StartCoroutine(nameof(MoveCursor));




                            }

                        }
                    }
                }
            }

            if (Input.GetKey(KeyCode.W))
            {
                if (Time.time - lastStep > timeBetweenSteps)
                {
                    AudioManager.instance.Play("LeftMenu");

                    lastStep = Time.time;
                    {
                        if (selectingCategory)
                        {
                            MoveVerticalIndex = 0;

                            if (categoryIndex > 0)
                            {
                                categoryIndex -= 1;
                                UpdateMoves();


                                var localPosition = categorySelector.transform.localPosition;
                                categoryDestination = new Vector3(localPosition.x, 65f - (60f * categoryIndex),
                                    localPosition.z);

                            }

                            UpdateText();
                            StartCoroutine(nameof(MoveCursor));


                        }
                        else
                        {
                            if (MoveVerticalIndex > 0)
                            {
                                slots[MoveVerticalIndex].GetComponent<SlotMove>().MoveSelect(false);
                                costSlots[MoveVerticalIndex].GetComponent<SlotMove>().MoveSelect(false);

                                MoveVerticalIndex--;
                                slots[MoveVerticalIndex].GetComponent<SlotMove>().MoveSelect(true);
                                costSlots[MoveVerticalIndex].GetComponent<SlotMove>().MoveSelect(true);

                                description.text = moves[MoveVerticalIndex].GetPSIMoveDescription;

                                StartCoroutine(nameof(MoveCursor));


                            }

                        }
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.Space) && moves.Count != 0)
            {
                AudioManager.instance.Play("Click");

                if (selectingCategory)
                {
                    description.text = moves[MoveVerticalIndex].GetPSIMoveDescription;
                    selectingCategory = false;

                    psiSelector.SetActive(true);
                    slots[MoveVerticalIndex].GetComponent<SlotMove>().MoveSelect(true);
                    costSlots[MoveVerticalIndex].GetComponent<SlotMove>().MoveSelect(true);

                    StartCoroutine(nameof(MoveCursor));


                }

            }



            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (!selectingCategory)
                {
                    slots[MoveVerticalIndex].GetComponent<SlotMove>().MoveSelect(false);
                    costSlots[MoveVerticalIndex].GetComponent<SlotMove>().MoveSelect(false);

                    psiSelector.SetActive(false);
                    selectingCategory = true;

                    description.text = "Offensive psi moves focused on damage";

                }
                else
                {
                    StartCoroutine(nameof(MoveCursor));

                }

            }

        }
    }



    void MoveContainer()
    {
        if(MoveVerticalIndex == 0 || MoveVerticalIndex == slots.Count - 1)
            return;
        contentDest = new Vector3(contentDest.x, 0f + 70f * (MoveVerticalIndex - 1),contentDest.z);
    }
    private IEnumerator MoveCursor()
    {
        MoveContainer();

        while (!V3Equal(psiSelector.transform.position, new Vector3(slots[MoveVerticalIndex].transform.position.x - 40f,
                   slots[MoveVerticalIndex].transform.position.y - 10f, 30f)) || !V3Equal(movesUIContainer.transform.localPosition, contentDest))
        {
            movesUIContainer.transform.localPosition = Vector3.SmoothDamp(movesUIContainer.transform.localPosition,contentDest, ref contVelocity, 0.05f);
            
            psiSelector.transform.position = Vector3.SmoothDamp(psiSelector.transform.position,
                new Vector3(slots[MoveVerticalIndex].transform.position.x - 40f,
                    slots[MoveVerticalIndex].transform.position.y - 10f, 30f), ref velocity2, 0.05f);
            yield return null;
        }


        
    }
    
    private bool V3Equal(Vector3 a, Vector3 b){
        return Vector3.SqrMagnitude(a - b) < 0.005;
    }
    
}
