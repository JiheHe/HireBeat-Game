using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameUIController : MonoBehaviour
{
    public bool hasOneOn;
    public GameObject characterCustomizationUI;
    public GameObject backgroundChangerUI;
    public GameObject voiceChatPanelUI;
    public int zoneNumber;

    public PlayerMenuUIController UIController;

    // Start is called before the first frame update
    public void Start()
    {
        UIController = GetComponent<PlayerMenuUIController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!UIController.hasOneOn)
        {
            switch (zoneNumber)
            {
                case 1: //Livingroom computer top left
                    if (Input.GetKeyDown(KeyCode.Q))
                    {
                        instantiateCharacterCustomization();
                    }
                    else if (Input.GetKeyDown(KeyCode.E))
                    {
                        instantiateBackgroundChanger();
                    }
                    break;
                case 2: //Voice chat panel activator
                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        ActivateGlobalVoiceChatPanel();
                    }
                    break;
            }
        }
    }

    public void instantiateCharacterCustomization()
    {
        if(!hasOneOn && !UIController.hasOneOn)
        {
            hasOneOn = true;
            Instantiate(characterCustomizationUI, new Vector3(0, 0, 0), Quaternion.identity);
        }
        else
        {
            DisplayMessage();
        }
    }

    public void instantiateBackgroundChanger()
    {
        if (!hasOneOn && !UIController.hasOneOn)
        {
            hasOneOn = true;
            Instantiate(backgroundChangerUI, new Vector3(0, 0, 0), Quaternion.identity);
        }
        else
        {
            DisplayMessage();
        }
    }

    public void ActivateGlobalVoiceChatPanel()
    {
        if(!hasOneOn && !UIController.hasOneOn)
        {
            hasOneOn = true;
            voiceChatPanelUI.SetActive(true);
            if (voiceChatPanelUI.GetComponent<VoiceChatController>().gameUIController == null)
                voiceChatPanelUI.GetComponent<VoiceChatController>().gameUIController = this;
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
