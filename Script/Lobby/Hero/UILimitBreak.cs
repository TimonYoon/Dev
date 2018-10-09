using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using B83.ExpressionParser;
using LitJson;

public class UILimitBreak : MonoBehaviour {

    public static UILimitBreak Instance;
    
    //################################################################################
       
    [Header("한계돌파용 레퍼런스")]
    public GameObject limitBreakCanvas;
    public GridLayoutGroup battleScrollViewContent;
    public GridLayoutGroup territoryScrollViewContent;
    public ScrollRect scrollBattleHero;
    public ScrollRect scrollterritoryHero;
    [SerializeField]
    GameObject heroSlotContainerPrefab;
    public GameObject heroSlotPrefab;
    public Button buttonLimitBreak;
    public GameObject limitBreakPanel;
    public Text textLimitBreak;
    public Text textBeforeTrainingPoint;
    public Text textAfterTrainingPoint;
    public GameObject limitBreakSubscribe;
    public Text notHeroLimitBreakText;

    [SerializeField]
    Text textUnlimitedJewelCount;

    [SerializeField]
    Text textUnlimitedJewelUse;

    [SerializeField]
    Color noneColor = Color.white;

    [SerializeField]
    Color upColor = Color.green;

    //###############################################################################

    //한계돌파에 필요한 정보를 임시로 저장하기 위한 레퍼런스
    public HeroData limitBreakHero = null;

    //초상화 삭제용 게임오브젝트
    GameObject before = null;

    //한계돌파에 사용될 영웅 리스트
    public List<string> sacrificeHeroList = new List<string>();

    //한계돌파용 컨테이너 리스트
    List<UIHeroSlotContainer> containerList = new List<UIHeroSlotContainer>();

    //한계돌파용 컨테이너, 슬롯 풀링
    public List<UIHeroSlotContainer> heroContainerPool = new List<UIHeroSlotContainer>();
    public static List<UIHeroSlot> heroSlotList = new List<UIHeroSlot>();

    [SerializeField]
    Transform slotStackArea;

    int beforePoint = 0;
    int afterPoint = 0;

    //무한의정수 사용량
    int unlimitedJeweluse = 0;
    
    
    enum HeroSortingType
    {
        Name,
    }
    
    private void Awake()
    {
        Instance = this;
    }
    

