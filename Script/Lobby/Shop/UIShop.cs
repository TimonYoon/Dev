using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

/// <summary> 상점 타입 </summary>
public enum ShopType
{
    Hero,
    Gold,
    Ruby,
    Buff,
    Package,
}

/// <summary> 상점에 관한 UI처리 </summary>
public class UIShop : MonoBehaviour
{
    public static UIShop Instance;

    [SerializeField]
    Canvas canvas;
    [Header("상점 관련 프리펩")]
    [SerializeField]
    GameObject shopProductSlotPrefab;


    [Header("상점 슬롯 생성 위치")]
    [SerializeField]
    GridLayoutGroup shopScrollViewContect;
    [SerializeField]
    GameObject scrollView;

    
    [Header("패널 게임 오브젝트")]
    [SerializeField]
    GameObject itemPanel;

    [Header("구매 확인 팝업")]
    public GameObject loadingPanel;

    public Transform pivotDrawTicket;

    /// <summary> 상점 아이템 슬롯 리스트 </summary>
    List<UIShopProductSlot> shopItemSlotList;
    /// <summary> 상점 재화 골드 리스트 </summary>
    List<UIShopProductSlot> shopGoldSlotList;

    /// <summary> 상점 다이아몬드 슬롯 리스트 </summary>
    List<UIShopProductSlot> shopDiamondSlotList;

    /// <summary> 상점 버프 슬롯 리스트 </summary>
    List<UIShopProductSlot> shopBuffSlotList;

    private void OnEnable()
    {        
        // 서버에서 상점 데이터 완료되면 오는 콜백
        ShopDataController.Instance.onShopDataPrepared += ShopListCreate;
        // UIShopPackageSlot.onClickPackageButtonCallback += GoodsGoldChange;       
        ShopDataController.Instance.onChangedShowShop += ShowShop;
        ShopDataController.Instance.OnRemove += RemoveSlot;
        // 예외 상황 ) 재화 부족시 채널 이동함
        //MoneyManager.onMoneyManagerLackGold += GoldPanel;
        //MoneyManager.onMoneyManagerLackDiamond += RubyPanel;
        SceneLobby.Instance.OnChangedMenu += OnChangedMenu;

    }

    private void OnDisable()
    {
        ShopDataController.Instance.onShopDataPrepared -= ShopListCreate;
        ShopDataController.Instance.OnRemove -= RemoveSlot;
        //MoneyManager.onMoneyManagerLackGold -= GoldPanel;
        //MoneyManager.onMoneyManagerLackDiamond -= RubyPanel;

        SceneLobby.Instance.OnChangedMenu -= OnChangedMenu;
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = Camera.main;
    }

    void OnChangedMenu(LobbyState state)
    {
        if (state != LobbyState.Shop)
            Close();
    }
    
    void RemoveSlot(string id)
    {
        for (int i = 0; i < objectList.Count; i++)
        {
            UIShopProductSlot slot = objectList[i].GetComponent<UIShopProductSlot>();
            if (slot.shopProductSlotData.id == id)
            {
                Destroy(objectList[i].gameObject);
                objectList.Remove(objectList[i]);
            }
                
        }
    }

    void ShopListCreate(ShopDataController shopDataController)
    {
        //ShowShop(ShopType.Hero);
    }    

    void ShowProductPanel(List<ShopData> _test = null)
    {
        scrollView.SetActive(false);
        int count = 0;
        if (_test != null)
        {
            count = _test.Count;
        }
        ObjectPool(count, shopProductSlotPrefab);

        for (int i = 0; i < count; i++)
        {
            objectList[i].GetComponent<UIShopProductSlot>().InitProductSlotData(_test[i]);
            objectList[i].gameObject.SetActive(true);
        }
        SizeControl(count);

        scrollView.SetActive(true);
    }

    // TODO : 버튼 관련 매서드
    //*****************************************************************************
    public void OnClickCloseButton()
    { 
        //SceneLobby.Instance.SceneChange(LobbyState.Lobby);
        //Close();
    }
    void Close()
    {
        canvas.enabled = false;
        //SceneManager.UnloadSceneAsync("Shop");
    }

    IEnumerator WaitingForDataInitialization(ShopType type)
    {
        while (ShopDataController.Instance.isInitialized == false)
            yield return null;

        ShowShop(type);

    }
    [SerializeField]
    Toggle heroToggle;
    [SerializeField]
    Toggle GoldToggle;
    [SerializeField]
    Toggle RubyToggle;
    [SerializeField]
    Toggle BuffToggle;


    /// <summary> 원하는 상점 패널을 열어준다. </summary>
    public void ShowShop(ShopType type)
    {
        if(ShopDataController.Instance.isInitialized == false)
        {
            StopCoroutine("WaitingForDataInitialization");
            StartCoroutine("WaitingForDataInitialization", type);
            return;
        }

        canvas.enabled = true;
        switch (type)
        {
            
            case ShopType.Hero:
                {
                    heroToggle.isOn = true;
                    ShowProductPanel(ShopDataController.shopHeroDataList);
                    break;
                }
            case ShopType.Gold:
                {
                    GoldToggle.isOn = true;
                    ShowProductPanel(ShopDataController.shopGoldDataList);
                    break;
                }
            case ShopType.Ruby:
                {
                    RubyToggle.isOn = true;
                    ShowProductPanel(ShopDataController.shopRubyDataList);
                    break;
                }
            case ShopType.Buff:
                {
                    BuffToggle.isOn = true;
                    ShowProductPanel(ShopDataController.shopBuffDataList);
                    break;
                }
            default:
                break;
        }

    }

    /// <summary> 버튼으로 클릭했을 때 </summary>
    public void OnClickTabButton(string type)
    {
      
        switch (type)
        {
            case "Hero":
                {
                    ShowProductPanel(ShopDataController.shopHeroDataList);
                    break;
                }
            case "Gold":
                {
                    ShowProductPanel(ShopDataController.shopGoldDataList);
                    break;
                }
            case "Ruby":
                {
                    ShowProductPanel(ShopDataController.shopRubyDataList);
                    break;
                }
            case "Buff":
                {
                    ShowProductPanel(ShopDataController.shopBuffDataList);
                    break;
                }
            default:
                break;
        }



        //소지한 뽑기권은 뽑기 메뉴에서만 보여짐
        pivotDrawTicket.gameObject.SetActive(type == "Hero");
    }
   
    //**********************************************************************



    List<Transform> objectList = new List<Transform>();
    void ObjectPool(int count, GameObject prefab)
    {
        RectTransform content = shopScrollViewContect.GetComponent<RectTransform>();
        int poolCount = 0;
        if (objectList.Count > 0)
        {
            for (int i = 0; i < objectList.Count; i++)
            {
                objectList[i].gameObject.SetActive(false);
            }
            poolCount = objectList.Count;
        }


        if (count > poolCount)
        {
            for (int i = 0; i < (count - poolCount); i++)
            {
                GameObject slot = Instantiate(prefab);
                slot.transform.SetParent(content, false);
                slot.SetActive(false);
                objectList.Add(slot.transform);
                ShopDataController.Instance.shopSlotList.Add(slot.GetComponent<UIShopProductSlot>());
            }
        }
    }

    void SizeControl(float count)
    {
        shopScrollViewContect.GetComponent<RectTransform>().sizeDelta = new Vector2(0, ((shopScrollViewContect.spacing.y + shopScrollViewContect.cellSize.y) * (count)));

    }
}
