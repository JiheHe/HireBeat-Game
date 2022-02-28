using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; //need this line!!
using Photon.Pun;

public class AvatarCustomizationScript : MonoBehaviour
{
    public string currSkin;
    public bool hasHair;
    public string currHairStyle;
    public string currHairColor;
    public bool hasTopWear;
    public string currTopWearStyle;
    public string currTopWearColor;
    public bool hasBotWear;
    public string currBotWearStyle;
    public string currBotWearColor;
    public bool hasShoes;
    public string currShoesColor;

    public int skinLayerIndex;
    public int hairLayerIndex;
    public int topWearLayerIndex;
    public int botWearLayerIndex;
    public int shoesLayerIndex;

    public string[] skinToneColors;
    public string[] hairStyleNames;
    public string[] hairColors;
    public string[] clothTopNames;
    public string[] clothTopColors;
    public string[] clothBotNames;
    public string[] clothBotColors;
    public string[] shoesColors;

    public string charName;
    public InputField field;

    public GameObject playerObj;
    public cameraController playerCamera;

    public InGameUIController UIController;

    PlayFabController PFC;

    // Start is called before the first frame update
    void Start()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (player.GetComponent<PhotonView>().IsMine) //can also use GetComponent<playerController>().view.IsMine
            {
                playerObj = player;
                break;
            }
        }

        PFC = GameObject.Find("PlayFabController").GetComponent<PlayFabController>();

        //playerObj = GameObject.FindGameObjectWithTag("Player");
        playerObj.SetActive(false);
        playerCamera = GameObject.FindGameObjectWithTag("PlayerCamera").GetComponent<cameraController>();
        playerCamera.turnOnUICamera();
        UIController = GameObject.FindGameObjectWithTag("PlayerCamera").GetComponent<InGameUIController>();
        hasNoHair(); //false...
        hasNoTopWear();
        hasNoBotWear();
        hasNoShoes();
    }

    // Update is called once per frame
    void Update()
    {
        Sprite newSkinColor = Resources.Load("Animations/ThatCoolSprite/SkinTones/" + currSkin + "/Skin" + currSkin) as Sprite;
        gameObject.transform.GetChild(skinLayerIndex).GetComponent<Image>().sprite = newSkinColor;
        
        if(hasHair)
        {
            Sprite newHair = Resources.Load("Animations/ThatCoolSprite/HairStyles/" + currHairStyle
            + "/" + currHairColor + "/" + currHairStyle + currHairColor + "_0") as Sprite;
            gameObject.transform.GetChild(hairLayerIndex).GetComponent<Image>().sprite = newHair;
        }

        if(hasTopWear)
        {
            Sprite newTopWear = Resources.Load("Animations/ThatCoolSprite/Clothes/" + currTopWearStyle
            + "/" + currTopWearColor + "/" + currTopWearStyle + currTopWearColor + "_0") as Sprite;
            gameObject.transform.GetChild(topWearLayerIndex).GetComponent<Image>().sprite = newTopWear;
        }

        if (hasBotWear)
        {
            Sprite newBotWear = Resources.Load("Animations/ThatCoolSprite/Clothes/" + currBotWearStyle
            + "/" + currBotWearColor + "/" + currBotWearStyle + currBotWearColor + "_0") as Sprite;
            gameObject.transform.GetChild(botWearLayerIndex).GetComponent<Image>().sprite = newBotWear;
        }

        if (hasShoes)
        {
            Sprite newShoes = Resources.Load("Animations/ThatCoolSprite/Clothes/Shoes/" + currShoesColor 
            + "/Shoes" + currShoesColor + "_0") as Sprite;
            gameObject.transform.GetChild(shoesLayerIndex).GetComponent<Image>().sprite = newShoes;
        }

    }

    public void setCurrSkin(string updateSkin)
    {
        currSkin = updateSkin;
    }

    public void hasNoHair()
    {
        hasHair = false;
        gameObject.transform.GetChild(hairLayerIndex).gameObject.SetActive(false);
    }

    public void setCurrHairStyle(string updateHairStyle)
    {
        hasHair = true;
        gameObject.transform.GetChild(hairLayerIndex).gameObject.SetActive(true) ;
        currHairStyle = updateHairStyle;
    }

    public void setCurrHairColor(string updateHairColor)
    {
        currHairColor = updateHairColor;
    }

    public void hasNoTopWear()
    {
        hasTopWear = false;
        gameObject.transform.GetChild(topWearLayerIndex).gameObject.SetActive(false);
    }

    public void setCurrTopWearStyle(string updateTopWearStyle)
    {
        hasTopWear = true;
        gameObject.transform.GetChild(topWearLayerIndex).gameObject.SetActive(true);
        currTopWearStyle = updateTopWearStyle;
    }

    public void setCurrTopWearColor(string updateTopWearColor)
    {
        currTopWearColor = updateTopWearColor;
    }

    public void hasNoBotWear()
    {
        hasBotWear = false;
        gameObject.transform.GetChild(botWearLayerIndex).gameObject.SetActive(false);
    }

    public void setCurrBotWearStyle(string updateBotWearStyle)
    {
        hasBotWear = true;
        gameObject.transform.GetChild(botWearLayerIndex).gameObject.SetActive(true);
        currBotWearStyle = updateBotWearStyle;
    }

    public void setCurrBotWearColor(string updateBotWearColor)
    {
        currBotWearColor = updateBotWearColor;
    }

    public void hasBasicShoes()
    {
        hasShoes = true;
        gameObject.transform.GetChild(shoesLayerIndex).gameObject.SetActive(true);
    }
    public void hasNoShoes()
    {
        hasShoes = false;
        gameObject.transform.GetChild(shoesLayerIndex).gameObject.SetActive(false);
    }

    public void setCurrShoesColor(string updateShoesColor)
    {
        currShoesColor = updateShoesColor;
    }

    public void generateRandomConfig()
    {
        currSkin = skinToneColors[Random.Range(0, 6)]; //inclusive, exclusive actually...
        currHairStyle = hairStyleNames[Random.Range(0, 8)];
        currHairColor = hairColors[Random.Range(0, 13)];
        currTopWearStyle = clothTopNames[Random.Range(0, 4)]; 
        currTopWearColor = clothTopColors[Random.Range(0, 13)];
        currBotWearStyle = clothBotNames[Random.Range(0, 3)];
        currBotWearColor = clothBotColors[Random.Range(0, 13)];
        currShoesColor = shoesColors[Random.Range(0, 4)];
        setCurrSkin(currSkin);
        setCurrHairStyle(currHairStyle);
        setCurrHairColor(currHairColor);
        setCurrTopWearStyle(currTopWearStyle);
        setCurrTopWearColor(currTopWearColor);
        setCurrBotWearStyle(currBotWearStyle);
        setCurrBotWearColor(currBotWearColor);
        hasBasicShoes();
        setCurrShoesColor(currShoesColor);
    }

    public void CopyText()
    {
        charName = field.text;
        if (charName.Trim(' ').Length == 0) charName = "Player"; //default to player
        //could add a display of info and a confirmation, but no for now
        sendInfo();
        playerObj.SetActive(true);
        closeWindow(); 
    }

    public void sendInfo()
    {
        playerObj.SetActive(true);
        CompositionSetter mainComp = playerObj.GetComponent<CompositionSetter>();
        mainComp.charName = charName;
        mainComp.hasHair = hasHair;
        mainComp.hasClothTop = hasTopWear;
        mainComp.hasClothBot = hasBotWear;
        mainComp.hasShoes = hasShoes;
        mainComp.skinColor = currSkin;
        mainComp.hairStyle = currHairStyle;
        mainComp.hairColor = currHairColor;
        mainComp.clothingTop = currTopWearStyle;
        mainComp.clothingTopColor = currTopWearColor;
        mainComp.clothingBot = currBotWearStyle;
        mainComp.clothingBotColor = currBotWearColor;
        mainComp.shoesColor = currShoesColor;
        mainComp.updateChar();

        PFC.SetUserData("charName", charName, "Private");
        PFC.SetUserData("skinColor", currSkin, "Private");
        PFC.SetUserData("hasHair", PersistentData.boolToStr(hasHair), "Private");
        PFC.SetUserData("hairStyle", currHairStyle, "Private");
        PFC.SetUserData("hairColor", currHairColor, "Private");
        PFC.SetUserData("hasTopWear", PersistentData.boolToStr(hasTopWear), "Private");
        PFC.SetUserData("topWearStyle", currTopWearStyle, "Private");
        PFC.SetUserData("topWearColor", currTopWearColor, "Private");
        PFC.SetUserData("hasBotWear", PersistentData.boolToStr(hasBotWear), "Private");
        PFC.SetUserData("botWearStyle", currBotWearStyle, "Private");
        PFC.SetUserData("botWearColor", currBotWearColor, "Private");
        PFC.SetUserData("hasShoes", PersistentData.boolToStr(hasShoes), "Private");
        PFC.SetUserData("shoesColor", currShoesColor, "Private");
    }

    public void closeWindow()
    {
        Destroy(gameObject.transform.parent.gameObject);
        playerCamera.turnOffUICamera();
        playerObj.SetActive(true);
        UIController.hasOneOn = false;
        playerObj.GetComponent<playerController>().isMoving = false;
    }

    public void grabFromMainComp()
    {
        playerObj.SetActive(true);
        CompositionSetter mainComp = playerObj.GetComponent<CompositionSetter>();
        field.text = mainComp.charName;
        hasHair = mainComp.hasHair;
        hasTopWear = mainComp.hasClothTop;
        hasBotWear = mainComp.hasClothBot;
        hasShoes = mainComp.hasShoes;
        currSkin = mainComp.skinColor;
        currHairStyle = mainComp.hairStyle;
        currHairColor = mainComp.hairColor;
        currTopWearStyle = mainComp.clothingTop;
        currTopWearColor = mainComp.clothingTopColor;
        currBotWearStyle = mainComp.clothingBot;
        currBotWearColor = mainComp.clothingBotColor;
        currShoesColor = mainComp.shoesColor;
        setCurrSkin(currSkin);
        gameObject.transform.GetChild(hairLayerIndex).gameObject.SetActive(hasHair);
        gameObject.transform.GetChild(topWearLayerIndex).gameObject.SetActive(hasTopWear);
        gameObject.transform.GetChild(botWearLayerIndex).gameObject.SetActive(hasBotWear);
        gameObject.transform.GetChild(shoesLayerIndex).gameObject.SetActive(hasShoes);
        playerObj.SetActive(false);
    }

    public void scrollRectReset(ScrollRect rect)
    {
        rect.verticalNormalizedPosition = 1f;
    }
}
