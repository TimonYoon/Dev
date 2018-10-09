using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Reflection;
using System.Linq;

public enum HeroSlotState
{
    Default,
    Buy,
    LimitBreak,
    Battle,
    Territory,
    Inventory,
    Training
}

public class UIHeroSlot : MonoBehaviour
{
    HeroSlotState _state = HeroSlotState.Default;
    public HeroSlotState state
    {
        get { return _state; }
        private set
        {
            if (_state == value && value != HeroSlotState.Default)
                return;

            _state = value;

            switch (value)
            {
                case HeroSlotState.Default:
                    updateBehavior = new HeroSlotUpdatableDefault(this);
                    break;
                case HeroSlotState.Inventory:
                    updateBehavior = new HeroSlotUpdatableInventory(this);
                    break;
                case HeroSlotState.LimitBreak:
                    updateBehavior = new HeroSlotUpdatableLimitBreak();
                    break;
                case HeroSlotState.Training:
                    updateBehavior = new HeroSlotUpdatableTraining(this);
                    break;
                case HeroSlotState.Territory:
                    updateBehavior = new HeroSlotUpdatableTerritory();
                    break;
                case HeroSlotState.Battle:
                    updateBehavior = new HeroSlotUpdatableBattle();
                    break;
            }

            //UpdateSlotContents();
        }
    }



    [Header("등급별 표시")]
    public GameObject[] gradeArray;
    public Image bg;
    public Color[] colorArray;
    public bool showGradeColor = true;

    [Header("기본 정보")]
    public Image heroImage;
    public GameObject newChecker;

    public Text heroNameText;
    public Text textEnhance;
    public Text textRebirth;
    public Button heroSlotButton = null;

    [SerializeField]
    GameObject unableToSelectPanel;

    /// <summary> 히어로 데이타 참조용. (처음 초기화할 때 저장) </summary>
    public string id { get; private set; }

    /// <summary> 새로운 획득한 영웅인가 </summary>
    public bool isNew { get { return HeroManager.heroDataDic[id].isChecked; } }

    public HeroData heroData
    {
        get
        {
            if (HeroManager.heroDataDic.ContainsKey(id))
                return HeroManager.heroDataDic[id];
            else
                return null;
        }

    }
    

    bool _isSelectedToBattle = false;
    public bool isSelectedToBattle
    {
        get { return _isSelectedToBattle; }
        set
        {
            _isSelectedToBattle = value;
            if (state == HeroSlotState.Battle)
                AddPanel.SetActive(value);
        }
    }

    bool _isSelectedToTerritory = false;
    public bool isSelectedToTerritory
    {
        get { return _isSelectedToTerritory; }
        set
        {
            _isSelectedToTerritory = value;
            if(state == HeroSlotState.Territory)
                AddPanel.SetActive(value);
        }
    }

    bool _isSelectedToHero = false;
    public bool isSelectedToHero
    {
        get { return _isSelectedToHero; }
        set
        {
            _isSelectedToHero = value;
            AddPanel.SetActive(value);
        }
    }

    bool _isSelectedToLimitBreak = false;
    public bool isSelectedToLimitBreak
    {
        get { return _isSelectedToLimitBreak; }
        set
        {
            _isSelectedToLimitBreak = value;
            if(state == HeroSlotState.LimitBreak)
                AddPanel.SetActive(value);
        }
    }

    bool _isUnableToSelect = false;
    public bool isUnableToSelect
    {
        get { return _isUnableToSelect; }
        set
        {
            _isUnableToSelect = value;
            unableToSelectPanel.SetActive(value);
        }
    }

    IHeroSlotUpdatable updateBehavior = null;

    IHeroSlotClickable clickBehavior = null;

    bool _hasBattleID = false;
    public bool hasBattleID
    {
        get { return _hasBattleID; }
        set
        {
            _hasBattleID = value;

            if (state == HeroSlotState.Default)
                BattlePanel.SetActive(value);
        }
    }

    Image[] imageList;
    //#######################################################################################################
    private void Awake()
    {

        imageList = GetComponentsInChildren<Image>(true);
        heroImage.RegisterDirtyMaterialCallback(OnChangedImage);
    }

    private void OnEnable()
    {
        HeroManager.onPromotedHeroData += ResetSlotData;
    }

    private void OnDisable()
    {
        HeroManager.onPromotedHeroData -= ResetSlotData;
    }

    bool _isValidImage = false;
    public bool isValidImage
    {
        get { return _isValidImage; }
        set
        {
            _isValidImage = value;

            if (!value)
            {
                for (int i = 0; i < imageList.Length; i++)
                {
                    imageList[i].CrossFadeAlpha(0f, 0f, true);
                }
            }
            else
            {
                for (int i = 0; i < imageList.Length; i++)
                {
                    imageList[i].CrossFadeAlpha(1f, 0.2f, true);
                }
            }
        }
    }

    void OnChangedImage()
    {
        isValidImage = !string.IsNullOrEmpty(id) && heroData != null && heroData.baseData != null && heroImage.sprite != null && heroImage.sprite.name == heroData.baseData.image;
        //Debug.Log(heroImage.sprite + ", " + isValidImage);
    }

    public void OnClick()
    {
        if (clickBehavior != null)
        {
            clickBehavior.OnClick();
        }
        else
        {
            Debug.Log("clickBehavior is null");
        }
           
    }


    //void OnChangedHeroParam(PropertyInfo property)
    //{
    //    return;      
    //}
    
