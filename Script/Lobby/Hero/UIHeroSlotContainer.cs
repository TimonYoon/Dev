using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using CodeStage.AntiCheat.ObscuredTypes;

public class UIHeroSlotContainer : MonoBehaviour {

    ObscuredString _heroInvenID;
    public ObscuredString heroInvenID { get { return _heroInvenID; }
        set
        {
            _heroInvenID = value;
            
            //Debug.Log("캐싱 됨 " + CachedHeroSprite.name);
        }
    }

    string imageName;
    //public Sprite CachedHeroSprite { get; private set; }

    public HeroSlotState _state = HeroSlotState.Default;
    public HeroSlotState state
    {
        get { return _state; }
        set
        {
            _state = value;

            switch (value)
            {
                case HeroSlotState.Default:
                    clickBehavior = null;
                    break;
                case HeroSlotState.Inventory:
                    clickBehavior = new HeroSlotClickableInventory(heroData, this);
                    break;
                case HeroSlotState.LimitBreak:
                    clickBehavior = new HeroSlotClickableLimitBreak(heroInvenID, this);
                    break;
                case HeroSlotState.Training:
                    clickBehavior = new HeroSlotClickableTraining(this);
                    break;
                case HeroSlotState.Territory:
                    clickBehavior = new HeroSlotClickableTerritory(this);
                    break;
                case HeroSlotState.Battle:
                    clickBehavior = new HeroSlotClickableBattle(this, heroData);
                    break;
            }

            if (heroSlot)
                heroSlot.ChangeState(value, clickBehavior);
        }
    }

    IHeroSlotUpdatable updateBehavior = null;
    IHeroSlotClickable clickBehavior = null;

    public HeroData heroData
    {
        get
        {
            if (HeroManager.heroDataDic.ContainsKey(heroInvenID))
                return HeroManager.heroDataDic[heroInvenID];
            else
                return null;
        }
    }


    public UIHeroSlot _heroSlot = null;
    UIHeroSlot heroSlot
    {
        get
        {
            if (_heroSlot != null)
            {
                _heroSlot.isSelectedToHero = isSelectedToHero;
                _heroSlot.isSelectedToBattle = isSelectedToBattle;
                _heroSlot.isSelectedToTerritory = isSelectedToTerritory;
                _heroSlot.isSelectedToLimitBreak = isSelectedToLimitBreak;
                _heroSlot.isUnableToSelect = isUnableToSelect;
                _heroSlot.ShowDeployedPlace();
                _heroSlot.UpdateSlotContents();
               
            }
                
            return _heroSlot; }
        set
        {
            if (!value)
            {
                if(_heroSlot)
                    _heroSlot.gameObject.SetActive(false);

                _heroSlot = null;

                return;
            }

            _heroSlot = value;

            value.transform.SetParent(transform);
            value.transform.localPosition = Vector3.zero;
            value.transform.localScale = Vector3.one;
            
            value.SlotDataInit(heroInvenID, state);
            _heroSlot.InitImage(AssetLoader.cachedAtlasDic["Atlas_HeroImage"].GetSprite(imageName));

            _heroSlot.ChangeState(state, clickBehavior);

            _heroSlot.isSelectedToHero = isSelectedToHero;
            _heroSlot.isSelectedToBattle = isSelectedToBattle;
            _heroSlot.isSelectedToTerritory = isSelectedToTerritory;
            _heroSlot.isSelectedToLimitBreak = isSelectedToLimitBreak;
            _heroSlot.isUnableToSelect = isUnableToSelect;
            value.ShowDeployedPlace();
            value.UpdateSlotContents();
            value.gameObject.SetActive(true);
        }
    }

    bool _isUnableToSelect;
    public bool isUnableToSelect
    {
        get { return _isUnableToSelect; }
        set
        {
            if (isSelectedToBattle || isSelectedToTerritory)
                return;

            _isUnableToSelect = value;
            if (heroSlot)
                heroSlot.isUnableToSelect = value;
        }
    }

    bool _isSelectedToBattle;
    public bool isSelectedToBattle
    {
        get { return _isSelectedToBattle; }
        set
        {
            _isSelectedToBattle = value;
            if (heroSlot)
                heroSlot.isSelectedToBattle = value;
        }
    }


    bool _isSelectedToTerritory;
    public bool isSelectedToTerritory
    {
        get { return _isSelectedToTerritory; }
        set
        {
            _isSelectedToTerritory = value;
            if (heroSlot)
                heroSlot.isSelectedToTerritory = value;
        }
    }

    bool _isSelectedToHero = false;
    public bool isSelectedToHero
    {
        get { return _isSelectedToHero; }
        set
        {
            _isSelectedToHero = value;
            if (heroSlot)
                heroSlot.isSelectedToHero = value;
        }
    }

    bool _isSelectedToLimitBreak = false;
    public bool isSelectedToLimitBreak
    {
        get { return _isSelectedToLimitBreak; }
        set
        {
            _isSelectedToLimitBreak = value;
            if (heroSlot)
                heroSlot.isSelectedToLimitBreak = value;
        }
    }
    bool _hasBattleID = false;
    public bool hasBattleID
    {
        get { return _hasBattleID; }
        set
        {
            _hasBattleID = value;
            if (heroSlot)
                heroSlot.hasBattleID = value;
        }
    }

    ScrollRect scrollRect;

    RectTransform rectTransform;

