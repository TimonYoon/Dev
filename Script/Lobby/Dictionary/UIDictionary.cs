using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;


public class UIDictionary : MonoBehaviour
{

    public static UIDictionary Instance;
    

    public Canvas canvas;

    public ScrollRect battleScrollRect;
    public ScrollRect territoryScrollRect;

    public GridLayoutGroup battleScrollViewContent;
    public GridLayoutGroup territoryScrollViewContent;
    
    public GameObject heroSlotPrefab;

    public Toggle battleToggle;
    public Toggle territoryToggle;

    public Toggle filterToggle;
    public GameObject slotFilterPanel;


    public GameObject helpPanel;

    public Dictionary<string, UIDictionarySlot> heroSlotDic = new Dictionary<string, UIDictionarySlot>();
    public List<UIDictionarySlot> heroSlotList = new List<UIDictionarySlot>();

    public bool isInitialized { get; private set; }

    enum DictionaryTapType
    {
        NotDefined,
        Battle,
        Territory
    }

    DictionaryTapType _tabType = DictionaryTapType.NotDefined;
    DictionaryTapType tabType
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

            Show(tabType, DicionaryState.Default);

            //scrollBattleHero.gameObject.SetActive(value == InventoryTabType.Battle);
            //scrollTownHero.gameObject.SetActive(value == InventoryTabType.Territory);

