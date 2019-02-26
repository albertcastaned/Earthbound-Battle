using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveDescr : MonoBehaviour {
    public float dist = 0;
    public int dir = 0;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (dir==-1)
        {
            if (transform.localPosition.x > dist)
            {
                if (transform.localPosition.x <= -330.0f)
                {
                    transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
                    dist = -55.0f;
                }
                transform.localPosition = new Vector3(transform.localPosition.x - 5f, transform.localPosition.y, transform.localPosition.z);
            }
        }
        else if(dir==1)
        {
            if (transform.localPosition.x < dist)
            {
                if (transform.localPosition.x >= 0)
                {
                    transform.localPosition = new Vector3(-330.0f, 0.0f, 0.0f);
                    dist = -275.0f;
                }
                transform.localPosition = new Vector3(transform.localPosition.x + 5f, transform.localPosition.y, transform.localPosition.z);
            }
        }
	}


}
