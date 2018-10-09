using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Spine.Unity;
using Spine;
using System;
//using System.Diagnostics;

/// <summary> 게임 씬에 생성된 게임 오브젝트(영웅 캐릭터용). 전투영웅에 대한 데이터 처리(공격/체력깍임) </summary>
public class BattleHero : BattleUnit {

    public delegate void BattleHeroCallback();
    public delegate void HitCallback(float damage);

    public SimpleDelegate onChangedFlip;

    public bool flipX
    {
        get
        {
            if (!skeletonAnimation)
                return false;

            return skeletonAnimation.Skeleton.FlipX;
        }
        set
        {
            if (!skeletonAnimation)
                return;

            if (skeletonAnimation.Skeleton.FlipX == value)
                return;

            bool isChanged = skeletonAnimation.Skeleton.FlipX != value;

            skeletonAnimation.Skeleton.FlipX = value;

            if (isChanged && onChangedFlip != null)
                onChangedFlip();
        }
    }
    
    public SkeletonAnimation skeletonAnimation = null;

    [SpineAnimation(dataField: "skeletonAnimation")]
    public string idleAnimation = "Idle";

    [SpineAnimation(dataField: "skeletonAnimation")]
    public string runAnimation = "Run";

    [SpineAnimation(dataField: "skeletonAnimation")]
    public List<string> mixedAnimation = new List<string>();

    public Spine.Animation spineAnimationIdle;
    public Spine.Animation spineAnimationRun;
    public Spine.Animation spineAnimationDie;


    public enum ChaseTargetType
    {
        NearestEnemy,
        FrontMostAlly,
        EndPoint
    }

    /// <summary> 평상시 어딜 향해 가는지 </summary>
    public ChaseTargetType chaseTargetType = ChaseTargetType.NearestEnemy;

    public float approachDistanceMin = 0f;
    public float approachDistanceMax = 2f;


    /// <summary> 스폰되었는지 여부, 순차적으로 스폰될 때 까지 대기하기 위한 용도. false면 안 움직이고 기다림 </summary>    
    public bool isFinishSpawned
    {
        get { return _isFinishSpawned; }
        set { _isFinishSpawned = value; }
    }
    bool _isFinishSpawned = false;



    public Vector3 originalScale { get; private set; }
    public float originalPosY { get; private set; }
    public Transform uiPivot { get; private set; }

    public OrderController orderController { get; set; }

    MaterialPropertyBlock mpb; 

    //############################################################################################
    protected override void Awake()
    {
        base.Awake();

        base.onHit += OnTakeDamage;

        orderController = GetComponent<OrderController>();
        if (!orderController)
            orderController = gameObject.AddComponent<OrderController>();

        Transform[] ts = GetComponentsInChildren<Transform>();
        for (int i = 0; i < ts.Length; i++)
        {
            Transform t = ts[i].Find("UIPivot");
            if (t != null)
            {
                uiPivot = t;
                break;
            }
        }

        for(int i = 0; i < ts.Length; i++)
        {
            //그림자 오브젝트
            if (ts[i].gameObject.name == "Shadow")
            {
                objShadow = ts[i].gameObject;
                break;
            }
        }

        MeshRenderer r = skeletonAnimation.GetComponent<MeshRenderer>();        
        for (int i = 0; i < r.materials.Length; i++)
        {            
            //r.materials[i] = Instantiate(r.materials[i]);
        }

        //if (originalScale == Vector3.zero)
        originalScale = transform.localScale;

        originalPosY = skeletonAnimation.transform.localPosition.y;
        
        spineAnimationIdle = skeletonAnimation.SkeletonDataAsset.GetAnimationStateData().skeletonData.FindAnimation(idleAnimation);
        spineAnimationRun = skeletonAnimation.SkeletonDataAsset.GetAnimationStateData().skeletonData.FindAnimation(runAnimation);
        spineAnimationDie = skeletonAnimation.SkeletonDataAsset.GetAnimationStateData().skeletonData.FindAnimation("Die");

        mpb = new MaterialPropertyBlock();
    }

    void OnEnable()
    {
        onDie += OnDie;
        onRevive += OnRevive;
        onChangedCanResurrect += OnChangedCanResurrect;

        //페이드 아웃 되었던거 원래대로
        Renderer r = skeletonAnimation.GetComponent<Renderer>();
        float alpha = 1f;
        for (int i = 0; i < r.materials.Length; i++)
        {
            Color c = r.materials[i].color;
            c.a = alpha;
            r.materials[i].color = c;
        }

        if (objShadow)
            objShadow.gameObject.SetActive(!isDie);

        if (skeletonAnimation.state != null)
        {
            if (!isSummonded)
                skeletonAnimation.state.SetAnimation(0, idleAnimation, true);
            else
                skeletonAnimation.state.Interrupt += OnInterruptAnimation;
        }
    }

    void OnInterruptAnimation(Spine.TrackEntry entry)
    {
        //Debug.Log(entry.animation.name);
        if (entry.animation.name == "Resurrect")
        {
            isFinishSpawned = true;
            skeletonAnimation.state.Interrupt -= OnInterruptAnimation;
        }

        //if(heroData.heroID == "Centaur_01_Hero")
        //{
        //    Debug.Log(entry.animation.name);
        //}
    }

    void OnDisable()
    {
        onDie -= OnDie;
        onRevive -= OnRevive;
        onChangedCanResurrect -= OnChangedCanResurrect;
        skeletonAnimation.state.Interrupt -= OnInterruptAnimation;

        isSuperArmor = false;
        isBlockMove = false;
        isBlockAttack = false;
        airborne = false;
        notTargeting = false;

        //if(objHPGauge)
        //    objHPGauge.SetActive(false);

        //hp게이지 도로 풀로 보냄
        if (objHPGauge)
        {
            HPGauge ui1 = objHPGauge.GetComponent<HPGauge>();
            ui1.Init(null);
            objHPGauge.SetActive(false);
            objHPGauge = null;
        }
    }

