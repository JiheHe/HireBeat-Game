using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Byn.Awrtc;

public class WebRTCVCChair : MonoBehaviour
{
    public ConnectionId currentLocalConnectionId;

    public int chairId; //assign this via inspector, unique for each!

    WebRTCVoiceChat terminal; //assigned by it.

    //Either a mute button, or a volume setter.
    public Toggle muteSelfToggle;
    public Slider volumeControl;

    //This is called once at the beginning of initialization
    public void SetTerminal(WebRTCVoiceChat wrtcvc)
    {
        terminal = wrtcvc;
    }

    //This is called every time a new user that's not you joins this chair.
    public void SetCurrentChairOwner(ConnectionId currentLocalConnectionId)
    {
        this.currentLocalConnectionId = currentLocalConnectionId;
        volumeControl.gameObject.SetActive(true); //Will implement them with a proper interface soon. This is for testingg.
        muteSelfToggle.transform.parent.gameObject.SetActive(false);
    }
    //This is called if you join this chair
    public void SetCurrentChairOwner()
    {
        currentLocalConnectionId = ConnectionId.INVALID;
        volumeControl.gameObject.SetActive(false);
        muteSelfToggle.transform.parent.gameObject.SetActive(true);
    }

    //SlideBar, for others
    public void SetVolume(float volume)
    {
        if (terminal.currentLocalWebRTCVCCallObj != null)
            terminal.currentLocalWebRTCVCCallObj.SetVolume(volume, currentLocalConnectionId);
    }

    //Button, for self
    public void SetMute(bool isMute)
    {
        if (terminal.currentLocalWebRTCVCCallObj != null)
            terminal.currentLocalWebRTCVCCallObj.SetMute(isMute);
    }

    public void OccupyThisChair()
    {
        if(terminal.chairsOccupationList[chairId])
        {
            Debug.Log("Sorry, this chair is already occupied.");
        }
        else
        {
            //Play animation
            //Join chair
            terminal.AnnounceChairOccupation(chairId, true, terminal.myID);
            terminal.InitializeWebRTCCall();
            SetCurrentChairOwner();
        }
    }

    public void LeaveThisChair()
    {
        terminal.AnnounceChairOccupation(chairId, false, null);
        Destroy(terminal.currentLocalWebRTCVCCallObj.gameObject);
        terminal.currentLocalWebRTCVCCallObj = null;
        //Play animation
    }

    public void HideInterface()
    {
        volumeControl.gameObject.SetActive(false);
        muteSelfToggle.transform.parent.gameObject.SetActive(false);
    }

    //Use keys for testing purposes, hold while walking in/out to trigger.
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (Input.GetKey(KeyCode.V))
        {
            OccupyThisChair();
            Debug.Log("Joining Private VC");
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (Input.GetKey(KeyCode.B))
        {
            LeaveThisChair();
            Debug.Log("Leaving Private VC");
        }
    }
}
