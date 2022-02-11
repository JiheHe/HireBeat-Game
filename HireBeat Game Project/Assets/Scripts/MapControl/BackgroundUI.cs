using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundUI : MonoBehaviour
{
    public int skyIndex;
    public GameObject background;

    public GameObject playerObj;
    public cameraController playerCamera;
    public InGameUIController UIController;
    // Start is called before the first frame update
    void Start()
    {
        background = GameObject.FindGameObjectWithTag("Background");
        playerObj = GameObject.FindGameObjectWithTag("Player");
        playerObj.SetActive(false);
        playerCamera = GameObject.FindGameObjectWithTag("PlayerCamera").GetComponent<cameraController>();
        playerCamera.turnOnUICamera();
        UIController = GameObject.FindGameObjectWithTag("PlayerCamera").GetComponent<InGameUIController>();
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
    }

    public void CloseTab()
    {
        Destroy(gameObject);
        playerCamera.turnOffUICamera();
        playerObj.SetActive(true);
        UIController.hasOneOn = false;
        playerObj.GetComponent<playerController>().isMoving = false;
    }

    public void setSkyIndex(int index)
    {
        skyIndex = index;
    }

}
