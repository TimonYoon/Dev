using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;
using B83.ExpressionParser;

/// <summary> 영웅 보관함에 UI에 관한 역할을 한다. (영웅 슬롯생성/영웅 패널 닫기) </summary>
public class UIHeroInventory : MonoBehaviour {

    public delegate void UIHeroInventoryInitCallback();
    public static UIHeroInventoryInitCallback onHeroInventoryInitCallback;

    /// <summary> 인벤토리 열 때 발생. 모든 처리 끝낸 다음 가장 마지막에 </summary>
    public static SimpleDelegate onOpened;

    //###########################################################################################################

    public GameObject heroSlotContainerPrefab;
    public GameObject heroSlotPrefab;
    public GridLayoutGroup heroInventoryScrollViewContent;
    public GridLayoutGroup territoryheroInventoryScrollViewContent;
    public Canvas canvas;

    public GameObject heroTypeButtonPanel;

    public GameObject objTitle;
    public GameObject objListRoot;

    public Toggle toggleBattle;
    public Toggle toggleTown;

    public ScrollRect scrollBattleHero;
    public ScrollRect scrollTownHero; 

    //public Canvas popupCanvas;
    public RectTransform slotBG;
  


    //###########################################################################################################

    public static UIHeroInventory Instance;
    List<UIHeroSlot> heroSlotList = new List<UIHeroSlot>();
    static public List<UIHeroSlotContainer> heroSlotContainerList = new List<UIHeroSlotContainer>();
    List<UIHeroSlot> heroSlotPool = new List<UIHeroSlot>();
    //분리된 정렬기능을 사용하기 위한 레퍼런스
    UISortHeroSlot sortBehavior = null;

    [SerializeField]
    public GameObject heroSlotStackArea;

    /// <summary> 다음 인벤토리 열었을 때 슬롯 정보라던가 갱신해야 하는지 여부. 배틀그룹 추가 또는 삭제 되었을 때에는 필요 </summary>    
    static bool isRegisteredSorting = false;

    //public enum HeroInventorState
    //{
    //    Show,
    //    Buy,
    //    Upgrade
    //}
    //public HeroInventorState state { get; private set; }
    
    public enum InventoryTabType
    {
        NotDefined,
        Battle,
        Territory,
    }

    InventoryTabType _tabType = InventoryTabType.NotDefined;
    InventoryTabType tabType
    {
        get
        {
            return _tabType;
        }
        set
        {
            if (_tabType == value)
                return;

            _tabType = value;

            Show(value, currentHeroSlotState);

            //scrollBattleHero.gameObject.SetActive(value == InventoryTabType.Battle);
            //scrollTownHero.gameObject.SetActive(value == InventoryTabType.Territory);

            //if (onOpened != null)
            //    onOpened();
        }
    }


    public enum InventoryCurrentState
    {
        Inventory,
    }

    InventoryCurrentState _state = InventoryCurrentState.Inventory;
    InventoryCurrentState state
    {
        get { return _state; }
        set
        {
            if (_state == value)
                return;

            _state = value;


        }
    }

    public bool isInitialized { get; private set; }

    //###############################################################################################
    public void Awake()
    {
        Instance = this;


        Battle.battleGroupList.onAdd += OnChangedBattleGroupList;
        Battle.battleGroupList.onRemove += OnChangedBattleGroupList;

        //UILimitBreak.OnLimitBreakStart += Show;
        UILimitBreak.OnLimitBreakEnd += LimitBreakEnd;
        UILimitBreak.heroSlotList = heroSlotPool;

        InitSortBehavior();
    }

    //새로운 배틀그룹이 추가 또는 제거 되었을 때.
    void OnChangedBattleGroupList(BattleGroup b)
    {
        //다음 인벤토리 열었을 때 정렬 후 열기 등록. "전투중" 표시 여부 갱신 & 정렬
        isRegisteredSorting = true;
    }
    
