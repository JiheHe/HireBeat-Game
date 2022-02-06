using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlphaReducer : MonoBehaviour
{
    public GameObject wall;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        changeAlpha(0.4f);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        changeAlpha(1.0f);
    }

    private void changeAlpha(float val)
    {
        foreach (SpriteRenderer child in wall.GetComponentsInChildren<SpriteRenderer>())
        {
            Color tmp = child.color;
            tmp.a = val;
            child.color = tmp;
        }
    }
}
