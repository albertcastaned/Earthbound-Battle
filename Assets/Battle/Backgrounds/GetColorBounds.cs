using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetColorBounds : MonoBehaviour {
    private Texture2D sourceTex;
    private float min = 0, max = 256;

    Renderer rend;
	// Use this for initialization
	void Start () {
        rend = GetComponent<Renderer>();
        sourceTex = rend.material.GetTexture("_MainTex") as Texture2D;
        int x = Mathf.RoundToInt(transform.position.x);
        int y = Mathf.RoundToInt(transform.position.y);
        int width = Mathf.FloorToInt(sourceTex.width);
        int height = Mathf.FloorToInt(sourceTex.height);
        
        for(int i=y;i<height;i++)
        {
            for(int j=x;j<width;j++)
            {
                float aux = sourceTex.GetPixel(j, i).r * 255.0f;
                if (aux > min)
                    min = aux;
                if (aux < max)
                    max = aux;
            }
        }

        rend.material.SetFloat("_Max", max);
        rend.material.SetFloat("_Min", min);


    }

    // Update is called once per frame
    void Update () {
		
	}
}