    IEnumerator Start()
    {
        isInitialized = false;

        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = Camera.main;
        //popupCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        //popupCanvas.worldCamera = Camera.main;

        //리스트 사이즈 저장해두기. 다른 메뉴 같은데서 크기 막 변경해서 써먹거나 할 경우 초기화를 위해
        originalListSize = objListRoot.GetComponent<RectTransform>().sizeDelta;

        tabType = InventoryTabType.Battle;

        //state = HeroInventorState.Show;

        while (!HeroManager.isInitialized)
            yield return null;


        int count = 0;
        if (HeroManager.heroDataDic == null || HeroManager.heroDataDic.Count == 0)
        {
            notHeroText.gameObject.SetActive(true);
            territoryNotHeroText.gameObject.SetActive(true);
        }
        else
        {
            count = HeroManager.heroDataDic.Count;
            List<HeroData> heroList = HeroManager.heroDataDic.Values.ToList();
            for (int i = 0; i < heroList.Count; i++)
            {
                string heroID = heroList[i].heroID;
                if (heroID.EndsWith("_Hero"))
                {
                    GameObject go = Instantiate(heroSlotContainerPrefab) as GameObject;
                    go.transform.SetParent(heroInventoryScrollViewContent.transform, false);
                    go.transform.localPosition = Vector3.zero;
                    go.transform.localScale = Vector3.one;

                    UIHeroSlotContainer heroSlot = go.GetComponent<UIHeroSlotContainer>();
                    heroSlot.heroInvenID = heroList[i].id;

                    heroSlotContainerList.Add(heroSlot);
                }
                else if (heroID.EndsWith("_Territory"))
                {

                    GameObject go = Instantiate(heroSlotContainerPrefab) as GameObject;
                    go.transform.SetParent(territoryheroInventoryScrollViewContent.transform, false);
                    go.transform.localPosition = Vector3.zero;
                    go.transform.localScale = Vector3.one;

                    UIHeroSlotContainer heroSlot = go.GetComponent<UIHeroSlotContainer>();
                    heroSlot.heroInvenID = heroList[i].id;

                    heroSlotContainerList.Add(heroSlot);
                }
                continue;

                
            }
            
        }


        HeroManager.heroDataDic.onAdd += OnAddHero;
        HeroManager.heroDataDic.onRemove += OnRemoveHero;
        HeroManager.onHeroManagerInitCallback += OnHeroDataSetEnd;



        SizeControl();

        Close();

        isInitialized = true;
    }

    void OnHeroDataSetEnd(List<HeroData> list = null)
    {
        if(SceneLobby.currentState == LobbyState.Hero)
        {
            sortBehavior.SortHeroList(currentSortType);
        }
    }

    void InitSortBehavior()
    {
        GameObject sorter = new GameObject("SortBehavior");
        sorter.transform.SetParent(gameObject.transform);
        sortBehavior = sorter.AddComponent<UISortHeroSlot>();
        sortBehavior.heroContainerList = heroSlotContainerList;
    }

    /// <summary> 영웅 추가 </summary>
    /// <param name="id"></param>
    void OnAddHero(string id)
    {
        //영웅 슬롯 추가
        AddSlot(id);

        //HeroManger 초기화 끝난 이후 부터는 새로운 획득을 의미함
        if (!HeroManager.isInitialized)
            return;

        HeroData data = HeroManager.heroDataDic[id];

        //새로운 영웅 추가했다는 표현

        if (HeroManager.Instance.onNewHeroCheckerCallback != null)
            HeroManager.Instance.onNewHeroCheckerCallback(AlarmType.Hero, true);
        Debug.Log("새로운 영웅 추가 : " + data.heroName);

        
    }

    void OnRemoveHero(string id)
    {
        //영웅 슬롯 제거
        RemoveSlot(id);
    }

    Vector2 originalListSize;
    static public UIHeroSlot GetHeroSlotFromPool()
    {
        UIHeroSlot heroSlot = null;//  = Instance.heroSlotPool.Find(x => !x.gameObject.activeSelf);

        for (int i = 0; i < Instance.heroSlotPool.Count; i++)
        {
            if (Instance.heroSlotPool[i].gameObject.activeSelf == false)
            {
                heroSlot = Instance.heroSlotPool[i];
                break;
            }
                
        }

        if (!heroSlot)
        {
            GameObject go = Instantiate(Instance.heroSlotPrefab) as GameObject;
            //go.transform.SetParent(heroInventoryScrollViewContent.transform, false);
            //go.transform.localPosition = Vector3.zero;
            //go.transform.localScale = Vector3.one;
            heroSlot = go.GetComponent<UIHeroSlot>();

            Instance.heroSlotPool.Add(heroSlot);
        }

        return heroSlot;
    }

    public delegate void CheckNewHeroEarned();
    /// <summary> 새로운 영웅 확인 체크용 콜백 </summary>
    public static CheckNewHeroEarned onCheckNewHeroEarned;

