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
    //If don't want the null bug, the (parent) object has to be active....
    [PunRPC]
    public void changeSpriteRPC(byte[] imgByteArr)
    {
        SpriteRenderer newSprite = GetComponent<SpriteRenderer>();
        Texture2D myTexture = new Texture2D(1, 1, TextureFormat.RGB24, false, true); //or use constants //newSprite.sprite.texture.height
        myTexture.LoadImage(imgByteArr);
        Sprite spriteImg = Sprite.Create(myTexture, new Rect(0, 0, myTexture.width, myTexture.height), new Vector2(0.5f, 0.5f));
        newSprite.sprite = spriteImg;
        gameObject.transform.localScale = new Vector2(0.558f, 0.355f);
        //ANy function that's executed in rpc will be executed on all, so can safely add others
    }
}
