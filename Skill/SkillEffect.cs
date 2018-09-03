using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISkillEffect
{
    void TriggerEffect();
}

abstract public class SkillEffectBase : ISkillEffect
{
    protected SkillBase skill;

    protected SkillEvent skillEvent;

    public SkillEffectBase(SkillBase skill, SkillEvent skillEvent)
    {
        this.skill = skill;
        this.skillEvent = skillEvent;
    }

    virtual public void TriggerEffect()
    {

    }    
}
//#################################################################################################
/// <summary> 또다른 스킬 발동 </summary>
public class SkillEffectExecuteSkill : SkillEffectBase
{
    public struct SkillExecuteWeight
    {
        public string id;
        public int weight;
    }

    List<SkillExecuteWeight> skillListToExecute = new List<SkillExecuteWeight>();

    int totalExecWeight = 0;

    public SkillEffectExecuteSkill(SkillBase skill, List<SkillEvent> skillEventList) : base(skill, null)
    {
        this.skill = skill;

        if (skillEventList == null)
            return;
        
        for(int i = 0; i < skillEventList.Count; i++)
        {
            SkillExecuteWeight s = new SkillExecuteWeight();
            s.id = skillEventList[i].executeSkillID;
            s.weight = skillEventList[i].executeWeight;

            totalExecWeight += s.weight;

            skillListToExecute.Add(s);

            this.skillEvent = skillEventList[i];
        }

    }

    override public void TriggerEffect()
    {
        if (skillListToExecute == null || skillListToExecute.Count == 0)
            return;

        int r = UnityEngine.Random.Range(0, totalExecWeight);

        int curWeight = 0;
        for (int i = 0; i < skillListToExecute.Count; i++)
        {
            curWeight += skillListToExecute[i].weight;
            if (r <= curWeight)
            {
                SkillBase s = skill.owner.skillList.Find(x => x.skillData.id == skillListToExecute[i].id);
                if (s)
                {
                    s.CheckCastCondition();
                    s.Execute();
                }

                break;
            }
        }


        if (skill.onTriggerEvent != null)
            skill.onTriggerEvent(skill, skillEvent);
    }
}

/// <summary> 발사체 날리기 </summary>
public class SkillEffectFireProjectile : SkillEffectBase
{
    public SkillEffectFireProjectile(SkillBase skill, SkillEvent skillEvent) : base(skill, skillEvent)
    {
        this.skill = skill;
        this.skillEvent = skillEvent;
    }

    override public void TriggerEffect()
    {
        //발사체 없으면 아무일 없음? ㅇㅇ 아무일 없어야 할 듯..
        if (!skillEvent.projectile)
            return;

        skill.CollectTargets();

        if (skill.targetList == null || skill.targetList.Count == 0)
        {
            if (skill.onTriggerEvent != null)
                skill.onTriggerEvent(skill, skillEvent);
            return;
        }
        
        for(int i = 0; i < skill.targetList.Count; i++)
        {
            BattleHero target = skill.targetList[i];

            //풀링 안되어 있으면 함
            GameObject projectileObj = Battle.GetObjectInPool(skillEvent.projectile.name);
            if (!projectileObj)
            {
                projectileObj = GameObject.Instantiate(skillEvent.projectile, skill.owner.transform.position, Quaternion.identity, skill.owner.transform.parent) as GameObject;
                projectileObj.transform.localPosition = Vector3.zero;
                projectileObj.name = skillEvent.projectile.name;
                projectileObj.SetActive(false);
                if (!projectileObj.GetComponent<BattleGroupElement>())
                    projectileObj.AddComponent<BattleGroupElement>();

                Battle.AddObjectToPool(projectileObj);
            }

            if (!projectileObj)
            {
                if (skill.onTriggerEvent != null)
                    skill.onTriggerEvent(skill, skillEvent);
                return;
            }

            //시작 위치로 이동
            projectileObj.transform.position = skillEvent.projectilePivot.position;// + Vector3.back;


            //발사체에서 발생하는 스킬의 대상과, 스킬의 owner 설정
            Projectile projectile = projectileObj.GetComponent<Projectile>();
            IProjectile interfaceProjectile = null;
            if (projectile)
            {
                interfaceProjectile = projectile as IProjectile;
            }

            //발사 후 돌아오는 발사체
            ReturnProjectile returnProjectile = projectileObj.GetComponent<ReturnProjectile>();
            if (returnProjectile)
            {
                interfaceProjectile = returnProjectile as IProjectile;
            }

            ProjectileChain projectileChain = projectileObj.GetComponent<ProjectileChain>();
            if(projectileChain)
            {
                interfaceProjectile = projectileChain as IProjectile;
            }

            ProjectileNormal projectileNormal = projectileObj.GetComponent<ProjectileNormal>();
            if (projectileNormal)
            {
                interfaceProjectile = projectileNormal as IProjectile;
            }

            ProjectileGround projectileGround = projectileObj.GetComponent<ProjectileGround>();
            if(projectileGround)
            {
                interfaceProjectile = projectileGround as IProjectile;
            }

            if (interfaceProjectile != null)
            {
                projectileObj.gameObject.SetActive(true);
                interfaceProjectile.Init(skill.owner, target, skill);
                interfaceProjectile.SetBattleGroup(skill.owner.battleGroup);
                interfaceProjectile.Launch();
            }

            
        }   

        if (skill.onTriggerEvent != null)
            skill.onTriggerEvent(skill, skillEvent);
    }
}

