using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

/// <summary> 상점 : 아이템 슬롯  </summary>
public class UIShopProductSlot : MonoBehaviour
{
    // TO DO : 미란씨 사용 슬롯 프리펩
    [Header("새로운 슬롯")] 
    [SerializeField]
    Image GoodsBackGroundImage;
    [SerializeField]
    Image GoodContentsImage;
    //[SerializeField]
    //Image UnChangedImage;
    [SerializeField]
    Text GoodsNameText;
    [SerializeField]
    Image GoodsPayImage;
    [SerializeField]
    Text GoodsPriceText;
    [SerializeField]
    Text GoodsDescription;
    [SerializeField]
    GameObject GoodsCooltimePanel;
    [SerializeField]
    Text textCoolTime;
    [SerializeField]
    GameObject doubleFlag;
    [SerializeField]
    GameObject bonusPanel;
    [SerializeField]
    Text textBonus;
    

    [SerializeField]
    List<Sprite> productSpriteList;
    /// <summary> 상점 슬롯의 ShopData </summary>
    public ShopData shopProductSlotData { get; private set; }

    bool _isValidImage = false;
    public bool isValidImage
    {
        get { return _isValidImage; }
        set
        {
            _isValidImage = value;

            if (!value)
            {
                
                GoodContentsImage.CrossFadeAlpha(0f, 0f, true);
            }
            else
            {
                GoodContentsImage.CrossFadeAlpha(1f, 0.2f, true);
            }
        }
    }

    /// <summary> 상점 데이터를 매개변수로 받아서 슬롯의 변수를 채운다. </summary>
    public void InitProductSlotData(ShopData shopData)
    {
        ShopData _itemShopData = new ShopData();

        _itemShopData.id = shopData.id;
        _itemShopData.Index = shopData.Index;
        _itemShopData.category = shopData.category;
        _itemShopData.costType = shopData.costType;
        _itemShopData.price = shopData.price;
        _itemShopData.enable = shopData.enable;
        _itemShopData.productType = shopData.productType;
        _itemShopData.productAmount = shopData.productAmount;
        _itemShopData.goodsName = shopData.goodsName;
        _itemShopData.goodsDescription = shopData.goodsDescription;
        _itemShopData.goodsSpriteName = shopData.goodsSpriteName;
        _itemShopData.paySpriteName = shopData.paySpriteName;
        _itemShopData.doubleFlag = shopData.doubleFlag;
        _itemShopData.bonusAmount = shopData.bonusAmount;

        shopProductSlotData = _itemShopData;

        InitItemSlotUI(); ; 
    }

    private void OnEnable()
    {
        GoodContentsImage.RegisterDirtyMaterialCallback(OnChangedImage);
        UIAdAlarmController.onGetFreeBoost += ActiveBoostCoolTime;
        UIAdAlarmController.onGetFreeHero += ActiveHeroCoolTime;
        UIAdAlarmController.onGetFreeRuby += ActiveRubyCoolTime;
    }

    private void OnDisable()
    {
        UIAdAlarmController.onGetFreeBoost -= ActiveBoostCoolTime;
        UIAdAlarmController.onGetFreeHero -= ActiveHeroCoolTime;
        UIAdAlarmController.onGetFreeRuby -= ActiveRubyCoolTime;
    }

    void OnChangedImage()
    {
        isValidImage = shopProductSlotData != null && GoodContentsImage.sprite != null && GoodContentsImage.sprite.name == shopProductSlotData.goodsSpriteName;
        //Debug.Log(heroImage.sprite + ", " + isValidImage);
    }

