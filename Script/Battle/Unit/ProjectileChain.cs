using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileChain : ProjectileUnit, IProjectile
{
    public float startMovingSpeed = 5f;
    public float idleTime = 2f;
    public LineRenderer lineRenderer;

    //=============================================================================================================

    protected override void Awake()
    {
        base.Awake();

        lineRenderer = GetComponent<LineRenderer>();

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
               
        if(lineRenderer != null)
        {
            lineRenderer.SetPosition(0, startPos);
            lineRenderer.SetPosition(1, startPos);
        }
        
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

        Vector2 firePoint = transform.position;

        float startTime = Time.time;
        float t = 0;
        //Vector2 targetPoint = new Vector2(target.transform.position.x, transform.position.y);
        while (startTime > 0)
        {
            t = (Time.time - startTime) / startMovingSpeed;
            if (t < 1)
            {
                transform.position = Vector2.Lerp(firePoint, target.collider.transform.position /*+ (Vector3.up * 2)*/, t);
                if(lineRenderer != null)
                    lineRenderer.SetPosition(1, transform.position);

            }
            else
            {
                break;
            }
            yield return null;
        }
        startTime = Time.time;
        for (int i = 0; i < skillList.Count; i++)
        {
            skillList[i].owner = owner;
            skillList[i].castTarget = target as BattleHero;
            skillList[i].CheckCastCondition();
            skillList[i].Execute();

        }

        while (startTime > 0)
        {
            t = (Time.time - startTime) / idleTime;
            if (t < 1)
            {
                if(target.isDie)
                {
                    break;
                }
                transform.position = target.transform.position + (Vector3.up * 2);

                float dis = Vector2.Distance(startPos, target.collider.transform.position /*+ (Vector3.up * 2)*/);
                if (dis > 4f)
                {
                    if (lineRenderer != null)
                        lineRenderer.SetPosition(1, transform.position);
                }
                else
                {
                    break;
                }
            }
            else
            {                
                break;
            }
            yield return null;
        }
        Despawn();
    }
    

    void Despawn()
    {
        StopAllCoroutines();
        StopCoroutine("MoveToTarget");
        gameObject.SetActive(false);
    }
}


