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

    public Button inviteButton; //all this defaults to on

    //2 buttons, one for profile generate, one for invite

    public void SetUserInfo(string userName, string userId, bool isOnline, VideoChatController vcc = null, bool isAlreadyInRoom = false, bool isYou = false)
    {
        this.userName.text = userName;
        this.userId = userId;
        if (isOnline) userStatus.text = "Online";
        else userStatus.text = "Offline";

        this.vcc = vcc;
        //These two booleans are for video room chats, without them the rest functions normal too for friend system.
        if (isYou || isAlreadyInRoom)
        {
            inviteButton.gameObject.SetActive(false); //no need to invite yourself/others again if already in room.
            if(isYou) this.userName.color = new Color32(230, 164, 87, 255);
        }
    }

    //On View Profile clicked.
    public void GenerateInfoCard()
    {
        if (userId != null)
        {
            var socialSystem = vcc.vcs.socialSystem;
            if (socialSystem.currentInfoCardOpened == null) //object self destructs into null on tab close
            {
                GameObject info = Instantiate(socialSystem.playerInfoCard, new Vector2(0, 0), Quaternion.identity); //can always use this to tune generation position/size
                info.transform.GetChild(0).transform.localPosition = new Vector2(278, 0); //shift x to the right of this generated card
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
