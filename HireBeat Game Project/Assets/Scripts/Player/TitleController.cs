using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleController : MonoBehaviour
{
    bool[] titles;
    int titleIndex;
    // Start is called before the first frame update
    void Start()
    {
        titleIndex = 0;
        titles = new bool[16]; //curr only 16 titles
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            int nextIndex = titleIndex + 1;
            if (nextIndex > titles.Length-1) nextIndex = 0;

            if (titles[nextIndex])
            {
                transform.GetChild(titleIndex).gameObject.SetActive(false);
                transform.GetChild(nextIndex).gameObject.SetActive(true);
                titleIndex = nextIndex;
            }
            else
            {
                Debug.Log("No title access");
            }
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            for (int i = 0; i < titles.Length; i++)
            {
                titles[i] = true;
            }
            Debug.Log("Accesses unlocked");
        }
    }
}
