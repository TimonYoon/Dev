using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

public class VisitedHeroData
{
    public VisitedHeroData(string _heroID)
    {
        heroID = _heroID;
    }

    /// <summary>영웅 아이디 </summary>
    public ObscuredString heroID { get; private set; }

    /// <summary> 영웅 베이스 데이터 </summary>
    public HeroBaseData baseData
    {
        get
        {
            HeroBaseData data = null;
            if (HeroManager.heroBaseDataDic.ContainsKey(heroID))
                data = HeroManager.heroBaseDataDic[heroID];

            return data;
        }
    }

    public ObscuredInt visitOrder { get; set; }

    public ObscuredFloat startTime { get; set; }

    /// <summary> 남은 시간 </summary>
    public ObscuredFloat remainingTime { get; set; }
}

