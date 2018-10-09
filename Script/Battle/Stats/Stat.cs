using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;


public class Stat
{
    public SimpleDelegate onChangedValue;

    public SimpleDelegate onChangedBaseValue;

    public StatBaseData baseData;
    
    public List<StatModifier> modifiers = new List<StatModifier>();

    ObscuredDouble _value = 0;
    public ObscuredDouble value
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

    protected ObscuredDouble _baseValue = 0;
    public virtual ObscuredDouble baseValue
    {
        get { return _baseValue; }
        set
        {
            bool isChanged = _baseValue != value;

            _baseValue = value;

            if (isChanged && onChangedBaseValue != null)
                onChangedBaseValue();

        }
    }
}

public interface IStatModifiable
{
    void AddModifier(StatModifier mod);
    void RemoveModifier(StatModifier mod);
    void ClearModifiers();
    void UpdateModifiers();
}

public class ModifiableStat : Stat, IStatModifiable
{   
    public override ObscuredDouble baseValue
    {
        get { return _baseValue; }
        set
        {
            bool isChanged = _baseValue != value;

            _baseValue = value;

            if (isChanged)
            {
                UpdateModifiers();

                if (onChangedBaseValue != null)
                    onChangedBaseValue();
            }                
        }
    }

    void OnChangedModifierValue()
    {
        UpdateModifiers();
    }

    public void AddModifier(StatModifier mod)
    {
        StatModifier modifier = modifiers.Find(x => x == mod);//.type == mod.type);
        if(modifier == null)
        {
            modifier = mod;
            modifiers.Add(mod);
            mod.onValueChange += OnChangedModifierValue;
        }
        else
        {
            //modifier.stack++;
        }
        
        UpdateModifiers();
    }

    public void ClearModifiers()
    {
        for(int i = 0; i < modifiers.Count; i++)
        {
            modifiers[i].onValueChange -= OnChangedModifierValue;
        }

        modifiers.Clear();

        UpdateModifiers();
    }

    public void RemoveModifier(StatModifier mod)
    {
        mod.onValueChange -= OnChangedModifierValue;
        mod.linker = null;        
        modifiers.Remove(mod);
        mod = null;

        UpdateModifiers();
    }

    public void UpdateModifiers()
    {
        double baseValueAdd = 0d;
        double baseValuePercent = 0d;

        for(int i = 0; i < modifiers.Count; i++)
        {
            StatModifier mod = modifiers[i];
            if(mod.type == StatModifier.Type.BaseValuePercent)
            {
                baseValuePercent += mod.value;
            }
            else if(mod.type == StatModifier.Type.BaseValueAdd)
            {
                baseValueAdd += mod.value;
            }
        }

        value = (baseValue * (1d + baseValuePercent * 0.0001d)) + baseValueAdd;
    }
}
