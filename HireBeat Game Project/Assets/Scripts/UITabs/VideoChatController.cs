using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Byn.Awrtc;
using Byn.Awrtc.Unity;
using Byn.Unity.Examples;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System;

public class VideoChatController : MonoBehaviour
{
    [Header("Statistic Data Grab")]
    const int maxUsersInRoom = 6;
    //If connectionId is > maxUsersInRoom - 1 (because excluding yourself), then you are full!
    //The dictionary below might be useless, because such data is already stored in remote panel info script object... delte if no use
    List<string> userInRoomIds = new List<string>(); //this is to be populated, send connect request to each at end step. 
    private Dictionary<ConnectionId, string> connectionIdWithPlayFabId = new Dictionary<ConnectionId, string>(); //link connection id to playfab through msg.

    [Header("Video chat panel")]
    IMediaNetwork communicator; //no need for sender and receiver! the receiver in the example is for 1 to N, as said in email. One network is enough.

    private string myID;
    private string selfAddress;
    private string roomName;

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
    //private Texture2D mVideoTexture; //should I make an array of this too? Having 1 buffering everything... overworked
    private Texture2D localTexture;
    //private Texture2D remoteTexture;
    private Texture2D rTex0;
    private Texture2D rTex1;
    private Texture2D rTex2;
    private Texture2D rTex3;
    private Texture2D rTex4;
    private Dictionary<ConnectionId, int> textureIndex = new Dictionary<ConnectionId, int>();

    //invite people into the room
    public TMP_InputField inviteInput;

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

    //Here's how the process works: Join room clicked -> if ok -> initialize VC system -> settings panel -> set up communicator -> connection statements.

    MediaConfig mediaConf; //this mediaConf is set to be the same as VCRS's, assigned at this prefab's creation
    NetworkConfig netConf; //same here, like the mediaConf above.

    public VideoChatRoomSearch vcs;

    public Text errorMsg;
    IEnumerator errorMsgDisplay;

    // Start is called before the first frame update
    void Awake()
    {
        pd = GameObject.Find("PersistentData").GetComponent<PersistentData>();

        myID = pd.acctID;
        selfAddress = "HireBeatProjVidC" + myID; //no need for Application.productName

        SetCellSizeBasedOnNum(); //for testing too.

        PersistentData.usingMicrophone = true; //this obj is initalized successfully, so yeah.
    }

    public void SetupMediaAndNetConfAndOther(MediaConfig mC, NetworkConfig nC, VideoChatRoomSearch vCS, List<string> userIds, string rName)
    {
        mediaConf = mC;
        netConf = nC;
        vcs = vCS;
        userInRoomIds = userIds;
        roomName = rName;
    }

    public void StartRoomCreateOrJoinProcess()
    {
        UnityCallFactory.EnsureInit(OnCallFactoryReady, OnCallFactoryFailed);
    }

    void OnCallFactoryReady()
    {
        UnityCallFactory.Instance.RequestLogLevel(UnityCallFactory.LogLevel.Info);
        InitVidCSystem();
    }

    void OnCallFactoryFailed(string error)
    {
        string fullErrorMsg = typeof(CallApp).Name + " can't start. The " + typeof(UnityCallFactory).Name + " failed to initialize with following error: " + error;
        Debug.LogError(fullErrorMsg);
    }

    void InitVidCSystem()
    {
        communicator = UnityCallFactory.Instance.CreateMediaNetwork(netConf);
        //Then the settings tab pop out here. //nope not anymore.
        SetupCommunicator(); 
    }