/// <summary> 파티클 보여주기 </summary>
public class SkillEffectShowParticle : SkillEffectBase
{
    public SkillEffectShowParticle(SkillBase skill, SkillEvent skillEvent) : base(skill, skillEvent)
    {
        this.skill = skill;
        this.skillEvent = skillEvent;
    }

    override public void TriggerEffect()
    {
        //풀링 안되어 있으면 함.
        GameObject particleObj = Battle.GetObjectInPool(skillEvent.particle.name);
        if (!particleObj)
        {
            particleObj = GameObject.Instantiate(skillEvent.particle, skill.owner.transform.position, Quaternion.identity, skill.owner.transform.parent) as GameObject;
            particleObj.transform.localPosition = Vector3.zero;
            particleObj.name = skillEvent.particle.name;
            //particleObj.SetActive(false);
            
            if (!particleObj.GetComponent<BattleGroupElement>())
                particleObj.AddComponent<BattleGroupElement>();

            if(Battle.Instance)
                Battle.AddObjectToPool(particleObj);
        }

        if (!particleObj)
            return;

        //particleObj.transform.position = skill.owner.transform.position;

        //캐릭터 위치에 따라 뒤집기
        if (skill.owner is BattleHero)
        {
            BattleHero h = skill.owner as BattleHero;
            bool isFlip = h.flipX;
            float x = isFlip ? -1f : 1f;
            particleObj.transform.localScale = new Vector3(x, 1f, 1f);
        }

        if (skillEvent.attachToTarget)
        {
            if(skill.owner is BattleHero)
                particleObj.transform.SetParent(skill.owner.GetComponent<BattleHero>().skeletonAnimation.transform);
            else
                particleObj.transform.SetParent(skill.owner.transform);

            particleObj.transform.localPosition = Vector3.zero;
        }
        else
        {
            particleObj.transform.SetParent(skill.owner.transform.parent);

            if (skill.owner is BattleHero)
                particleObj.transform.position = skill.owner.GetComponent<BattleHero>().skeletonAnimation.transform.position;
            else
                particleObj.transform.position = skill.owner.transform.position;
        }

        OrderController o = particleObj.GetComponent<OrderController>();
        if (o)
            o.parent = skill.owner.GetComponent<OrderController>();

        //발사체 오브젝트 활성
        if(Battle.Instance)
            particleObj.GetComponent<BattleGroupElement>().SetBattleGroup(skill.owner.battleGroup);

        particleObj.SetActive(true);

        ParticleSystem particle = particleObj.GetComponent<ParticleSystem>();


        if (particle)
            particle.Play();
    }
}

