using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class grow : MonoBehaviour {

    public float floatStrength = 1;
    float originalY;
    public float speed;
    public float id;
    public Rotate rotateScript;
    private float startTime;
    private float elapsedTime;
    public bool bobbing;
    SpriteRenderer m_SpriteRenderer;
    private Vector3 ogSize;
    // Use this for initialization
    void Start () {
        originalY = transform.localPosition.y;
        rotateScript = GetComponentInParent<Rotate>();
        startTime = Time.time;
        bobbing = true;

        m_SpriteRenderer = GetComponent<SpriteRenderer>();
        ogSize = transform.localScale;
        //Set the GameObject's Color quickly to a set Color (blue)

    }

    // Update is called once per frame
    void Update () {


        if (bobbing)
        {

            elapsedTime = Time.time - startTime;
            transform.localPosition = new Vector3(transform.localPosition.x, originalY + ((float)Mathf.Sin(elapsedTime * speed) * floatStrength), transform.localPosition.z);
            if (id == rotateScript.num && !rotateScript.rotating)
            {

                //m_SpriteRenderer.color = Color.yellow;
                if(transform.localScale.x < 3f)
                transform.localScale = new Vector3(transform.localScale.x + 0.2f, transform.localScale.y + 0.2f, transform.localScale.z);
                
            }
            else
            {
                if (transform.localScale.x > ogSize.x)
                    transform.localScale = new Vector3(transform.localScale.x - 0.2f, transform.localScale.y - 0.2f, transform.localScale.z);
            }


        }
    }
}
