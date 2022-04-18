using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Byn.Awrtc;
using Byn.Awrtc.Unity;
using Byn.Unity.Examples;
using System.Linq;


//Use PhotonNetwork.Instantiate to sync this game object!
// If we use Photon.Instantiate, then the new player can receive the newest updated list without us having to
// call RpcTarget.AllBuffered (so the new player who just joined won't get overwhelmed by many previous RPC commands).
// We just need to call RpcTarget.All to update the current players in room, and that would be sufficient
// That 's why Instantiate is better, else a local copy would just starts off with the default list value
// Can use isMine to delete the rest.
public class WebRTCVoiceChat : MonoBehaviour
{
    public List<string> idsOfConnectedUsers = new List<string>(); //this should EXCLUDE yourself.
    PhotonView view;

    public GameObject webRTCVCCallObjPrefab;
    public WebRTCVCCallObj currentLocalWebRTCVCCallObj;
    //Use this bool to check whether to unleash player control access and before initializing video chat connection. A local variable
    public bool isInVCCall = false; //can also use currentLocaletc != null, but bool should be faster

    public int identifyingId; //set this with PhotonNetwork.Instantiate upon creation, diff for each set.

    //You only need to know who's on what chair locally for volume purposes. Outsiders don't need to know.

    //This table script is the central terminal control! Chair operation call through this -> currentLocalObj -> setvolume, mute, etc.
    //Or from CallObj to chairs!

    //Basically each chair object has an "isOccupied" boolean. If someone sits on that chair then use this Dict to set it occupied.
    public Dictionary<int, bool> chairsOccupationList = new Dictionary<int, bool>(); //4 chairs per table rn.
    public Dictionary<int, string> chairsCurrentSitter = new Dictionary<int, string>(); //chair id to user id.
    public WebRTCVCChair[] chairs = new WebRTCVCChair[4]; //id should be in order: 0, 1, 2, 3, same as index.

    public string myID;

    public Canvas canvas;

