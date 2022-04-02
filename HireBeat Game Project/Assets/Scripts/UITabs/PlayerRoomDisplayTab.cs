using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerRoomDisplayTab : MonoBehaviour
{
    public Text roomOwnerName;
    public Text numMembers;
    public Text publicAccess; //this defaults to public, unless it's by invite then private.
    //Join button is always active! by default!
    string roomOwnerId; //idk maybe do a profile show with this?

    public void SetRoomInfo(string roomOwnerName, int numMembers, bool isPublic, string roomOwnerId)
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
    }

    public void UpdateNumMembers(int numMembers)
    {
        this.numMembers.text = numMembers.ToString();
    }
    //no need to update access scope, because it's set basically.

    public void OnConnectPressed() //the objects below should be active by the time connect is pressed.
    {
        //GameObject.FindGameObjectWithTag("PlayerHUD").transform.Find("VidCRoomSearch").GetComponent<VideoChatRoomSearch>().OnConnectPressed(roomName.text);
        Debug.Log("Connecting...");    
    }
}
