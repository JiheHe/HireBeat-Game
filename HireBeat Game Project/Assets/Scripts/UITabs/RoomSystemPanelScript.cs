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
    public IEnumerator errorMsgDisplay;

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
    bool inInvitedRoomsView = false;
    public List<string> listOfInvitedRoomIds = new List<string>(); //can be invited by public or private! private can only join through invite tho
    public GameObject playerSearchDisplayPrefab; //this is the prefab for each user display in search results
    public List<InvitePlayerToRoomTab> playerTabsOnDisplay = new List<InvitePlayerToRoomTab>();

    DataBaseCommunicator dbc = null; //the real time database!
    // Start is called before the first frame update
    void Start()
    {
        dbc = GameObject.FindGameObjectWithTag("DataCenter").GetComponent<DataBaseCommunicator>();
        dbc.GrabAllPublicRooms(0, OnGrabAllPublicRoomsNormCallback);

        myID = GameObject.Find("PersistentData").GetComponent<PersistentData>().acctID;


        //this is for testing
        listOfInvitedRoomIds = new List<string> { "B", "f", "Z", "i", "falkdfdjfsf;"};
    }

    public void OnEnable() //called everytime when panel gets active
    {
        //User can keep its previous preference, just a simple refresh!
        if (dbc != null) dbc.GrabAllPublicRooms(0, OnGrabAllPublicRoomsNormCallback); //need this because at obj first init, dbc not assigned yet, so null error. But in future can.
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
            dbc.GrabAllPublicRooms(1, OnGrabAllPublicRoomsNumCallback);
            previousOn = 1;
        }
        else if(!sortByNumPlayers.isOn && !sortByAlphanumeric.isOn && previousOn == 1)
        {
            dbc.GrabAllPublicRooms(3, OnGrabAllPublicRoomsDestCallback);
            previousOn = 0;
        }
    }

    public void OnSortByAlphaToggleChanged()
    {
        if (sortByAlphanumeric.isOn)
        {
            Debug.Log("Starting to sort by alpha");
            dbc.GrabAllPublicRooms(2, OnGrabAllPublicRoomsAlphaCallback);
            previousOn = 2;
        }
        else if (!sortByNumPlayers.isOn && !sortByAlphanumeric.isOn && previousOn == 2)
        {
            dbc.GrabAllPublicRooms(3, OnGrabAllPublicRoomsDestCallback); //3 initializes the room list destruction system.
            previousOn = 0;
        }
    }
    //This is used to create the effect that you've exited a sort mode


    Dictionary<string, QuickRoomInfo> userIdToQRICache; //gonna share this too, just small amt.
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
            string query;
            SQL4Unity.SQLParameter parameters = new SQL4Unity.SQLParameter();
            switch (cmd)
            {
                case "rsps": //searching for a room
                    userIdToQRICache = userData.ToDictionary(r => r.UserId, r => new QuickRoomInfo(r.UserName, r.IsRoomPublic));
                    //dbc.GrabAllRoomInfoFromGivenIds(userIdToQRICache.Keys.Distinct().ToArray(), "roomsearch"); //removes duplicate!
                    string[] userIds = userIdToQRICache.Keys.Distinct().ToArray();
                    if (userIds.Length < 2) //1 if room search, but could be 0 if invite ids!
                    {
                        query = "SELECT TrueOwnerID, NumPlayersInRoom FROM UserRoomsAssociated WHERE TrueOwnerID = %userId%"; //only 1 element.
                        if (userIds.Length == 1) parameters.SetValue("userId", userIds[0]);
                        else parameters.SetValue("userId", ";;;,,,"); //no way this is in the database lol
                        DataBaseCommunicator.Execute(query, DisplayRoomSearchResults, parameters);
                    }
                    else //2 
                    {
                        query = "SELECT TrueOwnerID, NumPlayersInRoom FROM UserRoomsAssociated WHERE TrueOwnerID IN ({0})";
                        string inClause = string.Join(",", userIds.Select(id => string.Concat("'", id, "'"))); //'id1','id2'... directly!
                        query = string.Format(query, inClause); //replaces {0} with the list of paramNames
                        DataBaseCommunicator.Execute(query, DisplayRoomSearchResults);
                    }
                    //just in case grabbing someone whose name == id twice. Diff row diff id.
                    break;
                case "invplayer": //searching for a player
                    userIdToUserNameCacheForPlayerSearch = userData.ToDictionary(r => r.UserId, r => r.UserName);
                    //dbc.GrabAllUserStatusFromGivenIds(userIdToUserNameCacheForPlayerSearch.Keys.Distinct().ToArray());
                    string[] userIdx = userIdToUserNameCacheForPlayerSearch.Keys.Distinct().ToArray();
                    if (userIdx.Length < 2) //1 
                    {
                        query = "SELECT UniqueID FROM IPAdressToUniqueID WHERE UniqueID = %userId%"; //only 1 element.
                        parameters.SetValue("userId", userIdx[0]);
                        DataBaseCommunicator.Execute(query, DisplayUserStatusResults, parameters);
                    }
                    else //2 
                    {
                        query = "SELECT UniqueID FROM IPAdressToUniqueID WHERE UniqueID IN ({0})";
                        string inClause = string.Join(",", userIdx.Select(id => string.Concat("'", id, "'"))); //'id1','id2'... directly!
                        query = string.Format(query, inClause); //replaces {0} with the list of paramNames
                        DataBaseCommunicator.Execute(query, DisplayUserStatusResults);
                    }
                    break;
                case "chkinvts":
                    userIdToQRICache = userData.ToDictionary(r => r.UserId, r => new QuickRoomInfo(r.UserName, r.IsRoomPublic));
                    //dbc.GrabAllRoomInfoFromGivenIds(userIdToQRICache.Keys.Distinct().ToArray(), "allinvites");
                    string[] userIdz = userIdToQRICache.Keys.Distinct().ToArray();
                    if (userIdz.Length < 2) //1 if room search, but could be 0 if invite ids!
                    {
                        query = "SELECT TrueOwnerID, NumPlayersInRoom FROM UserRoomsAssociated WHERE TrueOwnerID = %userId%"; //only 1 element.
                        if (userIdz.Length == 1) parameters.SetValue("userId", userIdz[0]);
                        else parameters.SetValue("userId", ";;;,,,"); //no way this is in the database lol
                        DataBaseCommunicator.Execute(query, DisplayInvitesSearchResults, parameters);
                    }
                    else //2 
                    {
                        query = "SELECT TrueOwnerID, NumPlayersInRoom FROM UserRoomsAssociated WHERE TrueOwnerID IN ({0})";
                        string inClause = string.Join(",", userIdz.Select(id => string.Concat("'", id, "'"))); //'id1','id2'... directly!
                        query = string.Format(query, inClause); //replaces {0} with the list of paramNames
                        DataBaseCommunicator.Execute(query, DisplayInvitesSearchResults);
                    }
                    break;
            }
        }
    }
    //This method is the callback from dbc to grab all room info from given list of ids, above
    public void DisplayRoomSearchResults(SQL4Unity.SQLResult result)
    {
        inInvitedRoomsView = true; //why here? so room you researched for will be destroyed instead of updated to make visual better ;D
        if (result != null)
        {
            hirebeatprojectdb_userroomsassociated[] dbRooms = result.Get<hirebeatprojectdb_userroomsassociated>();
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
        else
        {
            Debug.LogError("Error in trying to grab room search secondary info");
        }
    }
    public void DisplayInvitesSearchResults(SQL4Unity.SQLResult result)
    {
        inInvitedRoomsView = true;
        if (result != null)
        {
            hirebeatprojectdb_userroomsassociated[] dbRooms = result.Get<hirebeatprojectdb_userroomsassociated>();
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

            //in invite tab! //currently not sorted because I think there's no need? //since they are invites, give each accept/decline buttons as well.
            foreach (var room in dbRooms)
            {
                //If is not invited, then set join button to false. (join button default to true). very easy
                //No need to order! just 1-2 rooms anyway. (just in case if same username as someone's id)
                var QRIObj = userIdToQRICache[room.TrueOwnerID];
                AddNewRoomToList(QRIObj.roomName, room.TrueOwnerID, room.NumPlayersInRoom, QRIObj.isPublic, true, true);
            }
        }
        else
        {
            Debug.LogError("Error in trying to grab invited rooms' secondary info");
        }
    }
    //This method is the callback from dbc after grabbing user online status from a list of ids
    public void DisplayUserStatusResults(SQL4Unity.SQLResult result)
    {
        if (result != null)
        {
            List<string> idsOnline = result.Get<hirebeatprojectdb_ipadresstouniqueid>().Select(r => r.UniqueID).ToList();
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
                AddNewUserToList(user.Value, user.Key, isOnline, true);
            }
        }
        else
        {
            Debug.LogError("Error in trying to grab user status search secondary info");
        }
    }

    public IEnumerator DisplayErrorMessage(float time, string message)
    {
        errorMsg.gameObject.SetActive(true);
        errorMsg.text = message;
        yield return new WaitForSeconds(time);
        errorMsg.gameObject.SetActive(false);
    }

    public void OnNewRoomInviteReceived(string roomID)
    {
        if(!listOfInvitedRoomIds.Contains(roomID)) //if a user sends multiple invites to same room to same person.
        {
            listOfInvitedRoomIds.Add(roomID);

            //announce to the user that a new invite has been received through notification etc.
            //Could do a message notif here, or maybe changing display tab button color would be better?
        }
    }

    private void AddNewUserToList(string userName, string userId, bool isOnline, bool isInSearchView, bool isTrueOwner = false, bool isYou = false)
    {
        if (userId == myID) isYou = true; //a quick check to ensure that you don't misperform operations on yourself.
        var newPlayerSearchDisplay = Instantiate(playerSearchDisplayPrefab, playerRoomDisplayPanel); //using same panel as parent
        newPlayerSearchDisplay.GetComponent<InvitePlayerToRoomTab>().SetUserInfo(userName, userId, isOnline, isInSearchView, isTrueOwner, isYou);
        playerTabsOnDisplay.Add(newPlayerSearchDisplay.GetComponent<InvitePlayerToRoomTab>());
    }

    private void AddNewRoomToList(string roomName, string ownerID, int numMembers, bool isPublic, bool isInvited = false,
        bool inInviteTab = false) //this uses roomName
    {
        var newPlayerRoomDisplay = Instantiate(playerRoomDisplayPrefab, playerRoomDisplayPanel);
        //newVCRoomDisplay.name = prefix + roomName; //no need for prefix...
        //if (invitedRoomList.Contains(roomName)) newVCRoomDisplay.GetComponent<VidCDisplayTab>().SetRoomInfo(roomName, numMembers, isPublic, currOwnerID, true); //invited!
        newPlayerRoomDisplay.GetComponent<PlayerRoomDisplayTab>().SetRoomInfo(roomName, numMembers, isPublic, ownerID, isInvited, inInviteTab);
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
        string[] userIds = userIdToUserNameCache.Keys.ToArray();

        string query;
        SQL4Unity.SQLParameter parameters = new SQL4Unity.SQLParameter();
        Action<SQL4Unity.SQLResult> callback;

        switch (sortType)
        {
            case 0:
                callback = OnPublicRoomsNormSecInfoCallback;
                break;
            case 1:
                callback = OnPublicRoomsNumSecInfoCallback;
                break;
            case 2:
                callback = OnPublicRoomsAlphaSecInfoCallback;
                break;
            case 3:
                callback = OnPublicRoomsDestSecInfoCallback;
                break;
            default:
                callback = null; //this will never happen unless weird af bug
                break;
        }

        if (userIds.Length < 2) //1 or 0, rare case.
        {
            query = "SELECT TrueOwnerID, NumPlayersInRoom FROM UserRoomsAssociated WHERE TrueOwnerID = %userId%"; //only 1 element.
            if (userIds.Length == 1) parameters.SetValue("userId", userIds[0]);
            else parameters.SetValue("userId", ",,,"); //,,, is not possible, meaning the result will be empty.
            DataBaseCommunicator.Execute(query, callback, parameters);
        }
        else
        {
            if (sortType == 1) //sort by num
                query = "SELECT TrueOwnerID, NumPlayersInRoom FROM UserRoomsAssociated WHERE TrueOwnerID IN ({0}) ORDER BY NumPlayersInRoom"; //sort num here!
            else
                query = "SELECT TrueOwnerID, NumPlayersInRoom FROM UserRoomsAssociated WHERE TrueOwnerID IN ({0})";
            /*string[] paramNames = userIds.Select(
				(s, i) => "@id" + i.ToString() //string, index 
			).ToArray(); //this assigns each tag an index based on userIds length

			string inClause = string.Join(", ", paramNames); //@id0, @id1, @id2... format
			query = string.Format(query, inClause); //replaces {0} with the list of paramNames

			for (int i = 0; i < paramNames.Length; i++)
			{
				parameters.SetValue(paramNames[i], userIds[i]); //@id0 = firstId, etc...
			}*/

            //Might be simpler? no need for parameters.
            string inClause = string.Join(",", userIds.Select(id => string.Concat("'", id, "'"))); //'id1','id2'... directly!
            query = string.Format(query, inClause); //replaces {0} with the list of paramNames
            DataBaseCommunicator.Execute(query, callback);
        }
        //dbc.GrabAllUsernamesFromGivenIds(idsThatNeedRoomInfo, sortType);
    }
    void OnPublicRoomsNumSecInfoCallback(SQL4Unity.SQLResult result) //sorttype == 1
    {
        if (result != null)
        {
            hirebeatprojectdb_userroomsassociated[] rows = result.Get<hirebeatprojectdb_userroomsassociated>();
            Dictionary<string, int> userIdToNumPlayersInRm = rows.Reverse().ToDictionary(r => r.TrueOwnerID, r => r.NumPlayersInRoom);
            UpdatePlayerRoomList(userIdToNumPlayersInRm, 1);
        }
        else
        {
            Debug.LogError("Error occured in grab all public room secondary info num");
        }
    }
    void OnPublicRoomsAlphaSecInfoCallback(SQL4Unity.SQLResult result) //sorttype == 1
    {
        if (result != null)
        {
            hirebeatprojectdb_userroomsassociated[] rows = result.Get<hirebeatprojectdb_userroomsassociated>();
            Dictionary<string, int> userIdToNumPlayersInRm = rows.ToDictionary(r => r.TrueOwnerID, r => r.NumPlayersInRoom);
            UpdatePlayerRoomList(userIdToNumPlayersInRm, 2);
        }
        else
        {
            Debug.LogError("Error occured in grab all public room secondary info alpha");
        }
    }
    void OnPublicRoomsNormSecInfoCallback(SQL4Unity.SQLResult result) //sorttype == 1
    {
        if (result != null)
        {
            hirebeatprojectdb_userroomsassociated[] rows = result.Get<hirebeatprojectdb_userroomsassociated>();
            Dictionary<string, int> userIdToNumPlayersInRm = rows.ToDictionary(r => r.TrueOwnerID, r => r.NumPlayersInRoom);
            UpdatePlayerRoomList(userIdToNumPlayersInRm, 0);
        }
        else
        {
            Debug.LogError("Error occured in grab all public room secondary info norm");
        }
    }
    void OnPublicRoomsDestSecInfoCallback(SQL4Unity.SQLResult result) //sorttype == 1
    {
        if (result != null)
        {
            hirebeatprojectdb_userroomsassociated[] rows = result.Get<hirebeatprojectdb_userroomsassociated>();
            Dictionary<string, int> userIdToNumPlayersInRm = rows.ToDictionary(r => r.TrueOwnerID, r => r.NumPlayersInRoom);
            UpdatePlayerRoomList(userIdToNumPlayersInRm, 3);
        }
        else
        {
            Debug.LogError("Error occured in grab all public room secondary info dest");
        }
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
        //Then check if there are invited rooms. If yes then bye bye too.
        if(inInvitedRoomsView) 
        {
            foreach (var roomTabObj in playerRoomList.Values.Select(i => i.roomDisplayTab.gameObject))
            {
                Destroy(roomTabObj);
            }
            playerRoomList.Clear();
            inInvitedRoomsView = false;
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
    public void SetCurrentRoomInfoTexts(SQL4Unity.SQLResult result) //gonna consort the database on this. Photon room privacy not helpful.
    {
        if (result != null)
        {
            hirebeatprojectdb_userdatastorage roomInfo = result.Get<hirebeatprojectdb_userdatastorage>()[0];

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
        else
        {
            Debug.LogError("Erroring retrieving current room info");
        }
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
        //dbc.GetUserIdFromInfo(searchRoomBar.text, "rsps");
        string query = "SELECT UserName, UserId, IsRoomPublic FROM UserDataStorage WHERE UserName = %input% OR UserId = %input% OR Email = %input%";
        SQL4Unity.SQLParameter parameters = new SQL4Unity.SQLParameter();
        parameters.SetValue("input", searchRoomBar.text);
        DataBaseCommunicator.Execute(query, OnSearchRoomBarSubmitCallback, parameters);
    }
    void OnSearchRoomBarSubmitCallback(SQL4Unity.SQLResult result)
    {
        if (result != null)
        {
            if (result.rowsAffected == 0)
            {
                StoreInputSearchResults(null, "doesntmatter", searchRoomBar.text); //0 rows affected if nothing exists relating to the input.
            }
            else
            {
                hirebeatprojectdb_userdatastorage[] rows = result.Get<hirebeatprojectdb_userdatastorage>();
                StoreInputSearchResults(rows, "rsps"); //got something, regardless of its length.
            }
        }
        else
        {
            Debug.LogError("Error occured in search room bar submit");
        }
    }

    public void OnInviteUserSearchSubmit()
    {
        //dbc.GetUserIdFromInfo(searchUserBar.text, "invplayer");
        string query = "SELECT UserName, UserId, IsRoomPublic FROM UserDataStorage WHERE UserName = %input% OR UserId = %input% OR Email = %input%";
        SQL4Unity.SQLParameter parameters = new SQL4Unity.SQLParameter();
        parameters.SetValue("input", searchUserBar.text);
        DataBaseCommunicator.Execute(query, OnInviteUserSearchSubmitCallback, parameters);
    }
    void OnInviteUserSearchSubmitCallback(SQL4Unity.SQLResult result)
    {
        if (result != null)
        {
            if (result.rowsAffected == 0)
            {
                StoreInputSearchResults(null, "doesntmatter", searchUserBar.text); //0 rows affected if nothing exists relating to the input.
            }
            else
            {
                hirebeatprojectdb_userdatastorage[] rows = result.Get<hirebeatprojectdb_userdatastorage>();
                StoreInputSearchResults(rows, "invplayer"); //got something, regardless of its length.
            }
        }
        else
        {
            Debug.LogError("Error occured in invite user search submit");
        }
    }

    public void OnCheckInviteTabPressed()
    {
        //dbc.GrabAllInvitedRoomNameFromGivenIds(listOfInvitedRoomIds);
        string query;
        SQL4Unity.SQLParameter parameters = new SQL4Unity.SQLParameter();

        if (listOfInvitedRoomIds.Count < 2) //1 or 0
        {
            query = "SELECT UserName, UserId, IsRoomPublic FROM UserDataStorage WHERE UserId = %userId%"; //only 1 element.
            if (listOfInvitedRoomIds.Count == 1) parameters.SetValue("userId", listOfInvitedRoomIds[0]);
            else parameters.SetValue("userId", ";;;,,,"); //this is not possible, so let's see if it returns empty ;D
            DataBaseCommunicator.Execute(query, OnCheckInviteTabPressedCallback, parameters);
        }
        else //2 or more invites
        {
            query = "SELECT UserName, UserId, IsRoomPublic FROM UserDataStorage WHERE UserId IN ({0})";
            string inClause = string.Join(",", listOfInvitedRoomIds.Select(id => string.Concat("'", id, "'"))); //'id1','id2'... directly!
            query = string.Format(query, inClause); //replaces {0} with the list of paramNames
            DataBaseCommunicator.Execute(query, OnCheckInviteTabPressedCallback);
        }
    }
    void OnCheckInviteTabPressedCallback(SQL4Unity.SQLResult result)
    {
        if (result != null)
        {
            hirebeatprojectdb_userdatastorage[] rows = result.Get<hirebeatprojectdb_userdatastorage>();
            StoreInputSearchResults(rows, "chkinvts");
        }
        else
        {
            Debug.LogError("Error occured in check invite button submit");
        }
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
    public void UpdateSelfRoomPublicStatusFromDB(SQL4Unity.SQLResult result)
    {
        if (result != null)
        {
            bool isPublic = result.Get<hirebeatprojectdb_userdatastorage>()[0].IsRoomPublic;
            if (isPublic != selfRoomPublicStatus.isOn)
            {
                changedDueToDBCheck = true;
                selfRoomPublicStatus.isOn = isPublic; //this is only triggered if isOn is different from database.
            }
        }
        else
        {
            Debug.LogError("Error in retrieving self room public status");
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
            dbc.GrabAllPublicRooms(1, OnGrabAllPublicRoomsNumCallback);
        }
        else if (sortByAlphanumeric.isOn)
        {
            dbc.GrabAllPublicRooms(2, OnGrabAllPublicRoomsAlphaCallback);
        }
        else if (!sortByNumPlayers.isOn && !sortByAlphanumeric.isOn)
        {
            dbc.GrabAllPublicRooms(0, OnGrabAllPublicRoomsNormCallback);
        }
    }
    void OnGrabAllPublicRoomsNumCallback(SQL4Unity.SQLResult result)
    {
        if (result != null)
        {
            hirebeatprojectdb_userdatastorage[] rows = result.Get<hirebeatprojectdb_userdatastorage>();
            GrabAllPublicRoomInfo(rows, 1);
        }
        else
        {
            Debug.LogError("Error occured in grab all public room info num");
        }
    }
    void OnGrabAllPublicRoomsAlphaCallback(SQL4Unity.SQLResult result)
    {
        if (result != null)
        {
            hirebeatprojectdb_userdatastorage[] rows = result.Get<hirebeatprojectdb_userdatastorage>();
            GrabAllPublicRoomInfo(rows, 2);
        }
        else
        {
            Debug.LogError("Error occured in grab all public room info alpha");
        }
    }
    void OnGrabAllPublicRoomsNormCallback(SQL4Unity.SQLResult result)
    {
        if (result != null)
        {
            hirebeatprojectdb_userdatastorage[] rows = result.Get<hirebeatprojectdb_userdatastorage>();
            GrabAllPublicRoomInfo(rows, 0);
        }
        else
        {
            Debug.LogError("Error occured in grab all public room info norm");
        }
    }
    void OnGrabAllPublicRoomsDestCallback(SQL4Unity.SQLResult result)
    {
        if (result != null)
        {
            hirebeatprojectdb_userdatastorage[] rows = result.Get<hirebeatprojectdb_userdatastorage>();
            GrabAllPublicRoomInfo(rows, 3);
        }
        else
        {
            Debug.LogError("Error occured in grab all public room info dest");
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

    public void OnGetListOfAllUsersInRoomButtonPressed()
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

        var currentPlayers = PhotonNetwork.CurrentRoom.Players.Values;
        bool isTrueOwner = PhotonNetwork.CurrentRoom.Name.Substring("USERROOM_".Length) == myID; //Room name format is USERROOM_[userid], so...
        foreach(var p in currentPlayers)
        {
            AddNewUserToList(p.NickName, p.UserId, true, false, isTrueOwner); //obviously online
        }
    }

    public void OnReturnToHomeButtonPressed()
    {
        PersistentData.TRUEOWNERID_OF_JOINING_ROOM = myID;

        Debug.Log("Connecting...");

        //This is like just another form of joining a room! Except that HOPEFULLY you'll be able to join cuz it's your own room....
        GameObject.Find("PlayFabController").GetComponent<PhotonConnector>().DisconnectPlayer();
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