    private void OnEnable()
    {
        //acquiredTimeButtonText.text = "습득순";
        //nameButtonText.text = "이름순";

        if (SceneLobby.Instance)
            SceneLobby.Instance.OnChangedMenu += OnChangedMenu;

        //HeroManager.Instance.onOpenInventory += Show;
    }

    private void OnDisable()
    {
        
        if (SceneLobby.Instance)
            SceneLobby.Instance.OnChangedMenu -= OnChangedMenu;
    }

    /// <summary> 전투/내정 영웅 탭 눌렀을 때 </summary>
    /// <param name="type">Battle, Town 과 같이 토글 컴퍼넌트에 미리 정의된 값이 넘어옴 </param>
    public void OnValueChangedToggleListType(string type)
    {
        

        if(type == "Battle")
            tabType =  InventoryTabType.Battle;
        else if(type == "Town")
            tabType = InventoryTabType.Territory;


    }

    bool isInventoryCheck = false;

    void OnChangedMenu(LobbyState state)
    {
        //if (state != LobbyState.Hero)
        //    Close();

        if(state == LobbyState.Hero)
        {
            isInventoryCheck = true;
            Show(InventoryTabType.Battle, HeroSlotState.Inventory);
            //StartCoroutine(HeroListUIInit());
        }
        else if(state == LobbyState.BattlePreparation)
        {
            Show(InventoryTabType.Battle, HeroSlotState.Battle);
        }
        else
        {
            Close();
        }
        
    }

    HeroSortingType _currentSortType = HeroSortingType.Auto;
    public HeroSortingType currentSortType { get { return _currentSortType; } set { _currentSortType = value; } }
    HeroSlotState currentHeroSlotState = HeroSlotState.Inventory;
    void Show(InventoryTabType tab = InventoryTabType.Battle, HeroSlotState state = HeroSlotState.Inventory)
    {
        currentHeroSlotState = state;
        sortToggle.isOn = false;
        SortingTypeButtonPanel.SetActive(sortToggle.isOn);

        //slotBG.offsetMin = new Vector2(slotBG.offsetMin.x, -0.25f);
        //slotBG.offsetMax = new Vector2(slotBG.offsetMax.x, 0);

        if (!sortToggle.gameObject.activeSelf)
            sortToggle.gameObject.SetActive(true);

        if (coroutineShow != null)
            return;

        if (state == HeroSlotState.Territory || state == HeroSlotState.Training)
        {
            
            sortBehavior.SortHeroList(HeroSortingType.Auto, state);
        }
        else
        {
            
            sortBehavior.SortHeroList(currentSortType, HeroSlotState.Inventory);
        } 

        if(tab == InventoryTabType.Battle)
        {
            toggleTown.isOn = false;
            toggleBattle.isOn = true;// Select();
            heroInventoryScrollViewContent.gameObject.SetActive(true);
            territoryheroInventoryScrollViewContent.gameObject.SetActive(false);
            UIHeroSlotContainer[] temp = heroInventoryScrollViewContent.GetComponentsInChildren<UIHeroSlotContainer>();

            if (temp.Length > 0)
            {
                notHeroText.gameObject.SetActive(false);
            }
            else
            {
                notHeroText.gameObject.SetActive(true);
            }
        }
        else
        {
            toggleTown.isOn = true;//Select();
            toggleBattle.isOn = false;
            heroInventoryScrollViewContent.gameObject.SetActive(false);
            territoryheroInventoryScrollViewContent.gameObject.SetActive(true);
            UIHeroSlotContainer[] temp = territoryheroInventoryScrollViewContent.GetComponentsInChildren<UIHeroSlotContainer>();

            if (temp.Length > 0)
            {
                notHeroText.gameObject.SetActive(false);
            }
            else
            {
                notHeroText.gameObject.SetActive(true);
            }
        }
        
        coroutineShow = StartCoroutine(ShowA(tab, state));
    }

