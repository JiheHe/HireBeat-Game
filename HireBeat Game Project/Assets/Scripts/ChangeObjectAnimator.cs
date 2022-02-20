using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class ChangeObjectAnimator : MonoBehaviourPunCallbacks
{
    PhotonView view;
    // Start is called before the first frame update 
    //every obj needs a photon view else it bugs!
    void Start()
    {
        view = GetComponent<PhotonView>();

    }

    public void CreateSkin(string skinColor)
    {
        view.RPC("CreateSkinRPC", RpcTarget.All, skinColor);
    }

    [PunRPC]
    void CreateSkinRPC(string skinColor)
    {
        string charSkin = "Animations/ThatCoolSprite/SkinTones/" + skinColor + "/" + skinColor + "Controller";
        gameObject.GetComponent<Animator>().runtimeAnimatorController = Resources.Load(charSkin) as RuntimeAnimatorController;
    }

    public void CreateHair(bool hasHair, string hairStyle, string hairColor)
    {
        view.RPC("CreateHairRPC", RpcTarget.All, hasHair, hairStyle, hairColor);
    }

    [PunRPC]
    void CreateHairRPC(bool hasHair, string hairStyle, string hairColor)
    {
        if (hasHair)
        {
            gameObject.SetActive(true);
            string charHair = "Animations/ThatCoolSprite/HairStyles/" + hairStyle + "/" + hairColor + "/" + hairStyle + hairColor + "Controller";
            gameObject.GetComponent<Animator>().runtimeAnimatorController = Resources.Load(charHair) as RuntimeAnimatorController;
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void CreateTopWear(bool hasClothTop, string clothingTop, string clothingTopColor)
    {
        view.RPC("CreateTopWearRPC", RpcTarget.All, hasClothTop, clothingTop, clothingTopColor);
    }

    [PunRPC]
    void CreateTopWearRPC(bool hasClothTop, string clothingTop, string clothingTopColor)
    {
        if (hasClothTop)
        {
            gameObject.SetActive(true);
            string charTop = "Animations/ThatCoolSprite/Clothes/" + clothingTop + "/" + clothingTopColor + "/" + clothingTop + clothingTopColor + "Controller";
            gameObject.GetComponent<Animator>().runtimeAnimatorController = Resources.Load(charTop) as RuntimeAnimatorController;
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void CreateBotWear(bool hasClothBot, string clothingBot, string clothingBotColor)
    {
        view.RPC("CreateBotWearRPC", RpcTarget.All, hasClothBot, clothingBot, clothingBotColor);
    }

    [PunRPC]
    void CreateBotWearRPC(bool hasClothBot, string clothingBot, string clothingBotColor)
    {
        if (hasClothBot)
        {
            gameObject.SetActive(true);
            string charBot = "Animations/ThatCoolSprite/Clothes/" + clothingBot + "/" + clothingBotColor + "/" + clothingBot + clothingBotColor + "Controller";
            gameObject.GetComponent<Animator>().runtimeAnimatorController = Resources.Load(charBot) as RuntimeAnimatorController;
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void CreateShoes(bool hasShoes, string shoesColor)
    {
        view.RPC("CreateShoesRPC", RpcTarget.All, hasShoes, shoesColor);
    }

    [PunRPC]
    void CreateShoesRPC(bool hasShoes, string shoesColor)
    {
        if (hasShoes)
        {
            gameObject.SetActive(true);
            string charShoes = "Animations/ThatCoolSprite/Clothes/Shoes/" + shoesColor + "/Shoes" + shoesColor + "Controller";
            gameObject.GetComponent<Animator>().runtimeAnimatorController = Resources.Load(charShoes) as RuntimeAnimatorController;
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void UpdateName(string newName)
    {
        view.RPC("UpdateNameRPC", RpcTarget.All, newName);
    }

    [PunRPC]
    void UpdateNameRPC(string newName)
    {
        gameObject.GetComponent<TextMeshPro>().text = newName;
    }
}