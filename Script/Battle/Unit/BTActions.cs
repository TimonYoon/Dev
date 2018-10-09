using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace AI.Battle
{
    [TaskCategory("Battle")]    
    public class IsAlive : Conditional
    {
        public SharedBattleHero hero;

        public override TaskStatus OnUpdate()
        {
            if (!hero.Value.isDie)
                return TaskStatus.Success;
            else
                return TaskStatus.Failure;
        }
    }

    [TaskCategory("Battle")]
    public class IsIdle : Conditional
    {
        public SharedBattleHero hero;

        public override TaskStatus OnUpdate()
        {
            if (hero.Value.skeletonAnimState == BattleHero.SkeletonAnimState.Idle || hero.Value.skeletonAnimState == BattleHero.SkeletonAnimState.Run)
            {
                if (hero.Value.skeletonAnimation.transform.localPosition.y <= hero.Value.originalPosY * 1.2f
                    && hero.Value.skeletonAnimation.transform.localPosition.y >= hero.Value.originalPosY * 0.8f)
                    return TaskStatus.Success;
                else
                    return TaskStatus.Failure;
            }                
            else
                return TaskStatus.Failure;
        }
    }

    [TaskCategory("Battle")]
    public class IsFinishSpawned : Conditional
    {
        public SharedBattleHero owner;
        public override TaskStatus OnUpdate()
        {
            if(owner.Value && owner.Value.isFinishSpawned)
                return TaskStatus.Success;
            else
                return TaskStatus.Failure;
        }
    }

    [TaskCategory("Battle")]
    public class IsSummoned : Conditional
    {
        public SharedBattleHero owner;
        public override TaskStatus OnUpdate()
        {
            if (owner.Value && owner.Value.isSummonded)
                return TaskStatus.Success;
            else
                return TaskStatus.Failure;
        }
    }

    [TaskCategory("Battle")]
    public class IsNotCoolTime : Conditional
    {
        public SharedSkillBase skillBase;
        
        public override TaskStatus OnUpdate()
        {
            if (skillBase.Value.isCoolTime)
                return TaskStatus.Failure;
            else
                return TaskStatus.Success;
        }
    }

    [TaskCategory("Battle")]
    public class IsTargetInRange : Conditional
    {
        public SharedBattleHero castTarget;
        public SharedSkillBase skillBase;

        public override TaskStatus OnUpdate()
        {
            if (castTarget.Value/* && skillBase.Value.skillSetting.skillRange*/)
            {
                if (skillBase.Value.IsCastTargetInSkillRange())
                    return TaskStatus.Success;
                else
                    return TaskStatus.Failure;
            }
            else
                return TaskStatus.Failure;
        }
    }

    [TaskCategory("Battle")]
    public class IsAutoExecute : Conditional
    {
        public SharedSkillBase skillBase;

        public override TaskStatus OnUpdate()
        {
            if(skillBase.Value && skillBase.Value.skillData != null && skillBase.Value.skillData.autoExecute)
                return TaskStatus.Success;
            else
                return TaskStatus.Failure;            
        }
    }

    [TaskCategory("Battle")]
    public class canCastToAir : Conditional
    {
        public SharedSkillBase desiredSkill;

        public override TaskStatus OnUpdate()
        {
            if(desiredSkill.Value.skillData.canCastToAir)
                return TaskStatus.Success;
            else
                return TaskStatus.Failure;
        }
    }

    [TaskCategory("Battle")]
    public class canIgnoreIdle : Conditional
    {
        public SharedSkillBase desiredSkill;
        public override TaskStatus OnUpdate()
        {
            if (desiredSkill.Value.skillData.ignoreIdle)
                return TaskStatus.Success;
            else
                return TaskStatus.Failure;
        }
    }

    [TaskCategory("Battle")]
    public class IsTargetInAir : Conditional
    {
        public SharedBattleHero castTarget;

        public override TaskStatus OnUpdate()
        {
            if(castTarget.Value.skeletonAnimation.transform.localPosition.y > 3f)
                return TaskStatus.Success;
            else
                return TaskStatus.Failure;
        }
    }

    [TaskCategory("Battle")]
    public class IsBlockAttack : Conditional
    {
        public SharedBattleHero hero;

        public override TaskStatus OnUpdate()
        {
            if (hero.Value.isBlockAttack)
                return TaskStatus.Success;
            else
                return TaskStatus.Failure;
        }
    }

    [TaskCategory("Battle")]
    public class IsBlockMove : Conditional
    {
        public SharedBattleHero hero;

        public override TaskStatus OnUpdate()
        {
            if (hero.Value.isBlockMove)
                return TaskStatus.Success;
            else
                return TaskStatus.Failure;
        }
    }

    [TaskCategory("Battle")]
    public class IsValidCondtion : Conditional
    {
        public SharedBattleHero castTarget;
        public SharedSkillBase desiredSkill;

        public override TaskStatus OnUpdate()
        {
            if(desiredSkill.Value.IsValidCondition(castTarget.Value, desiredSkill.Value.skillData.castCondition))
                return TaskStatus.Success;
            else
                return TaskStatus.Failure;
        }
    }

    [TaskCategory("Battle")]
    public class IsFinishSkill : Conditional
    {
        public SharedBattleHero owner;
        //public SharedSkillBase desiredSkill;

        public override TaskStatus OnUpdate()
        {
            if (owner.Value.skeletonAnimState != BattleHero.SkeletonAnimState.Skill)
                return TaskStatus.Success;
            else
                return TaskStatus.Failure;
        }
    }

}