    Coroutine coroutineShow = null;
    IEnumerator ShowA(InventoryTabType tab, HeroSlotState state)
    {
        //인벤토리 열릴시 NewCheck해제
        onCheckNewHeroEarned();

        SizeControl();

        //canvas.enabled = false;
        canvas.gameObject.SetActive(true);

        if (state == HeroSlotState.Battle)
        {
            objTitle.SetActive(false);

            //.isOn = true;
            scrollBattleHero.GetComponent<CanvasRenderer>().SetAlpha(0f);
            heroTypeButtonPanel.SetActive(false);
        }
        else if (state == HeroSlotState.Territory)
        {
            objTitle.SetActive(false);

            //toggleTown.Select();
            heroTypeButtonPanel.SetActive(false);
            objListRoot.GetComponent<RectTransform>().sizeDelta = originalListSize;
        }
        else if (state == HeroSlotState.Training)
        {
            objTitle.SetActive(false);
            heroTypeButtonPanel.SetActive(true);
        }
        else
        {
             
            objTitle.SetActive(true);
            heroTypeButtonPanel.SetActive(true);
            objListRoot.GetComponent<RectTransform>().sizeDelta = originalListSize;
        }

        //if(state != HeroSlotState.Internal)
        //    OnValueChangedToggleListType("Battle");
        //else
        //    OnValueChangedToggleListType("Town");


        for (int i = 0; i < heroSlotContainerList.Count; i++)
        {
            heroSlotContainerList[i].state = state; 
        }

        scrollBattleHero.gameObject.SetActive(tab == InventoryTabType.Battle);
        scrollTownHero.gameObject.SetActive(tab == InventoryTabType.Territory);

        if (onOpened != null)
            onOpened();

        yield return null;

        
        if (tab == InventoryTabType.Battle)
        {
           scrollBattleHero.normalizedPosition = new Vector2(scrollBattleHero.normalizedPosition.x, 1f);
        }
        else if (tab == InventoryTabType.Territory)
        {
            scrollTownHero.normalizedPosition = new Vector2(scrollTownHero.normalizedPosition.x, 1f);
        }
        

        if (isRegisteredSorting)
        {
            sortBehavior.SortHeroList(sortBehavior.heroSortType);
            isRegisteredSorting = false;
        }

        


        scrollBattleHero.GetComponent<CanvasRenderer>().SetAlpha(1f);
        scrollTownHero.GetComponent<CanvasRenderer>().SetAlpha(1f);

        canvas.enabled = true;

       

        
        coroutineShow = null;

        yield break;
    }

    public void UpdateHeroSlotData(string id)
    {
        UIHeroSlot uIHero = heroSlotPool.Find(x => x.id == id);
        if (uIHero != null)
            uIHero.SlotDataInit(id, HeroSlotState.Inventory);
        //heroSlotList.Find(x => x.id == id).SlotDataInit(id, HeroSlotState.Inventory);

    }

    /// <summary> 내정 화면에서 내정영웅 인벤토리 불러오기 </summary>
    public void ShowTerritoryHeroList()
    {
        //popupCanvas.gameObject.SetActive(true);
        canvas.gameObject.SetActive(true);  
        
        Show(InventoryTabType.Territory, HeroSlotState.Territory);

        OnValueChangedToggleListType("Town");

    }
    public void ShowHeroInvenForTraining()
    {
        Show(InventoryTabType.Battle, HeroSlotState.Training);
    }
    
    /// <summary> 한계돌파 중인지 여부 </summary>
    public bool isLimitBreak = false;

    public void LimitBreakEnd()
    {
        isLimitBreak = false;
        SizeControl();
    }


    /// <summary> 내정 화면에서 내정영웅 인벤토리 끄기 </summary>
    public void CloseTerritoryHeroList()
    {
        //popupCanvas.gameObject.SetActive(false);
        Close();
    }
    
    public Text notHeroText;
    public Text territoryNotHeroText;
    
    

    public void AddSlot(string key)
    {
        GameObject go = Instantiate(heroSlotContainerPrefab) as GameObject;

        HeroData heroData = HeroManager.heroDataDic[key];

        string heroID = heroData.heroID;
        if (heroID.EndsWith("_Territory"))
        {
            go.transform.SetParent(territoryheroInventoryScrollViewContent.transform, false);
           
        }
        else if(heroID.EndsWith("_Hero"))
        {
            go.transform.SetParent(heroInventoryScrollViewContent.transform, false);

        }

        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.one;

        UIHeroSlotContainer heroSlot = go.GetComponent<UIHeroSlotContainer>();
        heroSlot.heroInvenID = heroData.id;
        heroSlot.InitContainer();

        heroSlotContainerList.Add(heroSlot);

        SizeControl();

    }

    public void RemoveSlot(string key)
    {
        UIHeroSlotContainer container = heroSlotContainerList.Find(x => x.heroInvenID == key);
        UIHeroSlot slot = container.GetComponentInChildren<UIHeroSlot>();
        container.isDestroy = true;

        if (slot != null)
            slot.transform.SetParent(heroSlotStackArea.transform);
        
        heroSlotContainerList.RemoveAt(heroSlotContainerList.FindIndex(x => x.heroInvenID == key));
        Destroy(container.gameObject);

        SizeControl();
    }

    
    List<string> deleteHeroIDList = new List<string>();
    


