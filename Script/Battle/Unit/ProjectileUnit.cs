using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IProjectile
{
    void Init(BattleUnit _owner, BattleUnit _target, SkillBase _parentSkill);
    void Launch();
    BattleUnit GetTarget();
    void SetBattleGroup(BattleGroup _battleGroup);
}


public class ProjectileUnit : BattleUnit
{
    /// <summary> 날아가서 무슨 스킬 발동할지 </summary>
    protected SkillBase skill
    {
        get
        {
            if (skillList != null && skillList.Count > 0)
                return skillList[0];

            return null;
        }
    }

    /// <summary> 이 발사체를 발사한 주인 </summary>
    public BattleUnit owner { get; set; }

    /// <summary> 날아갈 목표 </summary>
    public BattleUnit target { get; set; }

    public SkillBase parentSkill;

    public Vector3 startPos;

    protected ParticleSystem particle;

    protected ParticleSystemRenderer[] particleRenderers = null;

    protected override void Awake()
    {
        particle = GetComponentInChildren<ParticleSystem>();

        base.Awake();

        particleRenderers = GetComponentsInChildren<ParticleSystemRenderer>();

    }

    protected override void Start()
    {
        //스킬 초기화는 발사가 될 때 함. 풀링을 해놔야 해서... 스킬 초기화가 늦으면 풀링하기 전에 이상하게 보일 수 있어서 일단 풀링과 동시에 비활성 시켜버림
        //yield return StartCoroutine(base.Start());


        //뎁스 주인공 영웅과 동일하게 맞춰주기
        if (owner)
        {
            Renderer ownerRenderer = owner.GetComponentInChildren<Renderer>();
            List<ParticleSystemRenderer> renderers = new List<ParticleSystemRenderer>(GetComponentsInChildren<ParticleSystemRenderer>());
            for (int i = 0; i < renderers.Count; i++)
            {
                renderers[i].sortingOrder = ownerRenderer.sortingOrder + 100;
            }
        }
    }

    public virtual void Init(BattleUnit _owner, BattleUnit _target, SkillBase _parentSkill)
    {
        target = _target;
        owner = this;
        master = _owner;
        team = master.team;
        SetBattleGroup(master.battleGroup);
      
        //이 발사체가 발동하는 스킬의 주체는 이 발사체를 발사한 주체와 같음
        for (int i = 0; i < skillList.Count; i++)
        {
            skillList[i].owner = owner;
        }

        parentSkill = _parentSkill;

        startPos = transform.position;

    }
}
