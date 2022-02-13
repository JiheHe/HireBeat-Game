using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class soundControl : MonoBehaviour
{
    public GameObject sfxImage;

    public Sprite sfxImageHighSprite;
    public Sprite sfxImageLowSprite;
    public Sprite sfxImageMuteSprite;

    public int volume; //0 mute; 1 low; 2 high volume

    // Start is called before the first frame update
    void Start()
    {
        volume = 2;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void soundButtonClicked() {
        if (2 == volume) {              //currently at high volume; change to mute
            volume = 0;
            sfxImage.GetComponent<Image>().sprite = sfxImageMuteSprite;
        } else if (1 == volume) {       //currently at low volume; change to high
            volume = 2;
            sfxImage.GetComponent<Image>().sprite = sfxImageHighSprite;
        } else {                        //currently mute; change to low volume
            volume = 1;
            sfxImage.GetComponent<Image>().sprite = sfxImageLowSprite;
        }
    }
}
