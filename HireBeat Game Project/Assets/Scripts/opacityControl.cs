using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class opacityControl : MonoBehaviour
{
    public const float SENSITIVITY = 0.01f;
    public const float SCREEN_HEIGHT_PROPORTION = 0.5f;
    private const int MAX_ALPHA = 256;
    public const float MIN_ALPHA = 0.3f;

    public GameObject gameObj;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float mouseY = Input.mousePosition.y;
        float screenHeight = Screen.height;

        float alphaProportion = ( (SCREEN_HEIGHT_PROPORTION * screenHeight - mouseY) / screenHeight) * MAX_ALPHA * SENSITIVITY;
        if(alphaProportion < MIN_ALPHA) alphaProportion = MIN_ALPHA;

        Color myColor;

        Image myImage = gameObj.GetComponent<Image>();
        if(myImage) {
            myColor = myImage.color;
            myColor.a = alphaProportion;
            myImage.color = myColor;
        } 
        
        Text myText = gameObj.GetComponent<Text>();
        if(myText) {
            myColor = myText.color;
            myColor.a = alphaProportion;
            myText.color = myColor;
        }
    }
}