    public override void SetBattleGroup(BattleGroup _battleGroup)
    {
        if (battleGroup)
        {
            battleGroup.blueTeamList.Remove(this);
            battleGroup.redTeamList.Remove(this);

            //기존 배틀그룹 관련 콜백 등록 해제
            battleGroup.onChangedBattlePhase -= OnChangedBattlePhase;

            if (buffController && !string.IsNullOrEmpty(battleGroup.dungeonID))
                buffController.DetachBuff(GameDataManager.dungeonBaseDataDic[battleGroup.dungeonID].buffID);
        }

        base.SetBattleGroup(_battleGroup);

        if (battleGroup)
        {
            //배틀그룹 관련 콜백 등록
            battleGroup.onChangedBattlePhase += OnChangedBattlePhase;
        }

        if (!battleGroup)
        {
            //Despawn();
            return;
        }


        //팀원 추가 될 때 콜백. 패시브 버프 처럼 등장하면서 부터 적용되어야 하는 것들을 위해 필요
        if (team == Team.Red)
            battleGroup.redTeamList.onAdd += OnAddHeroToTeamList;
        else if (team == Team.Blue)
            battleGroup.blueTeamList.onAdd += OnAddHeroToTeamList;

        //hp게이지 달아주기
        if (!objHPGauge)
        {
            objHPGauge = Battle.GetObjectInPool(Battle.hpGaugePrefab.name);
            if (!objHPGauge)
            {
                objHPGauge = Instantiate(Battle.hpGaugePrefab, battleGroup.canvasUIBattleCharacter.transform);
                objHPGauge.name = Battle.hpGaugePrefab.name;
                Battle.AddObjectToPool(objHPGauge);
            }
        }

        objHPGauge.transform.SetParent(battleGroup.canvasUIBattleCharacter.transform);
        objHPGauge.SetActive(true);
        HPGauge ui = objHPGauge.GetComponent<HPGauge>();
        ui.Init(this);

        UpdateActiveState();
    }

    override public void Despawn(bool clearFromTeamList = true)
    {
        //부활 중단
        if (coroutineWaitToRevive != null)
        {
            StopCoroutine(coroutineWaitToRevive);
            coroutineWaitToRevive = null;
        }

        tomestone = null;

        if (skeletonAnimation != null)
        {
            skeletonAnimation.gameObject.SetActive(true);
            skeletonAnimation.enabled = true;
        }

        if (battleGroup)
        {
            //if (clearFromTeamList)
            {
                battleGroup.blueTeamList.Remove(this);
                battleGroup.redTeamList.Remove(this);
            }

            SetBattleGroup(null);
        }

        isFinishSpawned = false;

        //모든 버프 해제        
        if (buffController)
            buffController.DetachAllBuffs();
        

        //BuffManager.DetachAllBuffs(this);
        //if(buffController)
        //    buffController.DetatchBuffAll();

        base.Despawn(clearFromTeamList);
    }

    void OnChangedCanResurrect()
    {
        if (!canResurrect)
        {
            tomestone = null;
        }
    }

    override protected void Start()
    {
        //머터리얼 인스턴싱을 위해 지새깔 지한테 한 번 더 씌움
        MeshRenderer r = skeletonAnimation.GetComponentInChildren<MeshRenderer>();
        if (r)
        {
            for (int i = 0; i < r.materials.Length; i++)
            {
                r.materials[i] = Instantiate(r.materials[i]);
                //r.materials[i].enableInstancing = true;
                //Color c = r.materials[i].color;
                //r.materials[i].color = c;
            }
        }

        if (battleGroup)
        {
            //battleGroup.onChangedStage += OnChangedStage;
            battleGroup.onChangedBattlePhase += OnChangedBattlePhase;
        }

        skeletonAnimation.state.End += OnEndAnimation;

        base.Start();

        //skeletonAnimation.skeleton.flipX = flipX;
    }
    
    GameObject tomestoneCache = null;
    GameObject tomestone
    {
        get
        {
            if (tomestoneCache)
                return tomestoneCache;

            if (!Battle.Instance || !Battle.tombstonePrefab)
                return null;

            tomestoneCache = Battle.GetObjectInPool(Battle.tombstonePrefab.name);
            if(!tomestoneCache)
                tomestoneCache = Instantiate(Battle.tombstonePrefab, transform, false);
            tomestoneCache.name = Battle.tombstonePrefab.name;
            tomestoneCache.transform.parent = Battle.Instance.transform;
            Battle.AddObjectToPool(tomestoneCache);

            return tomestoneCache;
        }
        set
        {
            if (tomestoneCache != null)
                tomestoneCache.gameObject.SetActive(false);

            tomestoneCache = value;
        }
    }

    GameObject objHPGauge = null;

    GameObject objShadow;

    public void Restart()
    {
        //isDie = false;
        //curHP = maxHP;
        transform.localScale = originalScale;
        ReGen();
    }

    public void ReGen(bool keepPosition = false)
    {

        //피 꽉 채움
        //curHP = maxHP;

        //부활 가능
        if(!isSummonded)
            canResurrect = true;

        tomestone = null;

        if (skeletonAnimation != null)
        {
            skeletonAnimation.gameObject.SetActive(true);
            skeletonAnimation.enabled = true;

        }

        //제자리에서 시작이면 스폰포인트로 안 감
        if (!keepPosition && battleGroup)
        {
            //스폰 가능 한 곳중 랜덤 한 곳에서 스폰
            SpawnPoint.UnitType t = team == Team.Red ? SpawnPoint.UnitType.Red : SpawnPoint.UnitType.Blue;
            List<SpawnPoint> availableSpawnPoints = battleGroup.spawnPoints.FindAll(x => !x.isAssigned && x.unitType == t);

            int r = UnityEngine.Random.Range(0, availableSpawnPoints.Count);
            transform.position = availableSpawnPoints[r].transform.position;
            lastSpawnTime = Time.time;
        }

        
        transform.localScale = originalScale;
        
        //누적 데미지 초기화
        cumulativeDamage = 0;

        //피 꽉 채움
        curHP = maxHP;

        isFinishSpawned = true;
    }

