using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using Spine.Unity;
using B83.ExpressionParser;
using System.Reflection;

public class SkillBase : MonoBehaviour
{
    #region delegate들
    public delegate void SkillDelegate(SkillBase skill);
    public delegate void SkillEventDelegate(SkillBase skill, SkillEvent skillEvent);

    public SkillDelegate onExcuteSkill;
    public SkillEventDelegate onTriggerEvent;
    public SkillDelegate onStart;
    public SkillDelegate onFinish;
    #endregion

    #region enum모음
    /// <summary> 액티브, 패시브 </summary>
    public enum ActiveType
    {
        NotDefined,
        Active,
        Passive
    }

    /// <summary> 데미지 타입. 물리/마법/고정 </summary>
    public enum DamageType
    {
        NotDefined,
        Pure,
        Physical,
        Magical
    }

    /// <summary> 거리 타입. 근거리/원거리 </summary>
    public enum RangeType
    {
        NotDefined,
        Melee,
        Range
    }

    /// <summary> 거리 타입. 근거리/원거리 </summary>
    public enum ElementalType
    {
        None,
        Fire,
        Water,
        Earth,
        Light,
        Dark
    }

    /// <summary> 자동 발동 트리거 </summary>
    public enum TriggerType
    {
        None,
        OnHit,
        OnKill,
        OnDie        
    }

    public enum TargetFilter
    {
        None,
        Self,
        MySummon,
        All,
        AllExcludeSummons,
        AllSummons,
        FriendlyAll,
        FriendlyExcludeSummons,
        FriendlySummons,
        HostileAll,
        HostileExcludeSummons,
        HostileSummons,
    }

    /// <summary> 시전 가능 여부 체크를 위해 쳐다보는 대상. 1개체 </summary>
    public enum CastTargetType
    {
        None,
        Self,
        ProjectileTarget,
        NearestEnemy,   //따로 설정된 최소, 최대 거리를 봄
        FarthestEnemy,  //따로 설정된 최소, 최대 거리를 봄
        LowestHPEnemy,
        HighestHPEnemy,
        NearestAlly,
        FarthestAlly,
        LowestHPAlly,
        HighestHPAlly,
        FrontMostHero,
        HighestAttackPowerEnemy,
        AirEnemy,
        StartPosition,
    }

    /// <summary> 효과 적용을 하기 위한 대상을 검색하는 방식 </summary>
    public enum CollectTargetType
    {
        /// <summary> 대상을 취합하지 않음 </summary>
        None,

        /// <summary> 시전 대상과 같음 </summary>
        Target,

        /// <summary> 시전자 주변 </summary>
        CasterAround,

        /// <summary> 대상 주변 </summary>
        TargetAround,

        /// <summary> 특정 지점. 미구현 </summary>
        Ground,

        /// <summary> 특정 지점 사이 </summary>
        Trail,

        /// <summary> 모든 유닛 </summary>
        Global
    }

    /// <summary> 타격 시 대상에게 적용하는 물리적인 효과. 밀어내기, 끌어당기기, 넉백,  </summary>
    public enum ForceType
    {
        /// <summary> 없음 </summary>
        None,

        /// <summary> 흔들림. 위치 변화 없음 </summary>
        Shake,

        /// <summary> 밀기 </summary>
        Push,

        /// <summary> 당기기 </summary>
        Pull,

        /// <summary> 넉백 </summary>
        Knockback,

        /// <summary> 대쉬 </summary>
        Dash,

        Knockback2,

        /// <summary> 띄우기 </summary>
        Airborne,

        /// <summary> 내려찍기 </summary>
        FallDown,

    }

    #endregion


