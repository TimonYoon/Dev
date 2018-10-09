using UnityEngine;
using System.Collections;
using LitJson;
using CodeStage.AntiCheat.ObscuredTypes;

public class StatBaseData
{
    public StatBaseData(JsonData jsonData)
    {
        //json 파싱
        if (jsonData == null)
        {
            Debug.LogError("Failed to parse Stat json data");
            return;
        }

        id = jsonData["id"].ToString();
        name = jsonData["name"].ToString();
        description = jsonData["description"].ToString();
        icon = jsonData["icon"].ToString();
        index = jsonData["index"].ToInt();
        hideInUI = jsonData["hideInUI"].ToBool();

        string expressionTypeString = jsonData["expressionType"].ToString();
        if (string.IsNullOrEmpty(expressionTypeString))
            expressionType = ExpressionType.Value;
        else
            expressionType = (ExpressionType)System.Enum.Parse(typeof(ExpressionType), expressionTypeString, true);
    }

    /// <summary> StatType과 동일한 id 사용하는 것을 권장함 </summary>
    public ObscuredString id;

    /// <summary> 스탯 이름. ex) 물리속성 피해 증가 </summary>
    public string name;

    /// <summary> 툴팁 같은데 뜨는 설명 ex) "스킬의 위력에 영향을 준다." </summary>
    public string description;

    /// <summary> UI에 표시될 때 같이 뜰 아이콘 (미구현)" </summary>
    public string icon;

    /// <summary> 정보창에서 스탯 정렬 순서 </summary>
    public int index;

    /// <summary> 정보창에서 보여줄껀지 말껀지. </summary>
    public bool hideInUI;

    public enum ExpressionType
    {
        Value,
        Percent,
        BaseValue,
        AdditionalValue,
        TotalValue,
        BasePercent,
        AdditionalPercent,
        TotalPercent
    }

    /// <summary> Stat value를 보여주는 방식. Value: 123.4a, Percent: 123.45% (value * 0.01 한 값에 % 붙임) </summary>
    public ExpressionType expressionType;
}
