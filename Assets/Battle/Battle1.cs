﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;


//Battle controller
public class Battle1 : MonoBehaviour
{
    //Possible battle states
    protected enum STATE{
        PlayerTurn,
        EnemyTurn,
        Won,
        Lose,
        Flee
    };

    //Possible player states
    protected enum PlayerSTATE
    {
        Idle,
        ChoosingPSIMove,
        SelectingEnemyBash,
        SelectingEnemyPSI,
        ChoosingItem,
        ChoosingPartyMemberItem,
        ChoosingPartyMemberPSI,
        Attacking

    };

    protected enum MoveType
    {
        Bash,
        SelfHeal,
        PsiAttack,
        PsiRowAttack,
        RowHeal,
        StatusAttack
    }

    

    private STATE currentState;
    private PlayerSTATE currentPlayerState;
    private MoveType currentMove;

    public int NessHP;
    public List<PsiData> psiMoves;

    public CameraShake cam;
    public Text text;
    private bool isTyping;
    private bool isAttacking;
    private bool turnOver;
    private bool once;
    private int EnemyIndex;
    private int enemySelect;
    public List<Enemy> currentEnemies;
    public Inventory inventory;
    public MeterRoll hpBar;
    public MeterRoll ppBar;
    public GameObject border;
    private MovesData aux;
    public float borderDisplacement;
    public Vector3 menuDisplacement;
    public GameObject dialogue;
    public GameObject menuSpinner;
    private Rotate rotator;
    public GameObject NessMenu;
    private Vector3 velocity = Vector3.zero;
    private Vector3 enemyShiftVelocity = Vector3.zero;
    public GameObject moveEnemySelector;
    private MoveToEnemy moveEnemyScript;
    private MoveDialogue moveDialogue;
    public SelectPSI psiSelector;
    public ItemSelectorManager itemSelector;
    public GameObject psiObject;

    public GameObject dialoguePos;
    public GameObject itemObject;
    private bool halt = false;
    private bool movingEnemies = false;
    public GameObject partyMemberSelector;
    private int damage = 0;
    private bool usingPSI = false;
    public GameObject currentText;
    private Vector3 textVelocity = Vector3.zero;

    private int firstText = 0;


    private bool writing = false;

    private int lineIndex = 0;

    private List<string> lines = new List<string>();

    private Vector3 textDest;

    private GameObject psiAnimation;

    private bool enemyDie = false;

    private bool attacksAllRow = false;
    private bool psiSelf = false;

    public int enemyAuxIndex = 0;

    public GameObject healPrefab;

