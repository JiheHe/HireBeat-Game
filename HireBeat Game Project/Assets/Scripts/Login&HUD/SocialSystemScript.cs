using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SocialSystemScript : MonoBehaviour
{
    public GameObject playerObj;
    public cameraController playerCamera;
    public InGameUIController playerZoneTab;
    public PlayerMenuUIController UIController;
    public changeReceiver playerHud;

    public GameObject profileEditor;

    public GameObject outputFinal; //this is where img is stored on HUD, grab the image and put it in
    public Image targetProfileDisplayPic;

    // Start is called before the first frame update
    void Awake() //awake is called before start, so it works ;D!!!!!!!!!!!!!!!!
    {
        playerObj = GameObject.FindGameObjectWithTag("Player");
        GameObject cameraController = GameObject.FindGameObjectWithTag("PlayerCamera");
        playerCamera = cameraController.GetComponent<cameraController>();
        UIController = cameraController.GetComponent<PlayerMenuUIController>();
        playerZoneTab = cameraController.GetComponent<InGameUIController>();
        playerHud = GameObject.FindGameObjectWithTag("PlayerHUD").transform.GetChild(0).GetComponent<changeReceiver>();

        //OnTabOpen shouldbe auto called so chilling
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void closeWindow()
    {
        gameObject.SetActive(false); //want to keep data!
        CloseProfileEditor();
        if (!playerZoneTab.hasOneOn)
        {
            playerObj.GetComponent<playerController>().enabled = true;
            playerCamera.enabled = true;
            playerObj.GetComponent<playerController>().isMoving = false; //this line prevents the player from getitng stuck after
        }
        UIController.hasOneOn = false;
    }

    public void OpenProfileEditor()
    {
        //targetProfileDisplayPic.sprite = outputFinal.GetComponent<Image>().sprite;
        profileEditor.SetActive(true);
    }

    public void CloseProfileEditor()
    {
        profileEditor.SetActive(false);
    }

    //On tab open, updates current display pic with the current profile pic in HUD
    //thought this is more convenient, a more efficient method would be to update the PDP as soon as
    //the HUD image is changed, from avatar custom script. But i'm too lazy soo... plus it's w/e LOL
    public void OnTabOpen()
    {
        targetProfileDisplayPic.sprite = outputFinal.GetComponent<Image>().sprite;
        if (!playerZoneTab.hasOneOn) //prevents zone + UI
        {
            playerObj.GetComponent<playerController>().enabled = false;
            playerCamera.enabled = false;
        }
    }
}