/// <summary> 이름은 이래도 데미지 주는 기능을 함 </summary>
public class SkillEffectMeleeHit : SkillEffectBase
{
    public SkillEffectMeleeHit(SkillBase skill, SkillEvent skillEvent) : base(skill, skillEvent)
    {
        this.skill = skill;
        this.skillEvent = skillEvent;
    }

    override public void TriggerEffect()
    {
        //타겟 고르기
        skill.CollectTargets();
        //Debug.Log("타겟 초기화");
        if (skill.targetList == null || skill.targetList.Count == 0)
        {
            if (skill.onTriggerEvent != null)
                skill.onTriggerEvent(skill, skillEvent);

            return;
        }

        for (int i = 0; i < skill.targetList.Count; i++)
        {
            BattleHero target = skill.targetList[i];

            //타격 이펙트
            if (skillEvent != null && skillEvent.hitEffect)
            {
                GameObject hitEffectObj = Battle.GetObjectInPool(skillEvent.hitEffect.name);

                //풀링 안되어 있으면 함.
                if (!hitEffectObj)
                {
                    hitEffectObj = GameObject.Instantiate(skillEvent.hitEffect, target.transform.position, Quaternion.identity, skill.owner.transform.parent) as GameObject;
                    hitEffectObj.name = skillEvent.hitEffect.name;
                    //hitEffectObj.AddComponent<SelfDestroyParticle>();
                    hitEffectObj.SetActive(false);
                    hitEffectObj.AddComponent<BattleGroupElement>();

                    Battle.AddObjectToPool(hitEffectObj);
                }

                if (hitEffectObj)
                {
                    //Vector3 hitPos = target.GetClosestPoint(skill.owner.transform.position);
                    Vector3 hitPos = target.collider.transform.position;// .GetClosestPoint(skill.owner.transform.position);
                    hitEffectObj.transform.position = hitPos;
                    if (skillEvent.attachToTarget)
                        hitEffectObj.transform.SetParent(target.transform);

                    hitEffectObj.transform.localScale = Vector3.one;

                    //캐릭터 위치에 따라 뒤집기
                    if (skill.owner is BattleHero)
                    {
                        BattleHero h = skill.owner as BattleHero;
                        bool isFlip = h.flipX;
                        float x = isFlip ? -1f : 1f;
                        hitEffectObj.transform.localScale = new Vector3(x, 1f, 1f);
                    }


                    //히트 파티클 오브젝트 활성                
                    hitEffectObj.SetActive(true);
                    hitEffectObj.GetComponent<BattleGroupElement>().SetBattleGroup(skill.owner.battleGroup);

                    OrderController orderControllerA = hitEffectObj.GetComponent<OrderController>();
                    OrderController orderControllerB = target.GetComponent<OrderController>();
                    if (orderControllerA)
                    {
                        if (orderControllerB)
                            orderControllerA.parent = orderControllerB;
                        else
                            orderControllerA.parent = null;
                    }

                    ParticleSystem p = hitEffectObj.GetComponentInChildren<ParticleSystem>();
                    if (p)
                        p.Play();

                    Animation anim = hitEffectObj.GetComponentInChildren<Animation>();
                    if (anim)
                        anim.Play();
                }
            }

            bool isSuccessHit = false;

            //대상한테 데미지 적용. 힐은 피 채워줌
            if (!string.IsNullOrEmpty(skill.skillData.power))
            {
                double damageModifyType = 0d;
                if (skill.skillData.damageType == SkillBase.DamageType.Physical)
                    damageModifyType = skill.owner.master.stats.GetValueOf(StatType.AttackPowerPhysical);
                else if (skill.skillData.damageType == SkillBase.DamageType.Magical)
                    damageModifyType = skill.owner.master.stats.GetValueOf(StatType.AttackPowerMagical);

                double damageModifyRange = 0d;
                if (skill.skillData.rangeType == SkillBase.RangeType.Melee)
                    damageModifyRange = skill.owner.master.stats.GetValueOf(StatType.AttackPowerMelee);
                else if (skill.skillData.rangeType == SkillBase.RangeType.Range)
                    damageModifyRange = skill.owner.master.stats.GetValueOf(StatType.AttackPowerRange);

                double power = skill.GetPower(target) * (1 + damageModifyType * 0.0001d) * (1 + damageModifyRange * 0.0001d);

                //if (skill.owner.master.heroData.baseData.id.Contains("Centaur"))
                //    Debug.Log(power + ", " + skill.GetPower(target) + ", " + damageModifyType + ", " + damageModifyRange);

                if (skill.skillData.effectType == "Heal")
                    target.Damage(skill.owner.master, -power, SkillBase.DamageType.Pure, skill, "Heal");
                else if (skill.skillData.effectType == "Resurrect")
                {
                    target.Damage(skill.owner.master, -power, SkillBase.DamageType.Pure, skill, "Resurrect");
                    BattleHero h = target as BattleHero;
                    h.ReGen(true);
                }
                else
                {
                    //콜라 - 수호자 버프 처리
                    List<Buff> buffGuardList = target.buffController.buffList.FindAll(x => x.id == "Buff_Knight_Passive"
                                    && x.target == target && x.owner != target && !x.owner.isDie);
                    if (buffGuardList != null && buffGuardList.Count > 0)
                    {
                        int count = buffGuardList.Count;
                        for (int a = 0; a < count; a++)
                        {
                            //수호자들끼리 30%만큼 피해를 나눠서 받음
                            buffGuardList[a].owner.Damage(skill.owner.master, power * 0.3d * 1 / count, skill.skillData.damageType, skill, "Guard");
                        }

                        //직접 얻어 맞은애는 70%
                        isSuccessHit = target.Damage(skill.owner.master, power * 0.7d, skill.skillData.damageType, skill);
                    }
                    else
                    {
                        isSuccessHit = target.Damage(skill.owner.master, power, skill.skillData.damageType, skill);
                    }

                    //대상 흔들림
                    if (isSuccessHit && skill.skillData.forceType == SkillBase.ForceType.Shake)
                        target.Shake(0.4f, 0.15f, 0.03f);

                }
            }

            if (skill.skillData.effectType == "Trail_End")
            {
                skill.owner.GetComponentInChildren<EffectTrail>().OffEffect();
            }


            if (skill.skillData.forceType == SkillBase.ForceType.Dash)
            {
                if (skill.skillData.forcePower != 0f)
                {
                    float force = skill.skillData.forcePower;

                    Vector2 point = new Vector2(target.transform.position.x + force, target.transform.position.y);
                    target.Dash(point, 1);
                }
            }

            // 밀기
            if(skill.skillData.forceType == SkillBase.ForceType.Push)
            {
                if (skill.skillData.forcePower != 0f)
                {
                    float force = skill.skillData.forcePower;

                    //대상이 왼쪽에 있는 경우 반대로 함
                    if (target.transform.position.x < skill.transform.position.x)
                        force = -force;

                    //Debug.Log("force : " + force);
                    target.addedForce += ( Vector3.right * force);
                    //Debug.Log("force : " + force);
                }
                //target.Knockback2(skill, skill.skillData.forcePower);
            }

            if (skill.skillData.forceType == SkillBase.ForceType.Pull)
            {
                float force = skill.skillData.forcePower;
                float filp = Random.Range(2.5f, 3.5f);
                float y = Random.Range(-1f, 1f);
                //대상이 왼쪽에 있는 경우 반대로 함
                if (target.transform.position.x < skill.transform.position.x)
                    filp = -filp;
                Vector2 point = new Vector2(skill.owner.master.transform.position.x + filp, skill.owner.master.transform.position.y + y);
                //Debug.Log(target.heroData.heroName + " 끌려옴");
                target.Pull(point, 1);

            }
            else if (skill.skillData.forceType == SkillBase.ForceType.Knockback)
            {
                // 표현이 넉백에 가까움..
                //밀어내기/당기기 효과
                if (skill.skillData.forcePower != 0f)
                {
                    float force = skill.skillData.forcePower;

                    //대상이 왼쪽에 있는 경우 반대로 함
                    if (target.transform.position.x < skill.transform.position.x)
                        force = -force;

                    target.addedForce += Vector3.right * force;

                }
            }
            else if(skill.skillData.forceType == SkillBase.ForceType.Knockback2)
            {
                target.Knockback2(skill, skill.skillData.forcePower);
            }
            else if(skill.skillData.forceType == SkillBase.ForceType.Airborne)
            {
                target.Airborne(skill, skill.skillData.forcePower);
            }
            else if(skill.skillData.forceType == SkillBase.ForceType.FallDown)
            {
                target.FallingDown(skill, skill.skillData.forcePower);
            }

            //버프 적용
            int r = UnityEngine.Random.Range(0, 10000);
            if (r <= skill.skillData.buffProbability)
                target.buffController.AttachBuff(skill.owner.master, skill.skillData.buffID, skill.skillData.buffStack);


            if (!isSuccessHit)
                continue;
            
            
        }

        if (skill.onTriggerEvent != null)
            skill.onTriggerEvent(skill, skillEvent);

        OnTrailEnd();
    }