    public UIHeroSlotContainer GetHeroContainerFromPool(HeroData heroData)
    {

        UIHeroSlotContainer heroSlot = null;

        for (int i = 0; i < heroContainerPool.Count; i++)
        {
            if(heroContainerPool[i].transform.parent == slotStackArea.transform)
            {
                heroSlot = heroContainerPool[i];
                break;
            }
        }

        if (!heroSlot)
        {
            heroSlot = InitSameHeroSlot(heroData);
            GameObject go = Instantiate(heroSlotPrefab) as GameObject;
            UIHeroSlot slot = go.GetComponent<UIHeroSlot>();

            heroSlot.SetHeroSlot(slot);

            heroContainerPool.Add(heroSlot);
        }
        else
        {
            heroSlot.heroInvenID = heroData.id;
            heroSlot.ResetSlotData();

            string heroID = heroData.heroID;
            if (heroID.EndsWith("_Hero"))
            {
                heroSlot.transform.SetParent(battleScrollViewContent.transform, false);
                
            }
            else
            {
                heroSlot.transform.SetParent(territoryScrollViewContent.transform, false);
            }
        }

        return heroSlot;
    }

    
    /// <summary> 한계돌파할 같은 영웅 불러오기 </summary>
    public void ShowSameHeroList(string heroID, string id)
    {
        UIHeroInventory.Instance.isLimitBreak = true;

        if (before != null)
            Destroy(before);

        //대상 슬롯 생성 초기화
        GameObject go = Instantiate(heroSlotPrefab, limitBreakPanel.transform);
        before = go;
        go.GetComponent<UIHeroSlot>().SlotDataInit(limitBreakHero.id, HeroSlotState.Default);
        go.GetComponent<UIHeroSlot>().InitImage();

        textLimitBreak.text = "한계돌파 " + limitBreakHero.limitBreak + "단계 → " + limitBreakHero.limitBreak + "단계";
       

        //훈련포인트 표현 초기화
        int count = 0;
        for (int i = 0; i < limitBreakHero.trainingDataList.Count; i++)
        {
            count += limitBreakHero.trainingDataList[i].training;
        }

        beforePoint = limitBreakHero.limitBreak + 1 - count;

        textBeforeTrainingPoint.text = beforePoint.ToString();
        textBeforeTrainingPoint.color = noneColor;
        textAfterTrainingPoint.text = beforePoint.ToString();
        textAfterTrainingPoint.color = noneColor;

        //무한의 정수 관련 UI 초기화
        textUnlimitedJewelCount.text = MoneyManager.GetMoney(MoneyType.limitBreakTicket).value.ToString();
        unlimitedJeweluse = 0;
        textUnlimitedJewelUse.text = unlimitedJeweluse.ToString();


        List<HeroData> list = HeroManager.heroDataList.FindAll(x => x.heroID == heroID);

        int heroNum = 0;
        //같은 영웅만 컨테이너 생성 or 풀링
        for (int i = 0; i < list.Count; i++)
        {
            UIHeroSlotContainer heroContainer = null;

            //자기 자신은 제외
            if (list[i].id == id)
            {
                continue;
            }

            HeroData b = HeroManager.heroDataDic[list[i].id];
            
            //내정영웅이면
            if (b.heroType == HeroData.HeroType.NonBattle)
            {
                if (!string.IsNullOrEmpty(list[i].placeID))
                {
                    //클릭은 안되지만 생성하여 표시
                    heroContainer = GetHeroContainerFromPool(list[i]);
                    heroContainer.state = HeroSlotState.Default;
                    heroContainer.GetComponentInChildren<UIHeroSlot>().UpdateSlotContents();
                    containerList.Add(heroContainer);

                    heroNum += 1;
                    continue;
                }   
            }
            else//전투영웅이면
            {
                //전투중인지 확인
                if (!string.IsNullOrEmpty(b.battleGroupID))
                {
                    //전투중이면 클릭은 안되지만 생성하여 전투표시
                    heroContainer = GetHeroContainerFromPool(list[i]);
                    heroContainer.state = HeroSlotState.Default;
                    heroContainer.GetComponentInChildren<UIHeroSlot>().UpdateSlotContents();
                    containerList.Add(heroContainer);
                    
                    heroNum += 1;
                    continue;
                }
            }
            //위 조건에 해당하지 않으면 한계돌파용 슬롯으로 생성
            heroContainer = GetHeroContainerFromPool(list[i]);
            heroContainer.state = HeroSlotState.LimitBreak;
            containerList.Add(heroContainer);
            heroNum += 1;
        }

        if (heroNum > 0)
            notHeroLimitBreakText.gameObject.SetActive(false);
        else
            notHeroLimitBreakText.gameObject.SetActive(true);

        SortHeroList(HeroSortingType.Name);

        limitBreakCanvas.SetActive(true);

        //표현 패널 켜기
        limitBreakPanel.SetActive(true);

        //한계돌파 버튼 키고 인터렉티브 끄기
        buttonLimitBreak.gameObject.SetActive(true);
        buttonLimitBreak.interactable = false;

        if(limitBreakHero.heroType == HeroData.HeroType.Battle)
        {
            scrollBattleHero.gameObject.SetActive(true);
            scrollterritoryHero.gameObject.SetActive(false);
        }
        else
        {
            scrollBattleHero.gameObject.SetActive(false);
            scrollterritoryHero.gameObject.SetActive(true);
        }

       
        SizeControl(heroNum);
    }

    UIHeroSlotContainer InitSameHeroSlot(HeroData _heroData)
    {
        UIHeroSlotContainer heroSlot = new UIHeroSlotContainer();
        string heroID = _heroData.heroID;
        if (heroID.EndsWith("_Hero"))
        {
            GameObject go = Instantiate(heroSlotContainerPrefab) as GameObject;
            go.transform.SetParent(battleScrollViewContent.transform, false);
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;

            heroSlot = go.GetComponent<UIHeroSlotContainer>();
            
            heroSlot.heroInvenID = _heroData.id;
            heroSlot.state = HeroSlotState.LimitBreak;

        }
        else if (heroID.EndsWith("_Territory"))
        {

            GameObject go = Instantiate(heroSlotContainerPrefab) as GameObject;
            go.transform.SetParent(territoryScrollViewContent.transform, false);
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;

            heroSlot = go.GetComponent<UIHeroSlotContainer>();
            
            heroSlot.heroInvenID = _heroData.id;
            heroSlot.state = HeroSlotState.LimitBreak;
        }

        return heroSlot;
    }
    
