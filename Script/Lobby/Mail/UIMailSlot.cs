using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
public class UIMailSlot : MonoBehaviour {

    public Image mailIconImage;
    public Text senderText;
    public Text titleText;
    public Text messageText;
    public Image itemIconImage;
    public Text itemAmountText;
    public Text lifeTimeText;


    /// <summary> 메일고유 ID </summary>
    public string mailID { get; private set; }
    /// <summary> 메일 아이콘 </summary>
    public string mailIcon { get; private set; }
    /// <summary> 보낸사람 </summary>
    public string sender { get; private set; }
    /// <summary> 메일 제목 </summary>
    public string title { get; private set; }
    /// <summary> 메일 내용 </summary>
    public string message { get; private set; }
    /// <summary> 첨부 아이템 ID </summary>
    public string itemID { get; private set; }
    /// <summary> 첨부 아이템 수량 </summary>
    public string itemAmount { get; private set; }
    /// <summary> 첨부 아이템 아이콘 </summary>
    public string itemIcon { get; private set; }
    /// <summary> 메일 수신 시간 </summary>
    public string recievedTime { get; private set; }
    /// <summary> 메일 보관 시간 </summary>
    public string lifeTime { get; private set; }
    /// <summary> 아이템 타입(별도 표기 체크용) /summary>
    public string itemType { get; private set; }

   
    public void SlotDataInit(MailData data)
    {
        mailID = data.mailID;
        sender = data.sender;
        title = data.title;
        message = data.message;
        itemID = data.itemID;
        itemAmount = data.itemAmount;
        itemType = data.itemType;
        recievedTime = data.recievedTime;
        lifeTime = data.lifeTime; 
        SlotUIInit();
    }
    void SlotUIInit()
    {
        
        if (itemType == "buff")
        {
            string spriteName = "ShopIconGameSpeedx2";
            AssetLoader.AssignImage(itemIconImage, "sprite/product", "Atlas_Product", spriteName);
        }
        else if (itemType == "draw")
        {
            if (GameDataManager.heroBaseDataDic.ContainsKey(itemID))
            {
                string spriteName = GameDataManager.heroBaseDataDic[itemID].image;
                AssetLoader.AssignImage(itemIconImage, "sprite/hero", "Atlas_HeroImage", spriteName);
            }
        }
        else if (itemType == "money")
        {
            if(GameDataManager.moneyBaseDataDic.ContainsKey(itemID))
            {
                string spriteName = GameDataManager.moneyBaseDataDic[itemID].spriteName;
                AssetLoader.AssignImage(itemIconImage, "sprite/product", "Atlas_Product", spriteName);
            }
        }

        senderText.text = sender;

        LocalizeDynamicText dynamicTextTitle = titleText.gameObject.AddComponent<LocalizeDynamicText>();
        dynamicTextTitle = new LocalizeDynamicText(titleText);
        dynamicTextTitle.InitLocalizeText(title);

        LocalizeDynamicText dynamicTextMsg = messageText.gameObject.AddComponent<LocalizeDynamicText>();
        dynamicTextMsg = new LocalizeDynamicText(messageText);
        dynamicTextMsg.InitLocalizeText(message);

        //titleText.text = title;
        //messageText.text = message;

        if (itemType == "buff")
        {
            itemAmountText.text = Mathf.Floor(int.Parse(itemAmount) / 86400) + "Days";
        }
        else
        {
            itemAmountText.text = string.Format("{0:#,###}", Convert.ToInt32(itemAmount));// itemAmount;
        }

        ShowRemainTime();
    }

    /// <summary> 남은 시간 세팅 </summary>
    void ShowRemainTime()
    {
        // 남은시간 계산용 초기화
        DateTime dateTime = new DateTime();
        DateTime.TryParse(recievedTime, out dateTime);

        DateTime _recievedTime = dateTime;
        TimeSpan lifeTimeSpan = TimeSpan.Zero;

        // Default로 0일때는 남은기간 받은날짜로부터 7일 그외는 해당 라이프타임(초)가 받은날짜로부터의 남은날짜
        if (lifeTime.Equals(0))
        {
            lifeTimeSpan = _recievedTime.AddDays(7).Subtract(DateTime.Now);
        }
        else
        {
            lifeTimeSpan = _recievedTime.AddSeconds(int.Parse(lifeTime)).Subtract(DateTime.Now);
        }

        // 분 시간 일수를 구함
        int lifeSec = (int)lifeTimeSpan.Seconds;
        int lifeMin = (int)lifeTimeSpan.Minutes;
        int lifeHour = (int)lifeTimeSpan.Hours;
        int lifeDay = (int)lifeTimeSpan.Days;

        //기간
        if (int.Parse(lifeTime) > -1)
        {
            if (lifeDay > 0 && lifeHour <= 0)
                lifeTimeText.text = lifeDay.ToString() + "Day";
            else if (lifeDay > 0)
                lifeTimeText.text = lifeDay.ToString() + "Day" + lifeHour.ToString() + "Hour";
            else if (lifeHour > 0)
                lifeTimeText.text = lifeHour.ToString() + "Hour" + lifeMin.ToString() + "Minute";
            else if (lifeMin > 0)
                lifeTimeText.text = lifeMin.ToString() + "Minute";
            else if (lifeSec > 0)
                lifeTimeText.text = "1 Minute";
            else
                lifeTimeText.text = "Delete";
        }
        else
        {
            lifeTimeText.text = "???";
        }
    }



    public void OnClickReceiveItemButton()
    {
        UIMail.Instance.OnClickReceiveButton(mailID);
    }

}
