using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

public class PersistentData : MonoBehaviour
{
    //Can't use singleton with Pun.. fuc
    //public static PersistentData PD; //another singleton

    PlayFabController PFC;

    //these are set in Avatar customization
    public string charName;
    public string skinColor;
    public string hasHair;
    public string hairStyle;
    public string hairColor;
    public string hasTopWear;
    public string topWearStyle;
    public string topWearColor;
    public string hasBotWear;
    public string botWearStyle;
    public string botWearColor;
    public string hasShoes;
    public string shoesColor;

    //these are profile huds
    public string pfpImage;
    public string acctName;
    public string acctSignature;
    public string acctID;

    //this is set in Background Pref
    public string skyIndex;

    //DON'T FORGET TO CHANGE THE ARRAY LENGTH + NEW ELEM IN THE EDITOR AS WELL! ELSE THE FUKCING BUG IS STUPID AF
    public string[] charProperties = new string[] {
        "charName", "skinColor", "hasHair", "hairStyle", "hairColor", "hasTopWear", 
        "topWearStyle", "topWearColor", "hasBotWear", "botWearStyle", "botWearColor",
        "hasShoes", "shoesColor", "pfpImage", "acctName", "acctSignature", "acctID", "skyIndex"}; //var names 

    public static bool usingMicrophone = false; //3 instances, video chat, public voice chat, private room voice chat
    public static bool isMovementRestricted = false; //for example, sitting on a chair

    //This value defaults to null, but upon switching rooms validated it changes.
    public static string TRUEOWNERID_OF_CURRENT_ROOM = null;
    //This value is set by PlayerRoomDisplayTab's join button, a placeholder for future connection. Or by PFC at start
    public static string TRUEOWNERID_OF_JOINING_ROOM = null;
    public static string NAME_OF_JOINING_ROOM = null;

    public static List<string> listOfInvitedRoomIds = new List<string>(); //this is for room system, need to keep invites between rooms
    public static List<string> invitedRoomList = new List<string>(); //this is for video chat room system
    public static Dictionary<string, string> commonRoomNamesAndRelatedSceneName =
        new Dictionary<string, string> { //change the name "MainScene" to different scenes in the future!
            { "Common Room: Technology", "MainScene" },
            { "Common Room: Finance", "MainScene" },
            { "Common Room: Dummy", "MainScene" }
            };

    private void OnEnable() //making sure only 1 playfab controller
    {
        /*if (PersistentData.PD == null)
        {
            PersistentData.PD = this;
        }
        else
        {
            if (PersistentData.PD != this)
            {
                Destroy(this.gameObject);
            }
        }*/
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        PFC = GameObject.Find("PlayFabController").GetComponent<PlayFabController>();

        //Testing
        listOfInvitedRoomIds = new List<string> { "Common Room: Technology", "Common Room: Dummy", "B", "f", "Z", "i", "falkdfdjfsf;" };
        //listOfInvitedRoomIds = new List<string> { "Common Room: Finance", "COMING" };
    }

    public static bool strToBool(string str)
    {
        if (int.Parse(str) == 1) return true;
        else return false; //1 = true, 0 = false
    }

    public static string boolToStr(bool val)
    {
        if (val) return "1";
        else return "0";
    }

    public static int strToInt(string str)
    {
        if(int.TryParse(str, out int numVal))
        {
            return numVal;
        }
        Debug.LogError("Error in converting from string to int");
        return -999;
    }

    public static string intToStr(int num)
    {
        return num.ToString();
    }

    public void RetrieveUserData()
    {
        PFC.GetPlayerData();
    }

}
