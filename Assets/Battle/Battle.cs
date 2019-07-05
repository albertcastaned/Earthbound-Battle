using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


//Battle controller
public class Battle : MonoBehaviour
{

    #region States

    //Possible battle states
    private enum State{
        PlayerTurn,
        EnemyTurn,
        Commands,
        Won,
        Lose,
        Flee
    };

    //Possible player states
    private enum PlayerState
    {
        Idle,
        ChoosingPsiMove,
        SelectingEnemyBash,
        SelectingEnemyPsi,
        ChoosingItem,
        ChoosingPartyMemberItem,
        ChoosingPartyMemberPsi,
        TurnOver
    };
    #endregion

    private enum Direction
    {
        Self,
        Target,
        AllyTarget,
        Allies,
        Opponents,
        All
    }

    private enum TypeOfCommand
    {
        Message,
        Bash,
        PSI,
        Item,
        Die,
        Multitarget,
        END
    }
    
    #region Command
    public ChangeScene levelChanger;

    private class Command
    {
        private TypeOfCommand MyTypeOfCommand;
        public TypeOfCommand GetTypeOfCommand => MyTypeOfCommand;

        private TypeOfCommand OriginalTypeOfCommand;
        public TypeOfCommand GetOriginalTypeOfCommand => OriginalTypeOfCommand;
        public void SetTypeOfCommand(TypeOfCommand type)
        {
            MyTypeOfCommand = type;
        }
        public void SetOriginalTypeOfCommand(TypeOfCommand type)
        {
            OriginalTypeOfCommand = type;
        }
        private BattleEntity _caster;
        private BattleEntity _target;
        private int _value;

        private string _message;
        private int _speed;
        private bool _miss;
        private bool _dodge;
        private bool _smash;
        
        private GameObject _animation;
        private string _status;

        private ItemData _item;
        private PsiData _psi;
        
        private Direction myDirection = Direction.Target;
        public Direction GetDirection => myDirection;
        //Target and Caster reference
        public BattleEntity Target { get => _target; set => _target = value; }
        
        public BattleEntity Caster { get => _caster; set => _caster = value; }

        public int Value { get => _value; set => _value = value; }
        public string Message { get => _message; set => _message = value; }
        public int Speed { get => _speed; set => _speed = value; }
        public string MyType { get; set; }
        public GameObject Animation { get => _animation; set => _animation = value; }
        public bool Miss { get => _miss; set => _miss = value; }
        public bool Dodge { get => _dodge; set => _dodge = value; }
        public bool Smash { get => _smash; set => _smash = value; }
        
        public ItemData Item { get => _item; set => _item = value; }

        public PsiData PSI
        {
            get => _psi;
            set => _psi = value;
        }

        //Multiple constructors for easier instantiating
        
        //Constructor used for Messages
        public Command(string Message)
        {
            this.Message = Message;
            MyTypeOfCommand = TypeOfCommand.Message;
            OriginalTypeOfCommand = MyTypeOfCommand;
            Speed = -1;
        }
        
        //Constructor used by PSI
        public Command(PsiData psi, BattleEntity Caster, BattleEntity Target = null)
        {

            Message = "@ " + Caster.GetName + " used " + psi.GetPSIMoveName;
            MyTypeOfCommand = TypeOfCommand.PSI;
            OriginalTypeOfCommand = MyTypeOfCommand;
            myDirection = (Direction) psi.GetMoveTarget;
            this.Caster = Caster;
            this.Target = Target;
            PSI = psi;
            Value = psi.GetPSIMoveDamage();
            _status = psi.GetPSIStatusEffect;
            Animation = psi.GetPSIAnimation;
            Speed = 1;
        }

        //Constructor used by Item
        public Command(ItemData item, BattleEntity Caster, BattleEntity Target = null)
        {
            MyTypeOfCommand = TypeOfCommand.Item;
            OriginalTypeOfCommand = MyTypeOfCommand;
            this.Caster = Caster;
            this.Target = Target;
            if (Caster == Target)
                Message = "@ " + Caster.GetName + " used " + item.GetName();
            else
                Message = "@ " + Caster.GetName + " used " + item.GetName() + " on " + Target.GetName;
            Animation = null;
            
            Value = -1;
            Item = item;
            Speed = 1;

        }
        
        //Bash Constructor
        public Command(string Message, BattleEntity Caster, BattleEntity Target)
        {
            MyTypeOfCommand = TypeOfCommand.Bash;
            OriginalTypeOfCommand = MyTypeOfCommand;

            this.Message = Message;
            this.Caster = Caster;
            this.Target = Target;
            
            //Get probability of caster missing the attack
                if (Random.Range(1, 16) == 1)
                {
                    Value = 0;
                    _miss = true;
                    return;
                }
                //If the caster is a player
                if (Caster is Player)
                {
                    //Get probability of Critical attack
                    var auxiliarChance = _caster.GetGuts() / 500f;
                    if (auxiliarChance > 1f/20f)
                    {
                        if(Random.Range(0,500) <= auxiliarChance)
                        {
                            //Normal damage times 4
                            _smash = true;
                            Value = 4 * _caster.GetOffense() - _target.GetDefense();
                            return;
                        }
                    }
                    else
                    {
                        if (Random.Range(0, 20) <= 1)
                        {
                            _smash = true;
                            Value = 4 * _caster.GetOffense() - _target.GetDefense();
                            return;
                        }
                    }
                    
                    if (_target.Status != "Asleep")
                    {
                        //Get probability of target dodging the move
                        var auxDodge = 2 * _target.GetSpeed() - _caster.GetSpeed();
                        if (Random.Range(0, 500) <= auxDodge)
                        {
                            _dodge = true;
                            _value = 0;
                            return;
                        }
                    }
                    //Normal melee damage calculation
                    Value = 2 * _caster.GetOffense() - _target.GetDefense();
                    
                    if (_target.Defending)
                        Value /= 2;
                }
                //Enemy is caster
                else
                {
                    var auxDodge = 2 * _target.GetSpeed() - _caster.GetSpeed();
                    if (Random.Range(0, 500) <= auxDodge)
                    {
                        _dodge = true;
                        _value = 0;
                        return;
                    }
                    this.Value = 2 * _caster.GetOffense() - _target.GetDefense();
                    //Half damage if defending
                    if (_target.Defending)
                        Value /= 2;
                }
                Value += Mathf.Abs((int)Mathf.Round(Value * Random.Range(-0.25f, 0.25f)));
                Value = Mathf.Abs(Value);
            
        }


    }
    #endregion

