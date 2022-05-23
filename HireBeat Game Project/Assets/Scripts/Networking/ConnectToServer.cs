using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
    Vector2 trainStartPosition;
    public float trainSpeed;
    public GameObject train;
    public Camera cam;

    public GameObject[] backgrounds;

    public GameObject kickMessage;

    private void Awake()
    {
        backgrounds[UnityEngine.Random.Range(0, backgrounds.Length)].SetActive(true);
    }

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<AudioManager>().PlayAll();

        trainStartPosition = cam.WorldToViewportPoint(train.transform.localPosition);
        trainSpeed = 800;

        //PhotonNetwork.ConnectUsingSettings(); //need to authenticate first, then connect through loading scene.
        StartCoroutine(WaitForUserLeftRoomConfirmed());

        if (PhotonConnector.disconnectDueToKicked)
        {
            //Set the additional text to "disconnect cuz kicked"
            kickMessage.SetActive(true);
        }
    }

    private void Update()
    {
        Vector3 viewPos = cam.WorldToViewportPoint(train.transform.position);
        if (viewPos.x < 0) //viewPos.x > 1 for left to right
        {
            // Your object is in the range of the camera, you can apply your behaviour
            train.transform.localPosition = cam.ViewportToWorldPoint(trainStartPosition);
        }
        else
            train.transform.Translate(new Vector2(-1, 0) * trainSpeed * Time.deltaTime);
    }

    IEnumerator WaitForUserLeftRoomConfirmed()
    {
        yield return new WaitForEndOfFrame();

        if(PhotonConnector.userHasLeftPhotonRoom)
        {
            yield return null;
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            yield return null;
            StartCoroutine(WaitForUserLeftRoomConfirmed());
        }
    }

    //this function is not called yet, but it does allow you to (I think))...
    //just some useful functions below ig
    //ALso I'm not doing server -> room instead of server -> lobby -> room?
    public void CreatePrivateRoom()
    {
        string randomName = $"Room{Guid.NewGuid().ToString()}";
        //The two steps below connect you to the master server (room) upon joining
        //PhotonNetwork.AutomaticallySyncScene = true; //this lets the master client dominates the scene transition
        //PhotonNetwork.ConnectUsingSettings(); //not sure if this is useful
    }
}
