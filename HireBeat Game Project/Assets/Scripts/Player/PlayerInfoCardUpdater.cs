using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using PlayFab;
using PlayFab.ClientModels;

public class PlayerInfoCardUpdater : MonoBehaviour
{
    GameObject eventController;

    string pfpImage;
    string acctName;
    string acctSignature;
    string acctID;

    public Text username;
    public Text signature;
    public Image profileImg;
    public Text uniqueID;

    public GameObject loadingImage;

    public GameObject buttonController;
    public GameObject friendRemovalConfirmationTab;

    public int type; //0 = lobby click / search bar result, 1 = friend list,  2 = request list
    PlayFabController PFC;

    public GameObject listingObject; //this is the user tab object that summons playerinfo card, only set when there's one

    public void Start()
    {
        PFC = GameObject.Find("PlayFabController").GetComponent<PlayFabController>();
        PFC.GetFriends(); //start off by updating friends' list
        eventController = GameObject.FindGameObjectWithTag("PlayerCamera");
    }

    //Call the function below after prefab instantiation to update info, with PlayFabID
    public void InitializeInfoCard(string PlayFabID, int buttonType) //start off by playing loading scene
    {
        loadingImage.SetActive(true);
        type = buttonType;
        PlayFabClientAPI.GetUserData(new GetUserDataRequest()
        {
            PlayFabId = PlayFabID,
            Keys = new List<string>() { "pfpImage", "acctName", "acctSignature", "acctID" } //can get more in the future
            //else giving it a string returns specific, respective data
        }, SetCardProperties, OnUserDataFailed);
    }

    private void SetCardProperties(GetUserDataResult result)
    {
        if (result.Data == null)
        {
            Debug.Log("result is null");
        }
        else
        {
            //could be empty, but has to exist to not error
            pfpImage = result.Data["pfpImage"].Value;
            acctName = result.Data["acctName"].Value;
            acctSignature = result.Data["acctSignature"].Value;
            acctID = result.Data["acctID"].Value;

            //do button addition here
            ConfigureTopButton();

            if (SetProfileSprite(pfpImage))
            {
                SetUsername(acctName);
                SetSignature(acctSignature);
                SetUniqueID(acctID);
                loadingImage.SetActive(false);
            }
        }
    }

    void OnUserDataFailed(PlayFabError error)
    {
        Debug.LogError(error.GenerateErrorReport());
    }

    public void CloseTab()
    {
        PFC.GetFriends();
        onPointerOut();
        Destroy(gameObject);
    }

    private void SetUsername(string newUsername)
    {
        username.text = newUsername;
    }

    private void SetSignature(string newSignature)
    {
        signature.text = newSignature;
    }

    //this process takes the longest, so can use it as end condition
    private bool SetProfileSprite(string newPfpImg) //encoded in PNG
    {
        byte[] pfpByteArr = Convert.FromBase64String(newPfpImg);
        Texture2D myTexture = new Texture2D(1, 1, TextureFormat.RGB24, false, true); //I don't think size matters
        myTexture.LoadImage(pfpByteArr);
        Sprite spriteImg = Sprite.Create(myTexture, new Rect(0, 0, myTexture.width, myTexture.height), new Vector2(0.5f, 0.5f));
        profileImg.sprite = spriteImg;
        return true; //ready!
    }

    private void SetUniqueID(string newID)
    {
        uniqueID.text = "Unique ID: " + newID;
    }

    //PFC is local, so PFC.myID is my ID!
    private void ConfigureTopButton()
    {
        PFC.GetFriends();
        PlayFabClientAPI.GetFriendsList(new GetFriendsListRequest
        {
            IncludeSteamFriends = false,
            IncludeFacebookFriends = false,
            XboxToken = null
        }, result => {
            List<FriendInfo> friends = result.Friends;
            switch (type)
            {
                case 0: //lobby click or search bar result, add/already/req sent
                    if (friends != null) //this is updated, due to get friend call in the beginning
                    {
                        bool foundButton = false;
                        foreach (FriendInfo f in friends) //check if the user is your friend
                        {
                            if (acctID == f.FriendPlayFabId) //he's either your friend, a requester, or a requestee
                            {
                                switch (f.Tags[0])
                                {
                                    case "confirmed": //actual friends!
                                        SetButtonType(2); //so already friend
                                        foundButton = true;
                                        break;
                                    case "requester": //he's the requester, you are the requestee, so he's trying to add you!
                                        SetButtonType(0); //just do add friend, doesn't matter
                                        foundButton = true;
                                        break;
                                    case "requestee": //you have sent him a request before, but no response yet
                                        SetButtonType(3); //request sent :)
                                        foundButton = true;
                                        break;
                                }
                            }
                            if (foundButton) break;
                        }
                        //if there's no relationship between two of you, then add friend only option
                        if (!foundButton && acctID != PFC.myID) SetButtonType(0); //and it is not yourself
                    }
                    else
                    {
                        if (acctID != PFC.myID) SetButtonType(0); //add friend if you have no friends... obvious
                    }
                    break;
                case 1: //friend list, remove friend only
                    SetButtonType(1);
                    break;
                case 2: //request list, show nothing
                    break;
            }
        }, DisplayPlayFabError);
    }

