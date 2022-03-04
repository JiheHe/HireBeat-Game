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

    //if senderID = "0000" special code, then it's a private msg. Else public
    public string sendersID;

    public void UpdateMsgContent(string name, string time, string msg, bool isSelf, string senderID)
    {
        if (senderID == "0000") transform.GetChild(0).GetComponent<Button>().interactable = false; //special code! private msg
        else sendersID = senderID;
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

    //no multiple check rn, prob can use msg content as a controller. Try static info card count?
    public void GenerateInfoCard()
    {
        GameObject infoCard = GameObject.FindGameObjectWithTag("PlayerHUD").transform.Find("SocialSystem").GetComponent<SocialSystemScript>().playerInfoCard;

        GameObject info = Instantiate(infoCard, new Vector2(0, 0), Quaternion.identity); //need to make new instance! else gonna overwrite prefab
        info.GetComponent<PlayerInfoCardUpdater>().InitializeInfoCard(sendersID, 0);
    }

}
