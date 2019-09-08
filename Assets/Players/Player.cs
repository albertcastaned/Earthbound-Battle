using System;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Player : BattleEntity
{

    public PlayerData myPlayerData;

    public Inventory myInventory;
    
    private bool borderAttackingPosition;
    private float borderDisplacement;

    private Vector3 borderOriginalPos;
    private Vector3 portraitOriginalPos;

    private Vector3 borderVelocity = Vector3.zero;
    private Vector3 portraitVelocity = Vector3.zero;

    public GameObject healGUI;

    public GameObject psiGUI;

    public GameObject psiPrefab;
    
    private GetMeterValue hpMeter;
    public GetMeterValue getHPMeter => hpMeter;
    private GetMeterValue ppMeter;
    public GetMeterValue getPPMeter => ppMeter;


    private MeterRoll hpBar;
    public MeterRoll HPBar => hpBar;
    private MeterRoll ppBar;
    public MeterRoll PPBar => ppBar;

    private OscilateOpacity oscilateOverlay;
    public OscilateOpacity GetOscilate => oscilateOverlay;

    private ControlOpacity controlOpacity;

    private CameraShake cam;

    public GameObject healPrefab;
    
    private TileMove tileMove;

    private TextMeshProUGUI nameText;

    private GameObject border;

    private GameObject hpSprite;

    private GameObject ppSprite;

    private GameObject pattern;

    private bool revivingTimer;


    // Start is called before the first frame update
    void Awake()
    {
        myName = myPlayerData.playerName;
        HP = myPlayerData.CurrentHP;
        SP = myPlayerData.CurrentSP;
        Offense = myPlayerData.Offense;
        Defense = myPlayerData.Defense;
        Speed = myPlayerData.Speed;
        Guts = myPlayerData.Guts;

        paralysisSuccess = myPlayerData.ParalysisSuccessChance;
        hypnosisSuccess = myPlayerData.HypnosisSuccessChance;
        flashSuccess = myPlayerData.FlashSuccessChance;
        brainShockSuccess = myPlayerData.BrainShockSuccessChance;
        
        border = transform.Find("Border").gameObject;
        hpSprite = border.transform.Find("HP").gameObject;
        ppSprite = border.transform.Find("PP").gameObject;

        ppMeter = border.transform.Find("PP Meter").GetComponent<GetMeterValue>();
        hpMeter = border.transform.Find("HP Meter").GetComponent<GetMeterValue>();
        hpBar = hpMeter.transform.Find("Nums (2)").GetComponent<MeterRoll>();
        ppBar = ppMeter.transform.Find("Nums (2)").GetComponent<MeterRoll>();
        oscilateOverlay = border.transform.Find("selectOverlay").GetComponent<OscilateOpacity>();
        controlOpacity = border.transform.Find("healOverlay").GetComponent<ControlOpacity>();
        pattern = border.transform.Find("Pattern").gameObject;
        cam = GameObject.Find("CameraHolder").transform.Find("Camera").GetComponent<CameraShake>();
        nameText = border.transform.Find("Name").GetComponent<TextMeshProUGUI>();
        nameText.text = myName;
        battle = BattleController.instance;
        tileMove = border.transform.Find("Pattern").GetComponent<TileMove>();
        
    }
    
    void Start()
    {
        InitiateMeters();
        borderOriginalPos = transform.position;
    }

    private void InitiateMeters()
    {
        int og = HP;
        int num1 = og % 10;
        og /= 10;
        int num2 = og % 10;
        og /= 10;
        int num3 = og % 10;
        
        hpMeter.SetValue(num3,num2,num1);
        
        int og2 = SP;
        int ppNum1 = og2 % 10;
        og2 /= 10;
        int ppNum2 = og2 % 10;
        og2 /= 10;
        int ppNum3 = og2 % 10;
        
        ppMeter.SetValue(ppNum3,ppNum2,ppNum1);

    }

    // Update is called once per frame
    void Update()
    {
        if (!isDead && !revivingTimer)
        {
            if (!hpMeter.Rolling)
            {
                HP = GetHealthOfMeter();
                CheckDeath();
            }
        }
        transform.position = Vector3.SmoothDamp(transform.position,
            new Vector3(borderOriginalPos.x, borderOriginalPos.y + borderDisplacement, borderOriginalPos.z),
            ref borderVelocity, 0.2f);

    }
    
    //Passed as negative
    public override void ReceiveDamage(int dmg)
    {
        if (Defending)
            dmg /= 2;

        AudioManager.instance.Play("Hurt");

        hpBar.CalculateDistance(dmg);

        StartCoroutine(isMortalDamage(dmg) ? cam.Shake(3f, 4f) : cam.Shake(0.2f, 0.2f));
    }
    private int GetHealthOfMeter()
    {
        return hpMeter.Value;
    }
    
    private void CheckDeath()
    {
        if (HP > 0 || revivingTimer) return;
        DeathSequence();

    }

    public override void Revive(int healthRestore)
    {
        battle.RemoveCommandsBy(this);

        revivingTimer = true;
        Heal(healthRestore);
        isDead = false;
        tileMove.ScrollX = 0.2f;
        tileMove.ScrollY = 0.2f;
        StartCoroutine(nameof(ShiftToAliveColors));

    }

    public bool isMortalDamage(int dmg)
    {
        return (HP - dmg <= 0 || hpMeter.GetValue() - dmg <= 0);
    }
    private IEnumerator ShiftToAliveColors()
    {
        var newColor = new Color(1f, 1f, 1f, 1f);

        while (true)
        {
            nameText.color = Color.Lerp(nameText.color,newColor,0.1f);
            border.GetComponent<SpriteRenderer>().color = Color.Lerp(border.GetComponent<SpriteRenderer>().color,newColor,0.1f);
            hpSprite.GetComponent<SpriteRenderer>().color = Color.Lerp(hpSprite.GetComponent<SpriteRenderer>().color,newColor,0.1f);
            ppSprite.GetComponent<SpriteRenderer>().color = Color.Lerp(ppSprite.GetComponent<SpriteRenderer>().color,newColor,0.1f);
            ppMeter.gameObject.GetComponent<Image>().color = Color.Lerp(ppMeter.GetComponent<Image>().color,newColor,0.1f);
            hpMeter.gameObject.GetComponent<Image>().color = Color.Lerp(hpMeter.GetComponent<Image>().color,newColor,0.1f);

            pattern.GetComponent<SpriteRenderer>().material.color = Color.Lerp(pattern.GetComponent<SpriteRenderer>().material.color,newColor,0.1f);
            if (IsEqualTo(nameText.color, newColor))
            {
                revivingTimer = false;

                break;
            }

            yield return null;
        }

    }
    private IEnumerator ShiftToDeadColors()
    {
        var newColor = new Color(1f, 0f, 0.4313831f, 1f);
        var patternColor = new Color(1f, 0.3632075f, 0.4197571f, 1f);

        while (true)
        {
            if (!isDead)
                break;
            nameText.color = Color.Lerp(nameText.color,newColor,0.1f);
            border.GetComponent<SpriteRenderer>().color = Color.Lerp(border.GetComponent<SpriteRenderer>().color,newColor,0.1f);
            hpSprite.GetComponent<SpriteRenderer>().color = Color.Lerp(hpSprite.GetComponent<SpriteRenderer>().color,newColor,0.1f);
            ppSprite.GetComponent<SpriteRenderer>().color = Color.Lerp(ppSprite.GetComponent<SpriteRenderer>().color,newColor,0.1f);
            ppMeter.gameObject.GetComponent<Image>().color = Color.Lerp(ppMeter.GetComponent<Image>().color,newColor,0.1f);
            hpMeter.gameObject.GetComponent<Image>().color = Color.Lerp(hpMeter.GetComponent<Image>().color,newColor,0.1f);

            pattern.GetComponent<SpriteRenderer>().material.color = Color.Lerp(pattern.GetComponent<SpriteRenderer>().material.color,patternColor,0.1f);
            if (IsEqualTo(nameText.color, newColor))
                break;
            yield return null;
        }
    }
    
    private static bool IsEqualTo(Color me, Color other)
    {
        return Math.Abs(me.r - other.r) < 0.005f && Math.Abs(me.g - other.g) < 0.005f && Math.Abs(me.b - other.b) < 0.005f && Math.Abs(me.a - other.a) < 0.005f;
    }
    
    #region Abstract Methods

    public override int GetHealth()
    {
        return revivingTimer ? 999 : hpMeter.Value;
    }
    
    public override void ShiftToAttackPosition(bool value)
    {
        if (value == borderAttackingPosition)
            return;
        if (borderAttackingPosition)
        {
            
            borderDisplacement = 0f;
            borderAttackingPosition = false;
        }
        else
        {
            borderDisplacement = 15f;
            borderAttackingPosition = true;
        }
    }

    public override void Heal(int amount)
    {
        
        if (HP + amount > myPlayerData.MaxHP)
        {
            amount = myPlayerData.MaxHP - HP;
        }else if (hpMeter.GetValue() + amount > myPlayerData.MaxHP)
        {
            amount = myPlayerData.MaxHP - HP;
        }
        HP += amount;
        if(amount > 0)
            hpBar.CalculateDistance(amount);

        AudioManager.instance.Play("Heal");

        /*
        var canvasTransform = GameObject.FindGameObjectWithTag("Canvas").transform;
        var prefab = Instantiate(healGUI, canvasTransform, false);
        prefab.transform.localPosition =
            new Vector3(position.x, position.y + (borderAttackingPosition ? 20f : 50f), 0f);
        var text = prefab.transform.GetChild(0).GetComponent<TMP_Text>();
        text.text = "+" + amount;

        Instantiate(healPrefab, canvasTransform, false)
            .GetComponent<RotatePivot>().Setup(transform, controlOpacity);
            */
    }

    public override void ChangePP(int amount)
    {
        if (ppMeter.GetValue() + amount > myPlayerData.MaxSP)
            amount = myPlayerData.MaxSP - SP;

        if (ppMeter.GetValue() + amount <= 0)
            amount = SP;

        SP += amount;
        ppBar.CalculateDistance(amount);

        if (amount >= 0)
        {
            AudioManager.instance.Play("Heal");

            //TODO Heal effect to actual on screen character for Heal too 
            /*
            var canvasTransform = GameObject.FindGameObjectWithTag("Canvas").transform;
            var prefab = Instantiate(psiGUI, canvasTransform, false);
            prefab.transform.localPosition =
                new Vector3(position.x, position.y + (borderAttackingPosition ? 20f : 50f), 0f);
            var text = prefab.transform.GetChild(0).GetComponent<TMP_Text>();
            text.text = "+" + amount;

            Instantiate(psiPrefab, canvasTransform, false)
                .GetComponent<RotatePivot>().Setup(transform, controlOpacity);
                */
        }
    }
    public override void DeathSequence()
    {
        isDead = true;
        tileMove.ScrollX = 0;
        tileMove.ScrollY = 0;
        battle.RemoveCommandsBy(this);
        
        battle.InsertCommandMessage("@ " + myName + " has been defeated!");
        StartCoroutine(nameof(ShiftToDeadColors));
    }

    #endregion

}
