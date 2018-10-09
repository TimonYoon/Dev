using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICastTargetCollector
{
    //BattleUnit owner { get; set; }

    bool IsCastTargetInSkillRange();

    BattleHero CollectTarget(SkillBase skill);

}

/// <summary> 시전 대상 찾는 녀석. cast target collector </summary>
abstract public class SkillCTCollectorBase : ICastTargetCollector
{
    protected BattleUnit owner;
    protected BattleGroup battleGroup;
    protected CustomList<BattleHero> enemyList = null;
    protected CustomList<BattleHero> allyList = null;

    protected BattleHero lastTarget = null;

    protected float lastCollectTargetTime;

    protected float refreshInterval = 1f;

    protected bool isInitialized = false;

    SkillBase skill;


    protected void Init(SkillBase skill)
    {
        this.skill = skill;
        this.owner = skill.owner;

        battleGroup = owner.battleGroup;

        if (!battleGroup)
            return;

        //if (skillList == null || skillList.Count == 0)
        //    return;

        enemyList = null;
        allyList = null;

        BattleUnit.Team team = owner.team;

       

        if (team == BattleUnit.Team.none)
            return;

        else if (team == BattleUnit.Team.Red)
        {
            allyList = battleGroup.redTeamList;
            enemyList = battleGroup.blueTeamList;
        }
        else if (team == BattleUnit.Team.Blue)
        {
            allyList = battleGroup.blueTeamList;
            enemyList = battleGroup.redTeamList;
        }

        isInitialized = true;
    }

    virtual public BattleHero CollectTarget(SkillBase skill)
    {
        Init(skill);

        //아직 쿨타임이면 이전 대상 그대로 반환
        if (lastTarget && !lastTarget.isDie && Time.time < lastCollectTargetTime + refreshInterval)
        {
            Buff buff = owner.buffController.buffList.Find(x => x.baseData.effect == "Charm" && x.isActive);
            if (buff != null)
            {
                // 유혹됬을 때 마지막 타겟 제거
                lastTarget = null;
            }
            else
            {

                return lastTarget;
            }            
        }                    

        return null;
    }


    virtual public bool IsCastTargetInSkillRange()
    {
        if (!lastTarget)
            return false;

        Vector3 targetPos = lastTarget.collider.transform.position;

        //if (skill.skillSetting.skillRange)
        //{
        //    Collider2D collider = skill.skillSetting.skillRange.collider2D;
        //    if (collider)
        //    {
        //        bool isInRange = collider.bounds.Contains(targetPos);

        //        return isInRange;
        //    }

        //}

        float dist = 0f;
        if (targetPos.x > owner.collider.transform.position.x)
            dist = targetPos.x - owner.collider.transform.position.x;
        else
            dist = owner.collider.transform.position.x - targetPos.x;

        float rangeModify = skill.skillData.rangeType == SkillBase.RangeType.Melee ? 1f : 1f + (float) skill.owner.stats.GetValueOf(StatType.IncreaseAttackRange) * 0.0001f;
        float dMax = Mathf.Max(4f, skill.skillData.maxDistance * rangeModify);
        
        return dist >= skill.skillData.minDistance && dist <= dMax;// skill.skillData.maxDistance;
    }

    protected bool IsInCastRange(BattleHero target)
    {
        if (!target)
            return false;

        float distance = target.GetDistanceFrom(owner);
        if (distance >= skill.skillData.minDistance && distance <= skill.skillData.maxDistance)
            return true;
        else
            return false;
    }
}

public class SkillCTCollectorSelf : SkillCTCollectorBase
{
    public override BattleHero CollectTarget(SkillBase skill)
    {
        return skill.owner as BattleHero;
    }

    override public bool IsCastTargetInSkillRange()
    {
        return true;
    }
}


public class SkillCTCollectorNone : SkillCTCollectorBase
{
    public override BattleHero CollectTarget(SkillBase skill)
    {
        base.CollectTarget(skill);

        if (!owner.battleGroup)
            return null;

        //Debug.Log("CastTargetCollect - none");
        return null;
    }

    override public bool IsCastTargetInSkillRange()
    {
        return true;
    }
}

/// <summary> 발사체가 날아가는 목표 대상 </summary>
public class SkillCTCollectorProjectileTarget : SkillCTCollectorBase
{
    public override BattleHero CollectTarget(SkillBase skill)
    {
       
        base.CollectTarget(skill);
        if (!owner.battleGroup)
            return null;

        if (owner is IProjectile)
        {
            IProjectile p = owner as IProjectile;
            lastTarget = p.GetTarget() as BattleHero;
            return lastTarget;
        }

        return null;
    }

