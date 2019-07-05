using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateSun : MonoBehaviour
{

    public float speed;
    void Update()
    {
        transform.Rotate(speed, 0f, 0f, Space.Self);
    }
}
