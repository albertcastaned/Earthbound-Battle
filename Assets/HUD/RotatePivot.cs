using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatePivot : MonoBehaviour
{

    public float speed = 10;
    private Transform origin;
    private float angle;
    public GameObject healParticle;
    private float opacity = 1f;
    private new SpriteRenderer renderer;
    private ControlOpacity opacityBorder;
    private bool ready = false;
    // Start is called before the first frame update
    void Start()
    {
        angle = Random.Range(-10f, 10f);
        renderer = GetComponent<SpriteRenderer>();
    }
    public void Setup(Transform transf, ControlOpacity opc)
    {
        var position = transf.position;
        transform.position = new Vector3(position.x + 50f, position.y + 20f, position.z);
        origin = transf;
        opacityBorder = opc;
        opacityBorder.setOpacity(true);

        ready = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!ready)
            return;
        if (Random.Range(0, 100) < (30 - (1 / opacity)/2))
        {
            Instantiate(healParticle, new Vector3(transform.position.x, transform.position.y, transform.position.z), transform.rotation);
        }

        var aux = Quaternion.Euler(angle, 0, 0) * origin.up * 3;
        transform.RotateAround(origin.position, aux, speed * Time.deltaTime);
        opacity -= Time.deltaTime / 2f;
        renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, opacity);

        if(opacity<=0)
        {
            Destroy(gameObject);
        }
    }
}