    public SkillSetting skillSetting { get; set; }
    SkillData _skillData;
    public SkillData skillData
    {
        get { return _skillData; }
        set
        {
            _skillData = value;

            switch (value.castTargetType)
            {
                case CastTargetType.None:
                    castTargetCollector = new SkillCTCollectorNone();
                    break;
                case CastTargetType.Self:
                    castTargetCollector = new SkillCTCollectorSelf();
                    break;
                case CastTargetType.ProjectileTarget:
                    castTargetCollector = new SkillCTCollectorProjectileTarget();
                    break;
                case CastTargetType.NearestEnemy:
                    castTargetCollector = new SkillCTCollectorNearestEnemy();
                    break;
                case CastTargetType.FarthestEnemy:
                    castTargetCollector = new SkillCTCollectorFarthestEnemy();
                    break;
                case CastTargetType.NearestAlly:
                    castTargetCollector = new SkillCTCollectorNearestAlly();
                    break;
                case CastTargetType.LowestHPAlly:
                    castTargetCollector = new SkillCTCollectorLowestHPAlly();
                    break;
                case CastTargetType.FrontMostHero:
                    castTargetCollector = new SkillCTFrontMostHero();
                    break;
                case CastTargetType.HighestAttackPowerEnemy:
                    castTargetCollector = new SkillCTHighestAttackPowerEnemy();
                    break;
                case CastTargetType.AirEnemy:
                    castTargetCollector = new SkillCTCollectorAirTarget();
                    break;
            }

            switch (value.collectTargetType)
            {
                case CollectTargetType.None:
                    executeTargetCollector = new SkillETCollectorNone();
                    break;
                case CollectTargetType.Target:
                    executeTargetCollector = new SkillETCollectorTarget();
                    break;
                case CollectTargetType.CasterAround:
                    executeTargetCollector = new SkillETCollectorCasterAround();
                    break;
                case CollectTargetType.Global:
                    executeTargetCollector = new SkillETCollectorGlobal();
                    break;
                case CollectTargetType.TargetAround:
                    executeTargetCollector = new SkillETCollectorTargetAround();
                    break;
                case CollectTargetType.Trail:
                    executeTargetCollector = new SkillETCollectorTrail();
                    break;
            }
        }
    }

    public BattleUnit owner { get; set; }

    public CoolTimeController coolTimeController { get; protected set; }

    List<BattleHero> _targetList = new List<BattleHero>();
    public List<BattleHero> targetList { get { return _targetList; } set { _targetList = value; } }

    [NonSerialized]
    public bool isInitialized = false;

    bool canTargeting = true;

    bool _isCoolTime = false;
    public bool isCoolTime
    {
        get
        {
            if (!coolTimeController)
            {
                Debug.LogWarning("cannot find cooltimeController : " + this.skillData.id);
                return true;
            }

            return _isCoolTime;
        }
        set { _isCoolTime = value; }
    }

    public float currentCoolTime
    {
        get
        {
            if (!isCoolTime)
                return 0f;

            float coolTime1 = 0f;
            float coolTime2 = 0f;
            //쿨타임 컨트롤러에 돌고 있는지 체크. 돌고 있으면 true
            for (int i = 0; i < coolTimeController.coolTimeList.Count; i++)
            {
                string s = coolTimeController.coolTimeList[i].id;
                if (s == skillData.coolTime1ID)
                    coolTime1 = coolTimeController.coolTimeList[i].coolTime;
                if (s == skillData.coolTime2ID)
                    coolTime2 = coolTimeController.coolTimeList[i].coolTime;
            }

            return Mathf.Max(coolTime1, coolTime2);
        }
    }

    

    ExpressionParser parser = new ExpressionParser();

    bool _canExecute = false;
    public bool canCastSkill
    {
        get { return _canExecute; }
        protected set { _canExecute = value; }
    }

    public ICastTargetCollector castTargetCollector;
    public IExecuteTargetCollector executeTargetCollector;
    public Dictionary<string, List<ISkillEffect>> skillEffectDic = new Dictionary<string, List<ISkillEffect>>();

