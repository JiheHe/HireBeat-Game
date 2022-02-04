using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompositionTester : MonoBehaviour
{
    CompositionSetter setter;

    public string[] skinToneColors;
    public string[] hairStyleNames;
    public string[] hairColors;
    public string[] clothTopNames;
    public string[] clothTopColors;
    public string[] clothBotNames;
    public string[] clothBotColors;
    public string[] shoesColors;

    [Range(0, 5)]
    public int stcIndex;

    [Range(0, 7)]
    public int hsIndex;

    [Range(0, 12)]
    public int hcIndex;

    [Range(0, 2)] //ignore shirt for now
    public int ctIndex;

    [Range(0, 12)]
    public int ctcIndex;

    [Range(0, 2)]
    public int cbIndex;

    [Range(0, 12)]
    public int cbcIndex;

    [Range(0, 3)]
    public int scIndex;

    // Start is called before the first frame update
    void Start()
    {
        setter = GetComponent<CompositionSetter>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.O))
        {
            reconfigureChar();
        }
        if(Input.GetKeyDown(KeyCode.P))
        {
            generateRandomConfig();
        }
        
    }

    void reconfigureChar()
    {
        if (!(clothTopNames[ctIndex] == "Overalls" && clothTopColors[ctcIndex] == "Gray"))
        {
            Debug.Log("Current combination:\n skin color: " + skinToneColors[stcIndex]
            + ", hair style: " + hairStyleNames[hsIndex] + ", hair color: " + hairColors[hcIndex]
            + ", top cloth: " + clothTopNames[ctIndex] + ", top cloth color: " + clothTopColors[ctcIndex]
            + ", bottom cloth: " + clothBotNames[cbIndex] + ", bottom cloth color: " + clothBotColors[cbcIndex]
            + ", shoes color: " + shoesColors[scIndex] + ".\n");
            setter.skinColor = skinToneColors[stcIndex];
            setter.hairStyle = hairStyleNames[hsIndex];
            setter.hairColor = hairColors[hcIndex];
            setter.clothingTop = clothTopNames[ctIndex];
            setter.clothingTopColor = clothTopColors[ctcIndex];
            setter.clothingBot = clothBotNames[cbIndex];
            setter.clothingBotColor = clothBotColors[cbcIndex];
            setter.shoesColor = shoesColors[scIndex];
            setter.updateChar();
        }
        else
        {
            Debug.Log("No Gray Overalls :(, please do another set without such value.\n");
        }
    }

    void generateRandomConfig()
    {
        stcIndex = Random.Range(0, 6); //inclusive, exclusive actually...
        hsIndex = Random.Range(0, 8);
        hcIndex = Random.Range(0, 13);
        ctIndex = Random.Range(0, 3); //shirt not included
        ctcIndex = Random.Range(0, 13);
        cbIndex = Random.Range(0, 3); 
        cbcIndex = Random.Range(0, 13);
        scIndex = Random.Range(0, 4);
        reconfigureChar();
    }
}
