using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAnimation : MonoBehaviour
{
    public float delay = 0f;
    private Battle battle;
    private GameObject battleObject;
    private ControlOpacity opacityControl;
    private GameObject opacityObject;
    public bool affectColors = true;
    public List<SpriteRenderer> enemies;
    public List<Color> colors;
    // Start is called before the first frame update
    void Start()
    {
        if (affectColors)
        {
            enemies = new List<SpriteRenderer>();
            colors = new List<Color>();
            foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
            {
                SpriteRenderer aux = enemy.GetComponent<SpriteRenderer>();
                enemies.Add(aux);
                colors.Add(aux.color);
            }


            battleObject = GameObject.Find("BattleHandler");
            battle = battleObject.GetComponent<Battle>();
            opacityObject = GameObject.Find("Opacity");
            opacityControl = opacityObject.GetComponent<ControlOpacity>();
            opacityControl.setOpacity(true);
        }
        Destroy(gameObject, this.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length + delay);

    }
    void Update()
    {
        if (!affectColors)
        {
            return;
        }
        else
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                enemies[i].color = Color.Lerp(colors[i], Color.red, Time.time);
            }
        }
    }
    void OnDestroy()
    {
        if (affectColors)

        {
            for (int i = 0; i < enemies.Count; i++)
            {
                enemies[i].color = Color.Lerp(Color.red, colors[i], Time.time);
            }
            battle.SetHaltValue(false);
            opacityControl.setOpacity(false);
        }

    }

}
