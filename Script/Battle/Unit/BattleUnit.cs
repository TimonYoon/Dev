using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


/// <summary> 스킬의 주체, 대상이 될 수 있는 개체 /// </summary>
public partial class BattleUnit : BattleGroupElement
{
    public delegate void BattleUnitDelegate(BattleUnit unit);
    public delegate void BattleDelegate();

    public delegate void BattleDamageDelegate(double demage, string tag);

    
    
    public BattleUnitDelegate onDie;
    public BattleUnitDelegate onRevive;

    public BattleDamageDelegate onHit;

    public enum Team
    {
        Red,
        Blue,
        None,
    };



    //public string heroDataID { get; private set; }

    protected HeroData _heroData;
    public HeroData heroData
    {
        get
        {
            //if (_heroData == null)
            //    _heroData = HeroManager.heroDataBattleList.Find(x => x.id == heroDataID);

            return _heroData;
        }
        protected set
        {
            _heroData = value;

            //패시브 스킬들 초기화. 패시브 스킬은 hero데이타에 지정되어 있는 것중 패시브인것들만.. 딱히 skillsetting 필요 없을 것 같아서.
            if (heroData.baseData != null)
                passiveSkillList.Clear();

            for (int i = 0; i < heroData.baseData.skillDataList.Count; i++)
            {
                SkillData data = heroData.baseData.skillDataList[i];
                if (data.activeType != SkillBase.ActiveType.Passive)
                    continue;

                SkillBase skill = gameObject.AddComponent<SkillBase>();
                skill.skillData = data;

                ISkillEffect skillEffect = new SkillEffectMeleeHit(skill, null);
                List<ISkillEffect> skillEffects = new List<ISkillEffect>();
                skillEffects.Add(skillEffect);
                skill.skillEffectDic.Add("OnStart", skillEffects);

                skill.owner = this;
                passiveSkillList.Add(skill);
            }

            heroData.onChangedValue += OnChangedHeroDataParam;
            //ExcutePassiveSkill();

            //battleGroup.onAddBattleHero += OnAddBattleHero;
        }
    }

    void OnChangedHeroDataParam(PropertyInfo p)
    {
        if (p.Name == "enhance" || p.Name == "rebirth" || p.Name == "level")
            RecalculateBaseParams();
    }
    
    /// <summary> 이 영웅의 주인. 보통은 자기 자신. 소환수/소환물의 경우 자신을 소환한 개체 </summary>
    public BattleUnit master { get; set; }

    Team _team = Team.none;
    public Team team
    {
        get
        {
            for (int i = 0; i < buffController.buffList.Count; i++)
            {
                // 매혹 걸렸을 때 팀 판단 반대로
                if (buffController.buffList[i].isActive && buffController.buffList[i].baseData.effect == "Charm")
                {
                    Team fackTeam = _team;
                    if (fackTeam != Team.none)
                        fackTeam = fackTeam == Team.Blue ? Team.Red : Team.Blue;
                    return fackTeam;
                }
            }

            return _team;
        }
        set
        {
            _team = value;
        }
    }
       

    public CoolTimeController coolTimeController { get; protected set; }

    public Vector3 addedForce = Vector3.zero;

    public SimpleDelegate onChangedCumulativeDamage;
    double _cumulativeDamage;
    /// <summary> 한 스테이지 누적데미지 </summary>
    public double cumulativeDamage
    {
        get { return _cumulativeDamage; }
        protected set
        {

            bool isChack = _cumulativeDamage != value;

            _cumulativeDamage = value;

            if (isChack && onChangedCumulativeDamage != null)
                onChangedCumulativeDamage();

        }
    }


    bool _isSummonded = false;
    public bool isSummonded
    {
        get { return _isSummonded; }
        set { _isSummonded = value; }
    }

    bool _canResurrect = true;
    public bool canResurrect
    {
        get { return _canResurrect; }
        set
        {
            if (_canResurrect == value)
                return;

            _canResurrect = value;

            if (onChangedCanResurrect != null)
                onChangedCanResurrect();
        }
    }

    public SimpleDelegate onChangedCanResurrect;

