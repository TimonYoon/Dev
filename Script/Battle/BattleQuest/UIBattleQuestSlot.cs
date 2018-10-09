using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBattleQuestSlot : MonoBehaviour {

    BattleQuestController battleQuestController
    {
        get
        {
            if (!Battle.Instance || !Battle.currentBattleGroup)
                return null;

            return Battle.currentBattleGroup.battleQuestController;
        }
    }

    BattleLevelUpController battleLevelUpController
    {
        get
        {
            if (!Battle.Instance || !Battle.currentBattleGroup)
                return null;

            return battleQuestController.battleGroup.battleLevelUpController;
        }
    }

    /// <summary> 정렬 순서. 초기화 할 때 지정됨 </summary>
    public int index { get; set; }

    /// <summary> 아이콘 </summary>
    public Image imageIcon;

    public Image imageProgress;

    /// <summary> 퀘스트 이름 </summary>
    public Text textName;

    /// <summary> 퀘스트 현재 레벨 </summary>
    public Text textLevel;

    /// <summary> 완료 시 수익 </summary>
    public Text textIncome;

    /// <summary> 남은 시간 </summary>
    public Text textRemainingTime;

    /// <summary> 해금 비용 </summary>
    public Text textUnlockCost;

    /// <summary> 업그레이드 비용 </summary>
    public Text textUpgradeCost;

    /// <summary> 업그레이드 시 증가하는 수익 </summary>
    public Text textIncomeGrowth;

    /// <summary> 퀘스트 시작 버튼 </summary>
    public Button buttonStartQuest;

    /// <summary> 업그레이드 버튼 </summary>
    public Button buttonUpgrade;

    /// <summary> 업그레이드 버튼 10레벨 증가 </summary>
    public Button buttonUpgrade10;

    /// <summary> 10레벨 증가 비용 </summary>
    public Text textUpgradeCost10;

    /// <summary> 최대 업그레이드 가능한 레벨 </summary>
    public Text textUpgradeLevelMax;

    /// <summary> 최대 레벨 업그레이드 시 비용 </summary>
    public Text textUpgradeCostMax;

    /// <summary> 업그레이드 버튼 최대 </summary>
    public Button buttonUpgradeMax;

    /// <summary> 잠김 상태일 때 보여줄 오브젝트 </summary>
    public GameObject toggledObjectLocked;

    /// <summary> 퀘스트 시작 표시 오브젝트 </summary>
    public GameObject toggledObjectStartQuest;



    BattleQuest _battleQuest;
    public BattleQuest battleQuest
    {
        get
        {
            return _battleQuest;
        }
        set
        {
            if(_battleQuest != null)
            {
                _battleQuest.onUnlocked -= OnUnlocked;
                _battleQuest.onChangedLevel -= OnChangedLevel;
                _battleQuest.onFinished -= OnFinishedQuest;
                _battleQuest.onUnlockedAuto -= OnUnlockedAuto;
                _battleQuest.onChangedModifyValue -= UpdateStats;
                battleQuestController.onChangedUnlockableIndex -= OnChangedUnlockIndex;
                battleLevelUpController.onChangedTotalExp -= OnChangedTotalExp;
            }

            _battleQuest = value;

            if (value != null)
            {
                value.onUnlocked += OnUnlocked;
                value.onChangedLevel += OnChangedLevel;
                value.onFinished += OnFinishedQuest;
                value.onUnlockedAuto += OnUnlockedAuto;
                value.onChangedModifyValue += UpdateStats;
                battleQuestController.onChangedUnlockableIndex += OnChangedUnlockIndex;
                battleLevelUpController.onChangedTotalExp += OnChangedTotalExp;
            }

            //퀘스트 이름
            textName.text = value.baseData.name;

            //퀘스트 레벨
            textLevel.text = value.level.ToString();

            //수행 시간            
            textRemainingTime.text = SecChangeToDateTime(value.baseData.time);

            //아이콘
            AssetLoader.AssignImage(imageIcon, "sprite/quest", "Atlas_Quest_Icon", battleQuest.baseData.image);

            UpdateStats();

            UpdateUnlockState();
        }
    }

    //#######################################################

    void Awake()
    {
        buttonUpgrade10.gameObject.SetActive(false);
        buttonUpgradeMax.gameObject.SetActive(false);
    }

    /// <summary> 경험치 총량이 변경 되었을 때 </summary>
    void OnChangedTotalExp(BattleGroup battleGroup)
    {
        UpdateUnlockState();
    }

    /// <summary> 해금되었을 때 </summary>
    void OnUnlocked()
    {
        //시작 버튼 활성
        buttonStartQuest.interactable = true;
    }

    /// <summary> 오토 활성화 될 때 </summary>
    void OnUnlockedAuto()
    {
        //Debug.Log(battleQuest.baseData.id + ", " + battleQuest.isAutoRepeat);

        //퀘스트 시작 버튼 정보 갱신
        UpdateStartButtonState();
    }

    /// <summary> 레벨 변경되었을 때 </summary>
    void OnChangedLevel()
    {
        //퀘스트 레벨 갱신
        textLevel.text = battleQuest.level.ToString();

        //각종 수치 갱신
        UpdateStats();
    }

    /// <summary> 각종 수치들 갱신 </summary>
    void UpdateStats()
    {
        //완료 시 수익
        double income = battleQuest.income;
        textIncome.text = income.ToStringABC();
        
        //해금 비용
        textUnlockCost.gameObject.SetActive(battleQuest.level == 0);
        if (battleQuest.level == 0)
        {
            double unlockCost = battleQuest.unlockCost;
            textUnlockCost.text = unlockCost.ToStringABC();
        }
            

        //업그레이드 비용
        textUpgradeCost.gameObject.SetActive(battleQuest.level > 0);

        //업그레이드 시 수익 증가량
        textIncomeGrowth.gameObject.SetActive(battleQuest.level > 0);
        
        if (battleQuest.level > 0)
        {
            double upgradeCost = battleQuest.upgradeCost;
            textUpgradeCost.text = upgradeCost.ToStringABC();
            double incomeGrouth = battleQuest.incomeGrouth;
            textIncomeGrowth.text = incomeGrouth.ToStringABC();
        }
        
        //10레벨 증가 버튼 활성화되어 있으면
        if (buttonUpgrade10.gameObject.activeSelf)
        {
            double cost = battleQuest.GetUpgradeCost(10);
            if (cost > battleLevelUpController.totalExp)
                buttonUpgrade10.gameObject.SetActive(false);
            else
                textUpgradeCost10.text = cost.ToStringABC();
        }

        //100000레벨 증가 버튼 활성화되어 있으면
        if (buttonUpgradeMax.gameObject.activeSelf)
        {
            double cost = battleQuest.GetUpgradeCost(maxLevel);
            if(!double.IsNaN(cost) && cost > battleLevelUpController.totalExp)
                buttonUpgradeMax.gameObject.SetActive(false);
            else
                textUpgradeCostMax.text = cost.ToStringABC();
        }
    }
    
    int lastRemainingTime = int.MinValue;
    void Update()
    {
        if (battleQuest.level < 1)
        {
            imageProgress.fillAmount = 0f;
            buttonStartQuest.interactable = false;
            return;
        }

        if (battleQuest.progress <= 0f)
        {
            imageProgress.fillAmount = 0f;
            return;
        }   

        imageProgress.fillAmount = battleQuest.progress;
        
        //게이지 증가
        float elapsedTime = Time.time - battleQuest.startTime;

        int remainingTime = (int)( battleQuest.baseData.time - elapsedTime);

        if(remainingTime != lastRemainingTime)
        {
            textRemainingTime.text = SecChangeToDateTime(remainingTime);
            lastRemainingTime = remainingTime;
        }
    }

    void OnFinishedQuest()
    {
        if(!battleQuest.isAutoRepeat)
            buttonStartQuest.interactable = true;

        //수행 시간            
        textRemainingTime.text = SecChangeToDateTime(battleQuest.baseData.time);

    }

    void OnChangedUnlockIndex()
    {
        UpdateUnlockState();
    }

    void UpdateStartButtonState()
    {
        buttonStartQuest.interactable = !battleQuest.isAutoRepeat && battleQuest.progress == 0f;
    }

    void UpdateUnlockState()
    {
        bool isUnlocked = index < battleQuestController.unlockableIndex;
        if (isUnlocked)
        {
            buttonUpgrade.interactable = battleQuest.upgradeCost <= battleLevelUpController.totalExp;

            toggledObjectLocked.SetActive(false);
        }
        else
        {
            bool canUnlock = index == battleQuestController.unlockableIndex;
            if (canUnlock)
                buttonUpgrade.interactable = battleQuest.unlockCost <= battleLevelUpController.totalExp;
            else
                buttonUpgrade.interactable = false;

            toggledObjectLocked.SetActive(true);
        }
    }
    
    /// <summary> 퀘스트 시작 버튼 눌렀을 때 </summary>
    public void OnClickStartQuest()
    {
        buttonStartQuest.interactable = false;
        battleQuestController.StartQuest(battleQuest);
    }

    /// <summary> 업그레이드 버튼 눌렀을 때 </summary>
    public void OnClickUpgrade()
    {
        if (battleQuest.level < 1)
            battleQuestController.UnlockQuest(battleQuest);
        else
        {
            battleQuestController.Upgrade(battleQuest);

            lastClickUpgradeTime = Time.time;

            if (coroutineShowUpgradeButtons == null)
                coroutineShowUpgradeButtons = StartCoroutine(ShowUpgradeButtons());
        }
    }

    void OnDisable()
    {
        buttonUpgrade10.gameObject.SetActive(false);
        buttonUpgradeMax.gameObject.SetActive(false);
        if (coroutineShowUpgradeButtons != null)
        {
            StopCoroutine(ShowUpgradeButtons());
            coroutineShowUpgradeButtons = null;
        }
    }

    /// <summary> 업그레이드 버튼 눌렀을 때 </summary>
    public void OnClickUpgrade10()
    {
        battleQuestController.Upgrade(battleQuest, 10);

        lastClickUpgradeTime = Time.time;
    }

    /// <summary> 업그레이드 버튼 눌렀을 때 </summary>
    public void OnClickUpgradeMax()
    {
        battleQuestController.Upgrade(battleQuest, maxLevel);

        lastClickUpgradeTime = Time.time;
    }

    float lastClickUpgradeTime = float.MinValue;

    int maxLevel = 0;

    Coroutine coroutineShowUpgradeButtons = null;
    IEnumerator ShowUpgradeButtons()
    {
        double upgradeCost10 = battleQuest.GetUpgradeCost(10);

        if(upgradeCost10 > battleLevelUpController.totalExp)
        {
            buttonUpgrade10.gameObject.SetActive(false);
            buttonUpgradeMax.gameObject.SetActive(false);

            coroutineShowUpgradeButtons = null;
            yield break;
        }

        //+10레벨
        buttonUpgrade10.gameObject.SetActive(true);
        textUpgradeCost10.text = upgradeCost10.ToStringABC();
        
        double totalExp = battleLevelUpController.totalExp;

        int[] maxLevels = new int[] { 1000000000, 100000000, 10000000, 1000000, 100000, 10000, 1000, 100 };

        maxLevel = 0;
        double maxCost = -1;
        for (int i = 0; i < maxLevels.Length; i++)
        {
            maxCost = battleQuest.GetUpgradeCost(maxLevels[i]);
            if (double.IsNaN(maxCost))
                continue;

            if (maxCost < totalExp)
            {
                maxLevel = maxLevels[i];
                break;
            }   
        }

        //+100000레벨
        if(maxLevel > 0)
        {
            buttonUpgradeMax.gameObject.SetActive(true);
            textUpgradeLevelMax.text = maxLevel.ToString();
            textUpgradeCostMax.text = maxCost.ToStringABC();
        }
        else
        {
            buttonUpgradeMax.gameObject.SetActive(false);
        }

        while (Time.time - lastClickUpgradeTime < 5f)
        {
            yield return null;
        }

        buttonUpgrade10.gameObject.SetActive(false);
        buttonUpgradeMax.gameObject.SetActive(false);

        coroutineShowUpgradeButtons = null;
        yield break;
    }

    /// <summary> 초를 날/시/분/초 로 바꿈 </summary>
    string SecChangeToDateTime(float time)
    {
        int second = (int)time;
        int hour = 0;
        int minute = 0;

        bool isChack = false;
        while (true)
        {
            if (second > 59)
            {
                minute++;
                second -= 60;
                if (minute > 59)
                {
                    isChack = true;
                    hour++;
                    minute = 0;
                }
            }
            else
            {
                if (!isChack)
                {
                    string result = string.Format("{0:00} : {1:00}", minute, second);
                    return result;
                }
                else
                {
                    string result = string.Format("{0:00} : {1:00} : {2:00}", hour, minute, second);
                    return result;
                }

            }
        }
    }
}
