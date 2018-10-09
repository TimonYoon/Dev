using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIPopup : MonoBehaviour {

	public GameObject okPopupSlot;
	public GameObject yesNoPopupSlot;
    public GameObject instantPopupSlot;
    public GameObject backPanel;
    public Transform popUpGroup;

    
    private void Awake()
    {
        okPopupSlot.SetActive(false);
        yesNoPopupSlot.SetActive(false);
        backPanel.SetActive(false);
        instantPopupSlot.SetActive(false);
    }

    public GameObject CreateOKPopupSlot()
	{
		GameObject ok = Instantiate(okPopupSlot);
		ok.name = "OKPopupSlot";
		ok.transform.SetParent(popUpGroup, false);
        ok.GetComponent<UIOKPopupSlot>().onOK += OnOK;
        backPanel.SetActive(true);
        return ok;
	}

	public GameObject CreateYesNoPopupSlot()
	{
		GameObject ok = Instantiate(yesNoPopupSlot);
		ok.name = "YesNoPopupSlot";
		ok.transform.SetParent(popUpGroup, false);
        ok.GetComponent<UIYesNoPopupSlot>().onYesNo += OnYesNo;
        backPanel.SetActive(true);
        return ok;
	}
    List<GameObject> instantPopup = new List<GameObject>();
    
    public GameObject CreateInstantPopupSlot()
    {
        if(instantPopup.Count < 6)
        {
            GameObject ok = Instantiate(instantPopupSlot);
            instantPopup.Add(ok);
            ok.name = "InstantPopupSlot";
            ok.transform.SetParent(popUpGroup, false);
            ok.GetComponent<UIInstantPopupSlot>().onDestroy += OnInstantDestroy;
            return ok;
        }
        else
        {
            return null;
        }
        
    }
    void OnInstantDestroy()
    {
        instantPopup.RemoveAt(0);
        OnPopupChack();
    }
    void OnOK()
    {
        OnPopupChack();
    }
    void OnYesNo(string result)
    {
        OnPopupChack();
    }

    void OnPopupChack()
    {
        // 콜백을 날릴뒤 popup이 destory 되기 때문에 1개 체크
        if(popUpGroup.childCount <= 1)
        {
            backPanel.SetActive(false);
        }
    }
}
