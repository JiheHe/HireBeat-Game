using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public GameObject dadSkin;
    public GameObject dadHair;
    public GameObject dadClothTop;
    public GameObject dadClothBot;
    public GameObject dadShoes;


    public GameObject customizationUI; //will move this to an NPC later

    void Start()
    {
        CreateSkin();
        CreateHair();
        CreateTopWear();
        CreateBotWear();
        CreateShoes();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Instantiate(customizationUI, new Vector3(0, 0, 0), Quaternion.identity); //will move this to an NPC later
        }

    }

    public void updateChar()
    {
        CreateSkin();
        CreateHair();
        CreateTopWear();
        CreateBotWear();
        CreateShoes();
    }

    private void CreateSkin()
    {
        string charSkin = "Animations/ThatCoolSprite/SkinTones/" + skinColor + "/" + skinColor + "Controller";
        GameObject skin = dadSkin.transform.GetChild(0).gameObject; //index for skin is 1, can also do .FindChild("childName")
        skin.GetComponent<Animator>().runtimeAnimatorController = Resources.Load(charSkin) as RuntimeAnimatorController;
    }

    private void CreateHair()
    {
        GameObject hair = dadHair.transform.GetChild(0).gameObject; //index for hair is 2
        if (hasHair)
        {
            hair.SetActive(true);
            string charHair = "Animations/ThatCoolSprite/HairStyles/" + hairStyle + "/" + hairColor + "/" + hairStyle + hairColor + "Controller";
            hair.GetComponent<Animator>().runtimeAnimatorController = Resources.Load(charHair) as RuntimeAnimatorController;
        }
        else
        {
            hair.SetActive(false);
        }
    }

    private void CreateTopWear()
    {
        GameObject topWear = dadClothTop.transform.GetChild(0).gameObject; //index for topWear is 3
        if (hasClothTop)
        {
            topWear.SetActive(true);
            string charTop = "Animations/ThatCoolSprite/Clothes/" + clothingTop + "/" + clothingTopColor + "/" + clothingTop + clothingTopColor + "Controller";
            topWear.GetComponent<Animator>().runtimeAnimatorController = Resources.Load(charTop) as RuntimeAnimatorController;
        }
        else
        {
            topWear.SetActive(false);
        }
    }

    private void CreateBotWear()
    {
        GameObject botWear = dadClothBot.transform.GetChild(0).gameObject; //index for botWear is 4
        if (hasClothBot)
        {
            botWear.SetActive(true);
            string charBot = "Animations/ThatCoolSprite/Clothes/" + clothingBot + "/" + clothingBotColor + "/" + clothingBot + clothingBotColor + "Controller";
            botWear.GetComponent<Animator>().runtimeAnimatorController = Resources.Load(charBot) as RuntimeAnimatorController;
        }
        else
        {
            botWear.SetActive(false);
        }
    }

    private void CreateShoes()
    {
        GameObject shoes = dadShoes.transform.GetChild(0).gameObject; //index for shoes is 5
        if (hasShoes)
        {
            shoes.SetActive(true);
            string charShoes = "Animations/ThatCoolSprite/Clothes/Shoes/" + shoesColor + "/Shoes" + shoesColor + "Controller";
            shoes.GetComponent<Animator>().runtimeAnimatorController = Resources.Load(charShoes) as RuntimeAnimatorController;
        }
        else
        {
            shoes.SetActive(false);
        }
    }

}