    /// <summary> 상점 Package 슬롯의 UI를 표현한다.</summary>
    public void InitItemSlotUI()
    {
        // 상품 설명 - 유저가 얻게 될 재화
        GoodsNameText.text = shopProductSlotData.goodsName;

        GoodsDescription.text = shopProductSlotData.goodsDescription;

        if (!string.IsNullOrEmpty(shopProductSlotData.doubleFlag))
            doubleFlag.SetActive(true);
        else
            doubleFlag.SetActive(false);

        if(!string.IsNullOrEmpty(shopProductSlotData.bonusAmount))
        {
            textBonus.text = shopProductSlotData.bonusAmount;
            bonusPanel.SetActive(true);
        }
        else
        {
            bonusPanel.SetActive(false);
        }

       
        if(shopProductSlotData.costType == "cash")
        {
            GoodsPriceText.text = GetProductPrice(shopProductSlotData);
        }
        else if (shopProductSlotData.costType != "cash")
        {
            if(shopProductSlotData.category != "nickname" && shopProductSlotData.price != "광고보기")
            {
                GoodsPriceText.text = string.Format("{0:#,###}", Convert.ToInt32(shopProductSlotData.price));
            }
            else if (shopProductSlotData.category == "nickname" && User.Instance.changeNickname < 1)
            {
                GoodsPriceText.text = "1회 무료";
            }
            else
            {
                GoodsPriceText.text = shopProductSlotData.price;
            }
        }
        
        InitGoodsImage(shopProductSlotData.goodsSpriteName);
        InitPayImage(shopProductSlotData.paySpriteName);
        // GoodsCoolTimeText.text = shopProductSlotData.goodsCoolTime;
        // TO DO : 넣어줘야할 애가 있으면 넣어준다. - 번들로 관리해줘야함
        //GoodsBackGroundImage = Resources.Load<Image>(""+ shopProductSlotData.goodsSpriteName);

        // TODO : 이벤트 슬롯 - 그래픽 UI 변경하면서 구조가 바뀌어서 다시 만들어야함.
        //if (int.Parse(shopProductSlotData.goodsCoolTime) >0)
        //{
        //    EventSlotGameObject.SetActive(true);
        //}
        //else
        //{
        //    EventSlotGameObject.SetActive(false);
        //}
        
        
        if(shopProductSlotData.id == "goods_buff_001")
        {
            ActiveBoostCoolTime();
        }
        else if(shopProductSlotData.id == "goods_item_005")
        {
            ActiveHeroCoolTime();
        }
        else if(shopProductSlotData.id == "goods_diamond_000")
        {
            ActiveRubyCoolTime();
        }
        else
        {
            GoodsCooltimePanel.SetActive(false);
        }
    }
    string GetProductPrice(ShopData shopData)
    {
        return IAPManager.storeController.products.WithID("com.funmagic.projectl." + shopData.id).metadata.localizedPriceString;
    }
    void InitGoodsImage(string spritName)
    {
        GoodContentsImage.gameObject.SetActive(false);
        if (string.IsNullOrEmpty(spritName))
            return;
        GoodContentsImage.gameObject.SetActive(true);
        AssetLoader.AssignImage(GoodContentsImage, "sprite/product", "Atlas_Product", spritName);
        //for (int i = 0; i < productSpriteList.Count; i++)
        //{
        //    if(productSpriteList[i].name == spritName)
        //    {
        //        GoodContentsImage.gameObject.SetActive(true);
        //        GoodContentsImage.sprite = productSpriteList[i];
        //        break;
        //    }
        //}        
    }
    void InitPayImage(string spritName)
    {
        GoodsPayImage.gameObject.SetActive(false);
        
        if (string.IsNullOrEmpty(spritName))
            return;
        GoodsPayImage.gameObject.SetActive(true);
        AssetLoader.AssignImage(GoodsPayImage, "sprite/product", "Atlas_Product", spritName);
        //for (int i = 0; i < productSpriteList.Count; i++)
        //{
        //    if (productSpriteList[i].name == spritName)
        //    {
        //        GoodsPayImage.gameObject.SetActive(true);
        //        GoodsPayImage.sprite = productSpriteList[i];
        //        break;
        //    }
        //}
    }

    /// <summary> 상점 보통 슬롯 클릭 매서드 </summary>
    public void OnClickProductNormalSlotButton()
    {
        if (shopProductSlotData.category != "item" && DrawManager.Instance.drawCoroutine != null)
            return;
            
        UIPopupManager.ShowYesNoPopup("Item 상품", "구매하시겠습니까?", NormalSlotYesNoResult);
    }
    

    void ActiveBoostCoolTime()
    {
        if (shopProductSlotData.id == "goods_buff_001" && UIAdAlarmController.freeBoostRemainTime > 0f)
        {
            GoodsCooltimePanel.SetActive(true);
        }
        else
        {
            GoodsCooltimePanel.SetActive(false);
        }

    }

