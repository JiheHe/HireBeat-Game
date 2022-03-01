using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;
using PlayFab.Json;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;


//https://docs.microsoft.com/en-us/gaming/playfab/sdks/unity3d/quickstart#finish-and-execute copy pasta!
//In the future, change the login to login with HireBeat's data info
public class PlayFabController : MonoBehaviour
{
    #region LoginVariables
    public LoginPageScripts loginMenu;
    private string userEmail;
    private string userPassword;
    private string username;
    #endregion LoginVariables
    public string myID;
    PersistentData PD;

    //use these as default internal error report
    void DisplayPlayFabError(PlayFabError error)
    {
        Debug.Log(error.GenerateErrorReport());
    }
    void DisplayError(string error)
    {
        Debug.LogError(error);
    }
    
    private void OnEnable() //making sure only 1 playfab controller
    {
        /*if(PlayFabController.PFC == null)
        {
            PlayFabController.PFC = this;
        }
        else
        {
            if(PlayFabController.PFC != this)
            {
                Destroy(this.gameObject);
            }
        }*/ //cant use damn signleton in multiplayer
        DontDestroyOnLoad(this.gameObject);
    }

    public void Start()
    {
        PD = GameObject.Find("PersistentData").GetComponent<PersistentData>();

        if (string.IsNullOrEmpty(PlayFabSettings.staticSettings.TitleId))
        {
            /*
            Please change the titleId below to your own titleId from PlayFab Game Manager.
            If you have already set the value in the Editor Extensions, this can be skipped.
            */
            PlayFabSettings.staticSettings.TitleId = "BC556"; //I have, so I'll let it go
        }

        if (PlayerPrefs.HasKey("EMAIL")) //if remembered, but don't just login... input user data and for them to click the button ig
        {
            userEmail = PlayerPrefs.GetString("EMAIL");
            userPassword = PlayerPrefs.GetString("PASSWORD");
            loginMenu.DisplayEmailAndPassword(userEmail, userPassword);
            //var request = new LoginWithEmailAddressRequest { Email = userEmail, Password = userPassword }; //these two lines are auto login lines, not sure if good idea
            //PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnLoginFailure);
        }
    }

    private string _playFabPlayerIdCache;

    #region Login
    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Congratulations, your login attempt was successful!");
        if (!loginMenu.rememberMe) PlayerPrefs.DeleteAll(); //this removes saved info, so no auto login
        else SetPlayerLoginPrefs(); //"Remember me
        SceneManager.LoadScene("LoadingScene");
        loginMenu = null;
        GetStats();

        myID = result.PlayFabId; //this is the unique ID!!!
        SetUserData("acctID", myID, "Public");
        PD.RetrieveUserData();

