using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum placeInfoType
{
    OnlyInfo,
    AddPlace,
    EmptyPlace,
}

public class UIPlaceInfo : MonoBehaviour {

    public static UIPlaceInfo Instance;

    [SerializeField]
    GameObject panelPlaceInfo;

    [SerializeField]
    Text textPlaceName;

    [SerializeField]
    Text textDiscripcion;

    [SerializeField]
    Text textPlaceLevel;

    //[SerializeField]
    //GameObject remainingResourcePrefab;

    //[SerializeField]
    //HorizontalLayoutGroup horizontalLayoutContent;

    //RectTransform rectRemainingResource;

    Place place;

    [Header("영지구매")]

    [SerializeField]
    GameObject addPlaceInfopPanel;

    //[SerializeField]
    //Text textPlaceCost;

    [Header("영지구매 후")]

    [SerializeField]
    GameObject emptyPlaceInfoPanel;

    List<Image> resourcePoolList = new List<Image>();

    public Text textProductName;

    public Image imageProduct;

    public Button buttonPlaceAdd;
    public Text textAddCost;

    public Button buttonUpgrade;
    public Text textUpgradeCost;

    void Awake()
    {
        Instance = this;
    }

    IEnumerator Start()
    {
        heroSlotPool = new List<TerritoryDeployedHeroSlot>(deployHeroListParent.GetComponentsInChildren<TerritoryDeployedHeroSlot>());

        while (MoneyManager.Instance == null)
            yield return null;

        //소지 골드 변경될 때 콜백 등록. (버튼 활성/비활성 처리를 위한거)
        MoneyManager.RegisterOnChangedValueCallback(MoneyType.gold, OnChangedMoneyData);
        
        while (SceneLobby.Instance == null)
            yield return null;

        SceneLobby.Instance.OnChangedMenu += OnChangedMenu;
    }
    void OnChangedMenu(LobbyState state)
    {
        Close();
    }

    void OnChangedMoneyData()
    {
        if(MoneyManager.GetMoney(MoneyType.placeTicket).value == 0)
        {
            buttonUpgrade.interactable = false;
            buttonPlaceAdd.interactable = false;
            return;
        }
        
        if(place != null)
        {
            buttonUpgrade.interactable = MoneyManager.GetMoney(MoneyType.gold).value >= place.upgradeCost;
            buttonPlaceAdd.interactable = MoneyManager.GetMoney(MoneyType.gold).value >= place.addCost;
        }
           
    }
    public UIProductSlot productSlot;
    void OnChagedPlaceLevel()
    {
        textUpgradeCost.text = place.upgradeCost.ToStringABC();
    }

    void OnChangedDeployHeroData()
    {
        for (int i = 0; i < heroSlotPool.Count; i++)
        {
            heroSlotPool[i].InitSlot();
        }

        for (int i = 0; i < place.placeData.heroList.Count; i++)
        {
            heroSlotPool[i].InitSlot(place.placeData.heroList[i]);
        }
    }
    public void Show(string placeID, placeInfoType infoType)
    {

        panelPlaceInfo.SetActive(true);
        
        place = WorldMapController.Instance.placeDic[placeID];
        place.placeData.onChangedPlaceLevel += OnChagedPlaceLevel;
        place.placeData.onChangedHeroList += OnChangedDeployHeroData;
        productSlot.InitSlot(place.placeData);

        OnChangedDeployHeroData();


        textPlaceName.text = GameDataManager.placeBaseDataDic[placeID].name;
        string description = GameDataManager.placeBaseDataDic[placeID].placeBuffDescription.Replace("[formula]", "<color=#00ff00ff>" + place.placeData.power.ToString() + "</color>");
        textDiscripcion.text = description;
        KingdomManagement.Item baseData = GameDataManager.itemDic[GameDataManager.placeBaseDataDic[placeID].productID];
        textProductName.text = baseData.name;
        AssetLoader.AssignImage(imageProduct, "sprite/material", "Atlas_Material", baseData.image);

        

        textPlaceLevel.text = place.placeLevel.ToString();
        textAddCost.text = place.addCost.ToStringABC();
        textUpgradeCost.text = place.upgradeCost.ToStringABC();

        addPlaceInfopPanel.SetActive(infoType == placeInfoType.AddPlace);
        emptyPlaceInfoPanel.SetActive(infoType == placeInfoType.EmptyPlace);
        
       

        for (int i = 0; i < resourcePoolList.Count; i++)
        {
            resourcePoolList[i].gameObject.SetActive(false);
        }

        if (MoneyManager.GetMoney(MoneyType.placeTicket).value == 0)
        {
            buttonUpgrade.interactable = false;
            buttonPlaceAdd.interactable = false;
            return;
        }

        if (place != null)
        {
            //buttonUpgrade.interactable = MoneyManager.GetMoney(MoneyType.gold).value >= place.upgradeCost;
            //buttonPlaceAdd.interactable = MoneyManager.GetMoney(MoneyType.gold).value >= place.addCost;
            //to do : 테스트를 위해 placeTicket만 사용하여 영지 점령과 강화를 한다.
            buttonUpgrade.interactable = true;
            buttonPlaceAdd.interactable = true;
        }
    }
   


