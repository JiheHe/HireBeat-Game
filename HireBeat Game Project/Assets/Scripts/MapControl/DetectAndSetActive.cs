using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectAndSetActive : MonoBehaviour
{
    public GameObject arrow;

    public string soundOnEnter;
    public string soundOnExit;

    public int arrowNum; //0 is living, 1 is achiv

    public void OnTriggerEnter2D(Collider2D collision)
    {
        FindObjectOfType<AudioManager>().Play(soundOnEnter);
        //Collider2D[] globalPositionOfContact = new Collider2D[1]; 
        //collision.GetContacts(globalPositionOfContact);
        //Debug.Log(globalPositionOfContact[0].transform.position.x - transform.position.x);
        if (arrowNum == 0 && collision.transform.position.x - transform.position.x > 0) arrow.SetActive(true);
        else if (arrowNum == 1 && collision.transform.position.x - transform.position.x < 0) arrow.SetActive(true);
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        FindObjectOfType<AudioManager>().Play(soundOnExit);
        if (arrow != null) arrow.SetActive(false);
    }
}
