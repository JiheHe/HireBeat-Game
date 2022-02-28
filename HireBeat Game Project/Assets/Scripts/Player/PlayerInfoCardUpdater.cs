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
        switch (type)
        {
            case 0: //lobby click or search bar result, add/already/req sent
                if (PFC.myFriends != null) //this is updated, due to get friend call in the beginning
                {
                    bool foundButton = false;
                    foreach (FriendInfo f in PFC.myFriends) //check if the user is your friend
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
                    }
                    //if there's no relationship between two of you, then add friend only option
                    if(!foundButton && acctID != PFC.myID) SetButtonType(0); //and it is not yourself
                }
                else
                {
                    if(acctID != PFC.myID) SetButtonType(0); //add friend if you have no friends... obvious
                }
                break;
            case 1: //friend list, remove friend only
                SetButtonType(1);
                break;
            case 2: //request list, show nothing
                break;
        }
    }

    private void SetButtonType(int buttonIndex) //Set a button active!
    {
        // Request viewer ones should be plain, no buttons
        // in room click ones and search friend ones should have add or already friend
        // friend list ones should havee remove
        //0 = Add Friend, 1 = Remove Friend, 2 = Already Friend, 3 = Request Sent
        buttonController.transform.GetChild(buttonIndex).gameObject.SetActive(true);
    }

    public void OnAddFriendPressed() //This is NOT accept! Just a friend request
    {
        PFC.StartCloudSendFriendRequest(acctID);
        buttonController.transform.GetChild(0).gameObject.SetActive(false); //turn off add friend button
        SetButtonType(3); //turn on request sent button to show user success
        PFC.GetFriends();
    }

    public void OnRemoveFriendPressed() //gonna do a confirmation first, then remove, but gonna direct now for testing purpose
    {
        PFC.StartCloudDenyFriendRequest(acctID);
        Destroy(listingObject); //"unfriended"
        CloseTab();
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