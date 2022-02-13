using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMenuUIController : MonoBehaviour
{
    public bool hasOneOn;
    public GameObject profilePicPickerUI; //I might need a separate UI script for this; zone disable won't work...
    //this one will always be active with buttons, no zones
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
}
