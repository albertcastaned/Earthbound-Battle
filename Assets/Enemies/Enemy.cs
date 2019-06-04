using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using TMPro;


public class Enemy : MonoBehaviour
{
    private const string SHADER_COLOR_NAME = "_Color";
    private Material material;
    public float aux;
    public EnemyData enemyData;
    private string enemyName;

    private int HP;
    private int PP;
    private int Offense;
    private int Defense;
    private int Speed;
    private int Guts;
    private int EXP;

    private int hypnosisSuccess;

    private int paralysisSuccess;

    private int flashSuccess;

    private int brainShockSuccess;

    [StringInList("Idle", "Asleep", "Frozen", "Paralysis")] public string status;

    private string description;
    private SpriteRenderer sprite;
    private List<MovesData> moves;
    public GameObject damageGUI;
    public Battle battle;
    private bool selected;
    private bool attacking = false;
    private Vector3 dest;
    private Vector3 velocity;
    private bool dying = false;

    public string Status { get => status; set => status = value; }

    void Awake()
    {
        // makes a new instance of the material for runtime changes
        material = GetComponent<SpriteRenderer>().material;
    }
    // Start is called before the first frame update
    void Start()
    {
        dest = transform.position;
        aux = 0;
        enemyName = enemyData.enemyName;
        selected = false;

        HP = enemyData.HP;
        PP = enemyData.PP;
        Offense = enemyData.Offense;
        Defense = enemyData.Defense;
        Speed = enemyData.Speed;
        Guts = enemyData.Guts;
        EXP = enemyData.EXP;

        paralysisSuccess = enemyData.ParalysisSuccess;
        hypnosisSuccess = enemyData.HypnosisSuccess;
        flashSuccess = enemyData.FlashSuccess;
        brainShockSuccess = enemyData.BrainShockSuccess;

        description = enemyData.description;


        sprite = GetComponent<SpriteRenderer>();
        sprite.sprite = enemyData.sprite;
        moves = enemyData.moves;

    }

    public int GetHealth() => HP;
    public string GetName() => enemyName;
    public int GetOffense() => Offense;
    public int GetDefense() => Defense;
    public int GetSpeed() => Speed;
    public int GetHypnosis() => hypnosisSuccess;
    public int GetParalysis() => paralysisSuccess;
    public MovesData ChooseAttack()
    {

        if (moves.Count == 0)
            return moves[0];
        float accumulatedPorcentage = 0f;
        int index = 0;
        for (int i=0;i<moves.Count - 1;i++)
        {
            accumulatedPorcentage += moves[i].probability;
            if (accumulatedPorcentage >= Random.Range(0f, 100f))
            {

                break;
            }

            index++;
        }

        return moves[index];


    }
    public Vector3 getTransform() => transform.position;

    public void ReceiveDamage(int dmg)
    {
        HP -= dmg;
    }
    public void AttackingPosition()
    {
        attacking = !attacking;
        if(attacking)
        {
            dest = new Vector3(transform.position.x,transform.position.y,20f);
        }
        else
        {
            dest = new Vector3(transform.position.x, transform.position.y, 60f);

        }
    }

    public void SetDestination(Vector3 dst)
    {
        dest = dst;
    }



    // Update is called once per frame
    void Update()
    {
        if (dying)
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
    public void ActivateShake(int dmg)
    {
        StartCoroutine(Shake(0.5f, 5f));

        float width = GetComponent<SpriteRenderer>().bounds.size.x / 2f;
        GameObject prefab = Instantiate(damageGUI, GameObject.FindGameObjectWithTag("Canvas").transform, false) as GameObject;
        prefab.transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y + GetHeight(), 30f);
        TMP_Text text = prefab.transform.GetChild(0).GetComponent<TMP_Text>();
        text.text = dmg.ToString();

    }       

    public float GetHeight()
    {
        return GetComponent<SpriteRenderer>().bounds.size.y;
    }
    public IEnumerator Die()
    {
        battle.SetHaltValue(true);
        float elapsed = 0.0f;
        dying = true;
        while (battle.GetTyping())
            yield return null;
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
    public bool getDying()
    {
        return dying;
    }
     public IEnumerator Shake(float duration, float magnitude)
    {
        Vector3 originalPosition = transform.position;

        float elapsed = 0.0f;
        while (elapsed < duration)
        {
            float x = originalPosition.x + Random.Range(-1f, 1f) * magnitude;

            transform.position = new Vector3(x, originalPosition.y, originalPosition.z);

            elapsed += Time.deltaTime/2f;
            yield return null;
        }
        transform.position = originalPosition;
    }

    private void SetColor(Color color)
    {
        material.SetColor(SHADER_COLOR_NAME, color);
    }
}