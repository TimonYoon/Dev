using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class CoolTimeGroup
{
    public CoolTimeGroup(string _id, float _coolTime)
    {
        id = _id;
        coolTime = _coolTime;
    }

    public string id { get; set; }

    public float coolTime { get; set; }
}

public class CoolTimeController : MonoBehaviour
{
    public delegate void BattleDelegate();

    public BattleDelegate onFinishCoolTime;
        
    public BattleUnit unit { get; set; }

    List<CoolTimeGroup> _coolTimeList = new List<CoolTimeGroup>();
    public List<CoolTimeGroup> coolTimeList
    {
        get { return _coolTimeList; }
        private set { _coolTimeList = value; }
    }

    public Dictionary<string, float> coolTimeDic = new Dictionary<string, float>();

    //#############################################################################
    public void Init()
    {
        //실제로 효과가 발생할 때에만 쿨타임이 적용
        for(int i = 0; i < unit.skillList.Count; i++)
        {
            //unit.skillList[i].onExcuteSkill += OnExcuteSkill;
            unit.skillList[i].onTriggerEvent += OnTriggerEvent;
        }
        for (int i = 0; i < unit.counterSkillList.Count; i++)
        {
            //unit.skillList[i].onExcuteSkill += OnExcuteSkill;
            unit.counterSkillList[i].onTriggerEvent += OnTriggerEvent;
        }
    }

    //스킬에 설정된 이벤트가 발생했을 때
    void OnTriggerEvent(SkillBase skill, SkillEvent skillEvent)
    {
        if (skillEvent.eventType != SkillEvent.SkillEventType.MeleeHit
            && skillEvent.eventType != SkillEvent.SkillEventType.FireProjectile
            && skillEvent.eventType != SkillEvent.SkillEventType.ExecuteSkill
            && skillEvent.eventType != SkillEvent.SkillEventType.Summon)
            return;
        
        if (!string.IsNullOrEmpty(skill.skillData.coolTime1ID))
            ApplyCoolTime(skill.skillData.coolTime1ID, skill.skillData.coolTime1);

        if (!string.IsNullOrEmpty(skill.skillData.coolTime2ID))
            ApplyCoolTime(skill.skillData.coolTime2ID, skill.skillData.coolTime2);
    }

    void OnExcuteSkill(SkillBase skill)
    {
        if (!skill)
        {
            Debug.LogWarning("skil is null???");
            return;
        }

        if(skill.skillData == null)
        {
            Debug.LogWarning("skil date is null");
            return;
        }

        if (!string.IsNullOrEmpty(skill.skillData.coolTime1ID))
            ApplyCoolTime(skill.skillData.coolTime1ID, skill.skillData.coolTime1);

        if (!string.IsNullOrEmpty(skill.skillData.coolTime2ID))
            ApplyCoolTime(skill.skillData.coolTime2ID, skill.skillData.coolTime2);

    }
    

    public void ApplyCoolTime(string coolTimeID, float _coolTime)
    {
        for (int i = 0; i < coolTimeList.Count; i++)
        {
            if (coolTimeList[i].id == coolTimeID)
                return;
        }
        
        StartCoroutine(ApplyCoolTimeA(coolTimeID, _coolTime));
    }

    public IEnumerator ApplyCoolTimeA(string coolTimeID, float _coolTime)
    {
        if (string.IsNullOrEmpty(coolTimeID))
            yield break;
        
        if (coolTimeDic.ContainsKey(coolTimeID))
            coolTimeDic[coolTimeID] = _coolTime;
        else
            coolTimeDic.Add(coolTimeID, _coolTime);

        CoolTimeGroup ctg = new CoolTimeGroup(coolTimeID, _coolTime);
        coolTimeList.Add(ctg);
        
        for (int i = 0; i < unit.skillList.Count; i++)
        {
            SkillBase skill = unit.skillList[i];
            if (skill.skillData == null)
            {
                Debug.LogWarning("no skilldata : " + unit.name);
                continue;
            }

            if (skill.skillData.coolTime1ID == coolTimeID || skill.skillData.coolTime2ID == coolTimeID)
                skill.isCoolTime = true;
        }

        for (int i = 0; i < unit.counterSkillList.Count; i++)
        {
            SkillBase skill = unit.counterSkillList[i];
            if (skill.skillData == null)
            {
                Debug.LogWarning("no skilldata : " + unit.name);
                continue;
            }

            if (skill.skillData.coolTime1ID == coolTimeID || skill.skillData.coolTime2ID == coolTimeID)
                skill.isCoolTime = true;
        }
        
        //쿨타임 남은 시간 표기를 위해 남은 시간 계산
        float coolTime = _coolTime;
        float startTime = Time.time;
        while (coolTime > 0f)
        {
            float elapsedTime = Time.time - startTime;
            coolTime = _coolTime - elapsedTime;
            ctg.coolTime = coolTime;
            
            yield return null;
        }

        coolTimeDic.Remove(coolTimeID);

        //yield return new WaitForSeconds(coolTime);

        //쿨타임 끝난 애는 리스트에서 제거
        coolTimeList.Remove(ctg);

        for (int i = 0; i < unit.skillList.Count; i++)
        {
            SkillBase skill = unit.skillList[i];
            if(!coolTimeDic.ContainsKey(skill.skillData.coolTime1ID) && !coolTimeDic.ContainsKey(skill.skillData.coolTime2ID))
                skill.isCoolTime = false;

            //if (skill.skillData.coolTime1ID == coolTimeID || skill.skillData.coolTime2ID == coolTimeID)
            //    skill.isCoolTime = false;
        }
        for (int i = 0; i < unit.counterSkillList.Count; i++)
        {
            SkillBase skill = unit.counterSkillList[i];
            if (!coolTimeDic.ContainsKey(skill.skillData.coolTime1ID) && !coolTimeDic.ContainsKey(skill.skillData.coolTime2ID))
                skill.isCoolTime = false;

            //if (skill.skillData.coolTime1ID == coolTimeID || skill.skillData.coolTime2ID == coolTimeID)
            //    skill.isCoolTime = false;
        }

        //쿨타임 끝났다는 콜백 날림
        if (onFinishCoolTime != null)
            onFinishCoolTime();
    }
}
