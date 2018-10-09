using UnityEngine;
using System.Collections;
using LitJson;
using CodeStage.AntiCheat.ObscuredTypes;


/// <summary> 유물 관련 베이스 데이타 </summary>
public class ArtifactBaseData
{

    public ArtifactBaseData(JsonData json)
    {
        id = JsonParser.ToString(json["id"]);
        name = JsonParser.ToString(json["name"]);
        enable = JsonParser.ToBool(json["enable"]);
        message = JsonParser.ToString(json["message"]);
        type = JsonParser.ToString(json["type"]);
        filter = JsonParser.ToString(json["filter"]);
        formula = JsonParser.ToString(json["formula"]);
        buffID = JsonParser.ToString(json["buffID"]);
        icon = JsonParser.ToString(json["icon"]);
        maxStack = JsonParser.ToInt(json["maxStack"]);
    }

    /// <summary> 유물 고유 아이디 </summary>
    public ObscuredString id { get; private set; }

    /// <summary> 유물 이름 </summary>
    public string name { get; private set; }

    /// <summary> 사용 가능 여부 </summary>
    public bool enable { get; private set; }

    /// <summary> 유물 정보 </summary>
    public string message { get; private set; }

    /// <summary> 유물 타입 </summary>
    public string type { get; private set; }

    /// <summary> 적용대상 </summary>
    public ObscuredString filter { get; private set; }

    /// <summary> 적용 공식 </summary>
    public string formula { get; private set; }

    /// <summary> 버프 아이디 </summary>
    public ObscuredString buffID { get; private set; }

    /// <summary> 버프 아이디 </summary>
    public string icon { get; private set; }

    /// <summary> 최대 선택 가능한 수량 </summary>
    public ObscuredInt maxStack { get; private set; }
}
