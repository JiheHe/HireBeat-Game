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

    public Button inviteButton; //all 4 starts off false.
    public GameObject kickDivider;
    public Button kickButton;
    public GameObject viewDivider;

    //2 buttons, one for profile generate, one for invite

    // Start is called before the first frame update
    void Start()
    {
        socialSystem = GameObject.FindGameObjectWithTag("PlayerHUD").transform.Find("SocialSystem").GetComponent<SocialSystemScript>();
    }

    // Update is called once per frame
    public void SetUserInfo(string userName, string userId, bool isOnline, bool isInSearchView, bool isTrueOwner, bool isYou)
    {
        this.userName.text = userName;
        this.userId = userId;
        if (isOnline) userStatus.text = "Online";
        else userStatus.text = "Offline";

        if(isYou)
        {
            viewDivider.SetActive(true);
            this.userName.color = new Color32(230, 164, 87, 255);
        }
        else
        {
            if (isInSearchView) inviteButton.gameObject.SetActive(true);
            else if (isTrueOwner) //only owner can kick.
            {
                kickDivider.SetActive(true);
                kickButton.gameObject.SetActive(true);
            }
            else
            {
                viewDivider.SetActive(true);
            }
        }
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
        socialSystem.SendUserRoomInvite(userId); //each room's name will be the owner's unique id!

        //Tell the sender that the invite has been sent.
        var rsps = socialSystem.rsps;
        if (rsps.errorMsgDisplay != null) StopCoroutine(rsps.errorMsgDisplay); //"restart" coroutine
        rsps.errorMsgDisplay = rsps.DisplayErrorMessage(3f, "Room Invite to user \"" +
            userName.text + "\" has been sent."); //each time a coro is called, a new obj is formed.
        rsps.StartCoroutine(rsps.errorMsgDisplay);
    }

    public void OnKickPlayerPressed()
    {
        Debug.Log("User " + userId + " is getting kicked! (not banned, can still rejoin)");

        //Since kick button is only available when you are in the same room, RPC should go through!
        GameObject.FindGameObjectWithTag("DataCenter").GetComponent<RoomDataCentralizer>().SendKickPlayer(userId);

        //Now perform a tricky instant feedback upon kicking player
        //from the owner's perspective only, rpc call to all not needed I think
        int numPlayersOG = int.Parse(socialSystem.rsps.numPlayersInRoomTxt.text);
        socialSystem.rsps.numPlayersInRoomTxt.text = (numPlayersOG - 1).ToString();
        socialSystem.rsps.DestroyGivenUserTab(userId);
    }
}
