using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bob : MonoBehaviour {

    public float floatStrength = 1;
    float original;
    public float speed;
    private float startTime;
    private float elapsedTime;
    public bool vertical = true;
    public bool relative = false;
    // Use this for initialization
    void Start()
    {
        if (vertical)
            original = transform.localPosition.y;
        else
            original = transform.localPosition.x;
        startTime = Time.time;

    }

    // Update is called once per frame
    void Update()
    {



        elapsedTime = Time.time - startTime;
        if (relative)
        {
            if (vertical)
                transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y + ((float)Mathf.Sin(elapsedTime * speed) * floatStrength), transform.localPosition.z);
            else
                transform.localPosition = new Vector3(transform.localPosition.x + ((float)Mathf.Sin(elapsedTime * speed) * floatStrength), transform.localPosition.y, transform.localPosition.z);
        }
        else
        {
            if (vertical)
                transform.localPosition = new Vector3(transform.localPosition.x, original + ((float)Mathf.Sin(elapsedTime * speed) * floatStrength), transform.localPosition.z);
            else
                transform.localPosition = new Vector3(original + ((float)Mathf.Sin(elapsedTime * speed) * floatStrength), transform.localPosition.y, transform.localPosition.z);
        }



    }
}
