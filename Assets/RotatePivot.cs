using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatePivot : MonoBehaviour
{

    public float speed = 10;
    private Transform target;
    private float angle;
    public GameObject healParticle;
    private float opacity = 1f;
    private new SpriteRenderer renderer;
    private ControlOpacity opacityBorder;
    // Start is called before the first frame update
    void Start()
    {
        angle = Random.Range(-10f, 10f);
        opacityBorder = GameObject.Find("healOverlay").GetComponent<ControlOpacity>();
        target = GameObject.Find("healOverlay").transform;
        renderer = GetComponent<SpriteRenderer>();
        opacityBorder.setOpacity(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (Random.Range(0, 100) < (30 - (1 / opacity)/2))
        {
            Instantiate(healParticle, new Vector3(transform.position.x, transform.position.y, transform.position.z), transform.rotation);
        }
        Vector3 aux = Quaternion.Euler(angle, 0, 0) * target.up * 3;
        transform.RotateAround(target.position, aux, speed * Time.deltaTime);
        opacity -= 0.010f;
        renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, opacity);

        if(opacity<=0)
        {
            Destroy(gameObject);
        }
    }
}
