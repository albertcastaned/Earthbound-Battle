using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LINE : MonoBehaviour {

    void OnDrawGizmosSelected()
    {

        Gizmos.color = Color.blue;
        Vector3 og = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        Vector3 line = new Vector3(transform.position.x + 20f, transform.position.y, transform.position.z);
        Gizmos.DrawLine(og, line);

    }
}
