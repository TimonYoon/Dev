using UnityEngine;
using System.Collections;


public class StatLinker
{
    public SimpleDelegate onChangedValue;

    /// <summary> 쳐다보는 대상 (스탯) </summary>
    protected Stat linkedStat;

    /// <summary> 10000 = 100% </summary>
    protected float multiply;

    double _value;
    /// <summary> stat.vaule * multiply * 0.0001 반환 </summary>
    public double value
    {
        get { return _value; }
        set
        {
            bool isChanged = _value != value;

            _value = value;

            if (isChanged && onChangedValue != null)
                onChangedValue();
        }
    }

    public StatLinker() { }

    /// <summary> stat의 value를 multiplyAmount 만큼 곱한 다음 value 반환. 예) attackPower 250, multiply 300 => value 75 /// </summary>
    public StatLinker(Stat stat, float multiplyAmount)
    {
        this.linkedStat = stat;
        if (stat != null)
            stat.onChangedBaseValue += this.UpdateValue;

        this.multiply = multiplyAmount;

        this.UpdateValue();
    }

    virtual protected void UpdateValue()
    {
        double baseValue;
        if(linkedStat != null)
        {
            baseValue = linkedStat.baseValue; 
        }
        else
        {
            baseValue = 10000d;
        }
        //double baseValue = linkedStat != null ? linkedStat.baseValue : 10000d;

        value = baseValue * multiply * 0.0001d;
    }
}

public class StatLinkerRemainedHP : StatLinker
{
    Stat curHP;
    Stat maxHP;
    /// <summary> 남은 체력 비율(0~1)을 곱함 </summary>
    public StatLinkerRemainedHP(Stat stat, Stat curHP, Stat maxHP, float multiplyAmount)
    {
        this.linkedStat = stat;
        this.curHP = curHP;
        this.maxHP = maxHP;

        if (this.linkedStat != null)
            stat.onChangedBaseValue += UpdateValue;
        curHP.onChangedBaseValue += UpdateValue;
        this.multiply = multiplyAmount;

        UpdateValue();
    }

    override protected void UpdateValue()
    {
        if (curHP == null || maxHP == null)
            return;

        base.UpdateValue();

        //남은 체력 비율
        double remainedHPRatio = curHP.value / maxHP.value;

        //잃은 체력 비율 만큼 곱함
        value = base.value * remainedHPRatio;
    }
}

public class StatLinkerLostHP : StatLinker
{
    Stat curHP;
    Stat maxHP;
    /// <summary> 잃은 체력 비율(0~1)을 곱함 </summary>
    public StatLinkerLostHP(Stat stat, Stat curHP, Stat maxHP, float multiplyAmount)
    {
        this.linkedStat = stat;
        this.curHP = curHP;
        this.maxHP = maxHP;

        if (stat != null)
            stat.onChangedBaseValue += UpdateValue;
        curHP.onChangedBaseValue += UpdateValue;
        this.multiply = multiplyAmount;

        UpdateValue();
    }

    override protected void UpdateValue()
    {
        if (curHP == null || maxHP == null)
            return;

        base.UpdateValue();

        //잃은 체력 비율
        double lostHPRatio = 1d - curHP.value / maxHP.value;

        //잃은 체력 비율 만큼 곱함
        value = base.value * lostHPRatio;
    }
}

public class StatLinkerAdditionalStatValue : StatLinker
{
    /// <summary> 추가 능력치의 n%만큼 보정함.
    /// stat의 baseValue가 아닌 value를 쳐다 보기 때문에 무한 순환 참조가 발생할 위험 있음. 주의해서 사용
    /// </summary>
    public StatLinkerAdditionalStatValue(Stat stat, float multiplyAmount)
    {
        this.linkedStat = stat;
        if (stat != null)
            stat.onChangedValue += UpdateValue;

        this.multiply = multiplyAmount;

        UpdateValue();
    }

    override protected void UpdateValue()
    {
        if (linkedStat == null)
            return;

        //원래 스탯 미만에서는 작동 안 함?
        if (linkedStat.value < linkedStat.baseValue)
        {
            value = 0d;
            return;
        }

        //추가 %의 n% 만큼 
        value = (linkedStat.value - linkedStat.baseValue) * multiply * 0.0001f;
    }
}


public class StatLinkerFrontmostHero : StatLinker
{
    BattleUnit owner;
    BattleGroup battle;
    /// <summary> 최전방 영웅이 아니면 0 </summary>
    public StatLinkerFrontmostHero(Stat stat, BattleGroup battle, BattleUnit owner, float multiplyAmount)
    {
        this.linkedStat = stat;
        this.battle = battle;
        this.owner = owner;
        if (stat != null)
            stat.onChangedBaseValue += UpdateValue;

        if (battle)
        {
            battle.onChangedFrontmostHero -= UpdateValue;
            battle.onChangedFrontmostHero += UpdateValue;

        }

        this.multiply = multiplyAmount;

        UpdateValue();
    }

    override protected void UpdateValue()
    {
        base.UpdateValue();

        if (!battle)
        {
            value = 0d;
            return;
        }

        //Debug.Log(this.GetHashCode() + ", " + owner.heroData.baseData.id + ", " + owner.battleGroup + ", " + linkedStat);
        double a = battle.frontMostHero != owner ? 0d : 1d;

        //잃은 체력 비율 만큼 곱함
        value = base.value * a;
    }
}


