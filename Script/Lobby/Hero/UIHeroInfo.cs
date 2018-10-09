using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Reflection;


public interface IHeroInfoUpdateUI
{
    void UpdateUI();
}

/// <summary> 영웅 정보창 타입 (타입에 따라 화면에 보여지것과 기능이 달라짐 ) </summary>
public enum HeroInfoType
{
    Default,    // 강화 , 한계돌파 버튼이 존재하는 기본적인 영웅 정보창
    OnlyInfo,   // 모든 기능 버튼을 감추고 영웅의 정보만 표기함
    Dictionary  // 도감에서 사용된 영웅 정보창

}

/// <summary> 영웅 정보창. 영웅 정보창은 별도 씬으로 되어 있음 </summary>
public class UIHeroInfo : MonoBehaviour
{
    static public UIHeroInfo Instance;

    public Canvas canvas;

    public RectTransform pivotCharacter;

    [Header("영웅 기본 정보. 최상단 제목에 보여질 정보들")]
    public Text textName;

    /// <summary> 강화 </summary>
    public Text textEnhance;

    /// <summary> 환생 오브젝트 </summary>
    public GameObject objectRebirth;

    /// <summary> 환생횟수 </summary>
    public Text textRebirth;

    [Header("전투/내정에 따라 따로 표시해줘야 하는 오브젝트들")]
    public GameObject battleParamPanel;
    public GameObject TerritroyParamPanel;
    public Text textTrainingValue1;
    public Text textTrainingValue2;
    public Text textTrainingValue3;

    [Header("모든 버튼 그룹")]
    public GameObject buttonGroupPanel;

    [Header("강화 & 환생")]
    public GameObject enhancePanel;
    [Header("한계돌파")]
    public GameObject limitBreakPanel;


    public Button buttonLimitBreak;
    public Text textLimitBreak;

    [Header("승급")]
    public Button buttonPromote;
    public Canvas canvasPromote;
    public Text textNeedRubyPromote;
    int needRubyPromote;

    [Header("숙련도")]
    public GameObject proficiencyPanel;
    public Image proficiencyProgressbar;
    public Text proficiencyPercent;



    [Header("스킬 정보 관련")]
    /// <summary> 스킬 목록 </summary>
    public GameObject SkillListPanel;

    [Header("도감 관련")]
    public GameObject buttonBg;
    public GameObject[] buttons;
    public Button achievementButton;
    public Text achievementLevelText;


    /// <summary> 영웅 아이디 </summary>
    string heroID = "";

    bool _isDictionary = false;
    bool isDictionary
    {
        get { return _isDictionary; }
        set
        {
            if (_isDictionary == value)
                return;

            InitDictionaryOpen();
            _isDictionary = value;
        }
    }


    //----------------------------------------------------
    LobbyState currentLobbyState = LobbyState.Hero;

    BattleHero battleHero = null;

    HeroData _heroData;
    /// <summary> 영웅 데이터 </summary>
    public HeroData heroData
    {
        get { return _heroData; }
        private set
        {
            if (_heroData != null)
                _heroData.onChangedValue -= OnChangedHeroData;

            _heroData = value;

            if (value != null)
                value.onChangedValue += OnChangedHeroData;

        }
    }

    public GameObject statSlotPrefab;
    public RectTransform rtContentStat;

    void OnChangedHeroData(PropertyInfo p)
    {
        if (p.Name == "enhance" || p.Name == "rebirth" || p.Name == "level")
            UpdateStat();
    }

    bool canPromote
    {
        get
        {
            if (heroData == null)// || heroData.baseData == null)
                return false;

            string s = heroData.baseData.promoteID;

            if (heroData.isTraining == true)
                return false;

            if (!string.IsNullOrEmpty(s))
            {
                HeroBaseData data = HeroManager.heroBaseDataDic[s];

                if (data != null)
                    return true;
            }

            return false;
        }
    }



    GameObject objBattleCharacter = null;
    Coroutine coUpdateInfo = null;

    //#####################################################################################################
    void Awake()
    {
        Instance = this;

        canvas.gameObject.SetActive(false);
        if (SceneLobby.Instance)
            SceneLobby.Instance.OnChangedMenu += OnChangedLobbyState;

        HeroManager.onPromotedHeroData += OnPromoteHero;
    }

    public HeroInfoType heroInfoType { get; private set; }

