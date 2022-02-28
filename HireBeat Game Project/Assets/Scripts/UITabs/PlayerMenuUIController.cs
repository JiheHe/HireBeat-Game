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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void instantiateProfilePicPicker()
    {
        if (!hasOneOn)
        {
            hasOneOn = true;
            Instantiate(profilePicPickerUI, new Vector3(0, 0, 0), Quaternion.identity);
        }
    }

    public void instantiateSettingsTab()
    {
        if (!hasOneOn)
        {
            hasOneOn = true;
            GameObject settingsTab = Instantiate(settingsUI, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;

            settingsTab.transform.SetParent(GameObject.Find("hudCanvas").transform);
            settingsTab.transform.localPosition = new Vector2(0, 0);
            settingsTab.transform.localScale = new Vector2(1, 1);
            //Instantiate(settingsUI, new Vector3(0, 0, 0), Quaternion.identity);
        }
    }

    public void instantiateQuestsUI()
    {
        if (!hasOneOn)
        {
            hasOneOn = true;
            GameObject settingsTab = Instantiate(questsUI, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;

            settingsTab.transform.SetParent(GameObject.Find("hudCanvas").transform);
            settingsTab.transform.localPosition = new Vector2(0, 0);
            settingsTab.transform.localScale = new Vector2(1, 1);
        }
    }

    public void instantiateSocialSystemUI()
    {
        if (!hasOneOn)
        {
            hasOneOn = true;
            /*GameObject settingsTab = Instantiate(socialSystemUI, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;

            settingsTab.transform.SetParent(GameObject.Find("hudCanvas").transform);
            settingsTab.transform.localPosition = new Vector2(0, 0);
            settingsTab.transform.localScale = new Vector2(1, 1);*/
            socialSystemUI.SetActive(true); //want to keep data!
            socialSystemUI.GetComponent<SocialSystemScript>().OnTabOpen();
        }
    }
}
