using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using System;

public interface IMoveable
{
    BattleHero owner { get; set; }
    BattleHero castTarget { get; set; }
    void Move();
}

/// <summary> MoveTowards로 서서히 접근 </summary>
public class HeroMoveBehaviorMoveTowards : IMoveable
{
    public BattleHero owner { get; set; }
    public SkillBase skill { get; set; }

    public BattleHero castTarget { get; set; }
    public void Move()
    {
        if (!skill || !owner || owner.isDie || owner.isBlockMove || !skill.castTarget)
            return;

        if (skill.skillData.moveSpeed != 0f)
        {
            BattleHero u = skill.castTarget;
            
            owner.transform.position = Vector3.MoveTowards(owner.transform.position, new Vector3(u.transform.position.x, u.transform.position.y, owner.transform.position.z), skill.skillData.moveSpeed * Time.deltaTime);
        }
    }
}

public class HeroMoveBehaviorRun : IMoveable
{
    public BattleHero owner { get; set; }

    public BattleHero castTarget { get; set; }

    float lastMoveTime;

    public void Move()
    {
        if (!owner.battleGroup)
            return;

        if (owner.isDie)
            return;

        //지면 상태 체크
        if (owner.originalPosY < 0.1f &&
            (owner.skeletonAnimation.transform.localPosition.y > 0.3f || owner.skeletonAnimation.transform.localPosition.y < -0.3f))
            return;

        BattleHero.SkeletonAnimState animState = owner.skeletonAnimState;

        //걸어갈 목적지. 기본적으로 골인 지점이고, 적이 있다면 가장 가까이 있는 적이 목적지
        Vector3 moveDestination = owner.transform.position;

        if (owner.team == BattleUnit.Team.Red)
            moveDestination = owner.transform.position + Vector3.right * 10f;
            else if (owner.team == BattleUnit.Team.Blue)
                moveDestination = owner.transform.position + Vector3.left * 10f;
        
        if (castTarget)
            moveDestination = castTarget.transform.position;
        else
        {
            BattleHero nearestEnemy = null;

            if (owner.team == BattleUnit.Team.Red)
                nearestEnemy = owner.battleGroup.frontMostMonster;
            else if (owner.team == BattleUnit.Team.Blue)
                nearestEnemy = owner.battleGroup.frontMostHero;

            if(nearestEnemy && (owner.defaultSkill.skillData.canCastToAir || nearestEnemy.transform.localPosition.y < 3f))
                moveDestination = nearestEnemy.transform.position;
            else
            {
                CustomList<BattleHero> enemyList = null;
                if (owner.team == BattleUnit.Team.Red)
                    enemyList = owner.battleGroup.blueTeamList;
                else if (owner.team == BattleUnit.Team.Red)
                    enemyList = owner.battleGroup.redTeamList;

                BattleHero nearestEnemyOnGround = enemyList != null ? enemyList.Find(x => x.gameObject.activeSelf && !x.isDie && x.skeletonAnimation.transform.localPosition.y < 3f) : null;

                if (nearestEnemyOnGround)
                    moveDestination = nearestEnemyOnGround.transform.position;
                else if (nearestEnemy)
                    moveDestination = nearestEnemy.transform.position;
            }
        }
        
        
        //대상 바라보기
        if (!owner.isBlockMove && (owner.skeletonAnimState == BattleHero.SkeletonAnimState.Idle || owner.skeletonAnimState == BattleHero.SkeletonAnimState.Run))
            owner.flipX = moveDestination.x + 0.1f < owner.transform.position.x;

        //접근거리 계산
        float approachDistanceMaxTemp = owner.defaultSkill.skillData.maxDistance * 0.7f;
        if (owner.orderController)
            approachDistanceMaxTemp = approachDistanceMaxTemp * (1f + owner.orderController.scaleRatio);

        if (approachDistanceMaxTemp < 0.1f)
            approachDistanceMaxTemp = 0.1f;
        
        if (owner.isBlockAttack)
        {
            if (animState == BattleHero.SkeletonAnimState.Skill)
                owner.skeletonAnimation.state.SetAnimation(0, owner.idleAnimation, true);
        }

        if (owner.isBlockMove)
        {
            if (animState != BattleHero.SkeletonAnimState.Idle)
                owner.skeletonAnimation.state.SetAnimation(0, owner.idleAnimation, true);

            return;
        }

        if(owner.airborne)
        {
            if (animState != BattleHero.SkeletonAnimState.Idle)
                owner.skeletonAnimation.state.SetAnimation(0, owner.idleAnimation, true);

            return;
        }
        

        //x축으로 최대 접근 거리 이상일 경우에만 이동 애니함
        //y축으로 너무 멀면 보정해줌 (공격 거리, 접근거리 보다도 멀 경우)
        if ((moveDestination.x > owner.transform.position.x && moveDestination.x - owner.transform.position.x > approachDistanceMaxTemp * 0.95f)
            || (moveDestination.x < owner.transform.position.x && owner.transform.position.x - moveDestination.x > approachDistanceMaxTemp * 0.95f))
        {
            //이동 애니 재생. 넉백 중에는 이동 안 함. 대기 자세로 표현
            {
                if (animState != BattleHero.SkeletonAnimState.Run)
                {                    
                    owner.skeletonAnimation.state.SetAnimation(0, owner.spineAnimationRun, true);
                    if (owner.mixedAnimation.Count > 0)
                        owner.skeletonAnimation.state.SetAnimation(1, owner.mixedAnimation[0], true);
                }
                else
                {
                    if (owner.addedForce != Vector3.zero)
                        owner.skeletonAnimation.state.SetAnimation(0, owner.spineAnimationIdle, true);
                }
            }
        }
        else
        {
            if (animState != BattleHero.SkeletonAnimState.Idle)
            {
                owner.skeletonAnimation.state.SetAnimation(0, owner.spineAnimationIdle, true);
                if (owner.mixedAnimation.Count > 0)
                    owner.skeletonAnimation.state.SetAnimation(1, owner.mixedAnimation[0], true);
            }
        }
        
    }
}
