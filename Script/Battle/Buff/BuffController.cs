using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using B83.ExpressionParser;
using System;

public class BuffController : MonoBehaviour {
    public List<Buff> buffList = new List<Buff>();

    BattleUnit _owner;
    public BattleUnit owner
    {
        get { return _owner; }
        set
        {
            if (_owner != null && _owner.battleGroup != null)
            {
                _owner.battleGroup.onRestartBattle -= OnRestartBattle;
                _owner.battleGroup.redTeamList.onAdd -= OnAddHeroToTeamList;
                _owner.battleGroup.redTeamList.onRemovePost -= OnRemoveHeroFromTeamList;
                _owner.battleGroup.blueTeamList.onAdd -= OnAddHeroToTeamList;
                _owner.battleGroup.blueTeamList.onRemovePost -= OnRemoveHeroFromTeamList;

                TerritoryManager.onAddPlace -= OnAddPlace;
                TerritoryManager.onChangedPlaceData -= OnChangedPlace;
            }
                

            _owner = value;

            if (value != null && value.battleGroup != null)
            {
                if(value.team == BattleUnit.Team.Red)
                {
                    value.battleGroup.onRestartBattle += OnRestartBattle;
                    value.battleGroup.redTeamList.onAdd += OnAddHeroToTeamList;
                    value.battleGroup.redTeamList.onRemovePost += OnRemoveHeroFromTeamList;

                    TerritoryManager.onAddPlace += OnAddPlace;
                    TerritoryManager.onChangedPlaceData += OnChangedPlace;

                    UpdatePlaceBuff();
                }
                else
                {
                    value.battleGroup.blueTeamList.onAdd += OnAddHeroToTeamList;
                    value.battleGroup.blueTeamList.onRemovePost += OnRemoveHeroFromTeamList;
                }
            }
        }
    }

    void OnAddPlace()
    {
        UpdatePlaceBuff();
    }

    void OnChangedPlace()
    {
        UpdatePlaceBuff();
    }

    void OnRestartBattle(BattleGroup b)
    {
        for (int i = 0; i < b.redTeamList.Count; i++)
            b.redTeamList[i].ExcutePassiveSkill();

        UpdatePlaceBuff();
    }

    void OnAddHeroToTeamList(BattleHero hero)
    {
        //if (hero.team == BattleUnit.Team.Red)
        //    Debug.Log("Add " + hero.heroData.baseData.id);

        //if (hero.team == BattleUnit.Team.Red)
        //    Debug.Log("add hero to team list. " + hero.heroData.baseData.id + ", " + owner.battleGroup.redTeamList.Count + ", " + owner.battleGroup.originalMember.Count);

        //foreach (SkillBase skill in owner.passiveSkillList)
        //{
        //    if (skill.skillData.targetFilter == SkillBase.TargetFilter.FriendlyAll)
        //    {
        //        Buff passive = hero.buffController.buffList.Find(x => x.id == skill.skillData.buffID && x.isActive && x.owner == owner);
        //        if (passive == null || passive.owner == null)
        //            AttachBuff(owner, hero, skill.skillData.buffID, skill.skillData.buffStack);
        //    }                

        //}

        if (owner.battleGroup != null)
            owner.ExcutePassiveSkill();


    }

    void UpdatePlaceBuff()
    {
        if (owner.team != BattleUnit.Team.Red)
            return;

        for (int i = 0; i < TerritoryManager.Instance.myPlaceList.Count; i++)
        {
            PlaceData placeData = TerritoryManager.Instance.myPlaceList[i];
            string buffID = placeData.placeBaseData.buffID;

            if (string.IsNullOrEmpty(buffID))
                continue;

            if (placeData.placeBaseData.type != "Battle")
                continue;

            int stack = placeData.placeLevel;
            Buff buff = buffList.Find(x => x.baseData != null && x.baseData.id == buffID);
            if(buff != null)
            {
                stack = placeData.placeLevel - buff.stack;
            }

            //Debug.Log(owner.heroData.baseData.id + ", " + placeData.placeID + ", " + placeData.placeLevel + ", " + stack + ", " + placeData.placeBaseData.buffID);

            AttachBuff(owner, buffID, stack);
        }
    }

