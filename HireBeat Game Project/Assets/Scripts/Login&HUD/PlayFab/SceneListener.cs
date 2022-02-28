using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneListener : MonoBehaviour
{
    GameObject playerHUD;

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
        if(scene.name == "MainScene")
        {
            playerHUD = GameObject.FindGameObjectWithTag("PlayerHUD");
            GetComponent<PlayFabController>().friendsList = playerHUD.GetComponent<changeReceiver>().friendsList;
            GetComponent<PlayFabController>().requesterList = playerHUD.GetComponent<changeReceiver>().requesterList;
            GetComponent<PlayFabController>().requesteeList = playerHUD.GetComponent<changeReceiver>().requesteeList;
            GetComponent<PlayFabController>().GetFriends(); //update friends immediately upon entering
        }
    }
}