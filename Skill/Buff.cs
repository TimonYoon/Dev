using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Reflection;
using B83.ExpressionParser;

public interface IBuffBehavior
{
    Buff buff { get; set; }
    void ApplyEffect();
}

abstract public class BuffBehaviorBase :IBuffBehavior
{
    public Buff buff { get; set; }

    public BuffBehaviorBase(Buff buff)
    {
        this.buff = buff;
    }

    abstract public void ApplyEffect();
}

public class BuffBehaviorDoNothing : BuffBehaviorBase
{
    public BuffBehaviorDoNothing(Buff buff) : base(buff)
    {
        this.buff = buff;
    }

    override public void ApplyEffect()
    {
    }
}

public class BuffBehaviorDot : BuffBehaviorBase
{
    public BuffBehaviorDot(Buff buff) : base (buff)
    {
        this.buff = buff;
    }

    override public void ApplyEffect()
    {
        double damage = buff.GetPower(buff, buff.baseData.power);
        //데미지 적용. 일단 마법데미지로 취급
        buff.target.Damage(buff.owner, damage, SkillBase.DamageType.Magical, null, "Dot");
        
        if (buff.effectObject)
        {
            Animator animator = buff.effectObject.GetComponentInChildren<Animator>();
            if (animator)
            {
                animator.SetTrigger("hit");
            }
        }
    }
}



public class BuffAttachBehaviorModifyStat : BuffBehaviorBase
{
    public BuffAttachBehaviorModifyStat(Buff buff) : base (buff)
    {
        this.buff = buff;
    }
    
    override public void ApplyEffect()
    {
        //HeroData hData = null;
        //hData.stats.AddStatModifier(buff.baseData.stat, buff.statModifier);

        if (buff.statModifier != null)
            buff.target.stats.AddStatModifier(buff.baseData.stat, buff.statModifier);

        if (buff.statModifier2 != null)
            buff.target.stats.AddStatModifier(buff.baseData.stat2, buff.statModifier2);
    }
}


public class BuffDetachBehaviorModifyStat : BuffBehaviorBase
{
    public BuffDetachBehaviorModifyStat(Buff buff) : base(buff)
    {
        this.buff = buff;
    }

    override public void ApplyEffect()
    {
        //return;
        if (buff.statModifier != null)
        {
            buff.statModifier.stack = 0;
            //buff.target.stats.RemoveModifier(buff.baseData.stat, buff.statModifier);
        }
            

        if (buff.statModifier2 != null)
        {
            buff.statModifier2.stack = 0;
            //buff.target.stats.RemoveModifier(buff.baseData.stat2, buff.statModifier2);
        }
            
    }
}

/// <summary> 버프 해제 될 때 버프에 정의된 스킬 사용 (단 해당 스킬이 영웅에 있어야한다.) </summary>
public class BuffDetachSkillBehavior : BuffBehaviorBase
{
    public BuffDetachSkillBehavior(Buff buff) : base(buff)
    {
        this.buff = buff;
    }

    public override void ApplyEffect()
    {
        if(buff.target  == null)
        {
            //Debug.Log(buff.baseData.id + "buff 타겟 없음");
            return;
        }

        if(!buff.target.isDie)
        {
            List<SkillBase> skillList = buff.owner.skillList.FindAll(x => x.skillData.id == buff.baseData.skillID);

            for (int i = 0; i < skillList.Count; i++)
            {
                skillList[i].owner = buff.owner;
                skillList[i].castTarget = buff.target as BattleHero;
                skillList[i].Execute();
            }
        }
    }

}

[System.Serializable]
public class Buff
{
    
    /// <summary> 버프를 적용 시킨 유닛 </summary>
    public BattleUnit owner;
    
    /// <summary> 버프가 적용된 유닛 </summary>
    public BattleUnit target;
    
    public string id
    {
        get
        {
            if (baseData == null)
                return string.Empty;

            return baseData.id;
        }
    }

    
    public float startTime;

    public IBuffBehavior attachBehavior;

    public IBuffBehavior detachBehavior;

    public IBuffBehavior tickBehavior;

    public IBuffBehavior detachSkillBehavior;
    //public float duration;

    /// <summary> 실제론 안 씀. 디버깅 용 </summary>

    public float remainTime;

