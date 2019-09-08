using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Enemy : BattleEntity
{
    private const string SHADER_COLOR_NAME = "_Color";
    private Material material;
    private float aux;

    [SerializeField]
    private EnemyData enemyData;

    private enum AI{
              DefaultAI, Healer
          };
    
    [SerializeField] private AI MyAI = AI.DefaultAI;
    private string description;
    private SpriteRenderer sprite;
    private List<MovesData> moves;
    public GameObject damageGUI;
    private bool selected;
    private bool attacking;
    private Vector3 dest;
    private Vector3 velocity;
    private bool dying;

    private float spriteHeight;
    private float spriteWidth;
    private static readonly int Color = Shader.PropertyToID(SHADER_COLOR_NAME);

    private bool LOADED;
    void Awake()
    {
        battle = GameObject.Find("BattleHandler").GetComponent<BattleController>();
        material = GetComponent<SpriteRenderer>().material;
    }

    public void SetEnemyData(EnemyData newData)
    {
        enemyData = newData;
        Config();
    }

    private void Config()
    {
        name = enemyData.GetEnemyName;
        dest = transform.position;
        name = enemyData.GetEnemyName;
        myName = enemyData.GetEnemyName;
        selected = false;

        HP = enemyData.GetEnemyMaxHP;
        SP = enemyData.GetEnemyMaxPP;
        Offense = enemyData.GetEnemyOffense;
        Defense = enemyData.GetEnemyDefense;
        Speed = enemyData.GetEnemySpeed;
        Guts = enemyData.GetEnemyGuts;

        paralysisSuccess = enemyData.GetEnemyParalysisSucc;
        hypnosisSuccess = enemyData.GetEnemyHypnosisSucc;
        flashSuccess = enemyData.GetEnemyFlashSucc;
        brainShockSuccess = enemyData.GetEnemyBrainshockSucc;

        description = enemyData.GetEnemyDescription;


        MyAI = (AI) enemyData.GetEnemyAI;
        sprite = GetComponent<SpriteRenderer>();
        sprite.sprite = enemyData.GetEnemySprite;

        var bounds = sprite.bounds;
        spriteWidth = bounds.size.x;
        spriteHeight = bounds.size.y;

        moves = enemyData.moves;
        LOADED = true;
    }
    
    
    
    
    #region Abstract Methods
    public override void ReceiveDamage(int dmg)
    {
        AudioManager.instance.Play("EnemyDamaged");

        HP += dmg;
        ActivateShake(dmg);
    }

    public override void ChangePP(int amount)
    {
        SP -= amount;
    }
    public override int GetHealth()
    {
        return HP;
    }

    public override void Revive(int healthRestore)
    {
        throw new System.NotImplementedException();
    }

    public override void ShiftToAttackPosition(bool value)
    {
        attacking = value;
        var position = transform.position;
        dest = attacking ? new Vector3(position.x,position.y,20f) : new Vector3(position.x, position.y, 90f);
    }
    public override void Heal(int amount)
    {
        throw new System.NotImplementedException();
    }

    public override void DeathSequence()
    {
        StartCoroutine(nameof(Die));
    }
    #endregion
    
    
    public float GetWidth() => spriteWidth;
    public float GetHeight() => spriteHeight;

    public MovesData ChooseAttack()
    {
        switch (MyAI)
        {
            case AI.DefaultAI:
                return moves[Random.Range(0, moves.Count - 1)];
            case AI.Healer:
                return moves[0];
            default:
                return moves[Random.Range(0, moves.Count - 1)];
        }
    }

    public void SetDestination(Vector3 dst)
    {
        dest = dst;
    }
    
    // Update is called once per frame
    void Update()
    {
        if (!LOADED || dying)
            return;

        transform.position = Vector3.SmoothDamp(transform.position,dest,ref velocity, 0.1f);

        if (selected)
        {
            aux += 8f * Time.deltaTime;

            if (aux > 2 * Mathf.PI)
            {
                aux = 0;
            }
            SetColor(new Color(1, 1, 1, (0.4f + Mathf.Sin(aux) / 2f)));
        }
        else
            SetColor(new Color(1, 1, 1, 0f));

    }
    public void SelectColor()
    {
        selected = true;
    }

    public void ResetColor()
    {
        selected = false;

    }
    private void ActivateShake(int dmg)
    {
        StartCoroutine(Shake(0.5f, 5f));

        var prefab = Instantiate(damageGUI, GameObject.FindGameObjectWithTag("Canvas").transform, false);
        var currentTransform = transform;
        var localPosition = currentTransform.localPosition;
        
        prefab.transform.localPosition = new Vector3(localPosition.x, localPosition.y + GetHeight(), 30f);
        var text = prefab.transform.GetChild(0).GetComponent<TMP_Text>();
        text.text = (-dmg).ToString();

    }

    private IEnumerator Die()
    {
        battle.SetHaltValue(true);
        float elapsed = 0.0f;
        dying = true;
        while (battle.GetTyping())
            yield return null;
        AudioManager.instance.Play("EnemyDed");

        while(elapsed < 1.2f)
        {
            SetColor(new Color(1, 1, 1, elapsed));
            elapsed +=  4f * Time.deltaTime;
            yield return null;

        }
        while (elapsed > -1)
        {
            SetColor(new Color(elapsed, elapsed, elapsed, 1));
            elapsed -= 4f * Time.deltaTime;
            yield return null;


        }
        dying = false;
        battle.RemoveEnemy(this);
        Destroy(gameObject);

    }

    private IEnumerator Shake(float duration, float magnitude)
    {
        var originalPosition = transform.position;

        var elapsed = 0.0f;
        while (elapsed < duration)
        {
            var x = originalPosition.x + Random.Range(-1f, 1f) * magnitude;

            transform.position = new Vector3(x, originalPosition.y, originalPosition.z);

            elapsed += Time.deltaTime/2f;
            yield return null;
        }
        transform.position = originalPosition;
    }

    private void SetColor(Color color)
    {
        material.SetColor(Color, color);
    }
}