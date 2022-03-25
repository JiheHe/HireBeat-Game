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

    public bool canLeftEmpty; //rn username cannot be left empty, and signature can. Use that to distinguish

    public GameObject playerObj;
    OnMouseOverObject playerDataDisplay;

    PlayFabController PFC;

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

    public void OnConfirmationPressed() //exit edit mode
    {
        if(!canLeftEmpty && newInput.text.Replace(" ", "").Length == 0) 
        {
            Debug.Log("Such textfield cannot be empty!");
        } 
        else
        {
            textSpot.text = newInput.text;
            if(UITextTarget != null) UITextTarget.text = newInput.text;
            if (!canLeftEmpty)
            {
                //rn username cannot be left empty, and signature can. Use that to distinguish
                PFC.SetUserData("acctName", newInput.text, "Public");
                PFC.UpdateUserDisplayName(newInput.text); //also update Display name -> acct name is linked
                GameObject.Find("PersistentData").GetComponent<PersistentData>().acctName = newInput.text; //set PD, since VC net info regis. draws from there.
                PhotonNetwork.LocalPlayer.NickName = newInput.text; //changing photon name, which can be conveniently used for comparison! (real time update)
                GameObject.FindGameObjectWithTag("PlayerHUD").GetComponent<changeReceiver>().vcc.ChangeNetworkInfoName(newInput.text);
            }
            else
            {
                PFC.SetUserData("acctSignature", newInput.text, "Public");
            }
            originalDisplay.SetActive(true);
            editorDisplay.SetActive(false);
        }
        
    }

    public void OnCancelButtonPressed()
    {
        originalDisplay.SetActive(true);
        editorDisplay.SetActive(false);
    }
}
