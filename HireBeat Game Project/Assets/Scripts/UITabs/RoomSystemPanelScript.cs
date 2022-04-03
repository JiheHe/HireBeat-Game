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

    public GameObject roomInfoPanel;
    public Text roomNameTxt;
    public Text numPlayersInRoomTxt;
    public Text roomAccessTxt; //this is from database
    public InputField searchRoomBar;
    public GameObject ownRoomSettingsPanel;

    [Serializable]
    public class PlayerRoomInfo //roomName is the key
    {
        public string userId; //this is useless
        public int numPlayersInRoom;
        public bool isPublic; //this  might be useless too
        public PlayerRoomDisplayTab roomDisplayTab;

        public PlayerRoomInfo(string userId, int numPlayersInRoom, bool isPublic, PlayerRoomDisplayTab roomDisplayTab)
        {
            this.userId = userId;
            this.numPlayersInRoom = numPlayersInRoom;
            this.isPublic = isPublic;
            this.roomDisplayTab = roomDisplayTab;
        }
    }

    public Dictionary<string, PlayerRoomInfo> playerRoomList = new Dictionary<string, PlayerRoomInfo>(); //a cache for userName, rest of info
    public GameObject playerRoomDisplayPrefab; //this is the prefab for each player room display in list
    public RectTransform playerRoomDisplayPanel; //this is the content where room will be child of.
    public List<string> invitedRoomList = new List<string>(); //can be invited by public or private! private can only join through invite tho

    DataBaseCommunicator dbc = null; //the real time database!
    // Start is called before the first frame update
    void Start()
    {
        dbc = GameObject.FindGameObjectWithTag("DataCenter").GetComponent<DataBaseCommunicator>();
        dbc.GrabAllPublicRooms(0);
    }

    public void OnEnable() //called everytime when panel gets active
    {
        sortByAlphanumeric.isOn = false;
        sortByNumPlayers.isOn = false;
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


    private void AddNewRoomToList(string roomName, string ownerID, int numMembers, bool isPublic)
    {
        var newPlayerRoomDisplay = Instantiate(playerRoomDisplayPrefab, playerRoomDisplayPanel);
        //newVCRoomDisplay.name = prefix + roomName; //no need for prefix...
        //if (invitedRoomList.Contains(roomName)) newVCRoomDisplay.GetComponent<VidCDisplayTab>().SetRoomInfo(roomName, numMembers, isPublic, currOwnerID, true); //invited!
        newPlayerRoomDisplay.GetComponent<PlayerRoomDisplayTab>().SetRoomInfo(roomName, numMembers, isPublic, ownerID);
        playerRoomList.Add(roomName, new PlayerRoomInfo(ownerID, numMembers, isPublic, newPlayerRoomDisplay.GetComponent<PlayerRoomDisplayTab>()));
    }

    private void UpdateRoomInfo(string roomName, int numMembers) 
    {
        var info = playerRoomList[roomName]; //I think this is by reference eh?
        if (info.numPlayersInRoom != numMembers)
        {
            info.numPlayersInRoom = numMembers;
            info.roomDisplayTab.UpdateNumMembers(numMembers);
        }
        //if (invitedRoomList.Contains(roomName)) info.roomDisplayTab.UpdateJoinAccess(true);
    }

    public Toggle sortByNumPlayers; //if this is true by toggling, then...
    public Toggle sortByAlphanumeric;
    //Need to check playerRoomList room names against the data base ver.: if in room and not in data base then remove, if in data base and
    //not in room then add, if in both then update.
    public void UpdatePlayerRoomList(hirebeatprojectdb_userdatastorage[] dbRooms, int sortType)
    {
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

            for(int i = dbRooms.Length-1; i >= 0; i--) 
            {
                var roomPair = dbRooms[i];
                AddNewRoomToList(roomPair.UserName, roomPair.UserId, roomPair.NumPlayersInRoom, true); //public indeed
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

            for (int i = 0; i < dbRooms.Length; i++)
            {
                var roomPair = dbRooms[i];
                AddNewRoomToList(roomPair.UserName, roomPair.UserId, roomPair.NumPlayersInRoom, true); //public indeed
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

            var dbRoomsConverted = ConvertToReadableFormat(dbRooms);
            List<string> listRoomNames = playerRoomList.Keys.ToList(); //Roomnames (ownernames) are unique
            List<string> dbRoomNames = dbRoomsConverted.Keys.ToList(); //dbRooms.Select(r => r.RoomName).ToList(); //Select(r => (string)r["RoomName"]).ToList(); //was a list of dicts
            List<string> ToBeUpdated = listRoomNames.Intersect(dbRoomNames).ToList();
            List<string> ToBeDeleted = listRoomNames.Except(ToBeUpdated).ToList(); //they've become private!
            List<string> ToBeAdded = dbRoomNames.Except(ToBeUpdated).ToList(); //new public rooms!
            foreach (string roomName in ToBeDeleted)
            {
                Destroy(playerRoomList[roomName].roomDisplayTab.gameObject);
                playerRoomList.Remove(roomName);
            }
            foreach (string roomName in ToBeUpdated)
            {
                UpdateRoomInfo(roomName, dbRoomsConverted[roomName].numPlayersInRoom);    //dbRooms.Find()
            }
            foreach (string roomName in ToBeAdded)
            {
                var newInfo = dbRoomsConverted[roomName];
                AddNewRoomToList(roomName, newInfo.userId, newInfo.numPlayersInRoom, newInfo.isPublic);
            }
        }
    }

    private Dictionary<string, PlayerRoomInfo> ConvertToReadableFormat(hirebeatprojectdb_userdatastorage[] dbRooms)
    {
        Dictionary<string, PlayerRoomInfo> result = new Dictionary<string, PlayerRoomInfo>();
        foreach (var dbRoom in dbRooms) //can do some arrangements here maybe
        {
            string roomName = dbRoom.UserName;
            string ownerID = dbRoom.UserId;
            int numMembers = dbRoom.NumPlayersInRoom;
            bool isPublic = true; //all public rooms btw!

            result.Add(roomName, new PlayerRoomInfo(ownerID, numMembers, isPublic, null));
        }
        return result;
    }

    public void OnCurrentRoomInfoClicked()
    {
        if(!roomInfoPanel.activeSelf)
        {
            roomInfoPanel.SetActive(true);
            roomNameTxt.text = PhotonNetwork.CurrentRoom.Name;
            //Haven't set a cap on max player per room count yet, current it is 5. In MainMenu script.
            numPlayersInRoomTxt.text = PhotonNetwork.CurrentRoom.PlayerCount.ToString();

            SetRoomAccessText(false); //this should be a dbq database call to grab current access, direct for testing.
        }
        else
        {
            roomInfoPanel.SetActive(false);
        }
    }
    //This is the callback from DBC
    public void SetRoomAccessText(bool isPublic) //gonna consort the database on this. Photon room privacy not helpful.
    {
        if(isPublic)
        {
            roomAccessTxt.text = "Open to Public";
        }
        else
        {
            roomAccessTxt.text = "Private Invites Only";
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

    public void OnOwnRoomSettingsClicked()
    {
        //add more settings in future!
        if(!ownRoomSettingsPanel.activeSelf)
        {
            ownRoomSettingsPanel.SetActive(true);
        }
        else
        {
            ownRoomSettingsPanel.SetActive(false);
        }
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
