using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;
using LitJson;
using CodeStage.AntiCheat.ObscuredTypes;

public class BattleQuestController : MonoBehaviour {

    /// <summary> unlockableInex 변경 될 때 발생 </summary>
    public SimpleDelegate onChangedUnlockableIndex;

    /// <summary> 자동퀘스트  변경 될 때 발생 </summary>
    public SimpleDelegate onChangedAutoQuest;

    public List<BattleQuest> battleQuestList = new List<BattleQuest>();
    
    public BattleGroup battleGroup { get; set; }

    public double totalIncome = 0d;

    [System.NonSerialized]
    public bool isInitialized = false;

    ObscuredInt _unlockableIndex = 0;
    /// <summary> 현재 unlock 가능한 퀘스트의 index </summary>
    public ObscuredInt unlockableIndex
    {
        get { return _unlockableIndex; }
        set
        {
            bool isChanged = _unlockableIndex != value;

            _unlockableIndex = value;

            if (isChanged && onChangedUnlockableIndex != null)
                onChangedUnlockableIndex();
        }
    }

    BattleQuest _currentBattleQuestAuto = null;
    /// <summary> 지금 해금 가능한 자동퀘스트 </summary>
    public BattleQuest currentBattleQuestAuto
    {
        get { return _currentBattleQuestAuto; }
        set
        {
            bool isChanged = _currentBattleQuestAuto != value;
            
            _currentBattleQuestAuto = value;

            if (isChanged && onChangedAutoQuest != null)
                onChangedAutoQuest();
        }
    }

    void SaveDataInfo()
    {

    }

    public void SaveQuestData(BattleQuest quest)
    {
        BattleQuestSaveDataQuest data = quest.saveData;
        if(data == null)
        {
            data = new BattleQuestSaveDataQuest(quest);
            quest.saveData = data;
        }

        quest.lastSaveTime = Time.time;

        data.InitData(quest);
        
        JsonWriter jsonWriter = new JsonWriter();
        jsonWriter.PrettyPrint = true;
        JsonMapper.ToJson(data.saveForm, jsonWriter);
        string json = jsonWriter.ToString();
        //string json2 = JsonUtility.ToJson(data);  //이걸로 하면 몇몇 타입의 필드가 json에서 누락됨

        string fileName = GetSaveFileName(quest);

        File.WriteAllText(fileName, json);
    }

    public void LoadData()
    {
        for(int i = 0; i < battleQuestList.Count; i++)
        {
            BattleQuest quest = battleQuestList[i];
            string fileName = GetSaveFileName(quest);
            
            if (!File.Exists(fileName))
                continue;

            string json = File.ReadAllText(fileName);

            //Debug.Log("[Load]" + fileName + ", " + json);

            JsonData jsonData = JsonMapper.ToObject(json);

            string id = JsonParser.ToString(jsonData["id"]);
            if (id != quest.baseData.id)
                continue;

            int level = JsonParser.ToInt(jsonData["level"]);
            //float startTime = JsonParser.ToFloat(jsonData["startTime"]);
            float progress = JsonParser.ToFloat(jsonData["progress"]);
            //bool isAutoRepeat = false;
            //if (jsonData.ContainsKey("isAutoRepeat"))
            bool isAutoRepeat = JsonParser.ToBool(jsonData["isAutoRepeat"]);

            quest.level = level;
            //quest.startTime = startTime;
            quest.progress = progress;
            quest.isAutoRepeat = isAutoRepeat;

            quest.startTime = Time.time - quest.baseData.time * quest.progress;
            //Debug.Log(quest.isAutoRepeat + ", " + quest.progress);
            

            if (quest.isAutoRepeat || quest.progress > 0f)
                StartQuest(quest);

        }
    }

    string GetSaveFileName(BattleQuest quest)
    {
        string fileName = Application.persistentDataPath + "/" + battleGroup.battleType + "_" + User.Instance.userID + "_" + quest.baseData.id + ".dat";
        return fileName;
    }

    void ClearSaveData()
    {
        for (int i = 0; i < battleQuestList.Count; i++)
        {
            string fileName = GetSaveFileName(battleQuestList[i]);
            if (File.Exists(fileName))
                File.Delete(fileName);
        }
    }
    
    //###################################################################################
    void Awake()
    {
        battleGroup = GetComponent<BattleGroup>();

        battleGroup.onRestartBattle += OnRestartBattle;

        InitData();
    }

    void InitData()
    {
        List<BattleQuestBaseData> baseDataList = GameDataManager.battleQuestBaseDataDic.Values.ToList();
        for(int i = 0; i < baseDataList.Count; i++)
        {
            BattleQuestBaseData baseData = baseDataList[i];

            BattleQuest quest = new BattleQuest(this);
            quest.baseData = baseData;

            quest.onUnlocked += UpdateUnlockableIndex;

            battleQuestList.Add(quest);
        }

        //UpdateAutoQuest();

        isInitialized = true;
    }

    IEnumerator Start()
    {
        while (!battleGroup.isInitialized)
            yield return null;

        while (!UIBattleQuest.isInitialized)
            yield return null;

        //데이타 로드
        LoadData();

        //해금 가능한 퀘스트 갱신
        UpdateUnlockableIndex();

        //해금 가능한 자동 진행 퀘스트 갱신
        UpdateAutoQuest();

    }

