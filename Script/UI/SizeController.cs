using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class SizeController : MonoBehaviour {

    RectTransform myRect;

    RectTransform[] rects;

    bool isActive = true;

    private void Start()
    {   
        myRect = GetComponent<RectTransform>();

        rects = GetComponentsInChildren<RectTransform>();
        
    }

    public void ContentSizeChange()
    {
        if(isActive)
        {
            for (int i = 1; i < rects.Length; i++)
            {
                myRect.sizeDelta = new Vector2(myRect.sizeDelta.x, myRect.sizeDelta.y + rects[i].sizeDelta.y);
            }

            isActive = false;
        }
    }
}