    /// <summary> 전투 창에서 영웅 정보를 보여줄 때 </summary>
    static public void Init(BattleHero battleHero, bool showImmediately = true)
    {
        Instance.heroInfoType = HeroInfoType.Default;

        Instance.battleHero = battleHero;
        Instance.heroData = battleHero.heroData;

        if (battleHero.heroData == null)
            return;

        if (showImmediately)
            Instance.Show();
    }

    /// <summary> 전투가 아닌 다른 곳에서 정보를 보여줄 때 </summary>
    static public void Init(HeroData _heroData, bool showImmediately = true, string _heroID = null, HeroInfoType _heroInfoType = HeroInfoType.Default)
    {
        Instance.heroInfoType = _heroInfoType;

        Instance.battleHero = null;
        Instance.heroData = _heroData;

        if (string.IsNullOrEmpty(_heroID) == false)
            Instance.heroInfoType = HeroInfoType.Dictionary;

        Instance.heroID = _heroID;

        if (_heroData == null)
            return;

        if (showImmediately)
            Instance.Show();
    }

    void OnChangedLobbyState(LobbyState state)
    {
        //영웅 정보 창이 열렸을 때랑 메뉴가 달라지면 창 닫음
         Hide();
    }


    void Show()
    {

        if (coUpdateInfo != null)
        {
            promoteUpdateCo = StartCoroutine(UpdateInfo());
            return;
        }

        HeroEnhance.Instance.InitEnhance();

        buttonGroupPanel.SetActive(heroInfoType != HeroInfoType.OnlyInfo);

        enhancePanel.SetActive(heroInfoType == HeroInfoType.Default);
        limitBreakPanel.SetActive(heroInfoType == HeroInfoType.Default);

        currentLobbyState = SceneLobby.currentState;

        coUpdateInfo = StartCoroutine(UpdateInfo());

        //도감에서 열 경우
        InitDictionaryOpen();

        canvas.gameObject.SetActive(true);
    }

    void Hide()
    {
        if (onHide != null)
            onHide();

        if (coUpdateInfo != null)
        {
            StopCoroutine(coUpdateInfo);
            coUpdateInfo = null;
        }

        if(promoteUpdateCo != null)
        {
            StopCoroutine(promoteUpdateCo);
            promoteUpdateCo = null;
        }

        canvas.gameObject.SetActive(false);

        Destroy(objBattleCharacter);

        objBattleCharacter = null;
    }

    List<UIStatSlot> statSlots = new List<UIStatSlot>();
    void UpdateStat()
    {
        //미리 생성되어 있는 능력치들 hide
        for(int i = 0; i< statSlots.Count; i++)
        {
            statSlots[i].gameObject.SetActive(false);
        }

        List<StatType> keys = heroData.stats.paramDic.Keys.ToList();
        //능력치 슬롯들 생성
        for (int i = 0; i < heroData.stats.paramDic.Count; i++)
        {
            var stat = heroData.stats.GetParam(keys[i]);
            if (stat.baseData == null || stat.baseData.hideInUI)
                continue;

            if (stat.value == 0)
                continue;

            AddStatSlot(stat);
        }

        //index 별로 정렬. 매번 해줘야 하나..
        statSlots = statSlots.OrderBy(x => x.index).ToList();
        for (int i = 0; i < statSlots.Count; i++)
        {
            UIStatSlot s = statSlots[i];
            s.transform.SetSiblingIndex(s.index);
        }
    }
    
    void AddStatSlot(Stat stat)
    {
        //비활성 슬롯 검색
        UIStatSlot slot = statSlots.Find(x => !x.gameObject.activeSelf);

        //없으면 새로 만듬
        if(slot == null)
        {
            GameObject go = Instantiate(statSlotPrefab, rtContentStat);
            slot = go.GetComponent<UIStatSlot>();
            statSlots.Add(slot);
        }

        slot.Init(stat);        
        slot.gameObject.SetActive(true);
        
    }

