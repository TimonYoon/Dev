using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;




public class StatModifier
{
    public enum Type
    {
        NotDefined,
        BaseValuePercent,
        BaseValueAdd
    }

    public Type type = Type.NotDefined;

    public SimpleDelegate onValueChange;


    StatLinker _linker;
    public StatLinker linker
    {
        get { return _linker; }
        set
        {
            if (_linker != null)
                _linker.onChangedValue -= OnChangedLinkerValue;

            _linker = value;

            if (value != null)
            {
                value.onChangedValue += OnChangedLinkerValue;

                this.value = value.value;
            }
                
        }
    }

    void OnChangedLinkerValue()
    {
        value = linker.value;
    }

    public StatModifier(float value = 0f)
    {
        this.value = value;
    }

    double _value = 0f;

    public double value
    {
        get
        {
            return _value * stack;
        }
        set
        {
            if (_value == value)
                return;

            _value = value;

            if(onValueChange != null)
                onValueChange();
        }
    }

    int _stack = 1;
    public int stack
    {
        get { return _stack; }
        set
        {
            bool isChanged = _stack != value;

            _stack = value;

            if (isChanged & onValueChange != null)
                onValueChange();
        }
    }
}

//public class StatModifierBaseAdd : StatModifier
//{
//    public StatModifierBaseAdd(float value = 0f)
//    {
//        this.value = value;
//    }

//    public override int ApplyModifier(int statValue, float modValue)
//    {
//        return (int)(modValue);
//    }
//}

//public class StatModifierBasePercent : StatModifier
//{
//    public StatModifierBasePercent(float value = 0f)
//    {
//        this.value = value;
//    }

//    public override int ApplyModifier(int statValue, float modValue)
//    {
//        return (int)(statValue * modValue);
//    }
//}
