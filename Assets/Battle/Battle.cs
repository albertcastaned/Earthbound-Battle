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
        NessHP = 100;

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

    }
    private IEnumerator TextScroll(string lineOfText,bool attack)
    {

        int letter = 0;

        text.text = "";
        isTyping = true;
        if (attack)
        {
            isAttacking = true;
        }
        int lineLength = lineOfText.Length - 1;
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

            yield return new WaitForSeconds(0.05f);
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
                if (Input.GetKeyDown("1") && !isAttacking && !isTyping)
                {
                    currentEnemies[enemySelect].ResetColor();
                    StartCoroutine(TextScroll("Ness attacks the enemy!",true));
                    currentEnemies[enemySelect].ReceiveDamage(20);
                    currentEnemies[enemySelect].ActivateShake(20);


                }
                if (Input.GetKeyDown("2") && !isAttacking && !isTyping)
                {
                    currentEnemies[enemySelect].ResetColor();
                    StartCoroutine(TextScroll("Ness uses PK FIRE!",true));
                    currentEnemies[enemySelect].ReceiveDamage(50);
                    currentEnemies[enemySelect].ActivateShake(50);

                }

                if (Input.GetKeyDown("3") && !isAttacking && !isTyping && inventory.items.Count != 0)
                {
                    currentEnemies[enemySelect].ResetColor();
                    StartCoroutine(TextScroll("Ness eats a " + inventory.items[0].GetName(), true));
                    NessHP += inventory.items[0].GetHPGain();
                    inventory.items.RemoveAt(0);

                }
                if (turnOver)
                {
                    enemySelect = 0;
                    turnOver = false;
                    EnemyIndex = 0;
                    currentState = STATE.EnemyTurn;
                }


                break;

            case STATE.EnemyTurn:
                if (!isAttacking && !turnOver)
                {
                    MovesData aux = currentEnemies[EnemyIndex].ChooseAttack();
                    StartCoroutine(TextScroll(currentEnemies[EnemyIndex].GetName() + aux.moveMessage,true));
                    NessHP -= aux.moveDamage;
                    StartCoroutine(cam.Shake(0.2f,2f));
                }
                if (turnOver)
                {
                   
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