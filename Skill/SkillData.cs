using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
/// <summary> 스킬 json 데이터 </summary>
public class SkillData
{
    public SkillData(LitJson.JsonData jsonData)
    {
        //json 파싱
        if (jsonData == null)
        {
            Debug.LogError("Failed to parse Territory json data");
            return;
        }

        id = jsonData["id"].ToString();
        name = jsonData["name"].ToString();
        dialogue = jsonData["dialogue"].ToString();

        //액티브/패시브 구분
        string activeTypeString = jsonData["activeType"].ToString();
        if (string.IsNullOrEmpty(activeTypeString))
            activeType = SkillBase.ActiveType.NotDefined;
        else
            activeType = (SkillBase.ActiveType)System.Enum.Parse(typeof(SkillBase.ActiveType), activeTypeString, true);

        //데미지 타입
        string damageTypeString = jsonData["damageType"].ToString();
        if (string.IsNullOrEmpty(damageTypeString))
            damageType = SkillBase.DamageType.NotDefined;
        else
            damageType = (SkillBase.DamageType)System.Enum.Parse(typeof(SkillBase.DamageType), damageTypeString, true);

        //거리 타입
        string rangeTypeString = jsonData["rangeType"].ToString();
        if (string.IsNullOrEmpty(rangeTypeString))
            rangeType = SkillBase.RangeType.NotDefined;
        else
            rangeType = (SkillBase.RangeType)System.Enum.Parse(typeof(SkillBase.RangeType), rangeTypeString, true);

        string elementalTypeString = jsonData["elementalType"].ToString();
        if (string.IsNullOrEmpty(elementalTypeString))
            elementalType = SkillBase.ElementalType.None;
        else
            elementalType = (SkillBase.ElementalType)System.Enum.Parse(typeof(SkillBase.ElementalType), elementalTypeString, true);

        string triggerTypeString = jsonData["trigger"].ToString();
        if (string.IsNullOrEmpty(triggerTypeString))
            triggerType = SkillBase.TriggerType.None;
        else
            triggerType = (SkillBase.TriggerType)System.Enum.Parse(typeof(SkillBase.TriggerType), triggerTypeString, true);

        autoExecute = jsonData["autoExecute"].ToBool();

        ignoreIdle = jsonData["ignoreIdle"].ToBool();

        track = jsonData["track"].ToInt();

        priority = jsonData["priority"].ToInt();
        coolTime1ID = jsonData["coolTime1ID"].ToString();
        coolTime1 = jsonData["coolTime1"].ToInt() * 0.001f;
        coolTime2ID = jsonData["coolTime2ID"].ToString();
        coolTime2 = jsonData["coolTime2"].ToInt() * 0.001f;
        //condition = JsonParser.ToString(jsonData["condition"]);

        string targetFilterString = jsonData["targetFilter"].ToString();
        if (string.IsNullOrEmpty(targetFilterString))
            targetFilter = SkillBase.TargetFilter.None;
        else
            targetFilter = (SkillBase.TargetFilter) System.Enum.Parse(typeof(SkillBase.TargetFilter), targetFilterString, true);

        condition1 = jsonData["condition1"].ToString();
        //if(!string.IsNullOrEmpty(condition1A))
        //    Debug.Log(condition1A);
        condition1 = condition1.Trim();   //공백 제거
        condition1 = condition1.Replace(" ", "");

        canMove = jsonData["canMove"].ToBool();

        moveSpeed = jsonData["moveSpeed"].ToFloat() * 0.01f;

        duration = jsonData["duration"].ToFloat() * 0.001f;

        //죽은 유닛한테 스킬 시전 가능 여부.. 이것만 일단 하드코딩함. 나중에 범용적으로 쓰일 것 같으면 데이타화함
        canCastToDeadUnit = false;
        if(jsonData["effectType"].ToString().Contains("Resurrect"))
            canCastToDeadUnit = true;

        string castTargetTypeString = jsonData["castTargetType"].ToString();
        castTargetType = (SkillBase.CastTargetType)System.Enum.Parse(typeof(SkillBase.CastTargetType), castTargetTypeString, true);

        castCondition = jsonData["castCondition"].ToString();

        canCastToAir = jsonData["canCastToAir"].ToBool();

        castToDeadUnit = jsonData["castToDeadUnit"].ToBool();
        if (castToDeadUnit)
            canCastToDeadUnit = true;

        string collectTargetTypeString = jsonData["collectTargetType"].ToString();
        if (string.IsNullOrEmpty(collectTargetTypeString))
            collectTargetType = SkillBase.CollectTargetType.None;
        else
            collectTargetType = (SkillBase.CollectTargetType)System.Enum.Parse(typeof(SkillBase.CollectTargetType), collectTargetTypeString, true);

        collectRangeMin = jsonData["collectRangeMin"].ToFloat();
        collectRangeMax = jsonData["collectRangeMax"].ToFloat();

        maxTargetCount = jsonData["maxTargetCount"].ToInt();
        power = jsonData["power"].ToString();


        minDistance = jsonData["minDistance"].ToFloat();
        maxDistance = jsonData["maxDistance"].ToFloat();

        string forceTypeString = jsonData["forceType"].ToString();
        if (string.IsNullOrEmpty(forceTypeString))
            forceType = SkillBase.ForceType.None;
        else
            forceType = (SkillBase.ForceType)System.Enum.Parse(typeof(SkillBase.ForceType), forceTypeString, true);

        //단위는 m, m/sec
        forcePower = jsonData["forcePower"].ToFloat() * 0.01f;
        knockbackDistance = jsonData["knockbackDistance"].ToFloat() * 0.01f;

        effectType = jsonData["effectType"].ToString();
        
        buffID = jsonData["buffID"].ToString();
        buffStack = jsonData["buffStack"].ToInt();
        buffProbability = jsonData["buffProbability"].ToInt();

        summonID = jsonData["summonID"].ToString();
        summonTime = jsonData["summonTime"].ToFloat() * 0.001f;

        description = jsonData["description"].ToString();
    }

