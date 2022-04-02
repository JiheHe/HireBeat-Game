using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class ContentChangerScript : MonoBehaviour
{

    public GameObject originalDisplay;
    public GameObject editorDisplay;
    public Text textSpot;
    public InputField newInput;
    public Text UITextTarget;
    public Text errorMsg = null;
    private IEnumerator errorMsgDisplay = null; //stays null for signature

    public bool canLeftEmpty; //rn username cannot be left empty, and signature can. Use that to distinguish

    public GameObject playerObj;
    OnMouseOverObject playerDataDisplay;

    PlayFabController PFC;

    DataBaseCommunicator dbc;

    // Start is called before the first frame update
    void Start()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (player.GetComponent<PhotonView>().IsMine) //can also use GetComponent<playerController>().view.IsMine
            {
                playerObj = player;
                break;
            }
        }
        playerDataDisplay = playerObj.transform.Find("PlayerMouseDetector").GetComponent<OnMouseOverObject>();

        PFC = GameObject.Find("PlayFabController").GetComponent<PlayFabController>();
        dbc = GameObject.FindGameObjectWithTag("DataCenter").GetComponent<DataBaseCommunicator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnEditorPressed() //enter edit mode
    {
        if(newInput != null) newInput.text = textSpot.text; //user can change!
        originalDisplay.SetActive(false);
        editorDisplay.SetActive(true);
        
    }

    string tempUserName; //only be used if username.
    public void OnConfirmationPressed() //exit edit mode
    {
        if(!canLeftEmpty && newInput.text.Replace(" ", "").Length == 0) 
        {
            Debug.Log("Such textfield cannot be empty!");
        } 
        else
        {
            if (!canLeftEmpty)
            {
                tempUserName = newInput.text;
                dbc.ChangeUserName(PFC.myID, tempUserName, this); //starts the callback
                //rn username cannot be left empty, and signature can. Use that to distinguish
            }
            else
            {
                textSpot.text = newInput.text;
                if (UITextTarget != null) UITextTarget.text = newInput.text;
                PFC.SetUserData("acctSignature", newInput.text, "Public");
                originalDisplay.SetActive(true);
                editorDisplay.SetActive(false);
            }
        }
        
    }
    public void ChangeUserNameResultCallback(bool wentThrough) //called from dbc, if this is for a username
    {
        if(wentThrough)
        {
            PFC.SetUserData("acctName", tempUserName, "Public");
            PFC.UpdateUserDisplayName(tempUserName); //also update Display name -> acct name is linked
            GameObject.Find("PersistentData").GetComponent<PersistentData>().acctName = tempUserName; //set PD, since VC net info regis. draws from there.
            PhotonNetwork.LocalPlayer.NickName = tempUserName; //changing photon name, which can be conveniently used for comparison! (real time update)
            GameObject.FindGameObjectWithTag("PlayerHUD").GetComponent<changeReceiver>().vcc.ChangeNetworkInfoName(tempUserName);
            textSpot.text = newInput.text;
            if (UITextTarget != null) UITextTarget.text = newInput.text;

            originalDisplay.SetActive(true);
            editorDisplay.SetActive(false);
            if (errorMsgDisplay != null)
            {
                StopCoroutine(errorMsgDisplay);
                errorMsg.gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.Log("Username already exists!");
            if(errorMsgDisplay != null) StopCoroutine(errorMsgDisplay); //"restart" coroutine
            errorMsgDisplay = DisplayErrorMessage(2f, "Username already exists..."); //each time a coro is called, a new obj is formed.
            StartCoroutine(errorMsgDisplay);
        }
    }

    IEnumerator DisplayErrorMessage(float time, string message)
    {
        errorMsg.gameObject.SetActive(true);
        errorMsg.text = message;
        yield return new WaitForSeconds(time);
        errorMsg.gameObject.SetActive(false);
    }

    public void OnCancelButtonPressed()
    {
        originalDisplay.SetActive(true);
        editorDisplay.SetActive(false);
        if (errorMsgDisplay != null)
        {
            StopCoroutine(errorMsgDisplay);
            errorMsg.gameObject.SetActive(false);
        }
    }
}
