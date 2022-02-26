using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using System;
using UnityEngine.UI;

public class SpawnPlayers : MonoBehaviour
{

    public GameObject player;
    public float minX, minY, maxX, maxY;

    public GameObject manager;
    public GameObject background; //also unique to individual

    public Texture2D myTexture;

    GameObject newPlayer;

    //THIS IS TO INDIVIDUAL AS WELL!
    //probably is?


    // Start is called before the first frame update
    void Start()
    {
        object[] instanceData = InitializePlayerInformation();

        Vector2 randomPosition = new Vector2(UnityEngine.Random.Range(minX, maxX), UnityEngine.Random.Range(minY, maxY));
        newPlayer = PhotonNetwork.Instantiate(player.name, randomPosition, Quaternion.identity, 0, instanceData);
        Debug.Log("A new player has joined!");

        InstantiateLocalData();

        //assign the camera to the current, local manager
        manager.GetComponent<cameraController>().zoomCamera = newPlayer.transform.Find("PlayerCamera").GetComponent<Camera>();
        
        //assign background's camera to point towards the new player's
        background.GetComponent<Canvas>().worldCamera = newPlayer.transform.Find("PlayerCamera").GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //use this to spawn local data! personal instantiation is dealt with in PlayerDataUpdater.
    void InstantiateLocalData()
    {
        var p = GameObject.Find("PersistentData").GetComponent<PersistentData>(); //shorthand

        byte[] pfpByteArr = Convert.FromBase64String(p.pfpImage);
        Image newSprite = GameObject.FindGameObjectWithTag("PlayerHUD").transform.Find("profilePicture").transform.Find("placeholderImage").gameObject.GetComponent<Image>();
        myTexture = new Texture2D(newSprite.sprite.texture.width, newSprite.sprite.texture.height, TextureFormat.RGB24, false, true); //or use constants
        myTexture.LoadImage(pfpByteArr);
        Sprite spriteImg = Sprite.Create(myTexture, new Rect(0, 0, myTexture.width, myTexture.height), new Vector2(0.5f, 0.5f));
        newSprite.sprite = spriteImg;
    }

    object[] InitializePlayerInformation()
    {
        var p = GameObject.Find("PersistentData").GetComponent<PersistentData>();

        object[] instanceData = new object[14];
        //0 - 12 are for char appearence
        //13 - ? are for profile

        instanceData[0] = p.charName;
        instanceData[1] = p.hasHair;
        instanceData[2] = p.hasTopWear;
        instanceData[3] = p.hasBotWear;
        instanceData[4] = p.hasShoes;
        instanceData[5] = p.skinColor;
        instanceData[6] = p.hairStyle;
        instanceData[7] = p.hairColor;
        instanceData[8] = p.topWearStyle;
        instanceData[9] = p.topWearColor;
        instanceData[10] = p.botWearStyle;
        instanceData[11] = p.botWearColor;
        instanceData[12] = p.shoesColor;
        instanceData[13] = DivideStringIntoSub(p.pfpImage, 10000);

        return instanceData;
    }

    public static string[] DivideStringIntoSub(string str, int maxLenPerSubstr) //return an array of substrings, do 10000 for img
    {
        int strLen = str.Length;
        int numSubstringNeeded = (int)Math.Ceiling((float)strLen / maxLenPerSubstr); //for example, 5 / 2 = 2.5 => 3 arrays. 3 / 5 = = 0.6 => 1 array.
        string[] arraySubstrings = new string[numSubstringNeeded];

        int currStartIndex = 0;
        for(int i = 0; i < numSubstringNeeded; i++)
        {
            string currSubstring;
            if(i == numSubstringNeeded - 1) currSubstring = str.Substring(currStartIndex);
            else currSubstring = str.Substring(currStartIndex, maxLenPerSubstr); //syntax is start index, length... WTFFFFFFF

            arraySubstrings[i] = currSubstring;
            currStartIndex = currStartIndex + maxLenPerSubstr;
        }
        return arraySubstrings;
    }

    public static string ConnectArrayOfSubstrings(string[] strArr) //strArr shouldn't be empty anyway
    {
        string total = strArr[0]; //first row's array's string content
        for (int i = 1; i < strArr.Length; i++)
        {
            total += strArr[i];
        }
        return total;
    }
}