    private void Awake()
    {
        foreach(var chair in chairs)
        {
            chair.SetTerminal(this);
            chairsOccupationList.Add(chair.chairId, false); //then room properties overwrites occupation boolean hopefully.
            chairsCurrentSitter.Add(chair.chairId, null); 
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        view = GetComponent<PhotonView>();

        myID = GameObject.Find("PersistentData").GetComponent<PersistentData>().acctID;

        canvas.GetComponent<Canvas>().worldCamera = GameObject.FindGameObjectWithTag("PlayerCamera").GetComponent<cameraController>().zoomCamera;

        //UpdateCurrentTableCustomProperties(); //This line causes the disconnection error...
        //presumably should be called on RoomPropertiesChanged...

        //Since this method is only called ONCE upon first joining...
        //If you are the master client, then you are likely a room creator, so no need to do so.
        //If you are not, then you are joining people, and you will receive master client's callback. So can safely start it here.
        //If you are lurkers in the room, then this won't even be called, so no worries.
        if(!PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(ReadyToReceiveRoomProperties());
        }
    }

    public bool roomPropertiesReady = false; //it's like a one-time thing
    IEnumerator ReadyToReceiveRoomProperties()
    {
        yield return new WaitUntil(() => roomPropertiesReady);

        Debug.LogError("Room properties for this table received and updated");
        UpdateCurrentTableCustomProperties();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //This is called by a chair upon user entering
    public void InitializeWebRTCCall()
    {
        isInVCCall = true;
        currentLocalWebRTCVCCallObj = Instantiate(webRTCVCCallObjPrefab, this.gameObject.GetComponent<Transform>()).GetComponent<WebRTCVCCallObj>();
        currentLocalWebRTCVCCallObj.wrtcvc = this;
        currentLocalWebRTCVCCallObj.StartMyOwnQuickRTCVoiceConnection(myID);
    }

    /*public void UserJoinsPrivateRoomVC(string userID) //joiner calls this
    {
        view.RPC("UserJoinsPrivateRoomVCRPC", RpcTarget.All, userID, identifyingId);
    }

    [PunRPC] //multiple sets can receive this call. Need an identifying id.
    public void UserJoinsPrivateRoomVCRPC(string userID, int identifyingId)
    {
        if(this.identifyingId == identifyingId && userID != myID) idsOfConnectedUsers.Add(userID);
    }

    public void UserLeavesPrivateRoomVC(string userID) //leaver calls this
    {
        view.RPC("UserLeavesPrivateRoomVCRPC", RpcTarget.All, userID, identifyingId);
    }

    [PunRPC]
    public void UserLeavesPrivateRoomVCRPC(string userID, int identifyingId)
    {
        if(this.identifyingId == identifyingId && userID != myID) idsOfConnectedUsers.Remove(userID);
    }*/

    public void AnnounceChairOccupation(int chairId, bool state, string userId)
    {
        view.RPC("AnnounceChairOccupationRPC", RpcTarget.All, chairId, state, userId, identifyingId);
    }

    [PunRPC]
    public void AnnounceChairOccupationRPC(int chairId, bool state, string userId, int identifyingId)
    {
        if (this.identifyingId == identifyingId)
        {
            //Debug.LogError("At table id" + identifyingId + ", user " + userId + " at chair " + chairId + " has joined: " + state);
            chairsOccupationList[chairId] = state;

            if(userId != myID)
            {
                if(state) //true = someone's joining, false = someone's leaving.
                {
                    idsOfConnectedUsers.Add(userId);
                    chairsCurrentSitter[chairId] = userId;
                }
                else
                {
                    idsOfConnectedUsers.Remove(userId);
                    chairsCurrentSitter[chairId] = null;
                }
            }

            CheckStateAndUpdateInterface();
        }
    }

    public int FindChairIdFromUserId(string targetId)
    {
        foreach(var kvp in chairsCurrentSitter)
        {
            if (kvp.Value == targetId) return kvp.Key;
        }

        //Debug.LogError("Error in finding correct chair from user id");
        return -1;
    }

    private void CheckStateAndUpdateInterface()
    {
        foreach(var kvp in chairsOccupationList)
        {
            if(!kvp.Value) //if it's not occupied
            {
                chairs[kvp.Key].HideInterface();
            }
        }
    }

    // Hides all interface on user leaving
    public void OnLocalDisconnect()
    {
        foreach(var chair in chairs)
        {
            chair.HideInterface();
        }
    }

    //If you are master client then call this.
    //On new player join, the master client should update all room properties, and the newplayer will update their 
    //lists accordingly on the callback.
    public void UploadCurrentTableCustomProperties(ExitGames.Client.Photon.Hashtable tableCustomProperties)
    {
        tableCustomProperties["PVCT" + identifyingId + "COL"] = chairsOccupationList;
        tableCustomProperties["PVCT" + identifyingId + "CCS"] = chairsCurrentSitter;
    }

    //This is only called once at object instantiation at beginning. Rest of the time it's dealt with through rpc all
    private void UpdateCurrentTableCustomProperties()
    {
        var currProp = PhotonNetwork.CurrentRoom.CustomProperties;
        if (!currProp.ContainsKey("PVCT" + identifyingId + "COL") || //if you created the room then duhhh
            !currProp.ContainsKey("PVCT" + identifyingId + "CCS")) return;

        chairsOccupationList = (Dictionary<int, bool>) PhotonNetwork.CurrentRoom.CustomProperties["PVCT" + identifyingId + "COL"];
        chairsCurrentSitter = (Dictionary<int, string>) PhotonNetwork.CurrentRoom.CustomProperties["PVCT" + identifyingId + "CCS"];
        idsOfConnectedUsers = chairsCurrentSitter.Values.ToList();
        idsOfConnectedUsers.RemoveAll(item => item == null);
    }
}
