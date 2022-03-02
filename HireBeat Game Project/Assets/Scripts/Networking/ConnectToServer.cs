using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    // Update is called once per frame
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon");
        SceneManager.LoadScene("MainMenu");
        if (!PhotonNetwork.InLobby) {
            PhotonNetwork.JoinLobby();
        }
    }


    //this function is not called yet, but it does allow you to (I think))...
    //just some useful functions below ig
    //ALso I'm not doing server -> room instead of server -> lobby -> room?
    public void CreatePrivateRoom()
    {
        string randomName = $"Room{Guid.NewGuid().ToString()}";
        //The two steps below connect you to the master server (room) upon joining
        //PhotonNetwork.AutomaticallySyncScene = true; //this lets the master client dominates the scene transition
        //PhotonNetwork.ConnectUsingSettings(); //not sure if this is useful
    }
}
