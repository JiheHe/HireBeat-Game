using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class changeReceiver : MonoBehaviour
{
    public GameObject volumeControl;
    public GameObject hudText;
    public GameObject profilePicture;

    //gonna put in some direct reference to local variables here, so very easy for initialization to set them up
    public Text accountNameInEditor;
    public Text accountSignatureInEditor;
    public Image hudProfilePicture;
    public Text hudAccountName;
    public Text uniqueIDinEditor;
    public Transform friendsList;
    public Transform requesterList;
    public Transform requesteeList;

    PlayerMenuUIController pui;

    //below are special scripts that need initialization
    public VoiceChatController vcc;

    // Start is called before the first frame update
    void Start()
    {
        pui = GameObject.FindGameObjectWithTag("PlayerCamera").GetComponent<PlayerMenuUIController>();

        vcc.InitializationSteps();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void changeProfilePicture(Sprite sprite)
    {
        profilePicture.transform.Find("placeholderImage").GetComponent<Image>().sprite = sprite;
    }

    public void startProfileChanger()
    {
        pui.instantiateProfilePicPicker();
    }

    public void startSettingsTab()
    {
        pui.instantiateSettingsTab();
    }

    public void startQuestsTab()
    {
        pui.instantiateQuestsUI();
    }

    public void startSocialSystemTab()
    {
        pui.instantiateSocialSystemUI();
    }
}