    #region Variables
    private State _currentState;
    private PlayerState _currentPlayerState;

    private List<Command> _currentCommands;
    
    public TMP_Text text;
    private bool _isTyping;
    private bool _isAttacking;
    private bool _once;

    private int _playerIndex;
    private int _enemySelect;

    public List<Enemy> currentEnemies;
    public List<Player> currentPlayers;
    
    private MovesData _aux;

    public Vector3 menuDisplacement;
    public GameObject dialogue;
    public GameObject menuSpinner;
    private Rotate _rotator;
    public GameObject menu;
    private Vector3 _velocity = Vector3.zero;
    public GameObject moveEnemySelector;
    private MoveToEnemy _moveEnemyScript;
    private MoveDialogue _moveDialogue;
    public SelectPSI psiSelector;
    public ItemSelectorManager itemSelector;
    public GameObject psiObject;

    private GameObject _dialoguePos;
    public GameObject itemObject;
    private bool _halt;
    public GameObject partyMemberSelector;
    public GameObject currentText;
    private Vector3 _textVelocity = Vector3.zero;

    private int _firstText;


    private bool _writing;


    private Vector3 _textDest;
    
    private bool _psiSelf;

    public GameObject healPrefab;

    
    private int _commandIndex;
    
    private int _partySelectorIndex;

    private int _partyMultiTargetIndex = 0;

    private bool _reviving;
    
    private Stack<ItemData> _itemStack;
    
    private bool[] _usedItem = new bool[4];
    private int[] _usedItemIndex = new int[4];

    public GameObject enemyPrefab;
    
    public EnemyData enemyTest1;
    
    public EnemyData enemyTest2;
    
    #endregion
    #region Initiation
    void Awake()
    {
        _halt = true;
        SetupEnemies();
        _halt = false;
    }

    private void SetupEnemies()
    {
        currentEnemies = new List<Enemy>();
        int ran = Random.Range(1, 3);
        GameObject newEnemyObject;
        Enemy newEnemy;
        switch (ran)
        {
            case 1:
                newEnemyObject = Instantiate(enemyPrefab);
                newEnemy = newEnemyObject.GetComponent<Enemy>();
                newEnemy.SetEnemyData((Random.Range(0,2) == 0 ? enemyTest1 : enemyTest2));
                currentEnemies.Add(newEnemy);


                break;
            case 2:
                case 3:
                    
                for (int i = 0; i < ran; i++)
                {
                    newEnemyObject = Instantiate(enemyPrefab);
                    newEnemy = newEnemyObject.GetComponent<Enemy>();
                    newEnemy.SetEnemyData((Random.Range(0,2) == 0 ? enemyTest1 : enemyTest2));
                    currentEnemies.Add(newEnemy);
                }
                break;
        }

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

        //Command List iniciar
        _currentCommands = new List<Command>();
                //Menu rotador
        _rotator = menuSpinner.GetComponent<Rotate>();
        _enemySelect = 0;
        text.text = "";
        _isTyping = false;
        _isAttacking = false;
        _moveEnemyScript = moveEnemySelector.GetComponent<MoveToEnemy>();
        _moveDialogue = dialogue.GetComponent<MoveDialogue>();
        _currentState = State.PlayerTurn;
        _currentPlayerState = PlayerState.Idle;
        _once = false;
        _playerIndex = 0;

        var localPosition = menu.transform.localPosition;
        menuDisplacement = new Vector3(localPosition.x, localPosition.y + 335f, localPosition.z);
        moveEnemySelector.SetActive(false);
        partyMemberSelector.SetActive(false);
        psiObject.SetActive(false);
        menu.SetActive(true);
        itemObject.SetActive(false);
        _itemStack = new Stack<ItemData>();
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
            if (_firstText <= 1)
                break;
            currentText.transform.localPosition = Vector3.SmoothDamp(currentText.transform.localPosition, target, ref _textVelocity, 0.1f);
            yield return null;
        }

        if (_firstText <= 1)
        {
            _firstText++;
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
        while (_halt)
            yield return null;
        
        //Letter position and line length
        var letter = 0;
        var lineLength = lineOfText.Length - 1;


        //Move inner text of dialogue box up.
        if(_firstText>1)
        {
          StartCoroutine(nameof(MoveText));
        }
        
        //Type letter by letter
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
            //Speed of text writing
            yield return new WaitForSeconds(0.030f);
        }
        //Skip line once message line is finished
        text.text += "\n";
        