    override public bool IsCastTargetInSkillRange()
    {
        return true;
    }
}

/// <summary> 가장 가까이 있는 적 </summary>
public class SkillCTCollectorNearestEnemy : SkillCTCollectorBase
{
    
    BattleHero nearestEnemy = null;
    SkillBase skill;

    public override BattleHero CollectTarget(SkillBase skill)
    {
        this.skill = skill;

        //이전 대상이 살아있는데, 검색 주기 전이면 새로 검색 안 함
        if (lastTarget && !skill.skillData.canCastToDeadUnit && lastTarget.isDie)
        {
        }
        else
        {
            if (Time.time < lastCollectTargetTime + refreshInterval)
                return lastTarget;
        }


        base.CollectTarget(skill);

        if (!owner.battleGroup)
            return null;

        if (owner.team == BattleUnit.Team.none || enemyList == null || enemyList.Count == 0)
            return null;

        //도발한 적이 있으면 해당 적을 캐스팅 대상으로 지정
        if (owner.provokeEnemy && owner.provokeEnemy.gameObject.activeSelf)
        {
            if (skill.skillData.canCastToDeadUnit || (!skill.skillData.canCastToDeadUnit && !owner.provokeEnemy.isDie)
                || skill.skillData.castToDeadUnit == owner.provokeEnemy.isDie)
                return owner.provokeEnemy;
        }
        
        nearestEnemy = null;
        float distance = -9999f;
        //BattleHero frontMostEnemy = null;

        BattleUnit.Team team = owner.team;
        //Buff buff = owner.buffController.buffList.Find(x => x.id == "Buff_Succubus_Charm" && x.isActive);
        //if (buff != null)
        //{
        //    if (team != BattleUnit.Team.none)
        //        team = team == BattleUnit.Team.Blue ? BattleUnit.Team.Red : BattleUnit.Team.Blue;
        //    Debug.Log(" 매혹 걸림 캐스팅" + owner.team.ToString() + "->" + team.ToString());
        //}


        //거리 체크 안 해도 되는 조금 빠른 방식
        if (team == BattleUnit.Team.Red)
        {
            BattleHero enemy = owner.battleGroup.frontMostMonster;
            if (enemy == owner as BattleHero)
            {
                //Debug.Log("본인 타겟 " + owner.heroData.heroName);
            }
            else if (enemy && enemy.isFinishSpawned && skill.skillData.castToDeadUnit == enemy.isDie && enemy.transform.position.x > owner.transform.position.x)
            {
                if(skill.skillData.canCastToAir || enemy.skeletonAnimation.transform.localPosition.y < 3f)
                {
                    if(!enemy.isDie || (enemy.isDie && enemy.canResurrect))
                    //if(enemy.transform.position.x - owner.transform.position.x >= skill.skillData.minDistance
                    //    && enemy.transform.position.x - owner.transform.position.x <= skill.skillData.maxDistance)
                    {
                        lastTarget = enemy;
                        lastCollectTargetTime = Time.time;
                        return enemy;
                    }
                    
                }
            }   
        }
        else if(team == BattleUnit.Team.Blue)
        {
            BattleHero enemy = owner.battleGroup.frontMostHero;
            if(enemy == owner as BattleHero)
            {
                //Debug.Log("본인 타겟 " + owner.heroData.heroName);
            }
            else if (enemy && enemy.isFinishSpawned && skill.skillData.castToDeadUnit == enemy.isDie && enemy.transform.position.x < owner.transform.position.x)
            {
                if (skill.skillData.canCastToAir || enemy.skeletonAnimation.transform.localPosition.y < 3f)
                {
                    if (!enemy.isDie || (enemy.isDie && enemy.canResurrect))
                    //if (enemy.transform.position.x - owner.transform.position.x >= skill.skillData.minDistance
                    //       && enemy.transform.position.x - owner.transform.position.x <= skill.skillData.maxDistance)
                    {
                        lastTarget = enemy;
                        lastCollectTargetTime = Time.time;
                        return enemy;
                    }
                        
                }
            }
        }
            

        for (int i = 0; i < enemyList.Count; i++)
        {
            BattleHero enemy = enemyList[i];

            if (!enemy.gameObject.activeSelf)
                continue;

            if (enemy == owner as BattleHero)
            {
                //Debug.Log("11본인 타겟 " + owner.heroData.heroName);
                continue;

            }

            if (!skill.skillData.canCastToDeadUnit && enemy.isDie)
                continue;

            if (skill.skillData.castToDeadUnit != enemy.isDie)
                continue;

            if (enemy.isDie && !enemy.canResurrect)
                continue;

            if (skill.skillData.canCastToAir || enemy.skeletonAnimation.transform.localPosition.y < 3f)
            {
                distance = enemy.GetDistanceFrom(owner);

                //if (distance < skill.skillData.minDistance || distance > skill.skillData.maxDistance)
                //    continue;

                if (!nearestEnemy)
                {                    
                    nearestEnemy = enemy;
                    continue;                    
                }
                else
                {
                    //이전 대상 보다 가까우면 교체                    
                    if (nearestEnemy.GetDistanceFrom(owner) > distance)
                        nearestEnemy = enemy;
                }
            }                
        }
        


        lastTarget = nearestEnemy;
        lastCollectTargetTime = Time.time;
        //lastDistanceFromTarget = distance;

        return nearestEnemy;
    }    
}