    void OnChangedBattlePhase(BattleGroup b)
    {
        if (b != battleGroup)
            return;

        if(b.battlePhase == BattleGroup.BattlePhase.FadeOut)
        {

        }

        //페이드인 시작되면 몬스터건 영웅이건 지네들 스폰포인트 있는 곳으로 돌아감. 
        //(페이드아웃이 끝날 때 해야 하는 게 맞지만, 페이드아웃 끝날 때를 알기 힘들어서-귀찮아서- 여기서 함)
        if (b.battlePhase == BattleGroup.BattlePhase.FadeIn)
        {
            if (isDie)
            {
                //부활 대기중이면 중단. 
                if (coroutineWaitToRevive != null)
                {
                    StopCoroutine(coroutineWaitToRevive);
                    coroutineWaitToRevive = null;                    
                }

                //즉시 부활 시킴
                curHP = maxHP;
            }
            

            SpawnPoint.UnitType t = team == Team.Red ? SpawnPoint.UnitType.Red : SpawnPoint.UnitType.Blue;

            SpawnPoint spawnPoint = b.spawnPoints.Find(x => x.unitType == t);
            if (!spawnPoint)
                return;

            //다음 스폰 전까지 안 움직이게 막음
            isFinishSpawned = false;

            if (spawnPoint != null)
                transform.position = spawnPoint.transform.position;
        }
    }

    Coroutine coroutineFadeOut = null;
    public void HeroReset()
    {
        
        buffController.DetachAllBuffs();

      

        flipX = false;

        if (!HeroManager.heroDataDic.ContainsKey(heroData.id))
            return;

        HeroData hData = HeroManager.heroDataList.Find(x => x.id == heroData.id);
        if (hData == null)
            return;

        hData.level = 1;
        hData.exp = 0;
        skeletonAnimation.state.SetAnimation(0, idleAnimation, true);
    }
    void OnDie(BattleUnit unit)
    {
        //사망하면 모든 행동 종료
        //Issue: 예외도 있어야 하나?
        //skeletonAnimation.state.SetAnimation(0, idleAnimation, true);

        if(spineAnimationDie != null)
        {
            skeletonAnimation.state.SetAnimation(0, spineAnimationDie, false);
            skeletonAnimation.state.ClearTrack(1);
        }
            
        //if (skeletonAnimation.state.Data.SkeletonData.FindAnimation("Die") != null)
        //    skeletonAnimation.state.SetAnimation(0, "Die", false);
        else
        {
            skeletonAnimation.state.SetAnimation(0, spineAnimationIdle, false);

            if (coroutineFadeOut == null)
                coroutineFadeOut = StartCoroutine(FadeOut());
        }

        if (!isSummonded && tomestone)
        {
            BattleGroupElement be = tomestone.GetComponentInChildren<BattleGroupElement>();
            if (be)
                be.SetBattleGroup(battleGroup);

            //OrderController oc = tomestone.GetComponentInChildren<OrderController>();
            //if (oc)
            //    oc.parent = orderController;

            tomestone.gameObject.SetActive(true);
            if (airborne)
                tomestone.transform.position = beforeAirbornePos;
            else
                tomestone.transform.position = transform.position;
            tomestone.transform.localScale = transform.localScale;
        }

        if(objShadow)
            objShadow.gameObject.SetActive(false);

        //skeletonAnimation.gameObject.SetActive(false);
        //skeletonAnimation.enabled = false;

        //아군 영웅만 자동 부활 함
        //Todo: 자동 부활 여부에 의해서 판단해야 할 듯
        if (!isSummonded &&team == Team.Red)
        {
            return;
            if (coroutineWaitToRevive != null)
                return;

            coroutineWaitToRevive = StartCoroutine(WaitToRevive());
            
        }
        else if(isSummonded || team == Team.Blue)
        {
            if (coroutineWaitToDespawn != null)
                StopCoroutine(coroutineWaitToDespawn);

            coroutineWaitToDespawn = StartCoroutine(WaitToDespawn());
        }

        

        //battleGroup.Die(transform.position);        
        //ReGen();
    }

    IEnumerator FadeOut()
    {
        Renderer r = skeletonAnimation.GetComponent<Renderer>();
        
        float alpha = 1f;
        while(alpha > 0f)
        {
            if (alpha < 0f)
                alpha = 0f;

            for (int i = 0; i < r.materials.Length; i++)
            {                
                Color c = r.materials[i].color;
                c.a = alpha;
                r.materials[i].color = c;
            }

            alpha -= 0.02f;

            yield return null;
        }

        coroutineFadeOut = null;
    }

    void OnRevive(BattleUnit unit)
    {
        if(!isSummonded)
            skeletonAnimation.state.SetAnimation(0, idleAnimation, true);

        //SetBattleGroup(battleGroup);//?

        tomestone = null;
        //if (tomestone)
        //    tomestone.gameObject.SetActive(false);

        if (objShadow)
            objShadow.gameObject.SetActive(true);

        skeletonAnimation.gameObject.SetActive(true);
        skeletonAnimation.enabled = true;

        if (coroutineFadeOut != null)
        {
            StopCoroutine(coroutineFadeOut);
            coroutineFadeOut = null;
        }            

        Renderer r = skeletonAnimation.GetComponent<Renderer>();
        float alpha = 1f;
        for (int i = 0; i < r.materials.Length; i++)
        {
            Color c = r.materials[i].color;
            c.a = alpha;
            r.materials[i].color = c;
        }
    }

    
    Coroutine coroutineWaitToRevive = null;
    Coroutine coroutineWaitToDespawn = null;

    /// <summary> 부활 시간. Time.time </summary>
    public float regenTime;

    /// <summary> 부활 대기 </summary>
    IEnumerator WaitToRevive()
    {
        regenTime = Time.time + 5f;
        
        //Todo: 부활 대기 시간을 공식으로 계산해야 함
        yield return new WaitForSeconds(5f);

        //자리로 가서 부활
        ReGen();

        coroutineWaitToRevive = null;
    }

