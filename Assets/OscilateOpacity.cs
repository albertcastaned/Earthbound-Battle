using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OscilateOpacity : MonoBehaviour
{
    private new SpriteRenderer renderer;
    private float aux = 0f;
    private bool selected = false;
    // Start is called before the first frame update
    void Start()
    {
        renderer = GetComponent<SpriteRenderer>();
    }

    public void SetSelected(bool a)
    {
        selected = a;

    }
    // Update is called once per frame
    void Update()
    {
        if (selected)
        {
            aux += 0.1f;

            if (aux > 2 * Mathf.PI)
            {
                aux = 0;
            }
            renderer.color = new Color(1, 1, 1, (0.4f + (Mathf.Sin(aux) / 6f)));
        }
        else
        {
            renderer.color = new Color(1, 1, 1, 0);
        }
    }
}
