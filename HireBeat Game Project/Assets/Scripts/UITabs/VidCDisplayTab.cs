using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VidCDisplayTab : MonoBehaviour
{
    public Text roomName;
    public Text numMembers;
    public Text publicAccess;
    public Button joinButton; //this is default to inactive.
    private string currOwnerID;

    //should I add a reference to vcs here?
    
    public void SetRoomInfo(string roomName, int numMembers, bool isPublic, string currOwnerID)
    {
        this.roomName.text = roomName;
        this.numMembers.text = numMembers.ToString();
        if (isPublic)
        {
            publicAccess.text = "Public";
            joinButton.gameObject.SetActive(true); //set active if the room is public.
        }
        else
        {
            publicAccess.text = "Private";
        }
        this.currOwnerID = currOwnerID;
    }

    public void UpdateNumMembers(int numMembers)
    {
        this.numMembers.text = numMembers.ToString();
    }

    public void UpdateCurrOwnerID(string currOwnerID)
    {
        this.currOwnerID = currOwnerID;
    }

    public void OnConnectPressed() //the objects below should be active by the time connect is pressed.
    {
        GameObject.FindGameObjectWithTag("PlayerHUD").transform.Find("VidCRoomSearch").GetComponent<VideoChatRoomSearch>().OnConnectPressed(roomName.text);
    }

    //roomNname and publicAccess stay fixed. 
}
