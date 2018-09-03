using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IExecuteTargetCollector
{
    //BattleUnit owner { get; set; }

    //List<BattleHero> targetList { get; set; }

    List<BattleHero> CollectTargets(SkillBase skill);

}

abstract public class SkillETCollectorBase : IExecuteTargetCollector
{
    //List<BattleHero> _targetList = new List<BattleHero>();
    protected List<BattleHero> targetList = new List<BattleHero>();
    //{
    //    get { return _targetList; }
    //    set { _targetList = value; }
    //}

    protected CustomList<BattleHero> allyList = null;

    protected CustomList<BattleHero> enemyList = null;

    virtual public List<BattleHero> CollectTargets(SkillBase skill)
    {
        if (skill.owner.master.battleGroup)
        {
            if (skill.owner.master.team == BattleUnit.Team.Red)
            {
                allyList = skill.owner.master.battleGroup.redTeamList;
                enemyList = skill.owner.master.battleGroup.blueTeamList;
            }
            else if (skill.owner.master.team == BattleUnit.Team.Blue)
            {
                allyList = skill.owner.master.battleGroup.blueTeamList;
                enemyList = skill.owner.master.battleGroup.redTeamList;
            }
        }

        targetList.Clear();

        return targetList;
    }
}

/// <summary> 대상을 취합하지 않음 </summary>
public class SkillETCollectorNone : SkillETCollectorBase
{
    override public List<BattleHero> CollectTargets(SkillBase skill)
    {
        return targetList;
    }
}

/// <summary> 스킬의 캐스트 타겟과 동일 </summary>
public class SkillETCollectorTarget : SkillETCollectorBase
{
    override public List<BattleHero> CollectTargets(SkillBase skill)
    {
        targetList.Clear();

        if (!skill.castTarget)
        {
            if (skill.castTargetCollector != null)
                skill.castTarget = skill.castTargetCollector.CollectTarget(skill);

            if(!skill.castTarget)
                return targetList;
        }
            

        if (!skill.castTarget.gameObject.activeSelf)
            return targetList;

        //죽은 유닛한테 시전 불가 스킬인 경우 사망 상태 체크함
        if (!skill.skillData.canCastToDeadUnit && skill.castTarget.isDie)
            return targetList;

        if(skill.IsValidCondition(skill.castTarget, skill.skillData.condition1))
        {
            targetList.Add(skill.castTarget);
        }   

        return targetList;
    }
}

/// <summary> 스킬의 캐스트 타겟과 동일 </summary>
public class SkillETCollectorCasterAround : SkillETCollectorBase
{
    override public List<BattleHero> CollectTargets(SkillBase skill)
    {
        targetList = base.CollectTargets(skill);

        List<BattleHero> list = null;
        if (skill.skillData.targetFilter == SkillBase.TargetFilter.HostileAll)
            list = enemyList;
        else if (skill.skillData.targetFilter == SkillBase.TargetFilter.FriendlyAll)
            list = allyList;

        else if (skill.skillData.targetFilter == SkillBase.TargetFilter.All)
        {
            list = new List<BattleHero>();
            list.AddRange(allyList);
            list.AddRange(enemyList);
        }
        else if(skill.skillData.targetFilter == SkillBase.TargetFilter.Self)
        {
            if(skill.owner is BattleHero)
                targetList.Add(skill.owner as BattleHero);
            else if(skill.owner.master is BattleHero)
                targetList.Add(skill.owner.master as BattleHero);

            return targetList;
        }
        else if(skill.skillData.targetFilter == SkillBase.TargetFilter.None)
        {
            return targetList;
        }

        if (list == null || list.Count == 0)
            return targetList;

        for(int i = 0; i < list.Count; i++)
        {
            //스킬에 죽은 애한테 사용 불가인데 죽어 있으면 제외
            if (!skill.skillData.canCastToDeadUnit && list[i].isDie)
                continue;

            //디스폰 상태인 애는 제외
            if (!list[i].gameObject.activeSelf)
                continue;
            
            //거리 안 맞는 애 제외
            float dist = list[i].GetDistanceFrom(skill.owner);
            if (dist > skill.skillData.collectRangeMax || dist < skill.skillData.collectRangeMin)
                continue;

            //조건 안 맞는애 제외
            if (!skill.IsValidCondition(list[i], skill.skillData.condition1))
                continue;

            //걸르고 걸러서 살아남은 놈이면 리스트에 추가
            targetList.Add(list[i]);
        }

        if (skill.skillData.id.Contains("Skill_Saladin_DefenseUp"))
        {
            skill.owner.aroundEnemyCount = targetList.Count;
        }

        return targetList;
    }
}


/// <summary> 스킬의 캐스트 타겟과 동일 </summary>
public class SkillETCollectorGlobal : SkillETCollectorBase
{
    override public List<BattleHero> CollectTargets(SkillBase skill)
    {
        targetList = base.CollectTargets(skill);

        List<BattleHero> list = null;
        if (skill.skillData.targetFilter == SkillBase.TargetFilter.HostileAll)
            list = enemyList;
        else if (skill.skillData.targetFilter == SkillBase.TargetFilter.FriendlyAll)
            list = allyList;

        else if (skill.skillData.targetFilter == SkillBase.TargetFilter.All)
        {
            list = new List<BattleHero>();
            list.AddRange(allyList);
            list.AddRange(enemyList);
        }
        else if (skill.skillData.targetFilter == SkillBase.TargetFilter.Self)
        {
            if (skill.owner is BattleHero)
                targetList.Add(skill.owner as BattleHero);
            else if (skill.owner.master is BattleHero)
                targetList.Add(skill.owner.master as BattleHero);

            return targetList;
        }
        else if (skill.skillData.targetFilter == SkillBase.TargetFilter.None)
        {
            return targetList;
        }

        if (list == null || list.Count == 0)
            return targetList;

        for (int i = 0; i < list.Count; i++)
        {
            //스킬에 죽은 애한테 사용 불가인데 죽어 있으면 제외
            if (!skill.skillData.canCastToDeadUnit && list[i].isDie)
                continue;

            //디스폰 상태인 애는 제외
            if (!list[i].gameObject.activeSelf)
                continue;

            ////거리 안 맞는 애 제외
            //float dist = list[i].GetDistanceFrom(skill.owner);
            //if (dist > skill.skillData.collectRangeMax || dist < skill.skillData.collectRangeMin)
            //    continue;

            //조건 안 맞는애 제외
            if (!skill.IsValidCondition(list[i], skill.skillData.condition1))
                continue;

            //걸르고 걸러서 살아남은 놈이면 리스트에 추가
            targetList.Add(list[i]);
        }

        return targetList;
    }
}

