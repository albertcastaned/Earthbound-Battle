 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;


//Manejador de la batalla
public class Battle : MonoBehaviour
{
    //Estados posibles
    protected enum STATE{
        PlayerTurn,
        EnemyTurn,
        Won,
        Lose,
        Flee
    };


    protected enum PlayerSTATE
    {
        Idle,
        ChoosingPSIMove,
        SelectingEnemyBash,
        SelectingEnemyPSI,
        ChoosingItem,
        Attacking

    };

    private STATE currentState;
    private PlayerSTATE currentPlayerState;
    public int NessHP;
    public List<PsiData> psiMoves;

    public CameraShake cam;
    public Text text;
    private bool halt;
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
    private int damage;
    //Crear lista y ordenarlas dependiendo de la posicion x
    void Awake()
    {
        currentEnemies = new List<Enemy>();
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            Enemy aux = enemy.GetComponent<Enemy>();
            currentEnemies.Add(aux);
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
        halt = true;
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
    //Escribir dialogo letra por letra
    private IEnumerator TextScroll(string lineOfText,bool attack)
    {
        dialogue.SetActive(true);
        moveDialogue.SetDestination(new Vector3(0, 85, 0));
        int letter = 0;

        text.text = "";
        isTyping = true;
        if (attack)
        {
            isAttacking = true;
        }
        int lineLength = lineOfText.Length - 1;
        float speed;
        while (isTyping && (letter <= lineLength))
        {
            speed = 0.05f;
            if (Input.GetKey(KeyCode.Space))
            {
                speed = 0.01f;
            }
            else
            {
                speed = 0.05f;
            }

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

            yield return new WaitForSeconds(speed);
        }
        isTyping = false;

        if (attack)
        {
            isAttacking = false;
            turnOver = true;
        }

    }
    
    //Mover posicion de enemigos
    private IEnumerator ShiftEnemiesPosition()
    {

            Vector3 destination = new Vector3(0, 17, 60);
            switch (currentEnemies.Count)
            {
                case 1:
                    while (currentEnemies.Count > 0 && currentEnemies[0].getTransform() != destination)
                    {

                    currentEnemies[0].transform.position = Vector3.SmoothDamp(currentEnemies[0].getTransform(), destination, ref enemyShiftVelocity, 0.2f);
                        yield return null;

                    }
                    break;

            }
        
        

    }

    //Quitar enemigo de la lista
    public void removeEnemy(Enemy en)
    {
        currentEnemies.Remove(en);
        if (currentEnemies.Count != 0)
        {
            StartCoroutine("ShiftEnemiesPosition");
        }
    }
    // Update is called once per frame
    void Update()
    {
        //Gano
        if (currentEnemies.Count == 0 && !isTyping && currentState != STATE.Won)
            currentState = STATE.Won;
        //Perdio
        else if (NessHP <= 0 && !isTyping && currentState != STATE.Lose)
            currentState = STATE.Lose;

        switch (currentState)
        {
            case STATE.PlayerTurn:


                NessMenu.SetActive(true);
                if (currentPlayerState==PlayerSTATE.Idle && !isTyping)
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
                            StartCoroutine(TextScroll("Ness uses " + psiSelector.getPSIName(), true));
                            damage = psiSelector.getPSIDamage();
                            ppBar.CalculateDistance(-psiSelector.getPSICost());
                            psiSelector.Deactivate();

                        }
                        if (Input.GetKey(KeyCode.A) && !isTyping && enemySelect > 0)
                        {
                            currentEnemies[enemySelect].ResetColor();
                            enemySelect--;
                            currentEnemies[enemySelect].SelectColor();
                            moveEnemyScript.SetName(currentEnemies[enemySelect].GetName());
                            moveEnemyScript.SetDestination(currentEnemies[enemySelect].transform.position);

                        }
                        if (Input.GetKey(KeyCode.D) && !isTyping && enemySelect < currentEnemies.Count - 1)
                        {
                            currentEnemies[enemySelect].ResetColor();
                            enemySelect++;
                            currentEnemies[enemySelect].SelectColor();
                            moveEnemyScript.SetName(currentEnemies[enemySelect].GetName());
                            moveEnemyScript.SetDestination(currentEnemies[enemySelect].transform.position);

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
                            currentPlayerState = PlayerSTATE.SelectingEnemyPSI;
                            moveEnemySelector.SetActive(true);
                            moveEnemyScript.SetName(currentEnemies[enemySelect].GetName());
                            moveEnemyScript.SetPosition(currentEnemies[enemySelect].transform.position);
                            moveEnemyScript.SetDestination(currentEnemies[enemySelect].transform.position);


                            currentEnemies[enemySelect].SelectColor();
                            psiSelector.MoveMenu();
                        }
                        if (Input.GetKeyDown(KeyCode.Escape) && psiSelector.getChoosingCat())
                        {
                            psiSelector.MoveMenu();
                            currentPlayerState = PlayerSTATE.Idle;
                        }
                        break;
                    case PlayerSTATE.SelectingEnemyBash:
                        if (Input.GetKey(KeyCode.A) && !isTyping && enemySelect > 0)
                        {
                            currentEnemies[enemySelect].ResetColor();
                            enemySelect--;
                            currentEnemies[enemySelect].SelectColor();
                            moveEnemyScript.SetName(currentEnemies[enemySelect].GetName());
                            moveEnemyScript.SetDestination(currentEnemies[enemySelect].transform.position);

                        }
                        if (Input.GetKey(KeyCode.D) && !isTyping && enemySelect < currentEnemies.Count - 1)
                        {
                            currentEnemies[enemySelect].ResetColor();
                            enemySelect++;
                            currentEnemies[enemySelect].SelectColor();
                            moveEnemyScript.SetName(currentEnemies[enemySelect].GetName());
                            moveEnemyScript.SetDestination(currentEnemies[enemySelect].transform.position);

                        }
                        if (Input.GetKeyDown(KeyCode.Space) && !isTyping)
                        {
                            currentEnemies[enemySelect].ResetColor();
                            moveEnemySelector.SetActive(false);
                            damage = 50;
                            StartCoroutine(TextScroll("Ness attacks the enemy!", true));

                        }
                        if (Input.GetKeyDown(KeyCode.Escape))
                        {
                            currentEnemies[enemySelect].ResetColor();
                            moveEnemySelector.SetActive(false);
                            currentPlayerState = PlayerSTATE.Idle;
                        }
                        break;
                }

                if (Input.GetKeyDown("space") && !isAttacking && !isTyping && rotator.GetCanMove())
                {
                        switch (rotator.num)
                        {
                            case 1:
                            currentPlayerState = PlayerSTATE.SelectingEnemyBash;
                            moveEnemySelector.SetActive(true);
                            moveEnemyScript.SetName(currentEnemies[enemySelect].GetName());
                            moveEnemyScript.SetPosition(currentEnemies[enemySelect].transform.position);
                            moveEnemyScript.SetDestination(currentEnemies[enemySelect].transform.position);
                            currentEnemies[enemySelect].SelectColor();

                            break;
                            case 2:
                            currentPlayerState = PlayerSTATE.ChoosingPSIMove;
                            psiSelector.MoveMenu();
                            break;

                            case 3:
                                if (inventory.items.Count != 0)
                                {
                                    currentEnemies[enemySelect].ResetColor();
                                    StartCoroutine(TextScroll("Ness eats a " + inventory.items[0].GetName(), true));
                                    NessHP += inventory.items[0].GetHPGain();
                                    hpBar.CalculateDistance(inventory.items[0].GetHPGain());
                                    inventory.items.RemoveAt(0);
                                }
                                break;
                        }
                }


                if (turnOver)
                {
                    currentEnemies[enemySelect].ActivateShake(damage);
                    currentEnemies[enemySelect].ReceiveDamage(damage);
                    enemySelect = 0;
                    turnOver = false;
                    EnemyIndex = 0;
                    currentState = STATE.EnemyTurn;
                    currentPlayerState = PlayerSTATE.Idle;

                }


                break;

            case STATE.EnemyTurn:


                if (border.transform.position.y >= borderDisplacement - 28.0f)
                {
                    border.transform.position = new Vector3(border.transform.position.x, border.transform.position.y - 1f, border.transform.position.z);

                }
                if (!isAttacking && !turnOver)
                {
                    aux = currentEnemies[EnemyIndex].ChooseAttack();
                    StartCoroutine(TextScroll(currentEnemies[EnemyIndex].GetName() + aux.moveMessage,true));
                }
                if (turnOver )
                {
                    NessHP -= aux.moveDamage;
                    hpBar.CalculateDistance(-aux.moveDamage);
                    StartCoroutine(cam.Shake(0.2f, 2f));

                    EnemyIndex++;
                    turnOver = false;
                    if (EnemyIndex >= currentEnemies.Count)
                    {
                        moveDialogue.SetDestination(new Vector3(0, 150, 0));
                        currentState = STATE.PlayerTurn;
                    }

                }
                break;

            case STATE.Won:

                if (!once)
                {
                    once = true;
                    turnOver = false;
                    StartCoroutine(TextScroll("Ness wins!",false));
                }
                break;

            case STATE.Lose:

                if (!once)
                {
                    once = true;
                    turnOver = false;
                    StartCoroutine(TextScroll("Enemies wins!",false));
                }
                break;

            case STATE.Flee:
                Destroy(gameObject);
                break;
        }

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