    void OnTrailEnd()
    {
        if (skill.owner.heroData != null)
        {
            string heroID = skill.owner.heroData.baseData.id;
            if (heroID.Equals("Samurai_02_Hero") == false)
                return;

            if (skill.skillData.id.Contains("Skill_Samurai_Slash"))
            {
                int r = Random.Range(0, 100);
                if (r > 50)
                {
                    skill.owner.skillList.Find(x => x.skillData.id == "Skill_Samurai_SecondDash").Execute();
                }
            }
        }
    }

    void OnEndAddForceMove()
    {

    }
}

/// <summary> 소환 </summary>
public class SkillEffectSummon : SkillEffectBase
{
    public SkillEffectSummon(SkillBase skill, SkillEvent skillEvent) : base(skill, skillEvent)
    {
        this.skill = skill;
        this.skillEvent = skillEvent;
    }

    override public void TriggerEffect()
    {
        //풀링 안되어 있으면 함.
        GameObject summonObj = Battle.GetObjectInPool(skillEvent.summonObject.name);
        if (!summonObj)
        {
            summonObj = GameObject.Instantiate(skillEvent.summonObject, skill.owner.transform.position, Quaternion.identity, skill.owner.transform.parent) as GameObject;
            summonObj.transform.localPosition = Vector3.zero;
            summonObj.name = skillEvent.summonObject.name;
            //summonObj.GetComponent<Totem>().Init(owner);


            if (!summonObj.GetComponent<BattleGroupElement>())
                summonObj.AddComponent<BattleGroupElement>();

            Battle.AddObjectToPool(summonObj);
        }

        if (!summonObj)
            return;

        summonObj.SetActive(false);

        //랜덤한 위치, 배경에서 삐져나가지 않게 스폰
        float posX = UnityEngine.Random.Range(-1f, 1f);
        float posY = UnityEngine.Random.Range(-1f, 1f);

        Vector3 pos = skill.owner.transform.position + new Vector3(posX, posY, skill.owner.transform.position.z);
        BoxCollider unitArea = skill.owner.battleGroup.GetComponentInChildren<BoxCollider>();

        pos.y = Mathf.Clamp(pos.y, unitArea.bounds.min.y, unitArea.bounds.max.y);

        summonObj.transform.position = pos;

        if (skill.castTarget)
            summonObj.transform.position = skill.castTarget.transform.position;

        Totem t = summonObj.GetComponent<Totem>();
        if (t)
        {
            t.lifeTime = skill.skillData.summonTime;
            t.Init(skill.owner);
        }

        bool isNecromancy = skill.skillData.summonID.Contains("Skeleton");
        bool isDoppelganger = skill.skillData.summonID.Contains("Ninja");

        BattleHero hero = summonObj.GetComponent<BattleHero>();
        if (hero)
        {
            hero.team = skill.owner.team;

            HeroData hData = null;
            if (!string.IsNullOrEmpty(skill.skillData.summonID) && HeroManager.heroBaseDataDic.ContainsKey(skill.skillData.summonID))
                hData = new HeroData(HeroManager.heroBaseDataDic[skill.skillData.summonID]);

            Stat attackPower = skill.owner.stats.GetParam(StatType.AttackPower);
            //강령술 하드코딩. 시전자 공격력에 영향 받아서 hp, 공격력 결정
            if (isNecromancy)
            {
                hero.master = skill.owner;
                hData.baseData.maxHP = 1; /*attackPower.value * 2*/;//  (int)(skill.owner.attackPower * 2f);
                hData.baseData.attackPower = (int)(attackPower.value * 0.1f);//  (int) (skill.owner.attackPower * 0.1f);

                //강령술로 소환한 애들은 부활 불가
                hero.canResurrect = false;

                //강령술 대상이 된 애들도 부활 불가
                if(skill.castTarget)
                    skill.castTarget.canResurrect = false;

            }
           

            hero.lifeTime = skill.skillData.summonTime;

            hero.Init(skill.owner.battleGroup, hData, skill.owner.team);

            Stat statMaxHP = hero.stats.GetParam(StatType.MaxHP);
            if (statMaxHP != null)
                statMaxHP.baseValue = attackPower.value * 2;
            Stat statCurHP = hero.stats.GetParam(StatType.CurHP);
            statCurHP.value = statMaxHP.value;

            if (!isNecromancy)
                hero.isFinishSpawned = true;
            if (isDoppelganger)
            {
                hero.master = skill.owner;
                statMaxHP.value = 1d;
                statCurHP.value = statMaxHP.value;

                Stat statAttackPower = hero.stats.GetParam(StatType.AttackPower);
                statAttackPower.value = attackPower.value;

                hero.canResurrect = false;

            }


            //소환물 설정
            hero.isSummonded = true;

        }

        //소환된 애 오브젝트 활성                
        summonObj.SetActive(true);
        summonObj.GetComponent<BattleGroupElement>().SetBattleGroup(skill.owner.battleGroup);
        if (hero)
        {
            if (hero.team == BattleUnit.Team.Red)
                skill.owner.battleGroup.redTeamList.Add(hero);
            else if (hero.team == BattleUnit.Team.Blue)
                skill.owner.battleGroup.blueTeamList.Add(hero);

            if (isNecromancy)
            {                
                hero.skeletonAnimation.enabled = true;
                //Debug.Log(hero.skeletonAnimation.state.GetCurrent(0));
                hero.skeletonAnimation.state.ClearTrack(0);
                hero.skeletonAnimation.state.SetAnimation(0, "Resurrect", false);
                //hero.skeletonAnimation.state.Interrupt += OnInterruptResurrect;
                hero.skeletonAnimation.state.AddAnimation(0, hero.idleAnimation, true, 0f);
                //hero.StartCoroutine(CheckFinishResurrect(hero));
            }

            if(isDoppelganger)
            {
                //분신 위치 조정
                if (skill.owner.summonCount > 0)
                {
                    Vector3 summonPos = skill.owner.transform.position;
                    summonPos.x += -3f;
                    summonObj.transform.position = summonPos;
                }
                else
                {
                    Vector3 summonPos = skill.owner.transform.position;
                    summonPos.x += 3f;
                    summonObj.transform.position = summonPos;
                }


                hero.skeletonAnimation.enabled = true;
                hero.skeletonAnimation.state.ClearTrack(0);
                hero.skeletonAnimation.state.SetAnimation(0, "Skill_Clone", false);
                hero.skeletonAnimation.state.AddAnimation(0, hero.idleAnimation, true, 0f);

                hero.spineAnimationDie = hero.skeletonAnimation.SkeletonDataAsset.GetAnimationStateData().skeletonData.FindAnimation("Die_Clone");
            }
        }

        skill.owner.summonCount += 1;

        if (skill.onTriggerEvent != null)
            skill.onTriggerEvent(skill, skillEvent);
    }
    
    IEnumerator CheckFinishResurrect(BattleHero hero)
    {
        while (hero.skeletonAnimation.AnimationName != "Resurrect")
            yield return null;

        while (hero.skeletonAnimation.AnimationName == "Resurrect")
            yield return null;

        //hero.isFinishSpawned = true;
    }
}


