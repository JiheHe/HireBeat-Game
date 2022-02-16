using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class dropDownOpenScript : MonoBehaviour
{
    public GameObject self;
    public GameObject controller;
    GameObject eventController;
    public ScrollRect myScrollRect;

    //smart way: https://gamedev.stackexchange.com/questions/179585/how-can-i-economically-check-if-a-ui-dropdown-is-open-or-closed

    // Start is called before the first frame update
    void Start()
    {
        //unity creates an obj called DL on creation
        //so when template is enabled, this is called and that list is grabbed
        if (self.name == "Dropdown List")
        {
            //controller decides the state of dropdown
            titleSelectorScript titleSelector = controller.GetComponent<titleSelectorScript>();
            if (titleSelector.titleIndex == 0 || titleSelector.titleIndex == 1) myScrollRect.verticalNormalizedPosition = 1;
            else myScrollRect.verticalNormalizedPosition = 1 - (float)(titleSelector.titleIndex + 1) / titleSelector.currentDropDownLength;

            eventController = GameObject.FindGameObjectWithTag("PlayerCamera");
            eventController.GetComponent<EventSystem>().sendNavigationEvents = false; //avoids wasd in scroll
        }
    }
    private void OnDestroy()
    {
        if (self.name == "Dropdown List")
        {
            //controller.GetComponent<titleSelectorScript>().dropDownOpen = false;
            eventController.GetComponent<cameraController>().enabled = true;
            eventController.GetComponent<EventSystem>().sendNavigationEvents = true;
        }
    }
}