    /// <summary> 정보 갱신 </summary>
    IEnumerator UpdateInfo()
    {
        if (heroData == null)
            yield break;

        textName.text = heroData.heroName;

        UpdateUI();

        isDictionary = false;
        if (heroData.id == heroData.heroID)
            isDictionary = true;

        //전투/내정 영웅에 따라 끄고 켜야할 오브젝트들
        //battleParamPanel.SetActive(isBattleHero);
        //TerritroyParamPanel.SetActive(isBattleHero == false);

        UpdateStat();
        // #################################################### 파라미터 세팅부 
        //전투 영웅, 내정영웅 정보 다르게 표시
        if (heroData.heroType == HeroData.HeroType.Battle)
        {
            //영웅 모델 화면에 표시
            #region 영웅 모델 표시
            if (!string.IsNullOrEmpty(heroData.prefab))
            {
                GameObject prefab = null;
                yield return StartCoroutine(AssetLoader.Instance.LoadGameObjectAsync(heroData.assetBundle, heroData.prefab, x => prefab = x));

                if (!prefab)
                    yield break;

                if (objBattleCharacter != null)
                    Destroy(objBattleCharacter);

                objBattleCharacter = Instantiate(prefab, pivotCharacter);
                objBattleCharacter.transform.localPosition = Vector2.zero;
                objBattleCharacter.transform.localScale = Vector3.one * 50f;

                //order 맞추기
                OrderController orderController = objBattleCharacter.GetComponent<OrderController>();
                //orderController.orderOffset = 58;
                orderController.enabled = false;

                //공중에 뜬 애들 보정
                Spine.Unity.SkeletonAnimation s = objBattleCharacter.GetComponentInChildren<Spine.Unity.SkeletonAnimation>();
                if (s)
                {
                    s.transform.localPosition *= 0.2f;
                    if (Time.timeScale > 0f)
                        s.timeScale = 1f / Time.timeScale;
                    else
                        s.timeScale = 0f;
                }

                //레이어 UI로 변경
                for (int i = 0; i < objBattleCharacter.transform.childCount; i++)
                {
                    GameObject child = objBattleCharacter.transform.GetChild(i).gameObject;
                    child.layer = LayerMask.NameToLayer("UI");
                    Renderer r = child.GetComponent<Renderer>();
                    if (r)
                    {
                        r.sortingLayerName = "UI";
                        r.sortingOrder = 58;

                        if (r.gameObject.name == "Shadow")
                        {
                            r.sortingOrder = 57;
                        }
                    }
                }
            }
            #endregion
        }
        //내정 영웅
        else
        {
            //이미지 표시
            if (!string.IsNullOrEmpty(heroData.heroImageName))
            {


            }


        }

        SkillListPanel.SetActive(true);

        yield break;
    } 


    //차후 수정
    /// <summary> 수련에의해 완료된 숙련치 </summary>
    int training
    {
        get
        {
            if (heroData == null)
                return 0;

            return 0;
        }
    }


    /// <summary> 아직 수련되지 않은 잠재능력치 </summary>
    int limitBreak
    {
        get
        {
            if (heroData == null)
                return 0;
            int i = heroData.limitBreak;
            return i;
        }
    }

    /// <summary> 변경된 수치에 따라 UI 변경 하는 부분 </summary>
    public void UpdateUI()
    {
        //승급 버튼 표시 여부
        canvasPromote.enabled = (string.IsNullOrEmpty(heroData.battleGroupID) && string.IsNullOrEmpty(heroData.placeID) && canPromote);

        if(canvasPromote.enabled == true)
        {
            if(heroData.heroGrade == 2)
            {
                needRubyPromote= (heroData.limitBreak + 1) * 500;
                textNeedRubyPromote.text = needRubyPromote.ToString();
            }
            else if (heroData.heroGrade == 3)
            {
                needRubyPromote = (heroData.limitBreak + 1) * 1000;
                textNeedRubyPromote.text = needRubyPromote.ToString();
            }
        }

        if (heroInfoType == HeroInfoType.Dictionary)
            proficiencyPanel.SetActive(false);
        else
            proficiencyPanel.SetActive(true);

        if (buttonLimitBreak.gameObject.activeSelf)
        {
            //잠재능력 수련치/최대치 표시
            textLimitBreak.text = limitBreak + " 단계";
        }

        //수련 맥스값 표기
        if (heroInfoType != HeroInfoType.Dictionary)
            InitTrainingValue();


        objectRebirth.SetActive(heroData.rebirth > 0);
        textRebirth.text = heroData.rebirth.ToString();

        textEnhance.gameObject.SetActive(heroData.enhance > 0);
        textEnhance.text = "+" + heroData.enhance.ToString();        

        // 영웅 인벤토리 리스트에서 해당 영웅 변경된 데이터에 맞게 slot 수정
        if(heroInfoType == HeroInfoType.Default)
            UIHeroInventory.Instance.UpdateHeroSlotData(heroData.id);

        
        
        OnValueChangedProficiency();
    }