/// <summary> 자기 자신 </summary>
public class SkillETCollectorSelf : SkillETCollectorBase
{
    override public List<BattleHero> CollectTargets(SkillBase skill)
    {
        targetList = base.CollectTargets(skill);

        if (skill.owner is BattleHero)
            targetList.Add(skill.owner as BattleHero);

        return targetList;
    }
}


/// <summary> 스킬의 캐스타 타겟 주변 </summary>
public class SkillETCollectorTargetAround : SkillETCollectorBase
{
    override public List<BattleHero> CollectTargets(SkillBase skill)
    {
        targetList = base.CollectTargets(skill);

        List<BattleHero> list = null;
        if (skill.skillData.targetFilter == SkillBase.TargetFilter.HostileAll)
            list = enemyList;
        else if (skill.skillData.targetFilter == SkillBase.TargetFilter.FriendlyAll)
            list = allyList;

        else if (skill.skillData.targetFilter == SkillBase.TargetFilter.All)
        {
            list = new List<BattleHero>();
            list.AddRange(allyList);
            list.AddRange(enemyList);
        }
        else if (skill.skillData.targetFilter == SkillBase.TargetFilter.Self)
        {
            if (skill.owner is BattleHero)
                targetList.Add(skill.owner as BattleHero);
            else if (skill.owner.master is BattleHero)
                targetList.Add(skill.owner.master as BattleHero);

            return targetList;
        }
        else if (skill.skillData.targetFilter == SkillBase.TargetFilter.None)
        {
            return targetList;
        }

        if (list == null || list.Count == 0)
            return targetList;

        for (int i = 0; i < list.Count; i++)
        {
            //스킬에 죽은 애한테 사용 불가인데 죽어 있으면 제외
            if (!skill.skillData.canCastToDeadUnit && list[i].isDie)
                continue;

            //디스폰 상태인 애는 제외
            if (!list[i].gameObject.activeSelf)
                continue;

            //거리 안 맞는 애 제외
            float dist = list[i].GetDistanceFrom(skill.castTarget);
            if (dist > skill.skillData.collectRangeMax || dist < skill.skillData.collectRangeMin)
                continue;

            //조건 안 맞는애 제외
            if (!skill.IsValidCondition(list[i], skill.skillData.condition1))
                continue;

            //걸르고 걸러서 살아남은 놈이면 리스트에 추가
            targetList.Add(list[i]);
        }
        //Debug.Log("타겟 카운트 " + targetList.Count);
        return targetList;
    }
}

/// <summary> 스킬의 캐스트 타겟과 동일 </summary>
public class SkillETCollectorTrail : SkillETCollectorBase
{
    override public List<BattleHero> CollectTargets(SkillBase skill)
    {
        targetList = base.CollectTargets(skill);

        List<BattleHero> list = null;
        if (skill.skillData.targetFilter == SkillBase.TargetFilter.HostileAll)
            list = enemyList;
        else if (skill.skillData.targetFilter == SkillBase.TargetFilter.FriendlyAll)
            list = allyList;

        else if (skill.skillData.targetFilter == SkillBase.TargetFilter.All)
        {
            list = new List<BattleHero>();
            list.AddRange(allyList);
            list.AddRange(enemyList);
        }
        else if (skill.skillData.targetFilter == SkillBase.TargetFilter.Self)
        {
            if (skill.owner is BattleHero)
                targetList.Add(skill.owner as BattleHero);
            else if (skill.owner.master is BattleHero)
                targetList.Add(skill.owner.master as BattleHero);

            return targetList;
        }
        else if (skill.skillData.targetFilter == SkillBase.TargetFilter.None)
        {
            return targetList;
        }

        if (list == null || list.Count == 0)
            return targetList;

        for (int i = 0; i < list.Count; i++)
        {
            //스킬에 죽은 애한테 사용 불가인데 죽어 있으면 제외
            if (!skill.skillData.canCastToDeadUnit && list[i].isDie)
                continue;

            //디스폰 상태인 애는 제외
            if (!list[i].gameObject.activeSelf)
                continue;

            //거리 안 맞는 애 제외
            float max = 0f;
            float min = 0f;
            if(skill.owner.skillStartPos.x < skill.owner.transform.position.x)
            {
                max = skill.owner.transform.position.x;
                min = skill.owner.skillStartPos.x;
            }
            else
            {
                max = skill.owner.skillStartPos.x;
                min = skill.owner.transform.position.x;
            }
            if (list[i].transform.position.x < min || list[i].transform.position.x > max)
                continue;

            //조건 안 맞는애 제외
            if (!skill.IsValidCondition(list[i], skill.skillData.condition1))
                continue;

            //걸르고 걸러서 살아남은 놈이면 리스트에 추가
            targetList.Add(list[i]);
        }

        return targetList;
    }
}