using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowEffect : MonoBehaviour
{

    public Vector2 offset = new Vector2(-3, -3);

    private SpriteRenderer spriteCaster;
    private SpriteRenderer spriteShadow;

    private Transform transCaster;
    private Transform transShadow;

    public Material shadowMaterial;
    public Color shadowColor;
    // Start is called before the first frame update
    void Start()
    {

        transCaster = transform;
        transShadow = new GameObject().transform;
        transShadow.parent = transCaster;
        transShadow.gameObject.name = "Shadow";
        transShadow.localRotation = Quaternion.identity;
        transShadow.localScale = Vector3.one;

        spriteCaster = GetComponent<SpriteRenderer>();
        spriteShadow = transShadow.gameObject.AddComponent<SpriteRenderer>();

        spriteShadow.material = shadowMaterial;
        spriteShadow.color = shadowColor;
        spriteShadow.sortingLayerName = spriteCaster.sortingLayerName;
        spriteShadow.sortingOrder = spriteCaster.sortingOrder - 1;
        spriteShadow.sprite = spriteCaster.sprite;

    }

    // Update is called once per frame
    void Update()
    {
        transShadow.position = new Vector3(transCaster.position.x + offset.x, transCaster.position.y + offset.y, transCaster.position.z);
    }
}