    public GameObject objectConstruct;
    public GameObject objectBuyPlace;

    [Header("영웅 배치")]
    public GameObject deployHeroListParent;

    List<TerritoryDeployedHeroSlot> heroSlotPool = new List<TerritoryDeployedHeroSlot>();

    public void OnClickDeployHero()
    {
        if(place.placeState != PlaceState.MyPlace)
        {
            UIPopupManager.ShowInstantPopup("소유하지 않은 영지 입니다.");
            return;
        }
        UIDeployHeroInfo.Instance.Show(place.placeID);
    }

    /// <summary> 영지 구매 버튼 눌렀을 때 </summary>
    public void OnClickPlaceAddButton()
    {
        if (MoneyManager.GetMoney(MoneyType.gold).value < TerritoryManager.placeCost)
        {            
            string message = MoneyManager.GetMoney(MoneyType.gold).id + "@가 부족합니다.";
            UIPopupManager.ShowInstantPopup(message);
            return;
        }

        if (MoneyManager.GetMoney(MoneyType.placeTicket).value < 1)
        {
            string message = MoneyManager.GetMoney(MoneyType.placeTicket).id + "@가 부족합니다.";
            UIPopupManager.ShowInstantPopup(message);
            return;
        }

        place.PlaceAdd();
        objectBuyPlace.SetActive(false);
        objectBuyPlace.SetActive(true);
        Close();
    }
   
    void Construction(string buildingID)
    {
        Close();
    }

    /// <summary> 영지 업그레이드 버튼 눌렀을 때 </summary>
    public void OnClickPlaceUpgradeButton()
    {
        if (MoneyManager.GetMoney(MoneyType.gold).value < TerritoryManager.placeCost)
        {
            string message = MoneyManager.GetMoney(MoneyType.gold).id + "@가 부족합니다.";
            UIPopupManager.ShowInstantPopup(message);
            return;
        }

        if (MoneyManager.GetMoney(MoneyType.placeTicket).value < 1)
        {
            string message = MoneyManager.GetMoney(MoneyType.placeTicket).id + "@가 부족합니다.";
            UIPopupManager.ShowInstantPopup(message);
            return;
        }

        place.PlaceUpgrade();
        objectConstruct.SetActive(false);
        objectConstruct.SetActive(true);
        Close();
    }

    void Update()
    {
        if (place == null)
            return;

        textPlaceLevel.text = place.placeLevel.ToString();
    }

    void Close()
    {
        OnClcikCloseButton();
    }

    public void OnClcikCloseButton()
    {
        if(place != null)
        {
            place.placeData.onChangedPlaceLevel -= OnChagedPlaceLevel;
            place.placeData.onChangedHeroList -= OnChangedDeployHeroData;
        }

        place = null;
        
        panelPlaceInfo.SetActive(false);      
    }
}