    bool _isDie = false;
    public bool isDie
    {
        get { return _isDie; }
        protected set
        {
            bool isChanged = value != _isDie;

            if (value == _isDie)
                return;

            _isDie = value;
            if (_isDie)
            {

                if (isSummonded)
                    master.summonCount -= 1;

                if (isChanged && onDie != null)
                {
                    for (int i = 0; i < buffController.buffList.Count; i++)
                    {
                        if(buffController.buffList[i].baseData.resetType == "OnDie")
                            buffController.DetachBuff(buffController.buffList[i]);
                    }
                    onDie(this);
                }
                    
            }
            else
            {
                if (isChanged && onRevive != null)
                    onRevive(this);
            }
        }
    }

    [NonSerialized]
    public bool isInitialized = false;

    /// <summary> 스킬 리스트 초기화 여부 </summary>
    [NonSerialized]
    public bool isInitializedSkillList = false;

    public new CircleCollider2D collider = null;

    public Transform center = null;
    
    List<BattleHero> enemyList = null;
    List<BattleHero> allyList = null;

    /// <summary> 도발한 대상 </summary>
    public BattleHero provokeEnemy { get; set; }

    /// <summary> 가장 가까이 있는 적 </summary>
    public BattleHero nearestEnemy { get; protected set; }

    /// <summary> 가장 멀리 있는 적 </summary>
    public BattleHero farthestEnemy { get; protected set; }

    /// <summary> 가장 체력이 적은 적 </summary>
    public BattleHero lowestHPEnemy { get; protected set; }

    /// <summary> 가장 체력이 많은 적 </summary>
    public BattleHero highestHPEnemy { get; protected set; }

    /// <summary> 가장 가까이 있는 아군 </summary>
    public BattleHero nearestAlly { get; protected set; }

    /// <summary> 가장 멀리 있는 아군 </summary>
    public BattleHero farthestAlly { get; protected set; }

    /// <summary> 가장 체력이 적은 아군 </summary>
    public BattleHero lowestHPAlly { get; protected set; }

    /// <summary> 가장 체력이 많은 아군 </summary>
    public BattleHero highestHPAlly { get; protected set; }

    /// <summary> 최전방에 있는 아군 </summary>
    public BattleHero frontMostAlly { get; protected set; }

    public float distance { get; protected set; }

    public Vector3 skillStartPos { get; set; }

    public SimpleDelegate onChangedSummonCount;

    int _summonCount;
    /// <summary> 자신의 소환수 숫자 - 네크로맨서 스킬용 </summary>
    public int summonCount
    {
        get { return _summonCount; }
        set
        {
            bool isChanged = _summonCount != value;
            _summonCount = value;
            if (isChanged && onChangedSummonCount != null)
                onChangedSummonCount();
        }
    }

    public SimpleDelegate onChangedAroundEnemyCount;

    int _aroundEnemyCount;
    /// <summary> 자신의 소환수 숫자 - 네크로맨서 스킬용 </summary>
    public int aroundEnemyCount
    {
        get { return _aroundEnemyCount; }
        set
        {
            bool isChanged = _aroundEnemyCount != value;
            _aroundEnemyCount = value;
            if (isChanged && onChangedAroundEnemyCount != null)
                onChangedAroundEnemyCount();
        }
    }

    /// <summary> 살라딘 스킬용 hp저장 변수 </summary>
    public double[] beforeHP = new double[5];
    private int order = 0;
    private float saveTime = 0f;

    /// <summary> 데미지 받지 않는 상태 (무적상태) </summary>
    public bool isSuperArmor { get; set; } 

    float _lifeTime = 0f;
    public float lifeTime
    {
        get { return _lifeTime; }
        set { _lifeTime = value; }
    }

    BuffController _buffController = null;
    public BuffController buffController
    {
        get
        {
            if (_buffController)
                return _buffController;

            GameObject go = new GameObject(name + "_BuffController");
            _buffController = go.AddComponent<BuffController>();
            //_buffController.owner = this;
            if(Battle.Instance)
                _buffController.transform.parent = Battle.Instance.transform;

            //_buffController = gameObject.AddComponent<BuffController>();
            //_buffController.owner = this;

            return _buffController;
        }
    }

    protected List<SkillBase> _skillList = new List<SkillBase>();
    public List<SkillBase> skillList
    {
        get { return _skillList; }
        private set { _skillList = value; }
    }

    public List<SkillBase> passiveSkillList = new List<SkillBase>();

    public bool notTargeting { get; set; }
        
    public bool isBlockMove { get; set; }

    
    public bool isBlockAttack { get; set; }

    public bool airborne { get; set; }

    /// <summary> 보스인지 여부. 몬스터만 사용 </summary>
    public bool isBoss
    {
        get { return _isBoss; }
        set { _isBoss = value; }
    }
    public bool _isBoss = false;

