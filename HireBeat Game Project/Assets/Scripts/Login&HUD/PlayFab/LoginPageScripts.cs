using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginPageScripts : MonoBehaviour
{
    public GameObject loginButton;
    public GameObject registerButton;
    public GameObject usernameInputField;
    public GameObject toRegisterTabButton;
    public GameObject toLoginTabButton;

    public GameObject emailInputField;
    public GameObject passwordInputField;

    public GameObject loginErrorText;
    public GameObject registerErrorText;

    public bool rememberMe;

    // Start is called before the first frame update
    void Start()
    {
        rememberMe = true;
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

    public void SetRememberMe() //starts off false, so every change changes it now
    {
        rememberMe = !rememberMe;
    }

    public void DisplayErrorText(GameObject errorTextObj, string errorText)
    {
        errorTextObj.transform.Find("Label").GetComponent<Text>().text = errorText;
        errorTextObj.SetActive(true);
        StartCoroutine(WaitUntilClose(errorTextObj, 4f)); //no disappearing animation for now.
    }

    public void DisplayEmailAndPassword(string email, string password)
    {
        emailInputField.GetComponent<InputField>().text = email;
        passwordInputField.GetComponent<InputField>().text = password;
    }
}
