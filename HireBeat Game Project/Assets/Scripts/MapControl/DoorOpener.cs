using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorOpener : MonoBehaviour
{

    public Transform door;
    public Vector2 originalPosition;
    public Vector2 goalPosition;
    public float time;

    public string soundOnEnter;
    public string soundOnExit;

    IEnumerator OnTriggerEnter2D(Collider2D collision) //stay...
    {
        FindObjectOfType<AudioManager>().Play(soundOnEnter);
        yield return StartCoroutine(MoveObject(originalPosition, goalPosition, time));
    }

    IEnumerator OnTriggerExit2D(Collider2D collision)
    {
        FindObjectOfType<AudioManager>().Play(soundOnExit);
        yield return StartCoroutine(MoveObject(goalPosition, originalPosition, time));
    }

    IEnumerator MoveObject(Vector2 startPos, Vector2 endPos, float time)
    {
        //yield return new WaitForSeconds(5f);// first yield return after five seconds as you want
        var i = 0.0f;
        var rate = 1.0f / time;
        while (i < 1.0f)
        {
            i += Time.deltaTime * rate;
            door.position = Vector2.Lerp(startPos, endPos, i);
            yield return null;
        }
    }
}
