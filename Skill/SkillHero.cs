using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class SkillHero : SkillBase
{
    BattleHero _battleHeroCache = null;
    protected BattleHero battleHero
    {
        get
        {
            if (!_battleHeroCache)
                _battleHeroCache = owner as BattleHero;

            return _battleHeroCache;
        }
    }

    Spine.Unity.SkeletonAnimation _skeletonAnimation = null;
    protected Spine.Unity.SkeletonAnimation skeletonAnimation
    {
        get
        {
            if (skillSetting)
                return skillSetting.skeletonAnimation;
            else
                return _skeletonAnimation;
        }
        set
        {
            _skeletonAnimation = value;
        }
    }


    protected override void Start()
    {
        base.Start();

        //skeletonAnimation이 없어도 스킬은 쓸 수 있지 않을까..
        if (!skeletonAnimation)
            Debug.LogWarning(gameObject.name + ". Not found skeleton animation");

        if (skeletonAnimation)
        {
            skeletonAnimation.state.Event += OnEvent;
            skeletonAnimation.state.End += OnEndAnimation;
            skeletonAnimation.state.Start += OnStartAnimation;
            skeletonAnimation.state.Interrupt += OnInterruptAnimation;
        }

        isInitialized = true;
    }



    //다른 애니로 바뀌었을 때 발생
    virtual protected void OnInterruptAnimation(Spine.TrackEntry trackEntry)
    {
        //if (owner.gameObject.name == "CatFighter_01" && owner.team == BattleUnit.Team.Red)
        //    Debug.Log("Interrupt " + trackEntry);

        if (!isInAction)
            return;

        //해당 스킬 종료 된 것
        if(trackEntry.animation.name == skillSetting.animationName)
        {
            isInAction = false;

            if (onFinish != null)
                onFinish(this);
        }
    }

    bool isInAction = false;

    virtual protected void OnStartAnimation(Spine.TrackEntry trackEntry)
    {
        //if (owner.gameObject.name == "CatFighter_01" && owner.team == BattleUnit.Team.Red)
        //    Debug.Log("Start " + trackEntry);

        //해당 스킬 시작 된 것
        if (trackEntry.animation.name == skillSetting.animationName)
        {
            isInAction = true;

            if (onStart != null)
                onStart(this);
        }
    }
    

    override public void CheckCastCondition()
    {
        base.CheckCastCondition();

        if (!skeletonAnimation)
            return;

        //canExecute = true;
        //Idle상태 무시 스킬 아니면 아이들 상태 체크한다.
        if (!skillData.ignoreIdle && owner is BattleHero)
        {
            BattleHero battleHero = owner as BattleHero;
            if (battleHero && battleHero.skeletonAnimation)
            {
                //걷거나 대기 동작 중이 아니면 Idle 아님
                if (battleHero.skeletonAnimation.AnimationName != battleHero.idleAnimation
                    && battleHero.skeletonAnimation.AnimationName != battleHero.runAnimation)
                {
                    canCastSkill = false;
                    return;
                }

            }
        }
    }

    public override void Execute()
    {
        //아직 스폰 안되어 있으면 실행 안 함        
        if (battleHero && !battleHero.isFinishSpawned)
            return;

        base.Execute();

        if (!skeletonAnimation)
            return;

        if(owner.notTargeting && owner.buffController.buffList.Find(x=>x.baseData.notTargeting == true) != null)
        {
            Buff buff = owner.buffController.buffList.Find(x => x.baseData.notTargeting == true);

            Debug.Log(buff.baseData.id);
            owner.buffController.DetachBuff(buff);
        }

        if (skillData.id == "Skill_Gargoyle_Passive_ResistMagicalDamage")
            Debug.Log("asdf");

        if (string.IsNullOrEmpty(skillSetting.animationName))
        {
            if (castTarget && castTarget != owner)
            {
                //적을 향해서 몸 뒤집기. 적이 여럿일 경우... 그냥 첫번째? 타겟
                battleHero.flipX = castTarget.transform.position.x < transform.position.x;
            }

            return;
        }

        if (moveBehavior != null && battleHero.currentMoveBehavior != moveBehavior)
            battleHero.currentMoveBehavior = moveBehavior;
            

        //영웅 스킬의 경우 스킬이 실행됨에 따라 애니메이션 실행이 될 수도 있음
        //애니메이션 재생
        int track = skillData.track;
        if (skeletonAnimation.state.GetCurrent(track) == null ||
            (skeletonAnimation.state.GetCurrent(track) != null && skeletonAnimation.state.GetCurrent(track).Animation.Name != skillSetting.animationName))
        {
            bool loop = false;
            if (skillData.duration > 0f)
                loop = true;

            if (castTarget && castTarget != owner)
            {
                //if(skillData.id != "Skill_BirdSpear_DiveAttack")
                    battleHero.flipX = castTarget.transform.position.x < transform.position.x;
            }
            else if (targetList == null)
            {
                Debug.LogWarning("target list is null - " + skillData.id);
            }
            else if (targetList.Count > 0 && targetList[0] != owner)
            {
                //적을 향해서 몸 뒤집기. 적이 여럿일 경우... 그냥 첫번째? 타겟
                //if (skillData.id != "Skill_BirdSpear_DiveAttack")
                    //battleHero.flipX = targetList[0].transform.position.x < transform.position.x;
            }

            skeletonAnimation.state.SetAnimation(track, skillSetting.animation/*.animationName*/, loop);

            if (this is SkillDive) { }
            else
                skeletonAnimation.AnimationState.AddAnimation(track, battleHero.idleAnimation, true, skillData.duration);
        }
    }
    
    virtual public void OnEndAnimation(Spine.TrackEntry trackEntry)
    {
        if (!owner || owner.battleGroup == null || owner.isDie)
            return;

        if (this is SkillDive)
            return;

        return;

        //적 전멸했으면 idle
        if (owner.battleGroup.battlePhase == BattleGroup.BattlePhase.Finish)
        {
            if (battleHero.skeletonAnimState == BattleHero.SkeletonAnimState.Skill)
            {
                skeletonAnimation.state.SetAnimation(0, battleHero.idleAnimation, true);
            }
        }   
    }

    public void OnEvent(Spine.TrackEntry trackEntry, Spine.Event _event)
    {
        if (skeletonAnimation.state.GetCurrent(0).Animation.Name != skillSetting.animationName)
            return;

        if (skillEffectDic.ContainsKey(_event.Data.name))
        {
            List<ISkillEffect> skillEventList = skillEffectDic[_event.Data.name];
            for (int i = 0; i < skillEventList.Count; i++)
            {
                skillEventList[i].TriggerEffect();
            }
        }
    }

}
