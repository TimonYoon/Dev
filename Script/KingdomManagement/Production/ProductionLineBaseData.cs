using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

/// <summary> 내정 생산 라인 베이스 데이타 </summary>
public class ProductionLineBaseData
{
    public ProductionLineBaseData(JsonData jsonData)
    {
        string key = "id";
        if (jsonData.ContainsKey(key))
            id = JsonParser.ToString(jsonData[key]);

        key = "name";
        if (jsonData.ContainsKey(key))
            name = JsonParser.ToString(jsonData[key]);

        key = "openLevel";
        if (jsonData.ContainsKey(key))
            openLevel = JsonParser.ToInt(jsonData[key]);
    }

    public string id { get; private set; }

    public string name { get; private set; }

    public int openLevel { get; private set; }
}