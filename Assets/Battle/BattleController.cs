using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class BattleController : MonoBehaviour
{

    public static BattleController instance;
    #region States

    //Possible battle states
    private enum BattleState{
        PlayerTurn,
        EnemyTurn,
        Commands,
        Won,
        Lose,
        Flee
    }

    //Possible player states
    private enum PlayerMenuState
    {
        Idle,
        ChoosingSpecialMove,
        SelectingEnemyBash,
        SelectingEnemySpecial,
        ChoosingItem,
        ChoosingPartyMemberItem,
        ChoosingPartyMemberPsi,
        TurnOver
    }
    #endregion

    public enum TypeOfCommand
    {
        Message,
        Melee,
        Special,
        Item,
        Die,
        Multitarget,
        END
    }
    
    public enum MoveTarget
    {
        Self,
        OpponentSingleTarget,
        AllySingleTarget,
        OpponentRow,
        Allies,
        Opponents,
        All
    }


    [FormerlySerializedAs("levelChanger")] public ChangeScene sceneTransition;

    private BattleState currentBattleState;
    private PlayerMenuState currentPlayerMenuState;

    private List<Command> currentCommands;
    
    [FormerlySerializedAs("text")] public TMP_Text dialogText;
    private bool dialogIsTyping;
    private bool isAttacking;
    private bool once;

    private int playerIndex;
    private int enemySelectIndex;

    public List<Enemy> currentEnemies;
    public List<Player> currentPlayers;
    
    public Vector3 menuDisplacement;
    public GameObject dialogue;
    public GameObject menuSpinner;
    private Rotate menuRotator;
    public GameObject menu;
    private Vector3 _velocity = Vector3.zero;
    public GameObject moveEnemySelector;
    private MoveToEnemy enemySelector;
    private MoveDialogue moveTextBox;
    public SelectPSI psiSelector;
    public ItemSelectorManager itemSelector;
    public GameObject psiObject;

    private GameObject textBoxPosition;
    public GameObject itemObject;
    private bool PAUSE;
    
    public GameObject partyMemberSelector;
    public GameObject currentText;
    private Vector3 _textVelocity = Vector3.zero;

    private int textLines;


    private bool writingText;


    private Vector3 textBoxInnerPosition;
    
    private bool CastingSpecialOnSelf;

    [FormerlySerializedAs("healPrefab")] public GameObject HealEffectPrefab;

    
    private int commandIndex;
    
    private int partySelectorIndex;

    private int partyMultiTargetIndex = 0;

    private bool reviving;
    
    private Stack<ItemData> usedItem;
    
    private bool[] playerUsedItem = new bool[4];
    private int[] usedItemIndex = new int[4];

    public GameObject enemyPrefab;
    
    public EnemyData enemyTest1;
    
    public EnemyData enemyTest2;
    
    #region Initiation
    void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        
        PAUSE = true;
        SetupEnemies();
        PAUSE = false;
    }

    private void SetupEnemies()
    {
        currentEnemies = new List<Enemy>();
        var numberOfEnemies = Random.Range(1, 3);
        GameObject EnemyObject;
        Enemy newEnemy;
        
        for (int i = 0; i < numberOfEnemies; i++)
        {
            EnemyObject = Instantiate(enemyPrefab);
            newEnemy = EnemyObject.GetComponent<Enemy>();
            newEnemy.SetEnemyData((Random.Range(0,2) == 0 ? enemyTest1 : enemyTest2));
            currentEnemies.Add(newEnemy);
        }

        //TODD Set positions
        switch (currentEnemies.Count)
        {
            case 1:
                currentEnemies[0].SetDestination(new Vector3(0f, 17f, 90f));
                currentEnemies[0].gameObject.transform.position = new Vector3(0f, 17f, 90f);
                break;
            
            case 2:
                currentEnemies[0].SetDestination(new Vector3(-60f, 17f, 90f));
                currentEnemies[0].gameObject.transform.position = new Vector3(-60f, 17f, 90f);
                currentEnemies[1].SetDestination(new Vector3(60f, 17f, 90f));
                currentEnemies[1].gameObject.transform.position = new Vector3(60f, 17f, 90f);

                break;
            case 3:
                currentEnemies[0].SetDestination(new Vector3(-60f, 17f, 90f));
                currentEnemies[0].gameObject.transform.position = new Vector3(-60f, 17f, 90f);
                currentEnemies[1].SetDestination(new Vector3(60f, 17f, 90f));
                currentEnemies[1].gameObject.transform.position = new Vector3(60f, 17f, 90f);
                currentEnemies[2].SetDestination(new Vector3(0f, 17f, 90f));
                currentEnemies[2].gameObject.transform.position = new Vector3(0f, 17f, 90f);
                break;
        }

        currentEnemies = new List<Enemy>(currentEnemies.OrderBy((c) => c.transform.position.x));

    }
    
    
    //Inciar
    void Start()
    {
        AudioManager.instance.Play("Battle");
        currentCommands = new List<Command>();
        menuRotator = menuSpinner.GetComponent<Rotate>();
        enemySelectIndex = 0;
        dialogText.text = "";
        dialogIsTyping = false;
        isAttacking = false;
        enemySelector = moveEnemySelector.GetComponent<MoveToEnemy>();
        moveTextBox = dialogue.GetComponent<MoveDialogue>();
        currentBattleState = BattleState.PlayerTurn;
        currentPlayerMenuState = PlayerMenuState.Idle;
        once = false;
        playerIndex = 0;

        var localPosition = menu.transform.localPosition;
        menuDisplacement = new Vector3(localPosition.x, localPosition.y + 335f, localPosition.z);
        moveEnemySelector.SetActive(false);
        partyMemberSelector.SetActive(false);
        psiObject.SetActive(false);
        menu.SetActive(true);
        itemObject.SetActive(false);
        usedItem = new Stack<ItemData>();
        currentPlayers[0].ShiftToAttackPosition(true);
    }
    #endregion
    
    //Mover texto hacia arriba
    private IEnumerator MoveText()
    {
        var localPosition = currentText.transform.localPosition;
        var target = new Vector3(localPosition.x, localPosition.y + 85f, localPosition.z);

        while (!V3Equal(currentText.transform.localPosition,target))
        {
            if (textLines <= 1)
                break;
            currentText.transform.localPosition = Vector3.SmoothDamp(currentText.transform.localPosition, target, ref _textVelocity, 0.1f);
            yield return null;
        }

        if (textLines <= 1)
        {
            textLines++;
        }
        else
        {
            currentText.transform.localPosition = target;

        }
    }


    //Text Box controller 
    #region TextDialogue
    private IEnumerator TextScroll(string lineOfText)
    {
        //Wait till halt is true
        while (PAUSE)
            yield return null;
        
        //Letter position and line length
        var letter = 0;
        var lineLength = lineOfText.Length - 1;


        //Move inner text of dialogue box up.
        if(textLines>1)
        {
          StartCoroutine(nameof(MoveText));
        }
        
        //Type letter by letter
        while (letter <= lineLength)
        {
            if (lineOfText[letter] == '¬')
            {
                dialogText.text += "\n";
                letter++;
            }
            else
            {
                dialogText.text += lineOfText[letter];
                letter++;
            }
            //Speed of text writing
            yield return new WaitForSeconds(0.030f);
        }
        //Skip line once message line is finished
        dialogText.text += "\n";
        
        if (textLines < 2)
            textLines++;
        
        
        //Decide next step
        StartCoroutine(BattleAction());
        

    }
    #endregion


    private IEnumerator BattleAction()
    { 
        PsiData psiMove;
        if (currentCommands.Count == 0)
            yield break;
        Command cmd = currentCommands[commandIndex];
        switch (cmd.GetTypeOfCommand)
        {
            //Display regular message
            case TypeOfCommand.Message:
                if (cmd.Caster != null && cmd.Target != null)
                {
                    if (cmd.Caster is Player &&
                        cmd.Target.GetHealth() <= 0)
                    {
                        cmd.SetTypeOfCommand(TypeOfCommand.Die);
                        
                        StartCoroutine(TextScroll(
                            "@ " + cmd.Target.GetName + " has been defeated."));
                    }
                    else
                    {
                        NextCommand();
                    }
                }
                else
                {
                    NextCommand();
                }

                break;
            
            case TypeOfCommand.Die:
                cmd.Target.DeathSequence();
                while(PAUSE)
                    yield return null;
                if(cmd.GetMoveTarget == MoveTarget.Self || cmd.GetMoveTarget == MoveTarget.OpponentSingleTarget)
                    NextCommand();
                else
                {
                    if (cmd.GetOriginalTypeOfCommand == TypeOfCommand.Special)
                        goto case TypeOfCommand.Special;
                    
                    if (cmd.GetOriginalTypeOfCommand == TypeOfCommand.Item)
                        goto case TypeOfCommand.Item;

                    if (cmd.GetOriginalTypeOfCommand == TypeOfCommand.Multitarget)
                    {
                        cmd.SetTypeOfCommand(TypeOfCommand.Multitarget);
                        goto case TypeOfCommand.Multitarget;
                    }
                }
                break;
            
            //Melee damage
            case TypeOfCommand.Bash:
                cmd.SetTypeOfCommand(TypeOfCommand.Message);
                if(cmd.Miss)
                    StartCoroutine(TextScroll("@ " + cmd.Caster.GetName + " misses the attack."));
                else if (cmd.Smash)
                {
                    cmd.Target.ReceiveDamage(-cmd.Value);
                    StartCoroutine(TextScroll(
                        "@ SMASSHH!, " + cmd.Target.GetName + " takes " +
                        cmd.Value + " of damage."));
                }
                else if (cmd.Dodge)
                    StartCoroutine(TextScroll("@ " + cmd.Target.GetName + " dodges swiftly!"));
                else
                {
                    //Normal Damage
                    if (cmd.Target is Player && ((Player) cmd.Target).isMortalDamage(-cmd.Value))
                    {
                        cmd.Target.ReceiveDamage(-cmd.Value);
                        StartCoroutine(TextScroll(
                            "@ " + cmd.Target.GetName + " takes " +
                            cmd.Value + " of mortal damage."));
                    }
                    else
                    {
                        cmd.Target.ReceiveDamage(-cmd.Value);
                        StartCoroutine(TextScroll(
                            "@ " + cmd.Target.GetName + " takes " +
                            cmd.Value + " of damage."));
                    }
                }

                break;
            
            //PSI
            case TypeOfCommand.Special:
                psiMove = cmd.PSI;

                if (cmd.Animation != null)
                {
                    if (cmd.GetMoveTarget == MoveTarget.OpponentSingleTarget)
                    {
                        GameObject psiAnim = Instantiate(cmd.Animation);
                        psiAnim.transform.position = cmd.Target.gameObject.transform.position;
                    }
                    else
                        Instantiate(cmd.Animation);
                    PAUSE = true;
                    while (PAUSE)
                    {
                        yield return null;
                    }
                }
                //Reduce PP
                cmd.Caster.ChangePP(-psiMove.GetPSIMoveCost);

                
                //TODO Cast to ALL
                if (cmd.GetMoveTarget == MoveTarget.Opponents)
                {
                    cmd.SetTypeOfCommand(TypeOfCommand.Multitarget);
                    
                    if(cmd.Caster is Player)
                        StartCoroutine(TextScroll(psiMove.ApplyEffect(currentEnemies[partyMultiTargetIndex],cmd.Value)));
                    else if(cmd.Caster is Enemy)
                        StartCoroutine(TextScroll(psiMove.ApplyEffect(currentPlayers[partyMultiTargetIndex],cmd.Value)));
                    else
                        print("Error, caster null");
                }
                else if(cmd.GetMoveTarget == MoveTarget.Allies)
                {
                    cmd.SetTypeOfCommand(TypeOfCommand.Multitarget);
                    if(cmd.Caster is Player)
                        StartCoroutine(TextScroll(psiMove.ApplyEffect(currentPlayers[partyMultiTargetIndex],cmd.Value)));
                    else if(cmd.Caster is Enemy)
                        StartCoroutine(TextScroll(psiMove.ApplyEffect(currentEnemies[partyMultiTargetIndex],cmd.Value)));
                    else
                        print("Error, caster null");
                }
                else if(cmd.GetMoveTarget == MoveTarget.Self || cmd.GetMoveTarget == MoveTarget.OpponentSingleTarget)
                {
                    cmd.SetTypeOfCommand(TypeOfCommand.Message);
                    StartCoroutine(TextScroll(psiMove.ApplyEffect(cmd.Target,cmd.Value)));
                }
                
                break;
            case TypeOfCommand.Multitarget:
                if (partyMultiTargetIndex >= 0 && cmd.Caster is Player && cmd.GetMoveTarget == MoveTarget.Opponents &&
                    currentEnemies[partyMultiTargetIndex].GetHealth() <= 0)
                {
                    partyMultiTargetIndex--;
                    cmd.SetOriginalTypeOfCommand(TypeOfCommand.Multitarget);
                    cmd.SetTypeOfCommand(TypeOfCommand.Die);
                    StartCoroutine(TextScroll(
                        "@ " + cmd.Target.GetName + " has been defeated."));
                }
                else
                {
                    
                    partyMultiTargetIndex++;
                    
                    psiMove = cmd.PSI;
                    if ((cmd.Caster is Enemy && cmd.GetMoveTarget == MoveTarget.Opponents) || (cmd.Caster is Player && cmd.GetMoveTarget == MoveTarget.Allies))
                    {
                        while (partyMultiTargetIndex < currentPlayers.Count && currentPlayers[partyMultiTargetIndex].Dead)
                            partyMultiTargetIndex++;
                        if (partyMultiTargetIndex >= currentPlayers.Count)
                        {
                            NextCommand();
                        }
                        else
                        {
                            cmd.Target = currentPlayers[partyMultiTargetIndex];

                            StartCoroutine(TextScroll(
                                psiMove.ApplyEffect(cmd.Target)));
                        }


                    }else if((cmd.Caster is Enemy && cmd.GetMoveTarget == MoveTarget.Allies) || (cmd.Caster is Player && cmd.GetMoveTarget == MoveTarget.Opponents))
                    {
                        if (partyMultiTargetIndex >= currentEnemies.Count)
                        {
                            NextCommand();
                        }
                        else
                        {
                            cmd.Target = currentEnemies[partyMultiTargetIndex];

                            StartCoroutine(TextScroll(
                                psiMove.ApplyEffect(cmd.Target)));
                        }
                    }else
                    {
                        print("Error, >:(");
                    }

                }

                break;
            
            case TypeOfCommand.Item:
                ItemData item = cmd.Item;

                if (currentCommands[commandIndex].Target is Enemy)
                {
                    //TODO add enemy item usage
                }
                else if(currentCommands[commandIndex].Target is Player)
                {
                    cmd.SetTypeOfCommand(TypeOfCommand.Message);
                    //TODO Item effect on all party?
                    StartCoroutine(TextScroll(item.ApplyEffect(cmd.Target)));

                }

                break;

        }
    }
    #region Functions

    //Obtener PSI moves por categoria
    public List<PsiData> GetMovesByCategory(string category)
    {
        var temp = new List<PsiData>();
        var moves = currentPlayers[playerIndex].psiMoves;
        foreach (var t in moves)
        {
            if (t.GetPSIType == category)
            {
                temp.Add(t);
            }
        }
        return temp;
    }
    private void NextCommand()
    {
        if (currentCommands[commandIndex].Caster is Player || currentCommands[commandIndex].Caster is Enemy)
        {
            currentCommands[commandIndex].Caster.ShiftToAttackPosition(false);
        }

        partyMultiTargetIndex = 0;
        commandIndex++;
        writingText = false;

    }

    public void InsertCommandMessage(string msg)
    {
        
        if (commandIndex >= currentCommands.Count - 1)
            currentCommands.Add(new Command(msg));
        else
            currentCommands.Insert(commandIndex + 1, new Command(msg));
        
        print("New command added");
    }

    public void RemoveCommandsBy(BattleEntity entity)
    {
        for (var i = 0; i < currentCommands.Count; i++)
        {
            //Remove commands associated with dead player
            if (currentCommands[i].Caster == entity || currentCommands[i].Target == entity)
            {
                
                currentCommands.RemoveAt(i);
                commandIndex--;
            }


        }
    }

    private static bool V3Equal(Vector3 a, Vector3 b)
    {
        return Vector3.SqrMagnitude(a - b) < 0.05;
    }

    private void ResetUsedItemArray()
    {
        for(int i=0;i<4;i++)
        {
            playerUsedItem[i] = false;
            usedItemIndex[i] = 0;
        }
    }
    //Mover posicion de enemigos
    private IEnumerator ShiftEnemiesPosition()
    {

        Vector3 destination = new Vector3(0, 17, 90);
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
        PAUSE = false;

    }

    //Quitar enemigo de la lista
    public void RemoveEnemy(Enemy en)
    {
        PAUSE = true;
        currentEnemies.Remove(en);

        if (currentEnemies.Count != 0)
        {
            StartCoroutine(nameof(ShiftEnemiesPosition));

        }
        else
        {
            PAUSE = false;
        }
    }
    

    //Sortear comandos por speed
    private void SortCommands()
    {
        currentCommands = new List<Command>(currentCommands.OrderBy((c) => c.Speed));
    }

    #endregion
    // Update is called once per frame
    void Update()
    {

        if (PAUSE)
            return;

        //Gano
        if (currentEnemies.Count == 0 && !writingText && currentBattleState != BattleState.Won)
        {
            currentCommands.Clear();
            currentBattleState = BattleState.Won;
            AudioManager.instance.StopPlaying("Battle");
            AudioManager.instance.Play("YouWin");
            StartCoroutine(TextScroll("@ You won!"));

        }
        //Perdio
        else if (!writingText && currentBattleState != BattleState.Lose)
        {
            var flag = true;
            foreach (var player in currentPlayers)
            {
                if (!player.Dead)
                    flag = false;
            }
            if(flag)
                currentBattleState = BattleState.Lose;
        }

        //Mover Menu rotador
        if (currentPlayerMenuState == PlayerMenuState.Idle && !writingText && currentBattleState == BattleState.PlayerTurn)
        {
            menu.transform.localPosition = Vector3.SmoothDamp(menu.transform.localPosition, menuDisplacement, ref _velocity, 0.15f);
            if (menu.transform.localPosition.y >= menuDisplacement.y - 30f)
                menuRotator.SetCanMove(true);
        }
        else
        {
            menu.transform.localPosition = Vector3.SmoothDamp(menu.transform.localPosition, new Vector3(menuDisplacement.x, -335.0f, menuDisplacement.z), ref _velocity, 0.2f);
            menuRotator.SetCanMove(false);
        }
        switch (currentBattleState)
        {
            case BattleState.Commands:

                if(commandIndex >= currentCommands.Count)
                {

                    currentBattleState = BattleState.PlayerTurn;
                    currentCommands.Clear();
                    commandIndex = 0;
                    playerIndex = 0;
                    while(currentPlayers[playerIndex].Dead)
                        playerIndex++;
                    currentPlayers[playerIndex].ShiftToAttackPosition(true);
                    moveTextBox.SetDestination(new Vector3(0, 150, 0));
                    ResetUsedItemArray();
                    foreach (var player in currentPlayers)
                        player.HPBar.SetAmp(1);


                    return;
                }

                if (!writingText)
                {

                    
                    //Skip if caster is null and restart to player turn if every command has been played excluding normal messages.
                    if (currentCommands[commandIndex].MyType != "NormalMessage" && currentCommands[commandIndex].Caster == null)
                    {
                        if (commandIndex < currentCommands.Count)
                        {
                            commandIndex++;

                            return;
                        }

                        currentBattleState = BattleState.PlayerTurn;
                        currentPlayers[0].ShiftToAttackPosition(true);
                        playerIndex = 0;

                        currentCommands.Clear();
                        commandIndex = 0;
                        moveTextBox.SetDestination(new Vector3(0, 150, 0));
                        return;

                    }
                    
                    //Enemy command
                    if (currentCommands[commandIndex].Caster is Enemy)
                    {
                        //Skip if sleep status
                        if (currentCommands[commandIndex].Caster.Status == "Sleep" &&
                            currentCommands[commandIndex].MyType != "NormalMessage")
                        {
                            commandIndex++;
                            return;
                        }
                        
                        if (currentCommands[commandIndex].MyType != "NormalMessage")
                        {
                            currentCommands[commandIndex].Caster.ShiftToAttackPosition(true);
                            //Enemy select new target after player death.
                            if (currentCommands[commandIndex].Target.Dead)
                            {
                                var alivePlayers = new List<Player>();

                                foreach (var player in currentPlayers)
                                {
                                    if (!player.Dead)
                                        alivePlayers.Add(player);
                                }

                                currentCommands[commandIndex].Target =
                                    alivePlayers[Random.Range(0, (alivePlayers.Count - 1))];
                                
                            }
                        }
                        
                        AudioManager.instance.Play("EnemyTurn");

                    }
                    
                    //Player command
                    if (currentCommands[commandIndex].Caster is Player)
                    {
                        //Skip if player died
                        if (currentCommands[commandIndex].Caster.Dead)
                        {
                            commandIndex++;
                            return;
                        }

                        if (currentCommands[commandIndex].MyType != "NormalMessage")
                        {
                            currentCommands[commandIndex].Caster.ShiftToAttackPosition(true);
                            //Player select new target after enemy death.
                            if (currentCommands[commandIndex].Target == null)
                            {
                                var aliveEnemies = new List<Enemy>();

                                foreach (var enemy in currentEnemies)
                                {
                                    if (!enemy.Dead)
                                        aliveEnemies.Add(enemy);
                                }

                                currentCommands[commandIndex].Target =
                                    aliveEnemies[Random.Range(0, (aliveEnemies.Count - 1))];
                                
                            }

                        }

                        if (currentCommands[commandIndex].GetTypeOfCommand == TypeOfCommand.Special)
                        {
                            AudioManager.instance.Play("PsiCast");

                        }else
                            AudioManager.instance.Play("IsAttacking");


                    }
                    
                    //Start text box writing
                    writingText = true;
                    StartCoroutine(TextScroll(currentCommands[commandIndex].Message));

                }
                break;
            #region PlayerMenuSelection
            case BattleState.PlayerTurn:
                
                //Player dead change to next player
                if (playerIndex < currentPlayers.Count && currentPlayers[playerIndex].Dead)
                {
                    currentPlayerMenuState = PlayerMenuState.TurnOver;
                }

                switch (currentPlayerMenuState)
                {
                    case PlayerMenuState.Idle:
                        
                        //Return to previous player
                        if(Input.GetKeyDown(KeyCode.Escape) && playerIndex > 0)
                        {
                            var aux = playerIndex - 1;
                            while (aux >= 0 && currentPlayers[aux].Dead)
                            {
                                aux--;
                                if (aux < 0)
                                    break;
                            }
                            if (aux < 0)
                                return;
                            currentPlayers[playerIndex].ShiftToAttackPosition(false);
                            currentCommands.RemoveAt(currentCommands.Count - 1);
                            if (playerUsedItem[aux] && usedItem.Count > 0)
                                currentPlayers[aux].myInventory.items.Insert(usedItemIndex[aux],usedItem.Pop());
                            
                            playerIndex = aux;
                            currentPlayers[playerIndex].ShiftToAttackPosition(true);

                        }
                        break;
                    case PlayerMenuState.SelectingEnemySpecial:
                        if (Input.GetKeyDown(KeyCode.Space))
                        {
                            currentPlayerMenuState = PlayerMenuState.Idle;
                            currentEnemies[enemySelectIndex].ResetColor();
                            moveEnemySelector.SetActive(false);
                            if (psiSelector.GetPSIMove.GetMoveTarget == PsiData.Direction.Opponents)
                            {
                                foreach (var enemy in currentEnemies)
                                    enemy.ResetColor();
                                currentCommands.Add(new Command(psiSelector.GetPSIMove, currentPlayers[playerIndex]));

                            }
                            else
                            {
                                currentEnemies[enemySelectIndex].ResetColor();
                                currentCommands.Add(new Command(psiSelector.GetPSIMove, currentPlayers[playerIndex], currentEnemies[enemySelectIndex]));

                            }
                            
                            psiSelector.Deactivate();
                            currentPlayerMenuState = PlayerMenuState.TurnOver;
                            
                        }
                        if (Input.GetKeyDown(KeyCode.A) && !writingText && enemySelectIndex > 0 && psiSelector.GetPSIMove.GetMoveTarget != PsiData.Direction.Opponents)
                        {
                            AudioManager.instance.Play("LeftMenu");

                            currentEnemies[enemySelectIndex].ResetColor();
                            enemySelectIndex--;
                            currentEnemies[enemySelectIndex].SelectColor();
                            enemySelector.SetName(currentEnemies[enemySelectIndex].GetName);
                            enemySelector.SetDestination(new Vector3(currentEnemies[enemySelectIndex].transform.position.x, currentEnemies[enemySelectIndex].transform.position.y + currentEnemies[enemySelectIndex].GetHeight(), currentEnemies[enemySelectIndex].transform.position.z));

                        }
                        if (Input.GetKeyDown(KeyCode.D) && !writingText && enemySelectIndex < currentEnemies.Count - 1 && psiSelector.GetPSIMove.GetMoveTarget != PsiData.Direction.Opponents)
                        {
                            AudioManager.instance.Play("RightMenu");

                            currentEnemies[enemySelectIndex].ResetColor();
                            enemySelectIndex++;
                            currentEnemies[enemySelectIndex].SelectColor();
                            enemySelector.SetName(currentEnemies[enemySelectIndex].GetName);
                            enemySelector.SetDestination(new Vector3(currentEnemies[enemySelectIndex].transform.position.x, currentEnemies[enemySelectIndex].transform.position.y + currentEnemies[enemySelectIndex].GetHeight(), currentEnemies[enemySelectIndex].transform.position.z));
                        }
                        if (Input.GetKeyDown(KeyCode.Escape))
                        {
                            if (psiSelector.GetPSIMove.GetMoveTarget == PsiData.Direction.Opponents)
                            {
                                foreach (var enemy in currentEnemies)
                                    enemy.ResetColor();
                            }else
                                currentEnemies[enemySelectIndex].ResetColor();
                            moveEnemySelector.SetActive(false);
                            currentPlayerMenuState = PlayerMenuState.ChoosingSpecialMove;

                            psiSelector.MoveMenu();
                        }
                        break;
                    case PlayerMenuState.ChoosingSpecialMove:
                        if (Input.GetKeyDown(KeyCode.Space) && psiSelector.getInPosition() && !psiSelector.getChoosingCat() && psiSelector.CanAfford())
                        {
                            AudioManager.instance.Play("Click");

                            switch (psiSelector.GetPSIMove.GetMoveTarget)
                            {
                                case PsiData.Direction.Allies:
                                {
                                    currentPlayerMenuState = PlayerMenuState.ChoosingPartyMemberPsi;

                                    partyMemberSelector.SetActive(true);
                                    partyMemberSelector.GetComponent<MoveToPartyMember>().SetPosition(new Vector3(
                                        (currentPlayers[0].gameObject.transform.position.x + currentPlayers[currentPlayers.Count - 1].gameObject.transform.position.x) / 2f,
                                        currentPlayers[partySelectorIndex].gameObject.transform.position.y,
                                        currentPlayers[partySelectorIndex].gameObject.transform.position.z));
                                    partyMemberSelector.GetComponent<MoveToPartyMember>().SetDestination(new Vector3(
                                        currentPlayers[0].gameObject.transform.position.x + currentPlayers[currentPlayers.Count - 1].gameObject.transform.position.x,
                                        currentPlayers[partySelectorIndex].gameObject.transform.position.y,
                                        currentPlayers[partySelectorIndex].gameObject.transform.position.z));
                                    foreach (var player in currentPlayers)
                                    {
                                        if(player.Dead)
                                            continue;
                                        player.GetOscilate.SetSelected(true);
                                        player.ShiftToAttackPosition(true);
                                    }

                                    break;
                                }

                                case PsiData.Direction.AllyTarget:
                                    currentPlayerMenuState = PlayerMenuState.ChoosingPartyMemberPsi;

                                    partyMemberSelector.SetActive(true);
                                
                                    partySelectorIndex = playerIndex;

                                    partyMemberSelector.GetComponent<MoveToPartyMember>().SetPosition(new Vector3(
                                        currentPlayers[partySelectorIndex].gameObject.transform.position.x,
                                        currentPlayers[partySelectorIndex].gameObject.transform.position.y,
                                        currentPlayers[partySelectorIndex].gameObject.transform.position.z));
                                    partyMemberSelector.GetComponent<MoveToPartyMember>().SetDestination(new Vector3(
                                        currentPlayers[partySelectorIndex].gameObject.transform.position.x,
                                        currentPlayers[partySelectorIndex].gameObject.transform.position.y,
                                        currentPlayers[partySelectorIndex].gameObject.transform.position.z));
                                    currentPlayers[playerIndex].GetOscilate.SetSelected(true);
                                    break;
                                
                                case PsiData.Direction.Target:
                                    currentPlayerMenuState = PlayerMenuState.SelectingEnemySpecial;
                                    moveEnemySelector.SetActive(true);
                                    
                                    enemySelector.SetName(currentEnemies[enemySelectIndex].GetName);
                                    enemySelector.SetPosition(new Vector3(
                                        currentEnemies[enemySelectIndex].transform.position.x,
                                        currentEnemies[enemySelectIndex].transform.position.y +
                                        currentEnemies[enemySelectIndex].GetHeight(),
                                        currentEnemies[enemySelectIndex].transform.position.z));

                                    enemySelector.SetDestination(new Vector3(
                                        currentEnemies[enemySelectIndex].transform.position.x,
                                        currentEnemies[enemySelectIndex].transform.position.y +
                                        currentEnemies[enemySelectIndex].GetHeight(),
                                        currentEnemies[enemySelectIndex].transform.position.z));

                                    currentEnemies[enemySelectIndex].SelectColor();
                                    break;
                                
                                case PsiData.Direction.Opponents:
                                    currentPlayerMenuState = PlayerMenuState.SelectingEnemySpecial;
                                    moveEnemySelector.SetActive(true);
                                    
                                    enemySelector.SetName("All");
                                    enemySelector.SetPosition(new Vector3(
                                        (currentEnemies[0].transform.position.x + currentEnemies[currentEnemies.Count - 1].transform.position.x) / 2f ,
                                        currentEnemies[enemySelectIndex].transform.position.y +
                                        currentEnemies[enemySelectIndex].GetHeight(),
                                        currentEnemies[enemySelectIndex].transform.position.z));
                                    enemySelector.SetDestination(new Vector3(
                                        (currentEnemies[0].transform.position.x + currentEnemies[currentEnemies.Count - 1].transform.position.x) / 2f ,
                                        currentEnemies[enemySelectIndex].transform.position.y +
                                        currentEnemies[enemySelectIndex].GetHeight(),
                                        currentEnemies[enemySelectIndex].transform.position.z));

                                    foreach(var enemy in currentEnemies)
                                        enemy.SelectColor();

                                    break;
                            }


                            psiSelector.MoveMenu();

                        }
                        if (Input.GetKeyDown(KeyCode.Escape) && psiSelector.getChoosingCat())
                        {
                            psiSelector.MoveMenu();
                            currentPlayers[playerIndex].GetOscilate.SetSelected(false);

                            currentPlayerMenuState = PlayerMenuState.Idle;
                        }
                        break;

                    case PlayerMenuState.SelectingEnemyBash:
                        if (Input.GetKeyDown(KeyCode.A) && !writingText && enemySelectIndex > 0)
                        {
                            AudioManager.instance.Play("LeftMenu");

                            currentEnemies[enemySelectIndex].ResetColor();
                            enemySelectIndex--;
                            currentEnemies[enemySelectIndex].SelectColor();
                            enemySelector.SetName(currentEnemies[enemySelectIndex].GetName);
                            enemySelector.SetDestination(new Vector3(currentEnemies[enemySelectIndex].transform.position.x, currentEnemies[enemySelectIndex].transform.position.y + currentEnemies[enemySelectIndex].GetHeight(), currentEnemies[enemySelectIndex].transform.position.z));

                        }
                        if (Input.GetKeyDown(KeyCode.D) && !writingText && enemySelectIndex < currentEnemies.Count - 1)
                        {
                            AudioManager.instance.Play("RightMenu");

                            currentEnemies[enemySelectIndex].ResetColor();
                            enemySelectIndex++;
                            currentEnemies[enemySelectIndex].SelectColor();
                            enemySelector.SetName(currentEnemies[enemySelectIndex].GetName);
                            enemySelector.SetDestination(new Vector3(currentEnemies[enemySelectIndex].transform.position.x, currentEnemies[enemySelectIndex].transform.position.y + currentEnemies[enemySelectIndex].GetHeight(), currentEnemies[enemySelectIndex].transform.position.z));

                        }
                        if (Input.GetKeyDown(KeyCode.Space) && !writingText)
                        {
                            AudioManager.instance.Play("Click");

                            currentEnemies[enemySelectIndex].ResetColor();
                            moveEnemySelector.SetActive(false);

                            currentCommands.Add(new Command("@ " + currentPlayers[playerIndex].GetName + " bashes the enemy.", currentPlayers[playerIndex], currentEnemies[enemySelectIndex]));

                            currentPlayerMenuState = PlayerMenuState.TurnOver;


                        }
                        if (Input.GetKeyDown(KeyCode.Escape))
                        {
                            currentEnemies[enemySelectIndex].ResetColor();
                            moveEnemySelector.SetActive(false);
                            currentPlayerMenuState = PlayerMenuState.Idle;
                        }
                        break;

                    case PlayerMenuState.ChoosingItem:
                        if (Input.GetKeyDown(KeyCode.Escape))
                        {
                            itemSelector.MoveMenu();
                            currentPlayerMenuState = PlayerMenuState.Idle;
                        }
                        //Go to player selection
                        if (Input.GetKeyDown(KeyCode.Space))
                        {
                            AudioManager.instance.Play("Click");

                            if (currentPlayers[playerIndex].myInventory.items[itemSelector.getItemIndex()].GetEffect() == "Revive")
                            {
                                var deadPlayers = new List<Player>();
                                var indexAux = 0;
                                foreach (var player in currentPlayers)
                                {
                                    if (!player.Dead)
                                        continue;
                                    
                                    deadPlayers.Add(player);
                                }
                               
                                if (deadPlayers.Count == 0)
                                    return;
                                while (!currentPlayers[indexAux].Dead)
                                    indexAux++;
                                partySelectorIndex = indexAux;
                                reviving = true;
                            }
                            else
                                partySelectorIndex = playerIndex;
                            itemSelector.MoveMenu();

                            partyMemberSelector.SetActive(true);
                            partyMemberSelector.GetComponent<MoveToPartyMember>().SetPosition(new Vector3(currentPlayers[partySelectorIndex].gameObject.transform.position.x, currentPlayers[partySelectorIndex].gameObject.transform.position.y, currentPlayers[partySelectorIndex].gameObject.transform.position.z));
                            partyMemberSelector.GetComponent<MoveToPartyMember>().SetDestination(new Vector3(currentPlayers[partySelectorIndex].gameObject.transform.position.x, currentPlayers[partySelectorIndex].gameObject.transform.position.y, currentPlayers[partySelectorIndex].gameObject.transform.position.z));
                            currentPlayers[partySelectorIndex].GetOscilate.SetSelected(true);
    
                            currentPlayerMenuState = PlayerMenuState.ChoosingPartyMemberItem;
                        }
                        break;
                    //Choosing party member to give item
                    case PlayerMenuState.ChoosingPartyMemberItem:
                        if (Input.GetKeyDown(KeyCode.Escape))
                        {
                            itemSelector.MoveMenu();
                            partyMemberSelector.SetActive(false);
                            if(partySelectorIndex!=playerIndex)
                                currentPlayers[partySelectorIndex].ShiftToAttackPosition(false);
                            currentPlayers[playerIndex].ShiftToAttackPosition(true);
                            currentPlayers[partySelectorIndex].GetOscilate.SetSelected(false);
                            currentPlayers[playerIndex].GetOscilate.SetSelected(false);
                            reviving = false;

                            currentPlayerMenuState = PlayerMenuState.ChoosingItem;

                        }
                        if (Input.GetKeyDown(KeyCode.Space))
                        {
                            AudioManager.instance.Play("Click");

                            if(!reviving && currentPlayers[partySelectorIndex].Dead)
                                return;
                            var index = itemSelector.getItemIndex();

                            currentCommands.Add(new Command(currentPlayers[playerIndex].myInventory.items[index], currentPlayers[playerIndex],currentPlayers[partySelectorIndex]));

                            playerUsedItem[playerIndex] = true;
                            usedItemIndex[playerIndex] = index;
                            usedItem.Push(currentPlayers[playerIndex].myInventory.items[index]);

                            currentPlayers[playerIndex].myInventory.items.RemoveAt(index);
                            itemSelector.Deactivate();
                            partyMemberSelector.SetActive(false);
                            currentPlayers[partySelectorIndex].ShiftToAttackPosition(false);
                            currentPlayers[playerIndex].ShiftToAttackPosition(false);
                            currentPlayers[playerIndex].GetOscilate.SetSelected(false);
                            currentPlayers[partySelectorIndex].GetOscilate.SetSelected(false);


                            currentPlayerMenuState = PlayerMenuState.TurnOver;
                            
                        }
                        if (Input.GetKeyDown(KeyCode.A) && !writingText && partySelectorIndex > 0)
                        {
                            AudioManager.instance.Play("LeftMenu");

                            int aux;
                            if (reviving)
                            {
                                aux = partySelectorIndex - 1;
                                while (!currentPlayers[aux].Dead)
                                {
                                    aux--;
                                    if (aux < 0)
                                        break;
                                }
                            }
                            else
                            {
                                aux = partySelectorIndex - 1;
                                while (currentPlayers[aux].Dead)
                                {
                                    aux--;
                                    if (aux < 0)
                                        break;
                                }

                            }
                            if (aux < 0)
                                return;
                            currentPlayers[partySelectorIndex].ShiftToAttackPosition(false);
                            currentPlayers[partySelectorIndex].GetOscilate.SetSelected(false);
                            partySelectorIndex = aux;
                            currentPlayers[partySelectorIndex].GetOscilate.SetSelected(true);
                            currentPlayers[partySelectorIndex].ShiftToAttackPosition(true);  
                            partyMemberSelector.GetComponent<MoveToPartyMember>().SetDestination(new Vector3(currentPlayers[partySelectorIndex].gameObject.transform.position.x, currentPlayers[partySelectorIndex].gameObject.transform.position.y, currentPlayers[partySelectorIndex].gameObject.transform.position.z));

                        }
                        if (Input.GetKeyDown(KeyCode.D) && !writingText && partySelectorIndex < currentPlayers.Count - 1)
                        {
                            AudioManager.instance.Play("RightMenu");

                            int aux;
                            int size;
                            if (reviving)
                            {
                                aux = partySelectorIndex + 1; 
                                size = currentPlayers.Count;

                                while (!currentPlayers[aux].Dead)
                                {

                                    aux++;
                                    if (aux >= size)
                                        break;
                                }
                            }
                            else
                            {

                                aux = partySelectorIndex + 1;
                                size = currentPlayers.Count;

                                while (currentPlayers[aux].Dead)
                                {

                                    aux++;
                                    if (aux >= size)
                                        break;
                                }
                            }

                            if (aux >= size)
                                return;
                            currentPlayers[partySelectorIndex].ShiftToAttackPosition(false);
                            currentPlayers[partySelectorIndex].GetOscilate.SetSelected(false);
                            partySelectorIndex = aux;
                            currentPlayers[partySelectorIndex].GetOscilate.SetSelected(true);
                            currentPlayers[partySelectorIndex].ShiftToAttackPosition(true);

                            partyMemberSelector.GetComponent<MoveToPartyMember>().SetDestination(new Vector3(currentPlayers[partySelectorIndex].gameObject.transform.position.x, currentPlayers[partySelectorIndex].gameObject.transform.position.y, currentPlayers[partySelectorIndex].gameObject.transform.position.z));

                        }
                        break;
                    case PlayerMenuState.ChoosingPartyMemberPsi:
                        if (Input.GetKeyDown(KeyCode.Escape))
                        {
                            partyMemberSelector.SetActive(false);
                            psiSelector.MoveMenu();
                            if (psiSelector.GetPSIMove.GetMoveTarget == PsiData.Direction.Allies)
                            {
                                foreach (var player in currentPlayers)
                                {
                                    if(player.Dead)
                                        continue;
                                    if(player != currentPlayers[playerIndex])
                                        player.ShiftToAttackPosition(false);
                                    player.GetOscilate.SetSelected(false);
                                }
                            }
                            else
                            {
                                if (partySelectorIndex != playerIndex)
                                    currentPlayers[partySelectorIndex].ShiftToAttackPosition(false);
                                currentPlayers[playerIndex].ShiftToAttackPosition(true);
                                currentPlayers[partySelectorIndex].GetOscilate.SetSelected(false);
                                currentPlayers[playerIndex].GetOscilate.SetSelected(false);
                            }

                            currentPlayerMenuState = PlayerMenuState.ChoosingSpecialMove;

                        }
                        if (Input.GetKeyDown(KeyCode.Space))
                        {
                            AudioManager.instance.Play("Click");

                            if(currentPlayers[partySelectorIndex].Dead)
                                return;
                            if (psiSelector.GetPSIMove.GetMoveTarget == PsiData.Direction.Allies)
                            {
                                foreach (var player in currentPlayers)
                                {
                                    if(player.Dead)
                                        continue;
                                    player.ShiftToAttackPosition(false);
                                    player.GetOscilate.SetSelected(false);
                                }
                            }
                            
                            currentCommands.Add(new Command(psiSelector.GetPSIMove, currentPlayers[playerIndex],currentPlayers[partySelectorIndex]));
                            
                            
                            psiSelector.Deactivate();
                            partyMemberSelector.SetActive(false);
                            currentPlayers[partySelectorIndex].GetOscilate.SetSelected(false);
                            currentPlayers[partySelectorIndex].ShiftToAttackPosition(false);
                            currentPlayers[playerIndex].ShiftToAttackPosition(false);
                            currentPlayerMenuState = PlayerMenuState.TurnOver;

                        }
                        if (Input.GetKeyDown(KeyCode.A) && !writingText && partySelectorIndex > 0 && psiSelector.GetPSIMove.GetMoveTarget != PsiData.Direction.Allies)
                        {
                            AudioManager.instance.Play("LeftMenu");

                            var aux = partySelectorIndex - 1;
                            while (currentPlayers[aux].Dead)
                            {

                                aux--;
                                if (aux < 0)
                                    return;
                                
                            }
                                
                            currentPlayers[partySelectorIndex].ShiftToAttackPosition(false);
                            currentPlayers[partySelectorIndex].GetOscilate.SetSelected(false);
                            partySelectorIndex = aux;
                            currentPlayers[partySelectorIndex].GetOscilate.SetSelected(true);
                            currentPlayers[partySelectorIndex].ShiftToAttackPosition(true);


                            partyMemberSelector.GetComponent<MoveToPartyMember>().SetDestination(new Vector3(currentPlayers[partySelectorIndex].gameObject.transform.position.x, currentPlayers[partySelectorIndex].gameObject.transform.position.y, currentPlayers[partySelectorIndex].gameObject.transform.position.z));

                        }
                        if (Input.GetKeyDown(KeyCode.D) && !writingText && partySelectorIndex < currentPlayers.Count - 1 && psiSelector.GetPSIMove.GetMoveTarget != PsiData.Direction.Allies)
                        {
                            AudioManager.instance.Play("RightMenu");

                            var aux = partySelectorIndex + 1; 
                            var size = currentPlayers.Count;
                            while (currentPlayers[aux ].Dead)
                            {

                                aux++;
                                if (aux >= size)
                                    return;
                            }
                                
                            currentPlayers[partySelectorIndex].ShiftToAttackPosition(false);
                            currentPlayers[partySelectorIndex].GetOscilate.SetSelected(false);
                            partySelectorIndex = aux;
                            currentPlayers[partySelectorIndex].GetOscilate.SetSelected(true);
                            currentPlayers[partySelectorIndex].ShiftToAttackPosition(true);




                            partyMemberSelector.GetComponent<MoveToPartyMember>().SetDestination(new Vector3(currentPlayers[partySelectorIndex].gameObject.transform.position.x, currentPlayers[partySelectorIndex].gameObject.transform.position.y, currentPlayers[partySelectorIndex].gameObject.transform.position.z));

                        }
                        break;
            #endregion
                    
                    case PlayerMenuState.TurnOver:
                        if (playerIndex < currentPlayers.Count - 1)
                        {
                            currentPlayers[playerIndex].ShiftToAttackPosition(false);
                            playerIndex++;
                            reviving = false;
                            while (playerIndex < currentPlayers.Count && currentPlayers[playerIndex].Dead)
                            {
                                playerIndex++;
                            }
                            if (playerIndex >= currentPlayers.Count)
                            {
                                ClearPlayerTurn();
                                return;
                            }
                            
                            currentPlayers[playerIndex].ShiftToAttackPosition(true);
                            currentPlayerMenuState = PlayerMenuState.Idle;
                        }
                        else
                        {
                            ClearPlayerTurn();
                        }
                        break;


                    default:
                        throw new ArgumentOutOfRangeException();
                }
                //Opcion Menu Seleccionar
                if (Input.GetKeyDown("space") && !isAttacking && menuRotator.GetCanMove() && !writingText)
                {
                    AudioManager.instance.Play("Click");

                    switch (menuRotator.num)
                    {
                        //BASH
                        case 1:
                            currentPlayerMenuState = PlayerMenuState.SelectingEnemyBash;
                            moveEnemySelector.SetActive(true);
                            enemySelector.SetName(currentEnemies[enemySelectIndex].GetName);
                            enemySelector.SetPosition(new Vector3(currentEnemies[enemySelectIndex].transform.position.x, currentEnemies[enemySelectIndex].transform.position.y + currentEnemies[enemySelectIndex].GetHeight(), currentEnemies[enemySelectIndex].transform.position.z));

                            enemySelector.SetDestination(new Vector3(currentEnemies[enemySelectIndex].transform.position.x, currentEnemies[enemySelectIndex].transform.position.y + currentEnemies[enemySelectIndex].GetHeight(), currentEnemies[enemySelectIndex].transform.position.z));
                            currentEnemies[enemySelectIndex].SelectColor();

                            break;

                        //PSI
                        case 2:
                            psiObject.SetActive(true);
                            currentPlayerMenuState = PlayerMenuState.ChoosingSpecialMove;

                            psiSelector.MoveMenu();

                            break;

                            //DEFEND
                        case 3:

                            currentCommands.Add(new Command("@ " + currentPlayers[playerIndex].GetName + " guards agains the enemy."));
                            currentPlayers[playerIndex].Defending = true;
                            currentPlayers[playerIndex].HPBar.SetAmp(2);
                            currentPlayerMenuState = PlayerMenuState.TurnOver;
                            
                            break;
                        
                        case 4:
                            currentPlayerMenuState = PlayerMenuState.TurnOver;
                            break;
                        


                            //INVENTORY
                        case 6:
                            if (currentPlayers[playerIndex].myInventory.getItemsCount() > 0)
                            {
                                currentPlayerMenuState = PlayerMenuState.ChoosingItem;
                                itemObject.SetActive(true);
                                itemSelector.Activate();
                                itemSelector.MoveMenu();
                            }
                            break;
                    }
                }
                break;

            case BattleState.EnemyTurn:

                foreach (var enemy in currentEnemies)
                {
                    var alivePlayers = new List<Player>();

                    foreach (var player in currentPlayers)
                    {
                        if(!player.Dead)
                            alivePlayers.Add(player);
                    }
                    var ranTarget = alivePlayers[Random.Range(0, alivePlayers.Count)];

                    if (enemy.Status == "Sleep")
                    {
                        if(Random.Range(1,4) == 1)
                        {
                            enemy.Status = "Idle";
                            currentCommands.Add(new Command("@ " + enemy.GetName + " wakes up."));
                            var move = enemy.ChooseAttack();
                            currentCommands.Add(new Command("@ " + enemy.GetName + move.moveMessage,enemy, ranTarget));

                        }
                        else
                            currentCommands.Add(new Command("@ " + enemy.GetName + " is asleep."));
                    }    
                    else
                    {
                        var move = enemy.ChooseAttack();
                        currentCommands.Add(new Command("@ " + enemy.GetName + move.moveMessage, enemy, ranTarget));
                    }
                }

                //Move dialogue box
                dialogue.SetActive(true);
                moveTextBox.SetDestination(new Vector3(0, 85, 0));

                SortCommands();

                currentBattleState = BattleState.Commands;

                break;

            case BattleState.Won:

                if (Input.GetKeyDown(KeyCode.Space) && !writingText)
                {
                    sceneTransition.FadeToLevel("Overworld");
                }
                break;

            case BattleState.Lose:
                if (!once)
                {
                    once = true;
                    dialogIsTyping = true;
                    dialogue.SetActive(true);

                    moveTextBox.SetDestination(new Vector3(0, 85, 0));
                }
                break;

            case BattleState.Flee:
                Destroy(gameObject);
                break;
        }

    }

    private void ClearPlayerTurn()
    {
        dialogText.text = "";
        dialogText.transform.localPosition = new Vector3(0f, -11f, 0f);
        textLines = 0;
        enemySelectIndex = 0;
        currentPlayerMenuState = PlayerMenuState.Idle;
        psiObject.SetActive(false);
        itemObject.SetActive(false);
        if(playerIndex < currentPlayers.Count)
            currentPlayers[playerIndex].ShiftToAttackPosition(false);
        usedItem.Clear();
        reviving = false;
        currentBattleState = BattleState.EnemyTurn;
    }
    public bool GetTyping()
    {
        return dialogIsTyping;
    }

    public bool GetWriting()
    {
        return writingText;
    }
    public void SetHaltValue(bool a)
    {
        PAUSE = a;
    }

    public Player CurrentPlayerSelecting()
    {
        return currentPlayers[playerIndex];
    }

    private IEnumerator Halt(float duration)
    {
        PAUSE = true;
        float elapsed = 0.0f;
        while (elapsed < duration)
        {


            elapsed += Time.deltaTime;
            yield return null;
        }
        PAUSE = false;
    }


}