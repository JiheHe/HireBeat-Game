using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundSetter : MonoBehaviour
{

    public int skyIndex;
    public GameObject sky;
    public GameObject backgroundUI;
    // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.U))
        {
            Instantiate(backgroundUI);
        }
    }

    public void SetBackground()
    {
        string skyImg = "Backgrounds/Sky/" + "SkyAndCloud" + skyIndex;
        sky.GetComponent<Image>().sprite = Resources.Load<Sprite>(skyImg);
    }

}