        if (_firstText < 2)
            _firstText++;
        
        
        //Decide next step
        StartCoroutine(BattleAction());
        

    }
    #endregion


    private IEnumerator BattleAction()
    { 
        PsiData psiMove;
        if (_currentCommands.Count == 0)
            yield break;
        Command cmd = _currentCommands[_commandIndex];
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
                while(_halt)
                    yield return null;
                if(cmd.GetDirection == Direction.Self || cmd.GetDirection == Direction.Target)
                    NextCommand();
                else
                {
                    if (cmd.GetOriginalTypeOfCommand == TypeOfCommand.PSI)
                        goto case TypeOfCommand.PSI;
                    
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
            case TypeOfCommand.PSI:
                psiMove = cmd.PSI;

                if (cmd.Animation != null)
                {
                    if (cmd.GetDirection == Direction.Target)
                    {
                        GameObject psiAnim = Instantiate(cmd.Animation);
                        psiAnim.transform.position = cmd.Target.gameObject.transform.position;
                    }
                    else
                        Instantiate(cmd.Animation);
                    _halt = true;
                    while (_halt)
                    {
                        yield return null;
                    }
                }
                //Reduce PP
                cmd.Caster.ChangePP(-psiMove.GetPSIMoveCost);

                
                //TODO Cast to ALL
                if (cmd.GetDirection == Direction.Opponents)
                {
                    cmd.SetTypeOfCommand(TypeOfCommand.Multitarget);
                    
                    if(cmd.Caster is Player)
                        StartCoroutine(TextScroll(psiMove.ApplyEffect(currentEnemies[_partyMultiTargetIndex],cmd.Value)));
                    else if(cmd.Caster is Enemy)
                        StartCoroutine(TextScroll(psiMove.ApplyEffect(currentPlayers[_partyMultiTargetIndex],cmd.Value)));
                    else
                        print("Error, caster null");
                }
                else if(cmd.GetDirection == Direction.Allies)
                {
                    cmd.SetTypeOfCommand(TypeOfCommand.Multitarget);
                    if(cmd.Caster is Player)
                        StartCoroutine(TextScroll(psiMove.ApplyEffect(currentPlayers[_partyMultiTargetIndex],cmd.Value)));
                    else if(cmd.Caster is Enemy)
                        StartCoroutine(TextScroll(psiMove.ApplyEffect(currentEnemies[_partyMultiTargetIndex],cmd.Value)));
                    else
                        print("Error, caster null");
                }
                else if(cmd.GetDirection == Direction.Self || cmd.GetDirection == Direction.Target)
                {
                    cmd.SetTypeOfCommand(TypeOfCommand.Message);
                    StartCoroutine(TextScroll(psiMove.ApplyEffect(cmd.Target,cmd.Value)));
                }
                
                break;
            case TypeOfCommand.Multitarget:
                if (_partyMultiTargetIndex >= 0 && cmd.Caster is Player && cmd.GetDirection == Direction.Opponents &&
                    currentEnemies[_partyMultiTargetIndex].GetHealth() <= 0)
                {
                    _partyMultiTargetIndex--;
                    cmd.SetOriginalTypeOfCommand(TypeOfCommand.Multitarget);
                    cmd.SetTypeOfCommand(TypeOfCommand.Die);
                    StartCoroutine(TextScroll(
                        "@ " + cmd.Target.GetName + " has been defeated."));
                }
                else
                {
                    
                    _partyMultiTargetIndex++;
                    
                    psiMove = cmd.PSI;
                    if ((cmd.Caster is Enemy && cmd.GetDirection == Direction.Opponents) || (cmd.Caster is Player && cmd.GetDirection == Direction.Allies))
                    {
                        while (_partyMultiTargetIndex < currentPlayers.Count && currentPlayers[_partyMultiTargetIndex].Dead)
                            _partyMultiTargetIndex++;
                        if (_partyMultiTargetIndex >= currentPlayers.Count)
                        {
                            NextCommand();
                        }
                        else
                        {
                            cmd.Target = currentPlayers[_partyMultiTargetIndex];

                            StartCoroutine(TextScroll(
                                psiMove.ApplyEffect(cmd.Target)));
                        }


                    }else if((cmd.Caster is Enemy && cmd.GetDirection == Direction.Allies) || (cmd.Caster is Player && cmd.GetDirection == Direction.Opponents))
                    {
                        if (_partyMultiTargetIndex >= currentEnemies.Count)
                        {
                            NextCommand();
                        }
                        else
                        {
                            cmd.Target = currentEnemies[_partyMultiTargetIndex];

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

                if (_currentCommands[_commandIndex].Target is Enemy)
                {
                    //TODO add enemy item usage
                }
                else if(_currentCommands[_commandIndex].Target is Player)
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
        var moves = currentPlayers[_playerIndex].psiMoves;
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
        if (_currentCommands[_commandIndex].Caster is Player || _currentCommands[_commandIndex].Caster is Enemy)
        {
            _currentCommands[_commandIndex].Caster.ShiftToAttackPosition(false);
        }

        _partyMultiTargetIndex = 0;
        _commandIndex++;
        _writing = false;

    }

    public void InsertCommandMessage(string msg)
    {
        
        if (_commandIndex >= _currentCommands.Count - 1)
            _currentCommands.Add(new Command(msg));
        else
            _currentCommands.Insert(_commandIndex + 1, new Command(msg));
        
        print("New command added");
    }

    public void RemoveCommandsBy(BattleEntity entity)
    {
        for (var i = 0; i < _currentCommands.Count; i++)
        {
            //Remove commands associated with dead player
            if (_currentCommands[i].Caster == entity || _currentCommands[i].Target == entity)
            {
                
                _currentCommands.RemoveAt(i);
                _commandIndex--;
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
            _usedItem[i] = false;
            _usedItemIndex[i] = 0;
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
        _halt = false;

    }

    //Quitar enemigo de la lista
    public void RemoveEnemy(Enemy en)
    {
        _halt = true;
        currentEnemies.Remove(en);

        if (currentEnemies.Count != 0)
        {
            StartCoroutine(nameof(ShiftEnemiesPosition));

        }
        else
        {
            _halt = false;
        }
    }
    

    //Sortear comandos por speed
    private void SortCommands()
    {
        _currentCommands = new List<Command>(_currentCommands.OrderBy((c) => c.Speed));
    }

    #endregion
    // Update is called once per frame
    void Update()
    {

        if (_halt)
            return;

        //Gano
        if (currentEnemies.Count == 0 && !_writing && _currentState != State.Won)
        {
            _currentCommands.Clear();
            _currentState = State.Won;
            AudioManager.instance.StopPlaying("Battle");
            AudioManager.instance.Play("YouWin");
            StartCoroutine(TextScroll("@ You won!"));

        }
        //Perdio
        else if (!_writing && _currentState != State.Lose)
        {
            var flag = true;
            foreach (var player in currentPlayers)
            {
                if (!player.Dead)
                    flag = false;
            }
            if(flag)
                _currentState = State.Lose;
        }

        //Mover Menu rotador
        if (_currentPlayerState == PlayerState.Idle && !_writing && _currentState == State.PlayerTurn)
        {
            menu.transform.localPosition = Vector3.SmoothDamp(menu.transform.localPosition, menuDisplacement, ref _velocity, 0.15f);
            if (menu.transform.localPosition.y >= menuDisplacement.y - 30f)
                _rotator.SetCanMove(true);
        }
        else
        {
            menu.transform.localPosition = Vector3.SmoothDamp(menu.transform.localPosition, new Vector3(menuDisplacement.x, -335.0f, menuDisplacement.z), ref _velocity, 0.2f);
            _rotator.SetCanMove(false);
        }
        switch (_currentState)
        {
            case State.Commands:

                if(_commandIndex >= _currentCommands.Count)
                {

                    _currentState = State.PlayerTurn;
                    _currentCommands.Clear();
                    _commandIndex = 0;
                    _playerIndex = 0;
                    while(currentPlayers[_playerIndex].Dead)
                        _playerIndex++;
                    currentPlayers[_playerIndex].ShiftToAttackPosition(true);
                    _moveDialogue.SetDestination(new Vector3(0, 150, 0));
                    ResetUsedItemArray();
                    foreach (var player in currentPlayers)
                        player.HPBar.SetAmp(1);


                    return;
                }

                if (!_writing)
                {

                    
                    //Skip if caster is null and restart to player turn if every command has been played excluding normal messages.
                    if (_currentCommands[_commandIndex].MyType != "NormalMessage" && _currentCommands[_commandIndex].Caster == null)
                    {
                        if (_commandIndex < _currentCommands.Count)
                        {
                            _commandIndex++;

                            return;
                        }

                        _currentState = State.PlayerTurn;
                        currentPlayers[0].ShiftToAttackPosition(true);
                        _playerIndex = 0;

                        _currentCommands.Clear();
                        _commandIndex = 0;
                        _moveDialogue.SetDestination(new Vector3(0, 150, 0));
                        return;

                    }
                    
                    //Enemy command
                    if (_currentCommands[_commandIndex].Caster is Enemy)
                    {
                        //Skip if sleep status
                        if (_currentCommands[_commandIndex].Caster.Status == "Sleep" &&
                            _currentCommands[_commandIndex].MyType != "NormalMessage")
                        {
                            _commandIndex++;
                            return;
                        }
                        
                        if (_currentCommands[_commandIndex].MyType != "NormalMessage")
                        {
                            _currentCommands[_commandIndex].Caster.ShiftToAttackPosition(true);
                            //Enemy select new target after player death.
                            if (_currentCommands[_commandIndex].Target.Dead)
                            {
                                var alivePlayers = new List<Player>();

                                foreach (var player in currentPlayers)
                                {
                                    if (!player.Dead)
                                        alivePlayers.Add(player);
                                }

                                _currentCommands[_commandIndex].Target =
                                    alivePlayers[Random.Range(0, (alivePlayers.Count - 1))];
                                
                            }
                        }
                        
                        AudioManager.instance.Play("EnemyTurn");

                    }
                    
                    //Player command
                    if (_currentCommands[_commandIndex].Caster is Player)
                    {
                        //Skip if player died
                        if (_currentCommands[_commandIndex].Caster.Dead)
                        {
                            _commandIndex++;
                            return;
                        }

                        if (_currentCommands[_commandIndex].MyType != "NormalMessage")
                        {
                            _currentCommands[_commandIndex].Caster.ShiftToAttackPosition(true);
                            //Player select new target after enemy death.
                            if (_currentCommands[_commandIndex].Target == null)
                            {
                                var aliveEnemies = new List<Enemy>();

                                foreach (var enemy in currentEnemies)
                                {
                                    if (!enemy.Dead)
                                        aliveEnemies.Add(enemy);
                                }

                                _currentCommands[_commandIndex].Target =
                                    aliveEnemies[Random.Range(0, (aliveEnemies.Count - 1))];
                                
                            }

                        }

                        if (_currentCommands[_commandIndex].GetTypeOfCommand == TypeOfCommand.PSI)
                        {
                            AudioManager.instance.Play("PsiCast");

                        }else
                            AudioManager.instance.Play("IsAttacking");


                    }
                    
                    //Start text box writing
                    _writing = true;
                    StartCoroutine(TextScroll(_currentCommands[_commandIndex].Message));

                }
                break;
            #region PlayerMenuSelection
            case State.PlayerTurn:
                
                //Player dead change to next player
                if (_playerIndex < currentPlayers.Count && currentPlayers[_playerIndex].Dead)
                {
                    _currentPlayerState = PlayerState.TurnOver;
                }

                switch (_currentPlayerState)
                {
                    case PlayerState.Idle:
                        
                        //Return to previous player
                        if(Input.GetKeyDown(KeyCode.Escape) && _playerIndex > 0)
                        {
                            var aux = _playerIndex - 1;
                            while (aux >= 0 && currentPlayers[aux].Dead)
                            {
                                aux--;
                                if (aux < 0)
                                    break;
                            }
                            if (aux < 0)
                                return;
                            currentPlayers[_playerIndex].ShiftToAttackPosition(false);
                            _currentCommands.RemoveAt(_currentCommands.Count - 1);
                            if (_usedItem[aux] && _itemStack.Count > 0)
                                currentPlayers[aux].myInventory.items.Insert(_usedItemIndex[aux],_itemStack.Pop());
                            
                            _playerIndex = aux;
                            currentPlayers[_playerIndex].ShiftToAttackPosition(true);

                        }
                        break;
                    case PlayerState.SelectingEnemyPsi:
                        if (Input.GetKeyDown(KeyCode.Space))
                        {
                            _currentPlayerState = PlayerState.Idle;
                            currentEnemies[_enemySelect].ResetColor();
                            moveEnemySelector.SetActive(false);
                            if (psiSelector.GetPSIMove.GetMoveTarget == PsiData.Direction.Opponents)
                            {
                                foreach (var enemy in currentEnemies)
                                    enemy.ResetColor();
                                _currentCommands.Add(new Command(psiSelector.GetPSIMove, currentPlayers[_playerIndex]));

                            }
                            else
                            {
                                currentEnemies[_enemySelect].ResetColor();
                                _currentCommands.Add(new Command(psiSelector.GetPSIMove, currentPlayers[_playerIndex], currentEnemies[_enemySelect]));

                            }
                            
                            psiSelector.Deactivate();
                            _currentPlayerState = PlayerState.TurnOver;
                            
                        }
                        if (Input.GetKeyDown(KeyCode.A) && !_writing && _enemySelect > 0 && psiSelector.GetPSIMove.GetMoveTarget != PsiData.Direction.Opponents)
                        {
                            AudioManager.instance.Play("LeftMenu");

                            currentEnemies[_enemySelect].ResetColor();
                            _enemySelect--;
                            currentEnemies[_enemySelect].SelectColor();
                            _moveEnemyScript.SetName(currentEnemies[_enemySelect].GetName);
                            _moveEnemyScript.SetDestination(new Vector3(currentEnemies[_enemySelect].transform.position.x, currentEnemies[_enemySelect].transform.position.y + currentEnemies[_enemySelect].GetHeight(), currentEnemies[_enemySelect].transform.position.z));

                        }
                        if (Input.GetKeyDown(KeyCode.D) && !_writing && _enemySelect < currentEnemies.Count - 1 && psiSelector.GetPSIMove.GetMoveTarget != PsiData.Direction.Opponents)
                        {
                            AudioManager.instance.Play("RightMenu");

                            currentEnemies[_enemySelect].ResetColor();
                            _enemySelect++;
                            currentEnemies[_enemySelect].SelectColor();
                            _moveEnemyScript.SetName(currentEnemies[_enemySelect].GetName);
                            _moveEnemyScript.SetDestination(new Vector3(currentEnemies[_enemySelect].transform.position.x, currentEnemies[_enemySelect].transform.position.y + currentEnemies[_enemySelect].GetHeight(), currentEnemies[_enemySelect].transform.position.z));
                        }
                        if (Input.GetKeyDown(KeyCode.Escape))
                        {
                            if (psiSelector.GetPSIMove.GetMoveTarget == PsiData.Direction.Opponents)
                            {
                                foreach (var enemy in currentEnemies)
                                    enemy.ResetColor();
                            }else
                                currentEnemies[_enemySelect].ResetColor();
                            moveEnemySelector.SetActive(false);
                            _currentPlayerState = PlayerState.ChoosingPsiMove;

                            psiSelector.MoveMenu();
                        }
                        break;
                    case PlayerState.ChoosingPsiMove:
                        if (Input.GetKeyDown(KeyCode.Space) && psiSelector.getInPosition() && !psiSelector.getChoosingCat() && psiSelector.CanAfford())
                        {
                            AudioManager.instance.Play("Click");

                            switch (psiSelector.GetPSIMove.GetMoveTarget)
                            {
                                case PsiData.Direction.Allies:
                                {
                                    _currentPlayerState = PlayerState.ChoosingPartyMemberPsi;

                                    partyMemberSelector.SetActive(true);
                                    partyMemberSelector.GetComponent<MoveToPartyMember>().SetPosition(new Vector3(
                                        (currentPlayers[0].gameObject.transform.position.x + currentPlayers[currentPlayers.Count - 1].gameObject.transform.position.x) / 2f,
                                        currentPlayers[_partySelectorIndex].gameObject.transform.position.y,
                                        currentPlayers[_partySelectorIndex].gameObject.transform.position.z));
                                    partyMemberSelector.GetComponent<MoveToPartyMember>().SetDestination(new Vector3(
                                        currentPlayers[0].gameObject.transform.position.x + currentPlayers[currentPlayers.Count - 1].gameObject.transform.position.x,
                                        currentPlayers[_partySelectorIndex].gameObject.transform.position.y,
                                        currentPlayers[_partySelectorIndex].gameObject.transform.position.z));
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
                                    _currentPlayerState = PlayerState.ChoosingPartyMemberPsi;

                                    partyMemberSelector.SetActive(true);
                                
                                    _partySelectorIndex = _playerIndex;

                                    partyMemberSelector.GetComponent<MoveToPartyMember>().SetPosition(new Vector3(
                                        currentPlayers[_partySelectorIndex].gameObject.transform.position.x,
                                        currentPlayers[_partySelectorIndex].gameObject.transform.position.y,
                                        currentPlayers[_partySelectorIndex].gameObject.transform.position.z));
                                    partyMemberSelector.GetComponent<MoveToPartyMember>().SetDestination(new Vector3(
                                        currentPlayers[_partySelectorIndex].gameObject.transform.position.x,
                                        currentPlayers[_partySelectorIndex].gameObject.transform.position.y,
                                        currentPlayers[_partySelectorIndex].gameObject.transform.position.z));
                                    currentPlayers[_playerIndex].GetOscilate.SetSelected(true);
                                    break;
                                
                                case PsiData.Direction.Target:
                                    _currentPlayerState = PlayerState.SelectingEnemyPsi;
                                    moveEnemySelector.SetActive(true);
                                    
                                    _moveEnemyScript.SetName(currentEnemies[_enemySelect].GetName);
                                    _moveEnemyScript.SetPosition(new Vector3(
                                        currentEnemies[_enemySelect].transform.position.x,
                                        currentEnemies[_enemySelect].transform.position.y +
                                        currentEnemies[_enemySelect].GetHeight(),
                                        currentEnemies[_enemySelect].transform.position.z));

                                    _moveEnemyScript.SetDestination(new Vector3(
                                        currentEnemies[_enemySelect].transform.position.x,
                                        currentEnemies[_enemySelect].transform.position.y +
                                        currentEnemies[_enemySelect].GetHeight(),
                                        currentEnemies[_enemySelect].transform.position.z));

                                    currentEnemies[_enemySelect].SelectColor();
                                    break;
                                
                                case PsiData.Direction.Opponents:
                                    _currentPlayerState = PlayerState.SelectingEnemyPsi;
                                    moveEnemySelector.SetActive(true);
                                    
                                    _moveEnemyScript.SetName("All");
                                    _moveEnemyScript.SetPosition(new Vector3(
                                        (currentEnemies[0].transform.position.x + currentEnemies[currentEnemies.Count - 1].transform.position.x) / 2f ,
                                        currentEnemies[_enemySelect].transform.position.y +
                                        currentEnemies[_enemySelect].GetHeight(),
                                        currentEnemies[_enemySelect].transform.position.z));
                                    _moveEnemyScript.SetDestination(new Vector3(
                                        (currentEnemies[0].transform.position.x + currentEnemies[currentEnemies.Count - 1].transform.position.x) / 2f ,
                                        currentEnemies[_enemySelect].transform.position.y +
                                        currentEnemies[_enemySelect].GetHeight(),
                                        currentEnemies[_enemySelect].transform.position.z));

                                    foreach(var enemy in currentEnemies)
                                        enemy.SelectColor();

                                    break;
                            }


                            psiSelector.MoveMenu();

                        }
                        if (Input.GetKeyDown(KeyCode.Escape) && psiSelector.getChoosingCat())
                        {
                            psiSelector.MoveMenu();
                            currentPlayers[_playerIndex].GetOscilate.SetSelected(false);

                            _currentPlayerState = PlayerState.Idle;
                        }
                        break;

                    case PlayerState.SelectingEnemyBash:
                        if (Input.GetKeyDown(KeyCode.A) && !_writing && _enemySelect > 0)
                        {
                            AudioManager.instance.Play("LeftMenu");

                            currentEnemies[_enemySelect].ResetColor();
                            _enemySelect--;
                            currentEnemies[_enemySelect].SelectColor();
                            _moveEnemyScript.SetName(currentEnemies[_enemySelect].GetName);
                            _moveEnemyScript.SetDestination(new Vector3(currentEnemies[_enemySelect].transform.position.x, currentEnemies[_enemySelect].transform.position.y + currentEnemies[_enemySelect].GetHeight(), currentEnemies[_enemySelect].transform.position.z));

                        }
                        if (Input.GetKeyDown(KeyCode.D) && !_writing && _enemySelect < currentEnemies.Count - 1)
                        {
                            AudioManager.instance.Play("RightMenu");

                            currentEnemies[_enemySelect].ResetColor();
                            _enemySelect++;
                            currentEnemies[_enemySelect].SelectColor();
                            _moveEnemyScript.SetName(currentEnemies[_enemySelect].GetName);
                            _moveEnemyScript.SetDestination(new Vector3(currentEnemies[_enemySelect].transform.position.x, currentEnemies[_enemySelect].transform.position.y + currentEnemies[_enemySelect].GetHeight(), currentEnemies[_enemySelect].transform.position.z));

                        }
                        if (Input.GetKeyDown(KeyCode.Space) && !_writing)
                        {
                            AudioManager.instance.Play("Click");

                            currentEnemies[_enemySelect].ResetColor();
                            moveEnemySelector.SetActive(false);

                            _currentCommands.Add(new Command("@ " + currentPlayers[_playerIndex].GetName + " bashes the enemy.", currentPlayers[_playerIndex], currentEnemies[_enemySelect]));

                            _currentPlayerState = PlayerState.TurnOver;


                        }
                        if (Input.GetKeyDown(KeyCode.Escape))
                        {
                            currentEnemies[_enemySelect].ResetColor();
                            moveEnemySelector.SetActive(false);
                            _currentPlayerState = PlayerState.Idle;
                        }
                        break;

                    case PlayerState.ChoosingItem:
                        if (Input.GetKeyDown(KeyCode.Escape))
                        {
                            itemSelector.MoveMenu();
                            _currentPlayerState = PlayerState.Idle;
                        }
                        //Go to player selection
                        if (Input.GetKeyDown(KeyCode.Space))
                        {
                            AudioManager.instance.Play("Click");

                            if (currentPlayers[_playerIndex].myInventory.items[itemSelector.getItemIndex()].GetEffect() == "Revive")
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
                                _partySelectorIndex = indexAux;
                                _reviving = true;
                            }
                            else
                                _partySelectorIndex = _playerIndex;
                            itemSelector.MoveMenu();

                            partyMemberSelector.SetActive(true);
                            partyMemberSelector.GetComponent<MoveToPartyMember>().SetPosition(new Vector3(currentPlayers[_partySelectorIndex].gameObject.transform.position.x, currentPlayers[_partySelectorIndex].gameObject.transform.position.y, currentPlayers[_partySelectorIndex].gameObject.transform.position.z));
                            partyMemberSelector.GetComponent<MoveToPartyMember>().SetDestination(new Vector3(currentPlayers[_partySelectorIndex].gameObject.transform.position.x, currentPlayers[_partySelectorIndex].gameObject.transform.position.y, currentPlayers[_partySelectorIndex].gameObject.transform.position.z));
                            currentPlayers[_partySelectorIndex].GetOscilate.SetSelected(true);
    
                            _currentPlayerState = PlayerState.ChoosingPartyMemberItem;
                        }
                        break;
                    //Choosing party member to give item
                    case PlayerState.ChoosingPartyMemberItem:
                        if (Input.GetKeyDown(KeyCode.Escape))
                        {
                            itemSelector.MoveMenu();
                            partyMemberSelector.SetActive(false);
                            if(_partySelectorIndex!=_playerIndex)
                                currentPlayers[_partySelectorIndex].ShiftToAttackPosition(false);
                            currentPlayers[_playerIndex].ShiftToAttackPosition(true);
                            currentPlayers[_partySelectorIndex].GetOscilate.SetSelected(false);
                            currentPlayers[_playerIndex].GetOscilate.SetSelected(false);
                            _reviving = false;

                            _currentPlayerState = PlayerState.ChoosingItem;

                        }
                        if (Input.GetKeyDown(KeyCode.Space))
                        {
                            AudioManager.instance.Play("Click");

                            if(!_reviving && currentPlayers[_partySelectorIndex].Dead)
                                return;
                            var index = itemSelector.getItemIndex();

                            _currentCommands.Add(new Command(currentPlayers[_playerIndex].myInventory.items[index], currentPlayers[_playerIndex],currentPlayers[_partySelectorIndex]));

                            _usedItem[_playerIndex] = true;
                            _usedItemIndex[_playerIndex] = index;
                            _itemStack.Push(currentPlayers[_playerIndex].myInventory.items[index]);

                            currentPlayers[_playerIndex].myInventory.items.RemoveAt(index);
                            itemSelector.Deactivate();
                            partyMemberSelector.SetActive(false);
                            currentPlayers[_partySelectorIndex].ShiftToAttackPosition(false);
                            currentPlayers[_playerIndex].ShiftToAttackPosition(false);
                            currentPlayers[_playerIndex].GetOscilate.SetSelected(false);
                            currentPlayers[_partySelectorIndex].GetOscilate.SetSelected(false);


                            _currentPlayerState = PlayerState.TurnOver;
                            
                        }
                        if (Input.GetKeyDown(KeyCode.A) && !_writing && _partySelectorIndex > 0)
                        {
                            AudioManager.instance.Play("LeftMenu");

                            int aux;
                            if (_reviving)
                            {
                                aux = _partySelectorIndex - 1;
                                while (!currentPlayers[aux].Dead)
                                {
                                    aux--;
                                    if (aux < 0)
                                        break;
                                }
                            }
                            else
                            {
                                aux = _partySelectorIndex - 1;
                                while (currentPlayers[aux].Dead)
                                {
                                    aux--;
                                    if (aux < 0)
                                        break;
                                }

                            }
                            if (aux < 0)
                                return;
                            currentPlayers[_partySelectorIndex].ShiftToAttackPosition(false);
                            currentPlayers[_partySelectorIndex].GetOscilate.SetSelected(false);
                            _partySelectorIndex = aux;
                            currentPlayers[_partySelectorIndex].GetOscilate.SetSelected(true);
                            currentPlayers[_partySelectorIndex].ShiftToAttackPosition(true);  
                            partyMemberSelector.GetComponent<MoveToPartyMember>().SetDestination(new Vector3(currentPlayers[_partySelectorIndex].gameObject.transform.position.x, currentPlayers[_partySelectorIndex].gameObject.transform.position.y, currentPlayers[_partySelectorIndex].gameObject.transform.position.z));

                        }
                        if (Input.GetKeyDown(KeyCode.D) && !_writing && _partySelectorIndex < currentPlayers.Count - 1)
                        {
                            AudioManager.instance.Play("RightMenu");

                            int aux;
                            int size;
                            if (_reviving)
                            {
                                aux = _partySelectorIndex + 1; 
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

                                aux = _partySelectorIndex + 1;
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
                            currentPlayers[_partySelectorIndex].ShiftToAttackPosition(false);
                            currentPlayers[_partySelectorIndex].GetOscilate.SetSelected(false);
                            _partySelectorIndex = aux;
                            currentPlayers[_partySelectorIndex].GetOscilate.SetSelected(true);
                            currentPlayers[_partySelectorIndex].ShiftToAttackPosition(true);

                            partyMemberSelector.GetComponent<MoveToPartyMember>().SetDestination(new Vector3(currentPlayers[_partySelectorIndex].gameObject.transform.position.x, currentPlayers[_partySelectorIndex].gameObject.transform.position.y, currentPlayers[_partySelectorIndex].gameObject.transform.position.z));

                        }
                        break;
                    case PlayerState.ChoosingPartyMemberPsi:
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
                                    if(player != currentPlayers[_playerIndex])
                                        player.ShiftToAttackPosition(false);
                                    player.GetOscilate.SetSelected(false);
                                }
                            }
                            else
                            {
                                if (_partySelectorIndex != _playerIndex)
                                    currentPlayers[_partySelectorIndex].ShiftToAttackPosition(false);
                                currentPlayers[_playerIndex].ShiftToAttackPosition(true);
                                currentPlayers[_partySelectorIndex].GetOscilate.SetSelected(false);
                                currentPlayers[_playerIndex].GetOscilate.SetSelected(false);
                            }

                            _currentPlayerState = PlayerState.ChoosingPsiMove;

                        }
                        if (Input.GetKeyDown(KeyCode.Space))
                        {
                            AudioManager.instance.Play("Click");

                            if(currentPlayers[_partySelectorIndex].Dead)
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
                            
                            _currentCommands.Add(new Command(psiSelector.GetPSIMove, currentPlayers[_playerIndex],currentPlayers[_partySelectorIndex]));
                            
                            
                            psiSelector.Deactivate();
                            partyMemberSelector.SetActive(false);
                            currentPlayers[_partySelectorIndex].GetOscilate.SetSelected(false);
                            currentPlayers[_partySelectorIndex].ShiftToAttackPosition(false);
                            currentPlayers[_playerIndex].ShiftToAttackPosition(false);
                            _currentPlayerState = PlayerState.TurnOver;

                        }
                        if (Input.GetKeyDown(KeyCode.A) && !_writing && _partySelectorIndex > 0 && psiSelector.GetPSIMove.GetMoveTarget != PsiData.Direction.Allies)
                        {
                            AudioManager.instance.Play("LeftMenu");

                            var aux = _partySelectorIndex - 1;
                            while (currentPlayers[aux].Dead)
                            {

                                aux--;
                                if (aux < 0)
                                    return;
                                
                            }
                                
                            currentPlayers[_partySelectorIndex].ShiftToAttackPosition(false);
                            currentPlayers[_partySelectorIndex].GetOscilate.SetSelected(false);
                            _partySelectorIndex = aux;
                            currentPlayers[_partySelectorIndex].GetOscilate.SetSelected(true);
                            currentPlayers[_partySelectorIndex].ShiftToAttackPosition(true);


                            partyMemberSelector.GetComponent<MoveToPartyMember>().SetDestination(new Vector3(currentPlayers[_partySelectorIndex].gameObject.transform.position.x, currentPlayers[_partySelectorIndex].gameObject.transform.position.y, currentPlayers[_partySelectorIndex].gameObject.transform.position.z));

                        }
                        if (Input.GetKeyDown(KeyCode.D) && !_writing && _partySelectorIndex < currentPlayers.Count - 1 && psiSelector.GetPSIMove.GetMoveTarget != PsiData.Direction.Allies)
                        {
                            AudioManager.instance.Play("RightMenu");

                            var aux = _partySelectorIndex + 1; 
                            var size = currentPlayers.Count;
                            while (currentPlayers[aux ].Dead)
                            {

                                aux++;
                                if (aux >= size)
                                    return;
                            }
                                
                            currentPlayers[_partySelectorIndex].ShiftToAttackPosition(false);
                            currentPlayers[_partySelectorIndex].GetOscilate.SetSelected(false);
                            _partySelectorIndex = aux;
                            currentPlayers[_partySelectorIndex].GetOscilate.SetSelected(true);
                            currentPlayers[_partySelectorIndex].ShiftToAttackPosition(true);




                            partyMemberSelector.GetComponent<MoveToPartyMember>().SetDestination(new Vector3(currentPlayers[_partySelectorIndex].gameObject.transform.position.x, currentPlayers[_partySelectorIndex].gameObject.transform.position.y, currentPlayers[_partySelectorIndex].gameObject.transform.position.z));

                        }
                        break;
            #endregion
                    
                    case PlayerState.TurnOver:
                        if (_playerIndex < currentPlayers.Count - 1)
                        {
                            currentPlayers[_playerIndex].ShiftToAttackPosition(false);
                            _playerIndex++;
                            _reviving = false;
                            while (_playerIndex < currentPlayers.Count && currentPlayers[_playerIndex].Dead)
                            {
                                _playerIndex++;
                            }
                            if (_playerIndex >= currentPlayers.Count)
                            {
                                ClearPlayerTurn();
                                return;
                            }
                            
                            currentPlayers[_playerIndex].ShiftToAttackPosition(true);
                            _currentPlayerState = PlayerState.Idle;
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
                if (Input.GetKeyDown("space") && !_isAttacking && _rotator.GetCanMove() && !_writing)
                {
                    AudioManager.instance.Play("Click");

                    switch (_rotator.num)
                    {
                        //BASH
                        case 1:
                            _currentPlayerState = PlayerState.SelectingEnemyBash;
                            moveEnemySelector.SetActive(true);
                            _moveEnemyScript.SetName(currentEnemies[_enemySelect].GetName);
                            _moveEnemyScript.SetPosition(new Vector3(currentEnemies[_enemySelect].transform.position.x, currentEnemies[_enemySelect].transform.position.y + currentEnemies[_enemySelect].GetHeight(), currentEnemies[_enemySelect].transform.position.z));

                            _moveEnemyScript.SetDestination(new Vector3(currentEnemies[_enemySelect].transform.position.x, currentEnemies[_enemySelect].transform.position.y + currentEnemies[_enemySelect].GetHeight(), currentEnemies[_enemySelect].transform.position.z));
                            currentEnemies[_enemySelect].SelectColor();

                            break;

                        //PSI
                        case 2:
                            psiObject.SetActive(true);
                            _currentPlayerState = PlayerState.ChoosingPsiMove;

                            psiSelector.MoveMenu();

                            break;

                            //DEFEND
                        case 3:

                            _currentCommands.Add(new Command("@ " + currentPlayers[_playerIndex].GetName + " guards agains the enemy."));
                            currentPlayers[_playerIndex].Defending = true;
                            currentPlayers[_playerIndex].HPBar.SetAmp(2);
                            _currentPlayerState = PlayerState.TurnOver;
                            
                            break;
                        
                        case 4:
                            _currentPlayerState = PlayerState.TurnOver;
                            break;
                        


                            //INVENTORY
                        case 6:
                            if (currentPlayers[_playerIndex].myInventory.getItemsCount() > 0)
                            {
                                _currentPlayerState = PlayerState.ChoosingItem;
                                itemObject.SetActive(true);
                                itemSelector.Activate();
                                itemSelector.MoveMenu();
                            }
                            break;
                    }
                }
                break;

            case State.EnemyTurn:

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
                            _currentCommands.Add(new Command("@ " + enemy.GetName + " wakes up."));
                            var move = enemy.ChooseAttack();
                            _currentCommands.Add(new Command("@ " + enemy.GetName + move.moveMessage,enemy, ranTarget));

                        }
                        else
                            _currentCommands.Add(new Command("@ " + enemy.GetName + " is asleep."));
                    }    
                    else
                    {
                        var move = enemy.ChooseAttack();
                        _currentCommands.Add(new Command("@ " + enemy.GetName + move.moveMessage, enemy, ranTarget));
                    }
                }

                //Move dialogue box
                dialogue.SetActive(true);
                _moveDialogue.SetDestination(new Vector3(0, 85, 0));

                SortCommands();

                _currentState = State.Commands;

                break;

            case State.Won:

                if (Input.GetKeyDown(KeyCode.Space) && !_writing)
                {
                    levelChanger.FadeToLevel("Overworld");
                }
                break;

            case State.Lose:
                if (!_once)
                {
                    _once = true;
                    _isTyping = true;
                    dialogue.SetActive(true);

                    _moveDialogue.SetDestination(new Vector3(0, 85, 0));
                }
                break;

            case State.Flee:
                Destroy(gameObject);
                break;
        }

    }

    private void ClearPlayerTurn()
    {
        text.text = "";
        text.transform.localPosition = new Vector3(0f, -11f, 0f);
        _firstText = 0;
        _enemySelect = 0;
        _currentPlayerState = PlayerState.Idle;
        psiObject.SetActive(false);
        itemObject.SetActive(false);
        if(_playerIndex < currentPlayers.Count)
            currentPlayers[_playerIndex].ShiftToAttackPosition(false);
        _itemStack.Clear();
        _reviving = false;
        _currentState = State.EnemyTurn;
    }
    public bool GetTyping()
    {
        return _isTyping;
    }

    public bool GetWriting()
    {
        return _writing;
    }
    public void SetHaltValue(bool a)
    {
        _halt = a;
    }

    public Player CurrentPlayerSelecting()
    {
        return currentPlayers[_playerIndex];
    }

    private IEnumerator Halt(float duration)
    {
        _halt = true;
        float elapsed = 0.0f;
        while (elapsed < duration)
        {


            elapsed += Time.deltaTime;
            yield return null;
        }
        _halt = false;
    }


}