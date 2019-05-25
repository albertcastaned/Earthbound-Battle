using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour {
    public Vector3 EulerRotation = new Vector3(0f, 60f, 0f);
    public float TimeToRotate = .2f;
    public int num = 1;
    public bool rotating;
    public GameObject holder;
    private MoveDescr holdScript;
    private Vector3 velocity = Vector3.zero;
    private bool canMove;
    public void Start()
    {
        holdScript = holder.GetComponent<MoveDescr>();
        canMove = true;
    }

    public void SetCanMove(bool boo)
    {
        canMove = boo;
    }
    public bool GetCanMove()
    {
        return canMove;
    }
    private void Update()
    {
        if (!rotating && canMove)
        {
            if (Input.GetKey(KeyCode.D))
            {
                StartCoroutine(doRotate(EulerRotation));
                if (num == 6)
                {
                    holdScript.dist += -55.0f;
                    holdScript.dir = -1;
                    num = 1;

                }
                else
                {
                    num++;
                    holdScript.dist += -55.0f;
                    holdScript.dir = -1;
                }
            }
            else if (Input.GetKey(KeyCode.A))
            {
                StartCoroutine(doRotate(-EulerRotation));
                if (num == 1)
                {
                    num = 6;
                    holdScript.dist += 55.0f;
                    holdScript.dir = 1;
                }
                else
                {
                    num--;
                    holdScript.dist += 55.0f;
                    holdScript.dir = 1;

                }
            }
        }
    }

    private IEnumerator doRotate(Vector3 rotation)
    {
        Quaternion start = transform.rotation;
        Quaternion destination = start * Quaternion.Euler(rotation);
        float startTime = Time.time;
        float percentComplete = 0f;
        rotating = true;
        while (percentComplete <= 1.0f)
        {
            percentComplete = (Time.time - startTime) / TimeToRotate;
            transform.rotation = Quaternion.Slerp(start, destination, percentComplete);
            yield return null;
        }
        rotating = false;
        transform.rotation = destination;
    }

}