    private void SetButtonType(int buttonIndex) //Set a button active!
    {
        // Request viewer ones should be plain, no buttons
        // in room click ones and search friend ones should have add or already friend
        // friend list ones should havee remove
        //0 = Add Friend, 1 = Remove Friend, 2 = Already Friend, 3 = Request Sent
        buttonController.transform.GetChild(buttonIndex).gameObject.SetActive(true);
    }

    //before send, check for possible relationships?
    //Before add, check for whether the person you are requesting is already requesting you
    //if a person sends a request, then presses cancel before two way add friend goes through, then cancel takes pres...
    //because in this case, two way just became one way, so.... :(
    public void OnAddFriendPressed() //This is NOT accept! Just a friend request
    {
        PFC.GetFriends();
        PlayFabClientAPI.GetFriendsList(new GetFriendsListRequest
        {
            IncludeSteamFriends = false,
            IncludeFacebookFriends = false,
            XboxToken = null
        }, result => {
            List<FriendInfo> friends = result.Friends;
            foreach (FriendInfo f in friends)
            {
                if (f.FriendPlayFabId == acctID && f.Tags[0] == "requester") //if they are already related, then just change tags!
                {
                    PFC.StartCloudAcceptFriendRequest(acctID);
                    buttonController.transform.GetChild(0).gameObject.SetActive(false); //turn off add friend button
                    SetButtonType(2);
                    return;
                }
            }

            PFC.StartCloudSendFriendRequest(acctID);
            buttonController.transform.GetChild(0).gameObject.SetActive(false); //turn off add friend button
            SetButtonType(3); //turn on request sent button to show user success
        }, DisplayPlayFabError);
    }

    void DisplayPlayFabError(PlayFabError error)
    {
        Debug.Log(error.GenerateErrorReport());
    }

    public void OnRemoveFriendPressed() //gonna do a confirmation first, then remove, but gonna direct now for testing purpose
    {
        friendRemovalConfirmationTab.transform.GetChild(2).GetComponent<Text>().text = "Username: " + acctName;
        friendRemovalConfirmationTab.SetActive(true);
    }

    //before send, check for possible relationships?
    //No need for this one. If player B removes A and A removes B at same time, then bad things won't happen.
    public void OnConfirmRemovalPressed()
    {
        if(listingObject != null)
        {
            var chatPanelz = GameObject.FindGameObjectWithTag("PlayerHUD").transform.Find("SocialSystem").
                        GetComponent<SocialSystemScript>();
            Destroy(chatPanelz.chatPanels[listingObject.GetComponent<FriendsListing>().playerID]); //this is just for faster local visual
            chatPanelz.chatPanels.Remove(listingObject.GetComponent<FriendsListing>().playerID);
            chatPanelz.currentChatPanel = null; //don't null, turn to next friend
            if (chatPanelz.chatPanels.Count > 0)
            {
                GameObject.FindGameObjectWithTag("PlayerHUD").GetComponent<changeReceiver>().friendsList.GetChild(1).gameObject.GetComponent<FriendsListing>().OnProfileClicked(1); //go to next friend
                //get 1 because 0th child is destroyed after            
            } //if there are still friends, then turn to next panel
            chatPanelz.NoCurrentChat();
            //Remove does not throw if the key is not found (only if the key is null). If the key is not in the dictionary then it returns false. 
            //So no need to worry about not clicking on friend tab to instantiate the pair before removal = null
            Destroy(listingObject); //"unfriended"
        }
        PFC.StartCloudDenyFriendRequest(acctID);
        CloseTab();
        PFC.GetFriends();
    }

    public void OnCancelRemovalPressed()
    {
        friendRemovalConfirmationTab.SetActive(false);
        PFC.GetFriends();
    }

    public void onPointerIn()
    {
        eventController.GetComponent<cameraController>().enabled = false;
    }

    public void onPointerOut()
    {
        eventController.GetComponent<cameraController>().enabled = true;
    }

}
