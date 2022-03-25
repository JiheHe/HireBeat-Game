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

    DataBaseCommunicator dbc; //the real time database!

    void Start() //conf the permanent netConf at start.
    {
        //STEP1: instance setup
        netConf = new NetworkConfig();
        netConf.SignalingUrl = ExampleGlobals.Signaling; //will change the urls and servers later, post testing
        netConf.IceServers.Add(new IceServer(ExampleGlobals.StunUrl));

        dbc = GameObject.FindGameObjectWithTag("DataCenter").GetComponent<DataBaseCommunicator>();
        vcRoomList = new Dictionary<string, VidCRoomInfo>(); //key will be roomName, it stays fixed.
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.O))
        {
            InitializeVideoChatRoomPanel(); //this is for testing only.
        }
        if(Input.GetKeyDown(KeyCode.T))
        {
            dbc.CreateNewVCRoom("test", "asbfasd", true);
            dbc.CreateNewVCRoom("IamJack", ";fsdafasd", false);
        }
    }

    private void InitializeVideoChatRoomPanel()
    {
        //initialize the object, set parent to social sytem
        vCC = Instantiate(videoChatRoomPanelPrefab, transform.parent.GetComponent<RectTransform>()).GetComponent<VideoChatController>();
        vCC.gameObject.transform.localPosition = new Vector2(-380f, 0f); //since we start with textchat.
        //set this.mediaConf to the object's
        //set this netConf to the object's
        vCC.SetupMediaAndNetConfAndOther(mediaConf, netConf, this);
        //intialize object's userInRoomIds (or not, if you are creating a new room) //no for now, testing
        //announce new room to public in here.
        vCC.StartRoomCreateOrJoinProcess();

        gameObject.SetActive(false);
    }

    public void OnConnectPressed(string targetOwnerID) //address should be HireBeatProjVidC + myID //this is to avoid connect to other
    {
        //Then use Photon Chat to request the list of userInRoomIDs from the owner and send them through messages. 
    }

    #region Buttons
    public void CloseVideoChatRoomSearchPanel()
    {
        gameObject.SetActive(false);
    }

    public void OnRefreshVCRoomButtonPressed()
    {
        dbc.GrabAllVCRoomInfo();
    }
    #endregion 

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
