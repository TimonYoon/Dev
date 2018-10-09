using UnityEngine;
using System.Collections;
using System.Collections.Generic;




/// <summary> 발사체 </summary>
public class Projectile : ProjectileUnit, IProjectile
{
    public enum FryingType
    {
        /// <summary> 상대와 거리차 이런거 상관 없이 정해진 속도로 이동 </summary>
        Absolute,

        /// <summary> 상대와 거리차에 따라 상대적으로 움직임 </summary>
        Relative
    }

    public FryingType flyingType = FryingType.Absolute;

    public bool stopImmediately = false;

    public bool reverseX = false;

    float x, y;
    float speedTemp = 0f;
    public GameObject hitParticle = null;
    #region 리소스 설정

    [SerializeField]
    AnimationCurve animationCurveX = null;

    [SerializeField]
    AnimationCurve animationCurveY = null;

    [SerializeField]
    AnimationCurve flyingCurveX = null;

    [SerializeField]
    AnimationCurve flyingCurveY = null;


    [SerializeField]
    float flyingTime = 1f;
    
    [SerializeField]
    float acc = 5f;

    [SerializeField]
    float speed = 5f;

    //[SerializeField]
    //float startChaseTime = 0.7f;

    //[SerializeField]
    //float gravity = 0f;

    [SerializeField]
    float followTargetWeight = 1f;

    [SerializeField]
    float hitThreshold = 0.3f;

    #endregion


    public bool isCanCounter = true;

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
    override protected void Update()
    {
        //아무일 안 함. 베이스 클래스에서 업데이트 할 때 스킬 실행해서 막아둠
    }

