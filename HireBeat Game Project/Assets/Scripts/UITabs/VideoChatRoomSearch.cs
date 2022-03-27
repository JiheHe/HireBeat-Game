using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
using UnityEngine;
using Byn.Awrtc;
using Byn.Awrtc.Unity;
using Byn.Unity.Examples;
using UnityEngine.UI;
using System.Linq;


public class VideoChatRoomSearch : MonoBehaviour
{
    [Header("Settings panel")]
    /// <summary>
    /// Panel with the join button. Will be hidden after setup
    /// </summary>
    public RectTransform uSetupPanel;

    public Toggle uAudioToggle;
    public Toggle uVideoToggle;
    public Dropdown uVideoDropdown;
    public InputField uIdealWidth;
    public InputField uIdealHeight;
    public InputField uIdealFps;
    public Dropdown uFormatDropdown;
    private string mStoredVideoDevice = null;

    private string mPrefix = "VidCUI_";
    private static readonly string PREF_AUDIO = "audio";
    private static readonly string PREF_VIDEO = "video";
    private static readonly string PREF_VIDEODEVICE = "videodevice";
    private static readonly string PREF_IDEALWIDTH = "idealwidth";
    private static readonly string PREF_IDEALHEIGHT = "idealheight";
    private static readonly string PREF_IDEALFPS = "idealfps";
    private static readonly string PREF_FORMAT = "format";
    public bool uLoadSettings = true;

    public Text errorMessageObject;
    // Start is called before the first frame update

    /// <summary>
    /// Media configuration. Will be set during setup.
    /// </summary>
    private MediaConfig mediaConf = new MediaConfig();

    private NetworkConfig netConf;

    public GameObject videoChatRoomPanelPrefab; //this is the prefab for in call panel.
    public VideoChatController vCC = null;

    [Serializable]
    public class VidCRoomInfo //roomName is the key
    {
        public string currOwnerID;
        public int numMembers;
        public bool isPublic;
        public VidCDisplayTab roomDisplayTab;

        public VidCRoomInfo(string currOwnerID, int numMembers, bool isPublic, VidCDisplayTab roomDisplayTab)
        {
            this.currOwnerID = currOwnerID;
            this.numMembers = numMembers;
            this.isPublic = isPublic;
            this.roomDisplayTab = roomDisplayTab;
        }

        public VidCRoomInfo(string currOwnerID, int numMembers, bool isPublic) //this overload is for data cahce
        {
            this.currOwnerID = currOwnerID;
            this.numMembers = numMembers;
            this.isPublic = isPublic;
            this.roomDisplayTab = null;
        }
    }

    [SerializeField]
    public Dictionary<string, VidCRoomInfo> vcRoomList;
    string prefix = "VC_ROOM_"; //this is what to add to each new room obj's name
    public GameObject videoChatRoomDisplayPrefab; //this is the prefab for each room display in list
    public RectTransform vcRoomDisplayPanel; //this is the content where room will be child of.

    DataBaseCommunicator dbc = null; //the real time database!
    string prevRoomName; //this gets updated at room joining, but you get to keep the previous name until overwritten.


    public Toggle publicSwitch;
    public Toggle privateSwitch;
    public InputField vcRoomSearchField;
    public Text searchMessage;
    public GameObject vcRoomCreatePanel;
    public InputField vcRoomCreateField;
    public Toggle vcRoomPublicSwitch;
    public Button vcRoomCreateButton;

    public string myID;
    public SocialSystemScript socialSystem;

    void Start() //conf the permanent netConf at start.
    {
        //STEP1: instance setup
        netConf = new NetworkConfig();
        netConf.SignalingUrl = ExampleGlobals.Signaling; //will change the urls and servers later, post testing
        netConf.IceServers.Add(new IceServer(ExampleGlobals.StunUrl));

        dbc = GameObject.FindGameObjectWithTag("DataCenter").GetComponent<DataBaseCommunicator>();
        vcRoomList = new Dictionary<string, VidCRoomInfo>(); //key will be roomName, it stays fixed.
        myID = GameObject.Find("PersistentData").GetComponent<PersistentData>().acctID;

        dbc.GrabAllVCRoomInfo("VCRoomListTotalUpdate"); //called once at init.
    }

