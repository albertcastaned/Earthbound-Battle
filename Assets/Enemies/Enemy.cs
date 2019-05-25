using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;


public class Enemy : MonoBehaviour
{
    private const string SHADER_COLOR_NAME = "_Color";
    private Material material;
    public float aux;
    public EnemyData enemyData;
    private string enemyName;
    private int health;
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
        health = enemyData.maxHealth;
        description = enemyData.description;
        sprite = GetComponent<SpriteRenderer>();
        sprite.sprite = enemyData.sprite;
        moves = enemyData.moves;

    }
    public void SetDest(Vector3 d)
    {
        dest = d;
    }
    public int GetHealth() => health;
    public string GetName() => enemyName;

    public MovesData ChooseAttack()
    {

        if (moves.Count == 0)
            return moves[0];
        float accumulatedPorcentage = 0f;
        float aux = Random.Range(0f, 100f);
        int index = 0;
        for (int i=0;i<moves.Count - 1;i++)
        {
            accumulatedPorcentage += moves[i].probability;
            if (accumulatedPorcentage >= aux)
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
        health -= dmg;
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
    

    // Update is called once per frame
    void Update()
    {
        if (dying)
            return;

        if(battle.GetMoving() == false && battle.GetHalt() == false)
            transform.position = Vector3.SmoothDamp(transform.position,dest,ref velocity, 0.1f);
        

        aux += 8f * Time.deltaTime;

        if (aux > 2 * Mathf.PI)
        {
            aux = 0;
        }
        

        if (selected)
            SetColor(new Color(1, 1, 1, (0.4f + Mathf.Sin(aux) / 2f)));
        else
            SetColor(new Color(1, 1, 1, -0f));

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
        GameObject prefab = Instantiate(damageGUI, new Vector3(transform.position.x + width, transform.position.y, 0f), transform.rotation) as GameObject;
        prefab.transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform, false);
        Text text = prefab.GetComponent<Text>();
        text.text = dmg.ToString();

    }       

    public float GetHeight()
    {
        return GetComponent<SpriteRenderer>().bounds.size.y;
    }
    public IEnumerator Die()
    {
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
        battle.removeEnemy(this);
        battle.SetHaltValue(false);
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