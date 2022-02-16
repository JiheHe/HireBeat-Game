using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleController : MonoBehaviour
{
    int titleIndex;
    // Start is called before the first frame update
    void Start()
    {
        titleIndex = 0;
    }

    public void changeTitle(int nextIndex)
    {
        transform.GetChild(titleIndex).gameObject.SetActive(false);
        transform.GetChild(nextIndex).gameObject.SetActive(true);
        titleIndex = nextIndex;
    }
}
