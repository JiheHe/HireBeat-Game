using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class TitleController : MonoBehaviour
{
    int titleIndex;
    PhotonView view;

    bool titleOff;

    // Start is called before the first frame update
    void Start()
    {
        titleIndex = 0; //will change to suit user data
        titleOff = false;
        view = GetComponent<PhotonView>();
    }

    public void changeTitle(int nextIndex)
    {
        view.RPC("changeTitleRPC", RpcTarget.AllBuffered, nextIndex, titleIndex); //need to let other clients know the current title index (might not be 0 if joined late)
    }

    [PunRPC]
    void changeTitleRPC(int nextIndex, int titleIndex)
    {
        //if title off, then transparent nextIndex and make visible titleIndex
        transform.GetChild(titleIndex).gameObject.SetActive(false);
        if (titleOff) ChangeTitleAlpha(1f);
        this.titleIndex = nextIndex;
        if (titleOff) ChangeTitleAlpha(0f); //set the nextIndex to transparent
        transform.GetChild(nextIndex).gameObject.SetActive(true);
    }

    void ChangeTitleAlpha(float val)
    {
        view.RPC("ChangeTitleAlphaRPC", RpcTarget.AllBuffered, val);
    }

    
    [PunRPC]
    public void ChangeTitleAlphaRPC(float val) //change currently stored titleIndex
    {
        TMP_Text textElem = transform.GetChild(titleIndex).gameObject.GetComponent<TMP_Text>();
        Color myColor = textElem.color;
        myColor.a = val;
        textElem.color = myColor;
    }

    public void SetTitleOnOff(int state)
    {
        view.RPC("SetTitleOnOffRPC", RpcTarget.AllBuffered, state);
    }

    [PunRPC]
    public void SetTitleOnOffRPC(int state)
    {
        if (state == 1)
        {
            titleOff = true;
            ChangeTitleAlpha(0f);
        }
        else
        {
            titleOff = false;
            ChangeTitleAlpha(1f);
        }
    }
    
}