    /// <summary> 소멸 대기 </summary>
    IEnumerator WaitToDespawn()
    {
        string heroId = string.Empty;
        heroId = master.heroData.heroID;
        if (heroId.Contains("Ninja") && isSummonded)
        {
            if (gameObject.GetComponent<ParticleEffect>() != null)
            {
                gameObject.GetComponent<ParticleEffect>().PlayParticleEffect();
                yield return new WaitForSeconds(spineAnimationDie.duration);
            }   
        }

        if (!isSummonded)
            yield return new WaitForSeconds(15f);

        while (coroutineFadeOut != null)
            yield return null;

        coroutineWaitToDespawn = null;

        Despawn();
    }

    /// <summary> 스킬 사용 </summary>
    override protected void ExecuteAvailibleSkills()
    {
        if (battleGroup.battlePhase != BattleGroup.BattlePhase.Battle && battleGroup.battlePhase != BattleGroup.BattlePhase.Finish)
            return;

        if (isDie)
            return;

        //Todo: 스킬을 쓰면 안 되는 상황에서 return
        //스킬 사용 불가 상태란? 사망, 배틀페이즈가 배틀 상태가 아닐 때, 침묵, 저주, 행동불가?

        base.ExecuteAvailibleSkills();
    }

    override protected void OnStartSkill(SkillBase skill)
    {
        if (skill.moveBehavior != null)
        {
            currentMoveBehavior = skill.moveBehavior;
        }
    }

    override protected void OnFinishSkill(SkillBase skill)
    {
        for (int i = 0; i < skillList.Count; i++)
        {
            if (skill != skillList[i])
                continue;

            if (skillList[i].skillEffectDic.ContainsKey("OnEnd"))
            {
                List<ISkillEffect> skillEventList = skillList[i].skillEffectDic["OnEnd"];
                for (int j = 0; j < skillEventList.Count; j++)
                {
                    skillEventList[j].TriggerEffect();
                    //Debug.Log("호우");
                }
            }
        }

        for (int i = 0; i < counterSkillList.Count; i++)
        {
            if (skill != counterSkillList[i])
                continue;

            if (counterSkillList[i].skillEffectDic.ContainsKey("OnEnd"))
            {
                List<ISkillEffect> skillEventList = counterSkillList[i].skillEffectDic["OnEnd"];
                for (int j = 0; j < skillEventList.Count; j++)
                {
                    skillEventList[j].TriggerEffect();
                    //Debug.Log("호우");
                }
            }
        }
       
        
    }

    /// <summary> (영웅세팅)해당 클래스에서 가장 처름 시작되야 하는 곳</summary>
    public void Init(BattleGroup _battleGroup, HeroData data, BattleUnit.Team team = Team.Red)
    {
        heroData = data;

        if (_battleGroup != null)
            heroData.battleGroupID = _battleGroup.battleType.ToString();
        else
            heroData.battleGroupID = string.Empty;

        SetBattleGroup(_battleGroup);

        buffController.owner = this;

        if (team != Team.Red)
            data.level = battleGroup.stage;

        //heroDataID = data.id;        

        if (team == Team.Red && !isSummonded)
        {
            //ModifiableStat statHPRegen = stats.CreateOrGetStat<ModifiableStat>(StatType.HPRegen);
            //statHPRegen.baseValue = 5;
            //statHPRegen.UpdateModifiers();
        }            

        stats.Init();

        heroData.RecalculateStats(team == Team.Red, power);

        //RecalculateBaseParams();

        defaultMoveBehavior = new HeroMoveBehaviorRun();
        defaultMoveBehavior.owner = this;
        currentMoveBehavior = defaultMoveBehavior;

        curHP = maxHP;

        transform.localScale = originalScale;

        //보스 사이즈 보정
        orderController.bossModify = isBoss ? 1.3f : 1f;

        UpdateActiveState();

        shakeAmount = 0f;
    }

    public void InitPvP(BattleGroup _battleGroup, HeroData data)
    {
        heroData = data;

        if (_battleGroup != null)
            heroData.battleGroupID = _battleGroup.battleType.ToString();
        else
            heroData.battleGroupID = string.Empty;

        SetBattleGroup(_battleGroup);

        buffController.owner = this;

        stats.Init();

        heroData.RecalculateStats(true, power);

        defaultMoveBehavior = new HeroMoveBehaviorRun();
        defaultMoveBehavior.owner = this;
        currentMoveBehavior = defaultMoveBehavior;

        curHP = maxHP;

        transform.localScale = originalScale;

        UpdateActiveState();

        shakeAmount = 0f;
    }

    public IMoveable defaultMoveBehavior;
    IMoveable _currentMoveBehavior;
    public IMoveable currentMoveBehavior
    {
        get { return _currentMoveBehavior; }
        set
        {
            _currentMoveBehavior = value;

            if (value == null)
                moveBehavior = string.Empty;
            else
                moveBehavior = value.GetType().ToString();            
        }
    }

    string moveBehavior;

    void OnAddHeroToTeamList(BattleHero hero)
    {
        return;
        //패시브 스킬 적용
        ExcutePassiveSkill();
    }
    
    public float lastSpawnTime { get; set; }
    
    public enum SkeletonAnimState
    {
        None,
        Idle,
        Run,
        Skill
    }

    public SkeletonAnimState skeletonAnimState
    {
        get
        {
            if (!skeletonAnimation)
                return SkeletonAnimState.None;
            else
            {
                if(skeletonAnimation.AnimationName == idleAnimation)
                    return SkeletonAnimState.Idle;
                else if(skeletonAnimation.AnimationName == runAnimation)
                    return SkeletonAnimState.Run;
                else
                    return SkeletonAnimState.Skill;
            }
        }
    }


    float fallingSpeed = 2f;

