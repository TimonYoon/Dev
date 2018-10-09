using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIOKPopupSlot : MonoBehaviour {

	//public delegate void OnOK();
	//public event OnOK onOK;
	public System.Action onOK;
	// Use this for initialization
	public Text title;
	public Text msg;

	public void ClickOK()
	{
		if (onOK != null)
		{
			onOK();
		}
		Destroy(gameObject);
	}

	public void SetMsg( string title, string msg )
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
