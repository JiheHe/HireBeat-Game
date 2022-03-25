using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Byn.Awrtc;
using Byn.Awrtc.Unity;
using Byn.Unity.Examples;
using UnityEngine.UI;

public class VideoChatController : MonoBehaviour
{
    [Header("Statistic Data Grab")]
    const int maxUsersInRoom = 6;
    //If connectionId is > maxUsersInRoom - 1 (because excluding yourself), then you are full!
    //The dictionary below might be useless, because such data is already stored in remote panel info script object... delte if no use
    string[] userInRoomIds; //this is to be populated, send connect request to each at end step.
    private Dictionary<ConnectionId, string> connectionIdWithPlayFabId = new Dictionary<ConnectionId, string>(); //link connection id to playfab through msg.

    [Header("Video chat panel")]
    IMediaNetwork communicator; //no need for sender and receiver! the receiver in the example is for 1 to N, as said in email. One network is enough.

    private NetworkConfig netConf;
    private string myID;
    private string selfAddress;

    /// <summary>
    /// Media configuration. Will be set during setup.
    /// </summary>
    private MediaConfig mediaConf = new MediaConfig();

    /// <summary>
    /// Can be used to keep track of each connection. 
    /// </summary>
    private List<ConnectionId> mConnectionIds = new List<ConnectionId>();

    // I understood!
    private Dictionary<ConnectionId, RawImage> uVideoOutputs = new Dictionary<ConnectionId, RawImage>();
    public GameObject remoteDisplayObject; //this is a prefab
    public RawImage localDisplayPanel; //this is a legit gameobject

    public RectTransform parentPanel; //this is proportionalView
    public RectTransform speakerPanel; //this is speakerView
    bool inProportionalView = true; //this is default to true at room creation / join.
    ConnectionId currentSpeaker = ConnectionId.INVALID; //invalid strictly means none here

    /// <summary>
    /// Helper to give each instance an id to print via log output
    /// </summary>
    private int mIndex = 0;

    /// <summary>
    /// Texture2D used as buffer for local or remote video
    /// </summary>
    private Texture2D mVideoTexture; //should I make an array of this too? Having 1 buffering everything... overworked

    [Header("Text chat panel")]
    public GameObject vidCTextChatObj;
    public VidCTextChat textChatController;

    /// <summary>
    /// Input field to enter a new message.
    /// </summary>
    public InputField uMessageInput;

    /// <summary>
    /// Send button.
    /// </summary>
    public Button uSend;

    PersistentData pd; //we can grab username etc from here! username is updated in here too I believe. 

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

    //Here's how the process works: Join room clicked -> if ok -> initialize VC system -> settings panel -> set up communicator -> connection statements.

    // Start is called before the first frame update
    void Start()
    {
        pd = GameObject.Find("PersistentData").GetComponent<PersistentData>();

        myID = GameObject.Find("PlayFabController").GetComponent<PlayFabController>().myID;
        selfAddress = "HireBeatProjVidC" + myID; //no need for Application.productName

        OnCreatePressed(); //for testing.
        SetCellSizeBasedOnNum(); //for testing too.
    }

    public void OnCreatePressed() //you are the owner that makes a new room!
    {
        UnityCallFactory.EnsureInit(OnCallFactoryReady, OnCallFactoryFailed); //initializes your own server.

        //and find some ways to announce your new room to the public! 
    }

    public void OnConnectPressed(string targetOwnerID) //address should be HireBeatProjVidC + myID //this is to avoid connect to other
    {
        //Then use Photon Chat to request the list of userInRoomIDs from the owner and send them through messages. 
    }

    public void SendNoticeToCurrRoomUsers(string[] userInRoomIDs) //this method is ONLY CALLED when photon chat receives the message from owner and has finished setting up the array list
    {
        if (userInRoomIDs == null) //set it to null if the room is full... this is the special message.
        {
            Debug.LogError("My custom message: Room is full! 6/6 users."); //Add some sort of display in the future to tell the user.
            return;
        } //else not full! Actual list:

        //Set up your own personal server first
        UnityCallFactory.EnsureInit(OnCallFactoryReady, OnCallFactoryFailed);

        //It's better to wait till self communicator is ready, then connect to every user in userInRoomIDs.
        userInRoomIds = userInRoomIDs; //so we gonna store it here, call at the end.

        //But user should also send a message to everyone, telling them that a person is trying to join. 

        //Don't forget to include owner's ID in the result too! (from sender's end).
        /*foreach (string userID in userInRoomIDs) //this whole is below is useless... new connection already covered.
        {
            //Send the photon chat message to that ID, a new user has joined!.

            //Also initializes a new display raw image for it //this should be covered from getting a new connection.
        }*/
    }