    override protected void Update()
    {
        if (battleGroup == null)
            return;


        

        base.Update();

        //추락
        if ((airborneCoroutine == null && falldownCoroutine == null && coroutineShake == null) && (isDie || isBlockMove))
        {
            skeletonAnimation.transform.localPosition = Vector2.Lerp(skeletonAnimation.transform.localPosition, Vector2.zero, 2f * Time.deltaTime);
        }
        

        //공중 높이 복귀        
        if (skeletonAnimState != SkeletonAnimState.Skill && airborneCoroutine == null && falldownCoroutine == null && coroutineShake == null)
        {   
            if (skeletonAnimation.transform.localPosition.y < originalPosY || skeletonAnimation.transform.localPosition.y > originalPosY)
            {
                //공중 복귀
                if(originalPosY > 0f)
                {
                    if (skeletonAnimation.transform.localPosition.y < originalPosY + 0.02f && skeletonAnimation.transform.localPosition.y > originalPosY - 0.02f)
                    {
                        skeletonAnimation.transform.localPosition = new Vector2(0f, originalPosY);
                    }
                    else
                    {
                        skeletonAnimation.transform.localPosition = Vector2.Lerp(skeletonAnimation.transform.localPosition, new Vector2(0f, originalPosY), 2f * Time.deltaTime);
                    }
                }
                //추락
                else
                {
                    Vector2 destPos = Vector2.MoveTowards(skeletonAnimation.transform.localPosition, new Vector2(0f, originalPosY), fallingSpeed * Time.deltaTime);
                    if (skeletonAnimation.transform.localPosition.y < originalPosY && destPos.y >= originalPosY
                        || skeletonAnimation.transform.localPosition.y > originalPosY && destPos.y <= originalPosY
                        )
                    {
                        skeletonAnimation.transform.localPosition = new Vector2(0f, originalPosY);
                        fallingSpeed = 2f;
                    }
                    else
                    {
                        float acc = originalPosY > 0f ? 2f : 9.8f;
                        skeletonAnimation.transform.localPosition = destPos;

                        fallingSpeed += acc * Time.deltaTime;
                    }
                }   
            }            
        }
        Renderer r = skeletonAnimation.GetComponent<Renderer>();
        //최종 이동 속도 적용. 이동속도 증가량, 식량 상태에 의한 보정
        if (isDie)
        {
            skeletonAnimation.timeScale = 1f;

            mpb.SetColor("_OverlayColor", new Color(0f, 0f, 0f, 0.5f));
            r.SetPropertyBlock(mpb);
        }
        else if(shakeAmount > 0.01f)
        {
            skeletonAnimation.timeScale = 0f;
            if (coroutineLevelUp == null)
            {
                mpb.SetColor("_OverlayColor", new Color(1f,0f,0f,0.5f));
                r.SetPropertyBlock(mpb);
            }
        }
        else if(airborneCoroutine != null || falldownCoroutine != null)
        {
            skeletonAnimation.timeScale = 0f;
        }
        else
        {
            Stat statMoveSpeed = stats.GetParam(StatType.MoveSpeed);
            Stat statAttackSpeed = stats.GetParam(StatType.AttackSpeed);

            if (skeletonAnimation.AnimationName == runAnimation || skeletonAnimation.AnimationName == idleAnimation)
                skeletonAnimation.timeScale = Mathf.Max(0.1f, (float)(statMoveSpeed.value * 0.0001f));
            else
                skeletonAnimation.timeScale = Mathf.Max(0.1f, (float)(statAttackSpeed.value * 0.0001f));


            if (coroutineLevelUp == null)
            {
                r.SetPropertyBlock(null);
            }
        }

        //if(shakeAmount > 0f)
        {

            float a = 0f;
            if (shakeAmount > 0f)
            {
                a = Mathf.Sin(Time.time * 20f);
                if (a > 0)
                    a = 1;
                else if (a < 0)
                    a = -1;
            }

            float s = shakeAmount * a;// UnityEngine.Random.Range(-shakeAmount, shakeAmount);
            Vector3 shakePos = new Vector3(s, 0f, 0f);


            Vector3 localPos = new Vector3(0f, skeletonAnimation.transform.localPosition.y, skeletonAnimation.transform.localPosition.z);
            skeletonAnimation.transform.localPosition = Vector3.Lerp(skeletonAnimation.transform.localPosition, localPos + shakePos, 10f * Time.deltaTime);


        }


        ////스탯 테스트
        //if (!UnityEditor.Selection.Contains(gameObject))
        //    return;

        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {   
            StatModifier mod = new StatModifier();
            mod.type = StatModifier.Type.BaseValuePercent;
            mod.value = 1000f;
            stats.AddStatModifier(StatType.MoveSpeed, mod);
        }
        else if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            var mod = stats.GetParam<ModifiableStat>(StatType.MoveSpeed).modifiers.Find(x => x.type == StatModifier.Type.BaseValuePercent);
            stats.RemoveModifier(StatType.MoveSpeed, mod);
        }
    }
    
    void OnEndAnimation(TrackEntry entry)
    {
        //return;
        //Debug.Log("OnEndAnimation");
        //if (entry.animation.name != idleAnimation)
        //    ExecuteAvailibleSkills();

        //if (heroData.heroID == "Centaur_01_Hero")
        //{
        //    Debug.Log(entry.animation.name + " -> " + skeletonAnimation.AnimationName);
        //}
    }
    
    public void LevelUp(double _exp)
    {
        //현재 레벨 최대 체력 임시 저장
        double lastMaxHP = maxHP;

        //누적 경험치 증가
        heroData.exp += _exp;

        //레벨 상승
        heroData.level++;

        //최대 체력 증가량 만큼 현재 체력 올려줌. 사망 상태에서는 피 안 참
        if (!isDie)
        {
            double hpDelta = maxHP - lastMaxHP;
            curHP += hpDelta;
        }


        //레벨업 연출
        if (coroutineLevelUp != null)
        {
            StopCoroutine(coroutineLevelUp);
            coroutineLevelUp = null;
        }
        coroutineLevelUp = StartCoroutine(LevelUpEffect());
            
        //StopCoroutine("LevelUpEffect");
        //StartCoroutine("LevelUpEffect");
    }

    Coroutine coroutineLevelUp = null;
    IEnumerator LevelUpEffect()
    {
        Renderer r = skeletonAnimation.GetComponent<Renderer>();

        if (isDie)
        {
            r.SetPropertyBlock(null);
            yield break;
        }
            
        //레벨업 글씨 파티클
        GameObject objLevelUp = Battle.GetObjectInPool(Battle.levelUpPrefab.name);
        if (!objLevelUp)
        {
            objLevelUp = Instantiate(Battle.levelUpPrefab);
            objLevelUp.transform.SetParent(Battle.Instance.transform);
            objLevelUp.name = Battle.levelUpPrefab.name;
            Battle.AddObjectToPool(objLevelUp);
        }
        objLevelUp.GetComponent<BattleGroupElement>().SetBattleGroup(battleGroup);
        objLevelUp.GetComponent<OrderController>().parent = orderController;
        objLevelUp.transform.position = center.position;
        objLevelUp.gameObject.SetActive(true);
        
        if (!r)
            yield break;

        float time = 0.5f;

        Color color = Color.white;
        color.a = 1f;

        mpb.SetColor("_OverlayColor", color);
        r.SetPropertyBlock(mpb);

        
        float startTime = Time.time;
        float elapsedTime = 0f;
        while (elapsedTime < time)
        {
            elapsedTime = Time.time - startTime;

            float a = Mathf.Cos((elapsedTime / time) * Mathf.PI * 0.5f);
            color.a = a;

            mpb.SetColor("_OverlayColor", color);
            r.SetPropertyBlock(mpb);

            yield return null;
        }
        
        r.SetPropertyBlock(null);

        coroutineLevelUp = null;
        yield break;
    }

    float shakeAmount = 0f;

    Coroutine coroutineShake = null;
    public void Shake(float shake, float time, float interval)
    {

        if (coroutineShake != null)
        {
            StopCoroutine(coroutineShake);
            shakeAmount = 0f;
            coroutineShake = null;
        }
        shakeAmount = shake;

        if(gameObject.activeSelf)
            StartCoroutine(ShakeA(time, interval));
    }
    IEnumerator ShakeA(float time, float interval)
    {
        //float startAmount = shakeAmount;
        //float d = shakeAmount / time;
        float startTime = Time.time;
        float elapsedTime = 0f;
        while (elapsedTime < time)
        {
            yield return null;
            elapsedTime = Time.time - startTime;
            //shakeAmount = Mathf.Lerp(shakeAmount, 0f, elapsedTime / time);
        }

        shakeAmount = 0f;
        coroutineShake = null;
    }

    public void Pull(Vector2 pullPoint,float time)
    {
        if(pullCoroutine != null)
        {
            StopCoroutine(pullCoroutine);
            pullCoroutine = null;
        }
        
        if (gameObject.activeSelf)
            pullCoroutine = StartCoroutine(PullA(pullPoint, time));
    }
    Coroutine pullCoroutine;

    IEnumerator PullA(Vector2 pullPoint,float time)
    {
 
        float startTime = Time.time;
        float elapsedTime = 0f;
        while (elapsedTime < 1)
        {
            yield return null;
            elapsedTime = (Time.time - startTime) / time;
            
            transform.position = Vector2.Lerp(transform.position, pullPoint, elapsedTime);
        }
        pullCoroutine = null;
    }

    public void Dash(Vector2 dashPoint,float time)
    {
        
        if (DashCoroutine != null)
        {
            return;
            isBlockMove = false;
            StopCoroutine(DashCoroutine);
            DashCoroutine = null;
        }

        if (gameObject.activeSelf)
            DashCoroutine = StartCoroutine(DashA(dashPoint, time));
    }
    Coroutine DashCoroutine;

    IEnumerator DashA(Vector2 dashPoint, float time)
    {
        isBlockMove = true;
        isSuperArmor = true;
        float startTime = Time.time;
        float elapsedTime = 0f;
        //Debug.Log(transform.position + "->" + dashPoint);
        while (elapsedTime < 1)
        {
            elapsedTime = (Time.time - startTime) / time;
            transform.position = Vector2.Lerp(transform.position, dashPoint, elapsedTime);
            yield return null;
        }
       
        isBlockMove = false;
        isSuperArmor = false;
        DashCoroutine = null;
    }

    public void Push(SkillBase attackerSkill, float time)
    {
        if (pushCoroutine != null)
        {
            return;
        }

        if (gameObject.activeSelf)
            pushCoroutine = StartCoroutine(PushA(attackerSkill, time));
    }
    Coroutine pushCoroutine;

    IEnumerator PushA(SkillBase attackerSkill, float time)
    {
        //ProjectileUnit dd = new ProjectileUnit();
        float startTime = Time.time;
        float elapsedTime = 0f;
        //Debug.Log("밀림시작");
        
        while (elapsedTime < 1)
        {
            if (attackerSkill.owner.isDie || this.isDie || attackerSkill.gameObject.activeSelf == false)
                break;

            yield return null;
            elapsedTime = (Time.time - startTime) / time;
            float force = 2f;

            if (this.transform.position.x < attackerSkill.transform.position.x)
                force = -force;
          
            isBlockMove = true;
            Vector2 pushPoint = new Vector2(attackerSkill.transform.position.x + force,transform.position.y);

            


            float dis = Vector2.Distance((Vector2.right * attackerSkill.transform.position.x), (Vector2.right * transform.position.x));

            if (addedForce == Vector3.zero && attackerSkill.IsCastTargetInSkillRange())
                transform.position = Vector2.MoveTowards(transform.position, pushPoint, 10 * Time.deltaTime);
            else if(dis < 2)
                transform.position = Vector2.MoveTowards(transform.position, pushPoint, 10 * Time.deltaTime);
        }   
        isBlockMove = false;
        pushCoroutine = null;
    }

    public void Knockback2(SkillBase attackerSkill, float force)
    {
        if (Knockback2Coroutine != null)
        {
            //float dis = Vector2.Distance((Vector2.right * attackerSkill.transform.position.x), (Vector2.right * transform.position.x));
            //if (dis < 2)
            //{
            //    StopCoroutine(Knockback2Coroutine);
            //    Knockback2Coroutine = null;
            //}
            //else
            //    return;

            StopCoroutine(Knockback2Coroutine);
            Knockback2Coroutine = null;
        }

        if (gameObject.activeSelf)
            Knockback2Coroutine = StartCoroutine(Knockback2A(attackerSkill, force));
    }
    Coroutine Knockback2Coroutine;
    bool test = false;
    IEnumerator Knockback2A(SkillBase attackerSkill, float force, float time = 1)
    {

        float startTime = Time.time;
        float elapsedTime = 0f;
        //Debug.Log("밀림시작");

        bool isTestA = transform.position.x < attackerSkill.transform.position.x;

        ProjectileNormal projectile = attackerSkill.GetComponent<ProjectileNormal>();
        if(projectile)
        {
            if(projectile.isMoveForwardRight != isTestA)
                yield break;
        }
        //if(test != isTestA)
        //{
        //    test = isTestA;
        //    yield break;
        //}

        if (transform.position.x < attackerSkill.transform.position.x)
            force = -force;

        //bool isTestA = transform.position.x < attackerSkill.transform.position.x;
        //bool isTestC = false;
        while (elapsedTime < 1)
        {
            if (this.isDie)
                break;

            yield return null;

            // 시간 판단
            elapsedTime = (Time.time - startTime) / time;


            //bool isTestB = transform.position.x < attackerSkill.transform.position.x;
            //isTestC = isTestA == isTestB;
           
            //if (isTestC)
            //{
               
            //}
            
            //if (force < 0)
            //{
            //    Debug.Log("날라가");
            //}
           

            

            //isBlockMove = true;
            Vector2 pushPoint = new Vector2(attackerSkill.transform.position.x + force, transform.position.y);

            //float dis = Vector2.Distance((Vector2.right * pushPoint.x), (Vector2.right * transform.position.x));
            //if (dis < 2)
            //{
            //    break;
            //}


            

            if (attackerSkill.IsCastTargetInSkillRange())
            {
                //transform.position = Vector2.Lerp(transform.position, pushPoint, elapsedTime);
                transform.position = Vector2.MoveTowards(transform.position, pushPoint, 1);
            }
            else
            {
                break;
            }


            
            

        }
        //isBlockMove = false;
        pushCoroutine = null;
    }

    public void Airborne(SkillBase attackerSkill, float force)
    {
        if (airborneCoroutine != null)
        {
            return;
        }

        if (gameObject.activeSelf)
            airborneCoroutine = StartCoroutine(AirborneA(attackerSkill, force));
    }
    Vector2 beforeAirbornePos = Vector2.zero;
    Coroutine airborneCoroutine = null;
    IEnumerator AirborneA(SkillBase attackerSkill, float force, float time = 1)
    {       
        float startTime = Time.time;
        float elapsedTime = 0f;
        //Debug.Log("밀림시작");

        //?
        isBlockMove = false;

        float x = 5f;
        if (attackerSkill.owner.transform.position.x > transform.position.x)
            x = -5f;

        Vector3 destPos = transform.position + Vector3.right * x;

        Renderer r = skeletonAnimation.GetComponent<Renderer>();
        Color curColor = new Color(1f, 0f, 0f, 0.5f);
        mpb.SetColor("_OverlayColor", curColor);
        r.SetPropertyBlock(mpb);

        beforeAirbornePos = transform.position;
        float deltaTime = 0f;
        float lastTime = Time.time;
        while (elapsedTime < 2f)
        {
            if (isDie)
                break;

            elapsedTime = Time.time - startTime;

            deltaTime = Time.time - lastTime;

            float y = skeletonAnimation.transform.localPosition.y + 8f;
            if (y > 8f)
            {
                y = 8f;
            }

            //pushPoint = new Vector2(skeletonAnimation.transform.position.x, y);

            skeletonAnimation.transform.localPosition = Vector3.Lerp(skeletonAnimation.transform.localPosition, Vector3.up * y, y * deltaTime * 0.5f);

            transform.position = Vector3.Lerp(transform.position, destPos, x * deltaTime * 0.5f);

            if (elapsedTime > 0.4f)
            {
                curColor = Color.Lerp(curColor, new Color(1f, 0f, 0f, 0f), elapsedTime - 0.4f);
                mpb.SetColor("_OverlayColor", curColor);
                r.SetPropertyBlock(mpb);
            }


            lastTime = Time.time;

            yield return null;
        }

        airborneCoroutine = null;

        yield break;

        //orderController.enabled = false;
        Vector2 pushPoint = Vector2.zero;
        while (elapsedTime < 1 * Time.timeScale)
        {
            if (this.isDie)
                break;

            yield return null;

            // 시간 판단
            elapsedTime = (Time.time - startTime) / time;

            float y = skeletonAnimation.transform.position.y + force;
            if (y > -135.25f)
            {
                y = -135.25f;
            }

            pushPoint = new Vector2(skeletonAnimation.transform.position.x, y);
            
            skeletonAnimation.transform.position = Vector2.MoveTowards(skeletonAnimation.transform.position, pushPoint, 1 * Time.timeScale);
            
        }
        while (airborne)
        {
            if (isDie)
            {
                break;
            }
            

            skeletonAnimation.transform.position = pushPoint;
            yield return null;
        }
        
        //orderController.enabled = true;
        airborneCoroutine = null;
    }

    public void FallingDown(SkillBase attackerSkill, float force)
    {
        if (airborne || heroData.baseData.type == HeroData.HeroBattleType.Air)
        {
            if (falldownCoroutine != null)
            {
                return;
            }

            if (gameObject.activeSelf)
                falldownCoroutine = StartCoroutine(FallingDownA(attackerSkill, force));
        }
    }
    Coroutine falldownCoroutine = null;
    IEnumerator FallingDownA(SkillBase attackerSkill, float force, float time = 1)
    {
        if(beforeAirbornePos == Vector2.zero)
        {
            beforeAirbornePos = new Vector2(transform.position.x, transform.position.y);
        }

        float startTime = Time.time;
        float elapsedTime = 0f;
        //Debug.Log("밀림시작");


        if (transform.position.x < attackerSkill.transform.position.x)
            force = -force;

        orderController.enabled = false;
        Vector2 fallDownPoint = new Vector2(beforeAirbornePos.x + force, beforeAirbornePos.y);

        if (airborneCoroutine != null)
        {
            StopCoroutine(airborneCoroutine);
            airborneCoroutine = null;
        }
        
        //얻어 맞는 순간 타격감 표현
        Shake(0.4f, 0.1f, 0.06f);
        while (elapsedTime < 0.1f)
        {
            elapsedTime = Time.time - startTime;
            yield return null;
        }
        
        startTime = Time.time;
        elapsedTime = 0f;
        float speed = 25f;
        float gravity = 10f;

        Vector2 startPos = skeletonAnimation.transform.localPosition;

        float deltaTime = 0f;
        float lastTime = Time.time;
        while (elapsedTime < 2f)
        {
            if (isDie)
                break;

            // 시간 판단
            elapsedTime = Time.time - startTime;
            deltaTime = Time.time - lastTime;

            if (attackerSkill.IsCastTargetInSkillRange())
            {
                //transform.position = Vector2.MoveTowards(transform.position, fallDownPoint, 0.5f * Time.timeScale);
                speed += gravity * deltaTime;
                skeletonAnimation.transform.localPosition = Vector2.MoveTowards(skeletonAnimation.transform.localPosition, Vector3.zero, speed * deltaTime);
            }
            else
            {
                break;
            }

            if(skeletonAnimation.transform.localPosition.y <= 0f)
            {
                skeletonAnimation.transform.localPosition = Vector3.zero;
                break;
            }

            lastTime = Time.time;

            yield return null;
        }

        buffController.DetachBuff(buffController.buffList.Find(x => x.baseData.airborne));
        

        //if (isDie)
        //{
        //    transform.position = fallDownPoint;
        //    tomestone.transform.position = transform.position;
        //}
        //else
        //{
        //    transform.position = fallDownPoint;
        //}

        if(attackerSkill.owner.skillList.Find(x=>x.skillData.id.Contains("Impact")) != null)
        {
            SkillBase skill = attackerSkill.owner.skillList.Find(x => x.skillData.id.Contains("Impact"));
            skill.castTarget = this;
            skill.Execute();
        }
        orderController.enabled = true;
        falldownCoroutine = null;
    }

    void OnTakeDamage(double damage, string tag)
    {
        if (!isActive)
            return;

        if (!OptionManager.Instance.isOnDamageEffect)
            return;

        GameObject objDamageText = Battle.GetObjectInPool(Battle.damageTextPrefab.name);
        if (!objDamageText)
        {
            objDamageText = Instantiate(Battle.damageTextPrefab, battleGroup.canvasUIBattleCharacter.transform, false);
            objDamageText.name = Battle.damageTextPrefab.name;
            Battle.AddObjectToPool(objDamageText);
        }

        objDamageText.transform.SetParent(battleGroup.canvasUIBattleCharacter.transform, false);


        UIDamageText damageText = objDamageText.GetComponent<UIDamageText>();


        if (tag == "Heal" || tag == "Resurrect")
        {
            damageText.value = (-damage).ToStringABC();//.ToString("0");
        }        
        else if (tag == "Guard")
        {
            damageText.value = damage.ToStringABC() + "(보호)";
        }
        else if (tag == "ImmuneDamage")
        {
            damageText.value = "면역";
        }
        else if(tag == "Miss")
        {
            damageText.value = "Miss";
        }
        else
        {
            if (damage >= 0 && damage < 1f)
                damageText.value = ".";
            else
                damageText.value = damage.ToStringABC();
        }

        //tag에 따라 색상 다르게 표현
        if (tag == "Guard")
            damageText.color = Color.grey;
        else if (tag == "Heal" || tag == "Resurrect")
            damageText.color = Color.green;
        else if (tag == "Drain")
            damageText.color = Color.green * 0.8f;
        else
            damageText.color = Color.white;

        damageText.transform.position = uiPivot.transform.position;

        if(tag == "Guard" || tag == "Drain")
            damageText.transform.localScale = Vector3.one * 0.7f;
        else
            damageText.transform.localScale = Vector3.one;

        damageText.gameObject.SetActive(true);

        damageText.Show();
    }

