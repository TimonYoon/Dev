using UnityEngine;
using System.Collections;
using Spine.Unity;
using System.Collections.Generic;
using System;

public class SkillSetting : MonoBehaviour
{
    [SerializeField]
    public SkeletonAnimation skeletonAnimation;

    [SerializeField]
    public string skillID;


    [SerializeField]
    [Header("Animation")]
    [SpineAnimation(dataField: "skeletonAnimation")]
    public string animationName;

    public Collider2D skillRange;

    [SerializeField]
    SkillEventContainer skillEventContainer = new SkillEventContainer();

    public List<SkillEvent> skillEventList
    {
        get
        {
            if (skillEventContainer == null)
                return null;

            return skillEventContainer.skillEventList;
        }
    }

    Spine.Animation _animation;
    new public Spine.Animation animation
    {
        get
        {
            if (_animation != null)
                return _animation;

            if (!skeletonAnimation || string.IsNullOrEmpty(animationName))
                return null;

            _animation = skeletonAnimation.SkeletonDataAsset.GetAnimationStateData().SkeletonData.FindAnimation(animationName);

            return _animation;
        }
        
    }

    void Start()
    {
        if (!skeletonAnimation)
            return;

        Spine.Animation anim = skeletonAnimation.Skeleton.Data.FindAnimation(animationName);
    }
}