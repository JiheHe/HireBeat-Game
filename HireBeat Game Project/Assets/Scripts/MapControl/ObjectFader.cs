using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectFader : MonoBehaviour
{
    public Animator faderObject;
    public GameObject text;
    public InGameUIController UIController;
    public int zoneNumber;


    // Start is called before the first frame update
    /*public void Update()
    {
        
    }*/

    public void OnTriggerEnter2D(Collider2D collision)
    {
        UIController.enabled = true;
        UIController.zoneNumber = zoneNumber;
        text.SetActive(true);
        resetAnimation();
        faderObject.enabled = true;
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        UIController.enabled = false;
        text.SetActive(false);
        resetAnimation();
        faderObject.enabled = false;
    }

    private void resetAnimation()
    {
        faderObject.Rebind();
        faderObject.Update(0f);
    }
}