    void OnRestartBattle(BattleGroup b)
    {
        for (int i = 0; i < battleQuestList.Count; i++)
        {
            BattleQuest quest = battleQuestList[i];
            if(quest.baseData.tier == 1)
                quest.level = 1;
            else
                quest.level = 0;

            quest.isAutoRepeat = false;
        }

        currentBattleQuestAuto = battleQuestList[0];

        unlockableIndex = 1;

        totalIncome = 0d;

        ClearSaveData();
    }

    /// <summary> 현재 해금 가능한 퀘스트 갱신 </summary>
    void UpdateUnlockableIndex()
    {
        int a = int.MaxValue;
        for (int i = 0; i < battleQuestList.Count; i++)
        {
            if (battleQuestList[i].level == 0)
            {
                a = i;
                break;
            }
        }

        unlockableIndex = a;
    }

    /// <summary> 현재 해금 가능한 자동 퀘스트 갱신 </summary>
    void UpdateAutoQuest()
    {
        int index = -1;
        for (int i = 0; i < battleQuestList.Count; i++)
        {
            if (!battleQuestList[i].isAutoRepeat)
            {
                index = i;
                break;
            }
        }

        if (index == -1)
            currentBattleQuestAuto = null;
        else
            currentBattleQuestAuto = battleQuestList[index];
    }

    /// <summary> 퀘스트 시작 </summary>
    public void StartQuest(BattleQuest battleQuest)
    {
        if (battleQuest.coroutineDoQuest != null)
            return;

        float startTime = Time.time;
        if (battleQuest.progress > 0f)
            startTime = Time.time - battleQuest.baseData.time * battleQuest.progress;

        battleQuest.coroutineDoQuest = StartCoroutine(battleQuest.DoQuest(startTime));
    }

    public void UnlockQuest(BattleQuest battleQuest)
    {
        //업그레이드 비용
        double cost = battleQuest.unlockCost;

        //비용 모자라면 아무일 없음
        if (battleGroup.battleLevelUpController.totalExp < cost)
            return;

        //비용 차감
        battleGroup.battleLevelUpController.totalExp -= cost;

        //레벨 증가
        battleQuest.level = 1;

        //데이타 저장
        SaveQuestData(battleQuest);

        UpdateUnlockableIndex();
    }

    /// <summary> 업그레이드 </summary>
    public void Upgrade(BattleQuest battleQuest, int upgradeAmount = 1)
    {
        //if (battleQuest.level == 1)
        //    UnlockQuest(battleQuest);

        //업그레이드 비용
        double upgradeCost = battleQuest.GetUpgradeCost(upgradeAmount);

        //비용 모자라면 아무일 없음
        if (battleGroup.battleLevelUpController.totalExp < upgradeCost)
            return;

        //비용 차감
        battleGroup.battleLevelUpController.totalExp -= upgradeCost;

        //레벨 증가
        battleQuest.level += upgradeAmount;

        //데이타 저장
        SaveQuestData(battleQuest);

        //if (battleQuest.level == 1)
        //    UpdateUnlockableIndex();
    }

    /// <summary> 퀘스트 자동 실행 해금 </summary>
    public void UnlockAutoQuestGold()
    {
        if (currentBattleQuestAuto == null)
            return;

        double autoQuestCost = currentBattleQuestAuto.autoQuestCost;

        if (autoQuestCost > battleGroup.battleLevelUpController.totalExp)
            return;

        //비용 차감
        battleGroup.battleLevelUpController.totalExp -= autoQuestCost;

        //자동 실행 on
        currentBattleQuestAuto.isAutoRepeat = true;
        
        //데이타 저장
        SaveQuestData(currentBattleQuestAuto);

        int curIndex = battleQuestList.IndexOf(currentBattleQuestAuto);

        if(curIndex + 1 < battleQuestList.Count)
            currentBattleQuestAuto = battleQuestList[curIndex + 1];
    }

    /// <summary> 퀘스트 자동 실행 루비로 해금 </summary>
    public void UnlockAutoQuestRuby()
    {
        StartCoroutine(UnlockAutoQuestRubyA());
    }

    IEnumerator UnlockAutoQuestRubyA()
    {
        if (currentBattleQuestAuto == null)
            yield break;

        //클라에서 먼저 한 번 검사
        double autoQuestCost = currentBattleQuestAuto.autoQuestCostRuby;
        if (autoQuestCost > MoneyManager.GetMoney(MoneyType.ruby).value)
        {
            UIPopupManager.ShowOKPopup("", "루비가 부족합니다.", null);
            yield break;
        }
            

        //서버 통신
        LoadingManager.Show();

        string result = null;
        string php = "Battle.php";
        WWWForm form = new WWWForm();
        form.AddField("type", 3);
        form.AddField("userID", User.Instance.userID);
        form.AddField("questID", currentBattleQuestAuto.baseData.id);

        yield return StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(php, form, x => result = x));

        LoadingManager.Close();

        //실제로 가지고 있는 루비 부족하면 실패 처리
        if (result == "false")
        {
            UIPopupManager.ShowOKPopup("", "루비가 부족합니다.", null);
            yield break;
        }

        //자동 실행 on
        currentBattleQuestAuto.isAutoRepeat = true;        

        //데이타 저장
        SaveQuestData(currentBattleQuestAuto);

        int curIndex = battleQuestList.IndexOf(currentBattleQuestAuto);

        if (curIndex + 1 < battleQuestList.Count)
            currentBattleQuestAuto = battleQuestList[curIndex + 1];
    }

    public void AddIncome(double income)
    {
        totalIncome += income;
        battleGroup.battleLevelUpController.totalExp += income;

        if(UIBattle.battleMenuState == UIBattle.BattleMenuState.Info)
            UIBattleInfo.UpdateQuestInfo();
    }
}
