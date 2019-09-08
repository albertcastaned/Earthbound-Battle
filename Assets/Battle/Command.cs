using UnityEngine;

public class Command
{
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
    private TypeOfCommand MyTypeOfCommand;
    public TypeOfCommand GetTypeOfCommand => MyTypeOfCommand;

    private TypeOfCommand OriginalTypeOfCommand;
    public TypeOfCommand GetOriginalTypeOfCommand => OriginalTypeOfCommand;
    public void SetTypeOfCommand(BattleController.TypeOfCommand type)
    {
        MyTypeOfCommand = type;
    }
    public void SetOriginalTypeOfCommand(TypeOfCommand type)
    {
        OriginalTypeOfCommand = type;
    }
    private BattleEntity caster;
    private BattleEntity target;
    
    private int commandValue;

    private string message;
    private int order;
    private bool miss;
    private bool dodge;
    private bool critical;

    private GameObject animation;
    private string status;

    private ItemData item;
    private PsiData psi;

    private MoveTarget ThisMoveTarget = MoveTarget.OpponentSingleTarget;
    public MoveTarget GetMoveTarget => ThisMoveTarget;

    public BattleEntity Target { get => target; set => target = value; }

    public BattleEntity Caster { get => caster; set => caster = value; }

    public int Value { get => commandValue; set => commandValue = value; }
    public string Message { get => message; set => message = value; }
    public int Order { get => order; set => order = value; }
    public string MyType { get; set; }
    public GameObject Animation { get => animation; set => animation = value; }
    public bool Miss { get => miss; set => miss = value; }
    public bool Dodge { get => dodge; set => dodge = value; }
    public bool Smash { get => critical; set => critical = value; }

    public ItemData Item { get => item; set => item = value; }

    public PsiData PSI
    {
        get => psi;
        set => psi = value;
    }
    
    //Constructor used for Messages
    public Command(string Message)
    {
        this.Message = Message;
        MyTypeOfCommand = TypeOfCommand.Message;
        OriginalTypeOfCommand = MyTypeOfCommand;
        Order = -1;
    }

    //Constructor used by PSI
    public Command(PsiData specialMove, BattleEntity Caster, BattleEntity Target = null)
    {

        Message = "@ " + Caster.GetName + " used " + psi.GetPSIMoveName;
        MyTypeOfCommand = TypeOfCommand.Special;
        OriginalTypeOfCommand = MyTypeOfCommand;
        ThisMoveTarget = (MoveTarget) psi.GetMoveTarget;
        this.Caster = Caster;
        this.Target = Target;
        PSI = specialMove;
        Value = psi.GetPSIMoveDamage();
        status = psi.GetPSIStatusEffect;
        Animation = psi.GetPSIAnimation;
        Order = 1;
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
        Order = 1;

    }

    //Melee Constructor
    public Command(string Message, BattleEntity Caster, BattleEntity Target)
    {
        MyTypeOfCommand = TypeOfCommand.Melee;
        OriginalTypeOfCommand = MyTypeOfCommand;

        this.Message = Message;
        this.Caster = Caster;
        this.Target = Target;

        var missProbability = Random.Range(1, 16);
        if (missProbability == 1)
        {
            Value = 0;
            miss = true;
            return;
        }
        
        if (Caster is Player)
        {
            var criticalMultiplier = 4;
            var gutsChance = caster.GetGuts() / 500f;
            var useGutsAsProbability = gutsChance > 1f/20f;
            
            if (useGutsAsProbability)
            {
                if(Random.Range(0,500) <= gutsChance)
                {
                    critical = true;
                    Value = criticalMultiplier * caster.GetOffense() - target.GetDefense();
                    return;
                }
            }
            else
            {
                if (Random.Range(0, 20) <= 1)
                {
                    critical = true;
                    Value = criticalMultiplier * caster.GetOffense() - target.GetDefense();
                    return;
                }
            }
            
            if (target.Status != "Asleep")
            {
                var dodgeChance = 2 * target.GetSpeed() - caster.GetSpeed();
                if (Random.Range(0, 500) <= dodgeChance)
                {
                    dodge = true;
                    commandValue = 0;
                    return;
                }
            }
            //Default melee damage
            Value = 2 * caster.GetOffense() - target.GetDefense();

        }
        //Enemy is caster
        else
        {
            var auxDodge = 2 * target.GetSpeed() - caster.GetSpeed();
            if (Random.Range(0, 500) <= auxDodge)
            {
                dodge = true;
                commandValue = 0;
                return;
            }
            Value = 2 * caster.GetOffense() - target.GetDefense();

        }

        var damagePercentageModifier = 0.25f;
        Value += Mathf.Abs((int)Mathf.Round(Value * Random.Range(-damagePercentageModifier, damagePercentageModifier)));
        Value = Mathf.Abs(Value);
    }

}