    private void SetupCommunicator()
    {
        Debug.Log("communicator setup");

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
            Debug.LogError("Starting self address: " + selfAddress);

            //then connect to each target in room.
            foreach (string id in userInRoomIds) //if you create a room, then userInRoomIds would be empty, so nothing!
            {
                ConnectToVidCAddress(id); 
            }
        }
    }

    //This is for testing purposes: direct address connecting. will delete?
    public void ConnectToVidCAddress(string targetUserID)
    {
        communicator.Connect("HireBeatProjVidC" + targetUserID);
        Debug.LogError("VidCAddressSubmitted to " + "HireBeatProjVidC" + targetUserID);
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
                Debug.LogError("New connection received!");
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
                textureIndex.Add(evt.ConnectionId, FindFirstUnoccupiedTexture());

                SetCellSizeBasedOnNum(); //this wouldn't hurt no matter what view u in.

                Log("New connection id " + evt.ConnectionId); 
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
                    textureIndex.Remove(evt.ConnectionId);
                    Destroy(uVideoOutputs[evt.ConnectionId].gameObject.transform.parent.gameObject); //I see!
                    uVideoOutputs.Remove(evt.ConnectionId);
                    userInRoomIds.Remove(connectionIdWithPlayFabId[evt.ConnectionId]);
                    leavers.Add(connectionIdWithPlayFabId[evt.ConnectionId]); //add to leavers
                    connectionIdWithPlayFabId.Remove(evt.ConnectionId);

                    if (!inProportionalView && currentSpeaker == evt.ConnectionId) //if in speaker && the person leaving is speaker
                    {
                        ActivateSpeakerView(true, ConnectionId.INVALID); //doesn't matter id, set yourself to speaker, a refresh
                    }
                    SetCellSizeBasedOnNum(); //this wouldn't hurt no matter what view u in.

                    Log("Connection disconnected");
                    /*if (uSender == false)
                        this.GetComponent<Image>().color = new Color(0, 0.5f, 0, 1); //receiver lost connection.  */

                    vcs.RetrieveVCRoomCurrentOwnerWait(roomName, "leaverCheck", 1f);
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
            case NetEventType.ServerClosed: //I never closed the server... hmm
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

    List<string> leavers = new List<string>();
    public void OnLeaverCheckCallback(SQL4Unity.SQLResult result)
    {
        string roomOwnerID = null;
        try
        {
            roomOwnerID = result.Get<hirebeatprojectdb_videochatsavailable>()[0].CurrOwnerID;
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
            return;
        }

        if(leavers.Contains(roomOwnerID) || roomOwnerID == "-null-") //then compete! One user will succeed.
        {
            vcs.UpdateVCRoomOwner(roomName, myID);
            vcs.UpdateVCRoomNumMembers(roomName, userInRoomIds.Count + 1); //including you so +1
        }
        //else roomOwner is not a leaver or it is someone else that's not null. Then no need to do anything!

        leavers.Clear();
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
            if (!userInRoomIds.Contains(playfabID)) userInRoomIds.Add(playfabID);
        }
        else if(username.Contains('=')) //this is the special symbol for a leaver request, so owner can edit the data immediately. Only will receive this if you are owner!
        {
            string playfabID = content;
            userInRoomIds.Remove(playfabID); //immediately removes this, so other users can sense if req. Rest will be removed at disconnect received.
            vcs.UpdateVCRoomNumMembers(roomName, userInRoomIds.Count + 1); //including yourself! But did -1 from prev already.
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
                switch(textureIndex[frameId])
                {
                    case 0:
                        UpdateTexture(ref rTex0, frame); 
                        uVideoOutputs[frameId].texture = rTex0;
                        break;
                    case 1:
                        UpdateTexture(ref rTex1, frame);
                        uVideoOutputs[frameId].texture = rTex1;
                        break;
                    case 2:
                        UpdateTexture(ref rTex2, frame);
                        uVideoOutputs[frameId].texture = rTex2;
                        break;
                    case 3:
                        UpdateTexture(ref rTex3, frame);
                        uVideoOutputs[frameId].texture = rTex3;
                        break;
                    case 4:
                        UpdateTexture(ref rTex4, frame);
                        uVideoOutputs[frameId].texture = rTex4;
                        break;
                    default:
                        Debug.LogError("No index detected"); //this is not possible;
                        break;
                }
                //The data for texture objects don’t “exist” in a shader, they’re always references?
                /*UpdateTexture(remoteTexture, frame); //current this texture is being used n times per frame; We'll see if it's overloaded or not.
                uVideoOutputs[frameId].texture = remoteTexture;*/
            }
        }
        else if (frameId == ConnectionId.INVALID && frame != null) // I SEE
        {
            UpdateTexture(ref localTexture, frame); 
            localDisplayPanel.texture = localTexture;
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

    private int FindFirstUnoccupiedTexture()
    {
        var allUsed = textureIndex.Values.ToList(); //or can use containsValue in future.
        for(int i = 0; i <= 4; i++)
        {
            if (!allUsed.Contains(i)) return i;
        }
        return -1; //this is not supposed to happen: only happens if full!
    }
    #endregion

    private void OnDestroy()
    {
        //OnDisconnectPressed(); // I don't think this will be triggered if the user closes the browser, still need server-side check.
    }

    public void OnDisconnectPressed()
    {
        vcs.RetrieveVCRoomCurrentOwner(roomName, "disconnect"); //this creates a callback
    }
    public void OnDisconnectPressedSecondHalf(SQL4Unity.SQLResult result) //this will be called by dbc.
    {
        string roomOwnerID = null;
        try
        {
            roomOwnerID = result.Get<hirebeatprojectdb_videochatsavailable>()[0].CurrOwnerID;
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
            return;
        }


        if (roomOwnerID == myID) //If you are owner, check the list of users in room., 
        {
            if(userInRoomIds.Count == 0) //no more connections or incoming at this point! You are the only user in room, so delete room!
            {
                vcs.DeleteVCRoom(roomName);
            }
            else
            {
                vcs.UpdateVCRoomNumMembers(roomName, userInRoomIds.Count); //excluding yourself, so no +1, therefore everyone but you (-1).
                vcs.UpdateVCRoomOwner(roomName, userInRoomIds[0]); //assign new owner through database, which can easily be the first person
            }
        }
        else if(roomOwnerID != "-null-") //If you are not owner, then webrtc msg owner to - 1. //also ignore if null... eventual update hopefully
        {
            //'=' is indicator for leaver msg, "," is splitter, and 2nd part is content.
            byte[] msgData = Encoding.UTF8.GetBytes("=," + myID); //This is to tell the owner you are leaving.
            foreach(var pair in connectionIdWithPlayFabId)
            {
                if(pair.Value == roomOwnerID) //finding connection id through playfab id
                {
                    communicator.SendData(pair.Key, msgData, 0, msgData.Length, true);
                    break;
                }
            }
        }

        if (communicator != null)
        {
            communicator.Dispose();
            communicator = null;

            //userInRoomIds.Clear();
            //connectionIdWithPlayFabId.Clear();
            //mConnectionIds = new List<ConnectionId>();
            //other lists... but no need to remove! Since this obj will be destroyed, so auto gone anyway.

            //keep netconf: intialized once and forever at start.
            //keep mediaconf: just settings in general, save!

            //Remove the remote panels, etc //this should be covered.
        }

        PersistentData.usingMicrophone = false;

        Destroy(gameObject); //then destroy this prefab!
        vcs.gameObject.SetActive(true); //open chat room search panel after leaving
    }

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
        if(vidCTextChatObj.activeSelf)
        {
            CloseTextChatTab();
        }
        else
        {
            GetComponent<Transform>().localPosition = new Vector2(-380, 0);
            vidCTextChatObj.SetActive(true);
        }
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
        errorMsg.gameObject.SetActive(false);
    }

    public GameObject inviteUserSearchDisplayPrefab; //prefab
    public RectTransform displayUserSearchResultsPanel; //parent.
    string input;
    public void OnSearchUserToInviteSubmit() //enter key
    {
        input = inviteInput.text;
        string query = "SELECT UserName, UserId FROM UserDataStorage WHERE UserName = %input% OR UserId = %input% OR Email = %input%";
        SQL4Unity.SQLParameter parameters = new SQL4Unity.SQLParameter();
        parameters.SetValue("input", input);
        DataBaseCommunicator.Execute(query, OnSearchUserToInviteSubmitCallback, parameters);
    }
    Dictionary<string, string> userIdToUserNameCacheForPlayerSearch;
    void OnSearchUserToInviteSubmitCallback(SQL4Unity.SQLResult result)
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
                userIdToUserNameCacheForPlayerSearch = userData.ToDictionary(r => r.UserId, r => r.UserName);
                //dbc.GrabAllUserStatusFromGivenIds(userIdToUserNameCacheForPlayerSearch.Keys.Distinct().ToArray());
                string[] userIdx = userIdToUserNameCacheForPlayerSearch.Keys.Distinct().ToArray();
                string query;
                SQL4Unity.SQLParameter parameters = new SQL4Unity.SQLParameter();
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
            }
        }
        else
        {
            Debug.LogError("Error occured in search user to invite vidC submit");
        }
    }
    void DisplayUserStatusResults(SQL4Unity.SQLResult result)
    {
        if (result != null)
        {
            List<string> idsOnline = result.Get<hirebeatprojectdb_ipadresstouniqueid>().Select(r => r.UniqueID).ToList();

            RemoveAllUserSearchResults(); //remove all previous results! 1-2 at max.

            foreach (var user in userIdToUserNameCacheForPlayerSearch)
            {
                bool isOnline = idsOnline.Contains(user.Key);
                string userName = user.Value;
                string userId = user.Key;
                bool isYou = userId == myID;
                bool isAlreadyInRoom = userInRoomIds.Contains(userId);
                var newPlayerSearchDisplay = Instantiate(inviteUserSearchDisplayPrefab, displayUserSearchResultsPanel); //using same panel as parent
                newPlayerSearchDisplay.GetComponent<IPTR_Simple>().SetUserInfo(userName, userId, isOnline, null, this, isAlreadyInRoom, isYou);
            }
        }
        else
        {
            Debug.LogError("Error in trying to grab user status search secondary info in vidC!");
        }
    }
    public void RemoveAllUserSearchResults() //can be used for the clear button as well
    {
        foreach (Transform child in displayUserSearchResultsPanel.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void OnClearSearchButtonPressed()
    {
        inviteInput.text = "";
        RemoveAllUserSearchResults();
    }

    public void OnInviteUserButtonPressed(string userId)
    {
       // string userInvited = inviteInput.text; //input will only be alphanumeric (username or playfabid, alphabet + num), so no space!

        //currently, only id. A future database that links id with name will be used
        //also can use a database to check if the person is online.
        vcs.socialSystem.SendVidCInvite(userId, roomName);
        Debug.Log("Room invite sent to user " + userId);

        if (errorMsgDisplay != null) StopCoroutine(errorMsgDisplay); //"restart" coroutine
        errorMsgDisplay = DisplayErrorMessage(3f, "Room invite has been sent to user " + userId); //each time a coro is called, a new obj is formed.
        StartCoroutine(errorMsgDisplay);

        inviteInput.text = "";
        RemoveAllUserSearchResults();
    }
    #endregion

    public IEnumerator DisplayErrorMessage(float time, string message)
    {
        errorMsg.gameObject.SetActive(true);
        errorMsg.text = message;
        yield return new WaitForSeconds(time);
        errorMsg.gameObject.SetActive(false);
    }

    //This is only called when a new user is connecting
    public string GetUserInVidCRoomIDsInString(string senderID)
    {
        string result = myID;
        foreach(string id in userInRoomIds)
        {
            result += "," + id; //format: myID,id1,id2,...,id5 
        }

        //only owner adds this early, so the rest are still added through connection message.
        userInRoomIds.Add(senderID);

        //also owner updates room count by 1
        vcs.UpdateVCRoomNumMembers(roomName, userInRoomIds.Count + 1); //including yourself, the +1

        return result;
    }

}
