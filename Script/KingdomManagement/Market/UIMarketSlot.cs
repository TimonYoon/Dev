using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using KingdomManagement;

public class UIMarketSlot : MonoBehaviour {

    [HideInInspector]
    public TradeData tradeData;

    [SerializeField]
    Image imageSellMaterial;

    [SerializeField]
    Text textSellAmount;

    [SerializeField]
    Image imageBuyMaterial;

    [SerializeField]
    Text textBuyAmount;

    [SerializeField]
    Text textTradeTime;

    [SerializeField]
    Image progressBar;

    public string sellMaterialID;

    public string buyMaterialID;

    private void Start()
    {
        progressBar.fillAmount = 0;
        value = tradeData.tradeTime;
    }

    public void InitMarketSlotData(TradeData data)
    {
        tradeData = data;

        sellMaterialID = tradeData.sellMaterialID;
        buyMaterialID = tradeData.buyMaterialID;

        InitMarketSlotUI();
    }

    void InitMarketSlotUI()
    {
        InitMaterialImage(imageSellMaterial, GameDataManager.itemDic[tradeData.sellMaterialID].image);
        InitMaterialImage(imageBuyMaterial, GameDataManager.itemDic[tradeData.buyMaterialID].image);

        textSellAmount.text = tradeData.sellAmount + "개";
        textBuyAmount.text = tradeData.buyAmount + "개";

        textTradeTime.text = RemainTime(tradeData.tradeTime);
    }

    void InitMaterialImage(Image img, string spriteName)
    {
        img.gameObject.SetActive(false);
        if (string.IsNullOrEmpty(spriteName))
            return;
        img.gameObject.SetActive(true);
        AssetLoader.AssignImage(img, "sprite/material", "Atlas_Material", spriteName);
         
    }

    public delegate void OnClickRemove(TradeData tradeData);
    public OnClickRemove onClickRemove;

    void OnClickRemoveButton()
    {

        //MarketManager.Instance.tradeDataList.Remove(tradeData);
        UIPopupManager.ShowYesNoPopup("거래 취소", "거래가 취소되어 교역로가 삭제됩니다\n계속하시겠습니까", ResultCancleTrade);
    }

    void ResultCancleTrade(string result)
    {
        if(result == "yes")
        {
            if (onClickRemove != null)
                onClickRemove(tradeData);
        }
    }

    public void OnClickStartTradeButton()
    {
        if (tradeData == null)
            return;

        if (tradeData.isTrade == true)
        {
            OnClickRemoveButton();
            return;
        }

        if (Storage.GetItemStoredAmount(sellMaterialID) < tradeData.sellAmount)
        {
            UIPopupManager.ShowInstantPopup("재고가 부족합니다");
            return;
        }

        if (tradeData.isTrade == false)
        {
            tradeData.TradeStart();
            value = tradeData.remainingTime;
        }
        
    }
    

    float value = 0f;
    private void Update()
    {
        if (tradeData == null)
            return;
        

        if (tradeData.isTrade)
        {
            textTradeTime.text = RemainTime(tradeData.remainingTime);
            float s = (value - tradeData.remainingTime) / value;
            progressBar.fillAmount = s;
        }
            
    }

    string RemainTime(float time)
    {
        string result = "";
        
        if (tradeData == null)
            return result;

        float totalSecond = time;

        if (totalSecond > 3600f)
        {
            int a = (int)totalSecond % 3600;

            int hour = ((int)totalSecond - a) / 3600;

            int b = (int)a % 60;

            int minute = ((int)a - b) / 60;

            result = hour + "h " + minute + "m";
        }
        else if (totalSecond > 60f)
        {
            int a = (int)totalSecond % 60;

            int minute = ((int)totalSecond - a) / 60;

            result = minute + "m " + a + "s";
        }
        else
        {
            result = totalSecond.ToString("N0") + "s";
        }


        return result;
    }
}