/// <summary> 가장 멀리 있는 적 </summary>
public class SkillCTCollectorFarthestEnemy : SkillCTCollectorBase
{
    //float lastDistanceFromTarget = -9999f;

    BattleHero farthestEnemy = null;
    SkillBase skill;

    public override BattleHero CollectTarget(SkillBase skill)
    {
        //이전 대상이 살아있는데, 검색 주기 전이면 새로 검색 안 함
        if ((lastTarget && !skill.skillData.canCastToDeadUnit && !lastTarget.isDie)
            && Time.time < lastCollectTargetTime + refreshInterval)
            return lastTarget;

        base.CollectTarget(skill);

        if (!owner.battleGroup)
            return null;

        if (owner.team == BattleUnit.Team.none || enemyList == null || enemyList.Count == 0)
            return null;

        farthestEnemy = null;
        float distance = -9999f;
        for (int i = 0; i < enemyList.Count; i++)
        {
            BattleHero enemy = enemyList[i];
            if (enemy == owner as BattleHero)
                continue;


            if (!skill.skillData.canCastToDeadUnit && enemy.isDie && enemy == owner)
                continue;

            if (!farthestEnemy)
            {
                distance = enemy.GetDistanceFrom(owner);
                if(distance >= skill.skillData.minDistance && distance <= skill.skillData.maxDistance)
                {
                    //공중 공격 불가 시 공중에 있는 애는 타겟으로 잡지 않음
                    if (skill.skillData.canCastToAir || (!skill.skillData.canCastToAir && enemy.skeletonAnimation.transform.localPosition.y < 3f))
                    {
                        farthestEnemy = enemy;
                        continue;
                    }
                }
            }
            else
            {
                distance = enemy.GetDistanceFrom(owner);
                if (farthestEnemy.GetDistanceFrom(owner) < distance && distance >= skill.skillData.minDistance && distance <= skill.skillData.maxDistance)
                {
                    //공중 공격 불가 시 공중에 있는 애는 타겟으로 잡지 않음
                    if (skill.skillData.canCastToAir || (!skill.skillData.canCastToAir && enemy.skeletonAnimation.transform.localPosition.y < 3f))
                        farthestEnemy = enemy;
                }                    
            }
            
        }

        lastTarget = farthestEnemy;
        lastCollectTargetTime = Time.time;
        //lastDistanceFromTarget = distance;

        return farthestEnemy;
    }
}

/// <summary> 가장 가까이 있는 아군 </summary>
public class SkillCTCollectorNearestAlly : SkillCTCollectorBase
{
    public override BattleHero CollectTarget(SkillBase skill)
    {
        ////이전 대상이 살아있는데, 검색 주기 전이면 새로 검색 안 함
        //if ((lastTarget && !skill.skillData.canCastToDeadUnit && !lastTarget.isDie)
        //    && Time.time < lastCollectTargetTime + refreshInterval)
        //    return lastTarget;

        base.CollectTarget(skill);

        if (!owner.battleGroup)
            return null;

        if (owner.team == BattleUnit.Team.none || allyList == null || allyList.Count == 0)
            return null;


        //임시. Todo: 구현해야 함
        lastTarget = allyList[0];
        lastCollectTargetTime = Time.time;

        return null;
    }


}


/// <summary> 가장 남은 체력의 비율이 적은 아군 </summary>
public class SkillCTCollectorLowestHPAlly : SkillCTCollectorBase
{
    public override BattleHero CollectTarget(SkillBase skill)
    {
        ////이전 대상이 살아있는데, 검색 주기 전이면 새로 검색 안 함
        //if ((lastTarget && !skill.skillData.canCastToDeadUnit && !lastTarget.isDie)
        //    && Time.time < lastCollectTargetTime + refreshInterval)
        //    return lastTarget;

        base.CollectTarget(skill);

        if (!owner.battleGroup)
            return null;

        if (owner.team == BattleUnit.Team.none || allyList == null || allyList.Count == 0)
            return null;


        BattleHero LowestHPAlly = null;
        for (int i = 0; i < allyList.Count; i++)
        {
            BattleHero ally = allyList[i];

            if (!skill.skillData.canCastToDeadUnit && ally.isDie)
                continue;

            if (!LowestHPAlly)
            {
                LowestHPAlly = ally;                
                continue;
            }

            if (LowestHPAlly.curHP / LowestHPAlly.maxHP > ally.curHP / ally.maxHP)
                LowestHPAlly = ally;
        }

        lastTarget = LowestHPAlly;
        lastCollectTargetTime = Time.time;

        return LowestHPAlly;
    }
}


