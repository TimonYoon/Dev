using System.Collections;
using UnityEngine;



/// <summary> PVP 관련 클래스 </summary>
public class BattlePvP : BattleGroup
{
    public static BattlePvP Instance;
    
    public bool isGiveUp = false;
    public bool isWin { get; private set; }

    float pvpTime { get { return 120f; } }
    float pvpStartTime = 0;
    public float pvpReminingTime { get; private set; }

    public SimpleDelegate onStartBattle;
    public SimpleDelegate onEndBattle;


    protected override void Awake()
    {
        Instance = this;
        battleType = BattleType.PvP;

        base.Awake();        
    }

    void Start()
    {
        isRenderingActive = true;

        InitSpawnPoint();
        StartCoroutine(InitBattlePvPHeroData());

    }

    protected override void InitSpawnPoint()
    {
        base.InitSpawnPoint();
    }
    protected override void Update()
    {
        base.Update();
    }

    
    IEnumerator InitBattlePvPHeroData()
    {          
        yield return StartCoroutine(battleTeamList[0].InitCoroutine(this, BattleUnit.Team.Red, BattlePvPManager.redTeamDataList));

        yield return StartCoroutine(battleTeamList[1].InitCoroutine(this, BattleUnit.Team.Blue, BattlePvPManager.blueTeamDataList));

        StartCoroutine(BattleProcess());
    }


    /// <summary> 배틀 진행 </summary>
    IEnumerator BattleProcess()
    {
        if (onStartBattle != null)
            onStartBattle();


        float startTime = Time.time;
        float elapsedTime = 0f;
        
        //페이드인
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

        //레드 팀 출전
        yield return StartCoroutine(SpawnHero(BattleUnit.Team.Red));

        //블루 팀 출전
        yield return StartCoroutine(SpawnHero(BattleUnit.Team.Blue));



        //스테이지 시작 trigger
        for (int i = 0; i < redTeamList.Count; i++)
        {
            BattleHero hero = redTeamList[i];
            for (int j = 0; j < hero.buffController.buffList.Count; j++)
            {
                Buff buff = hero.buffController.buffList[j];
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

            if(isGiveUp)
            {
                isWin = false;
                break;
            }

            // 전투시간 초과시
            if(pvpReminingTime <= 0)
            {
                // 남은 체력 비교 
                isWin = CompareHp();
                break;
            }
            yield return null;
        }

        battlePhase = BattlePhase.Finish;

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

        BattlePvPManager.BattlePVPServerConnect(BattlePvPServerConnectType.PvPResult, ServerResult);

        yield break;

    }
    void ServerResult(bool isResult)
    {
        if(isResult)
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

        UIBattlePvPLobby.Instance.OnClickSearchButton();
        SceneLobby.Instance.SceneChange(LobbyState.PvPBattleLobby);

    }

    /// <summary> 사용한 영웅들 actorPool로 반납하기 </summary>
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

    /// <summary> 전투 시간초과후 남은 영웅 hp 비교하여 승패 여부 결정 </summary>
    bool CompareHp()
    {
        bool isResult = false;

        // 생존자 남은 체력 비율 비교하여 승리팀 결정
        double buleReminingHp = 0;
        int blueTeamSurvivorActorCount = 0;
        for (int i = 0; i < blueTeamList.Count; i++)
        {
            if (blueTeamList[i].isDie == false)
            {
                blueTeamSurvivorActorCount++;
                buleReminingHp += (blueTeamList[i].curHP / blueTeamList[i].maxHP);
            }
        }
        buleReminingHp /= blueTeamSurvivorActorCount;

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
