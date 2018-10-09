using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using KingdomManagement;

public class UIDeployHeroInfo : MonoBehaviour {

    public static UIDeployHeroInfo Instance;

    [SerializeField]
    GameObject deployedHeroPanel;

    [SerializeField]
    RectTransform rect;

    [Header("능력치 표현")]
    /// <summary> 생성된 슬롯 풀 </summary>
    List<GameObject> objectPool = new List<GameObject>();

    [SerializeField]
    GameObject heroAvilitySlotPrefab;

    [SerializeField]
    Transform beforePanel;

    [SerializeField]
    Transform afterPanel;

    [SerializeField]
    Color noneColor = Color.white;

    [SerializeField]
    Color upColor = Color.green;

    [SerializeField]
    Color downColor = Color.red;
    /// <summary> before에 표현된 능력치 리스트 </summary>
    List<UIHeroAbilitySlot> beforeAvilityList = new List<UIHeroAbilitySlot>();

    /// <summary> after에 표현된 능력치 리스트 </summary>
    List<UIHeroAbilitySlot> afterAvilityList = new List<UIHeroAbilitySlot>();

    double productionValue = 0;
    //float minionStorageValue = 0;
    //float buildingStorageValue = 0;


    [Header("배치된 영웅 표현")]
    [SerializeField]
    List<UIDeployHeroSlot> deployedHeroSlotList;

