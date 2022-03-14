using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateInfoCardOnClick : MonoBehaviour
{
    public string userID;

    public void GenerateInfoCard()
    {
        var socialSystem = GameObject.FindGameObjectWithTag("PlayerHUD").transform.Find("SocialSystem").GetComponent<SocialSystemScript>();
        if (socialSystem.currentInfoCardOpened == null) //object self destructs into null on tab close
        {
            GameObject info = Instantiate(socialSystem.playerInfoCard, new Vector2(0, 0), Quaternion.identity); //can always use this to tune generation position/size
            info.transform.GetChild(0).transform.localPosition = new Vector2(-243, 0); //shift x to the left?, of this generated card
            socialSystem.currentInfoCardOpened = info;
        }
        socialSystem.currentInfoCardOpened.GetComponent<PlayerInfoCardUpdater>().InitializeInfoCard(userID, 0);
    }
}
