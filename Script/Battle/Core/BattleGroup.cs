using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using CodeStage.AntiCheat.ObscuredTypes;


/// <summary> 전투그룹하나를 관리 (캐릭터 생성 배치/해당 원정대의 진행 정보) </summary>
public class BattleGroup : MonoBehaviour
{
    public delegate void BattleGroupSimpleCallback();

    public delegate void BattleGroupDelegate(BattleGroup battleGroup);

    public delegate void BattleHeroDelegate(BattleHero batttleHero);

    /// <summary> 페이즈가 변경된 경우에 콜백 </summary>
    public BattleGroupDelegate onChangedBattlePhase;

    /// <summary> 스테이지카운트 변경 알림 콜백 </summary>
    public BattleGroupDelegate onChangedStage;

    /// <summary> 현재 그룹의 스테이지를 보여줌 콜백 </summary>
    public BattleGroupDelegate onShowStage;

    /// <summary> 활성/비활성 상태가 바뀌었을 때 콜백 </summary>
    public BattleGroupDelegate onChangedActiveState;

    /// <summary> 전투 재시작 할 때 콜백 </summary>
    public BattleGroupDelegate onRestartBattle;

    public enum BattleType
    {
        Normal,
        DayDoungen,
        PvP,
    }

    /// <summary> 전투는 FadeIn -> Ready -> Battle -> Finsih -> FadeOut 의 순서로 진행된다. </summary>
    public enum BattlePhase
    {
        NotDefined,
        Ready,
        Battle,
        Finish,
        FadeOut,
        FadeIn,
    }

    BattlePhase _battlePhase = BattlePhase.NotDefined;
    public BattlePhase battlePhase
    {
        get { return _battlePhase; }
        protected set
        {
            if (_battlePhase == value)
                return;

            _battlePhase = value;

            //페이즈 변경 콜백
            if (onChangedBattlePhase != null)
                onChangedBattlePhase(this);
        }
    }

    public CustomList<BattleHero> originalMember = new CustomList<BattleHero>();

    public BattleTeam[] battleTeamList = new BattleTeam[2];

    /// <summary> 플레이어 영웅 리스트 -> 현재 스테이지에서 전투에 참여하는 영웅 리스트 </summary>
    public CustomList<BattleHero> redTeamList { get { return battleTeamList[0].actorList; } }

    /// <summary> 적 (몬스터) 리스트 -> 현재 스테이지에서 전투에 참여하는 몬스터 리스트  </summary>
    public CustomList<BattleHero> blueTeamList { get { return battleTeamList[1].actorList; } }

    public BattleHero boss { get; private set; }

    public Canvas canvasObject;

    BattleMoveCamera _battleCamera;
    public BattleMoveCamera battleCamera
    {
        get
        {
            if (!_battleCamera)
                _battleCamera = GetComponentInChildren<BattleMoveCamera>();

            return _battleCamera;
        }
    }


    /// <summary> 배틀 타입 </summary>
    public BattleType battleType { get; set; }

    /// <summary> 던전 아이디 </summary>
    public ObscuredString dungeonID { get; set; }

    ObscuredInt _stage = 1;
    public ObscuredInt stage
    {
        get { return _stage; }
        private set
        {
            if (_stage == value)
                return;

            _stage = value;

            if (onChangedStage != null)
                onChangedStage(this);
        }
    }

    /// <summary> 지금까지 획득한 아이템 누적수량 </summary>
    public double totalEnhanceStoneCount
    {
        get
        {
            float artifactStack = 0f;
            Artifact artifact = artifactController.artifactList.Find(x => x.baseData.type == "IncreaseEnhanceStone");
            if (artifact != null)
                artifactStack = artifact.stack;

            //유물 효과에 의해 스택당 10%씩 획득량 더 많다고 보여줌. (실제론 서버에서 처리)
            return 15d * (1 + artifactStack * 0.1f) * Math.Pow(1.03d, stage - 1);
        }
    }


    public BattleLevelUpController battleLevelUpController { get; private set; }
    
    public ArtifactController artifactController { get; private set; }

    public BattleQuestController battleQuestController { get; private set; }

    
    // 배경 오브젝트중 가장 상위 부모 transform
    public Transform streetRoot;

