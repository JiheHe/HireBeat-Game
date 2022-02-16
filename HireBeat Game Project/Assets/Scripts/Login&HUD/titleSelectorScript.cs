using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class titleSelectorScript : MonoBehaviour
{
    GameObject eventController;
    GameObject playerTitleController;
    //public bool dropDownOpen;
    public int titleIndex;
    public int currentDropDownLength;

    public Dropdown dropdown;
    public List<string> rankTitles = new List<string>
    {   "Novice", //0
        "Experienced Rookie", //1
        "Advanced Beginner", //2
        "Proficient Interviewee", //3
        "Veteran Of Knowledge", //4
        "Speaking Expert", //5
        "Master Of Interviews", //6
        "The Visionary", //7
        "Portrait Photographer", //8
        "The Lucky Color", //9
        "Robotician", //10
        "Social Star", //11
        "Explorer", //12
        "TruthSeeker", //13
        "Consistent Powerhouse", //14
        "Model Student", //15
        "Who Am I?", //16
        "The Mightier Pen", //17
        "Peak Of Perfection", //18
        "The Void", //19
        "Ultrasonic", //20
        "Lost In Time", //21
        "Hidden Mystery", //22
        "City's Nightmare" //23
    };
    bool[] titles;

    // Start is called before the first frame update
    void Start()
    {
        titleIndex = 0;
        titles = new bool[rankTitles.Count]; //curr only 24 titles
        eventController = GameObject.FindGameObjectWithTag("PlayerCamera");
        playerTitleController = GameObject.FindGameObjectWithTag("Player").transform.Find("PlayerTitles").gameObject;
        //Clear the old options of the Dropdown menu
        dropdown.ClearOptions();
        AddDropDownOptions(0); //allows novice
        currentDropDownLength = 1;

        //Add the options created in the List above
        //dropdown.AddOptions(rankTitles);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            for (int i = 1; i < rankTitles.Count; i++)
            {
                AddDropDownOptions(i);
            }
            Debug.Log("Dropdown Accesses unlocked");
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            AddDropDownOptions(Random.Range(1, 24));// currentDropDownLength);
            Debug.Log("Dropdown Accesses unlocked");
        }
    }

    //no need for ondropdownopen/close rn due to their interactions, the power of canvas masking 
    /*void OnDropDownOpen()
    {
        eventController.GetComponent<cameraController>().enabled = false;
        eventController.GetComponent<InGameUIController>().hasOneOn = true;
        eventController.GetComponent<PlayerMenuUIController>().hasOneOn = true;
    }

    void OnDropDownClosed()
    {
        eventController.GetComponent<cameraController>().enabled = true;
        eventController.GetComponent<InGameUIController>().hasOneOn = false;
        eventController.GetComponent<PlayerMenuUIController>().hasOneOn = false;
    }*/

    public void onPointerIn()
    {
        eventController.GetComponent<cameraController>().enabled = false;
        
    }

    public void onPointerOut()
    {
        eventController.GetComponent<cameraController>().enabled = true;
        
    }

    public void OnDropDownChanged(Dropdown dropDown)
    {
        //The Value is the index number of the current selection in the Dropdown. 0 is the first option in the Dropdown, 1 is the second, and so on.
        titleIndex = rankTitles.IndexOf(dropDown.options[dropDown.value].text);
        playerTitleController.GetComponent<TitleController>().changeTitle(titleIndex);
    }

    public void AddDropDownOptions(int index) //unlocking new titles //make sure everything can only be added once 
    {
        if(!titles[index]) //making sure unlocking for first time
        {
            List<string> newTitle = new List<string> { rankTitles[index] };
            dropdown.AddOptions(newTitle);
            titles[index] = true; 
            currentDropDownLength += 1;
        }
    }

    public void turnOffTitle()
    {
        playerTitleController.SetActive(false);
    }

    public void turnOnTitle()
    {
        playerTitleController.SetActive(true);
    }
}
