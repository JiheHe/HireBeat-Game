using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverTextDisplayScript : MonoBehaviour
{
    public GameObject displayedText;

    // Start is called before the first frame update
    void Start()
    {
        displayedText.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnMouseOver() //need pointer event now... these only works on old GUI
    {
        displayedText.SetActive(true);
    }

    public void OnMouseExit()
    {
        displayedText.SetActive(false);
    }
}
