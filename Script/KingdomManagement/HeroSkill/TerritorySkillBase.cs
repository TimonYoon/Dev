using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IExecute
{

}

/// <summary> 실행 조건 체크 </summary>
public interface ICheckExecutionCondition
{
    /// <summary> 실행 상태인가 </summary>
    bool isChackExecutionCondition();

    //bool isTimeChack(float time);
}

public interface IApply
{
    void ddd();
}

///// <summary> 배치 건물 조건 타입 </summary>
//public enum ConditionBuildingType
//{
//    All,
//    Main,
//    Collect,
//    Production,
//    Storage,
//    Specific,
//}

//public enum ConditionBuildingState
//{
//    Always,
//    TimeToTime,
//    Material
//}


//public enum ConditionMinionState
//{
//    None,
//    GoalBuildingMoveing,
//    HomeBuildingMoveing
//}

//public enum ApplyType
//{
//    None,
//    DeployHeroCount,
//}

/// <summary> 내정 영웅 스킬의 가장 상위 클래스 </summary>
public class TerritorySkillBase : MonoBehaviour {

    public string id { get; private set; }

    public TerritorySkillData territorySkillData { get; private set; }
   
    public ICheckExecutionCondition checkExecutionCondition { get; private set; } 



    public void InitTerritorySkillBase(TerritorySkillData data)
    {
        id = data.id;
        territorySkillData = data;

        //if (data.conditionBuildingState == ConditionBuildingState.Always)
        //    checkExecutionCondition = new ConditionAlways();
        //else if (data.conditionBuildingState == ConditionBuildingState.TimeToTime)
        //    checkExecutionCondition = new ConditionTimeToTime(data.intervalTime);

    }
}

//################### 실행 조건부 ###########################################

public class ConditionAlways : ICheckExecutionCondition
{
    public bool isChackExecutionCondition()
    {
        bool result = false;

        result = true;

        return result;
    }
}

public class ConditionTimeToTime : ICheckExecutionCondition
{
    /// <summary> 시간 간격 </summary>
    public float interval { get; private set; }

    public ConditionTimeToTime(float intervalTime)
    {
        interval = intervalTime;
    }

    public float startTime { get; private set; }

    /// <summary> 시작 시간 초기화 </summary>
    public void InitStartTime()
    {
        startTime = Time.unscaledTime + interval;
    }

    public bool isChackExecutionCondition()
    {
        bool result = false;
        if (startTime > Time.unscaledTime)
        {
           
        }
        else
        {
            InitStartTime();
            result = true;
        }
        return result;

    }
    
}

//################### 실행부 ###########################################

//public class DeployHeroCount : TerritorySkillBase, IApply
//{

//}