    /// <summary> 배치된 지역 보여주기 </summary>
    public void ShowDeployedPlace()
    {
        if (state == HeroSlotState.Territory && string.IsNullOrEmpty(heroData.placeID) == false)
        {
            string placeName = heroData.placeID;
            if (GameDataManager.productionLineBaseDataDic.ContainsKey(heroData.placeID))
                placeName = GameDataManager.productionLineBaseDataDic[heroData.placeID].name;
            else if (GameDataManager.placeBaseDataDic.ContainsKey(heroData.placeID))
                placeName = GameDataManager.placeBaseDataDic[heroData.placeID].name;


            Text text = BattlePanel.GetComponentInChildren<Text>();
            text.text = placeName;
            BattlePanel.SetActive(true);
        }
        else if (state == HeroSlotState.Battle && HeroManager.heroDataDic[id].heroType == HeroData.HeroType.Battle)
        {
            if (heroData != null && string.IsNullOrEmpty(heroData.battleGroupID) ==false)
            {
                //HeroData data = HeroManager.heroDataDic[id];
                string dungeonName = GameDataManager.dungeonBaseDataDic[Battle.battleGroupList.Find(x => x.battleType == heroData.battleGroupID).dungeonID].dungeonName;
                Text text = BattlePanel.GetComponentInChildren<Text>();
                text.text = dungeonName;
                BattlePanel.SetActive(true);
            }
            else
                BattlePanel.SetActive(false);

        }
        else if(state == HeroSlotState.Training)
        {
            if(string.IsNullOrEmpty(heroData.placeID) == false)
            {
                string placeName = heroData.placeID;
                if (GameDataManager.productionLineBaseDataDic.ContainsKey(heroData.placeID))
                    placeName = GameDataManager.productionLineBaseDataDic[heroData.placeID].name;
                else if (GameDataManager.placeBaseDataDic.ContainsKey(heroData.placeID))
                    placeName = GameDataManager.placeBaseDataDic[heroData.placeID].name;


                Text text = BattlePanel.GetComponentInChildren<Text>();
                text.text = placeName;
                BattlePanel.SetActive(true);
            }
            else if(HeroManager.heroDataDic[id].heroType == HeroData.HeroType.Battle)
            {
                if (heroData != null && string.IsNullOrEmpty(heroData.battleGroupID) == false)
                {
                    string dungeonName = GameDataManager.dungeonBaseDataDic[Battle.battleGroupList.Find(x => x.battleType == heroData.battleGroupID).dungeonID].dungeonName;
                    Text text = BattlePanel.GetComponentInChildren<Text>();
                    text.text = dungeonName;
                    BattlePanel.SetActive(true);
                }
                else
                    BattlePanel.SetActive(false);
            }
            else
            {
                BattlePanel.SetActive(false);
            }
        }
        else
        {
            BattlePanel.SetActive(false);
        }

    }

    void ResetSlotData(string id)
    {
        if(id == heroData.id)
        {
            Debug.Log("리셋시작");
            InitImage();
            SlotDataInit(heroData.id, state);
        }
    }
     
    public void SlotDataInit(string _id, HeroSlotState _state)
    {
        id = _id;

        state = _state;

        //heroData.onChangedValue += OnChangedHeroParam;

        InitUI();

        if (newChecker != null)
        {
            newChecker.SetActive(!heroData.isChecked);
        }
        
    }

    Color originalBGColor;
    void Start()
    {
        originalBGColor = bg.color;        
    }

    public void UpdateSlotContents()
    {
        if (updateBehavior != null)
        {
            updateBehavior.UpdateContents();
        }
        else
        {
            Debug.Log("updateBehavior is null");
        }
    }

    public void ChangeState(HeroSlotState _state, IHeroSlotClickable _clickBehavior = null)
    {
        state = _state;
        clickBehavior = _clickBehavior;
    }
    
    public void InitImage(Sprite sprite = null)
    {        
        if(sprite == null)
            AssetLoader.AssignImage(heroImage, "sprite/hero", "Atlas_HeroImage", heroData.heroImageName);        
        else
            heroImage.sprite = sprite;
    }

    void InitUI()
    {
        if(heroData == null)
        {
            Debug.LogError("not defined hero data");
            return;
        }

        ShowGrade(heroData.heroGrade);
        heroNameText.text = heroData.heroName;

        if (heroData.enhance != 0)
            textEnhance.text = "+" + heroData.enhance;
        else
            textEnhance.text = "";

        if(textRebirth != null)
        {
            if (heroData.rebirth != 0)
                textRebirth.text = heroData.rebirth.ToString();
            else
                textRebirth.text = "";
        }
    }

    // 영웅 slot에 등급 만큼 별 이미지 표시
    void ShowGrade(int grade)
    {
        foreach (var i in gradeArray)
        {
            i.SetActive(false);
        }
        if(grade < 5 && grade > 0)
            gradeArray[grade - 1].SetActive(true);
        else
            gradeArray[0].SetActive(true);

        if (showGradeColor)
        {
            if (grade < 1 || grade > colorArray.Length)
                bg.color = Color.gray;

            bg.color = colorArray[grade - 1];
        }
        else
        {
            //Color 안 보이기 하면 그냥 시작 색으로
            bg.color = originalBGColor;
        }
    }


    [SerializeField]
    public GameObject BattlePanel;

    [SerializeField]
    public GameObject AddPanel;

}