    RectTransform rectTransformViewport;
    RectTransform rectTransformContent;

    bool _isDestroy = false;
    public bool isDestroy
    {
        get { return _isDestroy; }
        set
        {
            if (UIHeroInventory.Instance && value == true)
            {
                heroSlot = null;
                
            }   

            _isDestroy = true;
            
        }
    }

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if(!UIHeroTraining.Instance)
        {
            UIHeroInventory.onOpened += OnOpenedInventory;
            UISortHeroSlot.onSort += OnSortInventory;
        }
    }

    //##################################################################
    void OnEnable()
    {

        if (!scrollRect)
            scrollRect = GetComponentInParent<ScrollRect>();

        if (scrollRect)
        {
            rectTransformViewport = scrollRect.viewport.GetComponent<RectTransform>();
            rectTransformContent = scrollRect.content.GetComponent<RectTransform>();

            //스크롤될 때 콜백 등록
            scrollRect.onValueChanged.AddListener(OnScrollRectValueChanged);
        }
        //UpdateContent();
    }

    public void InitContainer()
    {
        if (!scrollRect)
            scrollRect = GetComponentInParent<ScrollRect>();

        if (scrollRect)
        {
            rectTransformViewport = scrollRect.viewport.GetComponent<RectTransform>();
            rectTransformContent = scrollRect.content.GetComponent<RectTransform>();

            //스크롤될 때 콜백 등록
            scrollRect.onValueChanged.AddListener(OnScrollRectValueChanged);
        }
    }

 

    private void OnDestroy()
    {
        //heroSlot = null;
        
        UIHeroInventory.onOpened -= OnOpenedInventory;
        UISortHeroSlot.onSort -= OnSortInventory;
        
    }

    /// <summary> 인벤토리 열었을 때 </summary>
    void OnOpenedInventory()
    {
        //Debug.Log("OnOpened");
        //Debug.Log("");

        UpdateContent();
    }

    /// <summary> 인벤토리 정렬 후 </summary>
    void OnSortInventory()
    {
        UpdateContent();
    }

    void UpdateContent()
    {
        //if (UIHeroInventory.Instance.isLimitBreak)
        //    return;
        
        if (isNeedToShow)
        {
            if (UIHeroInventory.Instance.isLimitBreak)
                return;

            if (heroSlot)
                return;

            //Debug.Log("여기");
            //if (SceneLobby.currentState != LobbyState.SubMenu)
            

            if (!isDestroy)// && UIHeroInventory.heroSlotContainerList.Find(x => x.gameObject == this.gameObject) != null)
                heroSlot = UIHeroInventory.GetHeroSlotFromPool();

        }
        else
        {
            if(UILimitBreak.Instance.heroContainerPool.Find(x=>x == this))
            {
                return;
            }

            if (UIHeroInventory.heroSlotContainerList.Find(x => x.gameObject == this.gameObject) && heroSlot != null)
            {
                heroSlot.transform.SetParent(UIHeroInventory.Instance.heroSlotStackArea.transform);
                heroSlot = null;
            }
                

        }
    }

    void OnScrollRectValueChanged(Vector2 pos)
    {
        UpdateContent();
    }

    /// <summary> 지금 화면에 비춰지는가? </summary>
    bool isNeedToShow
    {
        get
        {
            if (!scrollRect)
                return false;

            float y = -rectTransform.anchoredPosition.y;
           
            float height = rectTransformViewport.rect.height;
            
            if (y == 0)
                return false;
            //Debug.Log("y : " + y);
            //Debug.Log("height : " + height);
            //Debug.Log("rectTransformContent.rect.height : " + rectTransformContent.localPosition.y);
            if (y - rectTransformContent.localPosition.y < height + 250
                && y - rectTransformContent.localPosition.y > - height - 250)
            {
               
                
                return true;
            }
                
            else
                return false;
        }
    }

    public void ResetSlotData()
    {
        if (heroSlot)
        {
            heroSlot.SlotDataInit(heroInvenID, HeroSlotState.Default);
            heroSlot.InitImage(AssetLoader.cachedAtlasDic["Atlas_HeroImage"].GetSprite(imageName));

            heroSlot.ChangeState(state, clickBehavior);
            
            heroSlot.ShowDeployedPlace();
            heroSlot.UpdateSlotContents();
            heroSlot.gameObject.SetActive(true);
        }
            
    }

    public void SetHeroSlot(UIHeroSlot _heroSlot)
    {
        heroSlot = _heroSlot;
    }

    /// <summary> 캐릭터 정보 상세보기 </summary>
    public void ShowHeroInfo(HeroData heroData)
    {
        if (coroutineShowHeroInfo != null)
            StopCoroutine(coroutineShowHeroInfo);

        coroutineShowHeroInfo = StartCoroutine(ShowHeroInfoA(heroData));
    }

    Coroutine coroutineShowHeroInfo;

    IEnumerator ShowHeroInfoA(HeroData heroData)
    {
        //씬 불러옴
        Scene scene = SceneManager.GetSceneByName("HeroInfo");
        if (!scene.isLoaded)
        {
            yield return StartCoroutine(AssetLoader.Instance.LoadLevelAsync("scene/heroinfo", "HeroInfo", true));

            scene = SceneManager.GetSceneByName("HeroInfo");

            while (!scene.isLoaded)
                yield return null;
        }

        if (UIHeroInfo.Instance)
            UIHeroInfo.Init(heroData);
    }
}
