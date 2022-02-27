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
    private string myID;
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
        SetUserData("acctID", myID);
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
        SetUserData("acctName", username); //acctName is the data version of display name
        SetUserData("acctID", myID);
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
    public void OnClickLogin()
    {
        var request = new LoginWithEmailAddressRequest { Email = userEmail, Password = userPassword };
        PlayFabClientAPI.LoginWithEmailAddress(request, RequestPhotonToken, OnLoginFailure); //not OnLoginSuccess anymore
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
    LoginResult result;
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

        OnLoginSuccess(result);
    }

    public void OnClickRegister()
    {
        var registerRequest = new RegisterPlayFabUserRequest { Email = userEmail, Password = userPassword, Username = username };
        PlayFabClientAPI.RegisterPlayFabUser(registerRequest, OnRegisterSuccess, OnRegisterFailure);
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

    public void SetUserData(string key, string value)
    {
        PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest()
        {
            Data = new Dictionary<string, string>()
            {
                {key, value}
            }
        }, SetDataSuccess, OnUserDataFailed);
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
        foreach (PlayFab.ClientModels.FriendInfo f in friendsCache)
        {
            bool isFound = false;

            if(myFriends != null)
            {
                foreach (PlayFab.ClientModels.FriendInfo g in myFriends)
                {
                    if (f.FriendPlayFabId == g.FriendPlayFabId)
                    {
                        isFound = true;
                        break;
                    }
                }
            }
            
            //making sure to not add duplicated friends
            if(isFound == false)
            {
                switch (f.Tags[0]) //might be mmultple for 2 way?
                {
                    case "confirmed":
                        GameObject listing = Instantiate(listingPrefab, friendsList);
                        FriendsListing tempListing = listing.GetComponent<FriendsListing>();
                        //probably need to set display name for it to work
                        tempListing.playerName.text = f.TitleDisplayName;
                        tempListing.playerID = f.FriendPlayFabId;
                        tempListing.PFC = this;
                        break;
                    case "requester":
                        GameObject requesterListing = Instantiate(requesterPrefab, requesterList);
                        FriendsListing requesterTempListing = requesterListing.GetComponent<FriendsListing>();
                        //probably need to set display name for it to work
                        requesterTempListing.playerName.text = f.TitleDisplayName;
                        requesterTempListing.playerID = f.FriendPlayFabId;
                        requesterTempListing.PFC = this;
                        break;
                    case "requestee":
                        GameObject requesteeListing = Instantiate(requesteePrefab, requesteeList);
                        FriendsListing requesteeTempListing = requesteeListing.GetComponent<FriendsListing>();
                        //probably need to set display name for it to work
                        requesteeTempListing.playerName.text = f.TitleDisplayName;
                        requesteeTempListing.playerID = f.FriendPlayFabId;
                        requesteeTempListing.PFC = this;
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

    List<PlayFab.ClientModels.FriendInfo> _friends = null; //friend result saved in there

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

    //Cloud script is retired...NVM JK I GOT SCAMMED THANK GOD
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
    }

    public void StartCloudAcceptFriendRequest(string friendPlayFabID)
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "AcceptFriendRequest", // Arbitrary function name
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