    public float spwonPointXMin { get; protected set; }
    public float spwonPointXMax { get; protected set; }

    public float spwonPointYMin { get; protected set; }
    public float spwonPointYMax { get; protected set; }

    public enum RestartType
    {
        Normal = 1,
        Double = 2
    }

    public bool isInitialized { get; private set; }

    /// <summary> StageState.Ready 보여줄 최소 시간 </summary>
    const float battleReadyTime = 0.5f;
    /// <summary> StageState.Finish 보여줄 최소 시간 </summary>
    const float battleFinsihTime = 2f;
    /// <summary> StageState.FadeOut 연출 시간 </summary>
    protected const float battleFadeOutTime = 1f;
    /// <summary> StageState.FadeIn 연출 시간 </summary>
    protected const float battleFadeInTime = 2f;

    /// <summary> StageState.FadeOut이 진행된 정도 </summary>
    public float fadeOutProcess { get; protected set; }

    /// <summary> StageState.FadeIn이 진행된 정도 </summary>
    public float fadeInProcess { get; protected set; }


    //==============================================================================================

    public Transform endingPoint;


    public List<SpawnPoint> spawnPointsRedTeam;
    public List<SpawnPoint> spawnPointsBlueTeam;
    
    //==============================================================================================
    public SimpleDelegate onChangedFrontmostHero;
    public SimpleDelegate onChangedRearmostHero;

    public SimpleDelegate onChangedFrontmostMonster;
    public SimpleDelegate onChangedRearmostMonster;


    BattleHero _frontMostHero;
    /// <summary> 최전방 영웅 </summary>
    public BattleHero frontMostHero
    {
        get { return _frontMostHero; }
        set
        {
            bool isChanged = _frontMostHero != value;

            _frontMostHero = value;

            if (isChanged && onChangedFrontmostHero != null)
                onChangedFrontmostHero();
        }
    }

    BattleHero _rearMostHero;
    /// <summary> 최후방 영웅 </summary>
    public BattleHero rearMostHero
    {
        get { return _rearMostHero; }
        set
        {
            bool isChanged = _rearMostHero != value;

            _rearMostHero = value;

            if (isChanged && onChangedRearmostHero != null)
                onChangedRearmostHero();
        }
    }

    
    BattleHero _frontMostMonster;
    /// <summary> 최전방 몬스터 </summary>
    public BattleHero frontMostMonster
    {
        get { return _frontMostMonster; }
        set
        {
            bool isChanged = _frontMostMonster != value;

            _frontMostMonster = value;

            if (isChanged && onChangedFrontmostMonster != null)
                onChangedFrontmostMonster();
        }
    }

    BattleHero _rearMostMonster;
    /// <summary> 최후방 몬스터 </summary>
    public BattleHero rearMostMonster
    {
        get { return _rearMostMonster; }
        set
        {
            bool isChanged = _rearMostMonster != value;

            _rearMostMonster = value;

            if (isChanged && onChangedRearmostMonster != null)
                onChangedRearmostMonster();
        }
    }

    //==============================================================================================

    public Canvas canvasUIBattleCharacter;

    bool _isRenderingActive = false;
    /// <summary> 현재 랜더러 on/off 상황 </summary>
    public bool isRenderingActive
    {
        get { return _isRenderingActive; }
        set
        {
            Canvas[] canvases = gameObject.GetComponentsInChildren<Canvas>();
            for (int i = 0; i < canvases.Length; i++)
            {
                canvases[i].enabled = value;
            }

            Camera[] cameras = gameObject.GetComponentsInChildren<Camera>();
            for (int i = 0; i < cameras.Length; i++)
            {
                cameras[i].enabled = value;
            }

            _isRenderingActive = value;

            if (Battle.onChangedBattleGroupActiveState != null)
                Battle.onChangedBattleGroupActiveState();

            if (onChangedActiveState != null)
                onChangedActiveState(this);

            if (value && onShowStage != null)
                onShowStage(this);
        }
    }

    [System.NonSerialized]
    public List<SpawnPoint> spawnPoints = new List<SpawnPoint>();

