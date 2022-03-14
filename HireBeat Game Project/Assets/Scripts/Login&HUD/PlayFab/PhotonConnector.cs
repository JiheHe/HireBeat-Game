using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System;
using System.Collections.Generic;


//This is like the persistent version of Photon main menu thingy
public class PhotonConnector: MonoBehaviourPunCallbacks
{
    public override void OnLeftRoom()
    {
        GameObject.FindGameObjectWithTag("DataCenter").GetComponent<RoomDataCentralizer>().UserLeavesRoomVC(GetComponent<PlayFabController>().myID);
        //This is for when you leave current room, thus leaving current VC

        Debug.Log("you have left a Photon Room");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"Another player has joined the room {newPlayer.UserId}");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        //gotta call it here... OnLeftRoom basically doesn't work if you leave directly..... I think OnDisconnected is called.
        GameObject.FindGameObjectWithTag("PlayerHUD").transform.Find("VoiceChat").GetComponent<VoiceChatController>().ClearSpeaker(otherPlayer.UserId);

        Debug.Log($"Player has left the room {otherPlayer.UserId}");
    }

    //When master client leaves, a new person becomes new master client
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log($"New Master Client is {newMasterClient.UserId}");
    }

    public override void OnDisconnected(DisconnectCause cause) //when you disconnect from game, self announce RPC that you disconnect from VC
    {
        //I think this works!
        GameObject.FindGameObjectWithTag("DataCenter").GetComponent<RoomDataCentralizer>().UserLeavesRoomVC(GetComponent<PlayFabController>().myID);
        //Honestly if this doesn't work, then just either do RPC call once on OnPlayerLeftRoom from master, or use if to check if the player is still
        //in the room before forming the tab. 
        //Actually the code might already be doing the check (can find speaker amongst all room users?) when forming the tab, so this might not even be
        //necessary! (not a concern!). Just gonna leave it be w/e.

        base.OnDisconnected(cause);
    }
}
