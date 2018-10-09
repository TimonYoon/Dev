using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIPopupManager : MonoBehaviour
{
	static UIPopup uiPopup;

	void Awake()
	{
		GameObject popup = UIResourceLoader.Load("UI", "UIPopup");
		uiPopup = popup.GetComponent<UIPopup>();
	}	
	
	static void Init()
	{
		if (uiPopup)
			return;

		GameObject go = new GameObject("UIPopupManager");
		go.transform.position = Vector3.zero;
		go.transform.rotation = Quaternion.identity;
		go.AddComponent<UIPopupManager>();
		DontDestroyOnLoad(go);
	}
	
	static public void ShowOKPopup(string title, string msg, System.Action okCallback)
	{
		if( null == uiPopup)
			Init();

		GameObject go = uiPopup.CreateOKPopupSlot();
		UIOKPopupSlot okPopupSlot = go.GetComponent<UIOKPopupSlot>();
		okPopupSlot.onOK += okCallback;
		okPopupSlot.SetMsg(title, msg);
		go.SetActive(true);
	}

	static public void ShowYesNoPopup(string title, string msg, System.Action<string> yesNoCallback)
	{
		if (null == uiPopup)
			Init();

		GameObject go = uiPopup.CreateYesNoPopupSlot();
		UIYesNoPopupSlot yesNoPopupSlot = go.GetComponent<UIYesNoPopupSlot>();
		yesNoPopupSlot.onYesNo += yesNoCallback;
		yesNoPopupSlot.SetMsg(title, msg);
		go.SetActive(true);
	}

    static public void ShowInstantPopup(string msg)
    {
        if (null == uiPopup)
            Init();

        GameObject go = uiPopup.CreateInstantPopupSlot();

        if (go == null)
            return;

        UIInstantPopupSlot instantPopupSlot = go.GetComponent<UIInstantPopupSlot>();
        instantPopupSlot.SetMsg(msg);
        go.SetActive(true);
    }
}
