using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEngine.Networking;

public class ProfilePictureLibScript : MonoBehaviour, IDragHandler, IBeginDragHandler
{
    public int pfpIndex; //0-15 is female, 16-31 is male (just -16)
    public ScrollRect rect;

    public RectTransform scrollRectTransform;
    public RectTransform contentPanel;
    RectTransform selectedRectTransform;
    GameObject lastSelected;

    string path;
    public RawImage rawImage;
    bool rawImageImported;
    float ratio = 1;
    float imgWidth;
    float imgHeight;
    Sprite newSprite;

    float xDiff;
    float yDiff;

    public int defaultWidth;
    public int defaultHeight;
    public Texture2D defaultTex;

    public GameObject testImage;
    public GameObject maskedRect;

    public RawImage targetRawImage;

    public RenderTexture rTex;
    public GameObject outputFinal;

    public string imgFormat;
    public GameObject format; //input field
    public Animator warningMessage;
    public Toggle sidePicker;

    public FlexibleColorPicker fcp; //thank you
    public Image backgroundImage;
    public Image backgroundImage2;

    public GameObject playerObj;
    public cameraController playerCamera;

    public InGameUIController playerZoneTab;
    public PlayerMenuUIController UIController;

    public changeReceiver playerHud;

    // Start is called before the first frame update
    void Start()
    {
        playerObj = GameObject.FindGameObjectWithTag("Player");

        GameObject cameraController = GameObject.FindGameObjectWithTag("PlayerCamera");

        //playerObj.SetActive(false);
        playerCamera = cameraController.GetComponent<cameraController>();
        
        //playerCamera.turnOnUICamera();
        UIController = cameraController.GetComponent<PlayerMenuUIController>();
        playerZoneTab = cameraController.GetComponent<InGameUIController>();
        //the line above also works at start! No need for at the end ;D

        playerHud = GameObject.FindGameObjectWithTag("PlayerHUD").transform.GetChild(0).GetComponent<changeReceiver>(); //does generation order matter...

        if (!playerZoneTab.hasOneOn) //prevents zone + UI
        {
            playerObj.GetComponent<playerController>().enabled = false;
            playerCamera.enabled = false;
        }


        scrollRectReset(rect);
        rawImageImported = false;

        imgFormat = "png";
        fcp.color = Color.white;
        newSprite = null;
 
    }

    // Update is called once per frame
    void Update()
    {
        string pfpImg = "ProfilePictures/PixelPortraits/" + numToGender(pfpIndex);
        scrollAdjustment();

        if (rawImageImported)
        {
            backgroundImage2.color = fcp.color;
            scaleImage();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            backgroundImage.color = fcp.color;
            if(pfpIndex != -1)
            {
                rawImageImported = false;
                targetRawImage.GetComponent<RectTransform>().localPosition = new Vector2(0, 0);
                targetRawImage.GetComponent<RectTransform>().sizeDelta = new Vector2(defaultWidth * 800/180, defaultHeight * 800/180);
                targetRawImage.texture = Resources.Load<Texture>(pfpImg);
            }
            Texture2D finalTex = ToTexture2D(rTex);
            newSprite = Sprite.Create((Texture2D)finalTex, new Rect(0, 0, finalTex.width, finalTex.height), new Vector2(0.5f, 0.5f));
            outputFinal.GetComponent<Image>().sprite = newSprite;

        }

    }

    public void setPfpIndex(int index) //if pfpIndex = -1, then that means user image is selected currently
    {
        pfpIndex = index;
        if(rawImage.texture != defaultTex)
        {
            rawImageImported = false; //temp disable editing access until toggle is pressed
            sidePicker.gameObject.SetActive(true);
            sidePicker.isOn = false;
        }
    }

    //doing this for fun
    private string numToGender(int index)
    {
        if(index <= 15)
        {
            if (index < 10) return "female_0" + index;
            return "female_" + index;
        }
        if (index-16 < 10) return "male_0" + (index-16);
        return "male_" + (index-16);
    }
    public void scrollRectReset(ScrollRect rect)
    {
        rect.verticalNormalizedPosition = 1f;
    }

