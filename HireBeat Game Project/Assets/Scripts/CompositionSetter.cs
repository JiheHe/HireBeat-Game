using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class CompositionSetter : MonoBehaviour
{
    // Start is called before the first frame update

    //public bool hasSkin; //if not use ghost figure, an easteregg
    public string skinColor;
    public bool hasHair;
    public string hairStyle;
    public string hairColor;
    public bool hasClothTop;
    public string clothingTop;
    public string clothingTopColor;
    public bool hasClothBot;
    public string clothingBot;
    public string clothingBotColor;
    public bool hasShoes;
    public string shoesColor;
    public string charName;
    public string charTitle;

    public GameObject skin;
    public GameObject hair;
    public GameObject topWear;
    public GameObject botWear;
    public GameObject shoes;
    public TextMeshPro playerIGN;
    public TextMeshPro playerTitle;

    void Start()
    {
        CreateSkin();
        CreateHair();
        CreateTopWear();
        CreateBotWear();
        CreateShoes();
        UpdateName();
        //playerTitle.text = charTitle;
    }

    public void updateChar()
    {
        CreateSkin();
        CreateHair();
        CreateTopWear();
        CreateBotWear();
        CreateShoes();
        UpdateName();
        //view.RPC("updateCharRPC", RpcTarget.All); //this doesn't work, need to try individual component..
    }

    public void updateTitle(string newTitle) //I don't think this function's ever called LOL, title system handles all
    {
        playerTitle.text = newTitle; //or something more than this: grab a unique material asset from Resource folder
    }

    private void UpdateName()
    {
        playerIGN.gameObject.GetComponent<ChangeObjectAnimator>().UpdateName(charName);
    }

    private void CreateSkin()
    {
        skin.GetComponent<ChangeObjectAnimator>().CreateSkin(skinColor);
    }

    private void CreateHair()
    {
        hair.GetComponent<ChangeObjectAnimator>().CreateHair(hasHair, hairStyle, hairColor);
    }

    private void CreateTopWear()
    {
        topWear.GetComponent<ChangeObjectAnimator>().CreateTopWear(hasClothTop, clothingTop, clothingTopColor);
    }

    private void CreateBotWear()
    {
        botWear.GetComponent<ChangeObjectAnimator>().CreateBotWear(hasClothBot, clothingBot, clothingBotColor);
    }

    private void CreateShoes()
    {
        shoes.GetComponent<ChangeObjectAnimator>().CreateShoes(hasShoes, shoesColor);
    }

}