    public new Renderer renderer;

    public StatCollection stats
    {
        get
        {
            return heroData.stats;
        }
    }// = new BattleStats();
    //#######################################################
    override protected void Awake()
    {
        base.Awake();

        GameObject go = new GameObject(name + "_CoolTimeController");
        coolTimeController = go.AddComponent<CoolTimeController>();
        coolTimeController.unit = this;

        if(Battle.Instance)
            coolTimeController.transform.parent = Battle.Instance.gameObject.transform;

        renderer = GetComponentInChildren<Renderer>();

        master = this;
    }

    virtual protected void Start()
    {
        //to do : 테스트를 위해 주석함
        //if (!Battle.Instance)
        //    return;

        InitSkillList();
    }

    virtual public void Despawn(bool clearFromTeamList = true)
    {
        gameObject.SetActive(false);
    }
    
    public void ExcutePassiveSkill()
    {        
        for(int i = 0; i < passiveSkillList.Count; i++)
        {
            SkillBase skill = passiveSkillList[i];
            skill.Execute();
        }
    }

    protected void InitSkillList()
    {
        if (isInitializedSkillList)
            return;

        //스킬세팅에 설정된 스킬들 전부 등록
        SkillSetting[] skillStettings = GetComponentsInChildren<SkillSetting>();
        for (int i = 0; i < skillStettings.Length; i++)
        { 
            if (this is BattleHero)
            {
                SkillHero s = null;
                if (skillStettings[i] is SkillSettingDive)
                    s = gameObject.AddComponent<SkillDive>();
                else
                    s = gameObject.AddComponent<SkillHero>();

                s.Init(this, skillStettings[i]);

                if (s.skillData.triggerType == SkillBase.TriggerType.None)
                    skillList.Add(s);
                else
                    counterSkillList.Add(s);
            }
            else
            {
                SkillBase s = gameObject.AddComponent<SkillBase>();

                s.Init(this, skillStettings[i]);

                if(s.skillData.triggerType == SkillBase.TriggerType.None)
                    skillList.Add(s);
                else
                    counterSkillList.Add(s);
            }
        }

        //설정해 둔 스킬들 우선순위별로 정렬
        if (skillList != null && skillList.Count > 0)
            skillList = skillList.OrderByDescending(x => x.skillData.priority).ToList();
                

        //스킬들 쿨타임 초기화. 한 번 실행
        coolTimeController.Init();
                
        for(int i = 0; i < skillList.Count; i++)
        {
            //Todo: 시작할 때 쿨타임 초기화 할 것인지 말것인지도 데이타에 설정해야 할 것 같은데..
            SkillData skillData = skillList[i].skillData;

            //초기화 후 쿨타임 적용
            if (!string.IsNullOrEmpty(skillData.coolTime1ID))
                coolTimeController.ApplyCoolTime(skillData.coolTime1ID, skillData.coolTime1);

            if (!string.IsNullOrEmpty(skillData.coolTime2ID))
                coolTimeController.ApplyCoolTime(skillData.coolTime2ID, skillData.coolTime2);

            skillList[i].onStart += OnStartSkill;
            skillList[i].onFinish += OnFinishSkill;
        }
        for (int i = 0; i < counterSkillList.Count; i++)
        {
            SkillData skillData = counterSkillList[i].skillData;
            //초기화 후 쿨타임 적용
            if (!string.IsNullOrEmpty(skillData.coolTime1ID))
                coolTimeController.ApplyCoolTime(skillData.coolTime1ID, skillData.coolTime1);

            if (!string.IsNullOrEmpty(skillData.coolTime2ID))
                coolTimeController.ApplyCoolTime(skillData.coolTime2ID, skillData.coolTime2);

            counterSkillList[i].onStart += OnStartSkill;
            counterSkillList[i].onFinish += OnFinishSkill;
        }

        if(skillList.Count > 0)
        {
            
            defaultSkill = skillList[skillList.Count - 1];
        }
            

        //스킬 초기화 완료
        isInitializedSkillList = true;
    }

    virtual protected void OnStartSkill(SkillBase skill)
    {
    }
    virtual protected void OnFinishSkill(SkillBase skill)
    {
        //Debug.Log("짠");
    }