    int _stack;
    public int stack
    {
        get { return _stack; }
        set
        {
            _stack = value;

            if (statModifier != null)
                statModifier.stack = value;

            if (statModifier2 != null)
                statModifier2.stack = value;
        }
    }

    public int level = 1;

    /// <summary> 현재 활성화 여부. </summary>
    public bool isActive = false;

    public StatModifier statModifier;

    public StatModifier statModifier2;

    public GameObject effectObject;
    

    public BuffBaseData baseData
    {
        get; set;
        //get
        //{
        //    if (string.IsNullOrEmpty(id) || !GameDataManager.buffBaseDataDic.ContainsKey(id))
        //        return null;

        //    return GameDataManager.buffBaseDataDic[id];
        //}
    }
    
    public float triggerProbability { get; set; }


    public float triggerCooltime { get; set; }

    public Coroutine coroutineDuration = null;
    virtual public void Init(BattleUnit owner, BattleUnit target, string buffID, int stack = 1, Buff refBuff = null)
    {
        baseData = GameDataManager.buffBaseDataDic[buffID];
        this.owner = owner;
        this.target = target;


        if (refBuff != null)
            level = refBuff.stack;

        if (!string.IsNullOrEmpty(baseData.trigger))
        {
            triggerProbability = (float)GetPower(this, baseData.triggerProbability);
            triggerCooltime = (float)GetPower(this, baseData.triggerCooltime);
        }

        //if(!string.IsNullOrEmpty(baseData.trigger))
        

        startTime = Time.time;

        isActive = true;

        //도트 데미지 버프 설정
        if (baseData.interval > 0 && !string.IsNullOrEmpty(baseData.power))
            tickBehavior = new BuffBehaviorDot(this);


        //도발 효과 있을 경우 적용
        if (baseData.provoke && target.provokeEnemy != owner)
            target.provokeEnemy = owner as BattleHero;



        //statModifier = null;
        //statModifier2 = null;

        //능력치 버프 설정
        if (baseData.stat != StatType.NotDefined)
        {
            float statValue = 0f;
            if(!string.IsNullOrEmpty(baseData.modifyValue))
                statValue = GetParamValue(target, baseData.modifyValue);

            if(statModifier == null)
                statModifier = new StatModifier(statValue);
                

            if (baseData.modifyType == "percent")
                statModifier.type = StatModifier.Type.BaseValuePercent;
            else
                statModifier.type = StatModifier.Type.BaseValueAdd;
                        
            //if(baseData.linkedStat != StatType.NotDefined)
            if(!string.IsNullOrEmpty(baseData.multiplyValue))
            {
                Stat stat = null;
                if (baseData.linkedStat != StatType.NotDefined)
                    stat = target.stats.CreateOrGetStat<ModifiableStat>(baseData.linkedStat);
                float multiplyValue = GetParamValue(target, baseData.multiplyValue);
                
                if (baseData.linkerType == "lostHPPercent")
                {
                    Stat curHP = target.stats.CreateOrGetStat<ModifiableStat>(StatType.CurHP);
                    Stat maxHP = target.stats.CreateOrGetStat<ModifiableStat>(StatType.MaxHP);
                    if (statModifier.linker == null)
                        statModifier.linker = new StatLinkerLostHP(stat, curHP, maxHP, multiplyValue);
                }
                else if (baseData.linkerType == "remainedHPPercent")
                {
                    Stat curHP = target.stats.CreateOrGetStat<ModifiableStat>(StatType.CurHP);
                    Stat maxHP = target.stats.CreateOrGetStat<ModifiableStat>(StatType.MaxHP);

                    if (statModifier.linker == null)
                        statModifier.linker = new StatLinkerRemainedHP(stat, curHP, maxHP, multiplyValue);
                }
                else if (baseData.linkerType == "additionalStatValue")
                {
                    if (statModifier.linker == null)
                        statModifier.linker = new StatLinkerAdditionalStatValue(stat, multiplyValue);
                }
                else if (baseData.linkerType == "frontmostHero")
                {
                    //Debug.Log(statModifier.linker);
                    if(statModifier.linker == null)
                        statModifier.linker = new StatLinkerFrontmostHero(stat, target.battleGroup, target, multiplyValue);
                }
                else if (baseData.linkerType == "rearmostHero")
                {
                    if (statModifier.linker == null)
                        statModifier.linker = new StatLinkerRearmostHero(stat, target.battleGroup, target, multiplyValue);
                }
                else if (baseData.linkerType == "buffStack")
                {
                    if (statModifier.linker == null)
                        statModifier.linker = new StatLinkerBuffStack(stat, multiplyValue, refBuff);
                }
                else if (baseData.linkerType == "enemyCount")
                {
                    if (statModifier.linker == null)
                        statModifier.linker = new StatLinkerEnemyCount(stat, multiplyValue, owner.battleGroup);
                }
                else if(baseData.linkerType == "friendlyDieCount")
                {
                    if (statModifier.linker == null)
                        statModifier.linker = new StatLinkerFriendlyDieCount(stat, multiplyValue, target, target.battleGroup);
                }
                else if(baseData.linkerType == "summonCount")
                {
                    if (statModifier.linker == null)
                        statModifier.linker = new StatLinkerSummonCount(stat, multiplyValue, target, target.battleGroup);
                }
                else if(baseData.linkerType == "aroundEnemyCount")
                {
                    if (statModifier.linker == null)
                        statModifier.linker = new StatLinkerAroundEnemyCount(stat, multiplyValue, target);
                }
                else
                {
                    if (statModifier.linker == null)
                        statModifier.linker = new StatLinker(stat, multiplyValue);

                }
            }

        }

        if (baseData.stat2 != StatType.NotDefined)
        {
            float statValue = 0f;
            if (!string.IsNullOrEmpty(baseData.modifyValue2))
                statValue = GetParamValue(target, baseData.modifyValue2);

            if(statModifier2 == null)
                statModifier2 = new StatModifier(statValue);

            if (baseData.modifyType2 == "percent")
                statModifier2.type = StatModifier.Type.BaseValuePercent;
            else
                statModifier2.type = StatModifier.Type.BaseValueAdd;

            //if (baseData.linkedStat2 != StatType.NotDefined)
            if (!string.IsNullOrEmpty(baseData.multiplyValue2))
            {
                Stat stat = null;
                if (baseData.linkedStat2 != StatType.NotDefined)
                    stat = target.stats.CreateOrGetStat<ModifiableStat>(baseData.linkedStat2);
                float multiplyValue = GetParamValue(target, baseData.multiplyValue2);

                if (baseData.linkerType2 == "lostHPPercent")
                {
                    Stat curHP = target.stats.CreateOrGetStat<ModifiableStat>(StatType.CurHP);
                    Stat maxHP = target.stats.CreateOrGetStat<ModifiableStat>(StatType.MaxHP);
                    if (statModifier2.linker == null)
                        statModifier2.linker = new StatLinkerLostHP(stat, curHP, maxHP, multiplyValue);
                }
                else if (baseData.linkerType2 == "remainedHPPercent")
                {
                    Stat curHP = target.stats.CreateOrGetStat<ModifiableStat>(StatType.CurHP);
                    Stat maxHP = target.stats.CreateOrGetStat<ModifiableStat>(StatType.MaxHP);
                    if (statModifier2.linker == null)
                        statModifier2.linker = new StatLinkerRemainedHP(stat, curHP, maxHP, multiplyValue);
                }
                else if (baseData.linkerType2 == "additionalStatValue")
                {
                    if (statModifier2.linker == null)
                        statModifier2.linker = new StatLinkerAdditionalStatValue(stat, multiplyValue);
                }
                else if (baseData.linkerType2 == "frontmostHero")
                {
                    if (statModifier2.linker == null)
                        statModifier2.linker = new StatLinkerFrontmostHero(stat, target.battleGroup, target, multiplyValue);
                }
                else if (baseData.linkerType2 == "rearmostHero")
                {
                    if (statModifier2.linker == null)
                        statModifier2.linker = new StatLinkerRearmostHero(stat, target.battleGroup, target, multiplyValue);
                }
                else if (baseData.linkerType2 == "buffStack")
                {
                    if (statModifier2.linker == null)
                        statModifier2.linker = new StatLinkerBuffStack(stat, multiplyValue, refBuff);
                }
                else if (baseData.linkerType2 == "enemyCount")
                {
                    if (statModifier2.linker == null)
                        statModifier2.linker = new StatLinkerEnemyCount(stat, multiplyValue, owner.battleGroup);
                }
                else if (baseData.linkerType2 == "friendlyDieCount")
                {
                    if (statModifier2.linker == null)
                        statModifier2.linker = new StatLinkerFriendlyDieCount(stat, multiplyValue, target, target.battleGroup);
                }
                else if (baseData.linkerType == "summonCount")
                {
                    if (statModifier.linker == null)
                        statModifier.linker = new StatLinkerSummonCount(stat, multiplyValue, target, target.battleGroup);
                }
                else if(baseData.linkerType == "aroundEnemyCount")
                {
                    if (statModifier.linker == null)
                        statModifier.linker = new StatLinkerAroundEnemyCount(stat, multiplyValue, target);
                }
                else
                {
                    if (statModifier2.linker == null)
                        statModifier2.linker = new StatLinker(stat, multiplyValue);
                }
            }
        }

        if (statModifier != null || statModifier2 != null)
        {
            attachBehavior = new BuffAttachBehaviorModifyStat(this);
            detachBehavior = new BuffDetachBehaviorModifyStat(this);
        }
        else
        {
            //attachBehavior = new BuffBehaviorDoNothing(this);
            //detachBehavior = new BuffBehaviorDoNothing(this);
        }
        
        if (!string.IsNullOrEmpty(baseData.skillID))
        {
            detachSkillBehavior = new BuffDetachSkillBehavior(this);
        }
        else
            detachSkillBehavior = null;

        //버프 스택
        this.stack = baseData.maxStackCount == 0 ? Mathf.Max(0, this.stack + stack) : Mathf.Clamp(this.stack + stack, 0, baseData.maxStackCount);
    }

