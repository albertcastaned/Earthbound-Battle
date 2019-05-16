using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlOpacity : MonoBehaviour
{
    private float aux = 0;
    private SpriteRenderer renderer;
    public bool darkTime = true;
    // Start is called before the first frame update
    void Start()
    {
        renderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (darkTime)
        {
            if (aux < 0.8f)
            {
                SetColor(new Color(0, 0, 0, aux));
                aux += 0.05f;
            }
        }
        else
        {
            if (aux > 0f)
            {
                SetColor(new Color(0, 0, 0, aux));
                aux -= 0.05f;
            }
        }
    }
    private void SetColor(Color color)
    {
        renderer.color = color;
    }
}
