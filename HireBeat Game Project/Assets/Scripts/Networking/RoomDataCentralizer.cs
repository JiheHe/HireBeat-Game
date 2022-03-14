using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class RoomDataCentralizer : MonoBehaviour
{
    // Start is called before the first frame update
    public List<string> playersInVoiceChat = new List<string>(); //using ids
    public SocialSystemScript socialSystem = null;
    public PhotonView view;

    void Start()
    {
        socialSystem = GameObject.FindGameObjectWithTag("PlayerHUD").transform.Find("SocialSystem").GetComponent<SocialSystemScript>();
        view = GetComponent<PhotonView>();

        Invoke("InstantiateAllVCUsersFromList", 2); //after 2 seconds, hopefully all the RPC calls are finished, then init all users!
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UserJoinsRoomVC(string userID)
    {
        view.RPC("UserJoinsRoomVCRPC", RpcTarget.AllBuffered, userID);
    }

    [PunRPC]
    public void UserJoinsRoomVCRPC(string userID)
    {
        playersInVoiceChat.Add(userID);
    }

    public void UserLeavesRoomVC(string userID)
    {
        view.RPC("UserLeavesRoomVCRPC", RpcTarget.AllBuffered, userID);
    }

    [PunRPC]
    public void UserLeavesRoomVCRPC(string userID)
    {
        playersInVoiceChat.Remove(userID);
    }

    public void InstantiateAllVCUsersFromList()
    {
        foreach(string playerID in playersInVoiceChat)
        {
            socialSystem.gameObject.transform.parent.Find("VoiceChat").GetComponent<VoiceChatController>().OnOtherPlayerConnected(playerID);
        }
    }
}
