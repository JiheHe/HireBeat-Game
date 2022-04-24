using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsScript : MonoBehaviour
{
    public GameObject playerObj;
    public cameraController playerCamera;
    public InGameUIController playerZoneTab;
    public PlayerMenuUIController UIController;
    public changeReceiver playerHud;

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

    public void closeWindow()
    {
        Destroy(gameObject);
        playerCamera.enabled = true;
        if (!PersistentData.isMovementRestricted)
        {
            playerObj.GetComponent<playerController>().enabled = true;
            playerObj.GetComponent<playerController>().actionParem = (int)playerController.CharActionCode.IDLE; //this line prevents the player from getitng stuck after
        }
        UIController.hasOneOn = false;
    }
}