    //################################################################################################################################
    virtual public void Init(BattleUnit owner, SkillSetting skillSetting)
    {
        if (!owner)
        {
            Debug.Log(gameObject.name + " : not defined owner. ");
            isInitialized = true;
            enabled = false;
            return;
        }

        if (!skillSetting)
        {
            Debug.Log(gameObject.name + " : not defined skillsetting");
            isInitialized = true;
            enabled = false;
            return;
        }

        this.owner = owner;

        //if (owner)
        coolTimeController = owner.coolTimeController;

        this.skillSetting = skillSetting;

        //if (skillSetting)
        skillData = GameDataManager.skillDataDic[skillSetting.skillID];
        
        if (skillData == null)
        {
            Debug.Log(gameObject.name + " : Not found skill data. Invalid id. " + skillSetting.skillID);
            isInitialized = true;
            enabled = false;
            return;
        }



        //스킬 효과 등록
        for (int i = 0; i < skillSetting.skillEventList.Count; i++)
        {
            SkillEvent skillEvent = skillSetting.skillEventList[i];
            
            //해당 이벤트 이름으로 등록 안 되어 있으면 추가
            if (!skillEffectDic.ContainsKey(skillEvent.eventName))
            {
                List<ISkillEffect> list = new List<ISkillEffect>();
                skillEffectDic.Add(skillEvent.eventName, list);
            }

            List<ISkillEffect> skillEventList = skillEffectDic[skillEvent.eventName];

            //같은 이벤트 이름인 애들 전부 같은 리스트에 묶어서 등록
            switch (skillSetting.skillEventList[i].eventType)
            {                    
                //case SkillEvent.SkillEventType.ExecuteSkill:
                //    skillEventList.Add(new SkillEffectExecuteSkill(this, skillSetting.skillEventList[i]));
                //    break;
                case SkillEvent.SkillEventType.FireProjectile:
                    skillEventList.Add(new SkillEffectFireProjectile(this, skillSetting.skillEventList[i]));
                    break;
                case SkillEvent.SkillEventType.ShowParticle:
                    skillEventList.Add(new SkillEffectShowParticle(this, skillSetting.skillEventList[i]));
                    break;
                case SkillEvent.SkillEventType.MeleeHit:
                    skillEventList.Add(new SkillEffectMeleeHit(this, skillSetting.skillEventList[i]));
                    break;
                case SkillEvent.SkillEventType.Summon:
                    skillEventList.Add(new SkillEffectSummon(this, skillSetting.skillEventList[i]));
                    break;
            }

            //스킬 발동 타입의 이벤트만 예외 처리. 같은 이벤트 이름으로 등록된 애들끼리 묶어서 초기화
            List<SkillEvent> executeSkillEvents = skillSetting.skillEventList.FindAll(x => x.eventName == skillEvent.eventName && x.eventType == SkillEvent.SkillEventType.ExecuteSkill);
            if (executeSkillEvents != null)
                skillEventList.Add(new SkillEffectExecuteSkill(this, executeSkillEvents));
        }
        
        if (skillData.moveSpeed != 0f)
        {
            HeroMoveBehaviorMoveTowards m = new HeroMoveBehaviorMoveTowards();
            m.skill = this;
            m.owner = owner as BattleHero;
            m.castTarget = castTarget;
            moveBehavior = m;
        }

        isInitialized = true;
    }

    public IMoveable moveBehavior = null;

    protected virtual void Start() { }


    /// <summary> 스킬 시전 대상. 주시 대상. 효과 적용을 할 대상을 선택하는 기준 중 하나 (대부분의 스킬이 이 대상을 기준으로 함) </summary>
    public BattleHero castTarget { get; set; }
    public GameObject targetObject;
    public bool IsCastTargetInSkillRange()
    {
        bool isInRange = false;
        if (castTargetCollector != null)
            isInRange = castTargetCollector.IsCastTargetInSkillRange();

        return isInRange;
    }

    public BattleHero CollectCastTarget(SkillBase skill)
    {        
        if (castTargetCollector != null)
        {
            castTarget = castTargetCollector.CollectTarget(skill);
            if(castTarget != null)
            {
                if (castTarget.notTargeting)
                    return null;

                targetObject = castTarget.gameObject;
            }
                
        }
           

        //if (skillData.id == "Skill_Gargoyle_Passive_ResistMagicalDamage")
        //    Debug.Log(castTarget);

        return castTarget;
    }

    /// <summary> 시전 가능 여부 체크 </summary>
    virtual public void CheckCastCondition()
    {
        if (!isInitialized || skillData == null || !owner)
        {
            canCastSkill = false;
            return;
        }

        ////현재 쿨타임 중이면 발동 불가
        //if (isCoolTime)
        //{
        //    canCastSkill = false;
        //    return;
        //}

        //if (castTargetCollector != null)
        //    castTarget = castTargetCollector.CollectTarget(this);

        castTarget = CollectCastTarget(this);
        

        if (castTarget && castTarget != owner)
        {
            
            float distance = castTarget.GetDistanceFrom(owner);
            if (distance > skillData.maxDistance || (skillData.minDistance > 0f && distance < skillData.minDistance))
            {
                canCastSkill = false;
                return;
            }

            if (!skillData.canCastToAir  && castTarget.skeletonAnimation.transform.localPosition.y > 3f)
            {
                canCastSkill = false;
                return;
            }
        }

        canCastSkill = IsValidCondition(castTarget, skillData.castCondition);
    }
    
    /// <summary> 스킬 효과를 적용시킬 대상을 결정. 타겟은 발사체를 날리거나, 데미지, 버프를 줄 때 필요 </summary>
    public void CollectTargets()
    {
        if(executeTargetCollector != null)
        {
            targetList = executeTargetCollector.CollectTargets(this);
            //최대 효과 적용 개체 수 적용
            if (targetList != null && targetList.Count > skillData.maxTargetCount)
                targetList = targetList.GetRange(0, skillData.maxTargetCount);
        }
            
                
        return;

        //조건 안 맞는애 거르기
        //targetList = targetList.FindAll(x => IsValidCondition(x));

        targetList.Sort(delegate (BattleHero a, BattleHero b)
        {
            if (a.GetDistanceFrom(owner) < b.GetDistanceFrom(owner))
                return -1;
            else if (a.GetDistanceFrom(owner) > b.GetDistanceFrom(owner))
                return 1;
            else
                return 0;
        });

        //최대 효과 적용 개체 수 적용
        if (targetList != null && targetList.Count > skillData.maxTargetCount)
            targetList = targetList.GetRange(0, skillData.maxTargetCount);
    }        

