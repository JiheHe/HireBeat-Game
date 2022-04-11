using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerRoomDisplayTab : MonoBehaviour
{
    public Text roomOwnerName;
    public Text numMembers;
    public Text publicAccess; //this defaults to public, unless it's by invite then private.
    public Button joinButton; //Join button is always active! by default!
    public Button rejectButton; //not active by default.
    string roomOwnerId; //idk maybe do a profile show with this? //wait this is good 

    public void SetRoomInfo(string roomOwnerName, int numMembers, bool isPublic, string roomOwnerId, bool isInvited, bool inInviteTab)
    {
        this.roomOwnerName.text = roomOwnerName;
        this.numMembers.text = numMembers.ToString();
        if (isPublic)
        {
            publicAccess.text = "Public"; //this basically show up in all tabs
        }
        else
        {
            publicAccess.text = "Private"; //this only show up in invite tab
        }
        this.roomOwnerId = roomOwnerId;

        //Usually public rooms only show in public, private rooms only show in invites. This is for specific room search only.
        if (!isPublic && !isInvited) joinButton.gameObject.SetActive(false);
        if (inInviteTab) rejectButton.gameObject.SetActive(true); //accept/reject an invite
    }

    public void UpdateNumMembers(int numMembers)
    {
        this.numMembers.text = numMembers.ToString();
    }
    //no need to update access scope, because it's set basically.

    public void UpdateRoomOwnerName(string newName) //someone could change name.
    {
        roomOwnerName.text = newName; //can add some variations here, like "'s room"
    }

    public void OnConnectPressed() //the objects below should be active by the time connect is pressed.
    {
        PersistentData.TRUEOWNERID_OF_JOINING_ROOM = roomOwnerId;

        Debug.Log("Connecting...");

        //Only connect forreal after everything is ready with callbacks n stuff
        GameObject.Find("PlayFabController").GetComponent<PhotonConnector>().DisconnectPlayer();
    }

    public void OnRejectPressed()
    {
        var rsps = GameObject.FindGameObjectWithTag("PlayerHUD").transform.Find("PlayerRoomSystem").GetComponent<RoomSystemPanelScript>();
        //tell rsps to remove it from the list
        rsps.listOfInvitedRoomIds.Remove(roomOwnerId); //remove it
        rsps.OnCheckInviteTabPressed(); //then update it
    }
}
