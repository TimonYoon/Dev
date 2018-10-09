using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class UIPackageSlot : MonoBehaviour {
    
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
    
    /// <summary> 상점 슬롯의 ShopData </summary>
    public ShopData shopProductSlotData { get; private set; }

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

        shopProductSlotData = _itemShopData;

        InitItemSlotUI(); ;
    }

    /// <summary> 상점 Package 슬롯의 UI를 표현한다.</summary>
    public void InitItemSlotUI()
    {
        // 상품 설명 - 유저가 얻게 될 재화
        GoodsNameText.text = shopProductSlotData.goodsName;

        GoodsDescription.text = shopProductSlotData.goodsDescription;

        if (shopProductSlotData.costType == "cash")
        {
            GoodsPriceText.text = GetProductPrice(shopProductSlotData);
        }
        else if (shopProductSlotData.costType != "cash" && shopProductSlotData.price != "광고보기")
        {
            GoodsPriceText.text = string.Format("{0:#,###}", Convert.ToInt32(shopProductSlotData.price));
        }
        else
        {
            GoodsPriceText.text = shopProductSlotData.price;
        }
        InitGoodsImage(shopProductSlotData.goodsSpriteName);
        

    }
    string GetProductPrice(ShopData shopData)
    {
        if (IAPManager.storeController.products.WithID("com.funmagic.projectl." + shopData.id).metadata.localizedPriceString != null)
            return IAPManager.storeController.products.WithID("com.funmagic.projectl." + shopData.id).metadata.localizedPriceString;
        else
            return shopData.price;
    }
    void InitGoodsImage(string spritName)
    {
        GoodContentsImage.gameObject.SetActive(false);
        if (string.IsNullOrEmpty(spritName))
            return;
        GoodContentsImage.gameObject.SetActive(true);
        AssetLoader.AssignImage(GoodContentsImage, "sprite/product", "Atlas_Product", spritName);   
    }

    /// <summary> 상점 보통 슬롯 클릭 매서드 </summary>
    public void OnClickProductNormalSlotButton()
    {
        if (shopProductSlotData.category != "item" && DrawManager.Instance.drawCoroutine != null)
            return;

        UIPopupManager.ShowYesNoPopup("패키지 상품", "구매하시겠습니까?", NormalSlotYesNoResult);
    }

    void NormalSlotYesNoResult(string result)
    {
        if(result == "yes")
        {
            if (onClickItemButtonCallback != null)
                onClickItemButtonCallback(shopProductSlotData);
        }
    }

    public delegate void UIPackageSlotButtonClickCallback(ShopData shopData);
    /// <summary> 상점 아이템 구매 콜백 </summary>
    public static UIPackageSlotButtonClickCallback onClickItemButtonCallback;

 

}
