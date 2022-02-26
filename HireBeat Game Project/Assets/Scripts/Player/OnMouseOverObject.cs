using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class OnMouseOverObject : MonoBehaviour
{
    public GameObject hoverIndicator;
    public GameObject pfpDisplay;

    public GameObject playerInfoCardDisplay;
    public Text username;
    public Text signature;
    public Image profileSprite;
    public Text uniqueID;

    public GameObject playerObj;
    public cameraController playerCamera;
    public InGameUIController playerZoneTab;
    public PlayerMenuUIController UIController;
    public changeReceiver playerHud;

    PhotonView view;

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
    void OnMouseOver()
    {
        if(Input.GetMouseButtonDown(0))
        {
            UIController.instantiateProfileViewer(playerInfoCardDisplay);
        }
    }

    private void OnMouseEnter()
    {
        hoverIndicator.SetActive(true);
        pfpDisplay.SetActive(true);
    }

    void OnMouseExit()
    {
        hoverIndicator.SetActive(false);
        pfpDisplay.SetActive(false);
    }

    public void closeWindow()
    {
        playerInfoCardDisplay.SetActive(false); //want to keep data!
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
    }

    public void UpdateUsername(string newName)
    {
        view.RPC("UpdateUsernameRPC", RpcTarget.AllBuffered, newName);
    }

    [PunRPC]
    public void UpdateUsernameRPC(string newName)
    {
        username.text = newName;
    }

    public void UpdateSignature(string newSignature)
    {
        view.RPC("UpdateSignatureRPC", RpcTarget.AllBuffered, newSignature);
    }

    [PunRPC]
    public void UpdateSignatureRPC(string newSignature)
    {
        signature.text = newSignature;
    }

    //It's RPC is called in the profile updater
    public void UpdatePfpImage(Sprite newSprite)
    {
        profileSprite.sprite = newSprite;
    }
}