    /// <summary> 스킬 실행. 효과의 발동을 의미하진 않는다. 효과 발생 시점과 발생하는 효과에 대한 정의는 스킬에 설정된 SkillEvent에 의해 결정된다. </summary>
    public virtual void Execute()
    {

        if (skillEffectDic.ContainsKey("OnStart"))
        {
            List<ISkillEffect> skillEventList = skillEffectDic["OnStart"];
            for (int i = 0; i < skillEventList.Count; i++)
            {
                skillEventList[i].TriggerEffect();
            }
        }

        //if (moveBehavior != null && owner is BattleHero)
        //{
        //    //BattleHero h = owner as BattleHero;
        //    //h.currentMoveBehavior = moveBehavior;
        //}

        //콜백 날림
        if (onExcuteSkill != null)
            onExcuteSkill(this);        
    }

    public bool IsValidCondition(BattleUnit target, string condition)
    {
        if (!target || (!skillData.canCastToDeadUnit && target.isDie))
            return false;

        if (skillData == null)
            return false;
        
        if (string.IsNullOrEmpty(condition))
            return true;


        //이런식으로 값이 들어있음
        //unit.hp<=unit.maxHP*0.5
        
        //비교연산자
        string op = GetOperator(condition);
        if (string.IsNullOrEmpty(op))
        {
            Debug.LogWarning("Invalid skill condition syntax");
            return true;
        }

        string[] ops = new string[] { op };
        //ss[0], ss[2]은 유닛능력치가 정의되어야 함, ss[1]은 비교연산자
        string[] ss = condition.Split(ops, StringSplitOptions.RemoveEmptyEntries);
        if(ss.Length != 2)
        {
            Debug.LogWarning("Invalid skill condition syntex - " + condition);
            return true;
        }
        BattleUnit unitA, unitB;
        
        if (ss[0].Contains("target"))
            unitA = target;
        else if (ss[0].Contains("self") || ss[1].Contains("owner"))
            unitA = owner;
        else if (ss[0].Contains("master"))
            unitA = owner.master;
        else
            unitA = null;

        if (ss[1].Contains("target"))
            unitB = target;
        else if (ss[1].Contains("self") || ss[1].Contains("owner"))
            unitB = owner;
        else if (ss[1].Contains("master"))
            unitB = owner.master;
        else
            unitB = null;

        if(unitA == null || unitB == null)
        {
            //Debug.LogWarning("unit is null");
        }
        
        double unitAParam = GetParamValue(unitA, ss[0]);
        double unitBParam = GetParamValue(unitB, ss[1]);
        
        bool isValid = true;
        switch (op)
        {
            case "<":
                isValid = unitAParam < unitBParam;
                break;
            case "<=":
                isValid = unitAParam <= unitBParam;
                break;
            case ">":
                isValid = unitAParam > unitBParam;
                break;
            case ">=":
                isValid = unitAParam >= unitBParam;
                break;
            case "==":
                isValid = unitAParam == unitBParam;
                break;
            case "!=":
                isValid = unitAParam != unitBParam;
                break;
        }

        //Debug.Log(unitAParam + "(" + ss[0] + ")" + op + unitBParam + "(" + ss[2] + ") : " + isValid);

        return isValid;
    }