    public static SimpleDelegate OnLimitBreakEnd;

    void RelocationLimitBreak()
    {
        StartCoroutine(ResetContainer());
        
        sacrificeHeroList.Clear();
        containerList.Clear();

        if (OnLimitBreakEnd != null)
            OnLimitBreakEnd();
        if (scrollBattleHero.gameObject.activeSelf || scrollterritoryHero.gameObject.activeSelf)
            UIHeroInfo.Init(limitBreakHero);
        
    }

    public void OnClickUseUnlimitedEssence()
    {
        if (MoneyManager.GetMoney(MoneyType.limitBreakTicket).value - unlimitedJeweluse <= 0)
        {
            UIPopupManager.ShowInstantPopup("무한의정수가 부족합니다");
            return;
        }
            

        unlimitedJeweluse += 1;
        textUnlimitedJewelCount.text = (MoneyManager.GetMoney(MoneyType.limitBreakTicket).value - unlimitedJeweluse).ToString();
        textUnlimitedJewelUse.text = unlimitedJeweluse.ToString();

        OnChangedTrainingMaxValue();
    }


    public void OnChangedTrainingMaxValue()
    {
        if (sacrificeHeroList.Count > 0 || unlimitedJeweluse > 0)
            buttonLimitBreak.interactable = true;
        else
            buttonLimitBreak.interactable = false;

       
        int limitBreak = limitBreakHero.limitBreak;
        int heroGrade = limitBreakHero.heroGrade;
        int point = 0;
        for (int i = 0; i < sacrificeHeroList.Count; i++)
        {
            HeroData heroData = HeroManager.heroDataDic[sacrificeHeroList[i]];
            if (heroData.limitBreak == 0)
            {
                limitBreak += 1;
                point += 1;
            }
            else
            {
                limitBreak += heroData.limitBreak + 1;
                point += heroData.limitBreak + 1;
            }

        }
        limitBreak += unlimitedJeweluse;
        point += unlimitedJeweluse;

        textLimitBreak.text = "한계돌파 " + limitBreakHero.limitBreak + "단계 → " + limitBreak + "단계";

        afterPoint = beforePoint + point;
        textAfterTrainingPoint.text = afterPoint.ToString();
        textAfterTrainingPoint.color = SetTextColor(beforePoint, afterPoint);
            
    }

    double Parser(string express, int value1, int value2)
    {
        ExpressionParser parser = new ExpressionParser();

        Expression exp = parser.EvaluateExpression(express);
        
        exp.Parameters["heroGrade"].Value = value1;
        exp.Parameters["limitBreak"].Value = value2;
        
        return exp.Value;
    }

    Color SetTextColor(float val1, float val2)
    {
        Color color = noneColor;
        if(val1 == val2)
        {
            color = noneColor;
        }
        else if(val1 < val2)
        {
            color = upColor;
        }

        return color;
    }

    IEnumerator ResetContainer()
    {
        for (int i = 0; i < containerList.Count; i++)
        {
            containerList[i].isSelectedToLimitBreak = false;
            containerList[i].transform.SetParent(Instance.slotStackArea);
        }
        yield break;
    }