namespace AI.Battle
{

    [System.Serializable]
    public class SharedSkillBase : SharedVariable<SkillBase>
    {
        public static implicit operator SharedSkillBase(SkillBase value) { return new SharedSkillBase { Value = value }; }
    }

    [System.Serializable]
    public class SharedBattleHero : SharedVariable<BattleHero>
    {
        public static implicit operator SharedBattleHero(BattleHero value) { return new SharedBattleHero { Value = value }; }
    }



    [TaskCategory("Battle")]
    public class Init : Action
    {
        public SharedBattleHero owner;

        public SharedInt skillCount;

        public override void OnAwake()
        {
            if(!owner.Value)
                owner.Value = Owner.GetComponent<BattleHero>();
        }

        public override TaskStatus OnUpdate()
        {
            if (owner.Value.isInitializedSkillList)
            {
                skillCount.Value = owner.Value.skillList.Count;
                return TaskStatus.Success;
            }                
            else
                return TaskStatus.Failure;
        }
    }

    [TaskCategory("Battle")]
    public class SetDesiredSkillFromHighestPriority : Action
    {
        public SharedBattleHero hero;

        public SharedSkillBase desiredSkill;
        
        public SharedString desiredSkillName;

        public override TaskStatus OnUpdate()
        {

            desiredSkill.Value = null;
            for (int i = 0; i < hero.Value.skillList.Count; i++)
            {
                SkillBase skill = hero.Value.skillList[i];

                SkillData skillData = skill.skillData;

                //자동 발동 스킬 아니면 스킵
                if (!skillData.autoExecute)
                    continue;

                if (skill.isCoolTime)
                    continue;

                if (skill == hero.Value.defaultSkill)
                    continue;

                desiredSkill.Value = skill;
                break;
            }

            if (desiredSkill.Value)
            {
                desiredSkillName.Value = desiredSkill.Value.skillData.name;
                return TaskStatus.Success;
            }                
            else
            {                
                desiredSkillName.Value = string.Empty;
                return TaskStatus.Failure;
            }
                
        }
    }

    [TaskCategory("Battle")]
    public class SetDesiredSkillFromDefaultSkill : Action
    {
        public SharedBattleHero hero;

        public SharedSkillBase desiredSkill;

        public SharedString desiredSkillName;

        public override TaskStatus OnUpdate()
        {
            desiredSkill.Value = hero.Value.defaultSkill;

            if (desiredSkill.Value)
            {
                desiredSkillName.Value = desiredSkill.Value.skillData.name;
                return TaskStatus.Success;
            }
            else
            {
                desiredSkillName.Value = string.Empty;
                return TaskStatus.Failure;
            }   
        }
    }

