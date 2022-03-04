using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System;

public class SocialSystemScript : MonoBehaviour
{
    public GameObject playerObj;
    public cameraController playerCamera;
    public InGameUIController playerZoneTab;
    public PlayerMenuUIController UIController;
    public changeReceiver playerHud;

    public GameObject profileEditor;

    public GameObject outputFinal; //this is where img is stored on HUD, grab the image and put it in
    public Image targetProfileDisplayPic;

    public ContentChangerScript[] textChangers;

    //gonna centralize some controls here
    public GameObject addFriendSearchBar;
    public InputField friendsSearchBarInput;
    public GameObject requestsList;

    public GameObject playerInfoCard;

    PlayFabController PFC;
    PhotonChatManager PCM;

    public GameObject chatPanel; //this stores the prefab
    public GameObject msgViewPort; //this stores the local viewport
    public GameObject currentChatPanel = null;
    public Dictionary<string, GameObject> chatPanels = new Dictionary<string, GameObject>();
    public string message;
    public InputField messageField;
    public GameObject noChatOnSymbol;

    // Start is called before the first frame update
    void Awake() //awake is called before start, so it works ;D!!!!!!!!!!!!!!!!
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player"); 
        foreach (GameObject player in players)
        {
            if (player.GetComponent<PhotonView>().IsMine) //can also use GetComponent<playerController>().view.IsMine
            {
                playerObj = player;
                break;
            }
        }
        //playerObj = GameObject.FindGameObjectWithTag("Player");

        PFC = GameObject.Find("PlayFabController").GetComponent<PlayFabController>();
        PCM = GameObject.Find("PlayFabController").GetComponent<PhotonChatManager>();

        GameObject cameraController = GameObject.FindGameObjectWithTag("PlayerCamera");
        playerCamera = cameraController.GetComponent<cameraController>();
        UIController = cameraController.GetComponent<PlayerMenuUIController>();
        playerZoneTab = cameraController.GetComponent<InGameUIController>();
        playerHud = GameObject.FindGameObjectWithTag("PlayerHUD").transform.GetChild(0).GetComponent<changeReceiver>();

        NoCurrentChat();
        //OnTabOpen shouldbe auto called so chilling
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void closeWindow()
    {
        gameObject.SetActive(false); //want to keep data!
        CloseProfileEditor();
        if (!playerZoneTab.hasOneOn)
        {
            playerObj.GetComponent<playerController>().enabled = true;
            playerCamera.enabled = true;
            playerObj.GetComponent<playerController>().isMoving = false; //this line prevents the player from getitng stuck after
        }
        UIController.hasOneOn = false;

        //turn off all open stuff
        addFriendSearchBar.SetActive(false);
        friendsSearchBarInput.text = ""; //only clear at closing
        requestsList.SetActive(false);
        PFC.GetFriends();
    }

    public void OpenProfileEditor()
    {
        PFC.GetFriends();
        //targetProfileDisplayPic.sprite = outputFinal.GetComponent<Image>().sprite;
        profileEditor.transform.Find("Scroll View").GetComponent<ScrollRect>().verticalNormalizedPosition = 1; //resets position
        profileEditor.SetActive(true);
    }

    public void CloseProfileEditor()
    {
        PFC.GetFriends();
        profileEditor.SetActive(false);
        foreach (ContentChangerScript textChanger in textChangers)
        {
            textChanger.OnCancelButtonPressed(); //turn off name editing if happening
        }
    }

    //On tab open, updates current display pic with the current profile pic in HUD
    //thought this is more convenient, a more efficient method would be to update the PDP as soon as
    //the HUD image is changed, from avatar custom script. But i'm too lazy soo... plus it's w/e LOL

    //also grabs friends list from system
    //I think that refreshing friends list every time when tab starts might be inefficient?
    //we'll see if we need to transition into a better mode in the future: keep prev and just add new addition
    //The aboce can probably be achieved with a bool array to check if there's new friend
    //Need to edit GetFriends function to achieve that.
    public void OnTabOpen()
    {
        targetProfileDisplayPic.sprite = outputFinal.GetComponent<Image>().sprite;
        if (!playerZoneTab.hasOneOn) //prevents zone + UI
        {
            playerObj.GetComponent<playerController>().enabled = false;
            playerCamera.enabled = false;
            PFC.GetFriends();
        }
    }

    //Start not active, when leave set to not active
    //So when clicked once, !not = true, so active! click again then false.
    public void OnAddFriendPressed()
    {
        addFriendSearchBar.SetActive(!addFriendSearchBar.activeSelf);
        PFC.GetFriends();
    }

    public void OnRequestsListPressed()
    {
        requestsList.SetActive(!requestsList.activeSelf);
        //if(requestsList.activeSelf) PFC.GetFriends();
        PFC.GetFriends(); //GET UPDATE AS MUCH AS POSSSIBLEEEE
    }


    GameObject info;
    public void OnSearchFriendPressed()
    {
        PFC.GetFriends(); //lol gonna run it once here too
        Debug.Log("Searching for: " + friendsSearchBarInput.text);
        if (info != null) //object self destructs into null on tab close
        {
            Destroy(info.gameObject);
        }
        info = Instantiate(playerInfoCard, new Vector2(0, 0), Quaternion.identity); //can always use this to tune generation position/size
        info.GetComponent<PlayerInfoCardUpdater>().InitializeInfoCard(friendsSearchBarInput.text, 0); //search list
        //PFC.StartCloudSendFriendRequest(friendsSearchBarInput.text);
    }

    public void GetMessage(string input)
    {
        message = input;
    }

    public void SendPrivateMessage()
    {
        if (currentChatPanel != null)
        {
            DateTime sentTime = DateTime.UtcNow;
            PCM.chatClient.SendPrivateMessage(currentChatPanel.GetComponent<MsgContentController>().listing.playerID, 
                new string[] { message, sentTime.ToBinary().ToString()});
            currentChatPanel.GetComponent<MsgContentController>().AddMessage("You", 
                TimeZoneInfo.ConvertTimeBySystemTimeZoneId(sentTime, TimeZoneInfo.Local.Id).ToString(), message, true);
        }
        else Debug.Log("No one is selected.");
        messageField.text = "";
        message = "";
    }

    public void NoCurrentChat()
    {
        if (currentChatPanel == null) noChatOnSymbol.SetActive(true);
        else noChatOnSymbol.SetActive(false);
    }

    public void RemovePhotonChatFriend(string friendID)
    {
        PCM.chatClient.RemoveFriends(new string[] { friendID });
    }

    //this is sender's end, using Photon chat for insta feedback
    public void BroadcastFriendRemoval(string friendID)
    {
        PCM.chatClient.SetOnlineStatus(2, friendID); //ChatUserStatus.Online, 2+ //send a message id to person getting unfriended
        //In the future, can vary this message to create key real time chat effects!
    }
}