    //https://forum.unity.com/threads/scroll-rect-and-scroll-bar-arrow-keys-control.339661/
    //thanks to him, I don't have to code this... I'm lazy ;D
    //make sure pivots are on top left for good results!
    public void scrollAdjustment()
    {
        GameObject selected = EventSystem.current.currentSelectedGameObject;

        // Return if there are none.
        if (selected == null)
        {
            return;
        }
        // Return if the selected game object is not inside the scroll rect.
        if (selected.transform.parent != contentPanel.transform)
        {
            return;
        }
        // Return if the selected game object is the same as it was last frame,
        // meaning we haven't moved.
        if (selected == lastSelected)
        {
            return;
        }

        // Get the rect tranform for the selected game object.
        selectedRectTransform = selected.GetComponent<RectTransform>();
        // The position of the selected UI element is the absolute anchor position,
        // ie. the local position within the scroll rect + its height if we're
        // scrolling down. If we're scrolling up it's just the absolute anchor position.
        float selectedPositionY = Mathf.Abs(selectedRectTransform.anchoredPosition.y) + selectedRectTransform.rect.height;

        // The upper bound of the scroll view is the anchor position of the content we're scrolling.
        float scrollViewMinY = contentPanel.anchoredPosition.y;
        // The lower bound is the anchor position + the height of the scroll rect.
        float scrollViewMaxY = contentPanel.anchoredPosition.y + scrollRectTransform.rect.height;

        // If the selected position is below the current lower bound of the scroll view we scroll down.
        if (selectedPositionY > scrollViewMaxY)
        {
            float newY = selectedPositionY - scrollRectTransform.rect.height;
            contentPanel.anchoredPosition = new Vector2(contentPanel.anchoredPosition.x, newY);
        }
        // If the selected position is above the current upper bound of the scroll view we scroll up.
        else if (Mathf.Abs(selectedRectTransform.anchoredPosition.y) < scrollViewMinY)
        {
            contentPanel.anchoredPosition = new Vector2(contentPanel.anchoredPosition.x, Mathf.Abs(selectedRectTransform.anchoredPosition.y));
        }

        lastSelected = selected;
    }

    //THANK YOU https://www.youtube.com/watch?v=RuC-aU4Q7GA
    //also some of my modification
    public void OpenFileExplorer()
    {
        path = EditorUtility.OpenFilePanel("Show all images (." + imgFormat + ")", "", imgFormat); //change third paraem to extension desired
        StartCoroutine(GetTexture());
    }

    IEnumerator GetTexture()
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture("file:///" + path);
        
        
        yield return www.SendWebRequest();

