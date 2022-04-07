using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using System.Linq;

public class RoomSystemPanelScript : MonoBehaviour
{
    [HideInInspector] public GameObject playerObj;
    [HideInInspector] public cameraController playerCamera;
    [HideInInspector] public InGameUIController playerZoneTab;
    [HideInInspector] public PlayerMenuUIController UIController;
    [HideInInspector] public changeReceiver playerHud;

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

        GameObject cameraController = GameObject.FindGameObjectWithTag("PlayerCamera");
        playerCamera = cameraController.GetComponent<cameraController>();
        UIController = cameraController.GetComponent<PlayerMenuUIController>();
        playerZoneTab = cameraController.GetComponent<InGameUIController>();
        playerHud = GameObject.FindGameObjectWithTag("PlayerHUD").transform.GetChild(0).GetComponent<changeReceiver>();
    }

    string myID;

    public GameObject roomInfoPanel;
    public Text roomNameTxt;
    public Text numPlayersInRoomTxt;
    public Text roomAccessTxt; //this is from database
    public InputField searchRoomBar;
    public GameObject ownRoomSettingsPanel;
    public Toggle sortByNumPlayers; //if this is true by toggling, then...
    public Toggle sortByAlphanumeric;
    public Toggle selfRoomPublicStatus;
    public InputField searchUserBar;

    public Text errorMsg;
    private IEnumerator errorMsgDisplay;

    [Serializable]
    public class PlayerRoomInfo //roomName is the key
    {
        public string roomName; //this is useless
        public int numPlayersInRoom;
        public bool isPublic; //this  might be useless too
        public PlayerRoomDisplayTab roomDisplayTab;

        public PlayerRoomInfo(string roomName, int numPlayersInRoom, bool isPublic, PlayerRoomDisplayTab roomDisplayTab)
        {
            this.roomName = roomName;
            this.numPlayersInRoom = numPlayersInRoom;
            this.isPublic = isPublic;
            this.roomDisplayTab = roomDisplayTab;
        }
    }

    //This quick struct stores player-only information, as database grabs from it
    public struct QuickRoomInfo //just contains roomName and isPublic
    {
        public string roomName;
        public bool isPublic;

        public QuickRoomInfo(string roomName, bool isPublic)
        {
            this.roomName = roomName;
            this.isPublic = isPublic;
        }
    }

    // Each room is identified here by id!
    public Dictionary<string, PlayerRoomInfo> playerRoomList = new Dictionary<string, PlayerRoomInfo>(); //a cache for userName, rest of info
    public GameObject playerRoomDisplayPrefab; //this is the prefab for each player room display in list
    public RectTransform playerRoomDisplayPanel; //this is the content where room will be child of.
    public List<string> listOfInvitedRoomIds = new List<string>(); //can be invited by public or private! private can only join through invite tho
    public GameObject playerSearchDisplayPrefab; //this is the prefab for each user display in search results
    public List<InvitePlayerToRoomTab> playerTabsOnDisplay = new List<InvitePlayerToRoomTab>();

    DataBaseCommunicator dbc = null; //the real time database!
    // Start is called before the first frame update
    void Start()
    {
        dbc = GameObject.FindGameObjectWithTag("DataCenter").GetComponent<DataBaseCommunicator>();
        dbc.GrabAllPublicRooms(0);

        myID = GameObject.Find("PersistentData").GetComponent<PersistentData>().acctID;
    }

    public void OnEnable() //called everytime when panel gets active
    {
        //User can keep its previous preference, just a simple refresh!
        if (dbc != null) dbc.GrabAllPublicRooms(0); //need this because at obj first init, dbc not assigned yet, so null error. But in future can.
    }

    // Update is called once per frame
    void Update()
    {
    }

    int previousOn = 0; //1 is num, 2 is alpha, 0 is none.
    //Since it's a toggle group, on value change both calls will be triggered!
    //Will try condense in future.
    public void OnSortByNumToggleChanged()
    {
        if (sortByNumPlayers.isOn)
        {
            Debug.Log("Starting to sort by num");
            dbc.GrabAllPublicRooms(1);
            previousOn = 1;
        }
        else if(!sortByNumPlayers.isOn && !sortByAlphanumeric.isOn && previousOn == 1)
        {
            dbc.GrabAllPublicRooms(3);
            previousOn = 0;
        }
    }

    public void OnSortByAlphaToggleChanged()
    {
        if (sortByAlphanumeric.isOn)
        {
            Debug.Log("Starting to sort by alpha");
            dbc.GrabAllPublicRooms(2);
            previousOn = 2;
        }
        else if (!sortByNumPlayers.isOn && !sortByAlphanumeric.isOn && previousOn == 2)
        {
            dbc.GrabAllPublicRooms(3); //3 initializes the room list destruction system.
            previousOn = 0;
        }
    }
    //This is used to create the effect that you've exited a sort mode


    Dictionary<string, QuickRoomInfo> userIdToQRICache;
    Dictionary<string, string> userIdToUserNameCacheForPlayerSearch; //I'm too scared to reuse userIdToUserNameCache later below...;
    //This method is the callback from dbc once you've asked to grab room(user) search results.
    public void StoreInputSearchResults(hirebeatprojectdb_userdatastorage[] userData, string cmd, string input = null)
    {
        if(userData == null) //speical value for not found or error
        {
            if (errorMsgDisplay != null) StopCoroutine(errorMsgDisplay); //"restart" coroutine
            errorMsgDisplay = DisplayErrorMessage(3f, "Cannot find an username, id, or email associated with \"" +
                input + "\""); //each time a coro is called, a new obj is formed.
            StartCoroutine(errorMsgDisplay);
        }
        else
        {
            switch (cmd)
            {
                case "rsps": //searching for a room
                    userIdToQRICache = userData.ToDictionary(r => r.UserId, r => new QuickRoomInfo(r.UserName, r.IsRoomPublic));
                    dbc.GrabAllRoomInfoFromGivenIds(userIdToQRICache.Keys.Distinct().ToArray()); //removes duplicate!
                    //just in case grabbing someone whose name == id twice. Diff row diff id.
                    break;
                case "invplayer": //searching for a player
                    userIdToUserNameCacheForPlayerSearch = userData.ToDictionary(r => r.UserId, r => r.UserName);
                    dbc.GrabAllUserStatusFromGivenIds(userIdToUserNameCacheForPlayerSearch.Keys.Distinct().ToArray());
                    break;
            }
        }
    }
    //This method is the callback from dbc to grab all room info from given list of ids, above
    public void DisplayInputSearchResults(hirebeatprojectdb_userroomsassociated[] dbRooms)
    {
        //Delete everything
        foreach (var roomTabObj in playerRoomList.Values.Select(i => i.roomDisplayTab.gameObject))
        {
            Destroy(roomTabObj);
        }
        playerRoomList.Clear();
        foreach (var playerTabObj in playerTabsOnDisplay.Select(i => i.gameObject))
        {
            Destroy(playerTabObj);
        }
        playerTabsOnDisplay.Clear();

        foreach (var room in dbRooms)
        {
            //If is not invited, then set join button to false. (join button default to true). very easy
            //No need to order! just 1-2 rooms anyway. (just in case if same username as someone's id)
            var QRIObj = userIdToQRICache[room.TrueOwnerID];
            bool isInvited = listOfInvitedRoomIds.Contains(room.TrueOwnerID);  
            AddNewRoomToList(QRIObj.roomName, room.TrueOwnerID, room.NumPlayersInRoom, QRIObj.isPublic, isInvited);
        }
    }

    //This method is the callback from dbc after grabbing user online status from a list of ids
    public void DisplayUserStatusResults(List<string> idsOnline)
    {
        //Delete everything
        foreach (var roomTabObj in playerRoomList.Values.Select(i => i.roomDisplayTab.gameObject))
        {
            Destroy(roomTabObj);
        }
        playerRoomList.Clear();
        foreach (var playerTabObj in playerTabsOnDisplay.Select(i => i.gameObject))
        {
            Destroy(playerTabObj);
        }
        playerTabsOnDisplay.Clear();

        foreach (var user in userIdToUserNameCacheForPlayerSearch)
        {
            bool isOnline = idsOnline.Contains(user.Key);
            AddNewUserToList(user.Value, user.Key, isOnline);
        }
    }

    IEnumerator DisplayErrorMessage(float time, string message)
    {
        errorMsg.gameObject.SetActive(true);
        errorMsg.text = message;
        yield return new WaitForSeconds(time);
        errorMsg.gameObject.SetActive(false);
    }

    private void AddNewUserToList(string userName, string userId, bool isOnline)
    {
        var newPlayerSearchDisplay = Instantiate(playerSearchDisplayPrefab, playerRoomDisplayPanel); //using same panel as parent
        newPlayerSearchDisplay.GetComponent<InvitePlayerToRoomTab>().SetUserInfo(userName, userId, isOnline);
        playerTabsOnDisplay.Add(newPlayerSearchDisplay.GetComponent<InvitePlayerToRoomTab>());
    }

    private void AddNewRoomToList(string roomName, string ownerID, int numMembers, bool isPublic, bool isInvited = false) //this uses roomName
    {
        var newPlayerRoomDisplay = Instantiate(playerRoomDisplayPrefab, playerRoomDisplayPanel);
        //newVCRoomDisplay.name = prefix + roomName; //no need for prefix...
        //if (invitedRoomList.Contains(roomName)) newVCRoomDisplay.GetComponent<VidCDisplayTab>().SetRoomInfo(roomName, numMembers, isPublic, currOwnerID, true); //invited!
        newPlayerRoomDisplay.GetComponent<PlayerRoomDisplayTab>().SetRoomInfo(roomName, numMembers, isPublic, ownerID, isInvited);
        playerRoomList.Add(ownerID, new PlayerRoomInfo(roomName, numMembers, isPublic, newPlayerRoomDisplay.GetComponent<PlayerRoomDisplayTab>()));
    }

    private void UpdateRoomInfo(string roomId, string roomName, int numMembers) 
    {
        var info = playerRoomList[roomId]; //I think this is by reference eh?
        if (info.numPlayersInRoom != numMembers)
        {
            info.numPlayersInRoom = numMembers;
            info.roomDisplayTab.UpdateNumMembers(numMembers);
        }
        if (info.roomName != roomName)
        {
            info.roomName = roomName;
            info.roomDisplayTab.UpdateRoomOwnerName(roomName);
        }
        //if (invitedRoomList.Contains(roomName)) info.roomDisplayTab.UpdateJoinAccess(true);
    }

    //callback from dbc, now need to get username to match the ids
    Dictionary<string, string> userIdToUserNameCache;
    public void GrabAllPublicRoomInfo(hirebeatprojectdb_userdatastorage[] userInfo, int sortType)
    {
        userIdToUserNameCache = userInfo.ToDictionary(r=>r.UserId, r=>r.UserName); //all public rooms! no need to record.
        string[] idsThatNeedRoomInfo = userIdToUserNameCache.Keys.ToArray();

        dbc.GrabAllUsernamesFromGivenIds(idsThatNeedRoomInfo, sortType);
    }

    //The below should hopefully be the callback from name grab above, from dbc.
    //Need to check playerRoomList room names against the data base ver.: if in room and not in data base then remove, if in data base and
    //not in room then add, if in both then update.
    public void UpdatePlayerRoomList(Dictionary<string, int> userIdToNumPlayersInRm, int sortType)
    {
        //First, check if there are still tabs from player search. If yes then bye bye
        if(playerTabsOnDisplay.Count != 0)
        {
            foreach (var playerTabObj in playerTabsOnDisplay.Select(i => i.gameObject))
            {
                Destroy(playerTabObj);
            }
            playerTabsOnDisplay.Clear();
        }

        //Is doing the below more efficient than two nested forloops?
        if (sortType == 1) //toggle group!
        {
            Debug.Log("Sort by num players");
            //no need for update/delete/add! Delete everything, sort, and redo!
            /*List<string> allRoomNames = dbRooms.Select(r => r.UserName).ToList();
            List<int> allNumPInRm = dbRooms.Select(r => r.NumPlayersInRoom).ToList();
            var result = allRoomNames.Zip(allNumPInRm, (rm, num) => new Tuple<string, int>(rm, num)).ToList();
            result.Sort((t1, t2) => t2.Item2 - t1.Item2); //hopefully this sorts from max to min.

            //Delete everything
            foreach (var roomTabObj in playerRoomList.Values.Select(i => i.roomDisplayTab.gameObject))
            {
                Destroy(roomTabObj);
            }
            playerRoomList.Clear();

            //Readd in order from largest to smallest, hopefully
            foreach (string roomName in result.Select(t => t.Item1))
            {
                var newInfo = dbRoomsConverted[roomName];
                AddNewRoomToList(roomName, newInfo.userId, newInfo.numPlayersInRoom, newInfo.isPublic);
            }*/

            //Fast way: order by directly from SQL call
            //Delete everything
            foreach (var roomTabObj in playerRoomList.Values.Select(i => i.roomDisplayTab.gameObject))
            {
                Destroy(roomTabObj);
            }
            playerRoomList.Clear();

            //Directly go element by element from userId to NumPlayers, because everything in it is sorted to num
            foreach (var idNumPair in userIdToNumPlayersInRm) 
            {
                AddNewRoomToList(userIdToUserNameCache[idNumPair.Key], 
                    idNumPair.Key, idNumPair.Value, true); //public indeed
            }
        }
        else if(sortType == 2)
        {
            Debug.Log("Sort by alphabet");
            /*List<string> allRoomNames = dbRooms.Select(r => r.UserName).ToList();
            allRoomNames.Sort((n1, n2) => n1.CompareTo(n2)); //sorts from a-z and num etc

            //Delete everything
            foreach (var roomTabObj in playerRoomList.Values.Select(i => i.roomDisplayTab.gameObject))
            {
                Destroy(roomTabObj);
            }
            playerRoomList.Clear();

            //Readd in order from largest to smallest, hopefully
            foreach (string roomName in allRoomNames)
            {
                var newInfo = dbRoomsConverted[roomName];
                AddNewRoomToList(roomName, newInfo.userId, newInfo.numPlayersInRoom, newInfo.isPublic);
            }*/

            //Fast way: order by directly from SQL call
            //Delete everything
            foreach (var roomTabObj in playerRoomList.Values.Select(i => i.roomDisplayTab.gameObject))
            {
                Destroy(roomTabObj);
            }
            playerRoomList.Clear();

            //Directly go element by element from userIdToUserNameCache, because everything in it is sorted to username
            foreach(var idNamePair in userIdToUserNameCache)
            {
                AddNewRoomToList(idNamePair.Value, idNamePair.Key, userIdToNumPlayersInRm[idNamePair.Key], true); //public indeed
            }
        }
        else //sort type is 0
        {
            if(sortType == 3) //initiate destruction sequence ;D
            {
                foreach (var roomTabObj in playerRoomList.Values.Select(i => i.roomDisplayTab.gameObject))
                {
                    Destroy(roomTabObj);
                }
                playerRoomList.Clear();
            }

            //playerRoomList no longer stores roomName as key, so gonna compare using ids now (room = id here.)
            List<string> listIds = playerRoomList.Keys.ToList(); //Roomnames (ownernames) are unique, but we doing id here.
            List<string> dbRoomIds = userIdToUserNameCache.Keys.ToList(); //dbRooms.Select(r => r.RoomName).ToList(); //Select(r => (string)r["RoomName"]).ToList(); //was a list of dicts
            List<string> ToBeUpdated = listIds.Intersect(dbRoomIds).ToList();
            List<string> ToBeDeleted = listIds.Except(ToBeUpdated).ToList(); //they've become private!
            List<string> ToBeAdded = dbRoomIds.Except(ToBeUpdated).ToList(); //new public rooms!
            foreach (string roomId in ToBeDeleted)
            {
                Destroy(playerRoomList[roomId].roomDisplayTab.gameObject);
                playerRoomList.Remove(roomId);
            }
            foreach (string roomId in ToBeUpdated)
            {
                UpdateRoomInfo(roomId, userIdToUserNameCache[roomId], userIdToNumPlayersInRm[roomId]);    //dbRooms.Find()
            }
            foreach (string roomId in ToBeAdded)
            {
                AddNewRoomToList(userIdToUserNameCache[roomId], roomId, userIdToNumPlayersInRm[roomId], true); //public indeed.
            }
        }
    }

    /* //This is no longer needed. Was needed back when messiness was a thing.
    private Dictionary<string, PlayerRoomInfo> ConvertToReadableFormat()
    {
        //user id as key, rest info as values!
        Dictionary<string, PlayerRoomInfo> result = new Dictionary<string, PlayerRoomInfo>();
        foreach (var dbRoom in dbRoomsCache) //can do some arrangements here maybe
        {
            string ownerID = dbRoom.TrueOwnerID;
            //room name doesn't matter, just a temp. Not adding into actual! not checked anyway.
            int numMembers = dbRoom.NumPlayersInRoom;
            bool isPublic = true; //all public rooms btw!

            result.Add(ownerID, new PlayerRoomInfo(null, numMembers, isPublic, null));
        }
        return result;
    }*/

    public void OnCurrentRoomInfoClicked()
    {
        if(!roomInfoPanel.activeSelf)
        {
            dbc.GetCurrentRoomInfo(PersistentData.TRUEOWNERID_OF_CURRENT_ROOM, "CurrentRoomInfo");
            //roomNameTxt.text = PhotonNetwork.CurrentRoom.Name; //Don't do this! Actual Photon room name will be playfab id since it's fixed.
            //Haven't set a cap on max player per room count yet, current it is 5. In MainMenu script.
        }
        else
        {
            roomInfoPanel.SetActive(false);
            searchUserBar.gameObject.SetActive(false);
        }
    }
    //This is the callback from DBC from the above.
    public void SetCurrentRoomInfoTexts(hirebeatprojectdb_userdatastorage roomInfo) //gonna consort the database on this. Photon room privacy not helpful.
    {
        ownRoomSettingsPanel.SetActive(false); //this is a sneaky way to get user click n call refresh

        roomNameTxt.text = roomInfo.UserName; //can add some modifications to the name here if wanted.
        numPlayersInRoomTxt.text = PhotonNetwork.CurrentRoom.PlayerCount.ToString();

        if (roomInfo.IsRoomPublic)
        {
            roomAccessTxt.text = "Open to Public";
        }
        else
        {
            roomAccessTxt.text = "Private Invites Only";
        }

        roomInfoPanel.SetActive(true);
    }

    public void OnSearchRoomClicked()
    {
        //when inputting search options, only username will be sorted progressively
        //if it's account id, then wait till the end and enter
        if(!searchRoomBar.gameObject.activeSelf)
        {
            searchRoomBar.gameObject.SetActive(true);
        }
        else
        {
            searchRoomBar.gameObject.SetActive(false);
        }
    }

    // Since playfabId and Username can only be alphanumeric, and email address is alphanumeric + @ + . basically
    // ,,,...;;; won't be found! still safe. No character limit tho to keep user able to enter long email.
    public void OnSearchRoomBarSubmit() //enter key
    {
        dbc.GetUserIdFromInfo(searchRoomBar.text, "rsps");
    }

    public void OnInviteUserSearchSubmit()
    {
        dbc.GetUserIdFromInfo(searchUserBar.text, "invplayer");
    }

    //Sneaky way: grab your own room setting here then, because you can't see unless you clicked this!
    public void OnOwnRoomSettingsClicked()
    {
        //add more settings in future!
        if(!ownRoomSettingsPanel.activeSelf)
        {
            ownRoomSettingsPanel.SetActive(true);
            dbc.GetCurrentRoomInfo(myID, "SettingsPublicCheck");
            roomInfoPanel.SetActive(false);
            searchUserBar.gameObject.SetActive(false);
        }
        else
        {
            ownRoomSettingsPanel.SetActive(false);
        }
    }

    bool changedDueToDBCheck = false; //toggle is triggererd everytime it is opened, so just in case.
    //This is the callback function from dbc upon own room settings open.
    public void UpdateSelfRoomPublicStatusFromDB(bool isPublic)
    {
        if (isPublic != selfRoomPublicStatus.isOn)
        {
            changedDueToDBCheck = true;
            selfRoomPublicStatus.isOn = isPublic; //this is only triggered if isOn is different from database.
        }
    }

    //Gonna outline the structure here: (due to row level locking)
    //Each room will have dedicated Photon room owner (the Master client). He deals with all the database calls (doesn't have to be owner)
    //Photon will cover the automatic master client transition for us so no worries. If no one in room then room is auto-closed.
    //After room is autoclosed, set its access to private to hide it from others' eyes (last person leaving do a check (he'll be master client),
    //if so then send change to database. If he immediately leaves via exiting the tab, then SQLAdmin will check and set currOwner to null and
    //numplayersinroom to 0.
    //Have a feedback on the official owner's end telling him numPlayersinHisRoom?
    //Every user will update the room info upon people leaving, etc
    //But official room owner (not photon, just name) can still open the room up again
    public void OnSelfRoomPublicStatusChanged()
    {
        if (!changedDueToDBCheck) dbc.ChangeRoomPublicStatus(myID, selfRoomPublicStatus.isOn);
        else changedDueToDBCheck = false;
    }

    public void OnRefreshButtonPressed()
    {
        if (sortByNumPlayers.isOn)
        {
            dbc.GrabAllPublicRooms(1);
        }
        else if (sortByAlphanumeric.isOn)
        {
            dbc.GrabAllPublicRooms(2);
        }
        else if (!sortByNumPlayers.isOn && !sortByAlphanumeric.isOn)
        {
            dbc.GrabAllPublicRooms(0);
        }
    }

    public void OnInviteUserButtonPressed()
    {
        if (!searchUserBar.gameObject.activeSelf)
        {
            searchUserBar.gameObject.SetActive(true);
        }
        else
        {
            searchUserBar.gameObject.SetActive(false);
        }
    }

    public void OnTabOpen()
    {
        if (!playerZoneTab.hasOneOn) //prevents zone + UI
        {
            playerObj.GetComponent<playerController>().enabled = false;
            playerCamera.enabled = false;
        }
    }

    public void CloseWindow()
    {
        roomInfoPanel.SetActive(false);
        ownRoomSettingsPanel.SetActive(false);
        searchUserBar.gameObject.SetActive(false); //this should close with info panel.

        if (errorMsgDisplay != null)
        {
            StopCoroutine(errorMsgDisplay);
            errorMsg.gameObject.SetActive(false);
        }

        gameObject.SetActive(false); //want to keep data!
        if (!playerZoneTab.hasOneOn)
        {
            playerObj.GetComponent<playerController>().enabled = true;
            playerCamera.enabled = true;
            playerObj.GetComponent<playerController>().isMoving = false; //this line prevents the player from getitng stuck after
        }
        UIController.hasOneOn = false;
    }
}
