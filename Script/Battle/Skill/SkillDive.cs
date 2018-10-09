using System.Collections;
using System.Collections.Generic;
using Spine;
using UnityEngine;

public class SkillDive : SkillHero
{
    SkillSettingDive skillSettingDive;

    enum MoveDirection
    {
        Foward,
        Back,
        Up,
        Down
    }

    enum DiveState
    {
        MoveToDest,
        MoveToStartPos,
        Finish
    }

    //디버깅용
    DiveState diveState = DiveState.Finish;
    

    void OnEnable()
    {
        if(battleHero)
            battleHero.skeletonAnimation.transform.localPosition = originalSkeletonPos;

        diveState = DiveState.Finish;
        if (coDive != null)
        {
            StopCoroutine(coDive);
            coDive = null;
        }
    }

    protected override void Start()
    {
        originalSkeletonPos = battleHero.skeletonAnimation.transform.localPosition;

        base.Start();        

        owner.onDie += OnOwnerDie;
        if(owner.battleGroup != null)
        {
            owner.battleGroup.onChangedStage += OnChangedStage;
            owner.battleGroup.onRestartBattle += OnRestartedBattle;
        }
    }

    void OnChangedStage(BattleGroup b)
    {
        diveState = DiveState.Finish;
        if (coDive != null)
        {
            StopCoroutine(coDive);
            coDive = null;
        }
    }

    void OnRestartedBattle(BattleGroup b)
    {
        if (battleHero)
            battleHero.skeletonAnimation.transform.localPosition = originalSkeletonPos;

        diveState = DiveState.Finish;
        if (coDive != null)
        {
            StopAllCoroutines();
            coDive = null;
        }
    }

    void OnOwnerDie(BattleUnit unit)
    {
        //battleHero.skeletonAnimation.transform.localPosition = originalSkeletonPos;
        battleHero.skeletonAnimation.transform.localEulerAngles = Vector3.zero;

        diveState = DiveState.Finish;

        if (coDive != null)
        {
            StopAllCoroutines();
            coDive = null;
        }   
    }

    Vector3 backPoint;

    public override void Execute()
    {
        if (coDive != null)
            return;

        skillSettingDive = skillSetting as SkillSettingDive;

        startDiveTime = Time.time;

        //다시 돌아가기 예외처리(사무라이 스킬)
        if(castTarget == null)
        {
            backPoint = owner.skillStartPos;
            needFlip = backPoint.x < owner.transform.position.x;
        }
        else
        {
            needFlip = castTarget.transform.position.x < owner.transform.position.x;
        }
        

        if (skillSettingDive.animationName != skillSettingDive.animationFoward)
            skillSettingDive.animationName = skillSettingDive.animationFoward;

        base.Execute();

        if(skillData.effectType == "Trail")
        {
            owner.skillStartPos = skeletonAnimation.transform.position;
            owner.GetComponentInChildren<EffectTrail>().SetStartPoint();
            
        }
        //return;
        //skeletonAnimation.AnimationState.ClearTrack(0);
        //skillSettingDive.animationName = skillSettingDive.animationFoward;
        skeletonAnimation.state.SetAnimation(0, skillSettingDive.animation/*.animationName*/, true);

        coDive = StartCoroutine(Dive());
    }

    public bool needFlip { get; set; }

    public float startDiveTime { get; set; }

    float flipCoolTime = 0.1f;
    float lastFlipTime = 0f;
    bool _isFlip = false;
    bool isFlip
    {
        get { return _isFlip; }
        set
        {
            if (Time.time < lastFlipTime + flipCoolTime)
                return;

            if (skeletonAnimation.state.GetCurrent(0).Animation.Name == skillSettingDive.animationFinish)
                return;

            lastFlipTime = Time.time;

            _isFlip = value;
        }
    }


    public Vector3 originalSkeletonPos { get; set; }

    Coroutine coDive = null;

