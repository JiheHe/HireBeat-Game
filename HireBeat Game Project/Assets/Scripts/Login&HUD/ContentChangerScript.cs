using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContentChangerScript : MonoBehaviour
{

    public GameObject originalDisplay;
    public GameObject editorDisplay;
    public Text textSpot;
    public InputField newInput;
    public Text UITextTarget;

    public bool canLeftEmpty;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnEditorPressed() //enter edit mode
    {
        if(newInput != null) newInput.text = textSpot.text; //user can change!
        originalDisplay.SetActive(false);
        editorDisplay.SetActive(true);
        
    }

    public void OnConfirmationPressed() //exit edit mode
    {
        if(!canLeftEmpty && newInput.text.Replace(" ", "").Length == 0) 
        {
            Debug.Log("Such textfield cannot be empty!");
        } 
        else
        {
            textSpot.text = newInput.text;
            if(UITextTarget != null) UITextTarget.text = newInput.text;
            originalDisplay.SetActive(true);
            editorDisplay.SetActive(false);
        }
        
    }

    public void OnCancelButtonPressed()
    {
        originalDisplay.SetActive(true);
        editorDisplay.SetActive(false);
    }
}
