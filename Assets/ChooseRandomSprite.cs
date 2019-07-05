using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChooseRandomSprite : MonoBehaviour
{

    public List<Sprite> possibleSprites;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<SpriteRenderer>().sprite = possibleSprites[Random.Range(0, possibleSprites.Count)];
    }

}
