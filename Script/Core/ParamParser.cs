﻿using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using B83.ExpressionParser;
using System;
using System.Reflection;

/// <summary> 일단 테스트용 </summary>
public class ParamParser : MonoBehaviour {
    static ParamParser Instance;

    static ExpressionParser parser = new ExpressionParser();

    string _attackPower;// = "100";
    public string attackPower
    {
        get { return _attackPower; }
        set { _attackPower = value; }
    }

    void Awake()
    {
        Instance = this;
    }

    void Start ()
    {
        Debug.Log(Parse("(5+3)*8^2-5*(-2) + self.attackPower"));
    }

    /// <summary> 문자열로 된 수식을 계산 </summary>
    double Parse(string expression)
    {
        
        Expression exp = parser.EvaluateExpression(expression);

        //키값 확인용
        List<string> keys = exp.Parameters.Keys.ToList();
        for(int i = 0; i < keys.Count; i++)
        {
            string propertyName = keys[i];

            Debug.Log(propertyName);

            //self. 는 스킬을 사용한 개체
            if (propertyName.Contains("self."))
            {
                propertyName = propertyName.Replace("self.", "");
            }

            //self. target. party. global. (타입 구분은 미정)

            Debug.Log(propertyName);

            Type type = this.GetType();

            Debug.Log(type.ToString());

            FieldInfo[] fields = type.GetFields();
            for(int a = 0; a < fields.Length; a++)
            {
                Debug.Log(fields[a]);
            }

            PropertyInfo p = type.GetProperty(propertyName);
            Debug.Log(p.ToString());

            int value = int.Parse(p.GetValue(Instance, null).ToString());

            Debug.Log(value);

            exp.Parameters[keys[i]].Value = value;
        }
        
        //exp.Parameters["attackPower"].Value = Instance.attackPower;

        return exp.Value;
    }
}
