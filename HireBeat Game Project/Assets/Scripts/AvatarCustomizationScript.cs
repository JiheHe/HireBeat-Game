using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; //need this line!!

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

    // Start is called before the first frame update
    void Start()
    {
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
        currTopWearStyle = clothTopNames[Random.Range(0, 3)]; //shirt not included
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
}
