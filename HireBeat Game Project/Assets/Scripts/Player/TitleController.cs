using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class TitleController : MonoBehaviour
{
    int titleIndex;
    PhotonView view;

    // Start is called before the first frame update
    void Start()
    {
        titleIndex = 0;
        view = GetComponent<PhotonView>();
    }

    public void changeTitle(int nextIndex)
    {
        view.RPC("changeTitleRPC", RpcTarget.AllBuffered, nextIndex, titleIndex); //need to let other clients know the current title index (might not be 0 if joined late)
    }

    [PunRPC]
    void changeTitleRPC(int nextIndex, int titleIndex)
    {
        transform.GetChild(titleIndex).gameObject.SetActive(false);
        transform.GetChild(nextIndex).gameObject.SetActive(true);
        this.titleIndex = nextIndex;
    }
}
