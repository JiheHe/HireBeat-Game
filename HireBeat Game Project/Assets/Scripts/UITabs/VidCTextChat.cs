/* 
 * Copyright (C) 2021 because-why-not.com Limited
 * 
 * Please refer to the license.txt for license information
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// Shows a list of a text prefab.
/// 
/// Used to show the messages that are sent/received in the ChatApp.
/// </summary>
public class VidCTextChat : MonoBehaviour
{
    /// <summary>
    /// References to the "Text" prefab.
    /// 
    /// Needs to contain RectTransform and Text element.
    /// </summary>
    public GameObject msgObj; //a prefab



    /// <summary>
    /// Reference to the own rect transform
    /// </summary>
    private RectTransform mOwnTransform;

    /// <summary>
    /// Number of messages until the older messages will be deleted.
    /// </summary>
    private int mMaxMessages = 50;


    private int mCounter = 0;

    private void Awake()
    {
        mOwnTransform = GetComponent<RectTransform>();
    }

    /*private void Start()  //I won't have prev msgs so...
    {
        foreach (var v in mOwnTransform.GetComponentsInChildren<RectTransform>())
        {
            if (v != mOwnTransform)
            {
                v.name = "Element " + mCounter;
                mCounter++;
            }
        }
    }*/

    /// <summary>
    /// Allows the Chatapp to add new entires to the list
    /// </summary>
    /// <param name="text">Text to be added</param>
    public void AddTextEntry(string username, string content, bool isMe)
    {
        GameObject ngp = Instantiate(msgObj);

        ngp.transform.GetChild(0).GetComponent<Text>().text = username;
        ngp.transform.GetChild(1).GetComponent<Text>().text = content;

        //If usernanme is my username, set username color to orange.
        if (isMe) ngp.transform.GetChild(0).GetComponent<Text>().color = new Color32(255, 69, 0, 255);

        ngp.GetComponent<RectTransform>().SetParent(mOwnTransform, false);

        /*GameObject go = transform.gameObject;
        go.name = "Element " + mCounter;*/
        mCounter++;

        CheckMessageCapacity();
    }


    /// <summary>
    /// Destroys old messages if needed and repositions the existing messages.
    /// </summary>
    private void CheckMessageCapacity()
    {
        int destroy = mOwnTransform.childCount - mMaxMessages;
        for (int i = 0; i < destroy; i++)
        {
            var child = mOwnTransform.GetChild(i).gameObject;
            Destroy(child);
        }
    }

    /*private void Update() //gonna change this on upon sending / receiving messages.
    {
        int destroy = mOwnTransform.childCount - mMaxMessages;
        for (int i = 0; i < destroy; i++)
        {
            var child = mOwnTransform.GetChild(i).gameObject;
            Destroy(child);
        }
    }*/

}
