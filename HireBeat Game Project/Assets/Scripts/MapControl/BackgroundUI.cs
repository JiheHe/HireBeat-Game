using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BackgroundUI : MonoBehaviour
{
    public int skyIndex;
    public GameObject background;

    public GameObject playerObj;
    public cameraController playerCamera;
    public InGameUIController UIController;

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

        PFC = GameObject.Find("PlayFabController").GetComponent<PlayFabController>();

        background = GameObject.FindGameObjectWithTag("Background"); //no need to worry, since background is local, only 1
        //playerObj = GameObject.FindGameObjectWithTag("Player");
        //playerObj.SetActive(false);
        playerObj.GetComponent<playerController>().actionParem = (int)playerController.CharActionCode.IDLE;
        playerObj.GetComponent<playerController>().enabled = false;
        playerCamera = GameObject.FindGameObjectWithTag("PlayerCamera").GetComponent<cameraController>();
        playerCamera.enabled = false;
        //playerCamera.turnOnUICamera();
        UIController = playerCamera.gameObject.GetComponent<InGameUIController>();
        skyIndex = background.GetComponent<BackgroundSetter>().skyIndex;
    }

    public void okayButtonPressed()
    {
        sendInfo();
        CloseTab();
    }

    public void sendInfo()
    {
        BackgroundSetter bg = background.GetComponent<BackgroundSetter>();
        bg.skyIndex = skyIndex;
        bg.SetBackground();

        PFC.SetUserData("skyIndex", PersistentData.intToStr(skyIndex), "Private");
    }

    public void CloseTab()
    {
        Destroy(gameObject);
        //playerCamera.turnOffUICamera();
        //playerObj.SetActive(true);
        playerObj.GetComponent<playerController>().enabled = true;
        playerCamera.enabled = true;
        UIController.hasOneOn = false;
        playerObj.GetComponent<playerController>().actionParem = (int)playerController.CharActionCode.IDLE;
    }

    public void setSkyIndex(int index)
    {
        skyIndex = index;
    }

}
