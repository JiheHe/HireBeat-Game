using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDataManager : MonoBehaviour
{
    public SpriteRenderer playerPfpSprite;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void changeSprite(Sprite sprite)
    {
        playerPfpSprite.sprite = sprite;
        playerPfpSprite.transform.localScale = new Vector2(0.85f, 0.85f);
    }
}
