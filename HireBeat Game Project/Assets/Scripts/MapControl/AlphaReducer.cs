using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlphaReducer : MonoBehaviour
{
    public GameObject wall;
    public WallZonesController controller;
    public GameObject relatedZone; //this is for 2 zones that share a same wall, like door frame ones!

    public void Start()
    {
        controller = FindObjectOfType<WallZonesController>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        controller.OnZoneEntry(gameObject.name);
        if(relatedZone != null) controller.OnZoneEntry(relatedZone.name); //treat them as one
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        controller.OnZoneExit(gameObject.name);
        if (relatedZone != null) controller.OnZoneExit(relatedZone.name);
    }

    public void changeAlpha(float val)
    {
        foreach (SpriteRenderer child in wall.GetComponentsInChildren<SpriteRenderer>())
        {
            Color tmp = child.color;
            tmp.a = val;
            child.color = tmp;
        }
    }
}
