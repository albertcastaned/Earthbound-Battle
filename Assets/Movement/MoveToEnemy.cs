using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MoveToEnemy : MonoBehaviour {

    private Vector3 destination;
    private Vector3 velocity = Vector3.zero;
    public TMP_Text textmesh;
	// Use this for initialization
	void Start () {
        destination = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
        if(!FloatEqual(transform.position.x,destination.x))
        transform.position = Vector3.SmoothDamp(transform.position, destination, ref velocity, 0.2f);
    }

    public void SetName(string name)
    {
        textmesh.text = name;
    }
    public void SetDestination(Vector3 dest)
    {
        destination = dest;
    }
    public void SetPosition(Vector3 pos)
    {
        transform.position = pos;
    }
    public static bool FloatEqual(float a, float b)
    {
        return Mathf.Sqrt(a * a + b * b) < 0.025f;
    }
}
