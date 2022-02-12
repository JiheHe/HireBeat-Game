using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class npcCollisionStop : MonoBehaviour
{
    // Start is called before the first frame update

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "NPC" && collision.gameObject.layer == 6)
        {
            Vector2 currPos = collision.gameObject.transform.parent.transform.position;
            collision.gameObject.transform.parent.GetComponent<chibiRobotBehaviors>().targetArea = new Vector2(
                Mathf.Round(currPos.x * 100f) / 100f, Mathf.Round(currPos.y * 100f) / 100f);
            //"stopping the object" so it restarts, deci pts to avoid overprecision
        }
    }
}
