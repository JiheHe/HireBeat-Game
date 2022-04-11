using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class RoomDataCentralizer : MonoBehaviour
{
    // Start is called before the first frame update
    public List<string> playersInVoiceChat = new List<string>(); //using ids
    public SocialSystemScript socialSystem = null;
    public PhotonView view;

    public PhotonConnector pc;

    void Start()
    {
        socialSystem = GameObject.FindGameObjectWithTag("PlayerHUD").transform.Find("SocialSystem").GetComponent<SocialSystemScript>();
        view = GetComponent<PhotonView>();

        //This is not a good practice.
        Invoke("InstantiateAllVCUsersFromList", 2); //after 2 seconds, hopefully all the RPC calls are finished, then init all users!

        pc = GameObject.Find("PlayFabController").GetComponent<PhotonConnector>(); //pfc is persistent, so won't null.
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
    
    //You can call this function in your own room to kick people.
    public void SendKickPlayer(string playerID)
    {
        bool foundPlayer = false;
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if(player.UserId == playerID)
            {
                view.RPC("OnGettingKickedFromRoom", player);
                foundPlayer = true;
            }
        }
        
        if(!foundPlayer)
        {
            Debug.Log("Target player getting kicked has either left the room or is not found!");
        }
    }

    //I picked RPC method over CloseConnection by master client. This method is called by the room owner on YOU when he tries to kick you.
    //P.S. I don't think hackers can hack browser so should be good...
    [PunRPC]
    private void OnGettingKickedFromRoom()
    {
        PhotonConnector.disconnectDueToKicked = true;
        pc.DisconnectPlayer();
    }
}
