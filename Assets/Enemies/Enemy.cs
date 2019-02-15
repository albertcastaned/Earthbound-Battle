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
    private bool selected;
    void Awake()
    {
        // makes a new instance of the material for runtime changes
        material = GetComponent<SpriteRenderer>().material;
    }
    // Start is called before the first frame update
    void Start()
    {
        aux = 0;
        enemyName = enemyData.name;
        selected = false;
        health = enemyData.maxHealth;
        description = enemyData.description;
        sprite = GetComponent<SpriteRenderer>();
        sprite.sprite = enemyData.sprite;
        moves = enemyData.moves;

    }
    public int GetHealth() => health;
    public string GetName()
    {
        return enemyName;
    }
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
    
    public void ReceiveDamage(int dmg)
    {
        health -= dmg;
    }

    // Update is called once per frame
    void Update()
    {

        aux += 0.1f;
        if (aux > 6.28318530718)
            aux = 0f;
        if(selected)
            SetColor(new Color(1, 1, 1, (0.3f + Mathf.Sin(aux) / 5f)));
        else
            SetColor(new Color(1, 1, 1, 0f));
        if (health<=0)
        {
            Destroy(gameObject);
        }
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
        StartCoroutine(Shake(0.5f, 4f));
        float width = GetComponent<SpriteRenderer>().bounds.size.x / 2f;
        GameObject prefab = Instantiate(damageGUI, new Vector3(transform.position.x + width, transform.position.y, 0f), transform.rotation) as GameObject;
        prefab.transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform, false);
        Text text = prefab.GetComponent<Text>();
        text.text = dmg.ToString();


    }       

      public IEnumerator Shake(float duration, float magnitude)
    {
        Vector3 originalPosition = transform.position;

        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = originalPosition.x + Random.Range(-1f, 1f) * magnitude;

            transform.position = new Vector3(x, originalPosition.y, originalPosition.z);

            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = originalPosition;
    }
    private void SetColor(Color color)
    {
        material.SetColor(SHADER_COLOR_NAME, color);
    }
}