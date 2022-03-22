using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Byn.Awrtc;
using Byn.Awrtc.Unity;
using Byn.Unity.Examples;

public class VideoChatController : MonoBehaviour
{

    const int maxUsersInRoom = 6;
    int currUsersInRoom;
    List<string> userInRoomIDs;

    IMediaNetwork communicator; //no need for sender and receiver! the receiver in the example is for 1 to N, as said in email. One network is enough.
    private bool mCommunicatorConfigured = false;

    private NetworkConfig netConf;
    private string selfAddress;

    //public GameObject 

    // Start is called before the first frame update
    void Start()
    {
        currUsersInRoom = 1; //start with 1 because yourself is always included!
        userInRoomIDs = new List<string>(); //excluding yourself.
        selfAddress = "HireBeatProjVidC" + GameObject.Find("PlayFabController").GetComponent<PlayFabController>().myID; //no need for Application.productName
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

            //Also initializes a new display raw image for it
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

    private void SetupCommunicator()
    {
        Debug.Log("communicator setup");
        communicator = UnityCallFactory.Instance.CreateMediaNetwork(netConf);
        MediaConfig mediaConf = CreateMediaConfig();

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
        //mUi.SetGuiState(false);
    }

    private void UpdateCommunicator()
    {
        //STEP5: Sender update loop. IMediaNetwork uses polling instead of events
        communicator.Update();

        if (communicator.GetConfigurationState() == MediaConfigurationState.Failed)
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
        }

        NetworkEvent evt;

        while (communicator.Dequeue(out evt))
        {
            if (evt.Type == NetEventType.ServerInitialized)
            {
                //triggered if StartServer completed
                Debug.Log("communicator: server initialized.");
            }
            else if (evt.Type == NetEventType.ServerInitFailed)
            {
                //either network problem or address in use
                Debug.LogError("communicator: server init failed");
            }
            else if (evt.Type == NetEventType.NewConnection)
            {
                Debug.Log("communicator: New connection with id " + evt.ConnectionId);
            }
            else if (evt.Type == NetEventType.ConnectionFailed)
            {
                Debug.LogError("communicator: connection failed");
            }
        }
        communicator.Flush();
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

            //Remove the remote panels, etc
        }

        foreach (string userID in userInRoomIDs)
        {
            //Send the photon chat message to that ID: this user has left!
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (communicator != null)
            UpdateCommunicator();
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
