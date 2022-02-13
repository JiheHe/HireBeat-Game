using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainAreaModeSwitch : MonoBehaviour
{
    public GameObject mainOnColliders;
    public GameObject mainOffColliders;
    public GameObject mainOnVisuals;
    public GameObject mainOffVisuals;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (gameObject.tag == "Entrance")
        {
            mainOnColliders.SetActive(true);
            mainOffColliders.SetActive(false);
            mainOnVisuals.SetActive(true);
            mainOffVisuals.SetActive(false);
            
        }
        else if (gameObject.tag == "Exit")
        {
            mainOnColliders.SetActive(false);
            mainOffColliders.SetActive(true);
            mainOnVisuals.SetActive(false);
            mainOffVisuals.SetActive(true);
        }
    }
}