    void OnRemoveHeroFromTeamList(BattleHero hero)
    {
        //if (hero.team == BattleUnit.Team.Red)
        //    Debug.Log("Remove " + hero.heroData.baseData.id);

        //if (owner.heroData.baseData.id.Contains("Shaman") && hero.team == BattleUnit.Team.Red)
        //    Debug.Log("Remove hero from team list. " + hero.heroData.baseData.id + ", " + owner.battleGroup.redTeamList.Count + ", " + owner.battleGroup.originalMember.Count);

        foreach (SkillBase skill in hero.passiveSkillList)
        {
            //Debug.Log("add passive to red " + skill.skillData.id + " - " + hero.heroData.heroID);

            if (skill.skillData.targetFilter == SkillBase.TargetFilter.FriendlyAll)
            {
                for (int i = 0; i < buffList.Count; i++)
                {
                    Buff buff = buffList[i];
                    if (buff.owner != hero || buff.baseData.id != skill.skillData.buffID)
                        continue;

                    DetachBuff(buff);
                }
            }
        }
    }

    Buff GetBuffFromPool(string id)
    {
        Buff buff = buffList.Find(x => x.id == id && !x.isActive);
        if (buff == null)
        {
            buff = new Buff();
            buffList.Add(buff);
        }

        return buff;
    }

    public void TriggerBuff(BattleUnit buffOwner, string trigger, int stack = 1, Buff refBuff = null)
    {
        for (int a = 0; a < buffList.Count; a++)
        {
            Buff buff = buffList[a];
            if (buff.baseData.trigger != trigger)
                continue;

            //BattleUnit target = null;
            //if (buff.baseData.triggerTarget == "SkillTarget")
            //    target = this;
            //else if (buff.baseData.triggerTarget == "BuffTarget")
            //    target = attacker.master;

            if(trigger == "OnKill")
            {
                if (!owner.isDie)
                    AttachBuff(buffOwner, buff.baseData.triggerBuff, 1, buff);
            }

            
        }
    }

    public Buff AttachBuff(BattleUnit buffOwner, string buffID, int stack = 1, Buff refBuff = null)
    {
       
        //버프 아이디 없거나, 정의되지 않았으면 버프 적용 안 함
        if (string.IsNullOrEmpty(buffID) || !GameDataManager.buffBaseDataDic.ContainsKey(buffID))
        {
            Debug.LogWarning("[BuffManager]Invalid buff id : " + buffID);
            return null;
        }

        BuffBaseData buffBaseData = GameDataManager.buffBaseDataDic[buffID];
        if (buffBaseData == null)
            return null;
        

        //---------------------------------------------------------
        //이미 적용 중인 버프일 경우
        Buff buff = null;
        if (buffBaseData.isUnique)
            buff = buffList.Find(x => x.id == buffID && x.target == owner);
        else
            buff = buffList.Find(x => x.id == buffID && x.target == owner && x.owner == buffOwner);
        
        if (buff == null)
        {
            buff = GetBuffFromPool(buffID);
        }

        if (buff == null)
            return null;
        
        // maxStack일 경우 더 이상 접근하지 않는다. 
        if (buff.baseData != null && buff.stack == buff.baseData.maxStackCount)
            return null;
        //Debug.Log("버프 적용 : " + buffOwner.heroData.heroName + " / " + buffID);

        //버프 적용 or 갱신
        buff.Init(buffOwner, owner, buffID, stack, refBuff);

        //if(buff.baseData.id.Equals("Buff_Vampire_Passive3"))
        //{
        //    if (owner.skillList.Find(x => x.skillData.id == "Skill_Vampire_NormalAttack"))
        //    {
        //        owner.skillList.Find(x => x.skillData.id == "Skill_Vampire_NormalAttack").skillData.maxTargetCount = 3;
        //    }
        //}

        if (buffList.Count > 0 && owner.buffController.buffList.Find(x => x.baseData.effect == "ImmuneCC") != null && (buff.baseData.blockMove || buff.baseData.blockAttack || buff.baseData.airborne))
        {
            if (owner.onHit != null)
                owner.onHit(0d, "ImmuneDamage");

            return null;
        }

        //버프 적용될 때 효과
        if (buff.attachBehavior != null)
            buff.attachBehavior.ApplyEffect();

        //지속시간 적용
        if (buff.baseData.duration > 0f)
        {
            if (buff.coroutineDuration != null)
            {
                StopCoroutine(buff.coroutineDuration);
                buff.coroutineDuration = null;
            }

            buff.coroutineDuration = StartCoroutine(ApplyDuration(buff));
        }

        //이동 불가, 공격 불가
        if (buff.baseData.blockMove)
            owner.isBlockMove = true;

        if (buff.baseData.blockAttack)
            owner.isBlockAttack = true;

        //공중에 뜸
        if (buff.baseData.airborne)
            owner.airborne = true;


        //타게팅 방어 효과
        if (buff.baseData.notTargeting)
            owner.notTargeting = true;

        return buff;
    }


