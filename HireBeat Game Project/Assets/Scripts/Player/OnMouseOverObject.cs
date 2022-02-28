using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class OnMouseOverObject : MonoBehaviour
{
    public GameObject hoverIndicator;
    public GameObject pfpDisplay;

    public GameObject playerObj;
    public cameraController playerCamera;
    public InGameUIController playerZoneTab;
    public PlayerMenuUIController UIController;
    public changeReceiver playerHud;

    PhotonView view;

    //only need these 2
    public string PlayFabID;
    public GameObject playerInfoCard;

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
        } //cannot just get parent! else that client's obj will be disabled. need to find your own! (local)

        view = GetComponent<PhotonView>(); //maybe better to use this?

        GameObject cameraController = GameObject.FindGameObjectWithTag("PlayerCamera");
        playerCamera = cameraController.GetComponent<cameraController>();
        UIController = cameraController.GetComponent<PlayerMenuUIController>();
        playerZoneTab = cameraController.GetComponent<InGameUIController>();
        playerHud = GameObject.FindGameObjectWithTag("PlayerHUD").transform.GetChild(0).GetComponent<changeReceiver>();
    }

    // Start is called before the first frame update
    GameObject info;
    void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0) && !view.IsMine) //doing so disable user interacting on himself
        {
            //UIController.instantiateProfileViewer(playerInfoCardDisplay); //this is like the one window thing //info card shouldn't be controlled by it...
            if(info != null) //object self destructs into null on tab close
            {
                Destroy(info.gameObject);
            }
            info = Instantiate(playerInfoCard, new Vector2(0, 0), Quaternion.identity); //need to make new instance! else gonna overwrite prefab
            info.GetComponent<PlayerInfoCardUpdater>().InitializeInfoCard(PlayFabID, 0); //lobby click
        }
    }

    private void OnMouseEnter()
    {
        if(!view.IsMine)
        {
            hoverIndicator.SetActive(true);
            pfpDisplay.SetActive(true);
        }
    }

    void OnMouseExit()
    {
        if (!view.IsMine)
        {
            hoverIndicator.SetActive(false);
            pfpDisplay.SetActive(false);
        }
    }

    //these two are currently uses since I'm not using a saved card with UI controller. It's a prefab!
    /*public void closeWindow()
    {
        if (!playerZoneTab.hasOneOn)
        {
            playerObj.GetComponent<playerController>().enabled = true;
            playerCamera.enabled = true;
            playerObj.GetComponent<playerController>().isMoving = false; //this line prevents the player from getitng stuck after
        }
        UIController.hasOneOn = false;
    }

    public void OnTabOpen()
    {
        if (!playerZoneTab.hasOneOn) //prevents zone + UI
        {
            playerObj.GetComponent<playerController>().enabled = false;
            playerCamera.enabled = false;
        }
    }*/

    //No need for RPC update functions! grab directly from data base
}