    void ActiveHeroCoolTime()
    {
        if (shopProductSlotData.id == "goods_item_005" && UIAdAlarmController.freeHeroRemainTime > 0f)
        {
            GoodsCooltimePanel.SetActive(true);
        }
        else
        {
            GoodsCooltimePanel.SetActive(false);
        }
    }

    void ActiveRubyCoolTime()
    {
        if (shopProductSlotData.id == "goods_diamond_000" && UIAdAlarmController.freeRubyRemainTime > 0f)
        {
            GoodsCooltimePanel.SetActive(true);
        }
        else
        {
            GoodsCooltimePanel.SetActive(false);
        }

    }

    public delegate void UIShopItemSlotButtonClickCallback(ShopData shopData);
    /// <summary> 상점 아이템 구매 콜백 </summary>
    public static UIShopItemSlotButtonClickCallback onClickItemButtonCallback;

    void NormalSlotYesNoResult(string result)
    {
        if (result == "yes")
        {

            UIShop.Instance.loadingPanel.SetActive(true);
            // TO DO : 구매할 거라는 콜백
            if (onClickItemButtonCallback != null)
                onClickItemButtonCallback(shopProductSlotData);

            if (shopProductSlotData.id == "goods_buff_001" || shopProductSlotData.id == "goods_item_005" || shopProductSlotData.id == "goods_diamond_000")
                InitItemSlotUI();

            
            //GoodsCooltimePanel.SetActive(true);
        }
    }

    /// <summary> 상점 이벤트 슬롯 (무료 구매) 클릭 매서드</summary>
    public void OnClickProductEventSlotButton()
    {
        // TO DO : 팝업을 띄우고 Yes/No에 따른 실행을 해줘야 한다.
        UIPopupManager.ShowYesNoPopup("무료 뽑기", "구매하시겠습니까?", EventSlotYesNoResult);

    }

    public delegate void UIShopItemEventSlotButtonCallback(ShopData shopData);
    /// <summary> 상점 광고보기 콜백 </summary>
    public static UIShopItemEventSlotButtonCallback onClickItemEventButtonCallback;

    void EventSlotYesNoResult(string result)
    {
        if(result == "yes")
        {
            // TO DO : 광고를 보겠다는 콜백
            if (onClickItemEventButtonCallback != null)
                onClickItemEventButtonCallback(shopProductSlotData);
            
        }
    }
    string hour;
    string min;
    string sec;

    private void Update()
    {
        if(shopProductSlotData.id == "goods_buff_001" && UIAdAlarmController.freeBoostRemainTime > 0f)
        {
            float time = UIAdAlarmController.freeBoostRemainTime;
            textCoolTime.text = time.ToStringTime();
        }
        if(shopProductSlotData.id == "goods_item_005" && UIAdAlarmController.freeHeroRemainTime > 0f)
        {
            float time = UIAdAlarmController.freeHeroRemainTime;
            textCoolTime.text = time.ToStringTime();
        }
        if (shopProductSlotData.id == "goods_diamond_000" && UIAdAlarmController.freeRubyRemainTime > 0f)
        {
            float time = UIAdAlarmController.freeRubyRemainTime;
            textCoolTime.text = time.ToStringTime();
        }
    }


    //void RemainTime(float time)
    //{
    //    if (time >= 86400f)
    //    {
    //        if ((time / 86400) < 10)
    //        {
    //            min = Mathf.Floor(time / 86400).ToString("0");
    //        }
    //        else
    //        {
    //            min = Mathf.Floor(time / 86400).ToString("00");
    //        }

    //        textCoolTime.text = min + " 일";
    //    }
    //    else if (time < 86400f && time >= 3600f)
    //    {
    //        hour = Mathf.Floor(time / 3600).ToString("00");
    //        min = "";
    //        sec = "";

    //        textCoolTime.text = hour + " 시간";
    //    }
    //    else
    //    {
    //        min = Mathf.Floor(time / 60).ToString("00");
    //        sec = Mathf.Floor(time % 60).ToString("00");

    //        textCoolTime.text = min + ":" + sec;
    //    }
    //}
}
