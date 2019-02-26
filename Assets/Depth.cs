using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Depth : MonoBehaviour {
    private SpriteRenderer spriteLayer;
	// Use this for initialization
	void Start () {
        spriteLayer = GetComponent<SpriteRenderer>();	
	}
	
	// Update is called once per frame
	void Update () {
        if(transform.position.z > 60.0f)
            spriteLayer.sortingLayerName = "Default";
        else
            spriteLayer.sortingLayerName = "behindHPBox";

    }
}
