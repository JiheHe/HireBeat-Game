using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneListener : MonoBehaviour
{
    public GameObject playerHUD;

    void OnEnable()
    {
        //Tell our 'OnLevelFinishedLoading' function to start listening for a scene change as soon as this script is enabled.
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }

    void OnDisable()
    {
        //Tell our 'OnLevelFinishedLoading' function to stop listening for a scene change as soon as this script is disabled. Remember to always have an unsubscription for every delegate you subscribe to!
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }

    void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        if(scene.name == "MainScene" || PersistentData.commonRoomNamesAndRelatedSceneName.ContainsValue(scene.name))
        {
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(scene.name));
            SceneManager.UnloadSceneAsync("LoadingScene");
            if (!PhotonConnector.firstTimePCMConnectionDone) //hud doens't exist yet. use awake to assign stuff.
            {
                SceneManager.LoadSceneAsync("UIAdditiveOverlay", LoadSceneMode.Additive);
            }
            else
            {
                GetComponent<PhotonChatManager>().ReconnectToRoomPublicChat();
            }
        }

        if(scene.name == "UIAdditiveOverlay") //only init once.
        {
            Debug.Log("UI scene finished initializing!");

            playerHUD = GameObject.FindGameObjectWithTag("PlayerHUD");
            GetComponent<PlayFabController>().socialSystem = playerHUD.transform.Find("SocialSystem").GetComponent<SocialSystemScript>();
            //GetComponent<PlayFabController>().ClearMyFriends(); //no need for this, going to do additive scene loading.
            GetComponent<PlayFabController>().friendsList = playerHUD.GetComponent<changeReceiver>().friendsList;
            GetComponent<PlayFabController>().requesterList = playerHUD.GetComponent<changeReceiver>().requesterList;
            GetComponent<PlayFabController>().requesteeList = playerHUD.GetComponent<changeReceiver>().requesteeList;
            GetComponent<PlayFabController>().GetFriends(); //update friends immediately upon entering
            GetComponent<PhotonChatManager>().socialSystem = playerHUD.transform.Find("SocialSystem").GetComponent<SocialSystemScript>();
            playerHUD.transform.Find("SocialSystem").GetComponent<SocialSystemScript>().PCM = GetComponent<PhotonChatManager>();
            GetComponent<PhotonChatManager>().roomSystem = playerHUD.transform.Find("PlayerRoomSystem").GetComponent<RoomSystemPanelScript>();
            GetComponent<PhotonChatManager>().ConnectChat(); //make sure social system is ready, then connect

            PhotonConnector.firstTimePCMConnectionDone = true;
        }
    }
}