    virtual protected void Update()
    {    
        
        //넉백 띄우기 등
        if (Time.timeScale == 0f)
            return;

        Vector3 destPos = transform.position + new Vector3(addedForce.x, 0f, 0f);

        transform.position = destPos;

        if (addedForce != Vector3.zero)
            addedForce *= 0.8f;

        if (addedForce.magnitude < 0.1f)
        {
            addedForce = Vector3.zero;
        }
           

        if (Time.time > lastHPRegenTime + 1f)
        {
            if(!isDie && curHP < maxHP)
            {
                Stat statHPRegen = stats.GetParam(StatType.HPRegen);
                if (statHPRegen != null)
                {
                    double hpRegen = statHPRegen.value;

                    double regenAmount = hpRegen * 0.2f * (Time.time - lastHPRegenTime);
                    //if (regenAmount >= 1)
                    {
                        curHP += regenAmount;
                        lastHPRegenTime = Time.time;
                    }
                }
                
            }
        }



        if(isSummonded && lifeTime >= 0f)
        {
            lifeTime -= Time.deltaTime;
            if(lifeTime <= 0f)
            {
                curHP = 0;
                //Despawn();
                //SetBattleGroup(null);
            }
        }

        //살라딘 스킬용 변수 초기화 하드코딩
        string heroID = master.heroData.heroID;
        if(heroID.Contains("Saladin"))
        {
            if(saveTime < Time.time)
            {
                saveTime = Time.time + 1f;

                if(curHP != 0 || !isDie)
                {
                    beforeHP[order] = curHP;

                    order++;
                    if (order == 5) order = 0;
                }
            }
        }
            
    }

    float lastHPRegenTime = float.MinValue;

    virtual protected void ExecuteAvailibleSkills()
    {
        //if (transform.position.x < battleGroup.xMin + 0.5f || transform.position.x > battleGroup.xMax - 0.5f)
        //    return;

        for (int i = 0; i < skillList.Count; i++)
        {            
            SkillData skillData = skillList[i].skillData;

            //자동 발동 스킬 아니면 스킵
            if (!skillData.autoExecute)
                continue;
            
            //발동 가능한 상황인지 체크해서 발동
            skillList[i].CheckCastCondition();

            if (skillList[i].canCastSkill)
            {
                skillList[i].Execute();
                break;
            }
        }
    }

    public SkillBase defaultSkill { get; set; }

    public List<SkillBase> counterSkillList = new List<SkillBase>();

