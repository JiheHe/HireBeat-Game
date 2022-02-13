using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerDetector : MonoBehaviour
{
    public GameObject indicator;
    public GameObject speech;
    public bool inZone;
    public GameObject player;
    chibiRobotBehaviors script;
    // Start is called before the first frame update
    void Start()
    {
        script = gameObject.transform.parent.GetComponent<chibiRobotBehaviors>();
    }

    // Update is called once per frame
    void Update()
    {
        if(inZone && Input.GetKeyDown(KeyCode.E))
        {
            script.inConvo = true;
            script.turnToPlayer(player.transform.position);
            playSpeech();
            closeIndicator();
            Invoke("closeSpeech", 1);
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.transform.parent.tag == "Player")
        {
            openIndicator();
            player = collision.gameObject.transform.parent.gameObject;
        }
    }

    //multiplayer object interaction: if something enter and something else leaves then 
    //have to reenter, inconv...so check for if anything's else is in it on the exit line
    //actually just track first player that entered
    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.transform.parent.tag == "Player")
        {
            closeIndicator();
            player = null;
        }
    }

    private void playSpeech()
    {
        speech.SetActive(true);
    }

    private void closeSpeech()
    {
        speech.SetActive(false);
        script.inConvo = false;
        script.chibiBotNewLocation();
        script.decideDirection();
    }

    private void openIndicator()
    {
        indicator.SetActive(true);
        inZone = true;
    }

    private void closeIndicator()
    {
        indicator.SetActive(false);
        inZone = false;
    }
}
