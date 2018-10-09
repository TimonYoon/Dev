using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LitJson;
using System.IO;




/// <summary> 전투와 관련된 전투 정보 </summary>
public class Battle : MonoBehaviour {
    public static Battle Instance { get; private set; }

    #region 콜백들
    public delegate void BattleCallback();

    /// <summary> 배틀그룹 리스트에 새로운 그룹이 추가되거나 제거되었을 때 </summary>
    static public BattleCallback onChangedBattleGroupList;

    /// <summary> CurrnetBattleGroupID가 변동되었을 때 콜백 </summary>
    static public BattleCallback onChangedCurrentBattleGroupID;

    /// <summary> 배틀그룹의 isActive가 변동되었을 때 콜백 </summary>
    static public BattleCallback onChangedBattleGroupActiveState;

    #endregion

    #region 리소스세팅
    public GameObject battleGroupPrefab;
    public Transform battleList;
    //public Camera battleCamera;
    public Canvas battleCanvas;


    public GameObject _speechBubblePrefab;
    static public GameObject speechBubblePrefab { get { return Instance._speechBubblePrefab; } }

    public GameObject _tombstonePrefab;
    static public GameObject tombstonePrefab { get { return Instance._tombstonePrefab; } }
    
    public GameObject _levelUpPrefab;
    static public GameObject levelUpPrefab { get { return Instance._levelUpPrefab; } }

    public GameObject _hpGaugePrefab;
    static public GameObject hpGaugePrefab { get { return Instance._hpGaugePrefab; } }

    public GameObject _damageTextPrefab;
    static public GameObject damageTextPrefab { get { return Instance._damageTextPrefab; } }


    #endregion


    static string _currentBattleGroupID = "";
    /// <summary> 현재 표시되고 있는 배틀 그룹 ID </summary>
    public static string currentBattleGroupID
    {
        get
        {
            return _currentBattleGroupID;
        }
        private set
        {
            if (_currentBattleGroupID == value)
                return;
                        
            _currentBattleGroupID = value;
            if (onChangedCurrentBattleGroupID != null)
                onChangedCurrentBattleGroupID();
            
        }
    }

    static BattleGroup _currentBattleGroup;
    static public BattleGroup currentBattleGroup
    {
        get
        {
            return _currentBattleGroup;
        }
        set
        {
            _currentBattleGroup = value;

            if(onChangedBattleGroup != null)
                onChangedBattleGroup(value);
        }
    }

    /// <summary> 현재 전투가 진행 중인 battleGroup 리스트 </summary>
    public static CustomList<BattleGroup> battleGroupList = new CustomList<BattleGroup>();// { get; private set; }

    public static Dictionary<string, List<GameObject>> objectPool = new Dictionary<string, List<GameObject>>();

    public static GameObject GetObjectInPool(string name)
    {
        GameObject go = null;
        if (objectPool.ContainsKey(name))
        {   
            List<GameObject> list = objectPool[name];
            if (list == null)
                list = new List<GameObject>();

            go = list.Find(x => x != null && x.name == name && !x.gameObject.activeSelf);
        }

        return go;
    }

    public static void AddObjectToPool(GameObject go)
    {
        if (objectPool.ContainsKey(go.name))
        {
            List<GameObject> list = objectPool[go.name];
            if (list == null)
                list = new List<GameObject>();

            list.Add(go);
        }
        else
        {
            List<GameObject> list = new List<GameObject>();
            list.Add(go);

            objectPool.Add(go.name, list);
        }
    }


    public bool isInitialized = false;

    //################################################################################################################
    void Awake()
    {
        Instance = this;
    }
    
    IEnumerator Start()
    {
        isInitialized = false;
        
        //영지 데이타 초기화 될 때 까지 대기
        //while (Feudatory.feudatoryBasicDataList == null || Feudatory.feudatoryBasicDataList.Count == 0)
        //    yield return null;        

        //전부 다 닫기
        OnClickCloseButton();

        while (!SceneLobby.Instance)
            yield return null;

        SceneLobby.Instance.OnChangedMenu += OnChangedMenu;

        //몬스터풀 초기화 대기
        while (!actorPool.Instance.isInitialization)
            yield return null;

        while (HeroManager.heroDataList.Count == 0)
            yield return null;


        //데이타 불러오기
        LoadHeroProficiency();//영웅 수련치 로컬 저장 데이터 불러옴
        LoadAndCreateBattleGroups();

        //모든 배틀그룹 초기화될 때 까지 대기
        while (battleGroupList.Find(x => x.isInitialized == false))
            yield return null;

        isInitialized = true;

        //첫번째 배틀그룹 전투화면 보여주기
        //if (battleGroupList.Count > 0)
        //    SceneLobby.Instance.SceneChange(LobbyState.Battle);
        Show();
    }

