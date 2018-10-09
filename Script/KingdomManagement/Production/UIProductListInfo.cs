using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using KingdomManagement;
using CodeStage.AntiCheat.ObscuredTypes;

/// <summary> 생산품 리스트 표현</summary>
public class UIProductListInfo : MonoBehaviour {

    public static UIProductListInfo Instance;

    public GameObject productListPanel;

    public GameObject territoryProductSlotPrefab;

    public RectTransform rectProductSlotParent;


    List<Item> foodList = new List<Item>();
    List<Item> etcList = new List<Item>();

    private void Awake()
    {
        Instance = this;
        Close();
    }

    IEnumerator Start()
    {
        while (SceneLobby.Instance == false)
            yield return null;

        SceneLobby.Instance.OnChangedMenu += OnChangedMenu;

        while (ProductManager.Instance == false)
            yield return null;


        while (ProductManager.Instance.isInitialized == false)
            yield return null;


        for (int i = 0; i < ProductManager.Instance.productList.Count; i++)
        {
            if (ProductManager.Instance.productList[i].category == "food")
            {
                foodList.Add(ProductManager.Instance.productList[i]);
            }
            else
            {
                etcList.Add(ProductManager.Instance.productList[i]);
            }
        }
    }

    void OnChangedMenu(LobbyState state)
    {
        if (state != LobbyState.Territory)
            Close();
    }


    string productID;
    public Action<Item> callbackProductData;
    static public void Show(Action<Item> resultProductData = null , string _productID = null)
    {
        Instance.productListPanel.SetActive(true);
        

        if(resultProductData != null)
        {
            Instance.productID = _productID;
            Instance.callbackProductData = resultProductData;
        }

        Instance.InitProductionList();
    }

    public void OnClickToggle(string type)
    {
        InitProductionList(type);
    }

    public void OnClickCloseButton()
    {
        Close();
    }

    void Close()
    {
        productListPanel.SetActive(false);
        callbackProductData = null;
        productID = "";
    }

    /// <summary> 생산품 리스트 초기화 </summary>
    void InitProductionList(string type = "food")
    {
        for (int i = 0; i < productSlotList.Count; i++)
        {
            productSlotList[i].gameObject.SetActive(false);
        }

        int slotCount = 0;
        if (type == "food")
        {
            foodList.Sort((Item a, Item b) => {
                int indexA = a.index;
                int indexB = b.index;
                return indexA.CompareTo(indexB);
            });
            for (int i = 0; i < foodList.Count; i++)
            {
                if (foodList[i].isProduction && foodList[i].id != productID)
                    continue;
                slotCount++;
                UIProductSlot slot = CreateSlot();
                slot.InitSlot(foodList[i]);
                slot.gameObject.SetActive(true);
            }
            
        }
        else
        {
            etcList.Sort((Item a, Item b) => {
                int indexA = a.index;
                int indexB = b.index;
                return indexA.CompareTo(indexB);
            });
            for (int i = 0; i < etcList.Count; i++)
            {
                if (etcList[i].isProduction && etcList[i].id != productID)
                    continue;
                slotCount++;
                UIProductSlot slot = CreateSlot();
                slot.InitSlot(etcList[i]);
                slot.gameObject.SetActive(true);
            }
        }

        SizeControl(slotCount);
    }

    List<UIProductSlot> productSlotList = new List<UIProductSlot>();

    UIProductSlot CreateSlot()
    {
        UIProductSlot slot = null;
        for (int i = 0; i < productSlotList.Count; i++)
        {
            if (!productSlotList[i].gameObject.activeSelf)
            {
                slot = productSlotList[i];
                break;
            }
        }

        if (slot == null)
        {
            GameObject go = Instantiate(territoryProductSlotPrefab);
            go.transform.SetParent(rectProductSlotParent, false);
            slot = go.GetComponent<UIProductSlot>();
            slot.onApply += OnApply;
            productSlotList.Add(slot);
        }

        return slot;
    }

    void OnApply(Item data)
    {
        if (callbackProductData != null)
            callbackProductData(data);

        Close();
    }

    void SizeControl(float count)
    {
        GridLayoutGroup gridLayoutGroup = rectProductSlotParent.GetComponent<GridLayoutGroup>();
        rectProductSlotParent.sizeDelta = new Vector2(0, (gridLayoutGroup.spacing.y + gridLayoutGroup.cellSize.y) * count);
    }
}
