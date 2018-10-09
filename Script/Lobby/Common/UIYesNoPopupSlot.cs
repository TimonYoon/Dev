using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class UIYesNoPopupSlot : MonoBehaviour {

	public System.Action<string> onYesNo;
	public Text title;
	public Text msg;
	public void ClickYes()
	{
		if (onYesNo != null)
		{
			onYesNo("yes");
		}
		Destroy(gameObject);
	}
	public void ClickNo()
	{
		if (onYesNo != null)
		{
			onYesNo("no");
		}
		Destroy(gameObject);
	}
	public void SetMsg(string title, string msg)
	{
        //this.title.text = title;
        //this.msg.text = msg;

        LocalizeDynamicText dynamicTextTitle = this.title.gameObject.AddComponent<LocalizeDynamicText>();
        dynamicTextTitle = new LocalizeDynamicText(this.title);
        dynamicTextTitle.InitLocalizeText(title);

        LocalizeDynamicText dynamicTextMsg = this.msg.gameObject.AddComponent<LocalizeDynamicText>();
        dynamicTextMsg = new LocalizeDynamicText(this.msg);
        dynamicTextMsg.InitLocalizeText(msg);
    }
}
