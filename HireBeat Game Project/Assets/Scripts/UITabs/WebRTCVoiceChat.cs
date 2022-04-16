using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Byn.Awrtc;
using Byn.Awrtc.Unity;
using Byn.Unity.Examples;


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
            chairsOccupationList.Add(chair.chairId, false); //then RPC call overwrites occupation boolean hopefully.
            chairsCurrentSitter.Add(chair.chairId, null); 
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        view = GetComponent<PhotonView>();

        myID = GameObject.Find("PersistentData").GetComponent<PersistentData>().acctID;

        canvas.GetComponent<Canvas>().worldCamera = GameObject.FindGameObjectWithTag("PlayerCamera").GetComponent<cameraController>().zoomCamera;
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
            chairsOccupationList[chairId] = state;
            chairsCurrentSitter[chairId] = userId;

            if(userId != myID)
            {
                if(state) //true = someone's joining, false = someone's leaving.
                {
                    idsOfConnectedUsers.Add(userId);
                }
                else
                {
                    idsOfConnectedUsers.Remove(userId);
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

        Debug.LogError("Error in finding correct chair from user id");
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
}
