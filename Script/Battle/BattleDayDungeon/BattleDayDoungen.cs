using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using LitJson;
using System.Linq;

public class BattleDayDoungen : BattleGroup {

    public static BattleDayDoungen Instance;

    public SimpleDelegate onEndBattle;

    public bool isGiveUp = false;
    public bool isWin;
    float pvpTime = 120f;
    float pvpStartTime = 0;
    public float pvpReminingTime = 0;

    protected override void Awake()
    {
        Instance = this;
        battleType = BattleType.DayDoungen;
        //unitArea = GetComponentInChildren<BoxCollider>();
        BattleGroupElement be = GetComponentInChildren<BattleGroupElement>();
        if (be)
            be.SetBattleGroup(this);

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
        isRenderingActive = true;
        spawnPointsRedTeam = spawnPoints.FindAll(x => x.unitType == SpawnPoint.UnitType.Red);
        spawnPointsBlueTeam = spawnPoints.FindAll(x => x.unitType == SpawnPoint.UnitType.Blue);

        spwonPointYMin = spawnPoints.Min(x => x.transform.position.y);
        spwonPointYMax = spawnPoints.Max(x => x.transform.position.y);

        StartCoroutine(InitBattlePvPHeroData());
    }
    protected override void Update()
    {
        base.Update();
    }


    public SimpleDelegate onStartBattle;
    IEnumerator InitBattlePvPHeroData()
    {
        BattleTeam[] teamList = new BattleTeam[2];

        teamList[0].Init(this, BattleUnit.Team.Red, BattleDayDungeonManager.redTeamDataList);
        teamList[1].Init(this, BattleUnit.Team.Red, BattleDayDungeonManager.redTeamDataList);
        yield return StartCoroutine(Init(BattleUnit.Team.Red, BattleDayDungeonManager.redTeamDataList));

        yield return StartCoroutine(Init(BattleUnit.Team.Blue, BattleDayDungeonManager.blueTeamDataList));

    }

    IEnumerator Init(BattleUnit.Team team = BattleUnit.Team.Red, List<HeroData> heroList = null)
    {
        battleTeamList[(int)team].Init(this, team, heroList);
        
        if (team == BattleUnit.Team.Blue)
        {
            // 적 세팅이 되면 전투 시작
            StartCoroutine(BattleProcess());
        }

        yield return null;
    }


