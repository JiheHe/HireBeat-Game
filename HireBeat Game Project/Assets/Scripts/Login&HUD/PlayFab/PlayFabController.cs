using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;

//https://docs.microsoft.com/en-us/gaming/playfab/sdks/unity3d/quickstart#finish-and-execute copy pasta!
//In the future, change the login to login with HireBeat's data info
public class PlayFabController : MonoBehaviour
{
    public static PlayFabController PFC; //singleton

    #region LoginVariables
    public LoginPageScripts loginMenu;
    private string userEmail;
    private string userPassword;
    private string username;
    #endregion LoginVariables

    
    private void OnEnable() //making sure only 1 playfab controller
    {
        if(PlayFabController.PFC == null)
        {
            PlayFabController.PFC = this;
        }
        else
        {
            if(PlayFabController.PFC != this)
            {
                Destroy(this.gameObject);
            }
        }
        DontDestroyOnLoad(this.gameObject);
    }

    public void Start()
    {
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

    #region Login
    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Congratulations, your login attempt was successful!");
        if (!loginMenu.rememberMe) PlayerPrefs.DeleteAll(); //this removes saved info, so no auto login
        else SetPlayerLoginPrefs(); //"Remember me
        SceneManager.LoadScene("LoadingScene");
        loginMenu = null;
        GetStats();

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

    public void OnClickLogin()
    {
        var request = new LoginWithEmailAddressRequest { Email = userEmail, Password = userPassword };
        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnLoginFailure);
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
    public int playerExp; //for example

    #region PlayerStats

    //call this function anytime in our game when we want to push new info to the cloud
    //how to call it (cuz singleton): PlayFabController.PFC.SetStats();
    public void SetStats()
    {
        PlayFabClientAPI.UpdatePlayerStatistics(new UpdatePlayerStatisticsRequest
        {
            // request.Statistics is a list, so multiple StatisticUpdate objects can be defined if required.
            Statistics = new List<StatisticUpdate> {
                new StatisticUpdate { StatisticName = "PlayerExp", Value = playerExp },
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
                case "PlayerExp":
                    playerExp = eachStat.Value;
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

}