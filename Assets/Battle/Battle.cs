using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;


//Battle controller
public class Battle : MonoBehaviour
{
    //Possible battle states
    protected enum STATE{
        PlayerTurn,
        EnemyTurn,
        Commands,
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
        TurnOver

    };


    public class Command
    {
        private GameObject caster;
        private GameObject target;
        private int value;

        private string message;
        private int speed;
        private bool miss = false;
        private bool dodge = false;
        private bool smash = false;
        private GameObject animation;
        private string status;


        public GameObject Target { get => target; set => target = value; }
        public int Value { get => value; set => this.value = value; }
        public string Message { get => message; set => message = value; }
        public int Speed { get => speed; set => speed = value; }
        public string MyType { get; set; }
        public GameObject Caster { get => caster; set => caster = value; }
        public GameObject Animation { get => animation; set => animation = value; }
        public bool Miss { get => miss; set => miss = value; }
        public bool Dodge { get => dodge; set => dodge = value; }
        public bool Smash { get => smash; set => smash = value; }

        public Command(string msg, int moveTypeIndex, GameObject cast, GameObject targ, int val, int spd,GameObject anim, string stat = "None")
        {
            Message = msg;

            switch(moveTypeIndex)
            {
                case -1:
                    MyType = "NormalMessage";
                    break;
                case 0:
                    MyType = "BASH";
                    break;
                case 1:
                    MyType = "SingleTargetPSI";
                    break;
                case 2:
                    MyType = "MultipleTargetPSI";
                    break;
                case 3:
                    MyType = "SingleTargetHeal";
                    break;
                case 4:
                    MyType = "MultipleTargetHeal";
                    break;
                case 5:
                    MyType = "SingleTargetHealPSI";
                    break;
                case 6:
                    MyType = "MultipleTargetHealPSI";
                    break;
                case 7:
                    MyType = "SingleTargetStatus";
                    break;
                case 8:
                    MyType = "MultipleTargetStatus";
                    break;
                case 9:
                    MyType = "Defend";
                    break;

                default:
                    MyType = "Bash";
                    break;
            }
            Caster = cast;
            Target = targ;
            Speed = spd;
            Animation = anim;
            status = stat;
            if (moveTypeIndex!=0)
            Value = val;
            else
            {
                if (Random.Range(1, 16) == 1)
                {
                    Value = 0;
                    miss = true;
                    return;
                }

                if (Caster.tag == "Player")
                {
                    float aux = Caster.GetComponent<Player>().GetGuts() / 500f;
                    if (aux > 1f/20f)
                    {
                        if(Random.Range(0,500) <= aux)
                        {
                            smash = true;
                            Value = 4 * Caster.GetComponent<Player>().GetOffense() - Target.GetComponent<Enemy>().GetDefense();
                            return;
                        }
                    }
                    else
                    {
                        if (Random.Range(0, 20) <= 1)
                        {
                            smash = true;
                            Value = 4 * Caster.GetComponent<Player>().GetOffense() - Target.GetComponent<Enemy>().GetDefense();
                            return;
                        }
                    }

                    if (Target.GetComponent<Enemy>().Status != "Asleep")
                    {
                        int auxDodge = 2 * Target.GetComponent<Enemy>().GetSpeed() - Caster.GetComponent<Player>().GetSpeed();
                        if (Random.Range(0, 500) <= auxDodge)
                        {
                            dodge = true;
                            value = 0;
                            return;
                        }
                    }
                    Value = 2 * Caster.GetComponent<Player>().GetOffense() - Target.GetComponent<Enemy>().GetDefense();
                }
                else
                {
                    int auxDodge = 2 * Target.GetComponent<Player>().GetSpeed() - Caster.GetComponent<Enemy>().GetSpeed();
                    if (Random.Range(0, 500) <= auxDodge)
                    {
                        dodge = true;
                        value = 0;
                        return;
                    }
                    Value = 2 * Caster.GetComponent<Enemy>().GetOffense() - Target.GetComponent<Player>().GetDeffense();
                    if (Target.GetComponent<Player>().Defending)
                        Value /= 2;
                }

                Value += Mathf.Abs((int)Mathf.Round(Value * Random.Range(-0.25f, 0.25f)));
            }

        }

    }

    

