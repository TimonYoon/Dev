using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using CodeStage.AntiCheat.ObscuredTypes;

[System.Serializable]
public class BuffBaseData
{
    public BuffBaseData(LitJson.JsonData jsonData)
    {

        //json 파싱
        if (jsonData == null)
        {
            Debug.LogError("Failed to parse Buff json data");
            return;
        }

        id = jsonData["id"].ToString();

        name = jsonData["name"].ToString();

        duration = jsonData["duration"].ToFloat() * 0.001f;

        interval = jsonData["interval"].ToFloat() * 0.001f;

        resetType = jsonData["resetType"].ToString();

        maxStackCount = jsonData["maxStackCount"].ToInt();

        isUnique = jsonData["isUnique"].ToBool();
        
        linkerType = jsonData["linkerType"].ToString();
        linkerType2 = jsonData["linkerType2"].ToString();

        string statString = jsonData["stat"].ToString();
        if (string.IsNullOrEmpty(statString))
            stat = StatType.NotDefined;
        else
            stat = (StatType)System.Enum.Parse(typeof(StatType), statString, true);

        string statString2 = jsonData["stat2"].ToString();
        if (string.IsNullOrEmpty(statString2))
            stat2 = StatType.NotDefined;
        else
            stat2 = (StatType)System.Enum.Parse(typeof(StatType), statString2, true);

        modifyType = jsonData["modifyType"].ToString();
        modifyType2 = jsonData["modifyType2"].ToString();

        modifyValue = jsonData["modifyValue"].ToString();
        modifyValue2 = jsonData["modifyValue2"].ToString();


        string linkedStatString = jsonData["linkedStat"].ToString();
        if (string.IsNullOrEmpty(linkedStatString))
            linkedStat = StatType.NotDefined;
        else
            linkedStat = (StatType)System.Enum.Parse(typeof(StatType), linkedStatString, true);

        string linkedStatString2 = jsonData["linkedStat2"].ToString();
        if (string.IsNullOrEmpty(linkedStatString2))
            linkedStat2 = StatType.NotDefined;
        else
            linkedStat2 = (StatType)System.Enum.Parse(typeof(StatType), linkedStatString2, true);


        multiplyValue = jsonData["multiply"].ToString();
        multiplyValue2 = jsonData["multiply2"].ToString();


        effect = jsonData["effect"].ToString();

        power = jsonData["power"].ToString();

        provoke = jsonData["provoke"].ToBool();

        blockMove = jsonData["blockMove"].ToBool();

        blockAttack = jsonData["blockAttack"].ToBool();

        airborne = jsonData["airborne"].ToBool();

        notTargeting = jsonData["notTargeting"].ToBool();

        effectPrefab = jsonData["effectPrefab"].ToString();




        trigger = jsonData["trigger"].ToString();
        triggerTarget = jsonData["triggerTarget"].ToString();
        triggerBuff = jsonData["triggerBuff"].ToString();

        triggerProbability = jsonData["triggerProbability"].ToString();

        if(jsonData.ContainsKey("triggerCooltime"))
            triggerCooltime = jsonData["triggerCooltime"].ToString();
        if(jsonData.ContainsKey("skillID"))
        {
            skillID = jsonData["skillID"].ToStringJ();
        }

        description = jsonData["description"].ToString();
    }


    public ObscuredString id { get; private set; }
    public string name { get; private set; }

    /// <summary> 지속시간 </summary>
    public float duration { get; private set; }

    /// <summary> 효과 발생 주기. 0이면 처음 버프 적용될 때 한 번 효과 발생함 </summary>
    public ObscuredFloat interval { get; private set; }

    /// <summary> 버프 초기화 타입 (캐릭터가 죽었을 때) </summary>
    public string resetType { get; private set; }

    /// <summary> 같은 대상에게 동일한 버프가 몇개까지 동시에 적용될 수 있는지 </summary>
    public ObscuredInt maxStackCount { get; private set; }

    /// <summary> 고유한 버프인지 여부. 고유 버프의 경우 최초 버프를 적용한 시전자의 스택에 계속 쌓임 </summary>
    public bool isUnique { get; private set; }

    //----------

    /// <summary> 버프가 적용된 대상의 변경할 능력치 </summary>
    public StatType stat { get; private set; }
        
    /// <summary> 곲 or 더하기 </summary>
    public string modifyType { get; private set; }

    /// <summary> 능력치 변경할 정도. 공식으로 저장됨 </summary>
    public string modifyValue { get; private set; }
    
    public StatType linkedStat { get; private set; }

    public string multiplyValue { get; private set; }

    public string linkerType { get; private set; }

    //----------

    /// <summary> 버프가 적용된 대상의 변경할 능력치 </summary>
    public StatType stat2 { get; private set; }

    /// <summary> 곲 or 더하기 </summary>
    public string modifyType2 { get; private set; }

    /// <summary> 능력치 변경할 정도. 공식으로 저장됨 </summary>
    public string modifyValue2 { get; private set; }

    public StatType linkedStat2 { get; private set; }

    public string multiplyValue2 { get; private set; }

    public string linkerType2 { get; private set; }

    /// <summary> 데미지 주는 버프의 경우. 틱당 주는 피해량. 공식으로 정의되어 있음 </summary>
    public ObscuredString power { get; private set; }

    /// <summary> 특수한 효과 정의. 코드에서 예외 처리용 </summary>
    public string effect { get; private set; }


    /// <summary> 버프가 적용된 동안 도발 </summary>
    public bool provoke { get; private set; }

    /// <summary> 버프가 적용된 동안 이동 불가 </summary>
    public bool blockMove { get; private set; }

    /// <summary> 버프가 적용된 동안 공격 불가 </summary>
    public bool blockAttack { get; private set; }

    /// <summary> 버프가 적용된 공중에 떠있음 </summary>
    public bool airborne { get; private set; }

    /// <summary> 버프가 적용된 타게팅 대상이 되지 않음 </summary>
    public bool notTargeting { get; private set; }

    /// <summary> 버프가 적용된 대상에게 표시될 이펙트 프리팹 </summary>
    public string effectPrefab { get; private set; }



    public string trigger { get; private set; }

    public string triggerTarget { get; private set; }

    public string triggerBuff { get; private set; }

    public string triggerProbability { get; private set; }

    public string triggerCooltime { get; private set; }

    public string skillID { get; private set; }

    public string description { get; private set; }
}
