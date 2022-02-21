using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundSetter : MonoBehaviour
{

    public int skyIndex;
    public GameObject sky;

    public void SetBackground()
    {
        string skyImg = "Backgrounds/Sky/" + "SkyAndCloud" + skyIndex;
        sky.GetComponent<Image>().sprite = Resources.Load<Sprite>(skyImg);
    }

}
