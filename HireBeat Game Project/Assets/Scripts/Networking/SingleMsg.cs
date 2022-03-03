using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SingleMsg : MonoBehaviour
{
    // Start is called before the first frame update

    public Text senderName;
    public Text sendTime;
    public Text msgBody;
    public Image pfp;

    public void UpdateMsgContent(string name, string time, string msg, bool isSelf)
    {
        senderName.text = name;
        sendTime.text = time;
        msgBody.text = msg;
        if (isSelf) //default is white
        {
            Color32 newColor = new Color32(230, 164, 87, 255);
            pfp.color = newColor;
            senderName.color = newColor;
        }
    }

}
