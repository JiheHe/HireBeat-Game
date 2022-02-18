using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class changeReceiver : MonoBehaviour
{
    public GameObject volumeControl;
    public GameObject hudText;
    public GameObject profilePicture;

    PlayerMenuUIController pui;

    // Start is called before the first frame update
    void Start()
    {
        pui = GameObject.FindGameObjectWithTag("PlayerCamera").GetComponent<PlayerMenuUIController>();
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
