using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakeDoorOpener : MonoBehaviour
{

    public GameObject door;

    public void OnTriggerEnter2D(Collider2D collision) 
    {
        door.SetActive(true);
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        door.SetActive(false);
    }
}