    float animValue = 0f;
    IEnumerator Dive()
    {
        diveState = DiveState.MoveToDest;

        //battleHero.skeletonAnimation.transform.localPosition = originalSkeletonPos;

        Vector3 startPos = battleHero.transform.position;

        bool isFlipPreCheck = false;
        if (castTarget == null)
        {
            isFlipPreCheck = battleHero.transform.position.x > backPoint.x;
        }
        else
        {
            isFlipPreCheck = battleHero.transform.position.x > castTarget.transform.position.x;
        }
        
        //x축 이동은 캐릭터 통채로

        float destPosOffsetX = skillSettingDive.destPosOffset.x;
        if (isFlipPreCheck)
            destPosOffsetX *= -1f;

        Vector2 destPos;
        if(castTarget == null)
        {
            destPos = backPoint;
        }
        else
        {
            destPos = (Vector2)castTarget.transform.position + Vector2.right * destPosOffsetX;
        }
        
        destPos = new Vector2(Mathf.Clamp(destPos.x, owner.battleGroup.spwonPointXMin + 5f, owner.battleGroup.spwonPointXMax - 5f)
            , Mathf.Clamp(destPos.y, owner.battleGroup.spwonPointYMin, owner.battleGroup.spwonPointYMax));

        float distance = Vector2.Distance(battleHero.skeletonAnimation.transform.position, destPos);
        

        /*float */animValue = 0f;
        //float distance = castTarget.GetDistanceFrom(battleHero);
        float velocity = skillSettingDive.startSpeed;
        float goalTime = distance / velocity;//  (distance / 4f);
        if (goalTime < skillSettingDive.minDiveTime)
            goalTime = skillSettingDive.minDiveTime;

        float finishTime = goalTime + skillSettingDive.finishAnimStartOffset;


        //destPos = skillData.canCastToAir && castTarget.collider ? castTarget.collider.transform.position : castTarget.transform.position;
        //destPos = new Vector3(Mathf.Clamp(destPos.x, owner.battleGroup.xMin, owner.battleGroup.xMax), destPos.y, destPos.z);
        
        float startPosY = battleHero.skeletonAnimation.transform.localPosition.y;

        bool triggeredOnDive = false;
        bool changeIdleAtFinish = true;

        if(skillData.effectType == "Trail")
        {
            owner.GetComponentInChildren<EffectTrail>().DrawTrail(true);
        }
        //강습 시작
        float deltaTime = 0f;
        float lastTime = Time.time;
        float startTime = Time.time;
        while (animValue < 1f)
        {
            //if (!castTarget)
            //{
            //    changeIdleAtFinish = true;
            //    break;
            //}

            if (owner.isBlockMove || owner.isBlockAttack)
                break;
            
            deltaTime = Time.time - lastTime;
            float elapsedTime = Time.time - startTime;

            velocity = Mathf.Max(velocity + skillSettingDive.acc * deltaTime, 0.01f);

            //destPos = (Vector2)castTarget.skeletonAnimation.transform.position + Vector2.right * destPosOffsetX;
            //destPos = new Vector2(Mathf.Clamp(destPos.x, owner.battleGroup.xMin + 5f, owner.battleGroup.xMax - 5f), destPos.y);

            //distance = Vector3.Distance(battleHero.collider.transform.position, destPos + (Vector2)skillSettingDive.destPosOffset);//  castTarget.GetDistanceFrom(battleHero);
            distance = Vector2.Distance(battleHero.skeletonAnimation.transform.position, destPos);
            //goalTime = (distance / velocity);
            //if (goalTime < skillSettingDive.minDiveTime)
            //    goalTime = skillSettingDive.minDiveTime;

            finishTime = goalTime + skillSettingDive.finishAnimStartOffset;

            //animValue = elapsedTime / goalTime;

            if (goalTime <= 0f)
                animValue = 1f;
            else
            {
                animValue = elapsedTime / goalTime;
                //animValue += deltaTime / goalTime;
            }

            //목표 지점이 왼쪽에 있으면 몸 뒤집기
            if (skeletonAnimation.AnimationName == skillSettingDive.animationName)
            {
                isFlip = battleHero.transform.position.x > destPos.x;
                battleHero.flipX = isFlip;
            }                

            //y축 이동 + y축 커브 보정
            float offsetY = skillSettingDive.curveStartY.Evaluate(animValue);
            Vector3 skeletonPos = battleHero.skeletonAnimation.transform.localPosition;
            float destLocalPosY = 0f;

            if (castTarget != null)
            {
                destLocalPosY = skillData.canCastToAir && castTarget ? castTarget.skeletonAnimation.transform.localPosition.y : 0f;
            }
            
            //if (castTarget.collider)
            //    destLocalPosY += castTarget.collider.transform.localPosition.y;
            float y = Mathf.Lerp(/*skeletonPos.y*/ startPosY, destLocalPosY + skillSettingDive.destPosOffset.y, animValue);
            y = y + offsetY;

            battleHero.skeletonAnimation.transform.localPosition = Vector3.up * y;

            float x = skillSettingDive.curveStartX.Evaluate(animValue);
            if (isFlipPreCheck)
                x *= -1f;

            //Vector3 pos = Vector3.Lerp(battleHero.transform.position, destPos, animValue);
            Vector3 pos = Vector3.Lerp(startPos, destPos, animValue);
            pos = new Vector3(pos.x + x, pos.y, pos.z);

            //battleHero.flipX = castTarget.transform.position.x < transform.position.x;

            if (elapsedTime > finishTime)
            {
                if (skeletonAnimation.AnimationName != skillSettingDive.animationFinish)
                {
                    if (!string.IsNullOrEmpty(skillSettingDive.animationFinish))
                    {
                        skillSettingDive.animationName = skillSettingDive.animationFinish;
                        skeletonAnimation.state.SetAnimation(0, skillSettingDive.animationFinish, false);

                        changeIdleAtFinish = false;
                    }
                }

                // 목표 지점 도달 event 발생
                if (triggeredOnDive == false && skillEffectDic.ContainsKey("OnDive"))
                {
                    triggeredOnDive = true;
                    //if(castTarget)
                    //    battleHero.flipX = castTarget.transform.position.x < transform.position.x;

                    List<ISkillEffect> skillEventList = skillEffectDic["OnDive"];
                    for (int i = 0; i < skillEventList.Count; i++)
                    {
                        skillEventList[i].TriggerEffect();
                    }

                    changeIdleAtFinish = false;
                }
            }

            battleHero.transform.position = pos;


            lastTime = Time.time;
            yield return null;
        }       


        //위치 정확하게 리셋
        //battleHero.skeletonAnimation.transform.localPosition = originalSkeletonPos;
        //battleHero.skeletonAnimation.transform.localEulerAngles = Vector3.zero;
        
        //skeletonAnimation.AnimationState.ClearTrack(0);
        while (skeletonAnimation.AnimationName == skillSettingDive.animationFinish)
            yield return null;

        // 목표 지점 도달 event 발생
        if (changeIdleAtFinish)
            skeletonAnimation.AnimationState.SetAnimation(0, battleHero.idleAnimation, true);

        if (skillSettingDive.backOriginalPos)
            transform.position = startPos;

        diveState = DiveState.Finish;
        coDive = null;
    }
    
