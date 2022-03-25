using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VidCDisplayTab : MonoBehaviour
{
    public Text roomName;
    public Text numMembers;
    public Text publicAccess;
    private string currOwnerID;

    //should I add a reference to vcs here?
    
    public void SetRoomInfo(string roomName, int numMembers, bool isPublic, string currOwnerID)
    {
        this.roomName.text = roomName;
        this.numMembers.text = numMembers.ToString();
        if (isPublic) publicAccess.text = "Public";
        else publicAccess.text = "Private";
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

    //roomNname and publicAccess stay fixed. 
}
