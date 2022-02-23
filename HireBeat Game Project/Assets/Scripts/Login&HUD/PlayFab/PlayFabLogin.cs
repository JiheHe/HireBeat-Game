using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

//https://docs.microsoft.com/en-us/gaming/playfab/sdks/unity3d/quickstart#finish-and-execute copy pasta!
//In the future, change the login to login with HireBeat's data info
public class PlayFabLogin : MonoBehaviour
{

    private string userEmail;
    private string userPassword;
    private string username;
    public GameObject loginPanel;

    public GameObject loginButton;
    public GameObject registerButton;
    public GameObject usernameInputField;
    public GameObject toRegisterTabButton;
    public GameObject toLoginTabButton;

    public GameObject emailInputField;
    public GameObject passwordInputField;

    public GameObject loginErrorText;
    public GameObject registerErrorText;

    bool rememberMe;

    public void Start()
    {
        rememberMe = true;

        if (string.IsNullOrEmpty(PlayFabSettings.staticSettings.TitleId))
        {
            /*
            Please change the titleId below to your own titleId from PlayFab Game Manager.
            If you have already set the value in the Editor Extensions, this can be skipped.
            */
            PlayFabSettings.staticSettings.TitleId = "BC556"; //I have, so I'll let it go
        }

        //No need to type in username if it's already registered. Prob need to make a register page.
        if (PlayerPrefs.HasKey("EMAIL")) //if remembered, but don't just login... input user data and for them to click the button ig
        {
            userEmail = PlayerPrefs.GetString("EMAIL");
            userPassword = PlayerPrefs.GetString("PASSWORD");
            emailInputField.GetComponent<InputField>().text = PlayerPrefs.GetString("EMAIL");
            passwordInputField.GetComponent<InputField>().text = PlayerPrefs.GetString("PASSWORD");
            //var request = new LoginWithEmailAddressRequest { Email = userEmail, Password = userPassword }; //these two lines are auto login lines, not sure if good idea
            //PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnLoginFailure);
        }

    }

    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Congratulations, your login attempt was successful!");
        if (!rememberMe) PlayerPrefs.DeleteAll(); //this removes saved info, so no auto login
        else SetPlayerPrefs(); //"Remember me
        //loginPanel.SetActive(false); //jump scene
        SceneManager.LoadScene("LoadingScene");
    }

    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        Debug.Log("Congratulations, a new user has been registered!");
        if (!rememberMe) PlayerPrefs.DeleteAll();
        else SetPlayerPrefs();
        //loginPanel.SetActive(false); //jump scene
        SceneManager.LoadScene("LoadingScene");
    }

    private void OnLoginFailure(PlayFabError error)
    {
        //if login fail, then jump to register page (what if wrong password?, covered! Don't need to jump.. just give error message. Have switch button)
        //Debug.LogError(error.GenerateErrorReport()); //"AccountNotFound" (no account with that email) or "InvalidUsernameOrPassword".
        loginErrorText.transform.Find("Label").GetComponent<Text>().text = error.GenerateErrorReport();
        loginErrorText.SetActive(true);
        StartCoroutine(WaitUntilClose(loginErrorText, 4f)); //no disappearing animation for now.
    }

    private void OnRegisterFailure(PlayFabError error)
    {
        //Debug.LogError(error.GenerateErrorReport());
        registerErrorText.transform.Find("Label").GetComponent<Text>().text = error.GenerateErrorReport();
        registerErrorText.SetActive(true);
        StartCoroutine(WaitUntilClose(registerErrorText, 4f));
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

    public void SetRememberMe() //starts off false, so every change changes it now
    {
        rememberMe = !rememberMe;
    }

    private void SetPlayerPrefs() //email and password for now
    {
        PlayerPrefs.SetString("EMAIL", userEmail);
        PlayerPrefs.SetString("PASSWORD", userPassword);
    }

    public void toLoginMode()
    {
        loginButton.SetActive(true);
        registerButton.SetActive(false);
        usernameInputField.SetActive(false);
        toRegisterTabButton.SetActive(true);
        toLoginTabButton.SetActive(false);
        registerErrorText.SetActive(false);
    }

    public void toRegisterMode()
    {
        loginButton.SetActive(false);
        registerButton.SetActive(true);
        usernameInputField.SetActive(true);
        toRegisterTabButton.SetActive(false);
        toLoginTabButton.SetActive(true);
        loginErrorText.SetActive(false);
    }

    IEnumerator WaitUntilClose(GameObject obj, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        obj.SetActive(false);
    }
}