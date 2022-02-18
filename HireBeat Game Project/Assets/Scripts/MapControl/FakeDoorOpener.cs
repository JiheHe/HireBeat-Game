using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakeDoorOpener : MonoBehaviour
{

    public GameObject door;
    public string soundOnEnter;
    public string soundOnExit;

    public void OnTriggerEnter2D(Collider2D collision) 
    {
        FindObjectOfType<AudioManager>().Play(soundOnEnter);
        door.SetActive(true);
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        FindObjectOfType<AudioManager>().Play(soundOnExit);
        door.SetActive(false);
    }
}
