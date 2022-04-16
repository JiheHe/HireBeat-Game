using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IPTR_Simple : MonoBehaviour
{
    public Text userName;
    public Text userStatus; //has ip = online, no = offline.
    public string userId;
    VideoChatController vcc = null; //this is set for video chat only, not for friend system
    SocialSystemScript socialSystem = null; //this is for friend system only

    public Button inviteButton; //all this defaults to on
    public float infoCardXShift = 278; //278, to the right, for most cases, except when for friend search.

    //2 buttons, one for profile generate, one for invite

    public void SetUserInfo(string userName, string userId, bool isOnline, SocialSystemScript ss = null, VideoChatController vcc = null, bool isAlreadyInRoom = false, bool isYou = false)
    {
        this.userName.text = userName;
        this.userId = userId;

        if(vcc != null) //if it's from video chat room invite search, then show status and invite button
        {
            if (isOnline) userStatus.text = "Online";
            else userStatus.text = "Offline";
            this.vcc = vcc;
            socialSystem = vcc.vcs.socialSystem;

            //These two booleans are for video room chats, without them the rest functions normal too for friend system.
            if (isYou || isAlreadyInRoom)
            {
                inviteButton.gameObject.SetActive(false); //no need to invite yourself/others again if already in room.
                if (isYou) this.userName.color = new Color32(230, 164, 87, 255);
            }
        }
        else //otherwise no need, cuz friend system. Just need view
        {
            socialSystem = ss; //it's either SSS null or VCC null
            userStatus.transform.parent.gameObject.SetActive(false);
            inviteButton.gameObject.SetActive(false);
        }
    }

    //On View Profile clicked.
    public void GenerateInfoCard()
    {
        if (userId != null)
        {
            //var socialSystem = vcc.vcs.socialSystem;
            if (socialSystem.currentInfoCardOpened == null) //object self destructs into null on tab close
            {
                GameObject info = Instantiate(socialSystem.playerInfoCard, new Vector2(0, 0), Quaternion.identity); //can always use this to tune generation position/size
                info.transform.GetChild(0).transform.localPosition = new Vector2(infoCardXShift, 0); //shift x to the right of this generated card
                socialSystem.currentInfoCardOpened = info;
            }
            socialSystem.currentInfoCardOpened.GetComponent<PlayerInfoCardUpdater>().InitializeInfoCard(userId, 0);
        }
    }

    public void OnInvitePressed()
    {
        //Send a Photon message to the userId.
        vcc.OnInviteUserButtonPressed(userId);
    }
}