    private STATE currentState;
    private PlayerSTATE currentPlayerState;

    private List<Command> currentCommands;

    public int NessHP;
    public List<PsiData> psiMoves;

    public CameraShake cam;
    public TMP_Text text;
    private bool isTyping;
    private bool isAttacking;
    private bool once;
    private int enemySelect;

    public List<Enemy> currentEnemies;
    public List<Player> currentPlayers;

    public Inventory inventory;
    public MeterRoll hpBar;
    public MeterRoll ppBar;

    private MovesData aux;

    public Vector3 menuDisplacement;
    public GameObject dialogue;
    public GameObject menuSpinner;
    private Rotate rotator;
    public GameObject NessMenu;
    private Vector3 velocity = Vector3.zero;
    public GameObject moveEnemySelector;
    private MoveToEnemy moveEnemyScript;
    private MoveDialogue moveDialogue;
    public SelectPSI psiSelector;
    public ItemSelectorManager itemSelector;
    public GameObject psiObject;

    private GameObject dialoguePos;
    public GameObject itemObject;
    private bool halt = false;
    public GameObject partyMemberSelector;
    private int damage = 0;
    public GameObject currentText;
    private Vector3 textVelocity = Vector3.zero;

    private int firstText = 0;


    private bool writing = false;


    private Vector3 textDest;


    private bool attacksAllRow = false;

    public int EnemyIndex { get; private set; }

    private bool psiSelf = false;

    private int enemyAuxIndex = 0;

    public GameObject healPrefab;


    public OscilateOpacity playerOverlay;

    private int commandIndex = 0;

    private int ppAux = 0;

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
        //Command List iniciar
        currentCommands = new List<Command>();