    void OnChangedMenu(LobbyState state)
    {
        if (state != LobbyState.Battle && state != LobbyState.BattlePreparation)
            Close();
        else
            Show();
    }
        
    static public string lastBattleGroupID = "Battle_1";

    void Show()
    {
        ShowBattle(lastBattleGroupID);
    }

    void Close()
    {
        if(currentBattleGroup)
            lastBattleGroupID = currentBattleGroup.battleType;

        ShowBattle(false);

    }
       
    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.F5))
        //{
        //    ShowBattlePreparation(currentBattleGroup.id, currentBattleGroup.originalMember);
        //}        

        //if (Input.GetKeyDown(KeyCode.F9))
        //{
        //    // 데이터 삭제
        //    StartCoroutine(SaveLoadManager.Clear(SaveType.Battle));
        //}

        //if (Input.GetKeyDown(KeyCode.F5))
        //{
        //    SaveTest();
        //}
        //if (Input.GetKeyDown(KeyCode.F6))
        //{
        //    string fileName = Application.persistentDataPath + "/Battle_2_Stage.dat";
        //    File.Delete(fileName);
        //}

    }

    public List<string> battleIDList = new List<string>(new string[] { "Battle_1", "Battle_2", "Battle_3" });

    static public void SaveStageInfo(BattleGroup battleGroup)
    {
        BattleSaveDataStage data = new BattleSaveDataStage(battleGroup);

        //stageData.id = "Battle_2";
        //Debug.Log("스테이지 저장");
        JsonWriter jsonWriter = new JsonWriter();
        jsonWriter.PrettyPrint = true;
        JsonMapper.ToJson(data, jsonWriter);
        string json = jsonWriter.ToString();
        //string json2 = JsonUtility.ToJson(data);  //이걸로 하면 몇몇 타입의 필드가 json에서 누락됨
        
        string fileName = Application.persistentDataPath + "/" + battleGroup.battleType + "_" + User.Instance.userID + "_Stage.dat";

        File.WriteAllText(fileName, json);
        SaveHeroProficiency(battleGroup);
    }
    static public void SaveHeroProficiency(BattleGroup battleGroup)
    {
        HeroProficiencySave data = new HeroProficiencySave(battleGroup);
        //Debug.Log("영웅 숙련도 저장");

        JsonWriter jsonWriter = new JsonWriter();
        jsonWriter.PrettyPrint = true;
        JsonMapper.ToJson(data, jsonWriter);
        string json = jsonWriter.ToString();

        string fileName = Application.persistentDataPath + "/" + User.Instance.userID + "_HeroProficiency.dat";

        File.WriteAllText(fileName, json);
    }

    static public void SaveArtifactInfo(BattleGroup battleGroup)
    {
        BattleSaveDataArtifact data = new BattleSaveDataArtifact(battleGroup);
        //Debug.Log("유물 저장");
        JsonWriter jsonWriter = new JsonWriter();
        jsonWriter.PrettyPrint = true;
        JsonMapper.ToJson(data, jsonWriter);
        string json = jsonWriter.ToString();
        //string json2 = JsonUtility.ToJson(data);  //이걸로 하면 몇몇 타입의 필드가 json에서 누락됨

        string fileName = Application.persistentDataPath + "/" + battleGroup.battleType + "_" + User.Instance.userID + "_Artifact.dat";

        File.WriteAllText(fileName, json);
    }

    static public void DeleteSaveData(BattleGroup battleGroup)
    {
        string[] ss = new string[] { "_Stage.dat", "_Hero.dat", "_Artifact.dat" };

        for(int i = 0; i < ss.Length; i++)
        {
            string fileName = Application.persistentDataPath + "/" + battleGroup.battleType + "_" + User.Instance.userID + ss[i];
            if (File.Exists(fileName))
                File.Delete(fileName);
        }
    }

    void SaveTest()
    {
        if (battleGroupList.Count == 0)
            return;

        BattleGroup b = battleGroupList[0];

        //------------------------- 테스트 -----------------------        
        BattleSaveDataStage stageData = null;
        if (b != null)
            stageData = new BattleSaveDataStage(b);

        stageData.id = "Battle_2";

        JsonWriter jsonWriter = new JsonWriter();
        jsonWriter.PrettyPrint = true;
        JsonMapper.ToJson(stageData, jsonWriter);
        string json = jsonWriter.ToString();
        //string json2 = JsonUtility.ToJson(data);  //이걸로 하면 몇몇 타입의 필드가 json에서 누락됨

        Debug.Log(json);

        string fileName = Application.persistentDataPath + "/" + stageData.id + "_" + User.Instance.userID + "_Stage.dat";

        File.WriteAllText(fileName, json);
    }

    void LoadTest()
    {
        string fileName = Application.persistentDataPath + "/" + "Battle_2" + "_" + User.Instance.userID + "_Stage.dat";

        if (!File.Exists(fileName))
            return;

        string json = File.ReadAllText(fileName);

        Debug.Log(json);

        BattleSaveDataStage saveData = new BattleSaveDataStage();
        
        saveData = JsonUtility.FromJson<BattleSaveDataStage>(json);
        
        JsonData jsonData = JsonMapper.ToObject(json);

        Debug.Log(jsonData["battleHeroList"]);

        Debug.Log(saveData.battleHeroList.Count);

    }

    void LoadHeroProficiency()
    {
        string fileName = Application.persistentDataPath + "/" + User.Instance.userID + "_HeroProficiency.dat";

        if (!File.Exists(fileName))
            return;

        string json = File.ReadAllText(fileName);

        //Debug.Log(json);
        //HeroProficiencySave saveData = JsonUtility.FromJson<HeroProficiencySave>(json);

        JsonData jsonData = JsonMapper.ToObject(json);

        JsonData heroListJsonData = jsonData["heroProficiencyTimeDic"];

        //Debug.Log(heroListJsonData.Count + "개 = 불러옴");
        //List<string> keys = new List<string>(saveData.heroProficiencyTimeDic.Keys);

        for (int i = 0; i < HeroManager.heroDataList.Count; i++)
        {
            HeroData heroData = HeroManager.heroDataList[i];
            if (heroListJsonData.ContainsKey(heroData.id))
            {
                float value = 0f;
                float.TryParse(heroListJsonData[heroData.id].ToStringJ(), out value);
                heroData.proficiencyTime = value;
                //Debug.Log(heroData.heroName + " / " + heroData.proficiencyTime);
            }            
        }       
    }

    void LoadAndCreateBattleGroups()
    {
        List<BattleSaveDataStage> battleSaveDataList = new List<BattleSaveDataStage>();

        for (int i = 0; i < battleIDList.Count; i++)
        {
            string fileName = Application.persistentDataPath + "/" + battleIDList[i] + "_" + User.Instance.userID + "_Stage.dat";

            if (!File.Exists(fileName))
            {
                //첫 실행 할 때 첫번째 배틀그룹에 콜라 내보내기
                if(i == 0)
                {
                    List<HeroData> list = new List<HeroData>();
                    HeroData heroDataBattle = HeroManager.heroDataList.Find(x => x.heroID == "Knight_02_Hero");
                    if (heroDataBattle != null)
                    {
                        list.Add(heroDataBattle);
                        CreateBattleGroup(battleIDList[0], list);
                    }
                }

                continue;
            }                

            string json = File.ReadAllText(fileName);

            //Debug.Log(json);

            BattleSaveDataStage saveData = JsonUtility.FromJson<BattleSaveDataStage>(json);
            battleSaveDataList.Add(saveData);

            List<HeroData> heroDataList = GetHeroDataListFromSaveData(json);
            if (heroDataList == null || heroDataList.Count == 0)
                continue;

            CreateBattleGroup(saveData.id, heroDataList, saveData.stage, saveData);
        }
    }

    List<HeroData> GetHeroDataListFromSaveData(string json)
    {
        if (string.IsNullOrEmpty(json))
            return null;

        if (HeroManager.heroDataDic == null)
            return null;

        JsonData jsonData = JsonMapper.ToObject(json);


        JsonData heroListJsonData = jsonData["battleHeroList"];

        List<HeroData> heroDataList = new List<HeroData>();
        for (int i = 0; i < heroListJsonData.Count; i++)
        {
            JsonData _jsonData = heroListJsonData[i];
            
            if (_jsonData["invenID"] == null)
            {
                Debug.LogWarning("Failed to load hero info from save data. 'invenID' is null or empty");
                continue;
            }

            string invenID = _jsonData["invenID"].ToString();

            if (HeroManager.heroDataDic.ContainsKey(invenID))
            {
                HeroData heroData = HeroManager.heroDataDic[invenID];
                heroData.level = _jsonData["level"].ToInt();
                if (_jsonData.ContainsKey("exp"))
                    heroData.exp = _jsonData["exp"].ToInt();

                heroDataList.Add(heroData);
            }
            else
            {
                Debug.Log("해당 영웅이 존재하지 않습니다. " + invenID);
            }
        }

        return heroDataList;
    }
    
        
    /// <summary> 전투가 진행중인 배틀 그룹을 씬에 생성 </summary>
    static public void CreateBattleGroup(string battleGroupID, List<HeroData> heroList, int stage = 1, BattleSaveDataStage battleSaveData = null)
    {
        if (!Instance)
            return;

        currentBattleGroupID = battleGroupID;
        
        GameObject go = Instantiate(Instance.battleGroupPrefab, Instance.battleList.position, Quaternion.identity, Instance.battleList) as GameObject;
        BattleGroup battleGroup = go.GetComponent<BattleGroup>();
        
        if (battleSaveData != null)
        {
            battleGroup.Init(battleGroupID, heroList, stage, battleSaveData);
            battleGroup.isRenderingActive = false;
        }
            
        else
        {
            battleGroup.Init(battleGroupID, heroList, stage);
        }

        //리스트에 추가
        battleGroupList.Add(battleGroup);

        if (onChangedBattleGroupList != null)
            onChangedBattleGroupList();
    }
    
    public static SimpleDelegate onRemoveBattle;

    /// <summary> 회군 </summary>
    static public void RemoveBattle()
    {
        // 회군 콜백
        if (onRemoveBattle != null)
            onRemoveBattle();

        for(int i = 0; i < currentBattleGroup.originalMember.Count; i++)
        {
            currentBattleGroup.originalMember[i].heroData.battleGroupID = "";
        }

        DeleteSaveData(currentBattleGroup);

        battleGroupList.Remove(currentBattleGroup);
        Destroy(currentBattleGroup.gameObject);
        
        if (battleGroupList.Count == 0)
            return;

        //첫번째 전투 화면 보여주기
        BattleGroup battleGroup = battleGroupList[0];
        ShowBattle(battleGroup.battleType);

    }

    /// <summary> 배틀창 닫기 버튼 </summary>
    public void OnClickCloseButton()
    {
        SceneLobby.Instance.SceneChange(LobbyState.Lobby);
        ShowBattle(false);
    }

    static public void ShowBattle(bool isVisible)
    {
        if (!isVisible)
        {
            for(int i = 0; i < battleGroupList.Count; i++)
            {
                battleGroupList[i].isRenderingActive = false;
            }
        }

        if(currentBattleGroup)
        {
            currentBattleGroup.isRenderingActive = isVisible;            
        }
            

        if (!Instance)
            return;
        
        Instance.battleCanvas.enabled = isVisible;

        if(isVisible)
        {
            if(currentBattleGroup)
                UIBattle.ShowBattlePanel(currentBattleGroup.battleType);
        }
    }
   
    public static void ShowBattle(string battleGroupID)
    {
        currentBattleGroupID = battleGroupID;

        //보여줄것 없으면 닫기?
        if (string.IsNullOrEmpty(battleGroupID))
        {
            ShowBattle(false);
            return;
        }

        //현재 선택된거 빼고는 전부 끄기
        for(int i  = 0; i < battleGroupList.Count; i++)
        {
            battleGroupList[i].isRenderingActive = battleGroupList[i].battleType == battleGroupID;
        }

        //현재 전투 진행중이지 않으면 탐험 편성창 열기
        BattleGroup battleGroup = battleGroupList.Find(x => x.battleType == battleGroupID);
        if (!battleGroup)
        {
            lastBattleGroupID = currentBattleGroup.battleType;
            ShowBattlePreparation(battleGroupID);
        }
        else
        {
            currentBattleGroup = battleGroup;
            ShowBattle(true);
        }
    }

    static public void ShowBattlePreparation(string battleGroupID, List<BattleHero> heroList = null)
    {
        if (Instance.coroutineShowBattlePreparation != null)
            return;

        Instance.coroutineShowBattlePreparation = Instance.StartCoroutine(Instance.ShowBattlePreparationA(battleGroupID, heroList));
    }

    Coroutine coroutineShowBattlePreparation = null;
    IEnumerator ShowBattlePreparationA(string battleGroupID, List<BattleHero> heroList = null)
    {
        string sceneName = "BattlePreparation";

        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.isLoaded)
        {
            while (!AssetLoader.Instance)
                yield return null;

            yield return StartCoroutine(AssetLoader.Instance.LoadLevelAsync("scene/battlepreparation", sceneName, true));
        }

        UIBattlePreparation.Show(battleGroupID, heroList);

        coroutineShowBattlePreparation = null;
    }

    public delegate void OnChangedBattleGroup(BattleGroup battleGroup);
    public static OnChangedBattleGroup onChangedBattleGroup;

    public bool showGUI = true;

    //bool boostSpeed = false;

    //[System.NonSerialized]
    //public float gameSpeed = 1f;
    ////------------------------------------------------------------------------------------------------
    //void OnGUI()
    //{
    //    //float lineHeight = 10f;
    //    //for(int i = 0; i < battleGroupList.Count; i++)
    //    //{
    //    //    GUI.Label(new Rect(10, lineHeight * i, 200, 20), battleGroupList[i].id.ToString() + ", " + battleGroupList[i].isActive.ToString());
    //    //}        

    //    if (!showGUI)
    //        return;

    //    if (battleGroupList.Count == 0)
    //        return;

    //    if (currentBattleGroup == null || !currentBattleGroup.isActive)
    //        return;



    //    GUIStyle blStyle = new GUIStyle();
    //    blStyle.normal.textColor = Color.white;
    //    blStyle.alignment = TextAnchor.MiddleCenter;

    //    float x = Screen.width * 0.5f;
    //    GUI.Label(new Rect(x - 100, 45, 200, 20), "게임 속도: " + gameSpeed + "배속", blStyle);
    //    gameSpeed = (int) GUI.HorizontalSlider(new Rect(x - 65f, 65, 130, 20), gameSpeed, 0f, 10f);

    //    //GUI.Label(new Rect(x - 100, 45, 200, 20), "게임 속도: " + gameSpeed + "배속", blStyle);
    //    boostSpeed = GUI.Toggle(new Rect(x + 80, 45, 130, 20), boostSpeed, "100 배속 모드!");
    //    if (boostSpeed)
    //    {
    //        Time.timeScale = 100f;
    //        return;
    //    }

    //    Time.timeScale = gameSpeed;

    //    // ==========버프 라인 ==========================================

    //    //float y = Screen.height * 0.5f;
    //    //if (currentBattleGroup.artifactController.artifactList != null && currentBattleGroup.artifactController.artifactList.Count > 0)
    //    //{
    //    //    for (int i = 0; i < currentBattleGroup.artifactController.artifactList.Count; i++)
    //    //    {
    //    //        y += 20;
    //    //        GUI.Label(new Rect(20, y, 700, 20), "버프 이름 : " + currentBattleGroup.artifactController.artifactList[i].name + ", 효력 : " + currentBattleGroup.artifactController.artifactList[i].message + " X " + currentBattleGroup.artifactController.artifactList[i].stack);
    //    //    }

    //    //}
    //    //GUI.Label(new Rect(20, 370, 300, 20), "버프 포인트 : " + currentBattleGroup.artifactController.artifactPoint);
    //    //if (currentBattleGroup.artifactController.artifactPoint > 0)
    //    //{
    //    //    int _y = 400;
    //    //    if (GUI.Button(new Rect(20, _y, 80, 20), "버프선택"))
    //    //    {
    //    //        currentBattleGroup.artifactController.ShowBuff();
    //    //    }

    //    //    if (currentBattleGroup.artifactController.showArtifactIDList != null
    //    //        && currentBattleGroup.artifactController.showArtifactIDList.Count > 0)
    //    //    {
    //    //        int _x = 20;
    //    //        _y += 30;
    //    //        for (int i = 0; i < currentBattleGroup.artifactController.showArtifactIDList.Count; i++)
    //    //        {

    //    //            if (GUI.Button(new Rect(_x, _y, 150, 20), GameDataManager.ArtifactBaseDataDic[currentBattleGroup.artifactController.showArtifactIDList[i]].name))
    //    //            {
    //    //                currentBattleGroup.artifactController.AddArtifact(GameDataManager.ArtifactBaseDataDic[currentBattleGroup.artifactController.showArtifactIDList[i]]);
    //    //            }
    //    //            _x += 160;
    //    //        }
    //    //    }
    //    //}


    //    // ====== 포만감 라인 ===============================================





    //    //if (GUI.Button(new Rect(20, 350, 150, 20),"음식먹기"))
    //    //{
    //    //    currentBattleGroup.GetComponent<SatietyController>().SatietyIncrease();
    //    //}
    //    //if (GUI.Button(new Rect(170, 350, 150, 20), "포만감 최대량 증가"))
    //    //{
    //    //    currentBattleGroup.GetComponent<SatietyController>().MaxSatietyIncrease();
    //    //}




    //}
}
