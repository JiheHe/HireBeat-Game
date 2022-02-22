using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ProfileUpdater : MonoBehaviour
{
    //this shows PFP and "business card" profile page

    PhotonView view;

    void Start()
    {
        view = GetComponent<PhotonView>();
        //newSprite = null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void changeSprite(byte[] imgByteArr)
    {
        view.RPC("changeSpriteRPC", RpcTarget.AllBuffered, imgByteArr); 
    }


    //https://stackoverflow.com/questions/67576685/is-there-any-way-to-send-or-receive-images-audio-and-video-using-photon-pun2-or
    //encoding and decoding!
    //basically that's how data stream works. Can send other img files etc too!
    [PunRPC]
    void changeSpriteRPC(byte[] imgByteArr)
    {
        SpriteRenderer newSprite = GetComponent<SpriteRenderer>(); 
        Texture2D myTexture = new Texture2D(newSprite.sprite.texture.width, newSprite.sprite.texture.height, TextureFormat.RGB24, false, true); //or use constants
        myTexture.LoadImage(imgByteArr);
        Sprite spriteImg = Sprite.Create(myTexture, new Rect(0, 0, myTexture.width, myTexture.height), new Vector2(0.5f, 0.5f));
        newSprite.sprite = spriteImg;
        SetProfileDisplaySprite(spriteImg);
        gameObject.transform.localScale = new Vector2(0.85f, 0.85f);
        //gameObject.GetComponent<SpriteRenderer>().sprite = newSprite;
        //
    }

    void SetProfileDisplaySprite(Sprite newSprite)
    {
        gameObject.transform.parent.transform.parent.GetComponent<OnMouseOverObject>().UpdatePfpImage(newSprite);

    }
}
