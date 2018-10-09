using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using KingdomManagement;




/// <summary> 생산품 슬롯 표현 </summary>
public class UIProductSlot : MonoBehaviour {

    Item _product;
    public Item product
    {
        get { return _product; }
        set
        {
            if(product != null)
            {
                Storage.UnregisterOnChangedStoredAmountCallback(product.id, OnChangedAmount);
                product.onChangedProductionAmount -= OnChangedProductionAmount;
            }
            _product = value;

            if(_product == null && productionData != null)
            {
                productionData.onChangedProduct -= OnChangedProduct;
                productionData.onChangedProductionAmount -= OnChangedProductionAmount;
                productionData = null;
                //Debug.Log("해제");
            }
        }
    }
    ProductionData productionData;

    PlaceData placeData;

    public Text textProductName;

    public Text textProductLevel;

    public Image imageProduct;

    //public GameObject iconExclamation;

    public GameObject tipPanel;

    public Text textDescription;

    [Header("생산관련")]
    public Text textStorgaeAmount;

    public Text textProductionAmount;

    public Text textProductionTime;

    public RectTransform progressBar;
    float progressMaxValue;
    
    [Header("레벨업 부분")]
    public Button buttonLevelUp;

    public Text textLevelUpCost;

    public AudioSource soundLevelUp;

    [Header("필요재료")]
    public GameObject needMaterialPanel;

    public GameObject needMaterialSlotPrefab;

    public Transform needMaterialParent;

    [Header("배치 해제")]

    public Button buttonApply;

    public Button buttonRelease;

    public GameObject iconExclamation;

    void Start()
    {
        progressMaxValue = progressBar.sizeDelta.x;
        Storage.storedItemDic.onAdd += OnAddItemData;

        //소지 골드 변경될 때 콜백 등록. (버튼 활성/비활성 처리를 위한거)
        MoneyManager.RegisterOnChangedValueCallback(MoneyType.gold, OnChangedMoneyData);        
    }

    private void OnDisable()
    {
        product = null;
    }
    public void InitSlot(PlaceData data)
    {
        placeData = data;
        //placeData.onChangedPlaceLevel += OnChangedProductionAmount;
        placeData.onChangedProductionAmount += OnChangedProductionAmount;
        InitSlot(placeData.product);
    }
    
    public void InitSlot(ProductionData data)
    {
        productionData = data;
        productionData.onChangedProduct += OnChangedProduct;
        productionData.onChangedProductionAmount += OnChangedProductionAmount;
        //Debug.Log("콜백 등록됨");
        gameObject.SetActive(productionData.product != null);

        InitSlot(productionData.product);

        buttonApply.gameObject.SetActive(false);
        buttonRelease.gameObject.SetActive(false);
        
    }
    void OnChangedProduct()
    {
        if (productionData.product == null)
            gameObject.SetActive(false);
        else
            InitSlot(productionData.product);
    }

    public void InitSlot(Item data)
    {
        product = data;
        product.onChangedProductionAmount += OnChangedProductionAmount;

        textProductName.text = product.name;
        textProductLevel.text = "Lv " + product.level.ToString();
        TerritoryManager.Instance.ChangeMaterialImage(imageProduct, product.image);
        textDescription.text = product.description;
        // ================

        Storage.RegisterOnChangedStoredAmountCallback(product.id, OnChangedAmount);
        

        textStorgaeAmount.text = Storage.GetItemStoredAmount(product).ToStringABC();// itemData == null ? "0" : itemData.amount.ToStringABC();        
        float productionTime = product.productionTime;
        textProductionTime.text = productionTime.ToStringTime();

        OnChangedProductionAmount();
        //string buffText = "";
        //double productionAmount = 0;
        //double buffValue = 0;
        //if (productionData != null)
        //{
        //    buffValue = productionData.placeBuffValue;
        //    productionAmount = productionData.finalProductionAmount;
        //}            
        //else if (placeData != null)
        //{
        //    buffValue = placeData.placeBuffValue;
        //    productionAmount = placeData.finalProductionAmount;
        //}
        //else
        //{
        //    buffValue = product.productionAmount * (product.placeBuffValue /100);
        //    productionAmount = product.productionAmount;
        //}

        //// ===============
        //if (buffValue > 1)
        //    buffText = "<color=#00ff00ff>(+ " + buffValue.ToStringABC() + ")</color>";
        //textProductionAmount.text = productionAmount.ToStringABC() + buffText;
        //buttonLevelUp.interactable = product.canUpgrade;
        //textLevelUpCost.text = product.upgradeCost.ToStringABC();

        // ================

        for (int i = 0; i < ingredientSlotList.Count; i++)
        {
            ingredientSlotList[i].gameObject.SetActive(false);
        }

        needMaterialPanel.SetActive(product.ingredientList.Count > 0);

        if (product.ingredientList.Count > 0)
        {
            for (int i = 0; i < product.ingredientList.Count; i++)
            {
                UIIngredientSlot slot = CreateIngredientSlot();
                slot.gameObject.SetActive(true);
                if (productionData == null)
                    slot.Init(product, product.ingredientList[i].item, product.ingredientList[i].count);
                else
                    slot.Init(productionData, product.ingredientList[i].item, product.ingredientList[i].count);
            }
        }

        // ================
        buttonApply.gameObject.SetActive(!product.isProduction);
        buttonRelease.gameObject.SetActive(product.isProduction);
    }
    