    public string id { get; private set; }
    public string name { get; private set; }
    public string dialogue { get; private set; }

    public SkillBase.ActiveType activeType { get; private set; }

    public SkillBase.DamageType damageType { get; private set; }

    public SkillBase.RangeType rangeType { get; private set; }

    public SkillBase.ElementalType elementalType { get; private set; }

    public SkillBase.TriggerType triggerType { get; private set; }

    public bool autoExecute { get; private set; }

    public bool ignoreIdle { get; private set; }

    /// <summary> 스파인 애니 트랙. 0아니면 기존 동작에 믹싱으로 처리 </summary>
    public int track { get; private set; }

    public int priority { get; private set; }
    public string coolTime1ID { get; private set; }
    public float coolTime1 { get; private set; }
    public string coolTime2ID { get; private set; }
    public float coolTime2 { get; private set; }

    public bool canMove { get; private set; }
    public float moveSpeed { get; private set; }

    public float duration { get; private set; }

    /// <summary> 죽은 유닛한테 스킬 시전 가능 여부 </summary>
    public bool canCastToDeadUnit { get; private set; }
    
    /// <summary> 시전 대상 </summary>
    public SkillBase.CastTargetType castTargetType { get; private set; }

    /// <summary> 시전 조건 </summary>
    public string castCondition { get; private set; }

    /// <summary> 공중에 있는 유닛한테 스킬 시전 가능 여부 </summary>
    public bool canCastToAir { get; private set; }

    /// <summary> 죽은 유닛한테 스킬 시전 가능 여부 </summary>
    public bool castToDeadUnit { get; private set; }

    /// <summary> 효과 적용을 할 대상을 검색하는 방식 </summary>
    public SkillBase.CollectTargetType collectTargetType { get; private set; }


    /// <summary> 효과 적용을 할 대상을 검색하는 범위. collectTargetType이 Target인 경우 사용 안 함 </summary>
    public float collectRangeMin { get; private set; }
    public float collectRangeMax { get; private set; }

    /// <summary> 효과 적용을 할 대상의 관계 filter. 적/아군/모두 </summary>
    public SkillBase.TargetFilter targetFilter { get; private set; }

    public string condition1 { get; private set; }
    public int maxTargetCount { get; set; }
    public string power { get; private set; }
    public float minDistance { get; private set; }
    public float maxDistance { get; private set; }

    public SkillBase.ForceType forceType { get; private set; }

    public float forcePower { get; private set; }

    public float knockbackDistance { get; private set; }

    public string effectType { get; private set; }

    [System.Obsolete]
    public string effectID { get; private set; }

    public string buffID { get; private set; }
    
    public int buffStack { get; private set; }

    /// <summary> 버프 적용 확률 </summary>
    public int buffProbability { get; private set; }

    /// <summary> 소환 될 녀석의 heroDataID </summary>
    public string summonID { get; private set; }

    /// <summary> 소환 된 녀석의 지속시간(lifeTime) </summary>
    public float summonTime { get; private set; }


    public string description { get; private set; }    
}