    [TaskCategory("Battle")]
    public class SetDesiredSkill : Action
    {
        public SharedBattleHero hero;

        public SharedSkillBase desiredSkill;
        //public SharedSkillBase desiredSkillTemp;

        public SharedInt skillIndex;
        public SharedString desiredSkillName;

        public override TaskStatus OnUpdate()
        {
            BattleHero hero = this.hero.Value;

            desiredSkill.Value = hero.defaultSkill;

            for (int i = 0; i < hero.skillList.Count; i++)
            {
                if (!hero.skillList[i].skillData.autoExecute)
                    continue;

                if (hero.skillList[i].isCoolTime)
                    continue;

                if (!hero.skillList[i].skillData.ignoreIdle && hero.skeletonAnimState != BattleHero.SkeletonAnimState.Idle && hero.skeletonAnimState != BattleHero.SkeletonAnimState.Run)
                    continue;

                BattleHero castTarget = hero.skillList[i].CollectCastTarget(hero.skillList[i]);
                if (!castTarget)
                    continue;

                if (!hero.skillList[i].IsValidCondition(castTarget, hero.skillList[i].skillData.castCondition))
                    continue;

                desiredSkill.Value = hero.skillList[i];
                break;
            }
                        
            if (desiredSkill.Value)
            {
                desiredSkillName.Value = desiredSkill.Value.skillData.name;
            }
            return TaskStatus.Success;

        }
    }

    [TaskCategory("Battle")]
    public class SetDesiredSkill_bak : Action
    {
        public SharedBattleHero hero;

        public SharedSkillBase desiredSkill;
        //public SharedSkillBase desiredSkillTemp;

        public SharedInt skillIndex;
        public SharedString desiredSkillName;

        public override TaskStatus OnUpdate()
        {
            desiredSkill.Value = hero.Value.defaultSkill;

            if (hero.Value.skillList.Count < skillIndex.Value)
                return TaskStatus.Failure;

            desiredSkill.Value = hero.Value.skillList[skillIndex.Value];
            
            if (desiredSkill.Value)
            {
                desiredSkillName.Value = desiredSkill.Value.skillData.name;
                return TaskStatus.Success;
            }
            else
                return TaskStatus.Failure;


        }
    }


    [TaskCategory("Battle")]
    public class UpdateDesiredSkill : Action
    {
        public SharedBattleHero hero;

        public SharedSkillBase desiredSkill;

        public string defaultSkillName;
        public string desiredSkillName;

        public override TaskStatus OnUpdate()
        {

            desiredSkill.Value = null;
            for (int i = 0; i < hero.Value.skillList.Count; i++)
            {
                SkillBase skill = hero.Value.skillList[i];

                SkillData skillData = skill.skillData;

                //자동 발동 스킬 아니면 스킵
                if (!skillData.autoExecute)
                    continue;

                if (skill.isCoolTime)
                    continue;

                if (skill == hero.Value.defaultSkill)
                    continue;

                if(skill.castTarget && skill.IsCastTargetInSkillRange())
                {
                    desiredSkill.Value = skill;
                    break;
                }                    
            }

            if (!desiredSkill.Value)
                desiredSkill.Value = hero.Value.defaultSkill;

            if (desiredSkill.Value)
                desiredSkillName = desiredSkill.Value.skillData.name;

            defaultSkillName = hero.Value.defaultSkill.skillData.name;

            return TaskStatus.Success;
        }

    }


    [TaskCategory("Battle")]
    public class CollectCastTarget : Action
    {
        public SharedSkillBase desiredSkill;
        //public SharedSkillBase desiredSkillTemp;
        public SharedBattleHero castTarget;

        public override TaskStatus OnUpdate()
        {
            if (!desiredSkill.Value)
                return TaskStatus.Failure;

            //BattleHero h = desiredSkillTemp.Value.CollectCastTarget(desiredSkillTemp.Value);
            BattleHero h = desiredSkill.Value.CollectCastTarget(desiredSkill.Value);


            if (h != null)
            {
                castTarget.Value = h;
                //desiredSkill.Value = desiredSkillTemp.Value;
                return TaskStatus.Success;
            }                
            else
                return TaskStatus.Failure;
        }
    }

