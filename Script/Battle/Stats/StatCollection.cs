using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BattleStats : StatCollection
{
    public override void Init()
    {
        var curHP = CreateOrGetStat<ModifiableStat>(StatType.CurHP);
        curHP.baseValue = 100;
        curHP.UpdateModifiers();

        var maxHP = CreateOrGetStat<ModifiableStat>(StatType.MaxHP);
        maxHP.baseValue = 100;
        maxHP.UpdateModifiers();

        var attackPower = CreateOrGetStat<ModifiableStat>(StatType.AttackPower);
        attackPower.baseValue = 100;
        attackPower.UpdateModifiers();

        var defensePower = CreateOrGetStat<ModifiableStat>(StatType.DefensePower);
        defensePower.baseValue = 1;
        defensePower.UpdateModifiers();

        var attackSpeed = CreateOrGetStat<ModifiableStat>(StatType.AttackSpeed);
        attackSpeed.baseValue = 10000;
        attackSpeed.UpdateModifiers();

        var moveSpeed = CreateOrGetStat<ModifiableStat>(StatType.MoveSpeed);
        moveSpeed.baseValue = 10000;
        moveSpeed.UpdateModifiers();
    }
}

[System.Serializable]
public class StatCollection
{

    public Dictionary<StatType, Stat> paramDic = new Dictionary<StatType, Stat>();

    public virtual void Init() { }

    /// <summary> 해당 타입의 스탯 값 return. 해당 스탯이 존재하지 않으면 0 반환. null 걱정 없어서 안전함 </summary>
    public double GetValueOf(StatType type)
    {
        if (!ContainParam(type))
            return 0d;

        return GetParam(type).value;
    }

    public bool ContainParam(StatType type)
    {
        return paramDic.ContainsKey(type);
    }

    public Stat GetParam(StatType type)
    {
        if (ContainParam(type))
            return paramDic[type];

        return null;
    }

    public T GetParam<T>(StatType type) where T : Stat
    {
        return GetParam(type) as T;
    }

    /// <summary> 스탯 추가 </summary>
    public T CreateStat<T>(StatType statType) where T : Stat
    {
        T stat = System.Activator.CreateInstance<T>();

        string statID = statType.ToString();
        if (GameDataManager.statBaseDataDic.ContainsKey(statID))
            stat.baseData = GameDataManager.statBaseDataDic[statID];
        else
            Debug.LogWarning(statID + " is not defiend in stat base data");

        paramDic.Add(statType, stat);
        return stat;
    }

    /// <summary> 스탯 없으면 추가, 있으면 get </summary>
    public T CreateOrGetStat<T>(StatType statType) where T : Stat
    {
        T stat = GetParam<T>(statType);
        if (stat == null)
        {
            stat = CreateStat<T>(statType);
        }
        return stat;
    }

    /// <summary> Modifier 추가 </summary>
    public void AddStatModifier(StatType type, StatModifier mod, bool update = true)
    {
        if (!ContainParam(type))
        {
            CreateStat<ModifiableStat>(type);
        }

        if (ContainParam(type))
        {
            var stat = GetParam(type) as IStatModifiable;
            if (stat != null)
            {
                stat.AddModifier(mod);
                if (update)
                    stat.UpdateModifiers();
            }
            else
            {
                Debug.Log("[RPGStats] Trying to add Stat Modifier to non modifiable stat \"" + type.ToString() + "\"");
            }
        }
        else
        {
            Debug.Log("[RPGStats] Trying to add Stat Modifier to \"" + type.ToString() + "\", but RPGStats does not contain that stat");
        }
    }

    /// <summary> Modifier 제거 </summary>
    public void RemoveModifier(StatType statType, StatModifier modifier, bool update = true)
    {
        if (ContainParam(statType))
        {
            var stat = GetParam(statType) as IStatModifiable;
            if (stat != null)
            {
                stat.RemoveModifier(modifier);
                if (update)
                    stat.UpdateModifiers();
            }
            else
            {
                Debug.Log("[RPGStats] Trying to add Stat Modifier to non modifiable stat \"" + statType.ToString() + "\"");
            }
        }
        else
        {
            Debug.Log("[RPGStats] Trying to add Stat Modifier to \"" + statType.ToString() + "\", but RPGStats does not contain that stat");
        }
    }
}

