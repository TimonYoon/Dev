using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Totem : BattleUnit
{
    public AnimationClip animSpawn;
    public AnimationClip animIdle;
    public AnimationClip animDespawn;

    new Animation animation;

    override protected void Awake()
    {
        base.Awake();

        animation = GetComponentInChildren<Animation>();
    }
    
    void OnDisable()
    {
        master = null;

        if (!battleGroup)
            return;

        battleGroup.onChangedBattlePhase -= OnChangedBattlePhase;
    }

    public void Init(BattleUnit _owner)
    {        
        master = _owner;
        team = _owner.team;

        SetBattleGroup(_owner.battleGroup);
        //battleGroup = _owner.battleGroup;

        if (!battleGroup)
            return;

        battleGroup.onChangedBattlePhase += OnChangedBattlePhase;
    }

    void OnChangedBattlePhase(BattleGroup battleGroup)
    {
        if (battleGroup.battlePhase == BattleGroup.BattlePhase.FadeOut
            || battleGroup.battlePhase == BattleGroup.BattlePhase.FadeIn)
            Despawn();
    }
    
    void OnEnable()
    {
        if (!master)
            return;


        StartCoroutine("DoSomething");
    }

    bool asdf = false;

    IEnumerator DoSomething()
    {
        asdf = true;

        if (animation)
        {
            animation.clip = animSpawn;
            animation.Play();
            animation.PlayQueued(animIdle.name, QueueMode.CompleteOthers, PlayMode.StopAll);
        }
        
        float startTime = Time.time;
        while (Time.time - startTime < lifeTime)
        {
            yield return null;
        }
        asdf = false;

        if (animation)
        {
            animation.Stop();
            animation.clip = animDespawn;
            animation.Play();

            while (animation.isPlaying)
            {
                yield return null;
            }
        }

        Despawn();

        yield break;
    }

    protected override void Update()
    {
        if (!asdf || !isInitializedSkillList)
            return;

        for (int i = 0; i < skillList.Count; i++)
        {
            SkillData skillData = skillList[i].skillData;

            //자동 발동 스킬 아니면 스킵
            if (!skillData.autoExecute)
                continue;

            if (skillList[i].isCoolTime)
                continue;

            skillList[i].Execute();
        }
    }

    void Despawn()
    {
        StopAllCoroutines();
        StopCoroutine("DoSomething");

        gameObject.SetActive(false);
    }
}
