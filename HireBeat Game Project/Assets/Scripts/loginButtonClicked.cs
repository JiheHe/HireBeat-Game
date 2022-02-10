using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class loginButtonClicked : MonoBehaviour
{
    public GameObject loginWindow;
    public GameObject usernameTextObj;
    public GameObject passwordTextObj;
    public GameObject errorTextObj;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("hewwo");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void loginButtonClick()
    {
        string unText = usernameTextObj.GetComponent<Text>().text;
        string pwText = passwordTextObj.GetComponent<Text>().text;

        if(unText.Length < 5 || unText.Length > 12 || pwText.Length < 1 || pwText.Length > 100) {
            Debug.Log("Username must be between 5 and 12 characters. Password must be less than 100 characters.");
            errorTextObj.SetActive(true);
        } else {
            Debug.Log("Username: " + unText);
            Debug.Log("Password: " + pwText);
            loginWindow.SetActive(false);
        }
        //SceneManager.LoadScene("uiTestScene");
    }
}