    [TaskCategory("Battle")]
    [TaskIcon("Assets/Effect/Flames/fireball/fireball_0001.png")]
    public class CastSkill : Action
    {
        public SharedSkillBase desiredSkill;

        public override TaskStatus OnUpdate()
        {
            desiredSkill.Value.Execute();
            return TaskStatus.Success;
        }
    }

    [TaskCategory("Battle")]
    public class LookTarget : Action
    {
        public SharedSkillBase desiredSkill;
        public SharedVector2 destPosition;
        public SharedBattleHero hero;


        public override TaskStatus OnUpdate()
        {

            BattleHero castTarget = desiredSkill.Value.castTarget;
            if (!castTarget)
                return TaskStatus.Failure;

            hero.Value.currentMoveBehavior = hero.Value.defaultMoveBehavior;
            hero.Value.currentMoveBehavior.Move();
            return TaskStatus.Success;

        }
    }

    [TaskCategory("Battle")]
    [TaskIcon("Assets/UI/IconArtifact/UiIconArtifact027.png")]
    public class Move : Action
    {
        //public SharedSkillBase desiredSkill;
        //public SharedVector2 destPosition;
        public SharedBattleHero hero;

        //public SharedBattleHero castTarget;

        public override TaskStatus OnUpdate()
        {
            //if (desiredSkill.Value)
            //    castTarget.Value = desiredSkill.Value.castTarget;

            //if (!castTarget.Value)
            //    return TaskStatus.Failure;

            //if (hero.Value.currentMoveBehavior != hero.Value.defaultMoveBehavior)
            //    hero.Value.currentMoveBehavior = hero.Value.defaultMoveBehavior;

            if (hero.Value.currentMoveBehavior != null)
                hero.Value.currentMoveBehavior.Move();

            return TaskStatus.Success;
        }
    }

    [TaskCategory("Battle")]
    [TaskIcon("Assets/UI/IconArtifact/UiIconArtifact027.png")]
    public class MoveToTarget : Action
    {
        public SharedSkillBase desiredSkill;
        public SharedVector2 destPosition;
        public SharedBattleHero hero;

        public SharedBattleHero castTarget;

        public override TaskStatus OnUpdate()
        {
            if (desiredSkill.Value)
                castTarget.Value = desiredSkill.Value.castTarget;

            //if (!castTarget.Value)
            //{
            //    if (hero.Value.team == BattleUnit.Team.Red)
            //        castTarget.Value = hero.Value.battleGroup.frontMostMonster;
            //    else if (hero.Value.team == BattleUnit.Team.Blue)
            //        castTarget.Value = hero.Value.battleGroup.frontMostHero;
            //}

            //if (!castTarget.Value)
            //    return TaskStatus.Failure;

            if(hero.Value.currentMoveBehavior != hero.Value.defaultMoveBehavior)
                hero.Value.currentMoveBehavior = hero.Value.defaultMoveBehavior;

            hero.Value.currentMoveBehavior.castTarget = castTarget.Value;
            hero.Value.currentMoveBehavior.Move();
            return TaskStatus.Success;
        }
    }

    [TaskCategory("Battle")]
    [TaskIcon("Assets/UI/IconArtifact/UiIconArtifact027.png")]
    public class SkillMove : Action
    {
        public SharedSkillBase desiredSkill;
        public SharedBattleHero hero;
        public override TaskStatus OnUpdate()
        {
            if (desiredSkill.Value && desiredSkill.Value.moveBehavior != null)
                desiredSkill.Value.moveBehavior.Move();
            
            return TaskStatus.Success;
        }
    }

    [TaskCategory("Battle")]    
    public class Stop : Action
    {
        public SharedBattleHero hero;

        public override TaskStatus OnUpdate()
        {   
            if (!hero.Value)
                return TaskStatus.Failure;

            if (hero.Value.skeletonAnimState != BattleHero.SkeletonAnimState.Idle)
                hero.Value.skeletonAnimation.state.SetAnimation(0, hero.Value.idleAnimation, true);                

            return TaskStatus.Success;
        }
    }
}