    /// <summary> 승급 버튼 눌렀을 때 </summary>
    public void OnClickPromote()
    {
        if(needRubyPromote > MoneyManager.GetMoney(MoneyType.ruby).value)
        {
            UIPopupManager.ShowOKPopup("루비 부족", "루비가 부족합니다", null);
            return;
        }

        if (coPromote != null)
            return;

        coPromote = StartCoroutine(HeroPromote());
    }
    Coroutine coPromote = null;

    Coroutine promoteUpdateCo = null;

    void OnPromoteHero(string id)
    {
        heroData = HeroManager.heroDataDic[id];
        Init(heroData);
    }


    IEnumerator HeroPromote()
    {
        UIHeroPromote.Instance.Init(heroData);

        string php = "Hero.php";
        WWWForm form = new WWWForm();
        form.AddField("userID", User.Instance.userID);
        form.AddField("heroID", heroData.id);
        string heroID = heroData.heroID;
        form.AddField("promoteHeroID", heroData.baseData.promoteID);
        string promoteID = heroData.baseData.promoteID;
        form.AddField("promoteRuby", needRubyPromote);
        form.AddField("type", 9);
        string result = string.Empty;
        yield return StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(php, form, x => result = x));
        if(string.IsNullOrEmpty(result) == false && result == "1")
        {
            UIPopupManager.ShowOKPopup("루비 부족", "루비가 부족합니다", null);
            yield break;
        }

        if(UserQuestManager.Instance && UserQuestManager.Instance.colaPromoteCount < 1 && heroID == "Knight_02_Hero")
        {
            UserQuestManager.Instance.colaPromoteCount += 1;
            StartCoroutine(UserQuestManager.Instance.SetUserQuest(UserQuestType.ColaPromote));
        }

        if (UserQuestManager.Instance && UserQuestManager.Instance.platinumGetCount < 3 && GameDataManager.heroBaseDataDic[promoteID].grade == 4)
        {
            UserQuestManager.Instance.platinumGetCount += 1;
            StartCoroutine(UserQuestManager.Instance.SetUserQuest(UserQuestType.PlatinumGet));
        }

        yield return StartCoroutine(UIHeroPromote.Instance.ShowPromoteAnimation());

