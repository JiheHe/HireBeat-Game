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

    public string[] charProperties = {"charName", "skinColor", "hasHair", "hairStyle", "hairColor", "hasTopWear", "topWearStyle", "topWearColor", "hasBotWear",
        "botWearStyle", "botWearColor", "hasShoes", "shoesColor", "pfpImage" }; //var names

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


    }

    // Update is called once per frame
    void Update()
    {
        
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

    public void RetrieveUserData()
    {
        PFC.GetPlayerData();
    }

}
