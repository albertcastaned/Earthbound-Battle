using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Battle : MonoBehaviour
{
    protected enum STATE{
        PlayerTurn,
        EnemyTurn,
        Won,
        Lose,
        Flee
    };

    private STATE currentState;
    public int NessHP;
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
    // Start is called before the first frame update
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
    void Start()
    {
        NessHP = 30;
        rotator = menuSpinner.GetComponent<Rotate>();
        enemySelect = 0;
        halt = true;
        text.text = "";
        isTyping = false;
        isAttacking = false;
        turnOver = false;
        StartCoroutine(TextScroll("An enemy approaches!",false));
        currentEnemies[enemySelect].SelectColor();
        currentState = STATE.PlayerTurn;
        once = false;
        EnemyIndex = 0;
        borderDisplacement = border.transform.position.y + 28.0f;
        menuDisplacement = new Vector3(NessMenu.transform.localPosition.x, NessMenu.transform.localPosition.y + 335f, NessMenu.transform.localPosition.z);

    }
    private IEnumerator TextScroll(string lineOfText,bool attack)
    {
        dialogue.SetActive(true);
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
                print("a");
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

    // Update is called once per frame
    void Update()
    {
        currentEnemies.RemoveAll(enemy => enemy == null);
        if (currentEnemies.Count == 0 && !isTyping && currentState != STATE.Won)
            currentState = STATE.Won;
        if (NessHP <= 0 && !isTyping && currentState != STATE.Lose)
            currentState = STATE.Lose;

        switch (currentState)
        {
            case STATE.PlayerTurn:
                if(!isAttacking)
                dialogue.SetActive(false);

                NessMenu.SetActive(true);
                if (!isAttacking)
                {
                    NessMenu.transform.localPosition = Vector3.SmoothDamp(NessMenu.transform.localPosition, menuDisplacement, ref velocity, 0.2f);
                    if (NessMenu.transform.localPosition.y >= menuDisplacement.y - 30f)
                    rotator.SetCanMove(true);
                }
                else
                {
                    NessMenu.transform.localPosition = Vector3.SmoothDamp(NessMenu.transform.localPosition, new Vector3(menuDisplacement.x, -335.0f, menuDisplacement.z), ref velocity, 0.3f);
                    rotator.SetCanMove(false);

                }


                if (border.transform.position.y <= borderDisplacement)
                {
                    border.transform.position = new Vector3(border.transform.position.x, border.transform.position.y + 1f, border.transform.position.z);
                }
                if (Input.GetKeyDown("left") && !isTyping && enemySelect > 0)
                {
                    currentEnemies[enemySelect].ResetColor();
                    enemySelect--;
                    currentEnemies[enemySelect].SelectColor();

                }
                if (Input.GetKeyDown("right") && !isTyping && enemySelect < currentEnemies.Count - 1) 
                {
                    currentEnemies[enemySelect].ResetColor();
                    enemySelect++;
                    currentEnemies[enemySelect].SelectColor();

                }
                if (Input.GetKeyDown("space") && !isAttacking && !isTyping && rotator.GetCanMove())
                {
                    switch(rotator.num)
                    {
                        case 1:

                            currentEnemies[enemySelect].ResetColor();
                            StartCoroutine(TextScroll("Ness attacks the enemy!", true));
                            break;
                        case 2:

                            currentEnemies[enemySelect].ResetColor();
                            StartCoroutine(TextScroll("Ness uses PK FIRE!", true));
                            ppBar.CalculateDistance(-7);
                            break;

                        case 3:
                            if(inventory.items.Count!=0)
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

                    currentEnemies[enemySelect].ReceiveDamage(50);
                    currentEnemies[enemySelect].ActivateShake(50);
                    enemySelect = 0;
                    turnOver = false;
                    EnemyIndex = 0;
                    currentState = STATE.EnemyTurn;
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
                        currentEnemies[enemySelect].SelectColor();
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