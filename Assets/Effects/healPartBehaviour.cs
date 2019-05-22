using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class healPartBehaviour : MonoBehaviour
{
    private new SpriteRenderer renderer;
    private float opacity = 1f;
    private float ranSpeed;
    // Start is called before the first frame update
    void Start()
    {
        ranSpeed = Random.Range(0f, 1f);
        renderer = GetComponent<SpriteRenderer>();
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z + Random.Range(-20f, 20f));
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y - 1f - ranSpeed, transform.position.z);
        renderer.material.color = new Color(1f, 1f, 1f, opacity);
        opacity -= 0.020f;

        if (opacity <= 0)
            Destroy(gameObject);

    }
}
