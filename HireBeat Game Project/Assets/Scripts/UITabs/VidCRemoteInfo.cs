using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using Byn.Awrtc;


public class VidCRemoteInfo : MonoBehaviour, IPointerClickHandler
{
    public string userAcctID;
    public ConnectionId userConnectionID = ConnectionId.INVALID; //this stays default for local

    public VideoChatController vidCController;

    private float mLastClick;

    public bool isLocal; //this is only ticked to true for the local copy, rest should default to false.

    public void InitializeIds(string userAcctId, ConnectionId userConnectionId)
    {
        userAcctID = userAcctId;
        userConnectionID = userConnectionId;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetVolume(float volume)
    {
        vidCController.SetVolume(volume, userConnectionID);
    }

    public void GenerateInfoCard()
    {
        Debug.Log("Haven't wrote this yet");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //Check for two clicks short after each other. Should work
        //on mobile and desktop platforms
        if ((eventData.clickTime - mLastClick) < 0.5f)
        {
            vidCController.ActivateSpeakerView(isLocal, userConnectionID);
        }
        /*else
        {
            mParent.ShowOverlay();
        }*/
        mLastClick = eventData.clickTime;
    }
}
