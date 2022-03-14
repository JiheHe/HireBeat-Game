using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;
using System.Linq;

public class MainMenu : MonoBehaviourPunCallbacks //this is inheritance! for override
{

    public TMP_InputField createInput;
    public TMP_InputField joinInput;

    // Start is called before the first frame update
    public void CreateRoom()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 3; //will change
        roomOptions.PublishUserId = true;

        //roomOptions.IsOpen = true; //allows other users to join
        //roomOptions.IsVisible = true; //allows the room to be discovered by public

        PhotonNetwork.CreateRoom(createInput.text, roomOptions);
    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(joinInput.text); ;
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"You have joined the Photon Room named {PhotonNetwork.CurrentRoom.Name}");
        Debug.Log("Your userID is: " + PhotonNetwork.AuthValues.UserId); //this is now Playfab's, as set in the controller!
        //PhotonConnector.GetPhotonFriends?.Invoke(); //THIS ONE HMMm
        PhotonNetwork.LoadLevel("MainScene"); //instead of loadscene
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log($"You failed to join a Photon room: {message}");
    }

    //This function below is also not called yet. But I can see it being useful in future
    public void JoinOrCreateRoom(string roomName)
    {
        RoomOptions roomOptions = new RoomOptions();
        //set your options
        PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log($"You have created a Photon Room named {PhotonNetwork.CurrentRoom.Name}");
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined lobby");

        //This only works in lobby...
        /*PlayFabClientAPI.GetFriendsList(new GetFriendsListRequest
        {
            IncludeSteamFriends = false,
            IncludeFacebookFriends = false,
            XboxToken = null
        }, result => {
            var users = result.Friends;
            List<string> friendIDs = new List<string>();
            foreach (PlayFab.ClientModels.FriendInfo user in users)
            {
                if(user.Tags[0] == "confirmed")
                {
                    friendIDs.Add(user.FriendPlayFabId);
                }
            }
            PhotonNetwork.FindFriends(friendIDs.ToArray());
        }, DisplayPlayFabError);*/
    }

    void DisplayPlayFabError(PlayFabError error)
    {
        Debug.Log(error.GenerateErrorReport());
    }
}