        if(www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(www.error);
            //since nothing, then don't set it to true
        }
        else
        {
            imgWidth = defaultWidth;
            imgHeight = defaultHeight;
            rawImage.GetComponent<RectTransform>().localPosition = new Vector2(0, 0);

            Texture2D myTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            if (myTexture == null)
            {
                warningMessage.SetTrigger("Warning!"); //plays once then resets, convenient!
                rawImage.GetComponent<RectTransform>().sizeDelta = new Vector2(imgWidth, imgHeight);
                rawImage.texture = defaultTex;
                rawImageImported = false;
            }
            else
            {
                pfpIndex = -1;
                sidePicker.isOn = true;
                float hToWRatio = (float)myTexture.height / myTexture.width;

                if (hToWRatio > 1) //height dominates width, or equal  //is there a grab to grab rawImage.width/height directly?
                {
                    rawImage.GetComponent<RectTransform>().sizeDelta = new Vector2(imgWidth * (hToWRatio - 1), imgHeight);
                }
                else //width dominates height
                {
                    rawImage.GetComponent<RectTransform>().sizeDelta = new Vector2(imgWidth, imgHeight * hToWRatio);
                }

                ratio = 1;
                rawImage.texture = myTexture;

                rawImageImported = true;
                imgWidth = rawImage.GetComponent<RectTransform>().rect.width;
                imgHeight = rawImage.GetComponent<RectTransform>().rect.height;

                targetRawImage.GetComponent<RectTransform>().sizeDelta = rawImage.GetComponent<RectTransform>().sizeDelta * (float)800 / 180; //scale other obj up
                targetRawImage.transform.localPosition = rawImage.transform.localPosition * (float)800 / 180;
                targetRawImage.texture = rawImage.texture;
            }
        }
    }


    public void OnDrag(PointerEventData eventData)
    {

        if (rawImageImported)
        {
            rawImage.transform.position = new Vector2(Input.mousePosition.x-xDiff, Input.mousePosition.y-yDiff);
            targetRawImage.transform.localPosition = rawImage.transform.localPosition * (float)800 / 180;
        } 
    }

    public void OnBeginDrag(PointerEventData eventData) //record position when clicked, the maintain it ;D
    {
        xDiff = Input.mousePosition.x - rawImage.transform.position.x;
        yDiff = Input.mousePosition.y - rawImage.transform.position.y;
    }

    public void scaleImage()
    {
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            ratio += Input.GetAxis("Mouse ScrollWheel") / 5.0f;
            rawImage.GetComponent<RectTransform>().sizeDelta = new Vector2(imgWidth * ratio, imgHeight * ratio);
            targetRawImage.GetComponent<RectTransform>().sizeDelta = rawImage.GetComponent<RectTransform>().sizeDelta * (float)800 / 180;
        }
    }

    public Texture2D ToTexture2D(RenderTexture rTex)
    {
        RenderTexture currentActiveRT = RenderTexture.active;
        RenderTexture.active = rTex;
        Camera.current.Render();
        // Create a new Texture2D and read the RenderTexture image into it
        Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGB24, false, true); //avoid additional gamma correction!
        tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
        tex.Apply();
        RenderTexture.active = currentActiveRT;
        return tex;
    }

    void Texture2DCropper(Texture2D mTexture, int width, int height)
    {
        //Original way: make an array with pos and color data of each pixel, then fill up the rest with white (those outside of data)
        //Color[] c = mTexture.GetPixels(0, 0, width, height); //starting from bottom left
        Texture2D m2Texture = new Texture2D(width, height);
        //m2Texture.SetPixels(c);
        //m2Texture.Apply();
        //gameObject.GetComponent<MeshRenderer>().material.mainTexture = m2Texture;
        /*for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                //m2Texture.SetPixel(x, y, Color.black);
                Texture2D oldText = (Texture2D)rawImage.texture;
                m2Texture.SetPixel(x, y, oldText.GetPixel(x, y));
            }
        }
        m2Texture.Apply(); //screw the bottom method, will try the original way, but wait try bottom with render texture...*/
        /*m2Texture.ReadPixels(maskedRect.GetComponent<RectTransform>().rect, 0, 0);
        m2Texture.Apply();*/


        Sprite newSprite = Sprite.Create((Texture2D)m2Texture, new Rect(0, 0, m2Texture.width, m2Texture.height), new Vector2(0.5f, 0.5f));
        testImage.GetComponent<Image>().sprite = newSprite;
    }

    ////////////////////////////////////////////////////////////// (easier stuff)
    public void OnDropDownChanged(Dropdown dropDown)
    {
        switch(dropDown.value)
        {
            case 0:
                imgFormat = "png";
                format.SetActive(false);
                break;
            case 1:
                imgFormat = "jpg";
                format.SetActive(false);
                break;
            case 2:
                format.GetComponent<InputField>().text = "";
                format.SetActive(true);
                break;
        }
    }

    public void GrabCurrentInputValue()
    {
        imgFormat = format.GetComponent<InputField>().text;
    }

    public void OnToggleChanged() 
    {
        if(sidePicker.isOn) 
        {
            rawImageImported = true;
            pfpIndex = -1;
            targetRawImage.GetComponent<RectTransform>().sizeDelta = rawImage.GetComponent<RectTransform>().sizeDelta * (float)800 / 180;
            targetRawImage.transform.localPosition = rawImage.transform.localPosition * (float)800 / 180;
            targetRawImage.texture = rawImage.texture;
            fcp.color = backgroundImage2.color; //return prev color
            Invoke("disableSidePicker", 0.25f);
            
        } 
    }

    private void disableSidePicker()
    {
        sidePicker.gameObject.SetActive(false);
    }

    public void closeWindow()
    {
        Destroy(gameObject.transform.parent.gameObject);
        //playerCamera.turnOffUICamera();
        //playerObj.SetActive(true);
        if (!playerZoneTab.hasOneOn)
        {
            playerObj.GetComponent<playerController>().enabled = true;
            playerCamera.enabled = true;
            playerObj.GetComponent<playerController>().isMoving = false; //this line prevents the player from getitng stuck after
        }
        UIController.hasOneOn = false;
    }

    public void sendInfo()
    {
        if(newSprite != null)
        {
            playerHud.changeProfilePicture(newSprite);
            closeWindow();
        }
        
    }



    //Maybe I could grab sprites directly from the button icon's image component... w/e i'm too lazy
}