    virtual public bool Damage(BattleUnit attacker, double finalAttackPower, SkillBase.DamageType damageType = SkillBase.DamageType.NotDefined, SkillBase skill = null, string tag = "")
    {
        if(isSuperArmor)
        {
            //Debug.Log(attacker.heroData.heroName +"의 공격 막아짐 데미지 :" + finalAttackPower);
            return false;

        }

        if (skill != null && counterSkillList.Count > 0)
        {
            for(int i = 0; i < counterSkillList.Count; i++)
            {
                SkillBase counterSkill = counterSkillList[i];

                if(counterSkill.skillData.effectType == "CounterMeleePhysical")
                {
                    if (skill.skillData.damageType == SkillBase.DamageType.Physical
                        && skill.skillData.rangeType == SkillBase.RangeType.Melee
                        && skill.skillData.triggerType == SkillBase.TriggerType.None
                        && skill.skillData.collectTargetType == SkillBase.CollectTargetType.Target)
                    {
                        BattleHero h = attacker as BattleHero;
                        counterSkill.castTarget = h;
                        counterSkill.CheckCastCondition();
                        if (!counterSkill.isCoolTime && counterSkill.IsCastTargetInSkillRange())
                        {
                            counterSkill.Execute();
                            return false;
                        }
                        
                    }
                }

                if(counterSkill.skillData.effectType == "CounterRangePhysical")
                {
                    if(skill.skillData.damageType == SkillBase.DamageType.Physical
                        && skill.skillData.rangeType == SkillBase.RangeType.Range
                        && skill.skillData.triggerType == SkillBase.TriggerType.None)
                    {
                        BattleHero h = attacker as BattleHero;
                        counterSkill.castTarget = h;
                        counterSkill.CheckCastCondition();
                        if (!counterSkill.isCoolTime && counterSkill.IsCastTargetInSkillRange())
                        {
                            counterSkill.Execute();
                            
                            if (onHit != null)
                                onHit(finalAttackPower, "ImmuneDamage");
                            return false;
                        }
                    }
                }

                if(counterSkill.skillData.triggerType == SkillBase.TriggerType.OnHit && counterSkill.skillData.effectType == "Ninja_CounterAttack")
                {
                    if(damageType != SkillBase.DamageType.Pure)
                    {
                        BattleHero h = attacker as BattleHero;
                        counterSkill.castTarget = h;
                        counterSkill.CheckCastCondition();
                        //Debug.Log(counterSkill.currentCoolTime + "/" + counterSkill.isCoolTime);
                        if (counterSkill.canCastSkill && !counterSkill.isCoolTime && counterSkill.IsCastTargetInSkillRange())
                        {
                            SkillData skillData = counterSkill.skillData;
                            if (!string.IsNullOrEmpty(skillData.coolTime1ID))
                                coolTimeController.ApplyCoolTime(skillData.coolTime1ID, skillData.coolTime1);

                            if (!string.IsNullOrEmpty(skillData.coolTime2ID))
                                coolTimeController.ApplyCoolTime(skillData.coolTime2ID, skillData.coolTime2);

                            //counterSkill.onStart += OnStartSkill;
                            //counterSkill.onFinish += OnFinishSkill;

                            //Debug.Log("반격!");
                            // 닌자 반격 스킬 
                            counterSkill.Execute();
                            //캐릭터 위치에 따라 뒤집기
                            bool isFlip = false;
                            if (attacker is BattleHero)
                            {
                                isFlip = h.flipX;
                                float x = isFlip ? 1f : -1f;
                                transform.position = attacker.transform.position + (Vector3.right * 3 * x);
                            }

                            if (this is BattleHero)
                            {
                                BattleHero owner = this as BattleHero;
                                owner.flipX = isFlip;
                            }
                            return false;
                        }
                    }
                }
                else if(counterSkill.skillData.triggerType == SkillBase.TriggerType.OnHit && counterSkill.skillData.effectType == "CounterDash")
                {
                    if (damageType != SkillBase.DamageType.Pure)
                    {
                        
                        counterSkill.CheckCastCondition();
                        //Debug.Log(counterSkill.currentCoolTime + "/" + counterSkill.isCoolTime);
                        if (!counterSkill.isCoolTime)
                        {
                            SkillData skillData = counterSkill.skillData;
                            if (!string.IsNullOrEmpty(skillData.coolTime1ID))
                                coolTimeController.ApplyCoolTime(skillData.coolTime1ID, skillData.coolTime1);

                            if (!string.IsNullOrEmpty(skillData.coolTime2ID))
                                coolTimeController.ApplyCoolTime(skillData.coolTime2ID, skillData.coolTime2);

                            //Debug.Log("뱀파이어 회피");

                            BattleHero hero = this as BattleHero;
                            bool isFlip = hero.flipX;
                            float x = isFlip ? 1f : -1f;
                            
                            //카운터 대쉬 이기에 무적처리함 
                            isSuperArmor = true;

                            counterSkill.Execute();
                            return false;
                        }
                     
                    }
                }
                else if (counterSkill.skillData.triggerType == SkillBase.TriggerType.OnHit && counterSkill.skillData.effectType == "Saladin_TimeReverse")
                {
                    if (damageType != SkillBase.DamageType.Pure && finalAttackPower > counterSkill.owner.curHP)
                    {

                        counterSkill.CheckCastCondition();
                        //Debug.Log(counterSkill.currentCoolTime + "/" + counterSkill.isCoolTime);
                        if (!counterSkill.isCoolTime)
                        {
                            if(beforeHP[order] == 0)
                            {
                                curHP = maxHP;
                            }
                            else
                            {
                                curHP = beforeHP[order];
                            }
                            
                            counterSkill.Execute();
                            return false;
                        }

                    }
                }
            }
        }

        //공격력 식 재계산..

        //전체 피해감소
        double atk = attacker.stats.GetValueOf(StatType.AttackPower);
        double penetrateRatio = System.Math.Min(attacker.stats.GetValueOf(StatType.PenetrateRatio) * 0.0001, 0.99);
        double def = stats.GetValueOf(StatType.DefensePower) * (1 - penetrateRatio);
        double damageReduction = atk / def;


        //데미지 감소 적용
        if (damageType == SkillBase.DamageType.Magical || damageType == SkillBase.DamageType.Physical)
        {
            float resist = 0f;
            if (damageType == SkillBase.DamageType.Magical)
            {
                resist = (float) stats.GetValueOf(StatType.ReduceMagicalDamageRate) * 0.0001f;
            }                
            else if (damageType == SkillBase.DamageType.Physical)
            {
                resist = (float) stats.GetValueOf(StatType.ReducePhysicalDamageRate) * 0.0001f;
            }

            float reduceDamageRate = (float)stats.GetValueOf(StatType.ReduceDamageRate) * 0.0001f;


            finalAttackPower = finalAttackPower * damageReduction * Mathf.Max(1f - reduceDamageRate, 0f) * Mathf.Max(1f - resist, 0f);


            //Debug.Log(attacker.heroData.baseData.name + ", " + heroData.baseData.name + ", " + atk + ", " + def + ", " + finalAttackPower);
        }

        //보스 피해량 증가
        if (isBoss)
        {
            double increaseDamageRate = attacker.stats.GetValueOf(StatType.IncreaseDamageRateBoss) * 0.0001f;
            finalAttackPower = finalAttackPower * (1d + increaseDamageRate);
        }

        //무적 버프 처리를 위해 먼저 한 번 검색
        if (curHP < finalAttackPower)
        {
            for (int a = 0; a < buffController.buffList.Count; a++)
            {
                Buff buff = buffController.buffList[a];
                if (buff.baseData.trigger != "OnTakeDeadlyDamage")
                    continue;

                if (buff.triggerProbability < UnityEngine.Random.Range(1, 10001))
                    continue;

                BattleUnit target = null;
                if (buff.baseData.triggerTarget == "BuffTarget")
                    target = this;

                if (target && !target.isDie)
                    target.buffController.AttachBuff(target, buff.baseData.triggerBuff, 1, buff);
            }


            SkillBase skillBase = counterSkillList.Find(x => x.skillData.triggerType == SkillBase.TriggerType.OnDie);
            if (skillBase)
            {
                Buff buff = buffController.buffList.Find(x => x.id == "Buff_Shaman_Passive");
                if(buff != null && buff.isActive/* && buff.stack > 0*/)
                {
                    //Debug.Log("회생");
                    skillBase.Execute();
                    buffController.DetachBuff(buff);// .stack--;
                    return false;
                }               
               
            }
        }

        // 공중 영웅 추가 데미지 버프 처리
        Buff buffAirHero = attacker.buffController.buffList.Find(x => x.baseData.linkerType == "airHero" && x.isActive);
        if (buffAirHero != null && heroData.heroBattleType == HeroData.HeroBattleType.Air)
        {
            finalAttackPower *= (1 + (buffAirHero.statModifier.value * 0.0001d));
        }

        Buff buffDis = attacker.buffController.buffList.Find(x => x.baseData.linkerType == "distance" && x.isActive);
        if (buffDis != null)
        {
            float maxDis = 20f;
            float dis = Vector2.Distance(attacker.transform.position, this.transform.position);
            dis = Mathf.Min(dis, maxDis);
                       
            finalAttackPower *= (1 + (buffDis.statModifier.value * 0.0001d) * (maxDis - dis));
            //Debug.Log("거리 : " + dis + "/ " + (buffDis.statModifier.value * 0.0001d) * ((1 + maxDis - dis)));
        }



        //무적 버프
        Buff buffImmuneDamage = buffController.buffList.Find(x => x.baseData.effect == "ImmuneDamage" && x.isActive);
        if(buffImmuneDamage != null)
        {
            buffImmuneDamage.stack--;
            //Debug.Log(heroData.heroName + ") 공격 막음");
            finalAttackPower = 0;
            if (onHit != null)
                onHit(finalAttackPower, "ImmuneDamage");

            if (buffImmuneDamage.stack <= 0)
            {
                //Debug.Log("제거됨");
                buffController.DetachBuff(buffImmuneDamage.id);
            }
                
        }
        else
        {
            //얻어 맞았다는 콜백 호출
            if (onHit != null)
                onHit(finalAttackPower, tag);
        }

        //Stat dodgeStat = stats.GetParam(StatType.Dodge);
        Buff buffDodge = buffController.buffList.Find(x => x.baseData.stat == StatType.Dodge);
        if (buffDodge != null)
        {
            //Debug.Log(heroData.heroName + " / "+ buffDodge.id + " / " + buffDodge.baseData.triggerProbability);
            double prbability = buffDodge.GetPower(buffDodge, buffDodge.baseData.triggerProbability);
            //Debug.Log("회피성공률 : " + prbability);
            if (prbability >= UnityEngine.Random.Range(1, 10001))
            {
                //Debug.Log("성공");
                if (buffDodge.baseData.effect == "Miss")
                {
                    finalAttackPower = 0;
                    if (onHit != null)
                        onHit(finalAttackPower, "Miss");
                }
                else
                {
                    //얻어 맞았다는 콜백 호출
                    if (onHit != null)
                        onHit(finalAttackPower, tag);
                }
            }
        }


        //흡혈, 데미지 반사는 공격자가 살아있을 때만
        if (finalAttackPower > 1 && attacker && attacker.master && !attacker.master.isDie)
        {
            if (!attacker.master.isDie)
            {
                //공격자는 준 피해에 비례해서 흡혈 함
                Stat statLifeDrain = attacker.master.stats.GetParam(StatType.LifeDrainRate);
                if (statLifeDrain != null)
                {
                    double lifeDrain = statLifeDrain.value * 0.0001f;// attacker.master.lifeDrainRate;

                    if (lifeDrain > 0f)
                        attacker.master.LifeDrain(finalAttackPower * lifeDrain);
                }

                //얻어 맞은 애는 공격자 한테 데미지 반사 시킴
                Stat statReflect = attacker.master.stats.GetParam(StatType.ReflectDamageRate);
                if (statReflect != null)
                {
                    double reflect = statReflect.value * 0.0001f;// finalAttackPower * reflectDamageRate;
                    if (reflect > 0f)
                        attacker.master.ReflectDamage(this, reflect);
                }
            }            

            //공격자의 OnHit 설정되어 있는 버프 trigger 발동
            for (int a = 0; a < attacker.master.buffController.buffList.Count; a++)
            {
                Buff buff = attacker.master.buffController.buffList[a];
                if (buff.baseData.trigger != "OnHit")
                    continue;

                if (buff.triggerProbability < UnityEngine.Random.Range(1, 10001))
                    continue;

                BattleUnit target = null;
                if (buff.baseData.triggerTarget == "SkillTarget")
                    target = this;
                else if (buff.baseData.triggerTarget == "BuffTarget")
                    target = attacker.master;
                
                if(target && !target.isDie)
                    target.buffController.AttachBuff(attacker.master, buff.baseData.triggerBuff, 1, buff);
            }

            //뚜까맞은애의 OnTakeDamage 설정되어 있는 버프 trigger 발동
            for (int a = 0; a < buffController.buffList.Count; a++)
            {
                Buff buff = buffController.buffList[a];
                if (buff.baseData.trigger != "OnTakeDamage")
                    continue;

                if (buff.triggerProbability < UnityEngine.Random.Range(1, 10001))
                    continue;

                BattleUnit target = null;
                if (buff.baseData.triggerTarget == "SkillTarget")
                    target = attacker.master;
                else if (buff.baseData.triggerTarget == "BuffTarget")
                    target = this;

                if (target && !target.isDie)
                    target.buffController.AttachBuff(this, buff.baseData.triggerBuff, 1, buff);
            }

            if(skill && skill.skillData != null && skill.skillData.rangeType == SkillBase.RangeType.Range)
            {
                //뚜까맞은애의 OnTakeDamage 설정되어 있는 버프 trigger 발동
                for (int a = 0; a < attacker.master.buffController.buffList.Count; a++)
                {
                    Buff buff = attacker.master.buffController.buffList[a];
                    if (buff.baseData.trigger != "OnHitRange")
                        continue;

                    if (buff.triggerProbability < UnityEngine.Random.Range(1, 10001))
                        continue;

                    BattleUnit target = null;
                    if (buff.baseData.triggerTarget == "SkillTarget")
                        target = this;
                    else if (buff.baseData.triggerTarget == "BuffTarget")
                        target = attacker.master;

                    if (target && !target.isDie)
                        target.buffController.AttachBuff(attacker.master, buff.baseData.triggerBuff, 1, buff);
                }
            }

        }



        //Todo: 방어력 등에 의한 실제 받아야 할 피해 계산


        //데미지가 사망에 이르는 정도인지 체크. 공격자의 OnKill 설정되어 있는 버프 trigger 발동
        if(finalAttackPower >= maxHP)
        {
            for (int a = 0; a < attacker.master.buffController.buffList.Count; a++)
            {
                Buff buff = attacker.master.buffController.buffList[a];
                if (buff.baseData.trigger != "OnInstantKill")
                    continue;

                if (buff.triggerProbability < UnityEngine.Random.Range(1, 10001))
                    continue;
                                

                if (buff.baseData.triggerTarget == "OwnerTeam")
                {
                    CustomList<BattleHero> targetList = attacker.master.team == Team.Red ? battleGroup.redTeamList : battleGroup.blueTeamList;
                    for(int b = 0; b < targetList.Count; b++)
                    {
                        BattleUnit target = targetList[b];

                        if (target && !target.isDie)
                            target.buffController.AttachBuff(attacker.master, buff.baseData.triggerBuff, 1, buff);
                    }
                }
                else
                {
                    BattleUnit target = null;
                    if (buff.baseData.triggerTarget == "SkillTarget")
                        target = this;
                    else if (buff.baseData.triggerTarget == "BuffTarget")
                        target = attacker.master;

                    target = attacker.master;

                    if (target && !target.isDie)
                        target.buffController.AttachBuff(attacker.master, buff.baseData.triggerBuff, 1, buff);
                }
                    
            }
        }
        if(curHP < finalAttackPower)
        {
            for (int a = 0; a < attacker.master.buffController.buffList.Count; a++)
            {
                Buff buff = attacker.master.buffController.buffList[a];
                if (buff.baseData.trigger != "OnKill")
                    continue;

                if (buff.triggerProbability < UnityEngine.Random.Range(1, 10001))
                    continue;

                BattleUnit target = null;
                if (buff.baseData.triggerTarget == "SkillTarget")
                    target = this;
                else if (buff.baseData.triggerTarget == "BuffTarget")
                    target = attacker.master;
                else if (buff.baseData.triggerTarget == "OwnerTeam")
                {
                    CustomList<BattleHero> targetList = attacker.master.team == Team.Red ? battleGroup.redTeamList : battleGroup.blueTeamList;
                    for (int b = 0; b < targetList.Count; b++)
                    {
                        BattleUnit _target = targetList[b];

                        if (_target && !_target.isDie)
                            _target.buffController.AttachBuff(attacker.master, buff.baseData.triggerBuff, 1, buff);
                    }
                }

                if(target != null)
                {
                    if (target && !target.isDie)
                        target.buffController.AttachBuff(attacker.master, buff.baseData.triggerBuff, 1, buff);
                }
               

                //Debug.Log("공격력 : " + attacker.master.stats.GetValueOf(StatType.AttackPower) + " / 방어력 : " + attacker.master.stats.GetValueOf(StatType.DefensePower));
            }
        }

       
       

        //체력 감소
        curHP -= finalAttackPower;
        if (damageType == SkillBase.DamageType.Physical || damageType == SkillBase.DamageType.Magical)
            attacker.master.cumulativeDamage += finalAttackPower;

        return true;
    }