    void OnEnable()
    {
        //StopAllCoroutines();
        //StopCoroutine("MoveToTarget");
        //StartCoroutine("MoveToTarget");
        
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

        //startPos = transform.position;
        
        float startTime = Time.time;
        float elapsedTime = 0f;

        //대상 방향에 따라 연출 방향 좌우 뒤집음
        float isNeedFlip = 1f;
        //if (transform.position.x > target.transform.position.x)
        //    isNeedFlip = -1f;

        transform.localScale = Vector3.one * isNeedFlip;// new Vector3(isNeedFlip, transform.localScale.y, transform.localScale.z);

        Vector2 lastPos = transform.position;
        Vector2 velocity = Vector2.zero;
        //커브의 절대 값만큼 이동
        while (elapsedTime < flyingTime)
        {
            float a = elapsedTime / flyingTime; //진척도
            
            //현재 경과 시간의 비율에 비례해서 설정해둔 커브값 만큼 움직임
            x = startPos.x + animationCurveX.Evaluate(a) * isNeedFlip;
            y = startPos.y + animationCurveY.Evaluate(a);

            Vector2 destPos = new Vector3(x, y, transform.position.z);

            //Vector3 dir = enemy.position - transform.position;
            float angle = Mathf.Atan2(destPos.y, destPos.x) * Mathf.Rad2Deg;
            //transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            transform.LookAt(destPos, Vector3.up);

            transform.position = new Vector2(x, y);//, transform.position.z);

            velocity = (Vector2)transform.position - lastPos;


            elapsedTime = Time.time - startTime;
            lastPos = transform.position;

            if (!target || target.isDie)
            {
                if (parentSkill)
                {
                    parentSkill.CollectTargets();
                    if(parentSkill.targetList != null && parentSkill.targetList.Count > 0)
                    {
                        target = parentSkill.targetList[0];
                    }
                    else
                    {
                        //파티클 끄기. (뿅하고 끄면 이상함)
                        if (particle)
                            particle.Stop();

                        //파티클 완전히 꺼질 때 까지 대기
                        while (particle && !particle.isStopped)
                            yield return null;

                        //그리고 디스폰
                        Despawn();
                        yield break;
                    }
                }
            }
                

            yield return null;
        }

        transform.LookAt(target.transform, Vector3.up);
        
        //위치 
        x = startPos.x + animationCurveX.Evaluate(1f) * isNeedFlip;
        y = startPos.y + animationCurveY.Evaluate(1f);        
        transform.position = new Vector2(x, y);//, transform.position.z);


        //Debug.Log(velocity);

        if(flyingType == FryingType.Absolute)
        {
            startTime = Time.time;
            elapsedTime = 0f;
            float lastTime = Time.time;
            Vector3 startPosition = transform.position;
            while (target.GetDistanceFrom(this) > hitThreshold)
            {
                float deltaTime = Time.time - lastTime;

                //transform.position = Vector3.Lerp(startPosition, target.transform.position, acc * acc * speed * elapsedTime);

                Vector2 direction = (Vector2)transform.position + velocity * deltaTime;
                Vector2 targetDirection = (Vector2)transform.position + (Vector2)(target.transform.position - transform.position).normalized * velocity.magnitude * deltaTime;

                Vector3 nextPos = Vector3.Lerp(direction, targetDirection, followTargetWeight);


                //transform.LookAt(Vector2.Lerp(transform.position + transform.forward.normalized * speed,  target.transform.position, followTargetWeight * deltaTime));


                Vector2 fowardDir = transform.position + transform.right.normalized * speed - transform.position;
                Vector2 targetDir = target.GetClosestPoint(transform.position) /*target.transform.position*/ - transform.position;
                Vector2 final = Vector2.Lerp(fowardDir, targetDir, followTargetWeight * deltaTime);

                float angle = Mathf.Atan2(final.y, final.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);


                transform.position = Vector2.Lerp(transform.position, transform.position + transform.right, speed * deltaTime);

                //transform.position = Vector2.MoveTowards(transform.position, transform.position + transform.right, speed * deltaTime);

                //transform.position += transform.right * speed * Time.deltaTime;

                //transform.position = Vector3.Lerp(transform.position, transform.position.x)

                //transform.position = nextPos;// Vector3.Lerp(transform.position, nextPos, speed * deltaTime);

                //transform.position = Vector3.Lerp(nextPos, target.transform.position, speed * deltaTime);

                //velocity += targetDirection.normalized * acc * deltaTime;

                //transform.position += (Vector3) velocity;

                //transform.position = Vector3.Lerp(transform.position, target.transform.position, speed * deltaTime);
                velocity = (Vector2)transform.position - lastPos;

                elapsedTime = Time.time - startTime;

                lastTime = Time.time;

                lastPos = transform.position;

                if (!target || target.isDie)
                {
                    if (parentSkill)
                    {
                        parentSkill.CollectTargets();
                        if (parentSkill.targetList != null && parentSkill.targetList.Count > 0)
                        {
                            target = parentSkill.targetList[0];
                        }
                        else
                        {
                            speedTemp = speed;
                            Stop();
                            yield break;
                        }
                    }
                }

                yield return null;
            }
        }
        else
        {
            elapsedTime = 0f;
            float animValue = 0f;
            float lastTime = Time.time;

            startPos = transform.position;
            lastPos = transform.position;

            float distance = target.GetDistanceFrom(this);
            float goalTime = distance / speed;

            while (animValue < 1f)
            {
                elapsedTime = Time.time - lastTime;

                if (goalTime <= 0f)
                    animValue = 1f;
                else
                    animValue += elapsedTime / goalTime;
                

                float x, y;
                Vector3 targetPos = target.transform.position;
                if (target.collider)
                    targetPos = target.collider.transform.position;

                
                if (reverseX)
                {
                    //Vector3 temp = startPos;
                    startPos = targetPos;
                    targetPos = master.transform.position;
                }

                //x축은 그냥 시간에 비례해서 보간함.
                Vector3 a = Vector3.Slerp(startPos, targetPos, animValue);
                x = a.x;

                //y축은 애님그래프 값만큼 보정하고, 날아가야 하는 시간에 비례해서 높이 조절함
                y = a.y + flyingCurveY.Evaluate(animValue) * goalTime;

                transform.position = new Vector3(x, y, transform.position.z);

                //날아가는 방향 바라보기. 화살 같은거
                Vector2 targetDir = (Vector2)transform.position - lastPos;
                targetDir = targetDir.normalized;
                float angle = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg;

                Quaternion rot = Quaternion.AngleAxis(angle, Vector3.forward);

                transform.rotation = Quaternion.Lerp(transform.rotation, rot, 25f * elapsedTime);//.AngleAxis(angle, Vector3.forward);
                transform.localEulerAngles = new Vector3(0f, 0f, transform.localEulerAngles.z);

                lastTime = Time.time;
                lastPos = transform.position;

                yield return null;                
            }
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

        

        
        for (int i = 0; i < skillList.Count; i++)
        {
            skillList[i].owner = owner;
            //master = owner;
            //skillList[i].canExecute = true;
            skillList[i].castTarget = target as BattleHero;
            skillList[i].CheckCastCondition();
            skillList[i].Execute();

            //ExecuteSkill(skillList[i]);
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

                Animation a = hitEffectObj.GetComponentInChildren<Animation>();
                if (a)
                    anim.Play();
            }
        }

        //한 프레임 더 가서 발동. 너무 일찍 끊기는 것 같아서 임시방편
        //yield return new WaitForSeconds(0.1f);

        //쿨타임, idle상태 체크 이런건 안 함
        //SkillBase.ExecuteCondition ignoreCondition = SkillBase.ExecuteCondition.IsNotIdle | SkillBase.ExecuteCondition.IsCoolTime;

        
        
        //파티클 끄기. (뿅하고 끄면 이상함)
        if (particle)
            particle.Stop();

        //파티클 완전히 꺼질 때 까지 대기. 즉시 꺼지기 설정되어 있으면 이 과정 생략
        while (particle && !particle.isStopped && !stopImmediately)
            yield return null;

        //그리고 디스폰
        Despawn();
        //gameObject.SetActive(false);
    }    

    void Stop()
    {
        StopAllCoroutines();
        //gameObject.SetActive(false);
        StartCoroutine("StopA");

    }

    IEnumerator StopA()
    {
        //float s = Time.time;

        float elapsedTime = 0f;
        float lastTime = Time.time;
        float startTime = Time.time;
        while (true)
        {
            elapsedTime = Time.time - lastTime;

            //s *= (1f - elapsedTime);

            //if (s < -1f)
            //{
            //    Despawn();
            //    yield break;
            //}                

            transform.localEulerAngles = Vector3.Lerp(transform.localEulerAngles, transform.localEulerAngles + Vector3.forward, Random.Range(5f, 15f) * elapsedTime);

            //transform.localPosition = Vector3.MoveTowards(transform.localPosition, transform.localPosition + transform.right, 30f);
            transform.position = Vector2.Lerp(transform.position, transform.position + transform.right, speedTemp * elapsedTime);

            //일정 시간이 지나면 중단
            if (Time.time - startTime > 3f)
            {
                Despawn();
                yield break;
            }
            
            //화면 끝까지 가면 중단
            if (transform.position.x > owner.battleGroup.endingPoint.position.x)
            {
                Despawn();
                yield break;
            }
                
            //바닥에 닿으면 중단
            if (transform.position.y <= owner.transform.position.y)
            {
                Despawn();
                yield break;
            }

            lastTime = Time.time;

            yield return null;
        }
    }

    void Despawn()
    {
        StopAllCoroutines();
        StopCoroutine("MoveToTarget");
        gameObject.SetActive(false);
    }
}