    /// <summary> 한계돌파를 위한 제물 사용하는 부분 </summary>
    IEnumerator PotentialMaxUp()
    {

        int beforeLimitBreak = limitBreakHero.limitBreak;


        int count = 0;
        for (int i = 0; i < sacrificeHeroList.Count; i++)
        {
            HeroData heroData = HeroManager.heroDataDic[sacrificeHeroList[i]];
            if (heroData.limitBreak == 0)
            {
                limitBreakHero.limitBreak += 1;
                count += 1;
            }
            else
            {
                limitBreakHero.limitBreak += heroData.limitBreak + 1;
                count += heroData.limitBreak + 1;
            }

        }

        count += unlimitedJeweluse;

        int afterLimitBreak = beforeLimitBreak + count;

        yield return StartCoroutine(CheckUseEnhaceStoneAndRuby());

        yield return (StartCoroutine(PotentialMaxUpdate(count)));

        //yield return (StartCoroutine(PoolRemover(sacrificeHeroList)));

        yield return StartCoroutine(ResetContainer());

        if (sacrificeHeroList.Count > 0)
        {
            HeroManager.HeroDelete(sacrificeHeroList);

            while (!HeroManager.isDelete)
                yield return null;
        }

        //정리될때까지 한프레임 쉬고
        yield return null;
        
        if (OnLimitBreakEnd != null)
            OnLimitBreakEnd();

        sacrificeHeroList.Clear();
        containerList.Clear();
        
        if (scrollBattleHero.gameObject.activeSelf || scrollterritoryHero.gameObject.activeSelf)
            UIHeroInfo.Init(limitBreakHero);

        
        UIPopupManager.ShowOKPopup("한계돌파 완료", "한계돌파 결과\n" + beforeLimitBreak + "단계 → " + afterLimitBreak + "단계\n" + "훈련 포인트" + " : " + beforePoint + " → " + afterPoint + "\n" +
        "반환된 강화석 수량 : " + enhancePoint.ToStringABC(), null);
        
        
        

    }

    //강화석 수량 계산
    double enhancePoint = 0;
    string _enhancePointType = string.Empty;
    string enhancePointType
    {
        get
        {
            if (limitBreakHero == null)
                return string.Empty;

            switch (limitBreakHero.baseData.elementalType)
            {
                case ElementalType.NotDefined:
                    _enhancePointType = "";
                    break;
                case ElementalType.Fire:
                    _enhancePointType = "enhancePointA";
                    break;
                case ElementalType.Water:
                    _enhancePointType = "enhancePointB";
                    break;
                case ElementalType.Earth:
                    _enhancePointType = "enhancePointC";
                    break;
                case ElementalType.Light:
                        _enhancePointType = "enhancePointD";
                    break;
                case ElementalType.Dark:
                        _enhancePointType = "enhancePointE";
                    break;
                default:
                        _enhancePointType = "";
                    break;
            }

            return _enhancePointType;
            
        }
    }
    IEnumerator CheckUseEnhaceStoneAndRuby()
    {
        enhancePoint = 0;

        for (int i = 0; i < sacrificeHeroList.Count; i++)
        {
            HeroData heroData = HeroManager.heroDataDic[sacrificeHeroList[i]];

            for (int j = heroData.rebirth; j >= 0; j--)
            {

                int enhanceAmount = 0;
                if (j == heroData.rebirth)
                    enhanceAmount = heroData.enhance;
                else
                    enhanceAmount = 100;

                int rebirth = j * 100;

                //정수값 이하에서는 버림 처리..2147483647
                double curCost = 40 * System.Math.Pow(1.05, rebirth + 0 - 1);
                curCost = curCost < int.MaxValue ? System.Math.Truncate(curCost) : curCost;

                double destCost = 40 * System.Math.Pow(1.05, rebirth + enhanceAmount - 1);
                destCost = destCost < int.MaxValue ? System.Math.Truncate(destCost) : destCost;

                enhancePoint += destCost - curCost;
            }
        }
        

        yield return null;

        
    }

    

    public void OnClickCloseButton()
    {
        limitBreakCanvas.SetActive(false);
        RelocationLimitBreak();

    }

    public void OnClickStartLimitBreak()
    {
        UIPopupManager.ShowYesNoPopup("한계돌파", "한계돌파 후 선택된 영웅은 모두 사라집니다\n정말 진행하시겠습니까", LimitBreakYesOrNo);
    }
    void LimitBreakYesOrNo(string result)
    {
        if (result == "yes")
        {
            limitBreakCanvas.SetActive(false);
            StartCoroutine(PotentialMaxUp());
        }
    }
    