        NessHP = 30;
        //Menu rotador
        rotator = menuSpinner.GetComponent<Rotate>();
        enemySelect = 0;
        text.text = "";
        isTyping = false;
        isAttacking = false;
        moveEnemyScript = moveEnemySelector.GetComponent<MoveToEnemy>();
        moveDialogue = dialogue.GetComponent<MoveDialogue>();
        currentState = STATE.PlayerTurn;
        currentPlayerState = PlayerSTATE.Idle;
        once = false;
        EnemyIndex = 0;
        //Menu de cuadrov vida posicion y
        //Menu de opciones de ataque posicion y
        menuDisplacement = new Vector3(NessMenu.transform.localPosition.x, NessMenu.transform.localPosition.y + 335f, NessMenu.transform.localPosition.z);
        moveEnemySelector.SetActive(false);
        partyMemberSelector.SetActive(false);
        psiObject.SetActive(false);
        NessMenu.SetActive(true);
        itemObject.SetActive(false);

    }
    //Obtener PSI moves por categoria
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

    //Mover texto hacia arriba
    private IEnumerator MoveText()
    {
        
        Vector3 target = new Vector3(currentText.transform.localPosition.x, currentText.transform.localPosition.y + 85f, currentText.transform.localPosition.z);

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
    private IEnumerator TextScroll(string lineOfText, string thisMoveType)
    {
        while (halt)
            yield return null;

        int letter = 0;
        
        int lineLength = lineOfText.Length - 1;
        if(firstText>1)
        {
          StartCoroutine("MoveText");
        }
        switch(thisMoveType)
        {
            case "DAMAGE":
                if(currentCommands[commandIndex].Target.tag == "Player")
                {
                    hpBar.CalculateDistance(-currentCommands[commandIndex].Value);
                    StartCoroutine(cam.Shake(0.2f, 2f));
                }
                else
                {
                    currentCommands[commandIndex].Target.GetComponent<Enemy>().ActivateShake(currentCommands[commandIndex].Value);
                    currentCommands[commandIndex].Target.GetComponent<Enemy>().ReceiveDamage(currentCommands[commandIndex].Value);

                }
                break;
            case "MULTIDAMAGE":

                currentEnemies[enemyAuxIndex].ActivateShake(currentCommands[commandIndex].Value);
                currentEnemies[enemyAuxIndex].ReceiveDamage(currentCommands[commandIndex].Value);

                
                break;
                
            case "HEAL":
                hpBar.CalculateDistance(currentCommands[commandIndex].Value);
                Instantiate(healPrefab);
                currentPlayers[0].InstantiateHeal(currentCommands[commandIndex].Value);
                break;

            case "PSIHEAL":
                ppBar.CalculateDistance(ppAux);
                hpBar.CalculateDistance(currentCommands[commandIndex].Value);
                Instantiate(healPrefab);
                currentPlayers[0].InstantiateHeal(currentCommands[commandIndex].Value);

                break;

            case "SLEEP":
                currentCommands[commandIndex].Target.GetComponent<Enemy>().Status = "Asleep";
                break;
        }

        while (letter <= lineLength)
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

        text.text += "\n";

       
        if (firstText < 2)
            firstText++;

        switch (thisMoveType)
        {
            case "BASH":
                if (currentCommands[commandIndex].Target.tag == "Player")
                {
                    if(currentCommands[commandIndex].Miss)
                        StartCoroutine(TextScroll("@ " + currentCommands[commandIndex].Caster.GetComponent<Enemy>().GetName() + " misses the attack.", "MISS"));
                    else if (currentCommands[commandIndex].Dodge)
                        StartCoroutine(TextScroll("@ Ness dodges swiftly!", "MISS"));
                    else
                        StartCoroutine(TextScroll("@ Ness takes " + currentCommands[commandIndex].Value + " of damage.", "DAMAGE"));
                }
                else
                {
                    if (currentCommands[commandIndex].Miss)
                        StartCoroutine(TextScroll("@ Just missed!", "MISS"));
                    else if (currentCommands[commandIndex].Smash)
                    {
                        StartCoroutine(TextScroll("@ SMASSHH!, " + currentCommands[commandIndex].Target.GetComponent<Enemy>().GetName() + " takes " + currentCommands[commandIndex].Value + " of damage.", "DAMAGE"));
                    }
                    else if (currentCommands[commandIndex].Dodge)
                        StartCoroutine(TextScroll("@ " + currentCommands[commandIndex].Target.GetComponent<Enemy>().GetName() + " dodges the attack.", "MISS"));
                    else
                    {
                        StartCoroutine(TextScroll("@ " + currentCommands[commandIndex].Target.GetComponent<Enemy>().GetName() + " takes " + currentCommands[commandIndex].Value + " of damage.", "DAMAGE"));
                    }


                }
                break;

            case "SingleTargetHeal":
                if (currentCommands[commandIndex].Caster.tag == "Player")
                {
                    StartCoroutine(TextScroll("@ Ness recuperates " + currentCommands[commandIndex].Value + " of health.", "HEAL"));
                }
   
                break;
            case "SingleTargetHealPSI":
                if (currentCommands[commandIndex].Caster.tag == "Player")
                {
                    StartCoroutine(TextScroll("@ Ness recuperates " + currentCommands[commandIndex].Value + " of health.", "PSIHEAL"));
                }

                break;

            case "SingleTargetPSI":
                if (currentCommands[commandIndex].Target.tag == "Player")
                {

                    StartCoroutine(TextScroll("@ Ness takes " + currentCommands[commandIndex].Value + " of damage.", "DAMAGE"));
                }
                else
                {

                    Instantiate(currentCommands[commandIndex].Animation);
                    halt = true;
                    while(halt)
                    {
                        yield return null;
                    }
                    ppBar.CalculateDistance(ppAux);
                    StartCoroutine(TextScroll("@ " + currentCommands[commandIndex].Target.GetComponent<Enemy>().GetName() + " takes " + currentCommands[commandIndex].Value + " of damage.", "DAMAGE"));
                }
                break;

            case "SingleTargetStatus":
                if (currentCommands[commandIndex].Target.tag == "Player")
                {

                    StartCoroutine(TextScroll("@ Ness takes " + currentCommands[commandIndex].Value + " of damage.", "DAMAGE"));
                }
                else
                {

                    Instantiate(currentCommands[commandIndex].Animation);
                    halt = true;
                    while (halt)
                    {
                        yield return null;
                    }
                    ppBar.CalculateDistance(ppAux);
                    if(currentCommands[commandIndex].Target.GetComponent<Enemy>().Status == "Asleep")
                        StartCoroutine(TextScroll("@ " + currentCommands[commandIndex].Target.GetComponent<Enemy>().GetName() + " is already asleep.", "SLEEP"));
                    else if(Random.Range(0,100) > currentCommands[commandIndex].Target.GetComponent<Enemy>().GetHypnosis())
                        StartCoroutine(TextScroll("@ The PSI attack fails.", "NormalMessage"));
                    else
                        StartCoroutine(TextScroll("@ " + currentCommands[commandIndex].Target.GetComponent<Enemy>().GetName() + " falls asleep", "SLEEP"));
                }
                break;

            case "MultipleTargetPSI":
                if (currentCommands[commandIndex].Target.tag == "Player")
                {
                    StartCoroutine(TextScroll("@ Ness takes " + currentCommands[commandIndex].Value + " of damage.", "DAMAGE"));
                }
                else
                {

                    Instantiate(currentCommands[commandIndex].Animation);
                    halt = true;
                    while (halt)
                    {
                        yield return null;
                    }
                    ppBar.CalculateDistance(ppAux);

                    StartCoroutine(TextScroll("@ " + currentEnemies[enemyAuxIndex].GetName() + " takes " + currentCommands[commandIndex].Value + " of damage.", "MULTIDAMAGE"));
                    StartCoroutine(Halt(0.1f));

                }
                break;

            case "DAMAGE":

                if (commandIndex < currentCommands.Count)
                {
                    if (currentCommands[commandIndex].Caster.tag == "Player")
                    {
                        Enemy enemy = currentCommands[commandIndex].Target.GetComponent<Enemy>();
                        if (enemy.GetHealth() <= 0)
                        {
                            StartCoroutine(TextScroll("@ " + enemy.GetName() + " has been defeated.", "DieMessage"));

                        }else if(enemy.Status == "Asleep" && Random.Range(1,2) == 1)
                        {
                            enemy.Status = "Idle";
                            currentCommands.RemoveAll(p => p.Caster == currentCommands[commandIndex].Target);


                            StartCoroutine(TextScroll("@ " + enemy.GetName() + " wakes up from the hit.", "NormalMessage"));
                        }
                        else
                        {
                            commandIndex++;
                            writing = false;
                        }
                    }
                    else
                    {

                        currentCommands[commandIndex].Caster.GetComponent<Enemy>().AttackingPosition();
                        commandIndex++;
                        writing = false;
                        StartCoroutine(Halt(0.1f));

                    }
                }

                break;

            case "Defend":
            case "HEAL":
            case "PSIHEAL":
                StartCoroutine(Halt(0.1f));
                commandIndex++;
                writing = false;
                break;


            case "MULTIDAMAGE":


                if (currentCommands[commandIndex].Caster.tag == "Player")
                {

                    if (currentEnemies[enemyAuxIndex].GetHealth() <= 0)
                    {
                        StartCoroutine(TextScroll("@ " + currentEnemies[enemyAuxIndex].GetName() + " has been defeated.", "MultiDieMessage"));

                    }
                    else
                    {
                        enemyAuxIndex++;
                        if(enemyAuxIndex < currentEnemies.Count)
                        StartCoroutine(TextScroll("@ " + currentEnemies[enemyAuxIndex].GetName() + " takes " + currentCommands[commandIndex].Value + " of damage.", "MULTIDAMAGE"));
                        else
                        {
                            StartCoroutine(Halt(0.1f));
                            enemyAuxIndex = 0;
                            commandIndex++;
                            writing = false;
                        }
                    }
                }

                break;
            case "MultiDieMessage":
                currentEnemies[enemyAuxIndex].StartCoroutine("Die");

                while (halt)
                    yield return null;
                if (enemyAuxIndex < currentEnemies.Count)
                {
                    StartCoroutine(TextScroll("@ " + currentEnemies[enemyAuxIndex].GetName() + " takes " + currentCommands[commandIndex].Value + " of damage.", "MULTIDAMAGE"));
                }
                else
                {
                    StartCoroutine(Halt(0.1f));
                    enemyAuxIndex = 0;
                    commandIndex++;
                    writing = false;
                }
                break;

            case "MISS":
                if(currentCommands[commandIndex].Caster.gameObject.tag == "Enemy")
                currentCommands[commandIndex].Caster.GetComponent<Enemy>().AttackingPosition();
                StartCoroutine(Halt(0.1f));

                commandIndex++;
                writing = false;
                break;
            case "NormalMessage":
            case "SLEEP":

                commandIndex++;
                writing = false;
                StartCoroutine(Halt(0.1f));

                break;
                
            case "DieMessage":

                currentCommands[commandIndex].Target.GetComponent<Enemy>().StartCoroutine("Die");
                commandIndex++;
                writing = false;
                break;
        }

    }

    public static bool V3Equal(Vector3 a, Vector3 b)
    {
        return Vector3.SqrMagnitude(a - b) < 0.05;
    }
   
    //Mover posicion de enemigos
    private IEnumerator ShiftEnemiesPosition()
    {

        Vector3 destination = new Vector3(0, 17, 60);
        currentEnemies[0].SetDestination(destination);
        switch (currentEnemies.Count)
        {
            case 1:
            while (currentEnemies.Count > 0 && !V3Equal(currentEnemies[0].transform.position, destination))
            {

                yield return null;

            }
            break;

        }
        currentEnemies[0].transform.position = destination;
        currentEnemies[0].SetDestination(destination);
        halt = false;


        
        

    }

    //Quitar enemigo de la lista
    public void RemoveEnemy(Enemy en)
    {
        halt = true;
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

    //Obtener halt
    public bool GetHalt()
    {
        return halt;
    }


    //Sortear comandos por speed
    public void SortCommands()
    {
        currentCommands = new List<Command>(currentCommands.OrderBy((c) => c.Speed));
    }
    // Update is called once per frame
    void Update()
    {

        if (halt)
            return;

        //Gano
        if (currentEnemies.Count == 0 && !writing && currentState != STATE.Won)
            currentState = STATE.Won;
        //Perdio
        else if (hpBar.GetDead() && !writing && currentState != STATE.Lose)
            currentState = STATE.Lose;

        if(currentCommands.Count > commandIndex && currentCommands[commandIndex].Caster != null)
        {
            if(currentCommands[commandIndex].Caster.tag == "Player")
            {
                currentPlayers[0].MoveBorder(true);

            }
            else
            {
                currentPlayers[0].MoveBorder(false);

            }
        }
        else
        {

            if (currentState == STATE.PlayerTurn || currentState == STATE.Won)
            {
                currentPlayers[0].MoveBorder(true);
            }
            else
            {
                currentPlayers[0].MoveBorder(false);

            }
        }
        //Mover Menu rotador
        if (currentPlayerState == PlayerSTATE.Idle && !writing && currentState == STATE.PlayerTurn)
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
        switch (currentState)
        {
            case STATE.Commands:

                if(commandIndex >= currentCommands.Count)
                {

                    currentState = STATE.PlayerTurn;
                    currentCommands.Clear();
                    commandIndex = 0;
                    enemyAuxIndex = 0;
                    moveDialogue.SetDestination(new Vector3(0, 150, 0));
                    hpBar.SetAmp(1);

                    foreach (Player player in currentPlayers)
                        player.Defending = true;

                    return;
                }

                if (!writing)
                {
                    if (currentCommands[commandIndex].Caster == null)
                    {
                        if (commandIndex < currentCommands.Count)
                        {
                            commandIndex++;

                            return;
                        }

                        else
                        {
                            currentState = STATE.PlayerTurn;
                            currentCommands.Clear();
                            commandIndex = 0;
                            ppAux = 0;
                            moveDialogue.SetDestination(new Vector3(0, 150, 0));
                            return;
                        }
                    
                    }

                    if (currentCommands[commandIndex].Caster.tag == "Enemy")
                    {
                        if (currentCommands[commandIndex].Caster.GetComponent<Enemy>().Status == "Asleep" && currentCommands[commandIndex].MyType != "NormalMessage")
                        {
                            commandIndex++;
                            return;
                            }
                        if (currentCommands[commandIndex].MyType != "NormalMessage")
                            currentCommands[commandIndex].Caster.GetComponent<Enemy>().AttackingPosition();
                    }

                    writing = true;
                    StartCoroutine(TextScroll(currentCommands[commandIndex].Message,currentCommands[commandIndex].MyType));

                }
                break;
            case STATE.PlayerTurn:
            

                switch (currentPlayerState)
                {
                    case PlayerSTATE.SelectingEnemyPSI:
                        if (Input.GetKeyDown(KeyCode.Space))
                        {
                            currentPlayerState = PlayerSTATE.Idle;
                            currentEnemies[enemySelect].ResetColor();
                            moveEnemySelector.SetActive(false);




                            damage = psiSelector.getPSIDamage();

                            attacksAllRow = psiSelector.getPSIAllRow();
                            if (attacksAllRow)
                            {
                                currentCommands.Add(new Command("@ Ness uses " + psiSelector.getPSIName(), 2, currentPlayers[0].gameObject, currentEnemies[enemySelect].gameObject, damage, 1, psiSelector.getPSIAnimation()));
                            }
                            else
                            {
                                if(psiSelector.getPSIStatus() == "Sleep")
                                    currentCommands.Add(new Command("@ Ness uses " + psiSelector.getPSIName(), 7, currentPlayers[0].gameObject, currentEnemies[enemySelect].gameObject, damage, 1, psiSelector.getPSIAnimation(),"SLEEP"));
                                else
                                    currentCommands.Add(new Command("@ Ness uses " + psiSelector.getPSIName(), 1, currentPlayers[0].gameObject, currentEnemies[enemySelect].gameObject, damage, 1, psiSelector.getPSIAnimation()));

                            }

                            psiSelector.Deactivate();
                            currentPlayerState = PlayerSTATE.TurnOver;



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
                        if (Input.GetKeyDown(KeyCode.Space) && psiSelector.getInPosition() && !psiSelector.getChoosingCat() && psiSelector.CanAfford())
                        {
                            psiSelf = !psiSelector.getPSIOffensive();
                            ppAux = -psiSelector.getPSICost();

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

                            currentCommands.Add(new Command("@ Ness bashes the enemy.", 0, currentPlayers[0].gameObject, currentEnemies[enemySelect].gameObject, damage, 1,null));

                            currentPlayerState = PlayerSTATE.TurnOver;


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
                        if (Input.GetKeyDown(KeyCode.Space))
                        {
                            itemSelector.MoveMenu();
                            partyMemberSelector.SetActive(true);
                            currentPlayerState = PlayerSTATE.ChoosingPartyMemberItem;
                            playerOverlay.SetSelected(true);
                        }
                        break;
                    case PlayerSTATE.ChoosingPartyMemberItem:
                        if (Input.GetKeyDown(KeyCode.Escape))
                        {
                            itemSelector.MoveMenu();
                            partyMemberSelector.SetActive(false);
                            playerOverlay.SetSelected(false);
                            currentPlayerState = PlayerSTATE.ChoosingItem;
                        }
                        if (Input.GetKeyDown(KeyCode.Space))
                        {
                            int index = itemSelector.getItemIndex();


                            currentCommands.Add(new Command("@ Ness eats a " + inventory.items[index].GetName(), 3, currentPlayers[0].gameObject, null, inventory.items[index].GetHPGain(), 1, healPrefab));

                            inventory.items.RemoveAt(index);
                            itemSelector.Deactivate();
                            partyMemberSelector.SetActive(false);
                            playerOverlay.SetSelected(false);
                            currentPlayerState = PlayerSTATE.TurnOver;


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

                            damage = psiSelector.getPSIDamage();
                            currentCommands.Add(new Command("@ Ness uses " + psiSelector.getPSIName(), 5, currentPlayers[0].gameObject, null, damage, 3 ,healPrefab));
                            psiSelector.Deactivate();
                            partyMemberSelector.SetActive(false);
                            playerOverlay.SetSelected(false);
                            currentPlayerState = PlayerSTATE.TurnOver;

                        }
                        break;

                    case PlayerSTATE.TurnOver:

                        text.text = "";
                        text.transform.localPosition = new Vector3(0f, -11f, 0f);
                        firstText = 0;
                        damage = 0;
                        enemySelect = 0;
                        attacksAllRow = false;
                        EnemyIndex = 0;
                        currentPlayerState = PlayerSTATE.Idle;
                        psiObject.SetActive(false);
                        itemObject.SetActive(false);
                        
                        currentState = STATE.EnemyTurn;
                        break;


                }
                //Opcion Menu Seleccionar
                if (Input.GetKeyDown("space") && !isAttacking && rotator.GetCanMove() && !writing)
                {
                    switch (rotator.num)
                    {
                        //BASH
                        case 1:
                            currentPlayerState = PlayerSTATE.SelectingEnemyBash;
                            moveEnemySelector.SetActive(true);
                            moveEnemyScript.SetName(currentEnemies[enemySelect].GetName());
                            moveEnemyScript.SetPosition(new Vector3(currentEnemies[enemySelect].transform.position.x, currentEnemies[enemySelect].transform.position.y + currentEnemies[enemySelect].GetHeight(), currentEnemies[enemySelect].transform.position.z));

                            moveEnemyScript.SetDestination(new Vector3(currentEnemies[enemySelect].transform.position.x, currentEnemies[enemySelect].transform.position.y + currentEnemies[enemySelect].GetHeight(), currentEnemies[enemySelect].transform.position.z));
                            currentEnemies[enemySelect].SelectColor();

                            break;

                        //PSI
                        case 2:
                            psiObject.SetActive(true);
                            currentPlayerState = PlayerSTATE.ChoosingPSIMove;

                            psiSelector.MoveMenu();

                            break;

                            //INVENTORY
                        case 3:
                            if (inventory.items.Count != 0)
                            {
                                currentCommands.Add(new Command("@ Ness guards agains the enemy.", 9, currentPlayers[0].gameObject, null, 0, -1, null));
                                currentPlayers[0].Defending = true;
                                hpBar.SetAmp(2);
                                currentPlayerState = PlayerSTATE.TurnOver;
                            }
                            break;

                            //INVENTORY
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


                break;

            case STATE.EnemyTurn:

                foreach (Enemy enemy in currentEnemies)
                {
                    if (enemy.Status == "Asleep")
                    {
                        if(Random.Range(1,4) == 1)
                        {
                            enemy.Status = "Idle";
                            currentCommands.Add(new Command("@ " + enemy.GetName() + " wakes up.", -1, enemy.gameObject, null,0,-1,null));
                            MovesData move = enemy.ChooseAttack();
                            currentCommands.Add(new Command("@ " + enemy.GetName() + move.moveMessage, 0, enemy.gameObject, currentPlayers[0].gameObject, move.moveDamage, 1, null));

                        }
                        else
                        currentCommands.Add(new Command("@ " + enemy.GetName() + " is asleep.", -1, enemy.gameObject, null, 0, 1, null));
                    }
                    else
                    {
                        MovesData move = enemy.ChooseAttack();
                        currentCommands.Add(new Command("@ " + enemy.GetName() + move.moveMessage, 0, enemy.gameObject, currentPlayers[0].gameObject, move.moveDamage, 1, null));
                    }
                }

                //Move dialogue box
                dialogue.SetActive(true);
                moveDialogue.SetDestination(new Vector3(0, 85, 0));

                SortCommands();

                currentState = STATE.Commands;

                break;

            case STATE.Won:

                if (!once && !isTyping)
                {
                    once = true;
                    dialogue.SetActive(true);
                    moveDialogue.SetDestination(new Vector3(0, 85, 0));
                   // StartCoroutine(TextScroll("@ Ness wins!"));
                }
                break;

            case STATE.Lose:

                if (!once)
                {
                    once = true;
                    isTyping = true;
                    dialogue.SetActive(true);
                    moveDialogue.SetDestination(new Vector3(0, 85, 0));
                  //  StartCoroutine(TextScroll("@ Enemies wins!"));
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
            case STATE.Commands:
                GUI.Label(new Rect(10, 300, 100, 20), "COMMANDS");
                break;

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
                break;
        }
    }

}