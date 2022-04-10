using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InvitePlayerToRoomTab : MonoBehaviour
{
    public Text userName;
    public Text userStatus; //has ip = online, no = offline.
    public string userId;
    SocialSystemScript socialSystem;

    //2 buttons, one for profile generate, one for invite

    // Start is called before the first frame update
    void Start()
    {
        socialSystem = GameObject.FindGameObjectWithTag("PlayerHUD").transform.Find("SocialSystem").GetComponent<SocialSystemScript>();
    }

    // Update is called once per frame
    public void SetUserInfo(string userName, string userId, bool isOnline)
    {
        this.userName.text = userName;
        this.userId = userId;
        if (isOnline) userStatus.text = "Online";
        else userStatus.text = "Offline";
    }

    //On View Profile clicked.
    public void GenerateInfoCard()
    {
        if (userId != null)
        {
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
        socialSystem.SendUserRoomInvite(userId, PersistentData.TRUEOWNERID_OF_CURRENT_ROOM); //each room's name will be the owner's unique id!

        //Tell the sender that the invite has been sent.
        var rsps = socialSystem.rsps;
        if (rsps.errorMsgDisplay != null) StopCoroutine(rsps.errorMsgDisplay); //"restart" coroutine
        rsps.errorMsgDisplay = rsps.DisplayErrorMessage(3f, "Room Invite to user \"" +
            userName.text + "\" has been sent."); //each time a coro is called, a new obj is formed.
        StartCoroutine(rsps.errorMsgDisplay);
    }
}
