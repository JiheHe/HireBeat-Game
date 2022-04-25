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
    public PhotonChatManager PCM; //going to assign this directly from SceneListener, so need no to start sss to send messages.

    public GameObject chatPanel; //this stores the prefab
    public GameObject msgViewPort; //this stores the local viewport
    public ScrollRect msgScrollView;
    public GameObject currentChatPanel = null;
    public Dictionary<string, GameObject> chatPanels = new Dictionary<string, GameObject>();
    public string message;
    public InputField messageField;
    public GameObject noChatOnSymbol;

    public string currentPublicRoomChatName; //this is set upon room joining / connecting
    public GameObject publicRoomChatPanel; //same
    public bool isPrivate;

    public GameObject currentInfoCardOpened = null; //starts null, set throughout
    public GameObject lobbyInfoCardOpened = null; //i think making 2 info card aval is not bad? one in system, 1 in lobby

    public GameObject videoChatPanel;
    public RoomSystemPanelScript rsps; //put it here for convenience, directly-assigned.

    public Text errorMsg;
    IEnumerator errorMsgDisplay;

    // Start is called before the first frame update
    public void Initialize() //awake is called before start, so it works ;D!!!!!!!!!!!!!!!!
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
        //PCM = GameObject.Find("PlayFabController").GetComponent<PhotonChatManager>();

        GameObject cameraController = GameObject.FindGameObjectWithTag("PlayerCamera");
        playerCamera = cameraController.GetComponent<cameraController>();
        UIController = cameraController.GetComponent<PlayerMenuUIController>();
        playerZoneTab = cameraController.GetComponent<InGameUIController>();
        playerHud = GameObject.FindGameObjectWithTag("PlayerHUD").GetComponent<changeReceiver>();

        NoCurrentChat();
    }

    private void Start() //new scene, new ss, new list!
    {
        //PFC.GetFriends();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void closeWindow()
    {
        gameObject.SetActive(false); //want to keep data!
        CloseProfileEditor();
        //if (!playerZoneTab.hasOneOn)
        //{
        if (playerCamera != null) //null if destroyed! closed window due to kicked
        {
            playerCamera.enabled = true;
            if (!PersistentData.isMovementRestricted)
            {
                playerObj.GetComponent<playerController>().enabled = true;
                playerObj.GetComponent<playerController>().actionParem = (int)playerController.CharActionCode.IDLE; //this line prevents the player from getitng stuck after
            }
            //}
            UIController.hasOneOn = false;
        }

        //turn off all open stuff
        addFriendSearchBar.SetActive(false);
        ClearFriendSearch(); //only clear at closing
        errorMsg.gameObject.SetActive(false);
        requestsList.SetActive(false);
        if (currentInfoCardOpened != null) Destroy(currentInfoCardOpened);
        PFC.GetFriends();
        //voiceChatPanel.SetActive(false);
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

        //Two options: 1. set curr chat panel to null upon exiting. 2. upon on opening generating curr chat panel's info card (using 2 for now)
        if (currentChatPanel != null && currentChatPanel.GetComponent<MsgContentController>().listing != null) //if it's a private convo
        {
            currentChatPanel.GetComponent<MsgContentController>().listing.OnProfileClicked(1); //friends list
        }
    }

    //Start not active, when leave set to not active
    //So when clicked once, !not = true, so active! click again then false.
    public void OnAddFriendPressed()
    {
        if(addFriendSearchBar.activeSelf) //If it's active then you pressed it, then you are closing it basically
        {
            ClearFriendSearch();
        }
        addFriendSearchBar.SetActive(!addFriendSearchBar.activeSelf);
        PFC.GetFriends();
    }

    public void OnRequestsListPressed()
    {
        requestsList.SetActive(!requestsList.activeSelf);
        PFC.GetFriends(); //GET UPDATE AS MUCH AS POSSSIBLEEEE
    }


    public GameObject friendUserSearchDisplayPrefab; //prefab
    public RectTransform displayUserSearchResultsPanel; //parent.
    string input;
    public void OnSearchFriendPressed() //enter key
    {
        input = friendsSearchBarInput.text;
        string query = "SELECT UserName, UserId FROM UserDataStorage WHERE UserName = %input% OR UserId = %input% OR Email = %input%";
        SQL4Unity.SQLParameter parameters = new SQL4Unity.SQLParameter();
        parameters.SetValue("input", input);
        DataBaseCommunicator.Execute(query, OnSearchUserToFriendSubmitCallback, parameters);
    }
    void OnSearchUserToFriendSubmitCallback(SQL4Unity.SQLResult result)
    {
        if (result != null)
        {
            if (result.rowsAffected == 0)
            {
                //0 rows affected if nothing exists relating to the input.
                Debug.Log("The input you inputted does not exist!"); //show error msg to user.

                if (errorMsgDisplay != null) StopCoroutine(errorMsgDisplay); //"restart" coroutine
                errorMsgDisplay = DisplayErrorMessage(3f, "Cannot find an username, id, or email associated with \"" +
                    input + "\""); //each time a coro is called, a new obj is formed.
                StartCoroutine(errorMsgDisplay);
            }
            else
            {                //got something, regardless of its length.
                hirebeatprojectdb_userdatastorage[] userData = result.Get<hirebeatprojectdb_userdatastorage>();
                RemoveAllUserSearchResults(); //remove all previous results! 1-2 at max.

                foreach (var user in userData)
                {
                    string userName = user.UserName;
                    string userId = user.UserId;
                    var newPlayerSearchDisplay = Instantiate(friendUserSearchDisplayPrefab, displayUserSearchResultsPanel); //using same panel as parent
                    newPlayerSearchDisplay.GetComponent<IPTR_Simple>().SetUserInfo(userName, userId, false, this); //isOnline doesn't matter here.
                    newPlayerSearchDisplay.GetComponent<IPTR_Simple>().infoCardXShift = -243;
                }
            }
        }
        else
        {
            Debug.LogError("Error occured in search user to friend in social system");
        }
    }
    public void RemoveAllUserSearchResults() //can be used for the clear button as well
    {
        foreach (Transform child in displayUserSearchResultsPanel.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void ClearFriendSearch()
    {
        friendsSearchBarInput.text = "";
        RemoveAllUserSearchResults();
    }

    public IEnumerator DisplayErrorMessage(float time, string message)
    {
        errorMsg.gameObject.SetActive(true);
        errorMsg.text = message;
        yield return new WaitForSeconds(time);
        errorMsg.gameObject.SetActive(false);
    }


    /*public void OnSearchFriendPressed()
    {
        PFC.GetFriends(); //lol gonna run it once here too
        Debug.Log("Searching for: " + friendsSearchBarInput.text);



        if (currentInfoCardOpened == null) //object self destructs into null on tab close
        {
            GameObject info = Instantiate(playerInfoCard, new Vector2(0, 0), Quaternion.identity); //can always use this to tune generation position/size
            info.transform.GetChild(0).transform.localPosition = new Vector2(-243, 0); //shift x to the left, of this generated card
            info.GetComponent<PlayerInfoCardUpdater>().InitializeInfoCard(friendsSearchBarInput.text, 0); //search list
            currentInfoCardOpened = info;
        }
        else currentInfoCardOpened.GetComponent<PlayerInfoCardUpdater>().InitializeInfoCard(friendsSearchBarInput.text, 0); //search list
    }*/

    public void GetMessage(string input)
    {
        message = input;
    }

    public void SendChatMessage()
    {
        if (currentChatPanel != null)
        {
            if (isPrivate)
            {
                SendPrivateMessage();
            }
            else
            {
                SendPublicMessage();
            }
            messageField.text = "";
            message = "";
            messageField.Select(); //this blinks caret cuz select ;D (screen needs to be big enought to see caret... or make thicker)
            messageField.ActivateInputField(); //this makes sure that user can keep typing ;D
        }
        else Debug.Log("No chat is selected.");
        //messageField.text = ""; //don't wanna undo their stuff if they misselect lol
        //message = "";
    }

    private void SendPrivateMessage()
    {
        DateTime sentTime = DateTime.UtcNow;
        PCM.chatClient.SendPrivateMessage(currentChatPanel.GetComponent<MsgContentController>().listing.playerID,
            new string[] { message, sentTime.ToBinary().ToString() });
        currentChatPanel.GetComponent<MsgContentController>().AddMessage("You",
            TimeZoneInfo.ConvertTimeBySystemTimeZoneId(sentTime, TimeZoneInfo.Local.Id).ToString(), message, true, "0000");
    }

    private void SendPublicMessage()
    {
        DateTime sentTime = DateTime.UtcNow;
        string senderName = transform.parent.GetChild(2).GetChild(0).GetComponent<Text>().text; //I don't wanna set a string for this in social system.. grab from hud ;D
        PCM.chatClient.PublishMessage(currentPublicRoomChatName,
            new string[] { message, sentTime.ToBinary().ToString(), senderName, PFC.myID });
        currentChatPanel.GetComponent<MsgContentController>().AddMessage(senderName, //current chat panel should be the public room one, can put that in to safe check
            TimeZoneInfo.ConvertTimeBySystemTimeZoneId(sentTime, TimeZoneInfo.Local.Id).ToString(), message, true, "0000"); //why locally open yourself LOL
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

    //force a getfriend refresh on receiver's end
    public void RefreshReceiverFriendList(string friendID)
    {
        PCM.chatClient.SendPrivateMessage(friendID, "REFRESH LIST");
    }

    public void RefreshVoiceChatList(string userID)
    {
        PCM.chatClient.SendPrivateMessage(userID, "LEAVING VC");
    }

    public void UpdateVCUsernames(string userID)
    {
        PCM.chatClient.SendPrivateMessage(userID, "UPDATE VC NAMES");
    }

    public void AnnounceMeJoining(string userID)
    {
        PCM.chatClient.SendPrivateMessage(userID, "NEW VCP JOINED");
    }

    public void RequestVidCRoomInfo(string userID)
    {
        PCM.chatClient.SendPrivateMessage(userID, "REQSTING VCRM INFO");
    }

    public void SendVidCRoomInfo(string userID, string userIdsString)
    {
        PCM.chatClient.SendPrivateMessage(userID, "RECEI_VCRM_INFO" + userIdsString);
    }

    public void SendVidCInvite(string userID, string roomName)
    {
        PCM.chatClient.SendPrivateMessage(userID, "INVITE_TO_" + roomName);
    }

    public void SendUserRoomInvite(string userID)
    {
        //This is the safest way, grab directly from curr room you are in!
        string roomID = PhotonNetwork.CurrentRoom.Name.Substring("USERROOM_".Length); //Room name format is USERROOM_[userid], so...
        PCM.chatClient.SendPrivateMessage(userID, "RMINVT_TO_" + roomID);
    }

    public void CreatePublicRoomPanel(string publicRoomChatName)
    {
        //During room transitioning, destroy previous room's panel. no data caching for now
        if (publicRoomChatPanel != null)
        {
            chatPanels.Remove(this.currentPublicRoomChatName);
            Destroy(publicRoomChatPanel);
        }

        publicRoomChatPanel = Instantiate(chatPanel, msgViewPort.transform);
        publicRoomChatPanel.transform.SetParent(msgViewPort.transform, false);
        publicRoomChatPanel.GetComponent<RectTransform>().pivot = new Vector2(0, 0);
        publicRoomChatPanel.GetComponent<MsgContentController>().listing = null; //juust makin sure
        publicRoomChatPanel.SetActive(false);
        this.currentPublicRoomChatName = publicRoomChatName;
        chatPanels.Add(publicRoomChatName, publicRoomChatPanel);
    }

    public void OnPublicChatRoomClicked()
    {
        isPrivate = false;
        if (currentChatPanel != null) currentChatPanel.SetActive(false);
        publicRoomChatPanel.SetActive(true);
        //reset scroll view, will do later.
        currentChatPanel = publicRoomChatPanel;
        msgScrollView.content = publicRoomChatPanel.GetComponent<RectTransform>();
        Destroy(currentInfoCardOpened);
        currentInfoCardOpened = null;
        NoCurrentChat();
    }

    public void OnVideoChatPanelOpenClicked() //this will involve more decisions in the future.
    {
        if (videoChatPanel.GetComponent<VideoChatRoomSearch>().vCC == null) //destroyed and not assigned one
        {
            videoChatPanel.SetActive(true);
        }
        else
        {
            videoChatPanel.GetComponent<VideoChatRoomSearch>().vCC.gameObject.SetActive(true);
        }
    }
}

