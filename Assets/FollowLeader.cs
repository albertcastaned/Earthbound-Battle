using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowLeader : MonoBehaviour
{

    [SerializeField] private Movement target;


    [SerializeField] private int offset = 1;
    

    private Animator animator;

    
    // Start is called before the first frame update

    public float speed;

    private Vector3 direction;
    void Start()
    {
        animator = GetComponent<Animator>();
        direction = new Vector3();
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if (target.GetWaypointSize() <= offset) return;
        animator.enabled = true;

        Vector3 originalPosition = target.GetWayPoint(offset - 2);
        Vector3 destination = target.GetWayPoint(offset);
            
        direction = destination - originalPosition;

        if (transform.position != destination)
        {
            if (direction.x > 0f && Mathf.Abs(direction.z) < 0.08f)
            {
                animator.Play("Right");

            }
            else if (direction.x < 0f && Mathf.Abs(direction.z) < 0.08f)
            {
                animator.Play("Left");

            }
            else if (direction.z > 0f && Mathf.Abs(direction.x) < 0.08f)
            {
                animator.Play("Up");
            }
            else if (direction.z < 0f && Mathf.Abs(direction.x) < 0.08)
            {
                animator.Play("Down");

            }
            else if (direction.x > 0f && direction.z > 0f)
            {
                animator.Play("upRight");
            }
            else if (direction.x < -0f && direction.z > 0f)
            {
                animator.Play("upLeft");

            }
            else if (direction.x > 0f && direction.z < 0f)
            {
                animator.Play("downRight");

            }
            else if (direction.x < 0f && direction.z < 0f)
            {
                animator.Play("downLeft");
            }
        }
        else
        {
            animator.enabled = false;
        }


        if (transform.position != destination)
        {
            transform.position =
                Vector3.MoveTowards(transform.position, destination, Time.fixedDeltaTime * speed);
        }

    }

}
