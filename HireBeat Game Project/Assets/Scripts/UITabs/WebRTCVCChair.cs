using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Byn.Awrtc;
using Photon.Pun;

public class WebRTCVCChair : MonoBehaviour
{
    public ConnectionId currentLocalConnectionId;

    //Chair properties
    public int chairId; //assign this via inspector, unique for each!
    public Vector2 userInChairTurnOrientation;

    //These two variables are local, so they'll only track you!
    public Vector2 positionEntered = new Vector2(0, 0); //this keeps track of user entering position to connect / disconnect him
    public GameObject youThePlayer = null;

    WebRTCVoiceChat terminal; //assigned by it.

    //Either a mute button, or a volume setter.
    public Toggle muteSelfToggle;
    public Slider volumeControl;
    public Button leaveButton;
    public Button joinButton;

    //This is called once at the beginning of initialization
    public void SetTerminal(WebRTCVoiceChat wrtcvc)
    {
        terminal = wrtcvc;
    }

    //This is called every time a new user that's not you joins this chair.
    public void SetCurrentChairOwner(ConnectionId currentLocalConnectionId)
    {
        this.currentLocalConnectionId = currentLocalConnectionId;
        volumeControl.gameObject.SetActive(true); 
        muteSelfToggle.transform.parent.gameObject.SetActive(false);
        leaveButton.gameObject.SetActive(false);
    }
    //This is called if you join this chair
    public void SetCurrentChairOwner()
    {
        currentLocalConnectionId = ConnectionId.INVALID;
        volumeControl.gameObject.SetActive(false);
        muteSelfToggle.transform.parent.gameObject.SetActive(true);
        leaveButton.gameObject.SetActive(true);
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
            joinButton.gameObject.SetActive(false);
            youThePlayer = GameObject.FindGameObjectWithTag("PlayerCamera").GetComponent<cameraController>().zoomCamera.transform.parent.gameObject;
            youThePlayer.GetComponent<playerController>().enabled = false;
            youThePlayer.GetComponent<playerController>().ForceTurnTowards(userInChairTurnOrientation.x, userInChairTurnOrientation.y);
            youThePlayer.transform.position = GetComponent<Transform>().position;
            Debug.Log("Joining Private VC");
            //Join chair
            terminal.AnnounceChairOccupation(chairId, true, terminal.myID);
            terminal.InitializeWebRTCCall();
            SetCurrentChairOwner();
        }
    }

    //On leave button pressed.
    public void LeaveThisChair()
    {
        terminal.AnnounceChairOccupation(chairId, false, terminal.myID);
        Destroy(terminal.currentLocalWebRTCVCCallObj.gameObject);
        terminal.currentLocalWebRTCVCCallObj = null;
        //Play animation
        youThePlayer.GetComponent<Transform>().position = positionEntered;
        youThePlayer.GetComponent<playerController>().enabled = true;
        youThePlayer = null;
        Debug.Log("Leaving Private VC");
    }

    public void HideInterface()
    {
        volumeControl.gameObject.SetActive(false);
        muteSelfToggle.transform.parent.gameObject.SetActive(false);
        leaveButton.gameObject.SetActive(false);
    }

    //To avoid other users triggering the below, need to make sure it's you!
    //PositionEntered needs to save world position.
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (!terminal.chairsOccupationList[chairId] && collision.GetComponentInParent<PhotonView>().IsMine)
        {
            joinButton.gameObject.SetActive(true);
            positionEntered = collision.transform.parent.transform.position;
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        //Join button won't be active if we already clicked on it via joining
        //It only stays active if we didn't click on it.
        if(collision.GetComponentInParent<PhotonView>().IsMine && joinButton.gameObject.activeSelf)
        {
            joinButton.gameObject.SetActive(false);
            positionEntered = new Vector2(0, 0);
        }
    }
}
