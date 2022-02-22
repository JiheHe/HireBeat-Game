using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class ContentChangerScript : MonoBehaviour
{

    public GameObject originalDisplay;
    public GameObject editorDisplay;
    public Text textSpot;
    public InputField newInput;
    public Text UITextTarget;

    public bool canLeftEmpty; //rn username cannot be left empty, and signature can. Use that to distinguish

    public GameObject playerObj;
    OnMouseOverObject playerDataDisplay;

    // Start is called before the first frame update
    void Start()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (player.GetComponent<PhotonView>().IsMine) //can also use GetComponent<playerController>().view.IsMine
            {
                playerObj = player;
                break;
            }
        }
        playerDataDisplay = playerObj.transform.Find("PlayerMouseDetector").GetComponent<OnMouseOverObject>();
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
            if (!canLeftEmpty) playerDataDisplay.UpdateUsername(newInput.text); //rn username cannot be left empty, and signature can. Use that to distinguish
            else playerDataDisplay.UpdateSignature(newInput.text);
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
