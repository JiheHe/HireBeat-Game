using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Byn.Awrtc;
using Byn.Awrtc.Unity;
using Byn.Unity.Examples;
using UnityEngine.UI;

public class VideoChatController : MonoBehaviour
{

    const int maxUsersInRoom = 6;
    int currUsersInRoom; //can use database for this now
    List<string> userInRoomIDs;

    IMediaNetwork communicator; //no need for sender and receiver! the receiver in the example is for 1 to N, as said in email. One network is enough.
    private bool mCommunicatorConfigured = false;

    private NetworkConfig netConf;
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
    public RawImage remoteDisplayPanel; //this is a prefab
    public RawImage localDisplayPanel; //this is a legit gameobject

    public RectTransform parentPanel;

    /// <summary>
    /// Helper to keep to keep track of each instance
    /// </summary>
    private static int sInstances = 0;

    /// <summary>
    /// Helper to give each instance an id to print via log output
    /// </summary>
    private int mIndex = 0;

    /// <summary>
    /// If true this will create peers that send out video. False will
    /// not send anything.
    /// </summary>
    public bool uSender = false;

    /// <summary>
    /// Will be used to show the texture received (or sent)
    /// </summary>
    //public RawImage uVideoOutput; //this is single! no

    /// <summary>
    /// Texture2D used as buffer for local or remote video
    /// </summary>
    private Texture2D mVideoTexture;

    // Start is called before the first frame update
    void Start()
    {
        currUsersInRoom = 1; //start with 1 because yourself is always included!
        userInRoomIDs = new List<string>(); //excluding yourself.
        selfAddress = "HireBeatProjVidC" + GameObject.Find("PlayFabController").GetComponent<PlayFabController>().myID; //no need for Application.productName

        OnCreatePressed(); //for testing.
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

    public void SendNoticeToCurrRoomUsers() //this method is ONLY CALLED when photon chat receives the message from owner and has finished setting up the array list
    {
        if (userInRoomIDs == null) //set it to null if the room is full... this is the special message.
        {
            Debug.LogError("My custom message: Room is full! 6/6 users."); //Add some sort of display in the future to tell the user.
            return;
        } //else not full! Actual list:

        //Set up your own personal server first
        UnityCallFactory.EnsureInit(OnCallFactoryReady, OnCallFactoryFailed);

        //the amount of users is you + others now
        currUsersInRoom += userInRoomIDs.Count;

        //Don't forget to include owner's ID in the result too! (from sender's end).
        foreach (string userID in userInRoomIDs)
        {
            //Send the photon chat message to that ID, a new user has joined!.

            //Also initializes a new display raw image for it //this should be covered from getting a new connection.
        }
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
        //STEP1: instance setup

        netConf = new NetworkConfig();
        netConf.SignalingUrl = ExampleGlobals.Signaling; //will change the urls and servers later, post testing
        netConf.IceServers.Add(new IceServer(ExampleGlobals.StunUrl));
        SetupCommunicator();
    }

    //No need to set up false false receiver... receiver is an example of network application.
    //This is for testing purposes: direct address connecting
    public void ConnectToVidCAddress(string targetUserID)
    {
        communicator.Connect("HireBeatProjVidC" + targetUserID);
        Debug.LogError("VidCAddressSubmitted");
    }


    private void SetupCommunicator()
    {
        Debug.Log("communicator setup");
        communicator = UnityCallFactory.Instance.CreateMediaNetwork(netConf);
        mediaConf = CreateMediaConfig();

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
        communicator.StartServer(selfAddress); //Starting a self-server with my own address!
        //Debug.LogError("Starting self address!");
        //mUi.SetGuiState(false);
    }


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
        if (communicator.GetConfigurationState() == MediaConfigurationState.Failed)
        {
            //did configuration fail? error
            Debug.Log("communicator configuration failed " + communicator.GetConfigurationError());
            communicator.ResetConfiguration();
        }
        /*else if (communicator.GetConfigurationState() == MediaConfigurationState.Successful
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

                mConnectionIds.Add(evt.ConnectionId);
                uVideoOutputs.Add(evt.ConnectionId, Instantiate(remoteDisplayPanel, Vector2.zero, Quaternion.identity, parentPanel)); //I see!

                Debug.LogError("New connection id " + evt.ConnectionId);
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
                    Destroy(uVideoOutputs[evt.ConnectionId].gameObject); //I see!
                    uVideoOutputs.Remove(evt.ConnectionId);

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
        }
    }

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

            currUsersInRoom = 1;
            userInRoomIDs.Clear();

            //Remove the remote panels, etc //this should be covered.
        }

        foreach (string userID in userInRoomIDs)
        {
            //Send the photon chat message to that ID: this user has left!
        }
    }


    ///////////////////////////////////////////////////////////////////
    /// <summary>
    /// Create the default configuration for this CallApp instance.
    /// This can be overwritten in a subclass allowing the creation custom apps that
    /// use a slightly different configuration.
    /// </summary>
    /// <returns></returns>
    public virtual MediaConfig CreateMediaConfig()
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
    }
}
