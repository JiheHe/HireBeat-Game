using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class loadUiTestScene : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("debugger is working!");
        SceneManager.LoadScene("loginMenuTestScene");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
