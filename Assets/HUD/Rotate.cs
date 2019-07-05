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

    public List<GrowOnSelect> selections;
    public void Start()
    {
        holdScript = holder.GetComponent<MoveDescr>();
        canMove = true;
        selections[num - 1].SetGrow(true);

    }

    public void SetCanMove(bool boo)
    {
        canMove = boo;
    }
    public bool GetCanMove()
    {
        return canMove;
    }
    void Update()
    {
        if(!rotating)
            selections[num - 1].SetInPlace(true);
        else
            selections[num - 1].SetInPlace(false);

        if (rotating || !canMove) return;
        if (Input.GetKey(KeyCode.D))
        {
            AudioManager.instance.Play("LeftMenu");

            StartCoroutine(doRotate(EulerRotation));
            if (num == 6)
            {
                holdScript.dist += -55.0f;
                holdScript.dir = -1;
                selections[num - 1].SetGrow(false);
                num = 1;
                selections[num - 1].SetGrow(true);




            }
            else
            {
                selections[num - 1].SetGrow(false);
                num++;
                selections[num - 1].SetGrow(true);


                holdScript.dist += -55.0f;
                holdScript.dir = -1;
            }
        }
        else if (Input.GetKey(KeyCode.A))
        {
            AudioManager.instance.Play("RightMenu");

            StartCoroutine(doRotate(-EulerRotation));
            if (num == 1)
            {
                selections[num - 1].SetGrow(false);
                num = 6;
                selections[num - 1].SetGrow(true);


                holdScript.dist += 55.0f;
                holdScript.dir = 1;
            }
            else
            {
                selections[num - 1].SetGrow(false);
                num--;
                selections[num - 1].SetGrow(true);

                holdScript.dist += 55.0f;
                holdScript.dir = 1;

            }
        }
    }

    private IEnumerator doRotate(Vector3 rotation)
    {
        var start = transform.rotation;
        var destination = start * Quaternion.Euler(rotation);
        var startTime = Time.time;
        var percentComplete = 0f;
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