    void OnChangedProductionAmount()
    {
        if (product != null)
        {
            textProductLevel.text = "Lv " + product.level.ToString();
            double tempAmount = product.productionAmount;
            textProductionAmount.text = tempAmount.ToStringABC();
            buttonLevelUp.interactable = product.canUpgrade;
            textLevelUpCost.text = product.upgradeCost.ToStringABC();
        }
        
        string buffText = "";
        double productionAmount = 0;
        double buffValue = 0;

        double heroSkillValue = 0;
        string skillText = "";

        if (productionData != null)
        {
            buffValue = productionData.placeBuffValue;
            heroSkillValue = productionData.heroSkillCollectPower;
            productionAmount = productionData.finalProductionAmount;
        }
        else if (placeData != null)
        {
            buffValue = placeData.placeBuffValue;
            heroSkillValue = placeData.heroSkillCollectPower;
            //Debug.Log(placeData.placeID + ", Show UI hero Skill Power : " + heroSkillValue);
            productionAmount = placeData.finalProductionAmount;
        }
        else
        {
            buffValue = product.placeBuffValue;
            productionAmount = product.productionAmount;
        }
        //if(product != null)
            //Debug.Log(product.id+ ") buffValue : " + buffValue);
        // ===============
        if (buffValue > 1)
            buffText = "<color=#00ff00ff>(+ " + buffValue.ToStringABC() + ")</color>";
        if(heroSkillValue >1)
            skillText = "<color=#85B8FFFF>(+ " + heroSkillValue.ToStringABC() + ")</color>";
        textProductionAmount.text = productionAmount.ToStringABC() + buffText + skillText;
    }
    Coroutine coroutineScaleText = null;
    IEnumerator ScaleText(RectTransform text)
    {
        text.localScale = new Vector2(1.6f, 1.4f);

        float startTime = Time.time;
        while (Time.time - startTime < 1f)
        {
            text.localScale = Vector3.Lerp(text.localScale, Vector3.one, 4f * Time.deltaTime);

            if (text.localScale.x < 0.01f)
                break;

            yield return null;
        }

        text.localScale = Vector3.one;

        coroutineScaleText = null;
    }
    void OnAddItemData(string itemID)
    {
        if (product == null || itemID != product.id)
            return;
        Storage.RegisterOnChangedStoredAmountCallback(product.id, OnChangedAmount);
        textStorgaeAmount.text = Storage.GetItemStoredAmount(product).ToStringABC();
    }
    void OnChangedMoneyData()
    {
        if (product == null)
            return;
        buttonLevelUp.interactable = product.canUpgrade;
    }

    void OnChangedAmount()
    {
        textStorgaeAmount.text = Storage.GetItemStoredAmount(product).ToStringABC();
        bool isEnableEffect = false;
        if (productionData != null)
        {
            isEnableEffect = true;
        }
        if (placeData != null)
        {
            isEnableEffect = placeData.placeState == PlaceState.MyPlace;
        }

        if (isEnableEffect)
        {
            if (coroutineScaleText != null)
            {
                StopCoroutine(coroutineScaleText);
                coroutineScaleText = null;
            }
            coroutineScaleText = StartCoroutine(ScaleText(textStorgaeAmount.rectTransform));
        }

        
    }

    public void OnShowTipPanel()
    {
        tipPanel.SetActive(true);
    }

    public void OnHideTipPanel()
    {
        tipPanel.SetActive(false);
    }

    /// <summary> 업그레이드 버튼 눌렀을 때 </summary>
    public void OnClickUpgrade()
    {
        product.Upgrade();
        soundLevelUp.Play();
    }

    public delegate void OnApply(Item data);
    public OnApply onApply;

    public void OnClickApplyButton()
    {
        if (onApply != null)
            onApply(product);
    }

    public void OnClickReleaseButton()
    {

    }

    List<UIIngredientSlot> ingredientSlotList = new List<UIIngredientSlot>();

    UIIngredientSlot CreateIngredientSlot()
    {
        UIIngredientSlot slot = null;
        for (int i = 0; i < ingredientSlotList.Count; i++)
        {
            if (!ingredientSlotList[i].gameObject.activeSelf)
            {
                slot = ingredientSlotList[i];
                break;
            }
        }
        if (slot == null)
        {
            GameObject go = Instantiate(needMaterialSlotPrefab, needMaterialParent, false);
            slot = go.GetComponent<UIIngredientSlot>();
            ingredientSlotList.Add(slot);
        }
        return slot;
    }

    void Update()
    {
        if (product == null)
            return;

        //if (productionData == null)
        //{
        //    progressBar.sizeDelta = new Vector2(0, progressBar.sizeDelta.y);
        //    iconExclamation.SetActive(false);
        //}
      



        float progressValue = 0;

        if (productionData != null)
        {
            if (productionData.coroutineProduce == null)
                productionData.coroutineProduce = StartCoroutine(productionData.Produce());

            progressValue = productionData.progressValue;

        }
        else if(placeData != null)
        {
            progressValue = placeData.progressValue;
        }



        
        progressBar.sizeDelta = new Vector2(progressMaxValue * progressValue, progressBar.sizeDelta.y);
        iconExclamation.SetActive(false);
    }
}
