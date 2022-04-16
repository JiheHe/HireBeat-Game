using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Byn.Awrtc;
using Byn.Awrtc.Unity;
using Byn.Unity.Examples;
using System.Text;

//This is a prefab instantiated when a call is ready.
public class WebRTCVCCallObj : MonoBehaviour
{
    public WebRTCVoiceChat wrtcvc; //this is assigned by WebRTCVC at creation

    private NetworkConfig netConf;

    IMediaNetwork communicator; //local

    string addressPrefix = "HireBeatProjVC";

    private string myID;

    private Dictionary<ConnectionId, string> connectionIdWithPlayFabId = new Dictionary<ConnectionId, string>(); //link connection id to playfab through msg.

    //this is initialized by WebRTCVC at creation
    public void StartMyOwnQuickRTCVoiceConnection(string userID) //This uses myID
    {
        //STEP1: instance setup
        netConf = new NetworkConfig();
        netConf.SignalingUrl = ExampleGlobals.Signaling; //will change the urls and servers later, post testing
                                                         //watch out the signaling server needs to be configured properly for this to work:
                                                         //flag "address_sharing" needs to be set to true in config.json
                                                         //e.g. "ws://signaling.because-why-not.com/testshared"
                                                         //netConf.SignalingUrl = ExampleGlobals.SharedSignaling;

        netConf.IceServers.Add(new IceServer(ExampleGlobals.StunUrl));
        //The current version doesn't deal well with failed direct connections
        //thus a turn server is used to ensure users can connect.
        //
        netConf.IceServers.Add(new IceServer(ExampleGlobals.TurnUrl,
            ExampleGlobals.TurnUser,
            ExampleGlobals.TurnPass));

        myID = userID;

        communicator = UnityCallFactory.Instance.CreateMediaNetwork(netConf);
        MediaConfig mediaConf = new MediaConfig();
        mediaConf.Video = false;
        mediaConf.Audio = true; //just audio!
        communicator.Configure(mediaConf);

        string selfAddress = addressPrefix + userID;
        communicator.StartServer(selfAddress); //Starting a self-server with my own address!
        Debug.Log("Starting self address: " + selfAddress);

        //then connect to each target in room once the server is ready.
        StartCoroutine(ConnectToAllUsersInRoom());
    }

    bool serverInitializationReady = false;
    IEnumerator ConnectToAllUsersInRoom()
    {
        yield return new WaitUntil(() => serverInitializationReady);

        foreach (string id in wrtcvc.idsOfConnectedUsers) 
        {
            communicator.Connect(addressPrefix + id);
            Debug.Log("VCAddressSubmitted to " + addressPrefix + id);
        }
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

        //Flush will resync changes done in unity to the native implementation
        //(and possibly drop events that aren't handled in the future)
        if (communicator != null)
            communicator.Flush();
    }

    protected virtual void HandleNetworkEvent(NetworkEvent evt)
    {
        switch (evt.Type)
        {
            case NetEventType.NewConnection:
                Debug.Log("New connection received!");
                byte[] msgData = Encoding.UTF8.GetBytes("-," + myID); //This is to tell the person you just connected to who are you.
                communicator.SendData(evt.ConnectionId, msgData, 0, msgData.Length, true);
                break;
            case NetEventType.ConnectionFailed:
                //call failed
                Debug.LogError("Outgoing connection failed");
                break;
            case NetEventType.Disconnected:
                Debug.Log("Someone disconnected");
                connectionIdWithPlayFabId.Remove(evt.ConnectionId);

                //If by the time this event is called, the occupation of that chair has not been set to false and that chair 
                //is still occupied by the same user who left, then something must've went wrong! i.e. that user straight up
                //closed the browser. So we'll do it for him. 
                string userLeftId = connectionIdWithPlayFabId[evt.ConnectionId];
                int chairId = wrtcvc.FindChairIdFromUserId(userLeftId);
                if (wrtcvc.chairsCurrentSitter[chairId] == userLeftId && wrtcvc.chairsOccupationList[chairId])
                {
                    wrtcvc.AnnounceChairOccupation(chairId, false, null);
                }
                break;
            case NetEventType.ServerInitialized:
                //incoming calls possible
                Debug.Log("Server ready for incoming connections. Address: " + evt.Info);
                serverInitializationReady = true;
                break;
            case NetEventType.ServerInitFailed:
                Debug.LogError("Server init failed");
                break;
            case NetEventType.ServerClosed: //I never closed the server... hmm
                Debug.Log("Server stopped");
                break;
            case NetEventType.ReliableMessageReceived: 
            case NetEventType.UnreliableMessageReceived:
                {
                    HandleIncommingMessage(ref evt);
                }
                break;
        }
    }

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

        if (username.Contains('-')) //this is not possible for normal msg, so this indicates playfab id... might not even need photon chat for this LOL
        {
            string playfabID = content;
            connectionIdWithPlayFabId.Add(evt.ConnectionId, playfabID);
            wrtcvc.chairs[wrtcvc.FindChairIdFromUserId(playfabID)].SetCurrentChairOwner(evt.ConnectionId);
        }
        else
        {
            //Then we update username and string (make into a printable obj) here.
            //textChatController.AddTextEntry(username, content, false);
            Debug.LogError("This should not happen! I haven't implemented private group chats yet LOL");
        }

        //return the buffer so the network can reuse it
        buffer.Dispose();
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

        SLog.LW("SetVolume not supported", "WebRTCVC WebRTC Call");
    }

    private void OnDestroy()
    {
        Debug.Log("Current communicator is destroyed");
        wrtcvc.isInVCCall = false;
        if (communicator != null)
        {
            communicator.Dispose();
            communicator = null;
        }
    }
}
