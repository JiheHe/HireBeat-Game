using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class RoomSystemPanelScript : MonoBehaviour
{
    [HideInInspector] public GameObject playerObj;
    [HideInInspector] public cameraController playerCamera;
    [HideInInspector] public InGameUIController playerZoneTab;
    [HideInInspector] public PlayerMenuUIController UIController;
    [HideInInspector] public changeReceiver playerHud;

    void Awake() //awake is called before start, so it works ;D!!!!!!!!!!!!!!!!
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

        GameObject cameraController = GameObject.FindGameObjectWithTag("PlayerCamera");
        playerCamera = cameraController.GetComponent<cameraController>();
        UIController = cameraController.GetComponent<PlayerMenuUIController>();
        playerZoneTab = cameraController.GetComponent<InGameUIController>();
        playerHud = GameObject.FindGameObjectWithTag("PlayerHUD").transform.GetChild(0).GetComponent<changeReceiver>();
    }

    public GameObject roomInfoPanel;
    public Text roomNameTxt;
    public Text numPlayersInRoomTxt;
    public Text roomAccessTxt; //this is from database
    public InputField searchRoomBar;
    public GameObject ownRoomSettingsPanel;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnCurrentRoomInfoClicked()
    {
        if(!roomInfoPanel.activeSelf)
        {
            roomInfoPanel.SetActive(true);
            roomNameTxt.text = PhotonNetwork.CurrentRoom.Name;
            //Haven't set a cap on max player per room count yet, current it is 5. In MainMenu script.
            numPlayersInRoomTxt.text = PhotonNetwork.CurrentRoom.PlayerCount.ToString();

            SetRoomAccessText(false); //this should be a dbq database call to grab current access, direct for testing.
        }
        else
        {
            roomInfoPanel.SetActive(false);
        }
    }
    //This is the callback from DBC
    public void SetRoomAccessText(bool isPublic) //gonna consort the database on this. Photon room privacy not helpful.
    {
        if(isPublic)
        {
            roomAccessTxt.text = "Open to Public";
        }
        else
        {
            roomAccessTxt.text = "Private Invites Only";
        }
    }

    public void OnSearchRoomClicked()
    {
        //when inputting search options, only username will be sorted progressively
        //if it's account id, then wait till the end and enter
        if(!searchRoomBar.gameObject.activeSelf)
        {
            searchRoomBar.gameObject.SetActive(true);
        }
        else
        {
            searchRoomBar.gameObject.SetActive(false);
        }
    }

    public void OnOwnRoomSettingsClicked()
    {
        //add more settings in future!
        if(!ownRoomSettingsPanel.activeSelf)
        {
            ownRoomSettingsPanel.SetActive(true);
        }
        else
        {
            ownRoomSettingsPanel.SetActive(false);
        }
    }

    public void OnTabOpen()
    {
        if (!playerZoneTab.hasOneOn) //prevents zone + UI
        {
            playerObj.GetComponent<playerController>().enabled = false;
            playerCamera.enabled = false;
        }
    }

    public void CloseWindow()
    {
        gameObject.SetActive(false); //want to keep data!
        if (!playerZoneTab.hasOneOn)
        {
            playerObj.GetComponent<playerController>().enabled = true;
            playerCamera.enabled = true;
            playerObj.GetComponent<playerController>().isMoving = false; //this line prevents the player from getitng stuck after
        }
        UIController.hasOneOn = false;
    }
}