    /// <summary> 현재 표현중인 부서 아이디 </summary>
    public string currentPlaceID { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    IEnumerator Start()
    {
        while (!SceneLobby.Instance)
            yield return null;
        SceneLobby.Instance.OnChangedMenu += OnChangedMenu;
    }

    void OnChangedMenu(LobbyState state)
    {
        OnClickCloseButton();
    }

    
    public void Show(string placeID)
    {
        currentPlaceID = placeID;
        deployedHeroPanel.SetActive(true);

        ShowDeployedHeroList(currentPlaceID);        

        ShowHeroInventory(currentPlaceID);


        ShowBeforeAvilityList();
        ShowAfterAvilityList();
    }

    /// <summary> 배치된 영웅 리스트 보여주기 </summary>
    void ShowDeployedHeroList(string placeID)
    {        
        for (int i = 0; i < deployedHeroSlotList.Count; i++)
        {
            deployedHeroSlotList[i].gameObject.SetActive(false);
        }


        List<string> heroList = new List<string>();
        ProductionData productionData = ProductManager.Instance.productionLineDataList.Find(x => x.productionLineID == placeID);
        if (productionData != null)
        {
            for (int i = 0; i < productionData.heroList.Count; i++)
            {
                string id = productionData.heroList[i].id;
                heroList.Add(id);
            }
        }
        else
        {
            PlaceData data = TerritoryManager.Instance.myPlaceList.Find(x => x.placeID == placeID);
            for (int i = 0; i < data.heroList.Count; i++)
            {
                string id = data.heroList[i].id;
                heroList.Add(id);
            }
        }

        
        
        for (int i = 0; i < heroList.Count; i++)
        {
            deployedHeroSlotList[i].initDeployHeroSlot(heroList[i]);
            deployedHeroSlotList[i].gameObject.SetActive(true);
        }
    }

    public List<string> deployHeroIDList = new List<string>();

    /// <summary> 배치하기 눌렀을 때 </summary>
    public void OnClickDeployHeroButton()
    {
        ProductManager.DeployHero(currentPlaceID, deployHeroIDList);
        //TerritoryDeployHero.Instance.DeployHero(currentDepartmentID, deployHeroIDList);

        OnClickCloseButton();
    }

    
    /// <summary> 취소하기 눌렀을 때 </summary>
    public void OnClickCloseButton()
    {
        if(UIHeroInventory.Instance)
        {
            for (int i = 0; i < UIHeroInventory.heroSlotContainerList.Count; i++)
            {
                UIHeroInventory.heroSlotContainerList[i].isUnableToSelect = false;
            }

            UIHeroInventory.Instance.CloseTerritoryHeroList();
        }
            
        deployedHeroPanel.SetActive(false);
    }


    void ShowHeroInventory(string productionLineID)
    {
        for (int i = 0; i < UIHeroInventory.heroSlotContainerList.Count; i++)
        {
            UIHeroInventory.heroSlotContainerList[i].isUnableToSelect = false;
        }

        deployHeroIDList = new List<string>();
        CustomList<HeroData> heroList = new CustomList<HeroData>();


        for (int i = 0; i < ProductManager.Instance.productionLineDataList.Count; i++)
        {
            if(ProductManager.Instance.productionLineDataList[i].productionLineID == productionLineID)
            {
                heroList = ProductManager.Instance.productionLineDataList[i].heroList;
            }
        }

        for (int i = 0; i < TerritoryManager.Instance.myPlaceList.Count; i++)
        {
            if (TerritoryManager.Instance.myPlaceList[i].placeID == productionLineID)
            {
                heroList = TerritoryManager.Instance.myPlaceList[i].heroList;
            }
        }
       

        for (int i = 0; i < heroList.Count; i++)
        {
            deployHeroIDList.Add(heroList[i].id);
        }  

        // 선택됨 표시 하기
        if (deployHeroIDList == null || deployHeroIDList.Count == 0)
        {
            for (int i = 0; i < UIHeroInventory.heroSlotContainerList.Count; i++)
            {
                UIHeroInventory.heroSlotContainerList[i].isSelectedToTerritory = false;
            }
        }
        else
        {
            for (int i = 0; i < UIHeroInventory.heroSlotContainerList.Count; i++)
            {
                UIHeroInventory.heroSlotContainerList[i].isSelectedToTerritory = false;
            }

            for (int i = 0; i < UIHeroInventory.heroSlotContainerList.Count; i++)
            {
                for (int j = 0; j < deployHeroIDList.Count; j++)
                {
                    if (UIHeroInventory.heroSlotContainerList[i].heroInvenID == deployHeroIDList[j])
                    {
                        UIHeroInventory.heroSlotContainerList[i].isSelectedToTerritory = true;
                    }
                }
            }
        }
        DeployHeroCountChack();


        UIHeroInventory.Instance.ShowTerritoryHeroList();

        RectTransform t = UIHeroInventory.Instance.objListRoot.GetComponent<RectTransform>();
        t.sizeDelta = new Vector2(t.sizeDelta.x, -rect.sizeDelta.y - t.anchoredPosition.y);
        // 사이즈 조절

        productionValue = 1;
        //minionStorageValue = 0;
        //buildingStorageValue = 0;

        for (int i = 0; i < deployHeroIDList.Count; i++)
        {            
            productionValue += HeroManager.heroDataDic[deployHeroIDList[i]].productivePower;
            //minionStorageValue += HeroManager.heroDataDic[deployHeroIDList[i]].heroGrade * 1.5f;
            //buildingStorageValue += HeroManager.heroDataDic[deployHeroIDList[i]].heroGrade * 100f;



            for (int j = 0; j < HeroManager.heroDataDic[deployHeroIDList[i]].baseData.territorySkillDataList.Count; j++)
            {
                //productionValue += HeroManager.heroDataDic[deployHeroIDList[i]].baseData.territorySkillDataList[j].value;
            }
        }
    }

    public void AddHero(string id)
    {
        if (deployHeroIDList == null)
            return;

      
        

        deployHeroIDList.Add(id);

        DeployHeroCountChack();
        OnChangedDeployedHeroList();
    }

    public void RemoveHero(string id)
    {
        if (deployHeroIDList == null)
            return;

        deployHeroIDList.Remove(id);

        DeployHeroCountChack();
        OnChangedDeployedHeroList();
    }

    void DeployHeroCountChack()
    {

        int count = deployHeroIDList.Count;
        if (count >= 3)
        {
            for (int i = 0; i < UIHeroInventory.heroSlotContainerList.Count; i++)
            {
                UIHeroSlotContainer slot = UIHeroInventory.heroSlotContainerList[i];
                HeroData heroData = slot.heroData;

                if (string.IsNullOrEmpty(heroData.placeID) || (heroData.placeID != currentPlaceID || slot.isSelectedToTerritory == false))
                    slot.isUnableToSelect = true;
                else
                    slot.isUnableToSelect = false;

            }
        }
        else
        {
            for (int i = 0; i < UIHeroInventory.heroSlotContainerList.Count; i++)
            {
                UIHeroInventory.heroSlotContainerList[i].isUnableToSelect = false;
            }
        }
    }










    void InitBeforeAvility(string name, double value)
    {
        UIHeroAbilitySlot slot = null;
        for (int i = 0; i < beforeAvilityList.Count; i++)
        {
            if (beforeAvilityList[i].abilityID == name)
            {
                beforeAvilityList[i].InitSlot(name, value, noneColor);
                slot = beforeAvilityList[i];
            }

        }
        if (slot == null)
        {
            slot = ReturnToHeroAvilitySlot().GetComponent<UIHeroAbilitySlot>();
            slot.transform.SetParent(beforePanel, false);
            slot.InitSlot(name, value, noneColor);
            beforeAvilityList.Add(slot);
        }
    }

    void InitAfterAvility(string name, double value)
    {
        UIHeroAbilitySlot slot = null;
        for (int i = 0; i < afterAvilityList.Count; i++)
        {
            if (afterAvilityList[i].abilityID == name)
            {
                Color color = ComparisonValue(name, value);
                afterAvilityList[i].InitSlot(name, value, color);
                slot = afterAvilityList[i];
            }

        }
        if (slot == null)
        {
            slot = ReturnToHeroAvilitySlot().GetComponent<UIHeroAbilitySlot>();
            slot.transform.SetParent(afterPanel, false);
            Color color = ComparisonValue(name, value);
            slot.InitSlot(name, value, color);
            afterAvilityList.Add(slot);
        }
    }

    Color ComparisonValue(string name, double value)
    {

        Color result = upColor;
        for (int i = 0; i < beforeAvilityList.Count; i++)
        {
            if(beforeAvilityList[i].abilityID == name)
            {
                if (beforeAvilityList[i].abilityValue == value)
                    result = noneColor;
                else if (beforeAvilityList[i].abilityValue > value)
                    result = downColor;
                else if (beforeAvilityList[i].abilityValue < value)
                    result = upColor;
            }
        }

        return result;
    }

    void ShowBeforeAvilityList()
    {
        for (int i = 0; i < beforeAvilityList.Count; i++)
        {
            beforeAvilityList[i].gameObject.SetActive(false);
        }
        InitBeforeAvility("생산력", productionValue);
        //InitBeforeAvility("운송력", (int)minionStorageValue);
        //InitBeforeAvility("최대저장량", (int)buildingStorageValue);
      
    }

    void ShowAfterAvilityList()
    {
        for (int i = 0; i < afterAvilityList.Count; i++)
        {
            afterAvilityList[i].gameObject.SetActive(false);
        }
        InitAfterAvility("생산력", productionValue);
        //InitAfterAvility("운송력", (int)minionStorageValue);
        //InitAfterAvility("최대저장량", (int)buildingStorageValue);

        afterPanel.gameObject.SetActive(false);
        afterPanel.gameObject.SetActive(true);
    }

    GameObject ReturnToHeroAvilitySlot()
    {
        GameObject go = null;
        for (int i = 0; i < objectPool.Count; i++)
        {
            if(objectPool[i].activeSelf == false)
            {
                go = objectPool[i];
            }
        }

        if(go == null)
        {
            go = Instantiate(heroAvilitySlotPrefab);
            objectPool.Add(go);
        }

        return go;
    }
    
   

    void OnChangedDeployedHeroList()
    {
        productionValue = 1;
        //minionStorageValue = 0;
        //buildingStorageValue = 0;

        for (int i = 0; i < deployedHeroSlotList.Count; i++)
        {
            deployedHeroSlotList[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < deployHeroIDList.Count; i++)
        {
            deployedHeroSlotList[i].initDeployHeroSlot(deployHeroIDList[i]);
            deployedHeroSlotList[i].gameObject.SetActive(true);

            
            productionValue += HeroManager.heroDataDic[deployHeroIDList[i]].productivePower;
            //minionStorageValue += HeroManager.heroDataDic[deployHeroIDList[i]].heroGrade * 1.5f;
            //buildingStorageValue += HeroManager.heroDataDic[deployHeroIDList[i]].heroGrade * 100f;

            for (int j = 0; j < HeroManager.heroDataDic[deployHeroIDList[i]].baseData.territorySkillDataList.Count; j++)
            {
                //productionValue += HeroManager.heroDataDic[deployHeroIDList[i]].baseData.territorySkillDataList[j].value;
            }
        }

        ShowAfterAvilityList();
    }
}
