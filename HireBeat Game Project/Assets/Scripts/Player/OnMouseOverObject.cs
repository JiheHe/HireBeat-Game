using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnMouseOverObject : MonoBehaviour
{
    public GameObject hoverIndicator;
    public GameObject pfpDisplay;
    // Start is called before the first frame update
    void OnMouseOver()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Debug.Log("Opening Tab");
        }
    }

    private void OnMouseEnter()
    {
        hoverIndicator.SetActive(true);
        pfpDisplay.SetActive(true);
    }

    void OnMouseExit()
    {
        hoverIndicator.SetActive(false);
        pfpDisplay.SetActive(false);
    }
}
