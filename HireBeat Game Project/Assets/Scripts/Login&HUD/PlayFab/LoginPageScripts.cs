using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginPageScripts : MonoBehaviour
{
    public GameObject loginButton;
    public GameObject registerButton;
    public GameObject usernameInputField;
    public GameObject emailInputField;
    public GameObject toRegisterTabButton;
    public GameObject toLoginTabButton;

    public GameObject usernameOrEmailInputField;
    public GameObject passwordInputField;

    public GameObject loginErrorText;
    public GameObject registerErrorText;

    public GameObject loginPageObj;
    public GameObject askForAcctNameInputObj;
    public InputField acctNameInputField;
    public Text askForAcctNameErrorTxt;
    private IEnumerator errorMsgDisplay;

    public bool rememberMe;

    PlayFabController PFC = null;

    // Start is called before the first frame update
    void Start()
    {
        rememberMe = true;
    }

    public void toLoginMode()
    {
        loginButton.SetActive(true);
        registerButton.SetActive(false);
        usernameOrEmailInputField.SetActive(true);
        usernameInputField.SetActive(false);
        emailInputField.SetActive(false);
        toRegisterTabButton.SetActive(true);
        toLoginTabButton.SetActive(false);
        registerErrorText.SetActive(false);
    }

    public void toRegisterMode()
    {
        loginButton.SetActive(false);
        registerButton.SetActive(true);
        usernameOrEmailInputField.SetActive(false);
        usernameInputField.SetActive(true);
        emailInputField.SetActive(true);
        toRegisterTabButton.SetActive(false);
        toLoginTabButton.SetActive(true);
        loginErrorText.SetActive(false);
    }

    public void toAskAcctNameMode(PlayFabController pfc)
    {
        loginPageObj.SetActive(false);
        askForAcctNameInputObj.SetActive(true);
        PFC = pfc;
    }

    string acctName;
    public void OnPreferredAcctNameSubmit()
    {
        acctName = acctNameInputField.text;
        if(acctName.Length < 3)
        {
            DisplayErrorText(askForAcctNameErrorTxt, 3f, "Username has to have at least 3 characters");
        }
        else
        {
            string query = "SELECT * FROM UserDataStorage WHERE UserName = %acctName%";
            SQL4Unity.SQLParameter parameter = new SQL4Unity.SQLParameter();
            parameter.SetValue("acctName", acctName);
            DataBaseCommunicator.Execute(query, OnPreferredAcctNameSubmitCallback, parameter);
        }
    }
    private void OnPreferredAcctNameSubmitCallback(SQL4Unity.SQLResult result)
    {
        if(result.rowsAffected == 0) //the name is unique! Let it through and register the user on DBC.
        {
            PFC.setDataForAcctNameRegis = true; //this bool eventually lead to scene change.
            PFC.AddNewUserToDBCTable(acctName);
            PFC.SetUserData("acctName", acctName, "Public"); //acctName is the data version of display name
            PFC.UpdateUserDisplayName(acctName);
        }
        else //not unique... display error message
        {
            DisplayErrorText(askForAcctNameErrorTxt, 3f, "Username already exists");
        }
    }

    public void SetRememberMe() //starts off false, so every change changes it now
    {
        rememberMe = !rememberMe;
    }

    public void DisplayErrorText(Text errorMsg, float time, string message)
    {
        if (errorMsgDisplay != null) StopCoroutine(errorMsgDisplay); //"restart" coroutine
        errorMsgDisplay = DisplayErrorMessage(errorMsg, time, message);
        StartCoroutine(errorMsgDisplay);
    }

    IEnumerator DisplayErrorMessage(Text errorMsg, float time, string message)
    {
        errorMsg.gameObject.SetActive(true);
        errorMsg.text = message;
        yield return new WaitForSeconds(time);
        errorMsg.gameObject.SetActive(false);
    }

    public void DisplayEmailAndPassword(string usernameOrEmail, string password)
    {
        usernameOrEmailInputField.GetComponent<InputField>().text = usernameOrEmail;
        passwordInputField.GetComponent<InputField>().text = password;
    }
}
