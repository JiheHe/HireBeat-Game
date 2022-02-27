using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FriendsListing : MonoBehaviour
{
    public Text playerName;
    //I don't think there's a need to show avatar again. Make it into a hoverable
    //field that displays the card thingy to the left when you hover over it
    public Image playerAvatar;
    public Image playerOnStatus;
    public string playerID;
    public PlayFabController PFC; 
    //they are assigned at instantiation

    public void OnRequestAccept()
    {
        PFC.StartCloudAcceptFriendRequest(playerID);
        Destroy(gameObject);
    }

    public void OnRequestDeny()
    {
        PFC.StartCloudDenyFriendRequest(playerID);
        Destroy(gameObject);
    }
}
