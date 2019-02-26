using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bob : MonoBehaviour {

    public float floatStrength = 1;
    float originalY;
    public float speed;
    private float startTime;
    private float elapsedTime;
    // Use this for initialization
    void Start()
    {
        originalY = transform.localPosition.y;
        startTime = Time.time;

    }

    // Update is called once per frame
    void Update()
    {



            elapsedTime = Time.time - startTime;
            transform.localPosition = new Vector3(transform.localPosition.x, originalY + ((float)Mathf.Sin(elapsedTime * speed) * floatStrength), transform.localPosition.z);

        
    }
}
