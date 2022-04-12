using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RainbowArt
{
    public enum TriggerAnimType
    {
        Hover = 0,
        Click,
        Auto,
    }
    public class PlayAnimHelper : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public TriggerAnimType triggerAnimType = TriggerAnimType.Hover;
        Animator mAnimator;
        bool mAnimStarted = false;
        void Start()
        {
            mAnimator = GetComponent<Animator>();
            if (triggerAnimType == TriggerAnimType.Auto)
            {
                mAnimator.ResetTrigger("Stop");
                mAnimator.Play("Start");
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (triggerAnimType == TriggerAnimType.Click)
            {
                if (mAnimStarted == false)
                {
                    mAnimStarted = true;
                    mAnimator.ResetTrigger("Stop");
                    mAnimator.Play("Start");
                }
                else
                {
                    mAnimStarted = false;
                    mAnimator.SetTrigger("Stop");
                }
                
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if(triggerAnimType == TriggerAnimType.Hover)
            {
                mAnimator.ResetTrigger("Stop");
                mAnimator.Play("Start");
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (triggerAnimType == TriggerAnimType.Hover)
            {
                mAnimator.SetTrigger("Stop");
            }
        }
    }
}
