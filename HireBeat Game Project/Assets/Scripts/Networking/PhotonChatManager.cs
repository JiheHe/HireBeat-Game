using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Chat;
using Photon.Pun;
using PlayFab;
using PlayFab.ClientModels;
using ExitGames.Client.Photon;
using System.Linq;
using System;

public class PhotonChatManager : MonoBehaviour, IChatClientListener
{
    public SocialSystemScript socialSystem = null; //this is assigned by scene listener on load, it won't be null!
    public RoomSystemPanelScript roomSystem = null; //same
    public string publicRoomChatName;

    public void DebugReturn(DebugLevel level, string message)
    {
        //throw new System.NotImplementedException();
    }

    public void OnChatStateChange(ChatState state)
    {
        //throw new System.NotImplementedException();
    }

    public void OnConnected() //on connected, automatically connects to public room chat
    {
        AddPhotonChatFriends(); //hopefully this won't cause duplicates.
        ReconnectToRoomPublicChat();
        //throw new System.NotImplementedException();
    }

    public void ReconnectToRoomPublicChat()
    {
        //In order to avoid the complication that a user joins 10+ rooms -> 10+ channels, and I don't know how many public channels
        //a user can be in, it is the best to just allow 1 channel at a time (in room only!) and with no data caching. 
        chatClient.Unsubscribe(new string[] { socialSystem.currentPublicRoomChatName }); //will fail if null, so no worries.
        publicRoomChatName = PhotonNetwork.CurrentRoom.Name + " public room";
        chatClient.Subscribe(new string[] { publicRoomChatName }); //this is for the public room chat, room name will be owner ID
        socialSystem.CreatePublicRoomPanel(publicRoomChatName);
        Debug.Log("Public Chatroom Connected!"); //connected!
    }

    public void OnDisconnected()
    {
        //throw new System.NotImplementedException();
    }

