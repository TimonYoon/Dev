using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class UIBattleQuest : MonoBehaviour {
    static UIBattleQuest Instance;

    List<BattleQuest> battleQuestList;

    List<UIBattleQuestSlot> BattleQuestSlots = new List<UIBattleQuestSlot>();
    
    BattleQuestController battleQuestController
    {
        get
        {
            return Battle.currentBattleGroup.battleQuestController;
        }
    }

    public GameObject battleQuestSlotPrefab;
    public RectTransform rectTransformContent;

    public Image imageAutoQuestIcon;
    public Text textAutoQuestName;
    public Text textAutoQuestCost;
    public Text textAutoQuestCostRuby;
    public Button buttonAutoQuestGold;
    public Button buttonAutoQuestRuby;

    ///// <summary> 지금 해금 가능한 자동퀘스트 </summary>
    //BattleQuest currentBattleQuestAuto;

    static public bool isInitialized = false;
    //##############################################
    void Awake()
    {
        if (!GameDataManager.Instance)
            return;

        //자동 실행(루비) 텍스트 시리얼라이즈 안 되어 있어서 이렇게 가져옴
        textAutoQuestCostRuby = buttonAutoQuestRuby.GetComponentInChildren<Text>();

        Instance = this;

        Battle.battleGroupList.onAdd += OnAddBattleGroup;

        Battle.onChangedBattleGroup += OnChangedBattleGroup;
    }

    void OnAddBattleGroup(BattleGroup b)
    {
        if (coroutineInit != null)
            return;

        coroutineInit = StartCoroutine(Init());
    }

    Coroutine coroutineInit = null;
    IEnumerator Init()
    {
        while (!Battle.currentBattleGroup || !battleQuestController || !battleQuestController.isInitialized)
            yield return null;

        battleQuestList = battleQuestController.battleQuestList;

        //퀘스트 슬롯들 생성 (배운거 안 배운거 전부 포함)
        for(int i = 0; i < battleQuestList.Count; i++)
        {
            GameObject go = Instantiate(battleQuestSlotPrefab, rectTransformContent);
            UIBattleQuestSlot slot = go.GetComponent<UIBattleQuestSlot>();
            slot.index = i;
            slot.battleQuest = battleQuestList[i];
            BattleQuestSlots.Add(slot);
        }

        UpdateAutoQuestInfo();

        battleQuestController.onChangedAutoQuest += OnChangedAutoQuest;
        Battle.currentBattleGroup.battleLevelUpController.onChangedTotalExp += OnChangedTotalExp;


        //그리드 사이즈 조절. 컨텐츠 사이즈 필터 못써서 취한 조치
        GridLayoutGroup grid = rectTransformContent.GetComponent<GridLayoutGroup>();// layoutGroup as GridLayoutGroup;
        if (grid)
        {
            int childCount = rectTransformContent.childCount;
            float y = grid.cellSize.y * childCount + grid.spacing.y * (childCount - 1);
            rectTransformContent.sizeDelta = new Vector2(rectTransformContent.sizeDelta.x, y);
        }

        coroutineInit = null;

        isInitialized = true;
    }

    void OnChangedTotalExp(BattleGroup b)
    {
        BattleQuest quest = battleQuestController.currentBattleQuestAuto;
        if (quest == null)
            return;

        //자동 퀘스트 버튼 활성/비활성
        buttonAutoQuestGold.interactable = quest.level > 0 && quest.autoQuestCost <= b.battleLevelUpController.totalExp;
    }

    void OnChangedBattleGroup(BattleGroup b)
    {
    }

    void OnChangedAutoQuest()
    {
        UpdateAutoQuestInfo();
    }

    void UpdateAutoQuestInfo()
    {
        BattleQuest quest = battleQuestController.currentBattleQuestAuto;
        if (quest == null)
        {
            buttonAutoQuestGold.interactable = false;
            buttonAutoQuestRuby.interactable = false;
            return;
        }

        //이미지
        AssetLoader.AssignImage(imageAutoQuestIcon, "sprite/quest", "Atlas_Quest_Icon", quest.baseData.image);

        //퀘스트 이름, 해금 비용(골드, 루비) 표시
        textAutoQuestName.text = quest.baseData.name;
        double autoQuestCost = quest.autoQuestCost;
        textAutoQuestCost.text = autoQuestCost.ToStringABC();
        textAutoQuestCostRuby.text = quest.autoQuestCostRuby.ToString();

        buttonAutoQuestGold.interactable = quest.level > 0;
        buttonAutoQuestRuby.interactable = quest.level > 0;

        quest.onUnlocked += OnUnlockedCureentAutoQuest;
    }

    void OnUnlockedCureentAutoQuest()
    {
        UpdateAutoQuestInfo();
    }

    /// <summary> 자동퀘스트 골드로 해금 </summary>
    public void OnClickAutoQuestGold()
    {        
        battleQuestController.UnlockAutoQuestGold();
    }

    /// <summary> 자동퀘스트 루비로 해금 </summary>
    public void OnClickAutoQuestRuby()
    {
        battleQuestController.UnlockAutoQuestRuby();
    }
}
