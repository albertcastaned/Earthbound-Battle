using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveDialogue : MonoBehaviour {
    private Vector3 destination;
    private Vector3 velocity = Vector3.zero;

    void Start()
    {
        destination = transform.localPosition;
    }
    // Update is called once per frame
    void Update () {
        if (transform.localPosition != destination)
            transform.localPosition = Vector3.SmoothDamp(transform.localPosition, destination, ref velocity, 0.1f);
    }
    public void SetDestination(Vector3 dest)
    {
        destination = dest;
    }
}
