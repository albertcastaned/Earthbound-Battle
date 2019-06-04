using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlOpacity : MonoBehaviour
{
    private float aux = 0;
    private new SpriteRenderer renderer;
    public bool darkTime = false;
    public bool autoChange = false;
    public float maxOpacity = 0.8f;
    public float speed = 0.05f;

    // Start is called before the first frame update
    void Start()
    {
        renderer = GetComponent<SpriteRenderer>();
    }

    public void setOpacity(bool aux)
    {
        darkTime = aux;
    }
    // Update is called once per frame
    void Update()
    {
        if (darkTime)
        {
            if (aux < maxOpacity)
            {
                SetColor(new Color(renderer.color.r, renderer.color.g, renderer.color.b, aux));
                aux += speed * Time.deltaTime;
            }
            else
            {
                if(autoChange)
                {
                    if (aux > 1.5f)
                    {
                        darkTime = false;
                        aux = maxOpacity;
                    }
                    else
                        aux += speed * Time.deltaTime;
                }
                
            }
        }
        else
        {
            if (aux > 0f)
            {
                SetColor(new Color(renderer.color.r, renderer.color.g, renderer.color.b, aux));
                aux -= speed * Time.deltaTime;
            }
        }
    }
    private void SetColor(Color color)
    {
        renderer.color = color;
    }
}
