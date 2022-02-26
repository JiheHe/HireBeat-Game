using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class ChangeObjectAnimator : MonoBehaviour //I don't think monobehaviourcallbacks matters
{
    PhotonView view;
    // Start is called before the first frame update 
    //every obj needs a photon view else it bugs!
    //since RPC scripts are attached to the object, if disable the object then script doesn't work anymore! So better to make transparent
    void Start()
    {
        view = GetComponent<PhotonView>();

    }

    public void CreateSkin(string skinColor) 
    {
        view.RPC("CreateSkinRPC", RpcTarget.AllBuffered, skinColor);
    }

    [PunRPC]
    public void CreateSkinRPC(string skinColor) 
    {
        string charSkin = "Animations/ThatCoolSprite/SkinTones/" + skinColor + "/" + skinColor + "Controller";
        gameObject.GetComponent<Animator>().runtimeAnimatorController = Resources.Load(charSkin) as RuntimeAnimatorController;
    }

    public void CreateHair(bool hasHair, string hairStyle, string hairColor)
    {
        view.RPC("CreateHairRPC", RpcTarget.AllBuffered, hasHair, hairStyle, hairColor);
    }

    [PunRPC]
    public void CreateHairRPC(bool hasHair, string hairStyle, string hairColor)
    {
        SpriteRenderer rend = gameObject.GetComponent<SpriteRenderer>();
        if (hasHair)
        {
            changeAlpha(rend, 1f);
            string charHair = "Animations/ThatCoolSprite/HairStyles/" + hairStyle + "/" + hairColor + "/" + hairStyle + hairColor + "Controller";
            gameObject.GetComponent<Animator>().runtimeAnimatorController = Resources.Load(charHair) as RuntimeAnimatorController;
        }
        else
        {
            changeAlpha(rend, 0f);
        }
    }

    public void CreateTopWear(bool hasClothTop, string clothingTop, string clothingTopColor)
    {
        view.RPC("CreateTopWearRPC", RpcTarget.AllBuffered, hasClothTop, clothingTop, clothingTopColor);
    }

    [PunRPC]
    public void CreateTopWearRPC(bool hasClothTop, string clothingTop, string clothingTopColor)
    {
        SpriteRenderer rend = gameObject.GetComponent<SpriteRenderer>();
        if (hasClothTop)
        {
            changeAlpha(rend, 1f);
            string charTop = "Animations/ThatCoolSprite/Clothes/" + clothingTop + "/" + clothingTopColor + "/" + clothingTop + clothingTopColor + "Controller";
            gameObject.GetComponent<Animator>().runtimeAnimatorController = Resources.Load(charTop) as RuntimeAnimatorController;
        }
        else
        {
            changeAlpha(rend, 0f);
        }
    }

    public void CreateBotWear(bool hasClothBot, string clothingBot, string clothingBotColor)
    {
        view.RPC("CreateBotWearRPC", RpcTarget.AllBuffered, hasClothBot, clothingBot, clothingBotColor);
    }

    [PunRPC]
    public void CreateBotWearRPC(bool hasClothBot, string clothingBot, string clothingBotColor)
    {
        SpriteRenderer rend = gameObject.GetComponent<SpriteRenderer>();
        if (hasClothBot)
        {
            changeAlpha(rend, 1f);
            string charBot = "Animations/ThatCoolSprite/Clothes/" + clothingBot + "/" + clothingBotColor + "/" + clothingBot + clothingBotColor + "Controller";
            gameObject.GetComponent<Animator>().runtimeAnimatorController = Resources.Load(charBot) as RuntimeAnimatorController;
        }
        else
        {
            changeAlpha(rend, 0f);
        }
    }

    public void CreateShoes(bool hasShoes, string shoesColor)
    {
        view.RPC("CreateShoesRPC", RpcTarget.AllBuffered, hasShoes, shoesColor);
    }

    [PunRPC]
    public void CreateShoesRPC(bool hasShoes, string shoesColor)
    {
        SpriteRenderer rend = gameObject.GetComponent<SpriteRenderer>();
        if (hasShoes)
        {
            changeAlpha(rend, 1f);
            string charShoes = "Animations/ThatCoolSprite/Clothes/Shoes/" + shoesColor + "/Shoes" + shoesColor + "Controller";
            gameObject.GetComponent<Animator>().runtimeAnimatorController = Resources.Load(charShoes) as RuntimeAnimatorController;
        }
        else
        {
            changeAlpha(rend, 0f);
        }
    }

    public void UpdateName(string newName)
    {
        view.RPC("UpdateNameRPC", RpcTarget.AllBuffered, newName);
    }

    [PunRPC]
    public void UpdateNameRPC(string newName)
    {
        gameObject.GetComponent<TextMeshPro>().text = newName;
    }

    void changeAlpha(SpriteRenderer rend, float val)
    {
        Color tmp = rend.color;
        tmp.a = val;
        rend.color = tmp;
    }
}