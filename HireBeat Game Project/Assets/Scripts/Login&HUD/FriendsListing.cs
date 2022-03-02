using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab.ClientModels;
using PlayFab;

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

    public Image status;
    public bool isOnline = false;

    //Some brainstormed scenarios (same tab delay, just in case... this is only possible in FriendsListing):
    //User A sends a request to B, user B declines, then A cancels (done)
    //User A sends a request to B, user B accepts, then A cancels (then in this case, cancel should NOT go through)
    //User A sends a request to B, user A cancels, then B accepts (then in this case, accept should go through)
    //So accepts have power over cancel! so before cancel, check if they are friends (if they are then don't cancel). 
    //(if two players are already friends, then a friend request won't be generated (connections)

    public void OnRequestAccept()
    {
        PlayFabClientAPI.GetFriendsList(new GetFriendsListRequest
        {
            IncludeSteamFriends = false,
            IncludeFacebookFriends = false,
            XboxToken = null
        }, result => {
            List<FriendInfo> friends = result.Friends;
            foreach (FriendInfo f in friends) //_friends is the most updated list, then it's data is being thrown into display friends.
            {
                if (f.FriendPlayFabId == playerID) //if they are already friends, then nice! just need to change tags to accept
                {
                    Destroy(gameObject);
                    PFC.StartCloudAcceptFriendRequest(playerID);
                    return;
                }
            }
            //else FORCE THEM TO BE FRIENDS >:), first by connecting them and change their tags to confirmed! (another cloud script)
            Destroy(gameObject);
            PFC.StartCloudAddAndAcceptFriendRequest(playerID);
        }, DisplayPlayFabError);
    }

    public void OnRequestDeny()
    {
        PlayFabClientAPI.GetFriendsList(new GetFriendsListRequest
        {
            IncludeSteamFriends = false,
            IncludeFacebookFriends = false,
            XboxToken = null
        }, result => {
            List<FriendInfo> friends = result.Friends;
            foreach(FriendInfo f in friends) 
            {
                if (f.FriendPlayFabId == playerID && f.Tags[0] == "confirmed")
                {
                    Destroy(gameObject); //this is important! GetFriends check for these
                    PFC.StartCloudAcceptFriendRequest(playerID); //this should just serve as a local list updater
                    return;
                }
            }
            //else
            Destroy(gameObject);
            PFC.StartCloudDenyFriendRequest(playerID);
        }, DisplayPlayFabError);
    }

    void DisplayPlayFabError(PlayFabError error)
    {
        Debug.Log(error.GenerateErrorReport());
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

    public void changeOnStatus(bool isOn)
    {
        if(isOn)
        {
            isOnline = true;
            status.color = new Color32(67, 180, 106, 255);
        }
        else
        {
            isOnline = false;
            status.color = new Color32(216, 99, 42, 255);
        }
    }
}
