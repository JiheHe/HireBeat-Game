using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameUIController : MonoBehaviour
{
    public bool hasOneOn;
    public GameObject characterCustomizationUI;
    public GameObject backgroundChangerUI;
    public int zoneNumber;
    // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
        switch (zoneNumber)
        {
            case 1: //Livingroom computer top left
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    instantiateCharacterCustomization();
                }
                else if (Input.GetKeyDown(KeyCode.E))
                {
                    instantiateBackgroundChanger();
                }
                break;
        }
        
    }

    public void instantiateCharacterCustomization()
    {
        if(!hasOneOn)
        {
            hasOneOn = true;
            Instantiate(characterCustomizationUI, new Vector3(0, 0, 0), Quaternion.identity);
        }
    }
    public void instantiateBackgroundChanger()
    {
        if (!hasOneOn)
        {
            hasOneOn = true;
            Instantiate(backgroundChangerUI, new Vector3(0, 0, 0), Quaternion.identity);
        }
    }
}