#if UNITY_EDITOR
    public bool showDebug = false;
    //######################## Debug #################################################################

    void OnGUI()
    {
        if (!UnityEditor.Selection.Contains(gameObject))
            return;

        List<StatType> statTypes = stats.paramDic.Keys.ToList();
        
        GUIStyle blStyle = new GUIStyle();
        blStyle.normal.textColor = Color.white;
        blStyle.alignment = TextAnchor.MiddleLeft;
        
        //GUI.Label(new Rect(10, 45, 200, 20), " stat count : " + stats.paramDic.Count, blStyle);

        float y = 45;
        for (int i = 0; i < statTypes.Count; i++)
        {
            ModifiableStat stat = stats.GetParam<ModifiableStat>(statTypes[i]);            
            GUI.Label(new Rect(10, y, 200, 20), statTypes[i] + " : " + stat.value, blStyle);
            y += 20f;
            //Debug.Log(stat.name + " : " + stat.value);
            for (int a = 0; a < stat.modifiers.Count; a++)
            {
                GUI.Label(new Rect(30, y, 200, 20), "modifier : " + stat.modifiers[a].type + ", " + stat.modifiers[a].value , blStyle);
                y += 20f;
            }
        }

        GUI.Label(new Rect(30, y, 200, 20), "animTimeScale : " + skeletonAnimation.timeScale/*.state.TimeScale*/, blStyle);
        
    }

    void OnDrawGizmosSelected()
    {
        


        if (!showDebug)
            return;

        if (gameObject != UnityEditor.Selection.activeGameObject)
            return;

        if (!battleGroup)
            return;
        
        GUIStyle style = new GUIStyle();

        
        BattleHero enemy = nearestEnemy;

        Color color = Color.white;

        style.normal.textColor = color;

        UnityEditor.Handles.color = color;
        UnityEditor.Handles.Label(enemy.transform.position, enemy.GetDistanceFrom(this).ToString(), style);

    }
#endif
}