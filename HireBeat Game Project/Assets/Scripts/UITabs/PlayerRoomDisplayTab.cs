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
        //Normally, in other cases, you are good to join. But this one careful: before join, 
        //check if room is private and you are not invited (else you can't join)
        //No need for a parameter, just make a ez string call LOL
        string query = "SELECT IsRoomPublic FROM UserDataStorage WHERE UserId = \'" + roomOwnerId + "\'";
        DataBaseCommunicator.Execute(query, OnConnectedPressedCallback);
        //Also invites won't last like this if the list of invites is kept in rsps as it resets
        //upon entering a new room, so better to keep it in somewhere permanent... (not database-level perm,
        //but like per session at least... PersistentData!). //Yep I did. The one in rsps is a ref to pd's.

        //Make sure this logic also applies to video chats!
    }
    public void OnConnectedPressedCallback(SQL4Unity.SQLResult result)
    {
        Debug.Log("Connected press callback received!");
        if(result != null)
        {
            bool isRoomPublic = result.Get<hirebeatprojectdb_userdatastorage>()[0].IsRoomPublic;
            var rsps = GameObject.FindGameObjectWithTag("PlayerHUD").transform.Find("PlayerRoomSystem").GetComponent<RoomSystemPanelScript>();
            if (!isRoomPublic && !PersistentData.listOfInvitedRoomIds.Contains(roomOwnerId) && roomOwnerId != rsps.myID) 
            {
                //if the room is private and you are not invted, then...
                if (rsps.errorMsgDisplay != null) StopCoroutine(rsps.errorMsgDisplay); //"restart" coroutine
                rsps.errorMsgDisplay = rsps.DisplayErrorMessage(3f, "Unfortunately, the room has been set to private " +
                    "and you are not invited."); //each time a coro is called, a new obj is formed.
                rsps.StartCoroutine(rsps.errorMsgDisplay); 
                //Coroutines stop automatically if the object they are attached to is destroyed, hence why we can't start it here.

                rsps.OnRefreshButtonPressed(); //display an error message, then force a refresh!
                return;
            }
            else //either room is Public, or you are invited, or your room!!
            {
                //If you are invited... then by pressing connected you should remove that invite?
                //Else it'll be like a perm. key lol
                if (PersistentData.listOfInvitedRoomIds.Contains(roomOwnerId)) {
                    PersistentData.listOfInvitedRoomIds.Remove(roomOwnerId); //can directly operate on PD too! same list I believe.
                } //gonna consume the invitation here instead of on room joined, hopefully worth it...

                //The actual connection step:
                PersistentData.TRUEOWNERID_OF_JOINING_ROOM = roomOwnerId;

                Debug.Log("Connecting...");

                //Only connect forreal after everything is ready with callbacks n stuff
                GameObject.Find("PlayFabController").GetComponent<PhotonConnector>().DisconnectPlayer();
            }
        }
        else
        {
            Debug.LogError("Error retrieving room publicity!");
        }
    }

    public void OnRejectPressed()
    {
        var rsps = GameObject.FindGameObjectWithTag("PlayerHUD").transform.Find("PlayerRoomSystem").GetComponent<RoomSystemPanelScript>();
        //tell rsps to remove it from the list
        rsps.listOfInvitedRoomIds.Remove(roomOwnerId); //remove it
        rsps.OnCheckInviteTabPressed(); //then update it
    }
}
