using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIInstantPopupSlot : MonoBehaviour {
    
    public Text msg;
    public float sec = 3f;
    public System.Action onDestroy;

    public void SetMsg(string msg)
    {
        LocalizeDynamicText dynamicText = this.gameObject.AddComponent<LocalizeDynamicText>();
        dynamicText = new LocalizeDynamicText(this.msg);
        dynamicText.InitLocalizeText(msg);
        //this.msg.text = msg;
        
    }
    public void DestroyPopup()
    {
        if (onDestroy != null)
            onDestroy();
        Destroy(gameObject);
    }
}