    public void Reset()
    {
        stack = 0;
        owner = null;
        target = null;
        effectObject = null;

        isActive = false;
    }


    public double GetPower(Buff buff, string param)
    {
        //owner.attackPower * 1 - target.defensePower 이런 형태로 되어 있음
        string paramString = param;

        if (string.IsNullOrEmpty(paramString))
            return 0d;

        //식이 아니면 그냥 값을 그대로 파싱해서 그냥 뱉어냄
        double finalValue = 0d;
        if (double.TryParse(param, out finalValue))
            return finalValue;

        //공백 지우기
        paramString = paramString.Replace(" ", string.Empty);

        paramString = paramString.Replace("stack", buff.stack.ToString());

        paramString = paramString.Replace("level", buff.level.ToString());

        //수식을 제외한 값을 문자열로 따로 저장 후 파싱. (owner.attackPower 같은 것들 선계산 하기 위함)
        string[] seperator = { "+", "-", "*", "/", "(", ")", "<", ">", "=" };
        string[] ss = paramString.Split(seperator, System.StringSplitOptions.None);
        for (int i = 0; i < ss.Length; i++)
        {
            if (ss[i].Contains("target"))
            {
                paramString = paramString.Replace(ss[i], GetParamValue(buff.target, ss[i]).ToString());
            }
            else if (ss[i].Contains("owner") || ss[i].Contains("self"))
            {
                paramString = paramString.Replace(ss[i], GetParamValue(buff.owner, ss[i]).ToString());
            }
            else if (ss[i].Contains("master"))
            {
                paramString = paramString.Replace(ss[i], GetParamValue(buff.owner.master, ss[i]).ToString());
            }
        }
                
        //Debug.Log(skillData.name + " attackPower: " + paramString + " = " + finalValue);

        return parser.Evaluate(paramString);
    }

    ExpressionParser parser = new ExpressionParser();
    public float GetParamValue(BattleUnit unit, string paramString)
    {
        //selft, target이런거 제외            
        //self. target. party. global. (타입 구분은 미정)
        paramString = paramString.Replace("owner.", "");
        paramString = paramString.Replace("self.", "");
        paramString = paramString.Replace("target.", "");
        paramString = paramString.Replace("master.", "");
        paramString = paramString.Trim();

        Expression exp = parser.EvaluateExpression(paramString);

        List<string> keys = exp.Parameters.Keys.ToList();
        for (int i = 0; i < keys.Count; i++)
        {
            string propertyName = keys[i];

            StatType statType = (StatType)System.Enum.Parse(typeof(StatType), propertyName, true);
            Stat stat = unit.stats.GetParam(statType);

            if (stat != null)
            {
                exp.Parameters[keys[i]].Value = stat.value;
            }
        }

        return (float)exp.Value;
    }

}
