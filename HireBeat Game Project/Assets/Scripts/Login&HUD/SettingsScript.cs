using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class SettingsScript : MonoBehaviour
{
    public GameObject playerObj;
    public cameraController playerCamera;
    public InGameUIController playerZoneTab;
    public PlayerMenuUIController UIController;
    public changeReceiver playerHud;

    public Slider gameThemeVolumeSlider;

    public void Initialize() //awake is called before start, so it works ;D!!!!!!!!!!!!!!!!
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
        playerHud = GameObject.FindGameObjectWithTag("PlayerHUD").GetComponent<changeReceiver>();
    }

    // Start is called before the first frame update
    void Start()
    {
        playerObj = GameObject.FindGameObjectWithTag("Player");
        GameObject cameraController = GameObject.FindGameObjectWithTag("PlayerCamera");
        playerCamera = cameraController.GetComponent<cameraController>();
        UIController = cameraController.GetComponent<PlayerMenuUIController>();
        playerZoneTab = cameraController.GetComponent<InGameUIController>();
        playerHud = GameObject.FindGameObjectWithTag("PlayerHUD").transform.GetChild(0).GetComponent<changeReceiver>();
        if (!playerZoneTab.hasOneOn) //prevents zone + UI
        {
            playerObj.GetComponent<playerController>().enabled = false;
            playerCamera.enabled = false;
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTabOpen()
    {
        if (!playerZoneTab.hasOneOn) //prevents zone + UI
        {
            playerObj.GetComponent<playerController>().enabled = false;
            playerCamera.enabled = false;
        }
    }

    public void SetGameThemeVolume(float volume)
    {
        FindObjectOfType<AudioManager>().SetSceneThemeVolume(volume);
    }

    public void closeWindow()
    {
        //Destroy(gameObject);
        gameObject.SetActive(false);
        playerCamera.enabled = true;
        if (!PersistentData.isMovementRestricted)
        {
            playerObj.GetComponent<playerController>().enabled = true;
            playerObj.GetComponent<playerController>().actionParem = (int)playerController.CharActionCode.IDLE; //this line prevents the player from getitng stuck after
        }
        UIController.hasOneOn = false;
    }
}