    RectTransform battleHeroContentRect;
    RectTransform territoryHeroContentRect;

    /// <summary> Scroll content size conrtrol </summary>
    void SizeControl()
    {
        if (battleHeroContentRect == null)
            battleHeroContentRect = heroInventoryScrollViewContent.GetComponent<RectTransform>();

        if (territoryHeroContentRect == null)
            territoryHeroContentRect = territoryheroInventoryScrollViewContent.GetComponent<RectTransform>();

        // 전투영웅
        float count = heroInventoryScrollViewContent.transform.childCount;
        count /= 4;
        double quotient = System.Math.Ceiling((double)count);

        float sizeDeltaY = (heroInventoryScrollViewContent.cellSize.y + heroInventoryScrollViewContent.spacing.y) * ((int)quotient);

        battleHeroContentRect.sizeDelta = new Vector2(battleHeroContentRect.sizeDelta.x, sizeDeltaY);

        // 내정 영웅
        count = territoryheroInventoryScrollViewContent.transform.childCount;
        count /= 4;
        quotient = System.Math.Ceiling((double)count);

        sizeDeltaY = (territoryheroInventoryScrollViewContent.cellSize.y + territoryheroInventoryScrollViewContent.spacing.y) * ((int)quotient);

        territoryHeroContentRect.sizeDelta = new Vector2(territoryHeroContentRect.sizeDelta.x, sizeDeltaY);
       
    }

    public void OnClickCloseButton()
    {
        //if (UIBuildingInfo.Instance.isShowBuildingInfo)
        //{
        //    UIBuildingInfo.Instance.CloseHeroInventory();
        //    //CloseTerritoryHeroList();
        //    return;
        //}
        //if (isLimitBreak)
        //{
        //    //CancleLimitBreak();
        //    UILimitBreak.Instance.RelocationLimitBreak();
        //    return;
        //}

        
        //SceneLobby.Instance.SceneChange(LobbyState.Lobby);
        Close();
    }

    void Close()
    {

        canvas.gameObject.SetActive(false);

        if(isInventoryCheck == true)
        {
            isInventoryCheck = false;
            for (int i = 0; i < HeroManager.heroDataDic.Count; i++)
            {
                HeroData heroData = HeroManager.heroDataDic[heroSlotContainerList[i].heroInvenID];
                if (!heroData.isChecked)
                {
                    heroData.isChecked = !heroData.isChecked;
                    UpdateHeroSlotData(heroSlotContainerList[i].heroInvenID);
                }
            }
        }

        //스크롤 위치 젤 위로 초기화
        scrollBattleHero.normalizedPosition = new Vector2(scrollBattleHero.normalizedPosition.x, 1f);
    }

    public Toggle sortToggle;
    public GameObject SortingTypeButtonPanel;

    /// <summary> 정렬패널을 켜고 끄는 토글용 함수 </summary>
    public void OnClickSortToggle()
    {
        if (sortToggle.isOn)
        {
            SortingTypeButtonPanel.SetActive(sortToggle.isOn);
        }
        else
            SortingTypeButtonPanel.SetActive(sortToggle.isOn);
    }


    public void OnClickSortingType(string type)
    {
        if (type == "Auto")
            sortBehavior.SortHeroList(HeroSortingType.Auto, HeroSlotState.Inventory);
        else if (type == "AcquiredTime")
            sortBehavior.SortHeroList(HeroSortingType.AcquiredTime, HeroSlotState.Inventory);
        else if (type == "AcquiredTimeDesc")
            sortBehavior.SortHeroList(HeroSortingType.AcquiredTimeDesc, HeroSlotState.Inventory);
        else if (type == "Grade")
            sortBehavior.SortHeroList(HeroSortingType.Grade, HeroSlotState.Inventory);
        else if (type == "GradeDesc")
            sortBehavior.SortHeroList(HeroSortingType.GradeDesc, HeroSlotState.Inventory);
        else if (type == "Name")
            sortBehavior.SortHeroList(HeroSortingType.Name, HeroSlotState.Inventory);
        else if (type == "NameDesc")
            sortBehavior.SortHeroList(HeroSortingType.NameDesc, HeroSlotState.Inventory);

        sortToggle.isOn = false;
        SortingTypeButtonPanel.SetActive(sortToggle.isOn);
    }   
}
