using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BattleEntity : MonoBehaviour
{

    protected BattleController battle;
    protected string myName;
    public string GetName => myName;

    protected int HP;
    protected int SP;
    protected int Offense;
    protected int Defense;
    protected int Speed;
    protected int Guts;

    protected int hypnosisSuccess;

    protected int paralysisSuccess;

    protected int flashSuccess;

    protected int brainShockSuccess;

    [StringInList("Idle", "Sleep", "Frozen", "Paralysis")]
    private string status = "Idle";
    
    public void SetStatus(string value){ status = value; }
    
    public List<PsiData> psiMoves;

    public string Status { get => status; set => status = value; }    
    
    private bool defending;
    public bool Defending { get => defending; set => defending = value; }
    
    public int GetSP => SP;
    
    protected bool isDead;

    public bool Dead => isDead;

    public int GetOffense() => Offense;
    public int GetDefense() => Defense;
    public int GetSpeed() => Speed;
    public int GetGuts() => Guts;
    public int GetHypnosis() => hypnosisSuccess;
    public int GetParalysis() => paralysisSuccess;


    public abstract void ReceiveDamage(int dmg);
    public abstract void ShiftToAttackPosition(bool value);

    public abstract void Heal(int amount);

    public abstract void ChangePP(int amount);

    public abstract void DeathSequence();

    public abstract int GetHealth();
    public abstract void Revive(int healthRestore);


}
