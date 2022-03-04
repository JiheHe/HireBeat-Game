using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MsgContentController : MonoBehaviour
{
    public GameObject message; //the prefab

    public string tempName;

    int MAX_MSG = 50; //hmmm
    public FriendsListing listing;
    List<GameObject> messages = new List<GameObject>();

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddMessage(string name, string time, string msg, bool isSelf, string senderID)
    {
        var newMsg = Instantiate(message, gameObject.transform);
        newMsg.GetComponent<SingleMsg>().UpdateMsgContent(name, time, msg, isSelf, senderID);
        newMsg.transform.parent = gameObject.transform; //is this necessary?

        messages.Add(newMsg); //The object to be added to the end of the List<T>
        if(messages.Count > MAX_MSG) //then destroy the earliest message
        {
            Destroy(messages[0]);
        }
    }
}
