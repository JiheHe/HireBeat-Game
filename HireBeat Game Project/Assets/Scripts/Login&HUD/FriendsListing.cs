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

    public GameObject playerInfoCard;
    public string type; //use this to tell the system which mode it's in

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

    GameObject info;
    public void OnProfileClicked(int type) //1 = friend list,  2 = request list
    {
        if (info != null) //object self destructs into null on tab close
        {
            Destroy(info.gameObject);
        }
        info = Instantiate(playerInfoCard, new Vector2(0, 0), Quaternion.identity); //can always use this to tune generation position/size
        info.GetComponent<PlayerInfoCardUpdater>().listingObject = gameObject; //binding
        if (type == 1) info.GetComponent<PlayerInfoCardUpdater>().InitializeInfoCard(playerID, 1); //friend list click
        else info.GetComponent<PlayerInfoCardUpdater>().InitializeInfoCard(playerID, 2); //request list click
        
    }
}
