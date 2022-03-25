using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using Byn.Awrtc;


public class VidCRemoteInfo : MonoBehaviour, IPointerClickHandler
{
    public string userAcctID = null; //this is the default for "playfab id not ready yet, or local
    public ConnectionId userConnectionID = ConnectionId.INVALID; //this stays default for local

    public VideoChatController vidCController;

    private float mLastClick;

    public bool isLocal; //this is only ticked to true for the local copy, rest should default to false.

    /*public void InitializeIds(string userAcctId, ConnectionId userConnectionId)
    {
        userAcctID = userAcctId;
        userConnectionID = userConnectionId;
    }*/

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
        if(userAcctID != null)
        {
            var socialSystem = GameObject.FindGameObjectWithTag("PlayerHUD").transform.Find("SocialSystem").GetComponent<SocialSystemScript>();
            if (socialSystem.currentInfoCardOpened == null) //object self destructs into null on tab close
            {
                GameObject info = Instantiate(socialSystem.playerInfoCard, new Vector2(0, 0), Quaternion.identity); //can always use this to tune generation position/size
                info.transform.GetChild(0).transform.localPosition = new Vector2(288, 0); //shift x to the right of this generated card
                socialSystem.currentInfoCardOpened = info;
            }
            socialSystem.currentInfoCardOpened.GetComponent<PlayerInfoCardUpdater>().InitializeInfoCard(userAcctID, 0);
        }
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
