using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDataManager : MonoBehaviour
{
    //this kinda centralizes important data

    public ProfileUpdater playerPfpSprite;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //yh though you don't need to do that for most stuff unless both clients don't have the same media
    //there's probably a limit to the size of the rpc data too but you could likely split the byte array easily enough if needed
    public void changeSprite(Texture2D tex) //kinda redundant LOL, w/e
    {
        byte[] imgByteArr = tex.EncodeToPNG();
        playerPfpSprite.changeSprite(imgByteArr);
    }
}