        //StartCloudDenyFriendRequest("7A98A976DE472605"); this is for testing purposes
    }

    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        Debug.Log("Congratulations, a new user has been registered!");
        if (!loginMenu.rememberMe) PlayerPrefs.DeleteAll();
        else SetPlayerLoginPrefs();
        SceneManager.LoadScene("LoadingScene");
        loginMenu = null;
        GetStats(); //player shouldn't have any values at this point
        //maybe set up a default stats value later

        myID = result.PlayFabId;
        UpdateUserDisplayName(username);
        SetUserData("acctName", username, "Public"); //acctName is the data version of display name
        SetUserData("acctID", myID, "Public");
        PD.RetrieveUserData(); //not necessary, not data, unless manually set at backend (cuz mostly will be null!)
    }

    void OnDisplayName(UpdateUserTitleDisplayNameResult result)
    {
        Debug.Log(result.DisplayName + " is your new display name");
    }

    public void UpdateUserDisplayName(string name)
    {
        PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest { DisplayName = name }, OnDisplayName, OnLoginFailure);
    }

    private void OnLoginFailure(PlayFabError error)
    {
        //if login fail, then jump to register page (what if wrong password?, covered! Don't need to jump.. just give error message. Have switch button)
        //Debug.LogError(error.GenerateErrorReport()); //"AccountNotFound" (no account with that email) or "InvalidUsernameOrPassword".
        loginMenu.DisplayErrorText(loginMenu.loginErrorText, error.GenerateErrorReport()); 
    }

    private void OnRegisterFailure(PlayFabError error)
    {
        loginMenu.DisplayErrorText(loginMenu.registerErrorText, error.GenerateErrorReport());
    }

    public void GetUserEmail(string emailIn)
    {
        userEmail = emailIn;
    }

    public void GetUserPassword(string passwordIn)
    {
        userPassword = passwordIn;
    }

    public void GetUsername(string usernameIn)
    {
        username = usernameIn;
    }

    /*
     * Step 1
     * We authenticate current PlayFab user normally.
     * You can absolutely use any Login method you want.
     * We pass RequestPhotonToken as a callback to be our next step, if
     * authentication was successful.
     */
    int type; //0 is login, 1 is register
    public void OnClickLogin()
    {
        type = 0;
        var request = new LoginWithEmailAddressRequest { Email = userEmail, Password = userPassword };
        PlayFabClientAPI.LoginWithEmailAddress(request, RequestPhotonToken, OnLoginFailure); //not OnLoginSuccess anymore
    }

    public void OnClickRegister()
    {
        type = 1;
        var registerRequest = new RegisterPlayFabUserRequest { Email = userEmail, Password = userPassword, Username = username };
        PlayFabClientAPI.RegisterPlayFabUser(registerRequest, RequestPhotonToken, OnRegisterFailure);
    }

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
    LoginResult result; //can prob use var
    private void RequestPhotonToken(LoginResult obj)
    {
        Debug.Log("PlayFab authenticated. Requesting photon token...");
        result = obj; //keep a record of playfab login result

        //We can player PlayFabId. This will come in handy during next step
        _playFabPlayerIdCache = obj.PlayFabId;

        PlayFabClientAPI.GetPhotonAuthenticationToken(new GetPhotonAuthenticationTokenRequest()
        {
            PhotonApplicationId = "a3518642-79f5-47cb-bb62-0439b7f63136" //PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat
        }, AuthenticateWithPhoton, DisplayPlayFabError);
    }
    RegisterPlayFabUserResult registerResult;
    private void RequestPhotonToken(RegisterPlayFabUserResult obj) //overloading
    {
        Debug.Log("PlayFab authenticated. Requesting photon token...");
        registerResult = obj; //keep a record of playfab login result

        //We can player PlayFabId. This will come in handy during next step
        _playFabPlayerIdCache = obj.PlayFabId;

        PlayFabClientAPI.GetPhotonAuthenticationToken(new GetPhotonAuthenticationTokenRequest()
        {
            PhotonApplicationId = "a3518642-79f5-47cb-bb62-0439b7f63136" //PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat
        }, AuthenticateWithPhoton, DisplayPlayFabError);
    }

    /*
     * Step 3
     * This is the final and the simplest step. We create new AuthenticationValues instance.
     * This class describes how to authenticate a players inside Photon environment.
     */
    private void AuthenticateWithPhoton(GetPhotonAuthenticationTokenResult obj)
    {
        Debug.Log("Photon token acquired: " + obj.PhotonCustomAuthenticationToken + "  Authentication complete.");

        //We set AuthType to custom, meaning we bring our own, PlayFab authentication procedure.
        var customAuth = new AuthenticationValues { AuthType = CustomAuthenticationType.Custom };
        //We add "username" parameter. Do not let it confuse you: PlayFab is expecting this parameter to contain player PlayFab ID (!) and not username.
        customAuth.AddAuthParameter("username", _playFabPlayerIdCache);    // expected by PlayFab custom auth service

        //We add "token" parameter. PlayFab expects it to contain Photon Authentication Token issues to your during previous step.
        customAuth.AddAuthParameter("token", obj.PhotonCustomAuthenticationToken);

        //We finally tell Photon to use this authentication parameters throughout the entire application.
        PhotonNetwork.AuthValues = customAuth;

        if (type == 0) OnLoginSuccess(result);
        else OnRegisterSuccess(registerResult);
    }

    private void SetPlayerLoginPrefs() //email and password for now
    {
        PlayerPrefs.SetString("EMAIL", userEmail);
        PlayerPrefs.SetString("PASSWORD", userPassword);
    }
    #endregion Login 

    //need to be 32 bit ints
    public int playerScoreTotal; //for example

    //https://www.youtube.com/watch?v=Xo9zRhzfb24&list=PLWeGoBm1YHVgi6ZcwWGt27Y4NHUAG5smX&index=8
    //Skipped cloud scripts and leaderboards (will implement in the future for important variables, not sure yet)

    #region PlayerStats

    //call this function anytime in our game when we want to push new info to the cloud
    //how to call it (cuz singleton): PlayFabController.PFC.SetStats();
    public void SetStats()
    {
        PlayFabClientAPI.UpdatePlayerStatistics(new UpdatePlayerStatisticsRequest
        {
            // request.Statistics is a list, so multiple StatisticUpdate objects can be defined if required.
            Statistics = new List<StatisticUpdate> {
                new StatisticUpdate { StatisticName = "PlayerScoreTotal", Value = playerScoreTotal },
                /*new StatisticUpdate { StatisticName = "OtherName", Value = variableName },
                new StatisticUpdate { StatisticName = "PlayerExp", Value = playerExp },
                new StatisticUpdate { StatisticName = "PlayerExp", Value = playerExp },*/
            }
        },
        result => { Debug.Log("User statistics updated"); },
        error => { Debug.LogError(error.GenerateErrorReport()); });
    }

    void GetStats()
    {
        PlayFabClientAPI.GetPlayerStatistics(
            new GetPlayerStatisticsRequest(),
            OnGetStats,
            error => Debug.LogError(error.GenerateErrorReport())
        );
    }

    void OnGetStats(GetPlayerStatisticsResult result)
    {
        Debug.Log("Received the following Statistics:");
        foreach (var eachStat in result.Statistics)
        {
            Debug.Log("Statistic (" + eachStat.StatisticName + "): " + eachStat.Value);
            switch(eachStat.StatisticName)
            {
                case "PlayerScoreTotal":
                    playerScoreTotal = eachStat.Value;
                    break;
                /*case "PlayerExp": //same idea! add more and update vars
                    playerExp = eachStat.Value;
                    break;
                case "PlayerExp":
                    playerExp = eachStat.Value;
                    break;*/
            }
        }
    }

    #endregion PlayerStats

    //Currently I'm thinking getting all saved data on login, and set new data as you go. So really, you only need to retrieve data once...
    //So you can add more properties!
    #region PlayerData
    public void GetPlayerData()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest()
        {
            PlayFabId = myID,
            Keys = null //setting this to null returns all the player data
            //else giving it a string returns specific, respective data
        }, SetCharProperties, OnUserDataFailed);
    }

    //updates the retrieved all data result to PD
    void SetCharProperties(GetUserDataResult result) //this one populates character setup, should make mulitple copies and call ig
    {
        if (result.Data == null)
        {
            Debug.Log("result is null!");
        }
        else
        {
            for (int i = 0; i < PD.charProperties.Length; i++)
            {
                string varName = PD.charProperties[i];
                if (!result.Data.ContainsKey(varName))
                {
                    Debug.Log(varName + " not set");
                }
                else
                {
                    //this sets the "key"-named variable in PD to the value of the return Data
                    PD.GetType().GetField(varName).SetValue(PD, result.Data[varName].Value);
                }
            }
        }
    }


    void OnUserDataFailed(PlayFabError error)
    {
        Debug.LogError(error.GenerateErrorReport());
    }

    public void SetUserData(string key, string value, string permission)
    {
        UpdateUserDataRequest request = new UpdateUserDataRequest();
        request.Data = new Dictionary<string, string>() { { key, value } }; //can have multiple
        if (permission == "Public") request.Permission = UserDataPermission.Public;
        else request.Permission = UserDataPermission.Private;
        PlayFabClientAPI.UpdateUserData(request, SetDataSuccess, OnUserDataFailed);
    }

    

    void SetDataSuccess(UpdateUserDataResult result)
    {
        //Debug.Log(result.DataVersion);
    }

    #endregion PlayerData

    //No need to sync friend list so won't make it prefab


    #region Friends

    public GameObject listingPrefab; //confirmed friend
    public GameObject requesterPrefab;
    public GameObject requesteePrefab;
    [SerializeField]
    public Transform friendsList;
    public Transform requesterList;
    public Transform requesteeList;
    List<PlayFab.ClientModels.FriendInfo> myFriends;
    bool requestAccepted = false; //upon request accepted, update

    //Last function in the process, you decide how you wanna show it ;D
    void DisplayFriends(List<PlayFab.ClientModels.FriendInfo> friendsCache)
    {
        //when displaying, only display friends tagged with CONFIRMED in the list
        //if it's a requestee (you are requesting), then it will be displayed on a separate window
        //if it's a requestor (requesting to you), then it will be displayed on a separate window
        Debug.Log("Updating friends lists");

        //WAIT, CHECK EACH USER TAB TO SEE IF IT NEEDS TO EXIST!!!!!! (if no more friends connection, then should be gone forever)
        //if tag changes, then change tab: ex- (examples need another user's input to execute)
        //if prefab type was requester, and other player accept, so tag changes to confirmed -> destroy requester prefab and make a confirmed prefab
        //^ basically any requester/requestee to confirmed results in destruction of requester/requestee and construction of confirmed
        //in any other chases, if a prefab no longer has ANY connection to EITHER friend list (should be), then destroy that prefab on both end
        int numFriends = friendsList.childCount; 
        int numRequesters = requesterList.childCount;
        int numRequestees = requesteeList.childCount;
        GameObject[] userTabs = new GameObject[numFriends + numRequesters + numRequestees];
        int currIndex = 0;
        for(int i = 0; i < numFriends; i++)
        {
            userTabs[currIndex] = friendsList.GetChild(i).gameObject;
            currIndex++;
        }
        for (int i = 0; i < numRequesters; i++)
        {
            userTabs[currIndex] = requesterList.GetChild(i).gameObject;
            currIndex++;
        }
        for (int i = 0; i < numRequestees; i++)
        {
            userTabs[currIndex] = requesteeList.GetChild(i).gameObject;
            currIndex++;
        }

        //GameObject[] userTabs = GameObject.FindGameObjectsWithTag("UserTab"); //this is not efficient, also can't find inactive objects...
        foreach (GameObject userTab in userTabs) //try to merge the two big loops later, trying now it works! (nvm it doesn't feel efficient to merge)
        {
            bool isConnectedFriend = false; //check case 2
            foreach (PlayFab.ClientModels.FriendInfo f in friendsCache)
            {
                if (userTab.GetComponent<FriendsListing>().playerID == f.FriendPlayFabId) //find matching friend list first
                {
                    isConnectedFriend = true; //still have some connections, so keep as it is

                    //updates display acct name if it has been updated by the user (going to update on status like this too, somehow)
                    if(userTab.GetComponent<FriendsListing>().playerName.text != f.TitleDisplayName)
                    {
                        userTab.GetComponent<FriendsListing>().playerName.text = f.TitleDisplayName;
                    }

                    //check cases
                    if ((userTab.GetComponent<FriendsListing>().type == "requester" || userTab.GetComponent<FriendsListing>().type == "requestee")
                        && f.Tags[0] == "confirmed") //this is case 1
                    {
                        //Destroy request type prefab
                        Destroy(userTab);

                        foreach (PlayFab.ClientModels.FriendInfo g in myFriends) //hard coding remove
                        {
                            if (f.FriendPlayFabId == g.FriendPlayFabId) //if there are duplicates (f is the new list, g is the old list)
                            {
                                myFriends.Remove(g);
                                break;
                            }
                        }
                    }
                    break; //no other cases, we good. Move onto next userTab
                }
            }
            if (!isConnectedFriend) Destroy(userTab); //no connections at all, so destroy
        }

        //The below checks for if there's any new info
        foreach (PlayFab.ClientModels.FriendInfo f in friendsCache)
        {
            bool isFound = false;

            if(myFriends != null)
            {
                foreach (PlayFab.ClientModels.FriendInfo g in myFriends)
                {
                    if (f.FriendPlayFabId == g.FriendPlayFabId) //if there are duplicates (f is the new list, g is the old list)
                    {
                        isFound = true;
                        break;
                    }
                }
            }
            
            //making sure to not add duplicated friends
            //if there are new friends from data base. This is an elem from the new list that's not in the old list
            if(isFound == false)
            {
                switch (f.Tags[0]) //might be mmultple for 2 way?
                {
                    case "confirmed":
                        if (!requestAccepted) //if confirmed, then will make it. But request accepted also makes another one... avoids dup
                        {
                            GameObject listing = Instantiate(listingPrefab, friendsList);
                            FriendsListing tempListing = listing.GetComponent<FriendsListing>();
                            //probably need to set display name for it to work
                            tempListing.playerName.text = f.TitleDisplayName;
                            tempListing.playerID = f.FriendPlayFabId;
                            tempListing.PFC = this;
                            tempListing.type = "confirmed";
                        }
                        break;
                    case "requester":
                        GameObject requesterListing = Instantiate(requesterPrefab, requesterList);
                        FriendsListing requesterTempListing = requesterListing.GetComponent<FriendsListing>();
                        //probably need to set display name for it to work
                        requesterTempListing.playerName.text = f.TitleDisplayName;
                        requesterTempListing.playerID = f.FriendPlayFabId;
                        requesterTempListing.PFC = this;
                        requesterTempListing.type = "requester";
                        break;
                    case "requestee":
                        GameObject requesteeListing = Instantiate(requesteePrefab, requesteeList);
                        FriendsListing requesteeTempListing = requesteeListing.GetComponent<FriendsListing>();
                        //probably need to set display name for it to work
                        requesteeTempListing.playerName.text = f.TitleDisplayName;
                        requesteeTempListing.playerID = f.FriendPlayFabId;
                        requesteeTempListing.PFC = this;
                        requesteeTempListing.type = "requestee";
                        break;
                }
                //Debug.Log("Friend type is: " + f.Tags[0]);
            }

            if(requestAccepted) //instant local feedback upon accepting
            {
                GameObject listing = Instantiate(listingPrefab, friendsList);
                FriendsListing tempListing = listing.GetComponent<FriendsListing>();
                //probably need to set display name for it to work
                tempListing.playerName.text = f.TitleDisplayName;
                tempListing.playerID = f.FriendPlayFabId;
                tempListing.PFC = this;
                requestAccepted = false;
            }
        }
        myFriends = friendsCache;
    }

    IEnumerator WaitForFriend()
    {
        yield return new WaitForSeconds(2);
        GetFriends();
    }

    public void RunWaitFunction()
    {
        StartCoroutine(WaitForFriend());
    }

    public List<PlayFab.ClientModels.FriendInfo> _friends = null; //friend result saved in there

    public void GetFriends() //set to public so can be used with buttons 
    {
        PlayFabClientAPI.GetFriendsList(new GetFriendsListRequest
        {
            IncludeSteamFriends = false,
            IncludeFacebookFriends = false,
            XboxToken = null
        }, result => {
            _friends = result.Friends;
            DisplayFriends(_friends); // triggers your UI
        }, DisplayPlayFabError);
    }

    enum FriendIdType { PlayFabId, Username, Email, DisplayName };

    void AddFriend(FriendIdType idType, string friendId)
    {
        var request = new AddFriendRequest();
        switch (idType) //different ways to search for your friend
        {
            case FriendIdType.PlayFabId:
                request.FriendPlayFabId = friendId;
                break;
            case FriendIdType.Username:
                request.FriendUsername = friendId;
                break;
            case FriendIdType.Email:
                request.FriendEmail = friendId;
                break;
            case FriendIdType.DisplayName:
                request.FriendTitleDisplayName = friendId;
                break;
        }
        // Execute request and update friends when we are done
        PlayFabClientAPI.AddFriend(request, result => {
            Debug.Log("Friend added successfully!");
        }, DisplayPlayFabError);
    }

    string friendSearch;
    [SerializeField]
    //GameObject friendPanel; //search options and listings

    public void InputFriendID(string idIn)
    {
        friendSearch = idIn;
    }

    public void SubmitFriendRequest()
    {
        //if want to add by other method, change PlayFabId (enum) to Username, Email, or DisplayName
        //and make friendsearch ask for email/username/display name etc
        AddFriend(FriendIdType.PlayFabId, friendSearch);
    }

    /*public void OpenCloseFriends() //no need
    {
        friendPanel.SetActive(!friendPanel.activeInHierarchy); //inversing
    }*/

    //ACCEPT REQUEST ONLY CHANGE TAGS!!!!! THEY NEED TO BE FRIENDS THROUGH FRIEND REQUEST FIRST!!!!!!

    //Cloud script is retired...NVM JK I GOT SCAMMED THANK GOD
    //Before send cloud friend request, check to see if you two are already friended, and the requestee (him) is a requester (to you)
    public void StartCloudSendFriendRequest(string friendPlayFabID)
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "SendFriendRequest", // Arbitrary function name
            FunctionParameter = new { FriendPlayFabId = friendPlayFabID},
            GeneratePlayStreamEvent = false
        }, OnCloudSendFriendRequest, DisplayPlayFabError);
    }

    private void OnCloudSendFriendRequest(ExecuteCloudScriptResult result)
    {
        /*Debug.Log(result.FunctionResult.ToString());
        JsonObject jsonResult = (JsonObject)result.FunctionResult;
        object messageValue;
        jsonResult.TryGetValue("messageValue", out messageValue);
        Debug.Log((string)messageValue);*/
        Debug.Log("Friend Request sent!");
        GetFriends();
    }

    //This version sets the tags of two users into confirmed friends
    public void StartCloudAcceptFriendRequest(string friendPlayFabID)
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "AcceptFriendRequest", // Arbitrary function name
            FunctionParameter = new { FriendPlayFabId = friendPlayFabID },
            GeneratePlayStreamEvent = false
        }, OnCloudAcceptFriendRequest, DisplayPlayFabError);
    }

    //This version adds the two players into friends and sets their tags to confirmed, if they are not friends already
    public void StartCloudAddAndAcceptFriendRequest(string friendPlayFabID)
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "AddAndAcceptFriendRequest", // Arbitrary function name
            FunctionParameter = new { FriendPlayFabId = friendPlayFabID },
            GeneratePlayStreamEvent = false
        }, OnCloudAcceptFriendRequest, DisplayPlayFabError);
    }


    private void OnCloudAcceptFriendRequest(ExecuteCloudScriptResult result)
    {
        Debug.Log("Friend Request accepted!");
        requestAccepted = true;
        GetFriends();
    }

    public void StartCloudDenyFriendRequest(string friendPlayFabID)
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "DenyFriendRequest", // Arbitrary function name
            FunctionParameter = new { FriendPlayFabId = friendPlayFabID },
            GeneratePlayStreamEvent = false
        }, OnCloudDenyFriendRequest, DisplayPlayFabError);
    }

    private void OnCloudDenyFriendRequest(ExecuteCloudScriptResult result)
    {
        Debug.Log("Friend Request denied!");
        GetFriends();
    }





    #endregion Friends

}