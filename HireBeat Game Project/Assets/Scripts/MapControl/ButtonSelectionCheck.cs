using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonSelectionCheck : MonoBehaviour, ISelectHandler
{
    public void OnSelect(BaseEventData eventData)
    {
        gameObject.GetComponent<Button>().onClick.Invoke(); //this line simulates arrow key selection as if it's clicked
    }
}