    IEnumerator PotentialMaxUpdate(int upValue)
    {
        string php = "Hero.php";
        WWWForm form = new WWWForm();
        form.AddField("userID", User.Instance.userID);
        form.AddField("heroID", limitBreakHero.id);
        form.AddField("type", 6);
        form.AddField("potentialMaxUp", upValue);
        form.AddField("unlimitedJewel", unlimitedJeweluse);
        form.AddField("enhancePointType", enhancePointType);
        form.AddField("sacrificeHeroList", JsonMapper.ToJson(sacrificeHeroList));

        string result = "";
        yield return StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(php, form, x => result = x));

        if(UserQuestManager.Instance && UserQuestManager.Instance.colaLimitbreakCount < 3 && limitBreakHero.heroID == "Knight_02_Hero")
        {
            UserQuestManager.Instance.colaLimitbreakCount += upValue;
            yield return StartCoroutine(UserQuestManager.Instance.SetUserQuest(UserQuestType.ColaLimitbreak));
        }

        if (!string.IsNullOrEmpty(result))
        {
            Debug.Log("Potential Result : " + result);
        }
    }
    

    void SortHeroList(HeroSortingType sortingType = HeroSortingType.Name, string _placeID = null)
    {
        if (coroutineSort != null)
            return;
        
        coroutineSort = StartCoroutine(SortHeroListA(sortingType));
    }

    Coroutine coroutineSort = null;
    
    HeroSortingType heroSortType = HeroSortingType.Name;
    IEnumerator SortHeroListA(HeroSortingType sortingType = HeroSortingType.Name)
    {
        heroSortType = sortingType;

        containerList.Sort(SortDelegate);

        for (int i = 0; i < containerList.Count; i++)
        {
            containerList[i].transform.SetSiblingIndex(i);
        }

        //오브젝트 정렬 위해 한 프레임 기다려야 함
        yield return null;
        
        coroutineSort = null;
    }

    int SortDelegate(UIHeroSlotContainer a, UIHeroSlotContainer b)
    {
        HeroData heroDataA = HeroManager.heroDataDic[a.heroInvenID];
        HeroData heroDataB = HeroManager.heroDataDic[b.heroInvenID];


        //1차 정렬. 무조건 전투 참여중인애가 제일 위로        
        bool isUsingA = !string.IsNullOrEmpty(heroDataA.placeID) || !string.IsNullOrEmpty(heroDataA.battleGroupID);
        bool isUsingB = !string.IsNullOrEmpty(heroDataB.placeID) || !string.IsNullOrEmpty(heroDataB.battleGroupID);
        
        
        if (isUsingA && !isUsingB)
            return -1;
        else if (!isUsingA && isUsingB)
            return 1;

        
        switch (heroSortType)
        {
            case HeroSortingType.Name:
                return heroDataA.heroName.CompareTo(heroDataB.heroName);
            default:
                return 0;
        }

    }
    

    RectTransform battleHeroContentRect;
    RectTransform territoryHeroContentRect;

    /// <summary> Scroll content size conrtrol </summary>
    void SizeControl(int num)
    {
        if (battleHeroContentRect == null)
            battleHeroContentRect = battleScrollViewContent.GetComponent<RectTransform>();

        if (territoryHeroContentRect == null)
            territoryHeroContentRect = territoryScrollViewContent.GetComponent<RectTransform>();

        
        double count = (double)num / 3;
        int quotient = (int)System.Math.Ceiling(count);

        float sizeDeltaY = (battleScrollViewContent.cellSize.y + battleScrollViewContent.spacing.y) * (quotient);

        battleHeroContentRect.sizeDelta = new Vector2(battleHeroContentRect.sizeDelta.x, sizeDeltaY);

        scrollBattleHero.normalizedPosition = new Vector2(scrollBattleHero.normalizedPosition.x, 1f);

        count = (double)num / 3;
        quotient = (int)System.Math.Ceiling(count);

        sizeDeltaY = (territoryScrollViewContent.cellSize.y + territoryScrollViewContent.spacing.y) * (quotient);

        territoryHeroContentRect.sizeDelta = new Vector2(territoryHeroContentRect.sizeDelta.x, sizeDeltaY);

        scrollterritoryHero.normalizedPosition = new Vector2(scrollterritoryHero.normalizedPosition.x, 1f);
    }
}