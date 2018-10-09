using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using KingdomManagement;


/// <summary> 생산리스트 슬롯에 생산에 필요한 재료표현 </summary>
public class UIIngredientSlot : MonoBehaviour {

    public Image materialImage;

    public Text amountText;

    double ingredientAmount;

    Storage.StoredItemInfo itemData;

    /// <summary> 나는 누구의 재료인가 </summary>
    ProductionData productionLineData;

    Item product;

    /// <summary> 나 </summary>
    Item ingredient;

   
    void OnEnable()
    {
        if(Storage.isInitialized)
            Storage.storedItemDic.onAdd += OnAddStorageItem;
    }

    void OnDisable()
    {
        if(productionLineData != null)
        {
            productionLineData.onChangedProductionAmount -= OnChangedProductionAmount;
            productionLineData = null;
        }

        if(product != null)
        {
            product.onChangedProductionAmount -= OnChangedProductionAmount;
            product = null;
        }

        if (Storage.isInitialized)
            Storage.storedItemDic.onAdd -= OnAddStorageItem;

        ingredient = null;
    }
    void Start()
    {
        gradientColor = amountText.GetComponent<GradientColor>();
        if(gradientColor != null)
        {
            originalFirstColor = gradientColor.firstColor;
            originalSecondColor = gradientColor.secondColor;
        }
        UpdateAmountText();
    }

    /// <summary> 초기화 </summary>
    /// <param name="productionLineData"> 이 재료는 어떤 라인 생산품의 것인지. </param>
    /// <param name="ingredient"> 재료의 아이템 정의</param>
    /// <param name="count"> </param>
    public void Init(ProductionData _productionLineData, Item _ingredient, double count)
    {
        

        productionLineData = _productionLineData;

        productionLineData.onChangedProductionAmount += OnChangedProductionAmount;

        UpdateData(_ingredient, count);
    }

    /// <summary> 초기화 </summary>
    /// <param name="product"> 이 재료는 누구의 것인지. </param>
    /// <param name="ingredient"> 재료의 아이템 정의</param>
    /// <param name="count"> </param>
    public void Init(Item _product, Item _ingredient, double count)
    {
        product = _product;

        product.onChangedProductionAmount += OnChangedProductionAmount;

        UpdateData(_ingredient, count);
    }

    void UpdateData(Item _ingredient, double count)
    {
        ingredient = _ingredient;

        ingredientAmount = count;

        TerritoryManager.Instance.ChangeMaterialImage(materialImage, ingredient.image);

        UpdateAmountText();
        if (Storage.GetItem(ingredient.id) != null)
            Storage.RegisterOnChangedStoredAmountCallback(ingredient.id, OnChangedProductionAmount);
    }

    GradientColor gradientColor;
    Color originalFirstColor;
    Color originalSecondColor;

    void OnChangedProductionAmount()
    {
        UpdateAmountText();
    }

    void UpdateAmountText()
    {
        double count = 0;

        if (product != null)
            count = (product.productionAmount * ingredientAmount);

        if (productionLineData != null)
            count = (productionLineData.totalValue * ingredientAmount);

        amountText.text = count.ToStringABC();

        if(ingredient != null)
        {
            if(gradientColor != null)
            {
                if (Storage.GetItemStoredAmount(ingredient) < count)
                {
                    gradientColor.firstColor = Color.red;
                    gradientColor.secondColor = Color.red;
                }
                else
                {
                    gradientColor.firstColor = originalFirstColor;
                    gradientColor.secondColor = originalSecondColor;
                }                
            }
            else
            {
                if (Storage.GetItemStoredAmount(ingredient) < count)
                    amountText.color = Color.red;
                else
                    amountText.color = Color.white;
            }
            
        }
    }

    void OnAddStorageItem(string itemID)
    {
        if (ingredient == null || itemID != ingredient.id)
            return;
        
        Storage.RegisterOnChangedStoredAmountCallback(ingredient.id, OnChangedProductionAmount);
    }
}