    //triggered whenever a public chat message is published to a channel we are currently subscribed to
    //current just the public room chat
    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        if(channelName == publicRoomChatName) //could have more channels in the future
        {
            for(int i = 0; i < senders.Length; i++)
            {
                string sender = senders[i];
                object message = messages[i];
                if (sender != GetComponent<PlayFabController>().myID)
                {
                    GameObject receivingPanel = socialSystem.publicRoomChatPanel;
                    string msg = ((string[])message)[0];
                    string senderName = ((string[])message)[2];
                    string senderID = ((string[])message)[3];
                    DateTime dt = DateTime.FromBinary(long.Parse(((string[])message)[1]));
                    string sentTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(dt, TimeZoneInfo.Local.Id).ToString();
                    receivingPanel.GetComponent<MsgContentController>().AddMessage(senderName, sentTime, msg, false, senderID);
                }
            }
        }
    }

    //Whenever I send a private message to any user it also sends a copy of it because of its implementation in IChatClientListener interface.
    //This is by design, in Photon Chat all messages are broadcasted and received by sender even private ones.
    public void OnPrivateMessage(string sender, object message, string channelName)
    {
        //sender comes in as playfabID, so...
        //we know userID is constant, and we can get username from friendslisting because friendslisting's username updates with cloud ;D
        //Problem: if a user friends one before another one queues an update display, thehn the panel might not exist!
        //solution: upon receiving, if such panel doesn't exist, then create the panel now!
        if(message is string) //currently doing friend removal with status update's embedded message... could switch
        {
            switch ((string)message)
            {
                case "REFRESH LIST": //can use this for other friend removal, add friend, request sent, etc
                    GetComponent<PlayFabController>().GetFriends();
                    //Refresh info card too if there's one; make it refresh ONLY IF it's related with the request...
                    //nvm refresh is kinda trivia
                    return;
                case "LEAVING VC": //it's better to use voiceChatPanel.GetComponent<> for the next 3 msg, change in future.
                    GameObject.FindGameObjectWithTag("PlayerHUD").GetComponent<changeReceiver>().vcc.ClearSpeaker(sender);
                    return;
                case "NEW VCP JOINED":
                    GameObject.FindGameObjectWithTag("PlayerHUD").GetComponent<changeReceiver>().vcc.OnOtherPlayerConnected(sender);
                    return;
                case "UPDATE VC NAMES":
                    GameObject.FindGameObjectWithTag("PlayerHUD").GetComponent<changeReceiver>().vcc.CheckCurrentSpeakerNames();
                    return;
                case "REQSTING VCRM INFO": //normally only owner receieve this, and he usually has a room panel avail, so...
                    if(sender != GameObject.Find("PersistentData").GetComponent<PersistentData>().acctID) //sender sends a msg to myself too, avoid!
                    {
                        string userIdsString = socialSystem.videoChatPanel.GetComponent<VideoChatRoomSearch>().vCC.GetUserInVidCRoomIDsInString(sender);
                        socialSystem.SendVidCRoomInfo(sender, userIdsString); //sends back to the requester!
                    }
                    return;
            }
            if (sender != GameObject.Find("PersistentData").GetComponent<PersistentData>().acctID)
            {
                var reMessage = (string)message;
                if (reMessage.StartsWith("RECEI_VCRM_INFO")) //this is the callback from the owner
                {
                    reMessage = reMessage.Replace("RECEI_VCRM_INFO", ""); //thought replace replaces all occur, can do this because _ not in playfabid, so will only be first one.
                    socialSystem.videoChatPanel.GetComponent<VideoChatRoomSearch>().OnRoomOwnerInfoSendBack(reMessage.Split(","));
                }
                else if (reMessage.StartsWith("INVITE_TO_"))
                {
                    reMessage = reMessage.Substring("INVITE_TO_".Length); //this is the room name
                    Debug.Log("New Room Invite Receieved! Invited to " + reMessage);
                    socialSystem.videoChatPanel.GetComponent<VideoChatRoomSearch>().invitedRoomList.Add(reMessage); //you've been invited to this room!
                }
                else if(reMessage.StartsWith("RMINVT_TO_"))
                {
                    reMessage = reMessage.Substring("RMINVT_TO_".Length); //this is the roomID/name
                    Debug.Log("New User Room Invite Receieved! Invited to " + reMessage);
                    roomSystem.OnNewRoomInviteReceived(reMessage);
                }
            }
            return;
        }

        //actually chat message, they are sent in string arrays
        if(sender != GetComponent<PlayFabController>().myID) //watch out! also sends a copy to yourself
        {
            string msg = ((string[])message)[0];
            DateTime dt = DateTime.FromBinary(long.Parse(((string[])message)[1]));
            string sentTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(dt, TimeZoneInfo.Local.Id).ToString();
            //no need for this i think, since local conv. TimeZoneInfo.Local.StandardName
            if (socialSystem.chatPanels.ContainsKey(sender))
            {
                GameObject receivingPanel = socialSystem.chatPanels[sender];
                if(receivingPanel.GetComponent<MsgContentController>().listing != null) //listing's set, so display username
                {
                    receivingPanel.GetComponent<MsgContentController>().AddMessage(receivingPanel.GetComponent<MsgContentController>().listing.playerName.text,
                    sentTime, msg, false, "0000"); //haven't configurated time yet
                }
                else
                {
                    receivingPanel.GetComponent<MsgContentController>().AddMessage(receivingPanel.GetComponent<MsgContentController>().tempName, //gonna keep using id when sent 
                   sentTime, msg, false, "0000");
                }
                
            }
            else {
                GameObject tempPanel = Instantiate(socialSystem.chatPanel, socialSystem.msgViewPort.transform);
                tempPanel.transform.SetParent(socialSystem.msgViewPort.transform, false);
                tempPanel.GetComponent<RectTransform>().pivot = new Vector2(0, 0);
                tempPanel.GetComponent<MsgContentController>().listing = null;
                tempPanel.SetActive(false);
                socialSystem.chatPanels.Add(sender, tempPanel);

                PlayFabClientAPI.GetUserData(new GetUserDataRequest()
                {
                    PlayFabId = sender,
                    Keys = new List<string>() { "acctName" } //just acct name
                }, result => {
                    tempPanel.GetComponent<MsgContentController>().tempName = result.Data["acctName"].Value;
                    tempPanel.GetComponent<MsgContentController>().AddMessage(tempPanel.GetComponent<MsgContentController>().tempName,
                    sentTime, msg, false, "0000");
                }, (error) => {
                    Debug.Log("Got error retrieving user data:");
                    Debug.Log(error.GenerateErrorReport());
                });
            }
            
        }
    }

    //Friend status change call back
    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {
        //two possibitilies: 1. get friend is already called at this point with all prefabs ready. 2. set it, then call
        //Gonna try one: that's how the order goes in scene listener (rip latency)
        //Usually a friend has a chatbox yeah? and that chatbox uses socialsystem's dictionary. Prob can use that to locate status!
        if (status == ChatUserStatus.Online)
        {
            socialSystem.chatPanels[user].GetComponent<MsgContentController>().listing.changeOnStatus(true);
        }
        else
        {
            socialSystem.chatPanels[user].GetComponent<MsgContentController>().listing.changeOnStatus(false);
        }
    }

    public void OnSubscribed(string[] channels, bool[] results)
    {
        //Do something
        //throw new System.NotImplementedException();
    }

    public void OnUnsubscribed(string[] channels)
    {
        //throw new System.NotImplementedException();
    }

    public void OnUserSubscribed(string channel, string user)
    {
        //throw new System.NotImplementedException();
    }

    public void OnUserUnsubscribed(string channel, string user)
    {
        //throw new System.NotImplementedException();
    }

    public ChatClient chatClient;

    // Start is called before the first frame update
    void Awake()
    {
        chatClient = new ChatClient(this);
    }

    private string _playFabPlayerIdCache;

    //already did step 1 in playfabcontroller, gonna pass the result to step 2 as well

    /*
    * Step 2
    * We request Photon authentication token from PlayFab.
    * This is a crucial step, because Photon uses different authentication tokens
    * than PlayFab. Thus, you cannot directly use PlayFab SessionTicket and
    * you need to explicitly request a token. This API call requires you to
    * pass Photon App ID. App ID may be hard coded, but, in this example,
    * We are accessing it using convenient static field on PhotonNetwork class
    * We pass in AuthenticateWithPhoton as a callback to be our next step, if
    * we have acquired token successfully
    */
    public void RequestPhotonToken(LoginResult obj)
    {
        LogMessage("PlayFab authenticated. Requesting photon chat token...");
        //We can player PlayFabId. This will come in handy during next step
        _playFabPlayerIdCache = obj.PlayFabId;

        PlayFabClientAPI.GetPhotonAuthenticationToken(new GetPhotonAuthenticationTokenRequest()
        {
            PhotonApplicationId = PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat
        }, AuthenticateWithPhoton, OnPlayFabError);
    }

    /*
     * Step 3
     * This is the final and the simplest step. We create new AuthenticationValues instance.
     * This class describes how to authenticate a players inside Photon environment.
     */
    private void AuthenticateWithPhoton(GetPhotonAuthenticationTokenResult obj)
    {
        LogMessage("Photon chat token acquired: " + obj.PhotonCustomAuthenticationToken + "  Authentication complete.");

        //We set AuthType to custom, meaning we bring our own, PlayFab authentication procedure.
        var customAuth = new AuthenticationValues { AuthType = CustomAuthenticationType.Custom };
        //We add "username" parameter. Do not let it confuse you: PlayFab is expecting this parameter to contain player PlayFab ID (!) and not username.
        customAuth.AddAuthParameter("username", _playFabPlayerIdCache);    // expected by PlayFab custom auth service

        //We add "token" parameter. PlayFab expects it to contain Photon Authentication Token issues to your during previous step.
        customAuth.AddAuthParameter("token", obj.PhotonCustomAuthenticationToken);

        //Set up Photon Chat's userid to be Playfab's
        customAuth.UserId = _playFabPlayerIdCache;

        //We finally tell Photon to use this authentication parameters throughout the entire application.
        chatClient.AuthValues = customAuth;

        //Connect to Photon Server
        //Don't connect yet! wait for main scene load, then do it in scene listener
    }

    void DisplayPlayFabError(PlayFabError error)
    {
        Debug.Log(error.GenerateErrorReport());
    }

    public void ConnectChat()
    {
        chatClient.ConnectAndSetStatus(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat,
            PhotonNetwork.AppVersion, chatClient.AuthValues);
        isConnected = true;
        LogMessage("Connection to Photon Chat established.");
    }

    private void OnPlayFabError(PlayFabError obj)
    {
        Debug.LogError("PlayFab + PhotonChat: " + obj.GenerateErrorReport());
    }

    public void LogMessage(string message)
    {
        Debug.Log("PlayFab + PhotonChat: " + message);
    }

    bool isConnected = false;
    // Update is called once per frame
    void Update()
    {
        if(isConnected) chatClient.Service(); //this maintains client's connection to the server
    }

    void AddPhotonChatFriends() //no need to do this at beginning, playfab controller's display friend covers the case 1 by 1
     //Actaully idk if chat will be connected at that instance...
    {
        PlayFabClientAPI.GetFriendsList(new GetFriendsListRequest
        {
            IncludeSteamFriends = false,
            IncludeFacebookFriends = false,
            XboxToken = null
        }, result => {
            var friends = result.Friends;
            List<string> currFriends = new List<string>();
            foreach (FriendInfo f in friends)
            {
                if (f.Tags[0] == "confirmed")
                {
                    currFriends.Add(f.FriendPlayFabId);
                }
            }
            Debug.Log("Update Playfab friends attempt: " + chatClient.AddFriends(currFriends.ToArray())); //subscribe to current list of confirmed friends
        }, DisplayPlayFabError);
    }
}
