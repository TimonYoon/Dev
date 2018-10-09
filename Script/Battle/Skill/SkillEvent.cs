using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[System.Serializable]
public class SkillEventContainer
{
    [SerializeField]
    public List<SkillEvent> skillEventList = new List<SkillEvent>();
}


[System.Serializable]
public class SkillEvent
{
    public enum SkillEventType
    {
        DoNothing,
        ShowParticle,
        PlaySound,
        MeleeHit,
        FireProjectile,
        ExecuteSkill,
        Summon
    }

    [SerializeField]
    public string eventName = "";
        
    [SerializeField]
    public SkillEventType eventType = SkillEventType.DoNothing;
    
    public AudioSource audioSource = null;


    //-------------------------타격 파티클-------------------------------------------
    public GameObject hitEffect = null;
    //--------------------------------------------------------------------


    //-------------------------걍 파티클-------------------------------------------
    public GameObject particle = null;
    public bool attachToTarget = false;
    //--------------------------------------------------------------------


    //------------------------발사체 관련--------------------------------------------
    public GameObject projectile = null;    
    public Transform projectilePivot;
    //--------------------------------------------------------------------

    
    //-------------------------스킬 사용 관련-------------------------------------------
    public string executeSkillID = "";

    public int executeWeight = 10000;
    //--------------------------------------------------------------------



    public GameObject summonObject = null;
}
