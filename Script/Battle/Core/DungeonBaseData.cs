using UnityEngine;
using System.Collections;
using LitJson;
using CodeStage.AntiCheat.ObscuredTypes;

[System.Serializable]
/// <summary> 던전 정적 데이터 </summary>
public class DungeonBaseData
{
    public DungeonBaseData(JsonData jsonData)
    {
        dungeonID = JsonParser.ToString(jsonData["dungeonID"]);
        territoryID = JsonParser.ToString(jsonData["territoryID"]);
        dungeonName = JsonParser.ToString(jsonData["dungeonName"]);
        battleGroupPrefabName = JsonParser.ToString(jsonData["battleGroupPrefabName"]);
        dropItemID = JsonParser.ToString(jsonData["dropItemID"]);
        dropItemCountFormula = JsonParser.ToString(jsonData["dropItemValue"]);
        expValueFormula = JsonParser.ToString(jsonData["expValue"]);
        buffID = JsonParser.ToString(jsonData["buffID"]);
    }


    /// <summary> 던전의 고유 아이디 </summary>
    public string dungeonID { get; private set; }

    /// <summary> 던전이 소속된 영지 아이디 </summary>
    public string territoryID { get; private set; }

    /// <summary> 던전 이름 </summary>
    public string dungeonName { get; private set; }

    /// <summary> 생성할 배틀 그룹 프리팹 이름 </summary>
    public string battleGroupPrefabName { get; private set; }

    /// <summary> 드롭되는 아이템 아이디 </summary>
    public ObscuredString dropItemID { get; private set; }

    /// <summary> 드롭되는 아이템 수량 공식 </summary>
    public string dropItemCountFormula { get; private set; }

    /// <summary> 획득 경험치 공식 </summary>
    public string expValueFormula { get; private set; }

    /// <summary> 해당 던전의 기본 버프 </summary>
    public ObscuredString buffID { get; private set; }

}