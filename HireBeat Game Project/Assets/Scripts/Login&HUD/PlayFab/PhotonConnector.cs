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
    //public static Action GetPhotonFriends = delegate { };
    // Start is called before the first frame update

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

    //This only works in a lobby via main menu, not in a room :(
    /*public override void OnFriendListUpdate(List<FriendInfo> friendList) //every time this is called, updates status boolean in playfabcontroller
    {
        base.OnFriendListUpdate(friendList);

        Dictionary<string, bool> friendOnStatus = new Dictionary<string, bool>();

        foreach (FriendInfo f in friendList)
        {
            friendOnStatus.Add(f.UserId, f.IsOnline);
        }

        //should return a dictionary of( ID, status pair) to playfab controller. implement the if in update function.
        //actually don't. More efficient to call friend list update only. 
        gameObject.GetComponent<PlayFabController>().UpdateFriendListStatus(friendOnStatus);
    }*/
}
