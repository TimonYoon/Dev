using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using KingdomManagement;

/// <summary> 생산라인 슬롯 UI 구현 </summary>
public class UIProductionSlot : MonoBehaviour {

    Item product
    {
        get
        {
            return productionData.product;
        }
        set
        {
            productionData.product = value;
        }
    }

    public UIProductSlot productSlot = null;

    [Header("생산품 교체")]

    public GameObject buttonChangeProduct;

    [Header("영웅배치")]

    public GameObject deployHeroListParent;

    List<TerritoryDeployedHeroSlot> heroSlotPool = new List<TerritoryDeployedHeroSlot>();

    //Storage.StoredItemInfo itemData;

    public ProductionData productionData { get; private set; }

    public UITextMoveUp productionObject;


    public GameObject lockPanel;
    public Text textOpenDescription;

    

    
    public void InitSlot(ProductionData data)
    {
        productionData = data;
        productionData.onChangedHeroList += OnChangedDeployHeroData;
        productionData.onProduce += OnProduce;

        

        User.onChangedLevel += OnChangedLevel;
        lockPanel.SetActive(productionData.baseData.openLevel > User.Instance.userLevel);
        textOpenDescription.text = "왕국레벨 [" + productionData.baseData.openLevel + "] 달성시 사용가능";


        heroSlotPool = new List<TerritoryDeployedHeroSlot>(deployHeroListParent.GetComponentsInChildren<TerritoryDeployedHeroSlot>());
        for (int i = 0; i < productionData.heroList.Count; i++)
        {
            heroSlotPool[i].InitSlot(productionData.heroList[i]);
        }

        if(productionData.product == null)
        {
            if (productionData.productionLineID == "productionLine_01")
                ApplyProduct(ProductManager.Instance.productList.Find(x => x.id == "food_001"));
            else
                ApplyProduct();
        }
        else
        {
            ApplyProduct(productionData.product);
        }

        
    }

    void OnChangedLevel()
    {
        lockPanel.SetActive(productionData.baseData.openLevel > User.Instance.userLevel);
        //textOpenDescription.text = "왕국레벨 [" + productionData.baseData.openLevel + "] 달성시 사용가능";
    }

    public void ApplyProduct(Item item = null)
    {
        buttonChangeProduct.SetActive(item != null);
        
        if (product != null)
        {
            product.isProduction = false;
            if (productionData.coroutineProduce != null)
            {
                productSlot.StopCoroutine(productionData.coroutineProduce);
                productionData.coroutineProduce = null;
            }
        }

        product = item;
        productSlot.gameObject.SetActive(false);
        if (product == null)
            return;

        productSlot.InitSlot(productionData);
        productSlot.gameObject.SetActive(true);
        
        product.isProduction = true;

    }

    void OnProduce()
    {
        productionObject.Show("+" + productionData.finalProductionAmount.ToStringABC());
    }


    void OnChangedDeployHeroData()
    {
        for (int i = 0; i < heroSlotPool.Count; i++)
        {
            heroSlotPool[i].InitSlot();
        }

        for (int i = 0; i < productionData.heroList.Count; i++)
        {
            heroSlotPool[i].InitSlot(productionData.heroList[i]);
        } 
    }     

    public void OnClickProductChangeButton()
    {
        if(product == null)
            UIProductListInfo.Show(OnApply);
        else
        {
            UIProductListInfo.Show(OnApply, product.id);
        }

    }

    public void OnClickDeployHeroButton()
    {
        UIDeployHeroInfo.Instance.Show(productionData.productionLineID);
    }

    void OnApply(Item data)
    {
        if(product != null)
        {
            if(product.id == data.id)
            {
                ApplyProduct();
                return;
            }
            ApplyProduct();
        }
        ApplyProduct(data);

    }
   
}