    protected override void OnStartAnimation(TrackEntry trackEntry)
    {
        //해당 스킬 시작 된 것
        if (trackEntry.animation.name == skillSetting.animationName)
        {
            if (onStart != null)
                onStart(this);
        }
    }

    override protected void OnInterruptAnimation(Spine.TrackEntry trackEntry)
    {
        //if (diveState != DiveState.Finish)
        //    return;

        //해당 스킬 종료 된 것
        if (skillSettingDive && trackEntry.animation.name == skillSettingDive.animationFinish)
        {
            if (onFinish != null)
                onFinish(this);
        }        
    }

    //override protected void OnStartAnimation(Spine.TrackEntry trackEntry)
    //{
    //    return;

    //    //if (owner.gameObject.name == "CatFighter_01" && owner.team == BattleUnit.Team.Red)
    //    //    Debug.Log("Start " + trackEntry);

    //    ////해당 스킬 시작 된 것
    //    //if (trackEntry.animation.name == skillSetting.animationName)
    //    //{
    //    //    isInAction = true;

    //    //    if (onStart != null)
    //    //        onStart(this);
    //    //}
    //}


    /// <summary> 이동 궤적에 따라서 동작 다르게 표현 </summary>
    /// <param name="pos"> 궤적에 있는 이동 목표 지점. 공격 대상을 의미하는 것이 아님 </param>
    /// <param name="isFlip">몸 뒤집혔는지 여부</param>
    void UpdateAnimByDirection(Vector3 pos, bool isFlip)
    {
        MoveDirection moveDiction = MoveDirection.Foward;

        if (!isFlip)
        {
            if (pos.x < battleHero.transform.position.x)
                moveDiction = MoveDirection.Back;
            else
                moveDiction = MoveDirection.Foward;
        }
        else
        {
            if (pos.x < battleHero.transform.position.x)
                moveDiction = MoveDirection.Foward;
            else
                moveDiction = MoveDirection.Back;
        }
        


        if (moveDiction == MoveDirection.Back)
        {
            if (!string.IsNullOrEmpty(skillSettingDive.animationBack))
            {
                skillSettingDive.animationName = skillSettingDive.animationBack;
            }
        }
        else if (moveDiction == MoveDirection.Foward)
        {
            if (!string.IsNullOrEmpty(skillSettingDive.animationFoward))
                skillSettingDive.animationName = skillSettingDive.animationFoward;
        }
    }

    override public void OnEndAnimation(Spine.TrackEntry trackEntry)
    {

        //if (owner.battleGroup == null)
        //    return;

        //if (trackEntry == null || trackEntry.Animation == null || skillSettingDive == null)
        //    return;

        //if (trackEntry.Animation.Name == skillSettingDive.animationFinish)
        //{
        //    skeletonAnimation.state.SetAnimation(0, skillSettingDive.animationFoward, true);
        //}


        //base.OnEndAnimation(trackEntry);
    }
}