    void OnCallFactoryReady()
    {
        UnityCallFactory.Instance.RequestLogLevel(UnityCallFactory.LogLevel.Info);
        InitVidCSystem();
        SetGuiState(true); //bring up the vc settings stuff.
    }

    void OnCallFactoryFailed(string error)
    {
        string fullErrorMsg = typeof(CallApp).Name + " can't start. The " + typeof(UnityCallFactory).Name + " failed to initialize with following error: " + error;
        Debug.LogError(fullErrorMsg);
    }

    void InitVidCSystem()
    {
        //STEP1: instance setup
        netConf = new NetworkConfig();
        netConf.SignalingUrl = ExampleGlobals.Signaling; //will change the urls and servers later, post testing
        netConf.IceServers.Add(new IceServer(ExampleGlobals.StunUrl));
        communicator = UnityCallFactory.Instance.CreateMediaNetwork(netConf);
        mediaConf = new MediaConfig();
        //Then the settings tab pop out here.
        //SetupCommunicator(); //this should be called when the user presses green check arrow from settings
    }

    private void SetupCommunicator()
    {
        Debug.Log("communicator setup");
        //communicator = UnityCallFactory.Instance.CreateMediaNetwork(netConf); 
        //mediaConf = CreateMediaConfig();

        //make a deep clone to avoid confusion if settings are changed
        //at runtime. 
        MediaConfig mMediaConfigInUse = mediaConf.DeepClone();

        //try to pick a good default video device if the user wants to send video but
        //didn't bother to pick a specific device
        if (mMediaConfigInUse.Video && string.IsNullOrEmpty(mMediaConfigInUse.VideoDeviceName))
        {
            string[] devices = UnityCallFactory.Instance.GetVideoDevices();
            if (devices == null || devices.Length == 0)
            {
                Debug.Log("no device found or no device information available");
            }
            else
            {
                foreach (string s in devices)
                    Debug.Log("device found: " + s + " IsFrontFacing: " + UnityCallFactory.Instance.IsFrontFacing(s));
            }
            mMediaConfigInUse.VideoDeviceName = UnityCallFactory.Instance.GetDefaultVideoDevice();
        }

        Debug.Log("Configure call using MediaConfig: " + mMediaConfigInUse);
        communicator.Configure(mMediaConfigInUse);

        if (communicator.GetConfigurationState() == MediaConfigurationState.Failed)
        {
            //did configuration fail? error
            Debug.Log("communicator configuration failed " + communicator.GetConfigurationError());
            communicator.ResetConfiguration();
            SetupCommunicator(); //is this recursion bug-free? //I added this line, might be bug idk
        }
        else
        {
            //configuration successful.
            //StartServer corresponds to ICall.Listen
            communicator.StartServer(selfAddress); //Starting a self-server with my own address!
            SetGuiState(false); //turn of settings. Done!
            Debug.Log("Starting self address!");

            foreach(string id in userInRoomIds)
            {
                ConnectToVidCAddress(id); //then connect to each target in room.
            }
        }
    }

    //This is for testing purposes: direct address connecting. will delete?
    public void ConnectToVidCAddress(string targetUserID)
    {
        communicator.Connect("HireBeatProjVidC" + targetUserID);
        Debug.Log("VidCAddressSubmitted to " + targetUserID);
    }


    #region Update and Handle
    // Update is called once per frame
    void Update()
    {
        if (communicator == null)
            return;
        communicator.Update();

        //This is the event handler via polling.
        //This needs to be called or the memory will fill up with unhanded events!
        NetworkEvent evt;
        while (communicator != null && communicator.Dequeue(out evt))
        {
            HandleNetworkEvent(evt);
        }
        //polls for video updates
        HandleMediaEvents();

        //Flush will resync changes done in unity to the native implementation
        //(and possibly drop events that aren't handled in the future)
        if (communicator != null)
            communicator.Flush();
    }