public class SkillCTFrontMostHero : SkillCTCollectorBase
{
    public override BattleHero CollectTarget(SkillBase skill)
    {
        ////이전 대상이 살아있는데, 검색 주기 전이면 새로 검색 안 함
        //if ((lastTarget && !skill.skillData.canCastToDeadUnit && !lastTarget.isDie)
        //    && Time.time < lastCollectTargetTime + refreshInterval)
        //    return lastTarget;

        base.CollectTarget(skill);

        if (!owner.battleGroup)
            return null;

        if (owner.team == BattleUnit.Team.none || allyList == null || allyList.Count == 0)
            return null;
        

        lastTarget = owner.battleGroup.frontMostHero;
        //if (lastTarget != null)
        //{
        //    Debug.Log("타겟 없음");

        //    lastTarget = owner as BattleHero;

        //}
        lastCollectTargetTime = Time.time;

        return lastTarget;
    }
}

public class SkillCTHighestAttackPowerEnemy : SkillCTCollectorBase
{
    BattleHero highestAttackPowerEnemy = null;
    SkillBase skill;

    public override BattleHero CollectTarget(SkillBase skill)
    {
        this.skill = skill;

        ////이전 대상이 살아있는데, 검색 주기 전이면 새로 검색 안 함
        //if (lastTarget && !skill.skillData.canCastToDeadUnit && lastTarget.isDie)
        //{
        //}
        //else
        //{
        //    if (Time.time < lastCollectTargetTime + refreshInterval)
        //        return lastTarget;
        //}

        base.CollectTarget(skill);

        if (!owner.battleGroup)
            return null;

        if (owner.team == BattleUnit.Team.none || enemyList == null || enemyList.Count == 0)
            return null;

        highestAttackPowerEnemy = null;

        for (int i = 0; i < enemyList.Count; i++)
        {
            BattleHero enemy = enemyList[i];

            if (enemy == owner as BattleHero)
                continue;

            if (!skill.skillData.canCastToDeadUnit && enemy.isDie && enemy == owner)
                continue;

            if (!enemy.isDie)
            {
                if (highestAttackPowerEnemy == null)
                {

                    highestAttackPowerEnemy = enemy;
                
                }
                else
                {
                    if (highestAttackPowerEnemy.stats.GetParam<ModifiableStat>(StatType.AttackPower).value < enemy.stats.GetParam<ModifiableStat>(StatType.AttackPower).value)
                    {
                        highestAttackPowerEnemy = enemy;
                    }
                }
            }
        }

        lastTarget = highestAttackPowerEnemy;
        lastCollectTargetTime = Time.time;
        return lastTarget;
    }
}

public class SkillCTCollectorAirTarget : SkillCTCollectorBase
{
    BattleHero airEnemy = null;
    SkillBase skill;

    public override BattleHero CollectTarget(SkillBase skill)
    {
        this.skill = skill;

        ////이전 대상이 살아있는데, 검색 주기 전이면 새로 검색 안 함
        //if (lastTarget && !skill.skillData.canCastToDeadUnit && lastTarget.isDie)
        //{
        //}
        //else
        //{
        //    if (Time.time < lastCollectTargetTime + refreshInterval)
        //        return lastTarget;
        //}

        base.CollectTarget(skill);

        if (!owner.battleGroup)
            return null;

        if (owner.team == BattleUnit.Team.none || enemyList == null || enemyList.Count == 0)
            return null;
        
        airEnemy = null;
        

        for (int i = 0; i < enemyList.Count; i++)
        {
            BattleHero enemy = enemyList[i];

            if (enemy == owner as BattleHero)
                continue;

            if (!skill.skillData.canCastToDeadUnit && enemy.isDie && enemy == owner)
                continue;

            if (enemy.airborne || owner.heroData.baseData.type == HeroData.HeroBattleType.Air)
            {
                if (!enemy.isDie)
                {
                    if (airEnemy == null)
                    {
                        airEnemy = enemy;

                    }
                }
            }
        }

        lastTarget = airEnemy;
        lastCollectTargetTime = Time.time;
        return lastTarget;
    }
}