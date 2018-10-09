using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UIMailSlotContainer : MonoBehaviour,IMailReceive {

    
    /*
     * 메일 고유아이디 들고 있음
     * 
     * 일괄 보상 받기 기능 있음
     *  = 보상 아이디도 가지고 있음
     *  = 보상 아이디 체크로 일괄 보상받는 부분은 리스트 제거
     *  
     *  
     *
     */ 


    ///// <summary> 메일 고유값 </summary>
    //public string mailID { get; private set; }
	
    public MailData mailData { get; private set; }

    //public string itemType { get; private set; }

    ///// <summary> 보상 아이디 값 </summary>
    //public string receiveItemID { get; private set; }

    UIMailSlot _mailSlot = null;
    UIMailSlot mailSlot
    {
        get
        {
            return _mailSlot;
        }
        set
        {
            if (!value)
            {
                if (_mailSlot)
                    _mailSlot.gameObject.SetActive(false);

                _mailSlot = null;

                return;
            }

            _mailSlot = value;

            value.transform.SetParent(transform,false);
            //value.transform.localPosition = Vector3.zero;
            //value.transform.localScale = Vector3.one;
            value.SlotDataInit(mailData);

        }
    }


    ScrollRect scrollRect;
    RectTransform rectTransform;
    RectTransform rectTransformViewport;
    RectTransform rectTransformContent;

    /// <summary> 슬롯 초기화 </summary>
    public void SlotInit(MailData _mailData)
    {
        mailData = _mailData;
    }


    void Start()
    {

        if (!scrollRect)
            scrollRect = GetComponentInParent<ScrollRect>();

        if (scrollRect)
        {
            rectTransform = GetComponent<RectTransform>();
            rectTransformViewport = scrollRect.viewport.GetComponent<RectTransform>();
            rectTransformContent = scrollRect.content.GetComponent<RectTransform>();

            //스크롤될 때 콜백 등록
            scrollRect.onValueChanged.AddListener(OnScrollRectValueChanged);
        }
        UpdateContent();
    }

    void UpdateContent()
    {
        if (isNeedToShow)
        {
            if (mailSlot)
                return;

            mailSlot = UIMail.Instance.GetMailSlotFromPool();
            mailSlot.gameObject.SetActive(true);
        }
        else
        {
            mailSlot = null;
        }
    }

    void OnScrollRectValueChanged(Vector2 pos)
    {
        UpdateContent();
    }

    public void Receive()
    {
        Destroy(gameObject);
    }

    bool isNeedToShow
    {
        get
        {
            if (!scrollRect)
                return false;

            float y = -rectTransform.anchoredPosition.y;

            float height = rectTransformViewport.rect.height;

            if (rectTransformContent.anchoredPosition.y - y < 200 && rectTransformContent.anchoredPosition.y - y > -height - 200)
                return true;
            else
                return false;
        }
    }


}