    public void ReflectDamage(BattleUnit attacker, double damage)
    {
        //얻어 맞았다는 콜백 호출
        if (onHit != null)
            onHit(damage, "Reflect");
        
        //체력 감소
        curHP -= damage;        
    }



    public void LifeDrain(double drainAmount)
    {
        //Debug.Log("Damage. Attacker : " + attacker + ", finalAttackPower : " + finalAttackPower);

        //얻어 맞았다는 콜백 호출
        if (onHit != null)
            onHit(drainAmount, "Drain");

        //체력 증가
        curHP += drainAmount;
    }

    public Vector3 GetClosestPoint(Vector3 origin)
    {
        if (renderer)
            return renderer.bounds.ClosestPoint(origin);
        else
            return transform.position;
    }

    public float GetDistanceFrom(BattleUnit unit)
    {
        float dist = 0f;

        Vector2 posA = collider? collider.transform.position: transform.position;
        Vector2 posB = unit.collider ? unit.collider.transform.position : unit.transform.position;

        float radiusA = collider ? collider.radius * collider.transform.lossyScale.x : 0f;
        float radiusB = unit.collider ? unit.collider.radius * unit.collider.transform.lossyScale.x : 0f;

        //최종 거리는 중점사이의 거리에서 서로의 몸 두께를 뺀 값
        dist = Vector2.Distance(posA, posB) - radiusA - radiusB;

        //겹쳐져 있는 경우 마이너스가 나옴
        if (dist < 0f)
            dist = 0f;

        return dist;
    }
}




//---테스트 용-----------------------------------------------------------------------------------------------------------------------------------
public delegate void UnitDelegate(object unit);

public interface IUnitBaseDelegate
{
    event UnitDelegate onDie;
    
    UnitBaseParam param { get; set; }
    
}

public class UnitBaseParam
{
    public int hp { get; set; }
    public int attackPower { get; set; }
}

