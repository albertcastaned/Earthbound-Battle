using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class Player : MonoBehaviour
{
    public string playerName;

    public int level;

    public int HP;

    public int PP;

    public int Offense;
    public int GetOffense() => Offense;

    public int Defense;
    public int GetDeffense() => Defense;

    public int Speed;
    public int GetSpeed() => Speed;

    public int Guts;
    public int GetGuts() => Guts;

    private bool defending;

    public bool Defending { get => defending; set => defending = value; }

    public int GetPP => PP;
    public GameObject portrait;

    private bool borderAttackingPosition;
    private float borderDisplacement;
    private float portraitDisplacement;

    private Vector3 borderOriginalPos;
    private Vector3 portraitOriginalPos;

    private Vector3 borderVelocity = Vector3.zero;
    private Vector3 portraitVelocity = Vector3.zero;

    public GameObject healGUI;

    public GetMeterValue ppMeter;
    [StringInList("Idle", "Asleep", "Frozen", "Paralysis")] public string status;
    // Start is called before the first frame update
    void Start()
    {
        borderOriginalPos = transform.position;
        portraitOriginalPos = portrait.transform.position;
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.SmoothDamp(transform.position, new Vector3(borderOriginalPos.x, borderOriginalPos.y + borderDisplacement, borderOriginalPos.z), ref borderVelocity, 0.2f);
        portrait.transform.position = Vector3.SmoothDamp(portrait.transform.position, new Vector3(portraitOriginalPos.x, portraitOriginalPos.y + portraitDisplacement, portraitOriginalPos.z), ref portraitVelocity, 0.2f);

    }
    public void MoveBorder(bool value)
    {
        if (value == borderAttackingPosition)
            return;
        if (borderAttackingPosition)
        {
            borderDisplacement = 0f;
            portraitDisplacement = -35f;
            borderAttackingPosition = false;
        }
        else
        {
            borderDisplacement = 15f;
            portraitDisplacement = 20f;

            borderAttackingPosition = true;
        }
    }

    public void InstantiateHeal(int dmg)
    {
        GameObject prefab = Instantiate(healGUI, GameObject.FindGameObjectWithTag("Canvas").transform, false) as GameObject;
        prefab.transform.localPosition = new Vector3(portrait.transform.localPosition.x, portrait.transform.localPosition.y + 4f, 30f);
        TMP_Text text = prefab.transform.GetChild(0).GetComponent<TMP_Text>();
        text.text =  "+" + dmg.ToString();
    }
}
