using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public partial class BattleUnit
{
    public BattleDelegate onChangedHP;
    public BattleDelegate onChangedLevel;

    //public BattleDelegate onChangedMaxHPRatio;
    //public BattleDelegate onChangedMaxHPValue;
    //public BattleDelegate onChangedAttackPowerRatio;
    //public BattleDelegate onChangedAttackPowerValue;
    //public BattleDelegate onChangedDefensePowerRatio;
    //public BattleDelegate onChangedDefensePowerValue;


    //public int level { get; set; }

    ///// <summary> 현재 캐릭터의 경험치량 </summary>
    //public double exp { get; set; }
    
    public double LevelUpExpValue
    {
        get
        {
            double placeModify = 0f;
            for (int i = 0; i < TerritoryManager.Instance.myPlaceList.Count; i++)
            {
                PlaceData placeData = TerritoryManager.Instance.myPlaceList[i];
                if (placeData.placeBaseData.type == "Battle_DecreaseHeroLevelUpExp")
                {
                    float a = 0f;
                    float.TryParse(placeData.placeBaseData.formula, out a);
                    placeModify += a * 0.01f * placeData.placeLevel;
                }
            }
            double baseValue = 200d;
            if (placeModify > 0f)
                baseValue = 200d / (1 + placeModify);

            //Debug.Log(placeModify + ", " + baseValue);

            double exp = baseValue * System.Math.Pow(1.2d, heroData.level) - baseValue * System.Math.Pow(1.2d, heroData.level - 1);

            return exp;
        }
    }

    //#######################   체력 관련  #########################################

    /// <summary> 최대 체력 - 최종 </summary>
    public double maxHP
    {
        get
        {
            return stats.GetParam(StatType.MaxHP).value;

            //return maxHPBase * (1f + maxHPRatio * 0.0001f) + maxHPValue;
        }
    }

    public double curHP
    {
        get
        {
            return stats.GetParam(StatType.CurHP).value;
        }
        protected set
        {
            Stat statCurHP = stats.GetParam(StatType.CurHP);

            bool isChanged = statCurHP.baseValue != value;

            statCurHP.baseValue = value;

            if (statCurHP.baseValue > maxHP)
                statCurHP.baseValue = maxHP;

            if (statCurHP.baseValue <= 0d)
            {
                statCurHP.baseValue = 0d;

                isDie = true;
            }
            else
            {
                isDie = false;
            }

            //체력 변경 콜백
            if (isChanged && onChangedHP != null)
                onChangedHP();
        }
    }
    

    //#######################   이동속도  #########################################
    /// <summary> 대기/걷기 이동 애니메이션 속도를 올려줌 </summary>
    public float moveSpeedRate
    {
        get { return 1 + moveSpeedRatio * 0.0001f; }
    }

    /// <summary> 이동속도 n% 증가. </summary>
    public float moveSpeedRatio { get; protected set; }

    protected void RecalculateBaseParams()
    {
        float gradeModify = 1f;

        gradeModify = Mathf.Pow(1.3f, heroData.heroGrade - 1);

        var statHP = stats.CreateOrGetStat<ModifiableStat>(StatType.MaxHP);
        var statAttackPower = stats.CreateOrGetStat<ModifiableStat>(StatType.AttackPower);
        var statDefensePower = stats.CreateOrGetStat<ModifiableStat>(StatType.DefensePower);
        if (team == Team.Red)
        {
            int level = heroData.rebirth * 100 + heroData.rebirth + heroData.enhance + heroData.level - 1;
            statHP.baseValue = heroData.baseData.maxHP * gradeModify * System.Math.Pow(1.05, level);
            statAttackPower.baseValue = heroData.baseData.attackPower * gradeModify * System.Math.Pow(1.05, level);
            statDefensePower.baseValue = heroData.baseData.defensePower * gradeModify * System.Math.Pow(1.05, level);
        }
        else
        {
            statHP.baseValue = heroData.baseData.maxHP * gradeModify * power * 0.1f;
            statAttackPower.baseValue = heroData.baseData.attackPower * gradeModify * power * 0.1f;
            statDefensePower.baseValue = heroData.baseData.defensePower * gradeModify * power * 0.1f;
        }
    }

    public double power { get; set; }
}
