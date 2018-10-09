using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileNormal : ProjectileUnit, IProjectile
{

    public bool isCurve = false;
    public float curveHeight = 3f;
    public float flyTime = 3f;
    public float flySpeed = 4f;
    public bool isDisChacking = false;
    public bool isPush = false;
    public bool isCanCounter = true;
    public GameObject hitParticle = null;
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
        base.Init(_owner, _target, _parentSkill);

        isInitialized = true;
    }


    //=============================================================================================================
    public void Launch()
    {
        StopAllCoroutines();
        StopCoroutine("MoveToTarget");
        StartCoroutine("MoveToTarget");
    }

    public bool isMoveForwardRight = false;
    IEnumerator MoveToTarget()
    {
        //스킬 초기화 안 되어 있으면 초기화 한 다음에 날아감.
        if (!isInitializedSkillList)
            InitSkillList();
        //yield return StartCoroutine(InitSkillList());

        //발사 주체와 목표물 설정 안 되면 날아가지 않음
        while (!master || !target)
            yield return null;

        //파티클 있으면 플레이 시키고
        ParticleSystem p = GetComponentInChildren<ParticleSystem>();
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

        Vector3 targetPoint = target.transform.position;
        //transform.LookAt(targetPoint, Vector3.forward);
        Vector3 a = new Vector3(0, 0, 80);
        Vector3 b = new Vector3(0, 0, -80);

        float force = 10;
        if (target.transform.position.x < transform.position.x)
            force = -force;
        isMoveForwardRight = target.transform.position.x < transform.position.x;

        float startTime = Time.time;
        while (startTime > 0)
        {
            float t = (Time.time - startTime) / flyTime;
            if (isCurve)
            {
                transform.eulerAngles = Vector3.Lerp(a, b, t);
                transform.position = GetPointOnBezierCurve(startPos, startPos + (Vector3.up * curveHeight), targetPoint + (Vector3.up * curveHeight), targetPoint, t);                
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, transform.position + (Vector3.right * force), flySpeed * Time.deltaTime);
                //transform.position = Vector2.Lerp(transform.position, transform.position + transform.right, 3 * t);
            }


            if (isCanCounter)
            {
                if (isDisChacking)
                {
                    if (ChackingDistance())
                    {
                        break;
                    }
                }
            }
            else if (isPush)
            {
                for (int i = 0; i < skillList.Count; i++)
                {
                    SkillBase skill = skillList[i];
                    skill.owner = owner;

                    skill.CheckCastCondition();

                    if (skill.isCoolTime == false && skill.skillData.autoExecute)
                        skill.Execute();
                }
            }         

            if (t > 1)
                break;

            yield return null;
        }

        bool isCounter = false;
        if (isCanCounter && owner.master.team != target.team)
        {
            BattleUnit.Team team = owner.master.team;
            SkillBase counterRangeSkill = target.counterSkillList.Find(x => x.skillData.effectType == "CounterRange");

            if (counterRangeSkill != null && counterRangeSkill.isCoolTime == false)
            {
                //Debug.Log(owner.master.heroData.heroName + "의 공격 카운터 !!");
                isCounter = true;

                counterRangeSkill.Execute();
                BattleUnit attacker = target;
                target = owner.master;

                //캐릭터 위치에 따라 뒤집기
                if (attacker is BattleHero)
                {
                    BattleHero h = attacker as BattleHero;
                    bool isFlip = h.flipX;
                    float z = isFlip ? 180f : 0f;
                    //transform.Rotate(new Vector3(0, 0, z));
                    Quaternion quaternion = new Quaternion(0, 0, z, 0);
                    transform.rotation = quaternion;

                }

                float distance = target.GetDistanceFrom(this);

                while (distance > 1)
                {
                    distance = target.GetDistanceFrom(this);
                    transform.position = Vector3.MoveTowards(transform.position, target.collider.transform.position, 1);
                    //transform.position = Vector3.Lerp(transform.position,target.transform.position,t);
                    yield return null;
                }

                //Debug.Log(owner.master.heroData.heroName + " 끝");

                //Debug.Log("팀 변경 " + owner.master.team + "->" + attacker.team);
                owner.master.team = attacker.team;
            }
        }

        if (isPush == false)
        {
            for (int i = 0; i < skillList.Count; i++)
            {
                skillList[i].owner = owner;
                skillList[i].castTarget = target as BattleHero;
                skillList[i].Execute();
            }
        }
        if (isCounter)
        {
            //Debug.Log("팀 변경 " + owner.master.team + "->" + team);
            owner.master.team = team;
        }

        //발사체 명중 시 파티클 재생
        if (hitParticle)
        {
            GameObject hitEffectObj = Battle.GetObjectInPool(hitParticle.name);

            //풀링 안되어 있으면 함.
            if (!hitEffectObj)
            {
                hitEffectObj = Instantiate(hitParticle, target.transform.position, Quaternion.identity, owner.transform.parent) as GameObject;
                hitEffectObj.name = hitParticle.name;
                //if(!hitEffectObj.GetComponent<SelfDestroyParticle>())
                //    hitEffectObj.AddComponent<SelfDestroyParticle>();
                hitEffectObj.SetActive(false);
                hitEffectObj.AddComponent<BattleGroupElement>();

                Battle.AddObjectToPool(hitEffectObj);
            }

            if (hitEffectObj)
            {
                Vector3 hitPos = target.GetClosestPoint(transform.position);
                hitEffectObj.transform.position = hitPos;

                hitEffectObj.transform.localScale = Vector3.one;

                //발사체 오브젝트 활성                
                hitEffectObj.SetActive(true);
                hitEffectObj.GetComponent<BattleGroupElement>().SetBattleGroup(owner.battleGroup);
                ParticleSystem p2 = hitEffectObj.GetComponentInChildren<ParticleSystem>();
                if (p2)
                    p2.Play();

                Animation _anim = hitEffectObj.GetComponentInChildren<Animation>();
                if (_anim)
                    anim.Play();
            }
        }

        //파티클 끄기. (뿅하고 끄면 이상함)
        if (particle)
            particle.Stop();

        //파티클 완전히 꺼질 때 까지 대기. 즉시 꺼지기 설정되어 있으면 이 과정 생략
        while (particle && !particle.isStopped)
            yield return null;

        //그리고 디스폰
        Despawn();
    }

    bool TargetCounterCheck(List<BattleHero> tartgetList)
    {
        bool isCounter = false;
        for (int i = 0; i < tartgetList.Count; i++)
        {
            BattleHero target = tartgetList[i];
            SkillBase counterRangeSkill = target.counterSkillList.Find(x => x.skillData.effectType == "CounterRange");
            if(counterRangeSkill && counterRangeSkill.isCoolTime == false)
            {
                isCounter = true;
                counterRangeSkill.Execute();
                break;
            }

        }
        return isCounter;
    }

    /// <summary> 날아가는 동안 스킬의 타겟들과 거리 체크 </summary>
    bool ChackingDistance()
    {
        bool result = false;
        for (int i = 0; i < skillList.Count; i++)
        {
            SkillBase skill = skillList[i];
            BattleHero hero = Target(skill);
            if (hero != null)
            {
                target = hero;               
                result = true;
            }
        }

        return result;
    }

    BattleHero Target(SkillBase skill)
    {
        BattleHero hero = null;
        skill.CollectTargets();
        for (int i = 0; i < skill.targetList.Count; i++)
        {
            float dis = skill.targetList[i].GetDistanceFrom(this);
            if(dis < 0.5f)
            {
                hero = skill.targetList[i];
                return hero;
            }
        }
        return hero;
    }

    void Despawn()
    {
        StopAllCoroutines();
        StopCoroutine("MoveToTarget");
        gameObject.SetActive(false);
    }


    Vector3 GetPointOnBezierCurve(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        Vector3 a = Lerp(p0, p1, t);
        Vector3 b = Lerp(p1, p2, t);
        Vector3 c = Lerp(p2, p3, t);
        Vector3 d = Lerp(a, b, t);
        Vector3 e = Lerp(b, c, t);
        Vector3 pointOnCurve = Lerp(d, e, t);

        return pointOnCurve;
    }
    Vector3 Lerp(Vector3 a, Vector3 b, float t)
    {
        return (1f - t) * a + t * b;
    }
}