        coPromote = null;
        yield break;
    }


    /// <summary> 수련 버튼 눌렀을 때 </summary>
    public void OnClickLimitBreakHero()
    {
        Hide();

        UILimitBreak.Instance.limitBreakHero = heroData;
        UILimitBreak.Instance.ShowSameHeroList(heroData.heroID, heroData.id);
    }
    
    /// <summary> 숙련도 도움말 버튼 눌렀을 때 </summary>
    public void OnClickProficiencyHelp()
    {
        Debug.Log("OnClickProficiency");
    }

  
    /// <summary> 숙련도 슬라이더 값 바뀌었을 때 </summary>
    void OnValueChangedProficiency()
    {
        float fill = heroData.proficiencyTime / heroData.maxProficiencyTime;

        string text = "";
        if (heroData.isGetProficiencyReward)
        {
            fill = 1;
            text = "보상획득완료";
        }            
        else
        {
            float percent = fill * 100f;
            if (100 <= percent)
                text = "회군하여 보상을 획득하세요.";
            else
                text = percent.ToString("N1") + "%";

        }

        
        proficiencyProgressbar.fillAmount = fill;
        proficiencyPercent.text = text;
    }


    public delegate void OnHide();
    public OnHide onHide;

    /// <summary> 닫기 버튼 눌렀을 때 </summary>
    public void OnClickClose()
    {
        //if (onHide != null)
        //    onHide();

        Hide();
    }

    void InitTrainingValue()
    {
        if (!string.IsNullOrEmpty(heroData.trainingDataList[0].paramName))
            textTrainingValue1.text = heroData.trainingDataList[0].paramName + "    (" + heroData.trainingDataList[0].training + " / " + heroData.trainingMax + ")";
        if (!string.IsNullOrEmpty(heroData.trainingDataList[1].paramName))
            textTrainingValue2.text = heroData.trainingDataList[1].paramName + "    (" + heroData.trainingDataList[1].training + " / " + heroData.trainingMax + ")";
        else
            textTrainingValue2.text = "";
        if (!string.IsNullOrEmpty(heroData.trainingDataList[2].paramName))
            textTrainingValue3.text = heroData.trainingDataList[2].paramName + "    (" + heroData.trainingDataList[2].training + " / " + heroData.trainingMax + ")";
        else
            textTrainingValue3.text = "";
    }

    [SerializeField]
    GameObject gradePanel;

    void InitDictionaryOpen()
    {
        if (heroInfoType == HeroInfoType.Dictionary)
        {
            buttonBg.SetActive(false);
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].SetActive(false);
            }
            achievementButton.gameObject.SetActive(true);
            DictionaryManager.HeroDictionaryData heroDicData = DictionaryManager.heroDictionaryDataDic[heroData.heroID];
            if (heroDicData.dictionaryLevel > heroDicData.rewardStep)
            {
                achievementLevelText.text = (heroDicData.rewardStep + 1) + "단계 획득가능";
                achievementButton.interactable = true;
            }
            else
            {
                achievementButton.interactable = false;
                achievementLevelText.text = heroDicData.rewardStep + "단계 획득완료";

                DictionaryManager.Instance.DictionaryRewardRecieveOrNot();
            }

            
        }
        else
        {
            buttonBg.SetActive(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].SetActive(true);
            }
            achievementButton.gameObject.SetActive(false);
        }
    }

    /// <summary> 보상 획득 </summary>
    public void OnClickAchievementButton()
    {
        StartCoroutine(SetAchievementData());
    }

    /// <summary> 도감 DB에 변경된 수치를 Update시키는 코루틴 </summary>
    IEnumerator SetAchievementData()
    {
        int amount = 0;
        DictionaryManager.HeroDictionaryData heroDicData = DictionaryManager.heroDictionaryDataDic[heroData.heroID];
        int rewardStep = heroDicData.rewardStep;
        switch (rewardStep)
        {
            case 0:
                amount = 10;
                break;
            case 1:
                amount = 30;
                break;
            case 2:
                amount = 100;
                break;

        }
        rewardStep += 1;
        WWWForm form = new WWWForm();
        form.AddField("userID", User.Instance.userID);
        form.AddField("heroID", Instance.heroID);
        form.AddField("rewardAmount", amount);
        form.AddField("achievementLevel", rewardStep);
        form.AddField("type", 4);
        string php = "Dictionary.php";
        string result = "";
        yield return StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(php, form, x => result = x));
        
        if (!string.IsNullOrEmpty(result) && result == "0")
        {
            Debug.LogError("잘못된 접근입니다. 도감획득단계보다 보상단계가 높거나 같습니다");
            
          
        }
        else
        {
            heroDicData.rewardStep = rewardStep;
            UIPopupManager.ShowOKPopup("보상 획득", "보상을 획득했습니다\n메일함을 확인해주세요", InitDictionaryOpen);
            
           

            if(UIDictionary.Instance)
            {
                UIDictionary.Instance.heroSlotList.Find(x => x.heroData.heroID == Instance.heroID).SlotDataInit(Instance.heroID, DicionaryState.Default);
            }

            yield return StartCoroutine(MailManager.MailDataInitCoroutine());
        }
    }

    public void SetDictionaryDataLevel(int level)
    {
        StartCoroutine(SetDictionaryLevelData(level));
    }
    
    IEnumerator SetDictionaryLevelData(int type)
    {
        WWWForm form = new WWWForm();
        form.AddField("userID", User.Instance.userID);
        form.AddField("heroID", heroData.heroID);
        form.AddField("dictionaryLevel", DictionaryManager.heroDictionaryDataDic[heroData.heroID].dictionaryLevel);
        form.AddField("type", type);
        string php = "Dictionary.php";
        string result = "";
        yield return StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(php, form, x => result = x));

        if(!string.IsNullOrEmpty(result) && result == "1")
        {
            DictionaryManager.heroDictionaryDataDic[heroData.heroID].dictionaryLevel = type;
            UIPopupManager.ShowOKPopup("도감 갱신", "새로운 도감목표를 달성했습니다\n도감을 확인해주세요", InitDictionaryOpen);

            DictionaryManager.Instance.DictionaryRewardRecieveOrNot();
        }
    }

    
}