    /// <summary> 스킬 공격력 받아오기 </summary>
    public double GetPower(BattleUnit target)
    {
        //owner.attackPower * 1 - target.defensePower 이런 형태로 되어 있음
        string paramString = skillData.power;

        if (string.IsNullOrEmpty(paramString))
        {
            Debug.LogWarning("skill power is not defined");
            //if (owner)
            //    return owner.attackPower;
            
            return 0f;
        }
        
        //공백 지우기
        paramString.Replace(" ", string.Empty);
        
        //수식을 제외한 값을 문자열로 따로 저장 후 파싱. (owner.attackPower 같은 것들 선계산 하기 위함)
        //string[] seperator = { "+", "-", "*", "/", "(", ")", "<", ">", "=" };
        //string[] ss = paramString.Split(seperator, StringSplitOptions.None);

        //지수 제외 되서 regex 사용
        //@"([*()\^\/]|(?<!E)[\+\-])"
        string format = @"([<>=*()\^\/]|(?<!E)[\+\-])";
        string[] ss = System.Text.RegularExpressions.Regex.Split(paramString, format, System.Text.RegularExpressions.RegexOptions.ExplicitCapture);

        for (int i = 0; i < ss.Length; i++)
        {
            //if (owner.heroData.baseData.id == "Berserker_01_Hero")
            //{
            //    Debug.Log(ss[i]);
            //}

            if (ss[i].Contains("target"))
            {
                paramString = paramString.Replace(ss[i], GetParamValue(target, ss[i]).ToString());
            }                
            else if (ss[i].Contains("owner") || ss[i].Contains("self"))
            {
                paramString = paramString.Replace(ss[i], GetParamValue(owner, ss[i]).ToString());
            }
            else if (ss[i].Contains("master"))
            {
                paramString = paramString.Replace(ss[i], GetParamValue(owner.master, ss[i]).ToString());
            }
            else if (ss[i].Contains("animDuration") || ss[i].Contains("animationDuration"))
            {
                float duration = 1f;
                if(skillSetting.animation != null)
                    duration = skillSetting.animation.Duration;

                paramString = paramString.Replace(ss[i], duration.ToString());
            }
            if(ss[i].Contains("castTarget"))
            {
                paramString = paramString.Replace(ss[i], GetParamValue(castTarget, ss[i]).ToString());
            }
            
        }

        double finalValue = parser.Evaluate(paramString);

        //Debug.Log(skillData.name + " attackPower: " + paramString + " = " + finalValue);

        return finalValue;
    }
    
    int GetBuffStack(BuffController buffController)
    {
        int value = 0;

        if (string.IsNullOrEmpty(skillData.buffID))
            return value;
       
       
        
        for (int i = 0; i < buffController.buffList.Count; i++)
        {
            if(buffController.buffList[i].baseData.id == skillData.buffID)
            {
                value = buffController.buffList[i].stack;
                //Debug.Log("버프 스텍 " + value);
            }
        }

        return value;
    }

    double GetParamValue(BattleUnit unit, string paramString)
    {
        //selft, target이런거 제외            
        //self. target. party. global. (타입 구분은 미정)
        paramString = paramString.Replace("owner.", string.Empty);
        paramString = paramString.Replace("self.", string.Empty);
        paramString = paramString.Replace("target.", string.Empty);
        paramString = paramString.Replace("master.", string.Empty);
        paramString = paramString.Replace("castTarget.", string.Empty);
        paramString = paramString.Trim();

        if(paramString.Contains("buffStack"))
        {
            
            paramString = paramString.Replace("buffStack", GetBuffStack(unit.buffController).ToString());
            //Debug.Log("공식 : " + paramString);
        }

        Expression exp = parser.EvaluateExpression(paramString);

        List<string> keys = exp.Parameters.Keys.ToList();
        for (int i = 0; i < keys.Count; i++)
        {
            string propertyName = keys[i];
            
            object obj = Enum.Parse(typeof(StatType), propertyName, true);
            if(obj == null)
            {
                Debug.LogWarning(skillData.id + " - Cannot find stat : " + propertyName);
                continue;
            }

            StatType statType = (StatType)obj;
            Stat stat = unit.stats.GetParam(statType);
            if(stat == null)
                Debug.LogWarning(skillData.id + " - Unit has not stat : " + propertyName);
            else
                exp.Parameters[keys[i]].Value = stat.value;

            continue;

            //######### 백업 ##############
            //Type type = unit.GetType().BaseType;

            ////패러미터 못 찾을 경우 기본 값은 1. (주로 곱셈 연산이 많을 것 같아서? 해보고 이상하면 0으로..)
            //float value = 1f;

            //PropertyInfo p = type.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            //if(p == null)
            //    Debug.LogWarning("Cannot find property : " + unit + propertyName);
            //else
            //    float.TryParse(p.GetValue(unit, null).ToString(), out value);

            //exp.Parameters[keys[i]].Value = value;
        }

        if (paramString.Contains("E"))
            Debug.Log(paramString);

        return exp.Value;
    }



    string GetOperator(string formula)
    {
        if (formula.Contains("<="))
            return "<=";
        else if (formula.Contains("<"))
            return "<";
        else if (formula.Contains(">="))
            return ">=";
        else if (formula.Contains(">"))
            return ">";
        else if (formula.Contains("=="))
            return "==";
        else if (formula.Contains("!="))
            return "!=";
        else
            return "";
    }
}