    private bool itemHeal = false;
    private int hpGain = 0;
    public OscilateOpacity playerOverlay;
    //Crear lista y ordenarlas dependiendo de la posicion x
    void Awake()
    {
        currentEnemies = new List<Enemy>();
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            currentEnemies.Add(enemy.GetComponent<Enemy>());
        }
        currentEnemies = new List<Enemy>(currentEnemies.OrderBy((c) => c.transform.position.x));
    }
    //Inciar
    void Start()
    {
        NessHP = 30;
        //Menu rotador
        rotator = menuSpinner.GetComponent<Rotate>();
        enemySelect = 0;
        text.text = "";
        isTyping = false;
        isAttacking = false;
        turnOver = false;
        moveEnemyScript = moveEnemySelector.GetComponent<MoveToEnemy>();
        moveDialogue = dialogue.GetComponent<MoveDialogue>();
        currentState = STATE.PlayerTurn;
        currentPlayerState = PlayerSTATE.Idle;
        once = false;
        EnemyIndex = 0;
        //Menu de cuadrov vida posicion y
        borderDisplacement = border.transform.position.y + 15.0f;
        //Menu de opciones de ataque posicion y
        menuDisplacement = new Vector3(NessMenu.transform.localPosition.x, NessMenu.transform.localPosition.y + 335f, NessMenu.transform.localPosition.z);
        moveEnemySelector.SetActive(false);
        partyMemberSelector.SetActive(false);
        psiObject.SetActive(false);
        NessMenu.SetActive(true);
        itemObject.SetActive(false);

    }
    public List<PsiData> GetMovesByCategory(string category)
    {
        List<PsiData> temp = new List<PsiData>();
        for(int i=0;i<psiMoves.Count;i++)
        {
            if (psiMoves[i].type == category)
            {
                temp.Add(psiMoves[i]);
            }
        }
        return temp;
    }

    private IEnumerator MoveText()
    {
        
        Vector3 target = new Vector3(currentText.transform.localPosition.x, currentText.transform.localPosition.y + 81f, currentText.transform.localPosition.z);

        while (!V3Equal(currentText.transform.localPosition,target))
        {
            if (firstText <= 1)
                break;
            currentText.transform.localPosition = Vector3.SmoothDamp(currentText.transform.localPosition, target, ref textVelocity, 0.1f);
            yield return null;
        }

        if (firstText <= 1)
        {
            firstText++;
        }
        else
        {
            currentText.transform.localPosition = target;

        }
    }
    //Escribir dialogo letra por letra
    private IEnumerator TextScroll(string lineOfText)
    {
        int letter = 0;

        int lineLength = lineOfText.Length - 1;
        if(firstText>1)
        {
            StartCoroutine("MoveText");
        }

        while (isTyping && (letter <= lineLength))
        {
            if (lineOfText[letter] == '¬')
            {
                text.text += "\n";
                letter++;
            }
            else
            {
                text.text += lineOfText[letter];
                letter++;
            }

            yield return new WaitForSeconds(0.030f);
        }
        if(lineIndex == 0 && usingPSI)
        {
            Instantiate(psiAnimation);

            if(!psiSelf)
                halt = true;
            else
                hpBar.CalculateDistance(damage);

        }
        if(lineIndex == 0 && itemHeal)
        {

            Instantiate(healPrefab);
            NessHP += hpGain;
            hpBar.CalculateDistance(hpGain);
        }
        lineIndex++;
        text.text += "\n";
        isTyping = false;
        if(firstText<2)
            firstText++;


    }
    public bool V3Equal(Vector3 a, Vector3 b)
    {
        return Vector3.SqrMagnitude(a - b) < 0.05;
    }
    //Mover posicion de enemigos
    private IEnumerator ShiftEnemiesPosition()
    {

        Vector3 destination = new Vector3(0, 17, 60);
        movingEnemies = true;
        switch (currentEnemies.Count)
        {
            case 1:
                while (currentEnemies.Count > 0 && !V3Equal(currentEnemies[0].getTransform(), destination))
                {

                currentEnemies[0].transform.position = Vector3.SmoothDamp(currentEnemies[0].getTransform(), destination, ref enemyShiftVelocity, 0.1f);
                 yield return null;

                }
                break;

        }
        currentEnemies[0].transform.position = destination;
        currentEnemies[0].SetDestination(destination);
        movingEnemies = false;
        
        

    }

    //Quitar enemigo de la lista
    public void RemoveEnemy(Enemy en)
    {
        currentEnemies.Remove(en);

        if (currentEnemies.Count != 0)
        {
            StartCoroutine("ShiftEnemiesPosition");
        }
        else
        {
            halt = false;
        }
    }

    public bool GetMoving()
    {
        return movingEnemies;
    }
    public bool GetHalt()
    {
        return halt;
    }
    // Update is called once per frame
    void Update()
    {

        //Gano
        if (currentEnemies.Count == 0 && !writing && currentState != STATE.Won)
            currentState = STATE.Won;
        //Perdio
        else if (NessHP <= 0 && !writing && currentState != STATE.Lose)
            currentState = STATE.Lose;
        if (halt || movingEnemies)
            return;
        switch (currentState)
        {
            case STATE.PlayerTurn:


                
                if (currentPlayerState==PlayerSTATE.Idle && !writing)
                {
                    NessMenu.transform.localPosition = Vector3.SmoothDamp(NessMenu.transform.localPosition, menuDisplacement, ref velocity, 0.2f);
                    if (NessMenu.transform.localPosition.y >= menuDisplacement.y - 30f)
                        rotator.SetCanMove(true);
                }
                else
                {
                    NessMenu.transform.localPosition = Vector3.SmoothDamp(NessMenu.transform.localPosition, new Vector3(menuDisplacement.x, -335.0f, menuDisplacement.z), ref velocity, 0.2f);
                    rotator.SetCanMove(false);
                }
                if (border.transform.position.y <= borderDisplacement)
                {
                    border.transform.position = new Vector3(border.transform.position.x, border.transform.position.y + 1f, border.transform.position.z);
                }
                switch(currentPlayerState)
                {
                    case PlayerSTATE.SelectingEnemyPSI:
                        if (Input.GetKeyDown(KeyCode.Space))
                        {
                            currentPlayerState = PlayerSTATE.Idle;
                            currentEnemies[enemySelect].ResetColor();
                            moveEnemySelector.SetActive(false);
                            usingPSI = true;
                            psiAnimation = psiSelector.getPSIAnimation();
                            //Move dialogue box
                            dialogue.SetActive(true);
                            moveDialogue.SetDestination(new Vector3(0, 85, 0));


                            //Add lines
                            writing = true;
                            lineIndex = 0;
                            lines.Add("@ Ness uses " + psiSelector.getPSIName());
                            damage = psiSelector.getPSIDamage();

                            attacksAllRow = psiSelector.getPSIAllRow();
                            if (attacksAllRow)
                            {
                                for (int i = 0; i < currentEnemies.Count; i++)
                                {
                                    lines.Add("@ " + currentEnemies[i].GetName() + " receives " + damage + " of damage!");
                                }
                            }
                            else
                            {

                                lines.Add("@ " + currentEnemies[enemySelect].GetName() + " receives " + damage + " of damage!");
                            }

                            
                            ppBar.CalculateDistance(-psiSelector.getPSICost());
                            psiSelector.Deactivate();
                            

                        }
                        if (Input.GetKey(KeyCode.A) && !writing && enemySelect > 0)
                        {
                            currentEnemies[enemySelect].ResetColor();
                            enemySelect--;
                            currentEnemies[enemySelect].SelectColor();
                            moveEnemyScript.SetName(currentEnemies[enemySelect].GetName());
                            moveEnemyScript.SetDestination(new Vector3(currentEnemies[enemySelect].transform.position.x, currentEnemies[enemySelect].transform.position.y + currentEnemies[enemySelect].GetHeight(), currentEnemies[enemySelect].transform.position.z));

                        }
                        if (Input.GetKey(KeyCode.D) && !writing && enemySelect < currentEnemies.Count - 1)
                        {
                            currentEnemies[enemySelect].ResetColor();
                            enemySelect++;
                            currentEnemies[enemySelect].SelectColor();
                            moveEnemyScript.SetName(currentEnemies[enemySelect].GetName());
                            moveEnemyScript.SetDestination(new Vector3(currentEnemies[enemySelect].transform.position.x, currentEnemies[enemySelect].transform.position.y + currentEnemies[enemySelect].GetHeight(), currentEnemies[enemySelect].transform.position.z));
                        }
                        if (Input.GetKeyDown(KeyCode.Escape))
                        {
                            currentEnemies[enemySelect].ResetColor();
                            moveEnemySelector.SetActive(false);
                            currentPlayerState = PlayerSTATE.ChoosingPSIMove;
                           
                            psiSelector.MoveMenu();
                        }
                        break;
                    case PlayerSTATE.ChoosingPSIMove:
                        if (Input.GetKeyDown(KeyCode.Space) && psiSelector.getInPosition() && !psiSelector.getChoosingCat())
                        {
                            psiSelf = !psiSelector.getPSIOffensive();
                            if (psiSelf)
                            {
                                partyMemberSelector.SetActive(true);
                                playerOverlay.SetSelected(true);
                                currentPlayerState = PlayerSTATE.ChoosingPartyMemberPSI;
                            }
                            else
                            {
                                currentPlayerState = PlayerSTATE.SelectingEnemyPSI;
                                moveEnemySelector.SetActive(true);
                                moveEnemyScript.SetName(currentEnemies[enemySelect].GetName());
                                moveEnemyScript.SetPosition(new Vector3(currentEnemies[enemySelect].transform.position.x, currentEnemies[enemySelect].transform.position.y + currentEnemies[enemySelect].GetHeight(), currentEnemies[enemySelect].transform.position.z));

                                moveEnemyScript.SetDestination(new Vector3(currentEnemies[enemySelect].transform.position.x, currentEnemies[enemySelect].transform.position.y + currentEnemies[enemySelect].GetHeight(), currentEnemies[enemySelect].transform.position.z));

                                currentEnemies[enemySelect].SelectColor();
                            }

                            psiSelector.MoveMenu();

                        }
                        if (Input.GetKeyDown(KeyCode.Escape) && psiSelector.getChoosingCat())
                        {
                            psiSelector.MoveMenu();
                            currentPlayerState = PlayerSTATE.Idle;
                        }
                        break;
                    case PlayerSTATE.SelectingEnemyBash:
                        if (Input.GetKey(KeyCode.A) && !writing && enemySelect > 0)
                        {
                            currentEnemies[enemySelect].ResetColor();
                            enemySelect--;
                            currentEnemies[enemySelect].SelectColor();
                            moveEnemyScript.SetName(currentEnemies[enemySelect].GetName());
                            moveEnemyScript.SetDestination(new Vector3(currentEnemies[enemySelect].transform.position.x, currentEnemies[enemySelect].transform.position.y + currentEnemies[enemySelect].GetHeight(), currentEnemies[enemySelect].transform.position.z));

                        }
                        if (Input.GetKey(KeyCode.D) && !writing && enemySelect < currentEnemies.Count - 1)
                        {
                            currentEnemies[enemySelect].ResetColor();
                            enemySelect++;
                            currentEnemies[enemySelect].SelectColor();
                            moveEnemyScript.SetName(currentEnemies[enemySelect].GetName());
                            moveEnemyScript.SetDestination(new Vector3(currentEnemies[enemySelect].transform.position.x, currentEnemies[enemySelect].transform.position.y + currentEnemies[enemySelect].GetHeight(), currentEnemies[enemySelect].transform.position.z));

                        }
                        if (Input.GetKeyDown(KeyCode.Space) && !writing)
                        {
                            currentEnemies[enemySelect].ResetColor();
                            moveEnemySelector.SetActive(false);
                            damage = 50;

                            //Move dialogue box
                            dialogue.SetActive(true);
                            moveDialogue.SetDestination(new Vector3(0, 85, 0));
                            

                            //Add lines
                            writing = true;
                            lineIndex = 0;
                            lines.Add("@ Ness attacks the enemy!");
                            lines.Add("@ " + currentEnemies[enemySelect].GetName() + " receives " + damage + " of damage!");
                            

                        }
                        if (Input.GetKeyDown(KeyCode.Escape))
                        {
                            currentEnemies[enemySelect].ResetColor();
                            moveEnemySelector.SetActive(false);
                            currentPlayerState = PlayerSTATE.Idle;
                        }
                        break;
                    case PlayerSTATE.ChoosingItem:
                        if (Input.GetKeyDown(KeyCode.Escape))
                        {
                            itemSelector.MoveMenu();
                            currentPlayerState = PlayerSTATE.Idle;
                        }
                        if(Input.GetKeyDown(KeyCode.Space))
                        {
                            itemSelector.MoveMenu();
                            partyMemberSelector.SetActive(true);
                            currentPlayerState = PlayerSTATE.ChoosingPartyMemberItem;
                            playerOverlay.SetSelected(true);
                        }
                        break;
                    case PlayerSTATE.ChoosingPartyMemberItem:
                        if(Input.GetKeyDown(KeyCode.Escape))
                        {
                            itemSelector.MoveMenu();
                            partyMemberSelector.SetActive(false);
                            playerOverlay.SetSelected(false);
                            currentPlayerState = PlayerSTATE.ChoosingItem;
                        }
                        if(Input.GetKeyDown(KeyCode.Space))
                        {
                            int index = itemSelector.getItemIndex();
                            //Move dialogue box
                            dialogue.SetActive(true);
                            moveDialogue.SetDestination(new Vector3(0, 85, 0));


                            //Add lines
                            writing = true;
                            itemHeal = true;
                            lineIndex = 0;
                            hpGain = inventory.items[index].GetHPGain();
                            lines.Add("@ Ness eats a " + inventory.items[index].GetName());
                           
                            lines.Add("@ Ness recuperates " + hpGain + " of HP!");

                            inventory.items.RemoveAt(index);
                            itemSelector.Deactivate();
                            partyMemberSelector.SetActive(false);
                            playerOverlay.SetSelected(false);

                            currentPlayerState = PlayerSTATE.Idle;

                        }
                        break;
                    case PlayerSTATE.ChoosingPartyMemberPSI:
                        if (Input.GetKeyDown(KeyCode.Escape))
                        {
                            partyMemberSelector.SetActive(false);
                            psiSelector.MoveMenu();
                            playerOverlay.SetSelected(false);


                            currentPlayerState = PlayerSTATE.ChoosingPSIMove;

                        }
                        if (Input.GetKeyDown(KeyCode.Space))
                        {

                            usingPSI = true;
                            psiAnimation = psiSelector.getPSIAnimation();
                            //Move dialogue box
                            dialogue.SetActive(true);
                            moveDialogue.SetDestination(new Vector3(0, 85, 0));


                            //Add lines
                            writing = true;
                            lineIndex = 0;
                            damage = psiSelector.getPSIDamage();
                            lines.Add("@ Ness uses a " + psiSelector.getPSIName());

                            lines.Add("@ Ness recuperates " + damage + " of HP!");


                            NessHP += damage;
                            //adsdjasiodjasiodjasiodjdisioadosadpaosdjsaodjasio
                            psiSelector.Deactivate();
                            partyMemberSelector.SetActive(false);
                            playerOverlay.SetSelected(false);

                            ppBar.CalculateDistance(-psiSelector.getPSICost());
                            currentPlayerState = PlayerSTATE.Idle;

                        }
                        break;

                }
                //Opcion Menu Seleccionar
                if (Input.GetKeyDown("space") && !isAttacking && rotator.GetCanMove() && !writing)
                {
                        switch (rotator.num)
                        {
                            case 1:
                            currentPlayerState = PlayerSTATE.SelectingEnemyBash;
                            moveEnemySelector.SetActive(true);
                            moveEnemyScript.SetName(currentEnemies[enemySelect].GetName());
                            moveEnemyScript.SetPosition(new Vector3(currentEnemies[enemySelect].transform.position.x, currentEnemies[enemySelect].transform.position.y + currentEnemies[enemySelect].GetHeight(), currentEnemies[enemySelect].transform.position.z));

                            moveEnemyScript.SetDestination(new Vector3(currentEnemies[enemySelect].transform.position.x, currentEnemies[enemySelect].transform.position.y + currentEnemies[enemySelect].GetHeight(), currentEnemies[enemySelect].transform.position.z));
                            currentEnemies[enemySelect].SelectColor();

                            break;
                            case 2:
                            psiObject.SetActive(true);
                            currentPlayerState = PlayerSTATE.ChoosingPSIMove;
                            
                            psiSelector.MoveMenu();
                            
                            break;

                            case 3:
                                if (inventory.items.Count != 0)
                                {
                                    currentEnemies[enemySelect].ResetColor();
                                    StartCoroutine(TextScroll("@ Ness eats a " + inventory.items[0].GetName()));
                                    NessHP += inventory.items[0].GetHPGain();
                                    hpBar.CalculateDistance(inventory.items[0].GetHPGain());
                                    inventory.items.RemoveAt(0);
                                }
                                break;
                        case 6:
                            if (inventory.getItemsCount() > 0)
                            {
                                currentPlayerState = PlayerSTATE.ChoosingItem;
                                itemObject.SetActive(true);

                                itemSelector.Activate();

                                itemSelector.MoveMenu();
                            }
                            break;


                        }
                    return;
                }

                
                if(writing)
                { 
                    //Escribir cada linea
                    if (lineIndex < lines.Count)
                    {
                        if (!isTyping)
                        {
                            isTyping = true;

                            if(!psiSelf)
                            { 
                                if (attacksAllRow)
                                {
                                    if (lineIndex >= 1 && damage > 0 && !enemyDie)
                                    {
                                        currentEnemies[enemyAuxIndex].ActivateShake(damage);
                                        currentEnemies[enemyAuxIndex].ReceiveDamage(damage);


                                    }
                                }
                                else if (lineIndex == 1 && damage > 0)
                                {

                                    currentEnemies[enemySelect].ActivateShake(damage);
                                    currentEnemies[enemySelect].ReceiveDamage(damage);

                                }
                            }
                            StartCoroutine(TextScroll(lines[lineIndex]));

                            if(!psiSelf)
                            {
                                if (attacksAllRow)
                                {
                                    if (currentEnemies.Count > 0 && currentEnemies[enemyAuxIndex].GetHealth() <= 0)
                                    {
                                        if (!enemyDie)
                                        {
                                            enemyDie = true;
                                            lines.Insert(lineIndex + 1, "@ " + currentEnemies[enemyAuxIndex].GetName() + " has been defeated!");
                                        }
                                        else
                                        {
                                            halt = true;

                                            currentEnemies[enemyAuxIndex].StartCoroutine("Die");
                                            if (enemyAuxIndex > 0)
                                                enemyAuxIndex--;
                                            enemyDie = false;
                                        }
                                        return;
                                    }
                                    if (lineIndex >= 1 && enemyAuxIndex < currentEnemies.Count - 1)
                                        enemyAuxIndex++;



                                }
                                else
                                {
                                    if (currentEnemies.Count > 0 && currentEnemies[enemySelect].GetHealth() <= 0)
                                    {
                                        if (!enemyDie)
                                        {
                                            enemyDie = true;
                                            lines.Insert(lineIndex + 1, "@ " + currentEnemies[enemySelect].GetName() + " has been defeated!");
                                        }
                                        else
                                        {
                                            halt = true;
                                            enemyAuxIndex--;
                                            currentEnemies[enemySelect].StartCoroutine("Die");
                                            enemyDie = false;
                                        }
                                        return;
                                    }
                                }
                            }
                            

                        }
                    }
                    //Terminado lineas
                    else
                    {
                        turnOver = true;
                        writing = false;
                        enemyDie = false;
                    }
                }
                if (turnOver)
                {
                    usingPSI = false;
                    lines.Clear();
                    damage = 0;
                    enemySelect = 0;
                    enemyAuxIndex = 0;
                    turnOver = false;
                    attacksAllRow = false;
                    EnemyIndex = 0;
                    currentState = STATE.EnemyTurn;
                    currentPlayerState = PlayerSTATE.Idle;
                    psiObject.SetActive(false);
                    itemObject.SetActive(false);
                    psiSelf = false;
                    itemHeal = false;
                    hpGain = 0;
                    StartCoroutine(Halt(0.1f));

                }


                break;

            case STATE.EnemyTurn:


                if (border.transform.position.y >= borderDisplacement - 28.0f)
                {
                    border.transform.position = new Vector3(border.transform.position.x, border.transform.position.y - 1f, border.transform.position.z);
                    
                }
                if (!writing && !turnOver)
                {
                    currentEnemies[EnemyIndex].AttackingPosition();
                    aux = currentEnemies[EnemyIndex].ChooseAttack();

                    //Move dialogue box

                    dialogue.SetActive(true);
                    moveDialogue.SetDestination(new Vector3(0, 85, 0));

                    //Add lines
                    writing = true;
                    lineIndex = 0;
                    lines.Add("@ "+ currentEnemies[EnemyIndex].GetName() + aux.moveMessage);
                    lines.Add("@ Ness receives " + aux.moveDamage + " of damage!");

                }
                if (writing)
                {
                    if (lineIndex < lines.Count)
                    {
                        if (!isTyping)
                        {
                            isTyping = true;
                            StartCoroutine(TextScroll(lines[lineIndex]));
                            if(lineIndex == 1)
                            {
                                hpBar.CalculateDistance(-aux.moveDamage);
                                StartCoroutine(cam.Shake(0.2f, 2f));

                            }

                        }
                    }
                    else
                    {
                        turnOver = true;
                        writing = false;
                    }
                }
                if (turnOver )
                {
                    currentEnemies[EnemyIndex].AttackingPosition();

                    StartCoroutine(Halt(0.2f));
                    lines.Clear();
                    EnemyIndex++;
                    turnOver = false;
                    

                    if (EnemyIndex >= currentEnemies.Count)
                    {
                        moveDialogue.SetDestination(new Vector3(0, 150, 0));
                        currentState = STATE.PlayerTurn;
                        NessMenu.SetActive(true);
                        return;
                    }

                }
                break;

            case STATE.Won:

                if (!once && !isTyping)
                {
                    once = true;
                    turnOver = false;
                    isTyping = true;
                    dialogue.SetActive(true);
                    moveDialogue.SetDestination(new Vector3(0, 85, 0));
                    StartCoroutine(TextScroll("@ Ness wins!"));
                }
                break;

            case STATE.Lose:

                if (!once)
                {
                    once = true;
                    turnOver = false;
                    isTyping = true;
                    dialogue.SetActive(true);
                    moveDialogue.SetDestination(new Vector3(0, 85, 0));
                    StartCoroutine(TextScroll("@ Enemies wins!"));
                }
                break;

            case STATE.Flee:
                Destroy(gameObject);
                break;
        }

    }
    public bool GetTyping()
    {
        return isTyping;
    }

    public bool GetWriting()
    {
        return writing;
    }
    public void SetHaltValue(bool a)
    {
        halt = a;
    }


    public IEnumerator Halt(float duration)
    {

        halt = true;
        float elapsed = 0.0f;
        while (elapsed < duration)
        {


            elapsed += Time.deltaTime;
            yield return null;
        }
        halt = false;
    }
    void OnGUI()
    {
        GUI.Label(new Rect(10, 310, 100, 20), "Ness HP: " + NessHP.ToString());
        for(int index = 0; index < currentEnemies.Count; index++)
        {
            if(currentEnemies[index]!=null)
            GUI.Label(new Rect(10, 320 + (index * 10), 100, 20), "Enemy " + (index + 1) + " HP" + currentEnemies[index].GetHealth());
        }

        switch (currentState)
        {
            case STATE.PlayerTurn:
                GUI.Label(new Rect(10, 300, 100, 20), "Ness TURN");

                break;

            case STATE.EnemyTurn:
                GUI.Label(new Rect(10, 300, 100, 20), "Enemy TURN");

                break;

            case STATE.Won:
                GUI.Label(new Rect(10, 300, 100, 20), "Ness WON");
                break;

            case STATE.Lose:
                GUI.Label(new Rect(10, 300, 100, 20), "Ness lose");
                break;

            case STATE.Flee:
                GUI.Label(new Rect(10, 300, 100, 20), "Ness FLEE");
                Destroy(gameObject);
                break;
        }
    }

}