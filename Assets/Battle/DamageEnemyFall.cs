using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageEnemyFall : MonoBehaviour
{
    public float velocity;
    public float gravity;
    private float horizontalMovement;
    // Start is called before the first frame update
    void Start()
    {
    
        horizontalMovement = Random.Range(-0.6f, 0.6f);
        velocity += Random.Range(0.1f, 0.5f);
        gravity += Random.Range(0.03f, 0.07f);

    }

    // Update is called once per frame
    void Update()
    {

        transform.position = new Vector3(transform.position.x + horizontalMovement, transform.position.y + velocity, transform.position.z);
        velocity -= gravity;
        if (transform.position.y < -500f)
            Destroy(gameObject);
    }
}
