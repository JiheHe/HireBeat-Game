using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class cameraController : MonoBehaviourPunCallbacks
{
    public float scrollSpeed = 10;
    public float minSize;
    public float maxSize;
    public GameObject UICamera;
    public Camera zoomCamera;


    //from testing, this object exists separately and locally on two different platforms, so we chilling
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetAxis("Mouse ScrollWheel") != 0 && zoomCamera.isActiveAndEnabled && zoomCamera.orthographic)
        {
            zoomCamera.orthographicSize -= Input.GetAxis("Mouse ScrollWheel") * scrollSpeed;
            if (zoomCamera.orthographicSize < minSize) zoomCamera.orthographicSize = minSize;
            else if (zoomCamera.orthographicSize > maxSize) zoomCamera.orthographicSize = maxSize;
        }
        /*else
        {
            zoomCamera.fieldOfView -= Input.GetAxis("Mouse ScrollWheel") * scrollSpeed;
        }*/
    }

    public void turnOnUICamera()
    {
        UICamera.SetActive(true);
    }

    public void turnOffUICamera()
    {
        UICamera.SetActive(false);
    }

    //no need for this, can directly assign at player spawn
    public void AssignCamera()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (player.GetComponent<PhotonView>().IsMine) //can also use GetComponent<playerController>().view.IsMine
            {
                zoomCamera = player.transform.Find("PlayerCamera").GetComponent<Camera>();
                break;
            }
        }
    }

}