    IEnumerator ApplyDuration(Buff buff)
    {
        float lastTickTime = Time.time;
        while (Time.time < buff.startTime + buff.baseData.duration)
        {
            //Debug.Log("2");
            buff.remainTime = buff.startTime + buff.baseData.duration - Time.time;

            //일정 주기 마다 효과 발생
            if (Time.time > lastTickTime + buff.baseData.interval)
            {
                if (buff.tickBehavior != null)
                {
                    //Debug.Log(owner.heroData.heroName + " 공격");
                    buff.tickBehavior.ApplyEffect();
                }
                    

                lastTickTime = Time.time;
            }

            if (buff == null || buff.stack == 0)
                break;

            //시전자 사망하면 바로 종료
            //Todo: 예외가 있을 수 있음
            if (buff.owner.isDie)
            {
                break;
            }

            yield return null;
        }

        //buff.coroutineDuration = null;

        DetachBuff(buff);
    }

    /// <summary> buffID인 버프 전부 제거 </summary>
    public void DetachBuff(string buffID)
    {
        for(int i = 0; i < buffList.Count; i++)
        {
            Buff buff = buffList[i];
            if (buff.id != buffID)
                continue;

            DetachBuff(buff);
        }
    }

    public void DetachBuff(Buff buff)
    {
        if (buff == null)
            return;

        if (buff.detachBehavior != null)
            buff.detachBehavior.ApplyEffect();

        if (buff.coroutineDuration != null)
        {
            StopCoroutine(buff.coroutineDuration);
            buff.coroutineDuration = null;
        }

        if (buff.detachSkillBehavior != null)
            buff.detachSkillBehavior.ApplyEffect();

        //이동 불가 효과 해제
        if (owner.isBlockMove && buffList.Find(x => x != buff && x.baseData != null && x.target != null && x.baseData.blockMove) == null)
            owner.isBlockMove = false;

        //공격 불가 효과 해제
        if (owner.isBlockAttack && buffList.Find(x => x != buff && x.baseData != null && x.target != null && x.baseData.blockAttack) == null)
            owner.isBlockAttack = false;

        //띄어짐 효과 해제
        if (owner.airborne && buffList.Find(x => x != buff && x.baseData != null && x.target != null && x.baseData.airborne) == null)
            owner.airborne = false;

        //타게팅 방어 효과 해제
        if (owner.notTargeting && buffList.Find(x => x != buff && x.baseData != null && x.target != null && x.baseData.notTargeting) == null)
            owner.notTargeting = false;

        //if (buff.baseData.id.Equals("Buff_Vampire_Passive3"))
        //{
        //    if (owner.skillList.Find(x => x.skillData.id == "Skill_Vampire_NormalAttack"))
        //    {
        //        owner.skillList.Find(x => x.skillData.id == "Skill_Vampire_NormalAttack").skillData.maxTargetCount = 1;
        //    }
        //}

        buff.Reset();
        //buffList.Remove(buff);
    }

    public void DetachAllBuffs(bool includePassive = true)
    {
        if (buffList.Count < 1)
            return;

        for (int i = buffList.Count - 1; i >= 0; i--)
        {            
            Buff buff = buffList[i];

            if (buff.target != owner)
                continue;

            if (!includePassive && buff.baseData.duration == 0)
                continue;

            DetachBuff(buff);
        }
    }
    
    

    IEnumerator AddParticle(Buff buff)
    {
        if (string.IsNullOrEmpty(buff.baseData.effectPrefab))
            yield break;

        string effectPrefabName = buff.baseData.effectPrefab;
        string buffID = buff.baseData.id;
        

        Transform parent = owner.transform;
        if (owner.collider)
            parent = owner.collider.transform;

        //이펙트 연출 띄움
        GameObject effectObj = Battle.GetObjectInPool(effectPrefabName);        
        if(!effectObj)
        {
            GameObject effectPrefab = null;
            yield return StartCoroutine(AssetLoader.Instance.LoadGameObjectAsync("buff", buff.baseData.effectPrefab, x => effectPrefab = x));

            if (owner.isDie)
                yield break;

            if (!effectPrefab)
                yield break;
            
            

            effectObj = Instantiate(effectPrefab, parent, false);
            effectObj.name = effectPrefabName;

            Battle.AddObjectToPool(effectObj);
        }

        effectObj.transform.parent = parent;
        effectObj.transform.position = parent.transform.position;

        buff.effectObject = effectObj;

        OrderController oc = effectObj.GetComponent<OrderController>();
        if (!oc)
            oc = effectObj.AddComponent<OrderController>();
        oc.parent = owner.GetComponent<OrderController>();

        BattleGroupElement be = effectObj.GetComponent<BattleGroupElement>();
        if (!be)
            be = effectObj.AddComponent<BattleGroupElement>();
        be.SetBattleGroup(owner.battleGroup);

        effectObj.SetActive(true);

        if (!buff.effectObject.activeInHierarchy)
            yield break;

        ParticleSystem particle = buff.effectObject.GetComponentInChildren<ParticleSystem>();
        if (particle)
            particle.Play();

        Animation anim = buff.effectObject.GetComponentInChildren<Animation>();
        if (anim)
            anim.Play();
        
        Animator animator = buff.effectObject.GetComponentInChildren<Animator>();
        if (animator)
            animator.SetTrigger("start");
    }
    

