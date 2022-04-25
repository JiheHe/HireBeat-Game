using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMenuUIController : MonoBehaviour
{
    public bool hasOneOn;
    public GameObject profilePicPickerUI; //I might need a separate UI script for this; zone disable won't work...
    //this one will always be active with buttons, no zones
    public GameObject settingsUI;
    public GameObject questsUI;
    public GameObject socialSystemUI;
    public GameObject roomSystemUI;

    public InGameUIController UIController;

    private void Awake()
    {
        StartCoroutine(AssignSocialAndRoomSystemUIs());
    }

    // Start is called before the first frame update
    void Start()
    {
        UIController = GetComponent<InGameUIController>();
    }

    IEnumerator AssignSocialAndRoomSystemUIs()
    {
        var playerHud = GameObject.FindGameObjectWithTag("PlayerHUD");
        if (playerHud == null)
        {
            yield return null;
            StartCoroutine(AssignSocialAndRoomSystemUIs());
        }
        else
        {
            yield return null;
            socialSystemUI = playerHud.transform.Find("SocialSystem").gameObject;
            roomSystemUI = playerHud.transform.Find("PlayerRoomSystem").gameObject;
            UIController.voiceChatPanelUI = GameObject.Find("GlobalRoomVoiceChat").transform.Find("VoiceChat").gameObject;
        }
    }

    public void instantiateProfilePicPicker()
    {
        if (!hasOneOn && !UIController.hasOneOn)
        {
            hasOneOn = true;
            Instantiate(profilePicPickerUI, new Vector3(0, 0, 0), Quaternion.identity);
        }
        else
        {
            DisplayMessage();
        }
    }

    public void instantiateSettingsTab()
    {
        if (!hasOneOn && !UIController.hasOneOn)
        {
            hasOneOn = true;
            GameObject settingsTab = Instantiate(settingsUI, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;

            settingsTab.transform.SetParent(GameObject.Find("hudCanvas").transform);
            settingsTab.transform.localPosition = new Vector2(0, 0);
            settingsTab.transform.localScale = new Vector2(1, 1);
            //Instantiate(settingsUI, new Vector3(0, 0, 0), Quaternion.identity);
        }
        else
        {
            DisplayMessage();
        }
    }

    public void instantiateQuestsUI()
    {
        if (!hasOneOn && !UIController.hasOneOn)
        {
            hasOneOn = true;
            GameObject settingsTab = Instantiate(questsUI, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;

            settingsTab.transform.SetParent(GameObject.Find("hudCanvas").transform);
            settingsTab.transform.localPosition = new Vector2(0, 0);
            settingsTab.transform.localScale = new Vector2(1, 1);
        }
        else
        {
            DisplayMessage();
        }
    }

    public void instantiateSocialSystemUI()
    {
        if (!hasOneOn && !UIController.hasOneOn)
        {
            hasOneOn = true;
            /*GameObject settingsTab = Instantiate(socialSystemUI, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;

            settingsTab.transform.SetParent(GameObject.Find("hudCanvas").transform);
            settingsTab.transform.localPosition = new Vector2(0, 0);
            settingsTab.transform.localScale = new Vector2(1, 1);*/
            socialSystemUI.SetActive(true); //want to keep data!
            socialSystemUI.GetComponent<SocialSystemScript>().OnTabOpen();
        }
        else
        {
            DisplayMessage();
        }
    }

    public void instantiateRoomSystemUI()
    {
        if(!hasOneOn && !UIController.hasOneOn)
        {
            hasOneOn = true;
            roomSystemUI.SetActive(true);
            roomSystemUI.GetComponent<RoomSystemPanelScript>().OnTabOpen();
        }
        else
        {
            DisplayMessage();
        }
    }

    private void DisplayMessage()
    {
        Debug.Log("Please close the current tab before opening a new tab!");
    }
}