            //if (onOpened != null)
            //    onOpened();
        }
    }

    enum HeroFilterType
    {
       All,
       Platinum,
       Gold,
       Silver,
       Bronze,
    }
    HeroFilterType heroFilterType = HeroFilterType.All;

    private void Awake()
    {
        Instance = this;
    }

    IEnumerator Start()
    {
        isInitialized = false;

        while (!SceneLobby.Instance)
            yield return null;

        SceneLobby.Instance.OnChangedMenu += OnChangedMenu;

        //tabType = DictionaryTapType.Battle;

        while (!DictionaryManager.isInitialized)
            yield return null;

        yield return StartCoroutine(DictionaryManager.Instance.InitDictionaryLevelData());

        List<HeroBaseData> heroList = DictionaryManager.heroBaseDataDic.Values.ToList();
        
        for (int i = 0; i < heroList.Count; i++)
        {
            string id = heroList[i].id;
            Transform parent = id.EndsWith("_Hero") ? battleScrollViewContent.transform : territoryScrollViewContent.transform;
            
            GameObject go = Instantiate(heroSlotPrefab, parent) as GameObject;
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;

            UIDictionarySlot heroSlot = go.GetComponent<UIDictionarySlot>();
            
            heroSlot.id = heroList[i].id;

            heroSlot.SlotDataInit(heroList[i].id, DicionaryState.Default);
            heroSlotDic.Add(heroSlot.id, heroSlot);
            heroSlotList.Add(heroSlot);

            continue;
        }

        yield return StartCoroutine(SortHeroListA());

        tabType = DictionaryTapType.Battle;
        SizeControl();
        

        isInitialized = true;
    }

    private void OnDisable()
    {
        SceneLobby.Instance.OnChangedMenu -= OnChangedMenu;
    }

    void Show(DictionaryTapType tab = DictionaryTapType.Battle, DicionaryState state = DicionaryState.Default)
    {
        if (coroutineShow != null)
            return;

       // SortHeroList();

        if (tab == DictionaryTapType.Battle)
        {
            battleScrollViewContent.gameObject.SetActive(true);
            territoryScrollViewContent.gameObject.SetActive(false);
        }
        else
        {
            battleScrollViewContent.gameObject.SetActive(false);
            territoryScrollViewContent.gameObject.SetActive(true);
        }

        //for (int i = 0; i < heroSlotContainerList.Count; i++)
        //{
        //    if(heroSlotContainerList[i].heroInvenID == HeroManager.heroDataList[i].heroID)
        //    {
        //        heroSlotContainerList[i].GetComponentInChildren<UIHeroSlot>().heroImage.color = new Color(255, 255, 255);
        //    }
        //}
        
        coroutineShow = StartCoroutine(ShowA(tab, state));
    }

    

    Coroutine coroutineShow = null;
    IEnumerator ShowA(DictionaryTapType tab, DicionaryState state)
    {
        //인벤토리 열릴시 NewCheck해제
        //onCheckNewHeroEarned();

        
            
       
        
        SizeControl();
        

        //canvas.enabled = false;
        canvas.gameObject.SetActive(true);



        for (int i = 0; i < heroSlotList.Count; i++)
        {
            heroSlotList[i].state = state;
        }

        battleScrollRect.gameObject.SetActive(tab == DictionaryTapType.Battle);
        territoryScrollRect.gameObject.SetActive(tab == DictionaryTapType.Territory);

        yield return null;

        if (tab == DictionaryTapType.Battle)
            battleScrollRect.normalizedPosition = new Vector2(battleScrollRect.normalizedPosition.x, 1f);
        else if (tab == DictionaryTapType.Territory)
            territoryScrollRect.normalizedPosition = new Vector2(territoryScrollRect.normalizedPosition.x, 1f);

       

        battleScrollRect.GetComponent<CanvasRenderer>().SetAlpha(1f);
        territoryScrollRect.GetComponent<CanvasRenderer>().SetAlpha(1f);

        canvas.enabled = true;
        
        coroutineShow = null;

        yield break;
    }

    

    RectTransform battleHeroContentRect;
    RectTransform territoryHeroContentRect;

    void SizeControl()
    {
        if (battleHeroContentRect == null)
            battleHeroContentRect = battleScrollViewContent.GetComponent<RectTransform>();

        if (territoryHeroContentRect == null)
            territoryHeroContentRect = territoryScrollViewContent.GetComponent<RectTransform>();

        // 전투영웅
        double count = battleScrollViewContent.transform.childCount;
        count /= 4;
        int quotient = (int)System.Math.Ceiling(count);
        float sizeDeltaY = (battleScrollViewContent.cellSize.y + battleScrollViewContent.spacing.y ) * (quotient);

        battleHeroContentRect.sizeDelta = new Vector2(battleHeroContentRect.sizeDelta.x, sizeDeltaY);

        // 내정 영웅
        count = territoryScrollViewContent.transform.childCount;
        count /= 4;
        quotient = (int)System.Math.Ceiling(count);
        sizeDeltaY = (territoryScrollViewContent.cellSize.y + territoryScrollViewContent.spacing.y ) * (quotient);

        territoryHeroContentRect.sizeDelta = new Vector2(territoryHeroContentRect.sizeDelta.x, sizeDeltaY);

    }
    void SizeControl(int num)
    {
        if (battleHeroContentRect == null)
            battleHeroContentRect = battleScrollViewContent.GetComponent<RectTransform>();

        if (territoryHeroContentRect == null)
            territoryHeroContentRect = territoryScrollViewContent.GetComponent<RectTransform>();

        // 전투영웅
        double count = num;
        count /= 4;
        int quotient = (int)System.Math.Ceiling(count);
        float sizeDeltaY = (battleScrollViewContent.cellSize.y + battleScrollViewContent.spacing.y) * (quotient);

        battleHeroContentRect.sizeDelta = new Vector2(battleHeroContentRect.sizeDelta.x, sizeDeltaY);

        // 내정 영웅
        count = num;
        count /= 4;
        quotient = (int)System.Math.Ceiling(count);
        sizeDeltaY = (territoryScrollViewContent.cellSize.y + territoryScrollViewContent.spacing.y) * (quotient);

        territoryHeroContentRect.sizeDelta = new Vector2(territoryHeroContentRect.sizeDelta.x, sizeDeltaY);

    }

    public void OnValueChangedToggleListType()
    {
        if (battleToggle.isOn)
        {
            tabType = DictionaryTapType.Battle;
        }
        else if(territoryToggle.isOn)
        {
            tabType = DictionaryTapType.Territory;
        }
    }
    
    void OnChangedMenu(LobbyState state)
    {
        if (SceneLobby.currentSubMenuState != SubMenuState.Dictionary)
        {
            Close();
        }
    }

    public void OnClickCloseButton()
    {
        Close();
    }

    void Close()
    {
        SceneManager.UnloadSceneAsync("Dictionary");
    }

    void SortHeroList()
    {
        if (coroutineSort != null)
            return;

        coroutineSort = StartCoroutine(SortHeroListA());
    }

    //public static SimpleDelegate onSort;
    Coroutine coroutineSort = null;
    IEnumerator SortHeroListA()
    {
        heroSlotList.Sort(SortDelegate);

        for (int i = 0; i < heroSlotList.Count; i++)
        {
            heroSlotList[i].transform.SetSiblingIndex(i);
        }

        //오브젝트 정렬 위해 한 프레임 기다려야 함
        yield return null;

        //if (onSort != null)
        //    onSort();

        coroutineSort = null;
    }

    int SortDelegate(UIDictionarySlot a, UIDictionarySlot b)
    {
        DictionaryManager.HeroDictionaryData heroDataA = DictionaryManager.heroDictionaryDataDic[a.id];
        DictionaryManager.HeroDictionaryData heroDataB = DictionaryManager.heroDictionaryDataDic[b.id];
        int gradeA = heroDataA.heroData.heroGrade;
        int gradeB = heroDataB.heroData.heroGrade;

        int result = gradeB.CompareTo(gradeA);

        if (result == 0)
            result = heroDataA.heroData.heroName.CompareTo(heroDataB.heroData.heroName);

        return result;

    }

    public void OnClickFilteringType(string type)
    {
        if (type == "All")
            FilterHeroList(HeroFilterType.All);
        else if (type == "Platinum")
            FilterHeroList(HeroFilterType.Platinum);
        else if (type == "Gold")
            FilterHeroList(HeroFilterType.Gold);
        else if (type == "Silver")
            FilterHeroList(HeroFilterType.Silver);
        else if (type == "Bronze")
            FilterHeroList(HeroFilterType.Bronze);

        filterToggle.isOn = false;
        slotFilterPanel.SetActive(filterToggle.isOn);
    }

    Coroutine coroutineFilter = null;
    void FilterHeroList(HeroFilterType filterType = HeroFilterType.All)
    {
        if (coroutineFilter != null)
            return;

        coroutineFilter = StartCoroutine(FilterHeroListA(filterType));
    }

    IEnumerator FilterHeroListA(HeroFilterType filterType)
    {
        int grade = 0;
        switch (filterType)
        {
            case HeroFilterType.All:
                
                break;
            case HeroFilterType.Platinum:
                grade = 4;
                break;
            case HeroFilterType.Gold:
                grade = 3;
                break;
            case HeroFilterType.Silver:
                grade = 2;
                break;
            case HeroFilterType.Bronze:
                grade = 1;
                break;
        }

        if(filterType == HeroFilterType.All)
        {
            for (int i = 0; i < heroSlotList.Count; i++)
            {
                heroSlotList[i].gameObject.SetActive(true);
            }
            SortHeroList();

            yield return null;

            SizeControl();
        }
        else
        {
            int num = 0;
            for (int i = 0; i < heroSlotList.Count; i++)
            {
                if (heroSlotList[i].heroData.heroGrade == grade)
                {
                    heroSlotList[i].gameObject.SetActive(true);
                    num++;
                    continue;
                }
                heroSlotList[i].gameObject.SetActive(false);
            }

            SortHeroList();
            yield return null;

            SizeControl(num);
        }

        
        coroutineFilter = null;
    }
   

    public void OnClickfilterToggle()
    {
        if (filterToggle.isOn)
        {
            slotFilterPanel.SetActive(filterToggle.isOn);
        }
        else
            slotFilterPanel.SetActive(filterToggle.isOn);
    }

    public void OnClickOpenHelpPanel()
    {
        helpPanel.SetActive(true);
    }
    public void OnClickCloseHelpPanel()
    {
        helpPanel.SetActive(false);
    }

}