    IEnumerator DetatchBuffA(Buff buff)
    {
        GameObject effectObj = buff.effectObject;

        if (effectObj)
        {
            ParticleSystem particle = effectObj.GetComponentInChildren<ParticleSystem>();
            if (particle)
                particle.Stop();

            Animation anim = effectObj.GetComponentInChildren<Animation>();
            if (anim)
                anim.Stop();

            Animator animator = effectObj.GetComponentInChildren<Animator>();
            if (animator && animator.isActiveAndEnabled)
            {
                animator.SetTrigger("finish");

                //while (!animator.GetCurrentAnimatorStateInfo(0).IsName("Init"))
                    //yield return null;
            }

            //effectObj.SetActive(false);
            //while (particle.isPlaying)
            //    yield return null;
        }

        buff.Reset();

        //buffList.Remove(buff);

        //이동 불가 효과 해제
        if (owner.isBlockMove && buffList.Find(x => x.baseData != null && x.baseData.blockMove) == null)
            owner.isBlockMove = false;

        //공격 불가 효과 해제
        if (owner.isBlockAttack && buffList.Find(x => x.baseData != null && x.baseData.blockAttack) == null)
            owner.isBlockAttack = false;

        //에어본 효과 해제
        if (owner.airborne && buffList.Find(x => x.baseData != null && x.baseData.airborne) == null)
            owner.airborne = false;
        
        //타게팅 방어 효과 해제
        if (owner.notTargeting && buffList.Find(x => x.baseData != null && x.baseData.notTargeting) == null)
            owner.notTargeting = false;

        //UpdateParam();

        yield break;
    }


    ExpressionParser parser = new ExpressionParser();


    float GetPower(Buff buff, string param)
    {
        //owner.attackPower * 1 - target.defensePower 이런 형태로 되어 있음
        string paramString = param;

        if (string.IsNullOrEmpty(paramString))
        {            
            return 0f;
        }

        //공백 지우기
        paramString = paramString.Replace(" ", string.Empty);

        paramString = paramString.Replace("stack", buff.stack.ToString());

        //수식을 제외한 값을 문자열로 따로 저장 후 파싱. (owner.attackPower 같은 것들 선계산 하기 위함)
        string[] seperator = { "+", "-", "*", "/", "(", ")", "<", ">", "=" };
        string[] ss = paramString.Split(seperator, System.StringSplitOptions.None);
        for (int i = 0; i < ss.Length; i++)
        {
            if (ss[i].Contains("target"))
            {
                paramString = paramString.Replace(ss[i], GetParamValue(buff.target, ss[i]).ToString());
            }
            else if (ss[i].Contains("owner") || ss[i].Contains("self"))
            {
                paramString = paramString.Replace(ss[i], GetParamValue(buff.owner, ss[i]).ToString());
            }
            else if (ss[i].Contains("master"))
            {
                paramString = paramString.Replace(ss[i], GetParamValue(buff.owner.master, ss[i]).ToString());
            }
        }


        float finalValue = (float)parser.Evaluate(paramString);

        //Debug.Log(skillData.name + " attackPower: " + paramString + " = " + finalValue);

        return finalValue;
    }

    float GetParamValue(BattleUnit unit, string paramString)
    {
        //selft, target이런거 제외            
        //self. target. party. global. (타입 구분은 미정)
        paramString = paramString.Replace("owner.", "");
        paramString = paramString.Replace("self.", "");
        paramString = paramString.Replace("target.", "");
        paramString = paramString.Replace("master.", "");        
        paramString = paramString.Trim();

        Expression exp = parser.EvaluateExpression(paramString);

        List<string> keys = exp.Parameters.Keys.ToList();
        for (int i = 0; i < keys.Count; i++)
        {
            string propertyName = keys[i];

            StatType statType = (StatType)System.Enum.Parse(typeof(StatType), propertyName, true);
            Stat stat = unit.stats.GetParam(statType);

            if(stat != null)
            {
                exp.Parameters[keys[i]].Value = stat.value;
            }
        }

        return (float)exp.Value;
    }
}
