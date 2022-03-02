using Photon.Pun;
using Photon.Realtime;

//aliases
using PlayfabFriendInfo = PlayFab.ClientModels.FriendInfo;
using PhotonFriendInfo = Photon.Realtime.FriendInfo;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class PhotonFriendsController : MonoBehaviourPunCallbacks
{
    /*
    //basically yo
    public static Action<List<PhotonFriendInfo>> OnDisplayFriends;
    private void Awake()
    {
        OnDisplayFriends += PrintFriendListInfo;
        PlayFabController.OnFriendListUpdated += HandleFriendsUpdated;
    }

    private void OnDestroy()
    {
        OnDisplayFriends -= PrintFriendListInfo;
        PlayFabController.OnFriendListUpdated -= HandleFriendsUpdated;
    }

    private void HandleFriendsUpdated(List<PlayfabFriendInfo> friends)
    {
        if(friends.Count != 0)
        {
            string[] friendDisplayNames = friends.Select(f => f.TitleDisplayName).ToArray(); //go through list, pull out display name, and store in array. A lambda
            PhotonNetwork.FindFriends(friendDisplayNames);
        }
    }

    public override void OnFriendListUpdate(List<PhotonFriendInfo> friendList)
    {
        OnDisplayFriends?.Invoke(friendList); 
    }

    public void PrintFriendListInfo(List<PhotonFriendInfo> friends)
    {
        foreach (string name in friends.Select(f => f.UserId).ToArray())
        {
            Debug.Log("FFRIENFASDFfaAFADSFDSSFD NAME IS: " + name);
        }
    }*/
}
