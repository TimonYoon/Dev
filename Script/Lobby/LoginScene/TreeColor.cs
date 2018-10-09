using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TreeColor : MonoBehaviour {

    public bool isTree;
    public Image image;
    
    private void Awake()
    {
        image = GetComponent<Image>();
        if (isTree == true)
        {
            GetComponent<Image>().color = UIColorOverray.lerpedTreeColor;
        }
        else
        {
            GetComponent<Image>().color = UIColorOverray.lerpedCloudColor;
        }
    }
	
	void Update ()
    {
        if(isTree == true)
        {
            GetComponent<Image>().color = UIColorOverray.lerpedTreeColor;
        }
        else
        {
            GetComponent<Image>().color = UIColorOverray.lerpedCloudColor;
        }
        
	}
}
