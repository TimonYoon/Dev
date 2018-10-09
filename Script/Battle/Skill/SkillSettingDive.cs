using UnityEngine;
using System.Collections;
using Spine.Unity;
using System.Collections.Generic;
using System;

public class SkillSettingDive : SkillSetting
{
    [Header("시작 속도")]
    public float startSpeed = 0f;

    [Header("가속도")]
    public float acc = 4f;
    
    [Header("이동 궤적 보정")]
    public AnimationCurve curveStartX = AnimationCurve.Linear(0f, 0f, 1f, 0f);
    public AnimationCurve curveStartY = AnimationCurve.Linear(0f, 0f, 1f, 0f);

    [Header("목표 지점 보정")]
    public Vector2 destPosOffset = Vector2.zero;

    [Header("원래 자리 돌릴지 여부")]
    public bool backOriginalPos = false;

    [SerializeField]
    [Header("이동 궤적 방향별 애니")]
    [SpineAnimation(dataField: "skeletonAnimation")]
    public string animationFoward;

    [SpineAnimation(dataField: "skeletonAnimation")]
    public string animationBack;

    [SpineAnimation(dataField: "skeletonAnimation")]
    public string animationUp;

    [SpineAnimation(dataField: "skeletonAnimation")]
    public string animationDown;

    [SpineAnimation(dataField: "skeletonAnimation")]
    public string animationFinish;

    [Header("최소 시간")]
    public float minDiveTime = 0.5f;

    [Header("종료 애니 발생 시점 조절. 목표 지점에 도달하는 시간을 기준으로 함.")]
    public float finishAnimStartOffset = -0.5f;

    [Header("완료 후 이동 궤적 연장 여부. 공중에서의 부드러운 강습 연출을 할 때 필요")]
    public bool isNeedExpandPath = false;
}
