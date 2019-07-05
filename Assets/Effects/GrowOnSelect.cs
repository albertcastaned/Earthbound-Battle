using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrowOnSelect : MonoBehaviour
{

    private float originalScale;
    private bool grow;
    private bool inPlace = true;
    // Start is called before the first frame update
    void Start()
    {
        originalScale = transform.localScale.x;
    }

    // Update is called once per frame
    void Update()
    {
        if(grow)
        {
            if (transform.localScale.x < 3f && inPlace)
            {
                var transform1 = transform;
                var localScale = transform1.localScale;
                localScale = new Vector3(localScale.x + (Time.deltaTime * 4.5f), localScale.y + (Time.deltaTime * 4.5f), localScale.z);
                transform1.localScale = localScale;
            }
            else if(inPlace)
            {
                var transform1 = transform;
                transform1.localScale = new Vector3(3f, 3f, transform1.localScale.z);
            }
        }
        else
        {
            if (transform.localScale.x > originalScale)
            {
                var transform1 = transform;
                var localScale = transform1.localScale;
                localScale = new Vector3(localScale.x - (Time.deltaTime * 4.5f), localScale.y - (Time.deltaTime * 4.5f), localScale.z);
                transform1.localScale = localScale;
            }
            else
            {
                var transform1 = transform;
                transform1.localScale = new Vector3(originalScale,originalScale,transform1.localScale.z);
            }
        }
    }

    public void SetGrow(bool value)
    {
        grow = value;
    }
    public void SetInPlace(bool value)
    {
        inPlace = value;
    }
}
