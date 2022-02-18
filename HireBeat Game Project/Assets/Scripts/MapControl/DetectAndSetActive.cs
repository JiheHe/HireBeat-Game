using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectAndSetActive : MonoBehaviour
{
    public GameObject arrow;

    public void OnTriggerEnter2D(Collider2D collision)
    {
        arrow.SetActive(true);
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        arrow.SetActive(false);
    }
}
