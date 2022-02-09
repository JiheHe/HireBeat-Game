using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundSetter : MonoBehaviour
{

    public int skyIndex;
    public GameObject sky;
    // Start is called before the first frame update
    void Start()
    {
        SetBackground();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.U))
        {
            skyIndex++;
            if (skyIndex > 10) skyIndex = 1;
            SetBackground();
        }
    }

    void SetBackground()
    {
        string skyImg = "Backgrounds/Sky/" + "SkyAndCloud" + skyIndex;
        sky.GetComponent<Image>().sprite = Resources.Load<Sprite>(skyImg);
    }
}
