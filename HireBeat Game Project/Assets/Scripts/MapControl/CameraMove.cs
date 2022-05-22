using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public float speed;
    private float theSpeed = 0;
    //public int hourNow; //this can be used for testing below

    public GameObject backgroundMorning;
    public GameObject backgroundEvening;
    public GameObject backgroundNight;
    public UnityEngine.UI.Text titleText;

    private void Awake()
    {
        int hourNow = System.DateTime.Now.Hour; //0-24 hr. Morning: 6 - 16. Night: 20 - 6. Evening: 16 - 20
        Debug.Log("The current hour on your system clock is: " + hourNow);

        if(hourNow >= 6 && hourNow < 16) //morning
        {
            Debug.Log("Good morning!");
            titleText.text = "Good morning! Lovely birds R chirping!";
            backgroundMorning.SetActive(true);
        } 
        else if(hourNow >= 16 && hourNow < 20) //evening
        {
            Debug.Log("Good evening!");
            titleText.text = "Good evening! Have you had dinner yet?";
            backgroundEvening.SetActive(true);
        }
        else //hourNow >= 20 || hourNow < 6 //night
        {
            Debug.Log("Good night!");
            titleText.text = "Stars R bright tonight! How's your day?";
            backgroundNight.SetActive(true);
        }

        theSpeed = speed;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.right * theSpeed * Time.deltaTime);
    }
}