    public void OnEnable() //called everytime when panel gets active
    {
        publicSwitch.isOn = false;
        privateSwitch.isOn = false;
        if(dbc != null) dbc.GrabAllVCRoomInfo("VCRoomListTotalUpdate"); //need this because at obj first init, dbc not assigned yet, so null error. But in future can.
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.O))
        {
            //InitializeVideoChatRoomPanel(); //this is for testing only.
        }
        if(Input.GetKeyDown(KeyCode.T))
        {
            //dbc.DeleteALLVCRooms();
        }
    }

    //Basically, connect button only shows if the room is public or you've been invited, either case you should be able to join directly
    //When connect is pressed, this method in vcs will be called. Use roomName to grab the room stats and and request.
    //Wouldn't the ownerid stored in the display tab be useless then LOL, since database everything basically.
    string connectRoomName;
    public void OnConnectPressed(string roomName) //address should be HireBeatProjVidC + myID //this is to avoid connect to other
    {
        connectRoomName = roomName;
        Debug.Log("Connecting to VC_ROOM: " + roomName);
        //Check amount of current users. If full then fail... but if not full then immediately add 1 to the current count! (cuz you gona connect!)
        //Actually don't do it from your end! Remember only 1 user should edit a row, which should be the owner, so:
        dbc.RetrieveVCRoomInfo(roomName, "ConnectRoomCheck"); //dbc calls second half
    }
    public void OnConnectPressedSecondHalf(hirebeatprojectdb_videochatsavailable roomInfo) //called by dbc
    {
        if (roomInfo == null) //nothing was returned, so special result! This means that the room no longer exists.
        {
            //Print alert message
            DisplaySearchMessage(3, "The room no longer exists!");
            //Destroy the room immediately from user's view
            Destroy(vcRoomList[connectRoomName].roomDisplayTab.gameObject);
            vcRoomList.Remove(connectRoomName);
            return;
        }
        if (roomInfo.NumMembers >= 6) //Room's already full
        {
            //Print alert message
            DisplaySearchMessage(3, "The room is full! 6/6");
            //Don't destroy the room (even if it's in invite!), but set numUsers to 6
            vcRoomList[connectRoomName].roomDisplayTab.gameObject.GetComponent<VidCDisplayTab>().UpdateNumMembers(6);
            return;
        }//else:

        //Check who the owner is
        string currOwnerID = roomInfo.CurrOwnerID;
        prevRoomName = connectRoomName;

        //Then use Photon Chat to request the list of userInRoomIDs from the owner and send them through messages. 
        //Then the owner will add 1 to member and send back info upon receiving.
        socialSystem.RequestVidCRoomInfo(currOwnerID);
    }

    //This callback is received when the owner sends you the list all users in room: ready to go!
    //Called from photon chat manager
    public void OnRoomOwnerInfoSendBack(string[] userIds)
    {
        Debug.LogError("Userids to connect to received: " + userIds.ToString());
        InitializeVideoChatRoomPanel(userIds.ToList()); //you are joining!
    }

    private void InitializeVideoChatRoomPanel(List<string> userIds) //if userIds is empty, it indicates creating a room automatically. (no connect)
    {
        //initialize the object, set parent to social sytem
        vCC = Instantiate(videoChatRoomPanelPrefab, transform.parent.GetComponent<RectTransform>()).GetComponent<VideoChatController>();
        vCC.gameObject.transform.localPosition = new Vector2(-380f, 0f); //since we start with textchat.
        //set this.mediaConf to the object's
        //set this netConf to the object's
        //Initializes the userIds list: if you create a room then none, else have some.
        vCC.SetupMediaAndNetConfAndOther(mediaConf, netConf, this, userIds, prevRoomName);
        //Joining!
        vCC.StartRoomCreateOrJoinProcess();

        //Close the search panel.
        gameObject.SetActive(false);
    }


    #region UIButtons
    public void CloseVideoChatRoomSearchPanel()
    {
        gameObject.SetActive(false);
    }

    public void OnRefreshVCRoomButtonPressed()
    {
        if(publicSwitch.isOn || privateSwitch.isOn) //if user selects one, then update only that part.
        {
            SortVCRoomsByPublicity();
        }
        else dbc.GrabAllVCRoomInfo("VCRoomListTotalUpdate");
    }

    public void OnSearchVCRoomButtonClicked()
    {
        if(!vcRoomSearchField.gameObject.activeSelf) //not on, then turn on!
        {
            vcRoomSearchField.gameObject.SetActive(true);
            vcRoomSearchField.Select();
        } 
        else //on, then turn off!
        {
            vcRoomSearchField.text = "";
            vcRoomSearchField.gameObject.SetActive(false);
        }
    }

    public void OnCreateVCRoomButtonClicked()
    {
        if(!vcRoomCreatePanel.activeSelf)
        {
            vcRoomCreateField.text = "";
            vcRoomPublicSwitch.isOn = true; //default to public!
            vcRoomCreatePanel.SetActive(true);
        }
        else
        {
            vcRoomCreatePanel.SetActive(false); //already cleared every open start, no need to clear at end.
        }
    }
    #endregion 

    //won't change from server's end, will do overwrite from owner's local end
    public void UpdateVCRoomNumMembers(string roomName, int numMembers)
    {
        dbc.UpdateVCRoomNumMembers(roomName, numMembers);
    }

    string newRoomRoomName;
    bool newRoomIsPublic;
    //When create new vc room button is pressed
    public void OnCreateNewVCRoom()
    {
        //check if roomName is unique:
        newRoomRoomName = vcRoomCreateField.text.Trim(); //remove spaces beginning & end
        newRoomIsPublic = vcRoomPublicSwitch.isOn;
        if (newRoomRoomName.Length == 0)
        {
            string msg = "Sorry, the room name cannot be empty.";
            StartCoroutine(DisplaySearchMessage(3, msg));
        }
        dbc.CheckVCRoomExists(newRoomRoomName, "CreateRoomCheck"); //initiates second half check.
    }
    public void OnCreateNewVCRoomSecondHalf(bool doesNameExist) //this is called by DBC callback.
    {
        if(doesNameExist)
        {
            //roomname already exists, go tell user...
            string msg = "Sorry, this room name already exists!";
            StartCoroutine(DisplaySearchMessage(3, msg));
        }
        else
        {
            dbc.CreateNewVCRoom(newRoomRoomName, myID, newRoomIsPublic); //member default to 1
            OnCreateVCRoomButtonClicked(); //auto closes after room creation

            prevRoomName = newRoomRoomName;
            InitializeVideoChatRoomPanel(new List<string>()); //pass in an empty list, so nothing to connect to => create
        }
    }

    //On search field value changed, execute this. (Gonna refresh everytime?)
    public void ListVCRoomsWithKeyword()
    {
        if (vcRoomSearchField.text.Trim().Length == 0)
        {
            OnRefreshVCRoomButtonPressed();
            return; //this counts as default: nothing
        }

        dbc.GrabAllVCRoomInfo("VCRoomKeyword"); //grab newest info first, triggers callback
    }
    public void ListVCRoomsWithKeywordSecondHalf(hirebeatprojectdb_videochatsavailable[] dbRooms) //called by callback.
    {
        //Turn all these info into actual stuff
        UpdateVCRoomList(dbRooms);

        string keyword = vcRoomSearchField.text.Trim();
        var roomNames = vcRoomList.Keys.ToList();

        //Then sort through deletion. (Any better methods?)
        if (publicSwitch.isOn)
        {
            foreach (var roomName in roomNames)
            {
                if (!roomName.Contains(keyword) || !vcRoomList[roomName].isPublic) //Won't do startWith, maybe give them a higher priority? idk
                {
                    Destroy(vcRoomList[roomName].roomDisplayTab.gameObject);
                    vcRoomList.Remove(roomName);
                }
            }
        }
        else if (privateSwitch.isOn)
        {
            foreach (var roomName in roomNames)
            {
                if (!roomName.Contains(keyword) || vcRoomList[roomName].isPublic) //Won't do startWith, maybe give them a higher priority? idk
                {
                    Destroy(vcRoomList[roomName].roomDisplayTab.gameObject);
                    vcRoomList.Remove(roomName);
                }
            }
        }
        else
        {
            foreach (var roomName in roomNames)
            {
                if (!roomName.Contains(keyword)) //Won't do startWith, maybe give them a higher priority? idk
                {
                    Destroy(vcRoomList[roomName].roomDisplayTab.gameObject);
                    vcRoomList.Remove(roomName);
                }
            }
        }
    }

    //This will delete all the rooms except the target, if the target exists. 
    //This should grab roomInfo based on roomName. Call this on when Enter is pressed in search box.
    string searchRoomRoomName;
    public void SearchSpecificRoom(string roomName)
    {
        searchRoomRoomName = roomName.Trim();

        if (searchRoomRoomName.Length == 0)
        {
            string msg = "Sorry, the room you enter cannot be empty.";
            StartCoroutine(DisplaySearchMessage(3, msg));
            return;
        }

        foreach (var rNam in vcRoomList.Keys.ToList()) //delete everything first, committed
        {
            Destroy(vcRoomList[rNam].roomDisplayTab.gameObject);
            vcRoomList.Remove(rNam);
        }

        dbc.CheckVCRoomExists(searchRoomRoomName, "SearchRoomCheck"); //initiates second half callback
    }
    public void SearchSpecificRoomSecondHalf(bool doesNameExist) //this is called by the dbc room exist callback
    {
        if (!doesNameExist)
        {
            //do something to show that the room doesn't exist
            string msg = "Sorry, the room you entered does not exist.";
            StartCoroutine(DisplaySearchMessage(3, msg));
        }
        else
        {
            dbc.RetrieveVCRoomInfo(searchRoomRoomName, "SearchRoomCheck"); //retrieve that specific room, this creates third callback
        }
            
    }
    public void SearchSpecificRoomThirdHalf(hirebeatprojectdb_videochatsavailable targetRoom) //dbc calls this with retrieving specific room info
    {
        if(targetRoom == null)
        {
            Debug.LogError("This room no longer exists"); //is this the right way? is this needed here?
            return;
        }

        AddNewRoomToList(searchRoomRoomName, targetRoom.CurrOwnerID, targetRoom.NumMembers, targetRoom.IsPublic);

        //when you want a room, disregard it's status so more convenient.
        if (publicSwitch.isOn)
        {
            comeFromSpecSearch = true;
            publicSwitch.isOn = false;
        }
        else if (privateSwitch.isOn)
        {
            comeFromSpecSearch = true;
            privateSwitch.isOn = false;
        }
        //privateSwitch.isOn = false; //they triggered on value changed...
    }

    IEnumerator DisplaySearchMessage(float time, string message)
    {
        searchMessage.gameObject.SetActive(true);
        searchMessage.text = message;
        yield return new WaitForSeconds(time);
        searchMessage.gameObject.SetActive(false);
        searchMessage.text = ""; //this is pointless
    }

    bool comeFromSpecSearch = false; //don't want this to trigger again after disable, from spec search
    //Alternative: A good idea for public/private: instead of grab then destroy extra, use grab to directly grab non extra? (A bit more work tho, ehhh we’ll see)
    public void SortVCRoomsByPublicity() //xor, if both off then default! 
    {
        if(comeFromSpecSearch)
        {
            comeFromSpecSearch = false;
            return;
        }

        if (!(vcRoomSearchField.text.Trim().Length == 0)) { //if user has something, then allow the switch to filter too!
            ListVCRoomsWithKeyword();
            return;
        }

        //First, grab the newest info
        dbc.GrabAllVCRoomInfo("VCRoomPublicity"); //also triggers callback.
    }
    public void SortVCRoomsByPublicitySecondHalf(hirebeatprojectdb_videochatsavailable[] dbRooms) //this is called by callback from prev.
    {
        //Turn all these info into actual stuff
        UpdateVCRoomList(dbRooms); 

        //Then sort through deletion! (Is sorting through addition better?)
        if (publicSwitch.isOn) //destroy all private tabs
        {
            var roomNames = vcRoomList.Keys.ToList();
            foreach (var roomName in roomNames)
            {
                if (!vcRoomList[roomName].isPublic)
                {
                    Destroy(vcRoomList[roomName].roomDisplayTab.gameObject);
                    vcRoomList.Remove(roomName);
                }
            }
        }
        else if (privateSwitch.isOn) //destroy all public tabs
        {
            var roomNames = vcRoomList.Keys.ToList();
            foreach (var roomName in roomNames)
            {
                if (vcRoomList[roomName].isPublic)
                {
                    Destroy(vcRoomList[roomName].roomDisplayTab.gameObject);
                    vcRoomList.Remove(roomName);
                }
            }
        }
    }


    private void AddNewRoomToList(string roomName, string currOwnerID, int numMembers, bool isPublic)
    {
        var newVCRoomDisplay = Instantiate(videoChatRoomDisplayPrefab, vcRoomDisplayPanel);
        newVCRoomDisplay.name = prefix + roomName;
        newVCRoomDisplay.GetComponent<VidCDisplayTab>().SetRoomInfo(roomName, numMembers, isPublic, currOwnerID);
        vcRoomList.Add(roomName, new VidCRoomInfo(currOwnerID, numMembers, isPublic, newVCRoomDisplay.GetComponent<VidCDisplayTab>()));
    }

    private void UpdateRoomInfo(string roomName, string currOwnerID, int numMembers, bool isPublic) //isPublic basically stays the same...
    {
        var info = vcRoomList[roomName]; //I think this is by reference eh?
        if(info.currOwnerID != currOwnerID)
        {
            info.currOwnerID = currOwnerID;
            info.roomDisplayTab.UpdateCurrOwnerID(currOwnerID); //run test.
        }
        if(info.numMembers != numMembers)
        {
            info.numMembers = numMembers;
            info.roomDisplayTab.UpdateNumMembers(numMembers);
        }
    }

    //Need to check vcRoomList room names against the data base ver.: if in room and not in data base then remove, if in data base and
    //not in room then add, if in both then update.
    public void UpdateVCRoomList(hirebeatprojectdb_videochatsavailable[] dbRooms)
    {
        //Is doing the below more efficient than two nested forloops?
        var dbRoomsConverted = ConvertToReadableFormat(dbRooms);
        List<string> listRoomNames = vcRoomList.Keys.ToList();
        List<string> dbRoomNames = dbRoomsConverted.Keys.ToList(); //dbRooms.Select(r => r.RoomName).ToList(); //Select(r => (string)r["RoomName"]).ToList(); //was a list of dicts
        List<string> ToBeUpdated = listRoomNames.Intersect(dbRoomNames).ToList();
        List<string> ToBeDeleted = listRoomNames.Except(ToBeUpdated).ToList();
        List<string> ToBeAdded = dbRoomNames.Except(ToBeUpdated).ToList();
        foreach (string roomName in ToBeDeleted)
        {
            Destroy(vcRoomList[roomName].roomDisplayTab.gameObject);
            vcRoomList.Remove(roomName);
        }
        foreach (string roomName in ToBeUpdated)
        {
            var newInfo = dbRoomsConverted[roomName];
            UpdateRoomInfo(roomName, newInfo.currOwnerID, newInfo.numMembers, newInfo.isPublic);    //dbRooms.Find()
        }
        foreach (string roomName in ToBeAdded)
        {
            var newInfo = dbRoomsConverted[roomName];
            AddNewRoomToList(roomName, newInfo.currOwnerID, newInfo.numMembers, newInfo.isPublic);
        }
    }

    private Dictionary<string, VidCRoomInfo> ConvertToReadableFormat(hirebeatprojectdb_videochatsavailable[] dbRooms)
    {
        Dictionary<string, VidCRoomInfo> result = new Dictionary<string, VidCRoomInfo>();
        foreach (var dbRoom in dbRooms) //can do some arrangements here maybe
        {
            string roomName = dbRoom.RoomName;
            string currOwnerID = dbRoom.CurrOwnerID;
            int numMembers = dbRoom.NumMembers;
            bool isPublic = dbRoom.IsPublic;

            result.Add(roomName, new VidCRoomInfo(currOwnerID, numMembers, isPublic));
        }
        return result;
    }

    #region Pre Meeting Settings
    private void SaveSettings()
    {
        PlayerPrefsSetBool(mPrefix + PREF_AUDIO, uAudioToggle.isOn);
        PlayerPrefsSetBool(mPrefix + PREF_VIDEO, uVideoToggle.isOn);
        PlayerPrefs.SetString(mPrefix + PREF_VIDEODEVICE, GetSelectedVideoDevice());
        PlayerPrefs.SetString(mPrefix + PREF_IDEALWIDTH, uIdealWidth.text);
        PlayerPrefs.SetString(mPrefix + PREF_IDEALHEIGHT, uIdealHeight.text);
        PlayerPrefs.SetString(mPrefix + PREF_IDEALFPS, uIdealFps.text);
        PlayerPrefs.SetInt(mPrefix + PREF_FORMAT, uFormatDropdown.value);

        PlayerPrefs.Save();
    }

    /// <summary>
    /// Loads the ui state from last use
    /// </summary>
    private void LoadSettings()
    {
        //0 is on, 1 is off
        ChangeAudioToggle(PlayerPrefsGetBool(mPrefix + PREF_AUDIO, true));
        ChangeVideoToggle(PlayerPrefsGetBool(mPrefix + PREF_VIDEO, true));

        /*uAudioToggle.isOn = PlayerPrefsGetBool(mPrefix + PREF_AUDIO, true);
        uVideoToggle.isOn = PlayerPrefsGetBool(mPrefix + PREF_VIDEO, true);*/
        //can't select this immediately because we don't know if it is valid yet
        mStoredVideoDevice = PlayerPrefs.GetString(mPrefix + PREF_VIDEODEVICE, null);
        uIdealWidth.text = PlayerPrefs.GetString(mPrefix + PREF_IDEALWIDTH, "320");
        uIdealHeight.text = PlayerPrefs.GetString(mPrefix + PREF_IDEALHEIGHT, "240");
        uIdealFps.text = PlayerPrefs.GetString(mPrefix + PREF_IDEALFPS, "30");
        uFormatDropdown.value = PlayerPrefs.GetInt(mPrefix + PREF_FORMAT, 0);

        //and here are some default settings that must be true
        bool useEchoCancellation = true;
        if (useEchoCancellation)
        {
#if (!UNITY_WEBGL && !UNITY_WSA)
            var nativeConfig = new Byn.Awrtc.Native.NativeMediaConfig();
            nativeConfig.AudioOptions.echo_cancellation = true;

            mediaConf = nativeConfig;
#endif
        }

#if UNITY_WSA && !UNITY_EDITOR
        var uwpConfig = new Byn.Awrtc.Uwp.UwpMediaConfig();
        uwpConfig.Mrc = true;
        //uwpConfig.ProcessLocalFrames = false;
        //uwpConfig.DefaultCodec = "H264";
        mediaConf = uwpConfig;
        Debug.Log("Using uwp specific media config: " + mediaConfig);
#endif

        //mediaConf.Format = FramePixelFormat.ABGR;

        mediaConf.MinWidth = 160;
        mediaConf.MinHeight = 120;
        //Larger resolutions are possible in theory but
        //allowing users to set this too high is risky.
        //A lot of devices do have great cameras but not
        //so great CPU's which might be unable to
        //encode fast enough.
        mediaConf.MaxWidth = 1920 * 2;
        mediaConf.MaxHeight = 1080 * 2;
    }

    //I configurated the buttons in editor so that they impact isOn.
    //0 is on button, 1 is off button. Click on on leads to off, off leads to on.
    private void ChangeAudioToggle(bool value)
    {
        if (value) uAudioToggle.transform.GetChild(1).GetComponent<Button>().onClick.Invoke();
        else uAudioToggle.transform.GetChild(0).GetComponent<Button>().onClick.Invoke();
    }
    private void ChangeVideoToggle(bool value)
    {
        if (value) uVideoToggle.transform.GetChild(1).GetComponent<Button>().onClick.Invoke();
        else uVideoToggle.transform.GetChild(0).GetComponent<Button>().onClick.Invoke();
    }

    private static void PlayerPrefsSetBool(string name, bool value)
    {
        PlayerPrefs.SetInt(name, value ? 1 : 0);
    }

    private static bool PlayerPrefsGetBool(string name, bool defval)
    {
        int def = 0;
        if (defval)
            def = 1;
        return PlayerPrefs.GetInt(name, def) == 1 ? true : false;
    }

    private string GetSelectedVideoDevice()
    {
        if (uVideoDropdown.value <= 0 || uVideoDropdown.value >= uVideoDropdown.options.Count)
        {
            //return null if the first element is selected ("Any") or the ui returns
            //invalid values. This will trigger the app to pick a default device
            return null;
        }
        else
        {
            string devname = uVideoDropdown.options[uVideoDropdown.value].text;
            return devname;
        }
    }

    public void ResetSettings()
    {
        PlayerPrefs.DeleteKey(mPrefix + PREF_AUDIO);
        PlayerPrefs.DeleteKey(mPrefix + PREF_VIDEO);
        PlayerPrefs.DeleteKey(mPrefix + PREF_VIDEODEVICE);
        PlayerPrefs.DeleteKey(mPrefix + PREF_IDEALWIDTH);
        PlayerPrefs.DeleteKey(mPrefix + PREF_IDEALHEIGHT);
        PlayerPrefs.DeleteKey(mPrefix + PREF_IDEALFPS);
        PlayerPrefs.DeleteKey(mPrefix + PREF_FORMAT);
        LoadSettings();
        CheckSettings();
    }

    private void CheckSettings() 
    {
        if (ExampleGlobals.HasAudioPermission() == false)
        {
            ChangeAudioToggle(false);
        }
        if (ExampleGlobals.HasVideoPermission() == false)
        {
            //uVideoToggle.isOn = false;
            ChangeVideoToggle(false);
        }
    }

    private void InitFormatDropdown()
    {
        uFormatDropdown.ClearOptions();
        var formats = ExampleGlobals.PixelFormats;
        var options = new List<string>();
        foreach (var v in formats)
        {
            options.Add(v.ToString());
        }
        uFormatDropdown.AddOptions(options);
    }

    private FramePixelFormat GetSelectedFormat()
    {
        int index = uFormatDropdown.value;
        if (index < 0 || index >= ExampleGlobals.PixelFormats.Length)
        {
            index = 0;
        }
        return ExampleGlobals.PixelFormats[index];
    }

    public void OnAudioSettingsChanged() //I don't think on value change reacts...
    {
        if (uAudioToggle.isOn && ExampleGlobals.HasAudioPermission() == false)
        {
            StartCoroutine(RequestAudioPermissions());
        }
    }

    public void OnVideoSettingsChanged()
    {
        if (uVideoToggle.isOn && ExampleGlobals.HasVideoPermission() == false)
        {
            StartCoroutine(RequestVideoPermissions());
        }
    }

    IEnumerator RequestAudioPermissions()
    {
        yield return ExampleGlobals.RequestAudioPermission();
        ChangeAudioToggle(ExampleGlobals.HasAudioPermission());
    }
    IEnumerator RequestVideoPermissions()
    {
        yield return ExampleGlobals.RequestVideoPermission();
        ChangeVideoToggle(ExampleGlobals.HasVideoPermission());
    }

    private static int TryParseInt(string value, int defval)
    {
        int result;
        if (int.TryParse(value, out result) == false)
        {
            result = defval;
        }
        return result;
    }

    /// <summary>
    /// Turns on sending audio for the next call.
    /// </summary>
    /// <param name="value"></param>
    public void SetAudio(bool value)
    {
        mediaConf.Audio = value;
    }
    /// <summary>
    /// Turns on sending video for the next call.
    /// </summary>
    /// <param name="value"></param>
    public void SetVideo(bool value)
    {
        mediaConf.Video = value;
    }

    /// <summary>
    /// Sets a different format. 
    /// Experimental use only. Most formats only work on specific platforms / specific setups. 
    /// </summary>
    /// <param name="format"></param>
    public void SetFormat(FramePixelFormat format)
    {
        mediaConf.Format = format;
    }
    /// <summary>
    /// Allows to set a specific video device.
    /// This isn't supported on WebGL yet.
    /// </summary>
    /// <param name="deviceName"></param>
    public void SetVideoDevice(string deviceName)
    {
        mediaConf.VideoDeviceName = deviceName;
    }

    /// <summary>
    /// Changes the target resolution that will be used for
    /// sending video streams.
    /// The closest one the camera can handle will be used.
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public void SetIdealResolution(int width, int height)
    {
        mediaConf.IdealWidth = width;
        mediaConf.IdealHeight = height;
    }

    /// <summary>
    /// Sets the ideal FPS.
    /// This has a lower priority than the ideal resolution.
    /// Note that the FPS aren't enforced. It pick
    /// the closest FPS the video device supports.
    /// </summary>
    /// <param name="fps"></param>
    public void SetIdealFps(int fps)
    {
        mediaConf.IdealFrameRate = fps;
    }

    /// <summary>
    /// Updates the dropdown menu based on the current video devices and toggle status
    /// </summary>
    public void UpdateVideoDropdown()
    {
        uVideoDropdown.ClearOptions();
        uVideoDropdown.AddOptions(new List<string>(GetVideoDevices()));
        uVideoDropdown.interactable = CanSelectVideoDevice();

        //restore the stored selection if possible
        if (uVideoDropdown.interactable && mStoredVideoDevice != null)
        {
            int index = 0;
            foreach (var opt in uVideoDropdown.options)
            {
                if (opt.text == mStoredVideoDevice)
                {
                    uVideoDropdown.value = index;
                }
                index++;
            }
        }
    }

    /// <summary>
    /// Returns a list of video devices for the UI to show.
    /// This is used to avoid having the UI directly access the UnityCallFactory.
    /// </summary>
    /// <returns></returns>
    public string[] GetVideoDevices()
    {
        if (CanSelectVideoDevice())
        {
            List<string> devices = new List<string>();
            string[] videoDevices = UnityCallFactory.Instance.GetVideoDevices();
            devices.Add("Any");
            devices.AddRange(videoDevices);
            return devices.ToArray();
        }
        else
        {
            return new string[] { "Default" };
        }
    }

    /// <summary>
    /// Used by the UI
    /// </summary>
    /// <returns></returns>
    public bool CanSelectVideoDevice()
    {
        return UnityCallFactory.Instance.CanSelectVideoDevice();
    }

    /// <summary>
    /// Shows the setup screen or the chat + video
    /// </summary>
    /// <param name="showSetup">true Shows the setup. False hides it.</param> called after Factory ready
    public void InitializeSettingsView() //call this thing with the button.
    {
        //fill the video dropbox
        UpdateVideoDropdown();
        InitFormatDropdown();
        if (uLoadSettings)
        {
            LoadSettings();
        }
        CheckSettings();
        uSetupPanel.gameObject.SetActive(true);
        //this is going to hide the textures until it is updated with a new frame update
        /*UpdateLocalTexture(null);
        UpdateRemoteTexture(null);*/
    }

    //Save settings for future session! This applies the answers to mediaconf.
    private void SaveMediaConfSettings() //this is only called if the enter in the settings tab is clicked
    {
        SetVideoDevice(GetSelectedVideoDevice());
        SetAudio(uAudioToggle.isOn);
        SetVideo(uVideoToggle.isOn);
        SetFormat(GetSelectedFormat());

        int width = TryParseInt(uIdealWidth.text, 320);
        int height = TryParseInt(uIdealHeight.text, 240);
        int fps = TryParseInt(uIdealFps.text, 30);
        SetIdealResolution(width, height);
        SetIdealFps(fps);
        //SetupCommunicator(); //this puts the mediaconf setting into communicator. This should ONLY be set at room join. 
    }

    /// <summary>
    /// Join button pressed. Tries to join a room. NOT ANYMORE! This is like save settings more like.
    /// </summary>
    public void SaveSettingsButtonPressed()
    {
        //remember for next run
        if (CheckInputValidity())
        {
            mStoredVideoDevice = GetSelectedVideoDevice();
            SaveSettings();
            SaveMediaConfSettings();
            uSetupPanel.gameObject.SetActive(false);
        }
    }

    private bool CheckInputValidity()
    {
        int width = TryParseInt(uIdealWidth.text, 320);
        int height = TryParseInt(uIdealHeight.text, 240);
        if (width > 960 || height > 540) //local client can go 1920 x 1080 +, but webgl sucks so...
        {
            //display error message
            string errorMsg = "Please keep your width not exceeding 960 and your height not exceeding 540. " +
                "Multiples of such work as well. " +
                "The smaller the resolution, the smoother the session!";
            StartCoroutine(DisplayErrorMessage(3f, errorMsg));
            return false;
        }
        return true;
    }

    IEnumerator DisplayErrorMessage(float time, string msg)
    {
        errorMessageObject.text = msg;
        yield return new WaitForSeconds(time);
        errorMessageObject.text = "";
    }
    #endregion 
}
