using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;


//This is like the persistent version of Photon main menu thingy
public class PhotonConnector: MonoBehaviourPunCallbacks
{

    public override void OnJoinedRoom()
    {
        Debug.Log($"You have joined the Photon Room named {PhotonNetwork.CurrentRoom.Name}");
        Debug.Log("Your userID is: " + PhotonNetwork.AuthValues.UserId); //this is now Playfab's, as set in the controller!
        //PhotonConnector.GetPhotonFriends?.Invoke(); //THIS ONE HMMm
        PhotonNetwork.LoadLevel("MainScene"); //instead of loadscene

        if (PersistentData.TRUEOWNERID_OF_JOINING_ROOM != null) 
            PersistentData.TRUEOWNERID_OF_CURRENT_ROOM = PersistentData.TRUEOWNERID_OF_JOINING_ROOM;
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log($"You failed to join a Photon room: {message}");

        //Then you should be sent back to your current room (join or create), which hasn't been changed to joining room yet
        //Can do this because id is also the internal photon room name!
        //keep things as it is after.
        JoinOrCreateRoom(PersistentData.TRUEOWNERID_OF_CURRENT_ROOM);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log($"You have created a Photon Room named {PhotonNetwork.CurrentRoom.Name}");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);

        JoinOrCreateRoom(PersistentData.TRUEOWNERID_OF_CURRENT_ROOM);
    }

    public override void OnLeftRoom()
    {
        Debug.Log("you have left a Photon Room");
        SceneManager.LoadScene("LoadingScene"); //go back to this scene so reconnection can be triggered.
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
        //I think this works! I don't think leaving it here is good (proc on left room too). When you disconnect already this doesn't work w/e.
        //GameObject.FindGameObjectWithTag("DataCenter").GetComponent<RoomDataCentralizer>().UserLeavesRoomVC(GetComponent<PlayFabController>().myID);
        //Honestly if this doesn't work, then just either do RPC call once on OnPlayerLeftRoom from master, or use if to check if the player is still
        //in the room before forming the tab. 
        //Actually the code might already be doing the check (can find speaker amongst all room users?) when forming the tab, so this might not even be
        //necessary! (not a concern!). Just gonna leave it be w/e.

        base.OnDisconnected(cause);
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();

        Debug.Log("Connected to Photon Master Server!");
        //SceneManager.LoadScene("MainMenu");
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined lobby");
        JoinOrCreateRoom(PersistentData.TRUEOWNERID_OF_JOINING_ROOM);
    }

    private void JoinOrCreateRoom(string roomID)
    {
        RoomOptions roomOptions = new RoomOptions();

        /*Most Photon multiplayer games have 2 - 16 players, but the theoretical limit of players/ peers per room can be quite high. 
        There are Photon games live with 32 or even 64 players and in virtual conferencing scenarios it can be in the hundreds.However, 
        sending too many messages per second (msg/s per room) can cause performance issues depending on the client's processing power 
        coping with data. While high player numbers in e.g. turnbased games are totally fine, more than 8 players in a fast-paced action
        game likely will require you to implement interest management. This way not every player receives every message from all the 
        other players. The number of players per room is the main factor for increasing data traffic inside the game room: This is why
        we recommend keeping your msg/s per room count below 500. Photon does not enforce this limit, but relies on a fair use policy.
        Keeping an eye on your bandwidth usage is always important and it helps to ensure you stay within your plans included traffic 
        range of 3GB per CCU.*/

        roomOptions.MaxPlayers = 5; //will change
        roomOptions.PublishUserId = true;

        //roomOptions.IsOpen = true; //allows other users to join
        //roomOptions.IsVisible = true; //allows the room to be discovered by public

        PhotonNetwork.JoinOrCreateRoom("USERROOM_" + roomID, roomOptions, TypedLobby.Default);
    }

    public void DisconnectPlayer()
    {
        //Do all the things you need before you actually leave
        //This is for when you leave current room, thus leaving current VC
        GameObject.FindGameObjectWithTag("DataCenter").GetComponent<RoomDataCentralizer>().UserLeavesRoomVC(GetComponent<PlayFabController>().myID);

        //Then you leave.
        PhotonNetwork.LeaveRoom(false);
        //StartCoroutine(DisconnectAndLoad());
    }

    //This is to make sure that you leave completely if there's rejoin system, but by setting it to "false" you leave completely for sure.
    IEnumerator DisconnectAndLoad()
    {
        //yield return new WaitForSeconds(1f); //might not need this?
        PhotonNetwork.LeaveRoom();
        while(PhotonNetwork.InRoom)
        {
            yield return null;
        }
    }
}
