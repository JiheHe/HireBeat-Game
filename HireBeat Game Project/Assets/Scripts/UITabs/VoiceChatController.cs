using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FrostweepGames.VoicePro.Examples;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class VoiceChatController : MonoBehaviour
{
    public InGameUIController gameUIController = null; //assigned by that object at first start call.

    public GameObject connectButton;
    public GameObject disconnectButton;

    public SocialSystemScript socialSystem;
    public VoiceChatSystem voiceSystem;
    public string myID;

    public GameObject disabledText;
    public GameObject disabledText1;
    public GameObject notAvalSymb;

    public Text myNameLocalDisplay;
    public RoomDataCentralizer dataCenter;

    // Start is called before the first frame update
    void Start() 
    {
        socialSystem = gameObject.transform.parent.Find("SocialSystem").GetComponent<SocialSystemScript>();
        myID = GameObject.Find("PlayFabController").GetComponent<PlayFabController>().myID;
        dataCenter = GameObject.FindGameObjectWithTag("DataCenter").GetComponent<RoomDataCentralizer>();

        myNameLocalDisplay.text = GameObject.Find("PersistentData").GetComponent<PersistentData>().acctName + " (You)";

        //VCC starts in active, so they won't work unless opened... so we'll initailize them in changeReceiver instead.
        /*
        voiceSystem = GetComponent<VoiceChatSystem>();

        voiceSystem.muteRemoteClientsToggle.isOn = true;
        voiceSystem.MuteRemoteClientsToggleValueChanged(true);
        voiceSystem.muteMyClientToggle.isOn = false; //might not be enough... need to disable mic at start...

        voiceSystem.debugEchoToggle.gameObject.SetActive(false);
        disabledText1.SetActive(true);
        voiceSystem.muteRemoteClientsToggle.gameObject.SetActive(false);
        disabledText.SetActive(true);
        voiceSystem.muteMyClientToggle.gameObject.SetActive(false);
        notAvalSymb.SetActive(true);*/
    }

    public void InitializationSteps()
    {
        voiceSystem = GetComponent<VoiceChatSystem>();
        voiceSystem.InitializationSteps();

        //voiceSystem.muteRemoteClientsToggle.isOn = true;
        voiceSystem.MuteRemoteClientsToggleValueChanged(true);
        //voiceSystem.muteMyClientToggle.isOn = false; //might not be enough... need to disable mic at start...
        voiceSystem.MuteMyClientToggleValueChanged(false); //need to do this here, else handlers haven't been assigned yet.

        voiceSystem.debugEchoToggle.gameObject.SetActive(false);
        disabledText1.SetActive(true);
        voiceSystem.muteRemoteClientsToggle.gameObject.SetActive(false);
        disabledText.SetActive(true);
        voiceSystem.muteMyClientToggle.gameObject.SetActive(false);
        notAvalSymb.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void OnTabClose()
    {
        gameObject.SetActive(false);
        gameUIController.hasOneOn = false;
    }

    public void OnConnectPressed()
    {
        if(PersistentData.usingMicrophone)
        {
            Debug.Log("Please leave the current voice system first.");
            gameObject.GetComponentInParent<changeReceiver>().ShowCanvasMessage(2, "Please leave the current voice system first.");
            return;
        }
        else
        {
            PersistentData.usingMicrophone = true;
        }

        AnnounceMeJoining();
        dataCenter.UserJoinsRoomVC(myID);

        connectButton.SetActive(false);
        disconnectButton.SetActive(true);

        voiceSystem.debugEchoToggle.gameObject.SetActive(true);
        disabledText1.SetActive(false);
        voiceSystem.muteRemoteClientsToggle.gameObject.SetActive(true);
        disabledText.SetActive(false);
        voiceSystem.muteMyClientToggle.gameObject.SetActive(true);
        notAvalSymb.SetActive(false);

        voiceSystem.muteRemoteClientsToggle.isOn = false; //this doesn't auto change, need to set
        voiceSystem.MuteRemoteClientsToggleValueChanged(false);
        voiceSystem.muteMyClientToggle.isOn = true; //join with unmute first, so your tab shows up //no need for this anymoroe.

        //no need to add object, because when unmute, automatically add tab back to all!
    }

    //send
    public void OnDisconnectPressed()
    {
        PersistentData.usingMicrophone = false;

        dataCenter.UserLeavesRoomVC(myID);

        connectButton.SetActive(true);
        disconnectButton.SetActive(false);

        voiceSystem.muteRemoteClientsToggle.isOn = true; //this doesn't auto change, need to set
        voiceSystem.MuteRemoteClientsToggleValueChanged(true);
        voiceSystem.debugEchoToggle.isOn = false; //turn off self tab so it gets decomposed at the end
        voiceSystem.muteMyClientToggle.isOn = false;

        //voiceSystem.muteRemoteClientsToggle.interactable = false; //use this for another button
        voiceSystem.debugEchoToggle.gameObject.SetActive(false);
        disabledText1.SetActive(true);
        voiceSystem.muteRemoteClientsToggle.gameObject.SetActive(false);
        disabledText.SetActive(true);
        voiceSystem.muteMyClientToggle.gameObject.SetActive(false);
        notAvalSymb.SetActive(true);


        foreach (Player playerInRoom in PhotonNetwork.CurrentRoom.Players.Values)
        {
            //if(playerInRoom.UserId != myID) gonna add this so you can remove your own debug echo too
            socialSystem.RefreshVoiceChatList(playerInRoom.UserId); //this tells urself too, but doesn't matter, because you have nothing to remove
        }
    }

    //receive
    public void ClearSpeaker(string id)
    {
        voiceSystem.ClearSpeaker(id);
    }

    public void ChangeNetworkInfoName(string name)
    {
        voiceSystem.ChangeNetworkInfoName(name); //changes the network info name

        //also broadcast the change to everyone in room!
        foreach (Player playerInRoom in PhotonNetwork.CurrentRoom.Players.Values)
        {
            socialSystem.UpdateVCUsernames(playerInRoom.UserId); //this tells urself too, updates debug username if appl.
        }

        //finally updates your local name in VC display
        myNameLocalDisplay.text = name + " (You)";
    }

    public void CheckCurrentSpeakerNames()
    {
        voiceSystem.CheckCurrentSpeakerNames();
    }

    public void OnOtherPlayerConnected(string id)
    {
        if(id != myID) //bcs private chat sends a message to self too.
        {
            voiceSystem.OnOtherPlayerConnected(id);
        }
    }

    public void AnnounceMeJoining() 
    {
        foreach (Player playerInRoom in PhotonNetwork.CurrentRoom.Players.Values)
        {
            if(playerInRoom.UserId != myID) socialSystem.AnnounceMeJoining(playerInRoom.UserId); 
        }
    }
}
