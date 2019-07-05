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

    public string soundClipName;

    public bool singleTarget;
    public Color colorShift;
    // Start is called before the first frame update
    void Start()
    {
        if (affectColors)
        {
            AudioManager.instance.Play(soundClipName);
            


            battleObject = GameObject.Find("BattleHandler");
            battle = battleObject.GetComponent<Battle>();
            opacityObject = GameObject.Find("Opacity");
            opacityControl = opacityObject.GetComponent<ControlOpacity>();
            opacityControl.setOpacity(true);
        }
        Destroy(gameObject, this.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length + delay);

    }
    void OnDestroy()
    {
        if (affectColors)

        {

            battle.SetHaltValue(false);
            opacityControl.setOpacity(false);
        }

    }

}
