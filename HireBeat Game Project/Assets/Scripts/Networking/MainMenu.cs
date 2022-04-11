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

        PhotonNetwork.CreateRoom(createInput.text, roomOptions);
    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(joinInput.text);
    }

    //This function below is also not called yet. But I can see it being useful in future
    public void JoinOrCreateRoom(string roomName)
    {
        RoomOptions roomOptions = new RoomOptions();
        //set your options
        PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
    }
}