    /// <summary> 배틀 진행 </summary>
    IEnumerator BattleProcess()
    {
        if (onStartBattle != null)
            onStartBattle();


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

        //영웅들 하나씩 출전
        yield return StartCoroutine(SpawnHero(BattleUnit.Team.Red));

        //상대 팀 출전
        yield return StartCoroutine(SpawnHero(BattleUnit.Team.Blue));



        //스테이지 시작 trigger
        for (int i = 0; i < redTeamList.Count; i++)
        {
            BattleHero hero = redTeamList[i];
            for (int a = 0; a < hero.buffController.buffList.Count; a++)
            {
                Buff buff = hero.buffController.buffList[a];
                if (buff.baseData.trigger != "OnStartStage")
                    continue;

                if (buff.triggerProbability < UnityEngine.Random.Range(1, 10001))
                    continue;

                BattleUnit target = null;
                if (buff.baseData.triggerTarget == "SkillTarget")
                    target = hero;
                else if (buff.baseData.triggerTarget == "BuffTarget")
                    target = hero;

                if (target && !target.isDie)
                    target.buffController.AttachBuff(hero, buff.baseData.triggerBuff, 1, buff);
            }
        }
        for (int i = 0; i < blueTeamList.Count; i++)
        {
            BattleHero hero = blueTeamList[i];

            // 던전 버프 적용
            string key = UIDayDungeonLobby.Instance.currentTapDay.ToString() + "_" + UIDayDungeonLobby.Instance.dungeonLevel;            
            if (GameDataManager.dayDungeonBaseDataDic.ContainsKey(key))
            {
                DayDungeonBaseData data = GameDataManager.dayDungeonBaseDataDic[key];
                hero.buffController.AttachBuff(hero, data.buffID);
            }

            for (int a = 0; a < hero.buffController.buffList.Count; a++)
            {
                Buff buff = hero.buffController.buffList[a];
                if (buff.baseData.trigger != "OnStartStage")
                    continue;

                if (buff.triggerProbability < UnityEngine.Random.Range(1, 10001))
                    continue;

                BattleUnit target = null;
                if (buff.baseData.triggerTarget == "SkillTarget")
                    target = hero;
                else if (buff.baseData.triggerTarget == "BuffTarget")
                    target = hero;

                if (target && !target.isDie)
                    target.buffController.AttachBuff(hero, buff.baseData.triggerBuff, 1, buff);
            }
        }

        // 전투 시작 시간 체크
        pvpStartTime = Time.time + pvpTime;
        LoadingManager.Close();
        isWin = false;
        //아군 혹은 적군 전부 사망할 때 까지 전투 상태
        while (battlePhase == BattlePhase.Battle)
        {
            if (battleTeamList[0].isAllDie)
                break;

            if (battleTeamList[1].isAllDie)
            {
                isWin = true;
                break;
            }

            pvpReminingTime = pvpStartTime - Time.time;

            if (isGiveUp)
            {
                isWin = false;
                break;
            }

            if (pvpReminingTime <= 0)
            {

                // 남은 체력 비교 
                if (CompareHp())
                {
                    isWin = true;
                }
                break;
            }
            yield return null;
        }

        battlePhase = BattlePhase.Finish;

        //yield return new WaitForSeconds(2f);

        //페이드아웃
        battlePhase = BattlePhase.FadeOut;


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

        if(isWin)
        {
            BattleDayDungeonManager.DayDungeonServerConnect(DayDungeonServerConnectType.BattleResult, ServerResult);
        }
        else
        {
            BattleEnd();
        }
        
        yield break;

    }
    void ServerResult(bool isResult)
    {
        if (isResult)
        {
            BattleEnd();
        }
        else
        {
            UIPopupManager.ShowInstantPopup("서버 연결 실패");
        }
    }

    void BattleEnd()
    {
        if (onEndBattle != null)
            onEndBattle();
        
        UIDayDungeonLobby.Instance.SelectDay();
        SceneLobby.Instance.SceneChange(LobbyState.DayDungeonLobby);
        //SceneManager.UnloadSceneAsync("BattlePvP");

    }

    public void DespawnHero()
    {
        while (redTeamList.Count > 0)
        {
            redTeamList[0].Despawn(false);
        }

        while (blueTeamList.Count > 0)
        {
            blueTeamList[0].Despawn(false);
        }
    }

    bool CompareHp()
    {
        bool isResult = false;

        // 생존자 남은 체력 비율 비교하여 승리팀 결정
        double buleReminingHp = 0;
        int blueTeamSurvivorHeroCount = 0;
        for (int i = 0; i < blueTeamList.Count; i++)
        {
            if (blueTeamList[i].isDie == false)
            {
                blueTeamSurvivorHeroCount++;
                buleReminingHp += (blueTeamList[i].curHP / blueTeamList[i].maxHP);
            }
        }
        buleReminingHp /= blueTeamSurvivorHeroCount;

        double redReminingHp = 0;
        int redTeamSurvivorHeroCount = 0;
        for (int i = 0; i < redTeamList.Count; i++)
        {
            if (redTeamList[i].isDie == false)
            {
                redTeamSurvivorHeroCount++;
                redReminingHp += (redTeamList[i].curHP / redTeamList[i].maxHP);
            }
        }
        redReminingHp /= redTeamSurvivorHeroCount;

        isResult = redReminingHp > buleReminingHp;

        return isResult;
    }

}
