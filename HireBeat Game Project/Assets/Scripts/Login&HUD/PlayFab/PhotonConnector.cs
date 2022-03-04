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
        Debug.Log("you have left a Photon Room");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"Another player has joined the room {newPlayer.UserId}");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"Player has left the room {otherPlayer.UserId}");
    }

    //When master client leaves, a new person becomes new master client
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log($"New Master Client is {newMasterClient.UserId}");
    }
}
