using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileGround : ProjectileUnit, IProjectile
{
    public float delayTime =2.5f;
    public float idleTime = 2f;
    
    public bool stopImmediately = true;

    public bool isDuration = false;
    //=============================================================================================================


    protected override void Awake()
    {
        base.Awake();

        if (!battleGroup)
            return;

        battleGroup.onChangedBattlePhase += OnChangedBattlePhase;
        battleGroup.onChangedStage += OnChangedStage;
    }
    protected override void Start()
    {
        base.Start();

    }
    override protected void Update()
    {
        //아무일 안 함. 베이스 클래스에서 업데이트 할 때 스킬 실행해서 막아둠
    }
    void OnChangedBattlePhase(BattleGroup battleGroup)
    {
        if (battleGroup.battlePhase == BattleGroup.BattlePhase.FadeOut
            || battleGroup.battlePhase == BattleGroup.BattlePhase.FadeIn)
            Despawn();
    }

    void OnChangedStage(BattleGroup battleGroup)
    {
        Despawn();
    }

    public BattleUnit GetTarget()
    {
        return target;
    }
    public override void SetBattleGroup(BattleGroup _battleGroup)
    {
        base.SetBattleGroup(_battleGroup);
    }

    public override void Init(BattleUnit _owner, BattleUnit _target, SkillBase _parentSkill)
    {
        if (owner != null)
        {
            if (owner.battleGroup)
                owner.battleGroup.onChangedBattlePhase -= OnChangedBattlePhase;
        }

        base.Init(_owner, _target, _parentSkill);

        owner.battleGroup.onChangedBattlePhase += OnChangedBattlePhase;
        isInitialized = true;
    }

    void OnChangedBattlePhase(BattleGroup.BattlePhase battlePhase)
    {
        if(battlePhase == BattleGroup.BattlePhase.Finish)
            Despawn();
    }

    //=============================================================================================================

    public void Launch()
    {
        StopAllCoroutines();
        StopCoroutine("MoveToTarget");
        StartCoroutine("MoveToTarget");
    }

    IEnumerator MoveToTarget()
    {
        //스킬 초기화 안 되어 있으면 초기화 한 다음에 날아감.
        if (!isInitializedSkillList)
            InitSkillList();



        //발사 주체와 목표물 설정 안 되면 날아가지 않음
        while (!master || !target)
            yield return null;
        ParticleSystem p = GetComponentInChildren<ParticleSystem>();

        Vector3 targetPoint = target.transform.position;

        transform.position = targetPoint;

        //캐릭터 위치에 따라 뒤집기
        if (skill.owner is BattleHero)
        {
            BattleHero h = owner as BattleHero;
            bool isFlip = h.flipX;
            float x = isFlip ? -1f : 1f;
            transform.localScale = new Vector3(x, 1f, 1f);
        }

        //파티클 있으면 플레이 시키고

        if (p)
            p.Play();

        //애니메이션 있으면 플레이 시키고
        Animation anim = GetComponentInChildren<Animation>();
        if (anim)
            anim.Play();

        //사운드 재생
        AudioSource audio = GetComponentInChildren<AudioSource>();
        if (audio)
            audio.Play();


        float startTime = Time.time;
        while (startTime > 0)
        {
            float t = (Time.time - startTime) / delayTime;
           
            if (t > 1)
                break;

            yield return null;
        }

        if(isDuration == false)
        {
            for (int i = 0; i < skillList.Count; i++)
            {
                skillList[i].owner = owner;
                //skillList[i].castTarget = target as BattleHero;
                skillList[i].CheckCastCondition();
                skillList[i].Execute();
            }
            startTime = Time.time;
            while (startTime > 0)
            {
                float t = (Time.time - startTime) / idleTime;

                if (t > 1)
                    break;

                yield return null;
            }
        }
        else
        {
            startTime = Time.time;
            float testTime = Time.time;
            while (startTime > 0)
            {
                if(master.isDie)
                {
                    break;
                }
                float t = (Time.time - startTime) / idleTime;
                float a = Time.time - testTime;
                for (int i = 0; i < skillList.Count; i++)
                {
                    skillList[i].owner = owner;
                    //skillList[i].castTarget = target as BattleHero;
                    skillList[i].CheckCastCondition();
                    if(skillList[i].isCoolTime == false)
                        skillList[i].Execute();
                }

                if (a >=1)
                {
                    testTime = Time.time;
                    //for (int i = 0; i < skillList.Count; i++)
                    //{
                    //    skillList[i].owner = owner;
                    //    //skillList[i].castTarget = target as BattleHero;
                    //    skillList[i].CheckCastCondition();
                    //    skillList[i].Execute();
                    //}
                }

                if (t > 1)
                    break;

                yield return null;
            }
        }

        Despawn();
        
        yield break;
        
        ////파티클 완전히 꺼질 때 까지 대기. 즉시 꺼지기 설정되어 있으면 이 과정 생략
        //while (particle && !particle.isStopped)
        //    yield return null;
        //그리고 디스폰
        gameObject.SetActive(false);
        //Despawn();
    }



    void Despawn()
    {
        StopAllCoroutines();
        StopCoroutine("MoveToTarget");

        if (stopImmediately)
            gameObject.SetActive(false);
        else
            particle.Stop();

        return;

        gameObject.SetActive(false);
    }
}