    public delegate void BattleSaveDataStageDelegate(BattleGroup battleGroup, BattleSaveDataStage battleSaveDataStage);
    public BattleSaveDataStageDelegate onLoadData;

    public delegate void OnSaveData();
    public OnSaveData onSaveData;

    /// <summary> 회군할 때 발생. </summary>
    public BattleGroupDelegate onStopBattle;

    Coroutine battleProcessCoroutine;

    //#####################################################################################################

    protected virtual void Awake()
    {
        BattleGroupElement be = GetComponentInChildren<BattleGroupElement>();
        if (be)
            be.SetBattleGroup(this);

        artifactController = gameObject.GetComponent<ArtifactController>();
        if(artifactController == null)
            artifactController = gameObject.AddComponent<ArtifactController>();

        battleQuestController = gameObject.GetComponent<BattleQuestController>();
        if (battleQuestController == null)
            battleQuestController = gameObject.AddComponent<BattleQuestController>();

        battleLevelUpController = gameObject.GetComponent<BattleLevelUpController>();
        if (battleLevelUpController == null)
            battleLevelUpController = gameObject.AddComponent<BattleLevelUpController>();



        //스폰포인트 설정
        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            spawnPoints = GetComponentsInChildren<SpawnPoint>().ToList();
        }


        List<MeshRenderer> sr = streetRoot.GetComponentsInChildren<MeshRenderer>().ToList().OrderBy(x => x.transform.position.x).ToList();
        MeshRenderer min = sr[0];
        MeshRenderer max = sr[sr.Count - 1];
        spwonPointXMin = min.bounds.min.x;
        spwonPointXMax = max.bounds.max.x;
    }
    
    void Start()
    {
        InitSpawnPoint();
        battleProcessCoroutine = StartCoroutine(BattleProcess());
    }
    protected virtual void InitSpawnPoint()
    {
        spawnPointsRedTeam = spawnPoints.FindAll(x => x.unitType == SpawnPoint.UnitType.Red);
        spawnPointsBlueTeam = spawnPoints.FindAll(x => x.unitType == SpawnPoint.UnitType.Blue);

        spwonPointYMin = spawnPoints.Min(x => x.transform.position.y);
        spwonPointYMax = spawnPoints.Max(x => x.transform.position.y);
    }

    

    protected virtual void Update()
    {
        if (battlePhase != BattlePhase.Battle)
            return;

        frontMostHero = null;
        rearMostHero = null;

        BattleHero fHero = null;
        BattleHero rHero = null;
        for (int i = 0; i < redTeamList.Count; i++)
        {
            BattleHero h = redTeamList[i];
            if (!h || h.isDie)
                continue;

            if (!h.isFinishSpawned)
                continue;

            if (!fHero)
            {
                fHero = h;
            }

            if (!rHero)
            {
                rHero = h;
            }

            if (h.transform.position.x > fHero.transform.position.x)
                fHero = h;

            if (h.transform.position.x < rHero.transform.position.x)
                rHero = h;
        }
        
        frontMostHero = fHero;
        rearMostHero = rHero;


        frontMostMonster = null;
        rearMostMonster = null;

        BattleHero fMonster = null;
        BattleHero rMonster = null;
        for (int i = 0; i < blueTeamList.Count; i++)
        {
            BattleHero h = blueTeamList[i];
            if (!h || h.isDie)
                continue;

            if (!h.isFinishSpawned)
                continue;

            if (!fHero)
            {
                fMonster = h;
            }

            if (!rHero)
            {
                rMonster = h;
            }

            if (h.transform.position.x > fMonster.transform.position.x)
                fMonster = h;

            if (h.transform.position.x < rMonster.transform.position.x)
                rMonster = h;
        }

        frontMostMonster = fMonster;
        rearMostMonster = rMonster;
    }

    
       

    public void Init(BattleType _battleType, List<HeroData> heroList, int _stage = 1, BattleSaveDataStage saveData = null)
    {
        StartCoroutine(InitHero(_battleType, heroList, _stage = 1, saveData));
    }

    public IEnumerator InitHero(BattleType _battleType, List<HeroData> heroList, int _stage = 1, BattleSaveDataStage saveData = null)
    {
        battleType = _battleType;
        if (saveData != null)
        {
            stage = saveData.stage;
            
            dungeonID = saveData.dungeonID;
            while (!artifactController && !battleLevelUpController)
                yield return null;

            if (onLoadData != null)
                onLoadData(this, saveData);
        }
        else
        {
            stage = _stage;
            
            List<string> dungeonIDList = GameDataManager.dungeonBaseDataDic.Keys.ToList();
            int r = UnityEngine.Random.Range(0, dungeonIDList.Count);
            dungeonID = dungeonIDList[r];
        }
        yield return StartCoroutine(battleTeamList[0].InitCoroutine(this, BattleUnit.Team.Red, heroList));

        isInitialized = true;

        while (!Battle.Instance.isInitialized)
            yield return null;

        if (battleType == BattleType.PvP)
            yield break;

        if (saveData == null)
            Battle.SaveStageInfo(this);

    }

    /// <summary> 미리 생성해 놓은 몬스터 풀에서 출전 몬스터 세팅하기 </summary>
    IEnumerator InitMonster()
    {
        int monsterCount = 15;


        List<HeroData> monsterList = new List<HeroData>();

        for (int i = 0; i < monsterCount; i++)
        {
            int monsterIndex = UnityEngine.Random.Range(0, actorPool.Instance.monsterKeyList.Count);
            string monsterKey = actorPool.Instance.monsterKeyList[monsterIndex];

            if (string.IsNullOrEmpty(monsterKey))
                continue;

            HeroBaseData baseData = HeroManager.heroBaseDataDic[monsterKey];
            HeroData heroData = new HeroData(baseData);
            monsterList.Add(heroData);
        }

        yield return StartCoroutine(battleTeamList[1].InitCoroutine(this, BattleUnit.Team.Blue, monsterList));
    }

    /// <summary> 보스 초기화 </summary>
    IEnumerator InitBoss(int bossGrade)
    {

        string bossKey = "";
        List<HeroBaseData> bossList = actorPool.Instance.heroBaseDataList.FindAll(x => x.grade == bossGrade);
        int index = UnityEngine.Random.Range(0, bossList.Count);
        bossKey = bossList[index].id;

        yield return StartCoroutine(actorPool.Instance.GetActor(bossKey, x => boss = x));

        if (!boss)
            yield break;

        boss.team = BattleUnit.Team.Blue;
        float power = 6f * Mathf.Pow(1.07f, stage - 1);
        float bossPower = power * 0.17f;
        boss.power = bossPower;
        boss.isBoss = true;
        boss.Init(this, new HeroData(HeroManager.heroBaseDataDic[bossKey]), boss.team);

        boss.gameObject.SetActive(true);
        boss.ReGen();

        //던전 버프 적용
        boss.buffController.AttachBuff(boss, GameDataManager.dungeonBaseDataDic[dungeonID].buffID);
    }


    /// <summary> 회군으로 인해 전투 멈춤 </summary>
    public void StopBattle()
    {
        if (onStopBattle != null)
            onStopBattle(this);

        battlePhase = BattlePhase.Ready;
        if(battleProcessCoroutine != null)
        {
            StopCoroutine(battleProcessCoroutine);
            battleProcessCoroutine = null;
        }        

        //아군 데이타 레벨 1로 초기화.
        for (int i = 0; i < redTeamList.Count; i++)
        {            
            BattleHero hero = redTeamList[i];
            hero.HeroReset();
            hero.transform.position = spawnPointsRedTeam[0].transform.position;
        }

        // 스폰되었던 보스와 몬스터 제거
        ClearBossAndMonster();

        Battle.SaveStageInfo(this);
    }


    
    

   

    /// <summary> 회군 후 재시작. 1: 일반, 2: 강화석 2배 획득 </summary>
    public IEnumerator Restart(RestartType restartType)
    {
        LoadingManager.Show();

        string result = null;
        string php = "Battle.php";
        WWWForm form = new WWWForm();
        form.AddField("type", (int) restartType);
        form.AddField("userID", User.Instance.userID);
        form.AddField("enhanceType", GameDataManager.dungeonBaseDataDic[dungeonID].dropItemID);
        form.AddField("stage", stage);
        
        //유물에 의한 획득량 보정. 유물 스택값을 서버로 보냄
        int artifactStack = 0;
        Artifact artifact = artifactController.artifactList.Find(x => x.baseData.type == "IncreaseEnhanceStone");
        if (artifact != null)
            artifactStack = artifact.stack;
        form.AddField("artifactStack", artifactStack);
        
        //서버 통신 & 결과 대기
        yield return StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(php, form, x => result = x));

        Debug.Log(result);

        

        if(result == "false")
        {            
            LoadingManager.Close();

            UIPopupManager.ShowOKPopup("", "루비가 부족합니다.", null);

            yield break;
        }

        if(DailyMissionManager.Instance && DailyMissionManager.Instance.retreatCount < 3)
        {
            DailyMissionManager.Instance.retreatCount += 1;
            StartCoroutine(DailyMissionManager.Instance.SetDailyMission(DailyMissionType.Retreat));
        }

        if (UserQuestManager.Instance && UserQuestManager.Instance.retreatCount < 1)
        {
            UserQuestManager.Instance.retreatCount += 1;
            StartCoroutine(UserQuestManager.Instance.SetUserQuest(UserQuestType.Retreat));
        }

        if (UserQuestManager.Instance && UserQuestManager.Instance.dungeonArrivalCount < 30 && stage > 300)
        {
            UserQuestManager.Instance.dungeonArrivalCount += 1;
            StartCoroutine(UserQuestManager.Instance.SetUserQuest(UserQuestType.DungeonArrival));
        }

        LoadingManager.Close();

        //배틀 그룹 전투 프로세서 종료
        StopBattle();

        //재시작
        ReStartBattle();

        //정보화면 초기화
        UIBattleInfo.InitDungeonInfo();
    }

    /// <summary> 회군 후 재시작 </summary>
    public void ReStartBattle()
    {
        for (int i = 0; i < redTeamList.Count; i++)
        {
            redTeamList[i].Restart();

            if(isRenderingActive)
            {
                redTeamList[i].skeletonAnimation.GetComponent<MeshRenderer>().enabled = true;
                redTeamList[i].GetComponentInChildren<SpriteRenderer>().enabled = true;
            }
            else
            {
                redTeamList[i].skeletonAnimation.GetComponent<MeshRenderer>().enabled = false;
                redTeamList[i].GetComponentInChildren<SpriteRenderer>().enabled = false;
            }

        }

        //던전 랜덤하게
        List<string> dungeonIDList = GameDataManager.dungeonBaseDataDic.Keys.ToList();
        int r = UnityEngine.Random.Range(0, dungeonIDList.Count);
        dungeonID = dungeonIDList[r];

        //??
        if (onRestartBattle != null)
            onRestartBattle(this);
        

        stage = 1;

        StopCoroutine("BattleProcess");
        StartCoroutine("BattleProcess");

        Battle.SaveStageInfo(this);

    }
 

    
    
    /// <summary> 순차적으로 영웅 스폰. phase = Battle 이 된 직후 실행 </summary>
    protected IEnumerator SpawnHero(BattleUnit.Team team = BattleUnit.Team.Red, List<BattleHero> list = null)
    {
        List<BattleHero> unitList = list;
        if(unitList == null)
        {
            unitList = battleTeamList[(int)team].actorList;
        }

        if (unitList == null || unitList.Count == 0)
            yield break;

        //스폰될애들 리스트. 순서 무작위로 출현하기 위함
        List<BattleHero> spawnedHeroList = new List<BattleHero>();
        BattleHero temp = null;
        while (spawnedHeroList.Count != unitList.Count)
        {
            temp = unitList[UnityEngine.Random.Range(0, unitList.Count)];
            if (!spawnedHeroList.Find(x => x == temp))
                spawnedHeroList.Add(temp);

            yield return null;
        }

        //하나씩 출현
        for (int i = 0; i < spawnedHeroList.Count; i++)
        {
            //죽은 캐릭터는 스폰하지 않음
            if (spawnedHeroList[i].isDie)
                continue;

            spawnedHeroList[i].gameObject.SetActive(true);

            spawnedHeroList[i].ReGen();

            if (i > 0)
            {
                float dist = spawnedHeroList[i].team == BattleUnit.Team.Red ? -5f : 5f;

                float x = spawnedHeroList[i - 1].transform.position.x + dist;
                spawnedHeroList[i].transform.position = new Vector3(x, spawnedHeroList[i].transform.position.y, spawnedHeroList[i].transform.position.z);

                OrderController oc = spawnedHeroList[i].GetComponent<OrderController>();
                if (oc)
                    oc.UpdateScale();
            }
        }
    }

    /// <summary> 배틀 진행 </summary>
    IEnumerator BattleProcess()
    {        
        while (!Battle.Instance.isInitialized || !isInitialized)
            yield return null;
                   
        while (true)
        {
            float startTime = Time.time;
            float elapsedTime = 0f;
            ////페이드인
            battlePhase = BattlePhase.FadeIn;
            fadeInProcess = 0f;
            while (elapsedTime < battleFadeInTime)
            {
                fadeInProcess = elapsedTime / battleFadeInTime;

                elapsedTime = Time.time - startTime;

                yield return null;
            }

            fadeInProcess = 1f;

            //전투 준비
            battlePhase = BattlePhase.Ready;            
            
            //전투
            battlePhase = BattlePhase.Battle;

            //유물 스테이지 건너띄기 효과 적용
            int skipAmount = 0;
            Artifact artifact = artifactController.artifactList.Find(x => x.baseData.type == "SkipStage");
            if (artifact != null)
            {
                //확률 적용
                int r = UnityEngine.Random.Range(0, 100);
                if (r < 5)
                {
                    int.TryParse(artifact.baseData.formula, out skipAmount);
                    skipAmount = skipAmount * artifact.stack;
                }
            }
            stage = stage + skipAmount;


            // 몬스터 세팅
            yield return StartCoroutine(InitMonster());

            // 영웅들 하나씩 출전
            yield return StartCoroutine(SpawnHero(BattleUnit.Team.Red));
            // 몬스터 하나씩 출전
            yield return StartCoroutine(SpawnHero(BattleUnit.Team.Blue));

            
            int bossGrade = 3;
            bool isBoss = false;
            
            //보스 스폰 여부와 스폰될 보스 등급 결정 (기획상 변동여지 있음)
            if (stage % 100 == 0)
            {
                bossGrade = 4;
                isBoss = true;
            }
            else if(stage % 25 == 0)
            {
                bossGrade = 3;
                isBoss = true;
            }

            if(isBoss)
            {
                yield return StartCoroutine(InitBoss(bossGrade));
            }

            bool isLoss = false;
            //아군 혹은 적군 전부 사망할 때 까지 전투 상태
            while (battlePhase == BattlePhase.Battle)
            {
                if (battleTeamList[0].isAllDie)
                {
                    isLoss = true;
                    break;
                }


                if (battleTeamList[1].isAllDie)
                {
                    if (boss)
                    {
                        if (boss.isDie)
                            break;
                    }
                    else
                    {
                        break;
                    }

                }

                yield return null;
            }

            battlePhase = BattlePhase.Finish;

            yield return new WaitForSeconds(2f);

            //페이드아웃
            battlePhase = BattlePhase.FadeOut;

            ClearBossAndMonster();

            startTime = Time.time;
            elapsedTime = 0f;
            fadeOutProcess = 0f;
            while (elapsedTime < battleFadeOutTime)
            {
                fadeOutProcess = elapsedTime / battleFadeOutTime;

                elapsedTime = Time.time - startTime;

                yield return null;
            }
            fadeOutProcess = 1f;

            //스테이지 증가 or 감소
            if (isLoss)
            {
                if (stage == 1)
                    stage = 1;
                else
                    stage--;
            }
            else
            {
                stage = stage++;
            }

            if (isRenderingActive)
                System.GC.Collect();
            
            //전투 정보 로컬 저장
            Battle.SaveStageInfo(this);
            
            yield return null;
        }
    }

    void ClearBossAndMonster()
    {
        while(blueTeamList.Count > 0)
        {
            blueTeamList[0].Despawn(false);
        }
        if(boss != null)
        {
            boss.Despawn(false);
            boss = null;
        }
            
    }

}