    /// <summary>
    /// Handler polls the media network to check for new video frames.
    /// 
    /// </summary>
    protected virtual void HandleMediaEvents()
    {
        //just for debugging
        bool handleLocalFrames = true;
        bool handleRemoteFrames = true;

        if (communicator != null && handleLocalFrames)
        {
            IFrame localFrame = communicator.TryGetFrame(ConnectionId.INVALID); //invalid means local!
            if (localFrame != null)
            {
                UpdateTexture(localFrame, ConnectionId.INVALID);

            }
        }
        if (communicator != null && handleRemoteFrames)
        {
            //so far the loop shouldn't be needed. we only expect one // now it's needed
            foreach (var id in mConnectionIds)
            {
                if (communicator != null)
                {
                    IFrame remoteFrame = communicator.TryGetFrame(id);
                    if (remoteFrame != null)
                    {
                        UpdateTexture(remoteFrame, id);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Log method to help seeing what each of the different apps does.
    /// </summary>
    /// <param name="txt"></param>
    private void Log(string txt)
    {
        Debug.Log("Instance " + mIndex + ": " + txt);
    }

    /// <summary>
    /// Method is called to handle the network events triggered by the internal media network and 
    /// trigger related event handlers for the call object.
    /// </summary>
    /// <param name="evt"></param>
    protected virtual void HandleNetworkEvent(NetworkEvent evt)
    {
        /*if (communicator.GetConfigurationState() == MediaConfigurationState.Failed)
        {
            //did configuration fail? error
            Debug.Log("communicator configuration failed " + communicator.GetConfigurationError());
            communicator.ResetConfiguration();
        }
        else if (communicator.GetConfigurationState() == MediaConfigurationState.Successful
            && mCommunicatorConfigured == false)
        {
            //configuration successful.
            mCommunicatorConfigured = true;
            //StartServer corresponds to ICall.Listen
            communicator.StartServer(selfAddress); //Starting a self-server with my own address!
            Debug.LogError("Starting self address!");
        }*/

        switch (evt.Type)
        {
            case NetEventType.NewConnection:
                //'-' is indicator for playfab id msg, "," is splitter, and 2nd part is content.
                byte[] msgData = Encoding.UTF8.GetBytes("-," + myID); //This is to tell the person you just connected to who are you.
                communicator.SendData(evt.ConnectionId, msgData, 0, msgData.Length, true);

                //This part adds video panel
                mConnectionIds.Add(evt.ConnectionId);
                GameObject newRemoteDisplay;
                if (inProportionalView)
                {
                    newRemoteDisplay = Instantiate(remoteDisplayObject, Vector2.zero, Quaternion.identity, parentPanel); //add to an equal!
                }
                else
                {
                    RectTransform leftPanel = speakerPanel.Find("LeftPanels").GetComponent<RectTransform>();
                    newRemoteDisplay = Instantiate(remoteDisplayObject, Vector2.zero, Quaternion.identity, leftPanel);
                    newRemoteDisplay.GetComponent<RectTransform>().sizeDelta = new Vector2(320f, 180f); //add to a small!
                }
                newRemoteDisplay.GetComponent<VidCRemoteInfo>().userConnectionID = evt.ConnectionId;
                newRemoteDisplay.GetComponent<VidCRemoteInfo>().vidCController = this;
                uVideoOutputs.Add(evt.ConnectionId, newRemoteDisplay.transform.GetChild(0).GetComponent<RawImage>()); //I see!

                SetCellSizeBasedOnNum(); //this wouldn't hurt no matter what view u in.

                Debug.Log("New connection id " + evt.ConnectionId); 
                /*if (uSender == false)
                    this.GetComponent<Image>().color = Color.green; //receiving*/

                break;
            case NetEventType.ConnectionFailed:
                //call failed
                Log("Outgoing connection failed");
                /*if (uSender == false) //add in future
                    this.GetComponent<Image>().color = new Color(0, 0.125f, 0, 1); //receiver failed to connect*/
                break;
            case NetEventType.Disconnected: 

                if (mConnectionIds.Contains(evt.ConnectionId))
                {
                    mConnectionIds.Remove(evt.ConnectionId);
                    Destroy(uVideoOutputs[evt.ConnectionId].gameObject.transform.parent.gameObject); //I see!
                    uVideoOutputs.Remove(evt.ConnectionId);

                    if (!inProportionalView && currentSpeaker == evt.ConnectionId) //if in speaker && the person leaving is speaker
                    {
                        ActivateSpeakerView(true, ConnectionId.INVALID); //doesn't matter id, set yourself to speaker, a refresh
                    }
                    SetCellSizeBasedOnNum(); //this wouldn't hurt no matter what view u in.

                    Log("Connection disconnected");
                    /*if (uSender == false)
                        this.GetComponent<Image>().color = new Color(0, 0.5f, 0, 1); //receiver lost connection.  */
                }
                break;
            case NetEventType.ServerInitialized:
                //incoming calls possible
                Log("Server ready for incoming connections. Address: " + evt.Info);
                //this.GetComponent<Image>().color = Color.red;
                break;
            case NetEventType.ServerInitFailed:
                Log("Server init failed");
                //this.GetComponent<Image>().color = new Color(0.125f, 0, 0, 1); ; //server lost ability to receive connections (internet / signaling broken)
                break;
            case NetEventType.ServerClosed:
                Log("Server stopped");
                break;
            case NetEventType.ReliableMessageReceived:
            case NetEventType.UnreliableMessageReceived:
                {
                    HandleIncommingMessage(ref evt);
                }
                break;
        }
    }
    #endregion

    #region Messaging
    private void HandleIncommingMessage(ref NetworkEvent evt)
    {
        MessageDataBuffer buffer = (MessageDataBuffer)evt.MessageData;

        //we know username won't contain "," because it's alphanumeric!
        string[] msg = (Encoding.UTF8.GetString(buffer.Buffer, 0, buffer.ContentLength)).Split(new[] { ',' }, 2); //return 2 substrings by 1st occ. of ,

        //!!!!!
        //okay so usually, msg is split into part 1 sender and part 2 content, and we know sender is usually alphanumeric. But if sender contains a special
        //symbol, then we can treat it as a command!: ex. if part 1 sender contains "-", then part 2 content will be an actual playfab id, something like this.

        //Is it possible to get randy server msg? hmmm

        //if server -> forward the message to everyone else including the sender
        //we use the server side connection id to identify the client
        //ConnectionId senderId = evt.ConnectionId; //can use this to identify username, if have a list beforehand.
        //we can grab username too I believe. no need for id check.
        string username = msg[0];
        string content = msg[1];

        if(username.Contains('-')) //this is not possible for normal msg, so this indicates playfab id... might not even need photon chat for this LOL
        {
            string playfabID = content;
            connectionIdWithPlayFabId.Add(evt.ConnectionId, playfabID); //Don't forget to clear this in future! //this might be pointless, idk what for. 
            uVideoOutputs[evt.ConnectionId].transform.parent.GetComponent<VidCRemoteInfo>().userAcctID = playfabID;
        }
        else
        {
            //Then we update username and string (make into a printable obj) here.
            textChatController.AddTextEntry(username, content, false);
        }

        //return the buffer so the network can reuse it
        buffer.Dispose();
    }

    /// <summary>
    /// Sends a string as UTF8 byte array to ALL connections //no pm for now.
    /// </summary>
    /// <param name="msg">String containing the message to send</param>
    /// <param name="reliable">false to use unreliable messages / true to use reliable messages</param>
    private void SendString(string msg, bool reliable = true)
    {
        if (communicator == null)
        {
            Debug.Log("No connection. Can't send message.");
        }
        else //even if mConnections.Count == 0, you can still send! Just to yourself, tho...
        {
            byte[] msgData = Encoding.UTF8.GetBytes(msg);
            foreach (ConnectionId id in mConnectionIds)
            {
                communicator.SendData(id, msgData, 0, msgData.Length, reliable);
            }
        }
    }

    public void SendButtonPressed()
    {
        //get the message written into the text field
        string msg = uMessageInput.text;

        if (msg.Length != 0 || System.String.IsNullOrEmpty(msg))
        {
            SendString(pd.acctName + "," + msg); //first , is the splitter character.
            textChatController.AddTextEntry("You", msg, true);
            uMessageInput.text = "";
        }//no empty string spam!

        //make sure the text box is in focus again so the user can continue typing without clicking it again
        //select another element first. without this the input field is in focus after return pressed
        uSend.Select();
        uMessageInput.Select();

        /*if (msg.StartsWith("/disconnect"))
        {
            string[] slt = msg.Split(' ');
            if (slt.Length >= 2)
            {
                ConnectionId conId;
                if (short.TryParse(slt[1], out conId.id))
                {
                    mNetwork.Disconnect(conId);
                }
            }
        }*/
    }
    #endregion

    #region Videos
    /// <summary>
    /// Updates the ui with the new raw frame
    /// </summary>
    /// <param name="frame"></param>
    private void UpdateTexture(IFrame frame, ConnectionId frameId)
    {
        if (uVideoOutputs.ContainsKey(frameId) && uVideoOutputs[frameId] != null)
        {
            if (frame != null)
            {
                UpdateTexture(ref mVideoTexture, frame); //current this texture is being used n times per frame; We'll see if it's overloaded or not.
                uVideoOutputs[frameId].texture = mVideoTexture;
            }
        }
        else if (frameId == ConnectionId.INVALID && frame != null) // I SEE
        {
            UpdateTexture(ref mVideoTexture, frame); 
            localDisplayPanel.texture = mVideoTexture;
        }
    }

    /// <summary>
    /// Writes the raw frame into the given texture or creates it if null or wrong width/height.
    /// </summary>
    /// <param name="tex"></param>
    /// <param name="frame"></param>
    /// <returns></returns>
    protected bool UpdateTexture(ref Texture2D tex, IFrame frame)
    {
        bool newTextureCreated = false;
        //texture exists but has the wrong height /width? -> destroy it and set the value to null
        if (tex != null && (tex.width != frame.Width || tex.height != frame.Height))
        {
            Texture2D.Destroy(tex);
            tex = null;
        }
        //no texture? create a new one first
        if (tex == null)
        {
            newTextureCreated = true;
            Debug.Log("Creating new texture with resolution " + frame.Width + "x" + frame.Height + " Format:" + mediaConf.Format);
            if (mediaConf.Format == FramePixelFormat.ABGR)
            {
                tex = new Texture2D(frame.Width, frame.Height, TextureFormat.RGBA32, false);
            }
            else
            {
                //not yet properly supported.
                tex = new Texture2D(frame.Width, frame.Height, TextureFormat.YUY2, false);
            }
            tex.wrapMode = TextureWrapMode.Clamp;
        }
        ///copy image data into the texture and apply
        tex.LoadRawTextureData(frame.Buffer);
        tex.Apply();
        return newTextureCreated;
    }
    #endregion

    private void OnDestroy()
    {
        OnDisconnectPressed();
    }

    public void OnDisconnectPressed()
    {
        if (communicator != null)
        {
            communicator.Dispose();
            communicator = null;

            userInRoomIds = new string[] { };
            connectionIdWithPlayFabId.Clear();

            mConnectionIds = new List<ConnectionId>();

            //keep netconf: intialized once and forever at start.
            //keep mediaconf: just settings in general, save!

            //Remove the remote panels, etc //this should be covered.
        }
    }


    ///////////////////////////////////////////////////////////////////
    /// <summary>
    /// Create the default configuration for this CallApp instance.
    /// This can be overwritten in a subclass allowing the creation custom apps that
    /// use a slightly different configuration.
    /// </summary>
    /// <returns></returns>
    /*public virtual MediaConfig CreateDefaultMediaConfig()
    {
        MediaConfig mediaConfig = new MediaConfig();
        //testing echo cancellation (native only)
        bool useEchoCancellation = true;
        if (useEchoCancellation)
        {
#if (!UNITY_WEBGL && !UNITY_WSA)
            var nativeConfig = new Byn.Awrtc.Native.NativeMediaConfig();
            nativeConfig.AudioOptions.echo_cancellation = true;

            mediaConfig = nativeConfig;
#endif
        }

#if UNITY_WSA && !UNITY_EDITOR
        var uwpConfig = new Byn.Awrtc.Uwp.UwpMediaConfig();
        uwpConfig.Mrc = true;
        //uwpConfig.ProcessLocalFrames = false;
        //uwpConfig.DefaultCodec = "H264";
        mediaConfig = uwpConfig;
        Debug.Log("Using uwp specific media config: " + mediaConfig);
#endif

        //use video and audio by default (the UI is toggled on by default as well it will change on click )
        mediaConfig.Audio = true;
        mediaConfig.Video = true;
        mediaConfig.VideoDeviceName = null;

        mediaConfig.Format = FramePixelFormat.ABGR;

        mediaConfig.MinWidth = 160;
        mediaConfig.MinHeight = 120; 
        //Larger resolutions are possible in theory but
        //allowing users to set this too high is risky.
        //A lot of devices do have great cameras but not
        //so great CPU's which might be unable to
        //encode fast enough.
        mediaConfig.MaxWidth = 1920 * 2;
        mediaConfig.MaxHeight = 1080 * 2;

        //will be overwritten by UI in normal use
        mediaConfig.IdealWidth = 160;
        mediaConfig.IdealHeight = 120;
        mediaConfig.IdealFrameRate = 30;
        return mediaConfig;
    }*/

    #region In Meeting Settings
    //
    // Summary:
    //     Sets a volume for the replay of a remote connections audio stream.
    //
    // Parameters:
    //   volume:
    //     1 = normal volume, 0 = mute, everything above 1 might increase volume but reduce
    //     quality
    //
    //   remoteUserId:
    //     Id of the remote connection.
    public void SetVolume(double volume, ConnectionId remoteUserId) //currently it starts at 1 and ranges from 0 to 1.5
    {
        if (communicator != null)
        {
            communicator.SetVolume(volume, remoteUserId);
            return;
        }

        SLog.LW("SetVolume not supported", "VideoChatController WebRTC Call");
    }

    //
    // Summary:
    //     Checks if the local audio track (local microphone) is muted. True means it is
    //     muted. False means it isn't muted (via this system). This doesn't mean the microphone
    //     is actually sending. It still can be muted within the OS or via the physicial
    //     device.
    //
    // Returns:
    //     true - muted false - not muted
    public bool IsMute()
    {
        if (communicator != null)
        {
            return communicator.IsMute();
        }

        return true;
    }

    //
    // Summary:
    //     Allows to mute the local audio track (local microphone) True = mute False = send
    //     the microphone data if available
    //
    // Parameters:
    //   val:
    //     true - set to mute false - not muted
    public void SetMute(bool val)
    {
        if (communicator != null)
        {
            communicator.SetMute(val);
            
        }
    }
    #endregion

    #region View Modes
    public void SetCellSizeBasedOnNum() //this should be called after new user joins and leaves
    {
        var formatter = parentPanel.GetComponent<GridLayoutGroup>();

        //+1 because myself will always be there! 
        switch(mConnectionIds.Count + 1) //if want to adjust, use parentPanel.childCount for manual testing
        {
            case 1:
                formatter.cellSize = new Vector2(2304, 1296);
                break;
            case 2: case 3: case 4:
                formatter.cellSize = new Vector2(1152, 648); //case 1 / 2
                break;
            case 5: case 6:
                formatter.cellSize = new Vector2(768, 432); //case 1 / 3
                break;
        }
    }

    public void ActivateProportionalView()
    {
        Debug.Log("Entering proportional mode");

        inProportionalView = true;
        currentSpeaker = ConnectionId.INVALID;
        speakerPanel.gameObject.SetActive(false);
        parentPanel.gameObject.SetActive(true);
        SetCellSizeBasedOnNum();
        localDisplayPanel.transform.parent.gameObject.transform.SetParent(parentPanel); //local always first
        foreach(var remotePanel in uVideoOutputs.Values)
        {
            remotePanel.transform.parent.gameObject.transform.SetParent(parentPanel);
        }
    }

    public void ActivateSpeakerView(bool isLocal, ConnectionId id)
    {
        //if((!inProportionalView && id == currentSpeaker && localIsSpeaker == false) || (!inProportionalView && id == ConnectionId.INVALID && localIsSpeaker == true)) //first double click shapes, but another one on same reverts
        if(!inProportionalView && id == currentSpeaker) //have to be in speaker view first, no need for the complication on top LOL
        {
            ActivateProportionalView();
            return;
        }

        Debug.Log("Entering speaker mode");

        inProportionalView = false;
        speakerPanel.gameObject.SetActive(true);
        parentPanel.gameObject.SetActive(false);
        if(isLocal)
        {
            currentSpeaker = ConnectionId.INVALID; //invalid indicates self or none.
            localDisplayPanel.transform.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(1843.2f, 1036.8f);
            localDisplayPanel.transform.parent.gameObject.transform.SetParent(speakerPanel);

            RectTransform leftPanel = speakerPanel.Find("LeftPanels").GetComponent<RectTransform>();
            foreach(var remotePanel in uVideoOutputs.Values)
            {
                remotePanel.transform.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(320f, 180f);
                remotePanel.transform.parent.gameObject.transform.SetParent(leftPanel);
            }
        } 
        else
        {
            currentSpeaker = id;
            RectTransform leftPanel = speakerPanel.Find("LeftPanels").GetComponent<RectTransform>();
            localDisplayPanel.transform.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(320f, 180f);
            localDisplayPanel.transform.parent.gameObject.transform.SetParent(leftPanel); //local always first

            foreach (var connectionId in uVideoOutputs.Keys)
            {
                if (connectionId == id)
                {
                    uVideoOutputs[connectionId].transform.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(1843.2f, 1036.8f);
                    uVideoOutputs[connectionId].transform.parent.gameObject.transform.SetParent(speakerPanel);
                }
                else {
                    uVideoOutputs[connectionId].transform.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(320f, 180f);
                    uVideoOutputs[connectionId].transform.parent.gameObject.transform.SetParent(leftPanel);
                }
            }
        }
    }
    #endregion

    #region Buttons
    public void OnTextChatOpenButtonPressed()
    {
        GetComponent<Transform>().localPosition = new Vector2(-380, 0);
        vidCTextChatObj.SetActive(true);
    }

    public void CloseTextChatTab()
    {
        vidCTextChatObj.SetActive(false);
        GetComponent<Transform>().localPosition = new Vector2(0, 0);
    }

    //I think I won't touch textpanel... keep user pref for this session before minimize.
    public void CloseVideoChatPanel()
    {
        gameObject.SetActive(false);
    }
    #endregion

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
    private void ChangeVideoToggle(bool value) {
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

    public void OnAudioSettingsChanged()
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
    #endregion

    /// <summary>
    /// Shows the setup screen or the chat + video
    /// </summary>
    /// <param name="showSetup">true Shows the setup. False hides it.</param> called after Factory ready
    public void SetGuiState(bool showSetup)
    {
        if (showSetup)
        {
            //fill the video dropbox
            UpdateVideoDropdown();
            InitFormatDropdown();
            if (uLoadSettings)
            {
                LoadSettings();
            }
            CheckSettings();
        }
        uSetupPanel.gameObject.SetActive(showSetup);

        if (showSetup) CloseTextChatTab();
        else OnTextChatOpenButtonPressed();
        //this is going to hide the textures until it is updated with a new frame update
        /*UpdateLocalTexture(null);
        UpdateRemoteTexture(null);*/
    }

    /// <summary>
    /// Join button pressed. Tries to join a room. NOT ANYMORE! This is like save settings more like.
    /// </summary>
    public void SaveSettingsButtonPressed()
    {
        //remember for next run
        if(CheckInputValidity())
        {
            mStoredVideoDevice = GetSelectedVideoDevice();
            SaveSettings();
            SaveMediaConfSettings();
        }
    }

    private bool CheckInputValidity()
    {
        int width = TryParseInt(uIdealWidth.text, 320);
        int height = TryParseInt(uIdealHeight.text, 240);
        if(width > 960 || height > 540) //local client can go 1920 x 1080 +, but webgl sucks so...
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
}
