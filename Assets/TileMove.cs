using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMove : MonoBehaviour {
    public float ScrollX = 0.1f;
    public float ScrollY = 0.1f;

    // Update is called once per frame
    void LateUpdate () {
        float OffsetX = Time.time * ScrollX % 1.0f;
        float OffsetY = Time.time * ScrollY % 1.0f;
        GetComponent<Renderer>().material.mainTextureOffset = new Vector2(OffsetX, OffsetY);

    }
}
