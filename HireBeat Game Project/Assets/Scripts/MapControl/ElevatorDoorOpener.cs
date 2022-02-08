using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorDoorOpener : MonoBehaviour
{

    public GameObject doorClosed;
    public GameObject doorOpen;

    public void OnTriggerEnter2D(Collider2D collision) 
    {
        doorOpen.SetActive(true);
        doorClosed.SetActive(false);
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        doorOpen.SetActive(false);
        doorClosed.SetActive(true);
    }
}
