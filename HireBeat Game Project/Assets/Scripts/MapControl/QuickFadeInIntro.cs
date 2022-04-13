using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuickFadeInIntro : MonoBehaviour
{
    public Animator animator;
    public TMP_Text roomAnnouncer;

    // Start is called before the first frame update
    void Awake()
    {
        //animator.SetTrigger("FadeIn");
        roomAnnouncer.text = PersistentData.NAME_OF_JOINING_ROOM;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DestroyObject()
    {
        Destroy(gameObject.transform.parent.gameObject);
    }
}
