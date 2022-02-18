using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorDoorOpener : MonoBehaviour
{

    public GameObject doorClosed;
    public GameObject doorOpen;

    public string soundOnEnter;
    public string soundOnExit;

    public void OnTriggerEnter2D(Collider2D collision) 
    {
        FindObjectOfType<AudioManager>().Play(soundOnEnter);
        doorOpen.SetActive(true);
        doorClosed.SetActive(false);
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        FindObjectOfType<AudioManager>().Play(soundOnExit);
        doorOpen.SetActive(false);
        doorClosed.SetActive(true);
    }
}