public class StatLinkerRearmostHero : StatLinker
{
    BattleUnit owner;
    BattleGroup battle;
    /// <summary> 최후방 영웅이 아니면 0 </summary>
    public StatLinkerRearmostHero(Stat stat, BattleGroup battle, BattleUnit owner, float multiplyAmount)
    {
        this.linkedStat = stat;
        this.battle = battle;
        this.owner = owner;
        if (stat != null)
            stat.onChangedBaseValue += UpdateValue;

        if (battle)
        {
            battle.onChangedRearmostHero -= UpdateValue;
            battle.onChangedRearmostHero += UpdateValue;
        }

        this.multiply = multiplyAmount;

        UpdateValue();
    }

    override protected void UpdateValue()
    {
        base.UpdateValue();

        if (!battle)
        {
            value = 0d;
            return;
        }

        double a = battle.rearMostHero != owner ? 0d : 1d;

        //잃은 체력 비율 만큼 곱함
        value = base.value * a;
    }
}


public class StatLinkerBuffStack : StatLinker
{
    Buff relatedBuff;
    /// <summary> 이 버프를 발생시킨 버프의 스택만큼 곱함 </summary>
    public StatLinkerBuffStack(Stat linkedStat, float multiplyAmount, Buff relatedBuff)
    {
        this.linkedStat = linkedStat;
        this.relatedBuff = relatedBuff;
        if (linkedStat != null)
            linkedStat.onChangedBaseValue += UpdateValue;

        this.multiply = multiplyAmount;

        UpdateValue();
    }

    override protected void UpdateValue()
    {
        base.UpdateValue();

        //잃은 체력 비율 만큼 곱함
        value = base.value * relatedBuff.stack;
    }
}

public class StatLinkerEnemyCount : StatLinker
{
    BattleGroup battleGroup;
    /// <summary> 적 수 만큼 곱함 </summary>
    public StatLinkerEnemyCount(Stat linkedStat, float multiplyAmount, BattleGroup battleGroup)
    {
        this.linkedStat = linkedStat;
        if (linkedStat != null)
            linkedStat.onChangedBaseValue += UpdateValue;

        this.multiply = multiplyAmount;

        this.battleGroup = battleGroup;

        battleGroup.onChangedMonsterCount += UpdateValue;

        UpdateValue();
    }

    override protected void UpdateValue()
    {
        base.UpdateValue();

        //잃은 체력 비율 만큼 곱함
        value = base.value * battleGroup.monsterCount;
    }
}

public class StatLinkerFriendlyDieCount : StatLinker
{
    BattleGroup battle;
    BattleUnit target;
    public StatLinkerFriendlyDieCount(Stat linkerdStat, float multiplyAmount, BattleUnit _target , BattleGroup _battle)
    {
        if(battle != null)
        {
            battle.onChangedBlueTeamDieCount -= UpdateValue;
            battle.onChangedRedTeamDieCount -= UpdateValue;
        }
        this.multiply = multiplyAmount;
        battle = _battle;
        target = _target;
        if (battle != null)
        {
            battle.onChangedBlueTeamDieCount += UpdateValue;
            battle.onChangedRedTeamDieCount += UpdateValue;
            UpdateValue();
        }
        
    }

    override protected void UpdateValue()
    {
        base.UpdateValue();

        if (battle != null)
        {            
            if (target.team == BattleUnit.Team.Blue)
                value = base.value * battle.blueTeamDieCount;
            else if (target.team == BattleUnit.Team.Red)
                value = base.value * battle.redTeamDieCount;

            //Debug.Log(target.heroData.heroName + " / " + target.team.ToString() + " / " + value);
        }
            
    }
}

public class StatLinkerSummonCount : StatLinker
{
    BattleGroup battle;
    BattleUnit target;
    public StatLinkerSummonCount(Stat linkerdStat, float multiplyAmount, BattleUnit _target, BattleGroup _battle)
    {
        if (target != null)
        {
            target.onChangedSummonCount -= UpdateValue;
        }
        this.multiply = multiplyAmount;
        battle = _battle;
        target = _target;
        if (battle != null)
        {
            target.onChangedSummonCount += UpdateValue;
            UpdateValue();
        }

    }

    override protected void UpdateValue()
    {
        base.UpdateValue();

        if (battle != null)
        {
            if (target.summonCount > 0)
                value = base.value * target.summonCount;
            
            //Debug.Log(target.heroData.heroName + " / " + target.team.ToString() + " / " + value);
        }

    }
}


public class StatLinkerAroundEnemyCount : StatLinker
{
    BattleGroup battleGroup;
    BattleUnit target;
    /// <summary> 적 수 만큼 곱함 </summary>
    public StatLinkerAroundEnemyCount(Stat linkedStat, float multiplyAmount, BattleUnit target)
    {
        this.linkedStat = linkedStat;
        if (linkedStat != null)
            linkedStat.onChangedBaseValue += UpdateValue;
        
        this.multiply = multiplyAmount;

        this.target = target;
        
            
        target.onChangedAroundEnemyCount += UpdateValue;

        UpdateValue();
    }

    override protected void UpdateValue()
    {
        base.UpdateValue();

        //잃은 체력 비율 만큼 곱함
        value = base.value * target.aroundEnemyCount;
    }
}
