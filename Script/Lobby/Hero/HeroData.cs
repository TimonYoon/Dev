using UnityEngine;
using System.Collections;
using B83.ExpressionParser;
using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using LitJson;
using CodeStage.AntiCheat.ObscuredTypes;

public class TrainingData
{
    public string key { get; set; }
    public string paramName { get; set; }
    public ObscuredInt training { get; set; }
}



public delegate void SimpleDelegate();    


public class HeroData
{
    public delegate void HeroDataCallBack(PropertyInfo p);

    /// <summary> 무언가 바뀌었을 때... </summary>
    public HeroDataCallBack onChangedValue;

    public HeroBaseData baseData;

    public SimpleDelegate onChangedLevel;

    public enum HeroType
    {
        NotDefined,
        /// <summary> 전투 영웅 </summary>
        Battle,
        /// <summary> 내정 영웅 </summary>
        NonBattle
    }

    public StatCollection stats = new StatCollection();

    public HeroType heroType = HeroType.NotDefined;

    public enum HeroBattleType
    {
        None,
        Ground,
        Air,
    }

    public HeroBattleType heroBattleType = HeroBattleType.None;

    ObscuredInt _level = 1;
    /// <summary> 영웅 레벨 </summary>
    public ObscuredInt level
    {
        get { return _level; }
        set
        {
            if (_level == value)
                return;

            _level = value;

            RecalculateStats();

            //레벨 변동 관련 콜백
            if (onChangedLevel != null)
                onChangedLevel();

            if (onChangedValue != null)
            {
                PropertyInfo propertyInfo = GetType().GetProperty("level");
                onChangedValue(propertyInfo);

            }
        }
    }

    public float battleStartTime;
    public bool isGetProficiencyReward = false;
    /// <summary> 최대 숙련치 (24시간 -> 초) </summary>
    public float maxProficiencyTime { get { return 86400f; } }

    float _proficiencyTime = 0;
    /// <summary> 숙련 수치 </summary>
    public float proficiencyTime
    {
        get { return _proficiencyTime; }
        set
        {
            if (_proficiencyTime >= value)
                return;

            if (_proficiencyTime == maxProficiencyTime)
                return;

            if (maxProficiencyTime <= value)
                _proficiencyTime = maxProficiencyTime;
            else
                _proficiencyTime = value;

            if (onChangedValue != null)
            {
                PropertyInfo propertyInfo = GetType().GetProperty("proficiencyTime");
                onChangedValue(propertyInfo);

            }
        }
    }


    /// <summary> 레벨업에 사용된 누적 경험치 </summary>
    public ObscuredDouble exp = 0;

    ObscuredString _battleGroupID = string.Empty;
    /// <summary> 전투지역Id </summary>
    public ObscuredString battleGroupID
    {

        get { return _battleGroupID; }
        set
        {
            if (_battleGroupID == value)
                return;

            _battleGroupID = value;

            if (onChangedValue != null)
            {
                PropertyInfo propertyInfo = GetType().GetProperty("battleGroupID");
                onChangedValue(propertyInfo);
            }
        }
    }

    public void InitBaseData(HeroBaseData data)
    {
        baseData = data;

        heroID = data.id;
        heroName = data.name;
        heroGrade = data.grade;
        heroImageName = data.image;
        assetBundle = data.assetBundle;
        prefab = data.prefab;

        string id = data.id;

        if (id.Contains("Territory"))
            heroType = HeroData.HeroType.NonBattle;
        else
            //전투 전용
            heroType = HeroData.HeroType.Battle;

        heroBattleType = data.type;


        trainingTypeID = data.trainingTypeID;

        InitStats();
    }

    public HeroData(HeroBaseData _data) 
    {
        baseData = _data;
        
        heroID = _data.id;
        heroName = _data.name;
        heroGrade = _data.grade;
        heroImageName = _data.image;
        assetBundle = _data.assetBundle;
        prefab = _data.prefab;

        string _id = _data.id;
        if (_id.Contains("Territory"))
            heroType = HeroData.HeroType.NonBattle;
        else
            //전투 전용
            heroType = HeroData.HeroType.Battle;

        heroBattleType = _data.type;

        trainingTypeID = _data.trainingTypeID;
        
        InitStats();
    }

    /// <summary> 기본 스탯 초기화 </summary>
    void InitStats()
    {
        //전투, 내정 영웅 기본 스탯이 다름
        if (heroType == HeroType.Battle)
        {
            stats.CreateOrGetStat<ModifiableStat>(StatType.MaxHP);
            stats.CreateOrGetStat<ModifiableStat>(StatType.CurHP);
            stats.CreateOrGetStat<ModifiableStat>(StatType.AttackPower);
            stats.CreateOrGetStat<ModifiableStat>(StatType.DefensePower);
            stats.CreateOrGetStat<ModifiableStat>(StatType.MoveSpeed).baseValue = 10000;
            stats.CreateOrGetStat<ModifiableStat>(StatType.AttackSpeed).baseValue = 10000;
        }
        else if (heroType == HeroType.NonBattle)
        {
            stats.CreateOrGetStat<ModifiableStat>(StatType.ProductionPower);
            stats.CreateOrGetStat<ModifiableStat>(StatType.CollectPower);
            stats.CreateOrGetStat<ModifiableStat>(StatType.TaxPower);
        }

        RecalculateStats();
    }

    public void RecalculateStats(bool isHero = true, double rating = 0d)
    {
        float gradeModify = 1f;

        gradeModify = Mathf.Pow(1.3f, heroGrade - 1);

        if(heroType == HeroType.Battle)
        {
            var statHP = stats.CreateOrGetStat<ModifiableStat>(StatType.MaxHP);
            var statAttackPower = stats.CreateOrGetStat<ModifiableStat>(StatType.AttackPower);
            var statDefensePower = stats.CreateOrGetStat<ModifiableStat>(StatType.DefensePower);

            if (isHero)
            {
                int level = rebirth * 100 + rebirth + enhance + this.level - 1;
                if(id != "test")
                {
                    statHP.baseValue = baseData.maxHP * gradeModify * System.Math.Pow(1.05, level);
                    statAttackPower.baseValue = baseData.attackPower * gradeModify * System.Math.Pow(1.05, level);
                }
                statDefensePower.baseValue = baseData.defensePower * gradeModify * System.Math.Pow(1.05, level);
            }
            else
            {
                statHP.baseValue = baseData.maxHP * gradeModify * rating * 0.35d;
                statAttackPower.baseValue = baseData.attackPower * gradeModify * rating * 0.35d;
                statDefensePower.baseValue = baseData.defensePower * gradeModify * rating * 0.35d;
            }
        }
        else if (heroType == HeroType.NonBattle)
        {
            var statProductionPower = stats.CreateOrGetStat<ModifiableStat>(StatType.ProductionPower);
            var statCollectPower = stats.CreateOrGetStat<ModifiableStat>(StatType.CollectPower);
            var statTaxPower = stats.CreateOrGetStat<ModifiableStat>(StatType.TaxPower);
            if(isHero)
            {
                int level = rebirth * 100 + rebirth + enhance + this.level - 1;
                statProductionPower.baseValue = baseData.productionPower * gradeModify * System.Math.Pow(1.05, level);
                statCollectPower.baseValue = baseData.collectPower * gradeModify * System.Math.Pow(1.05, level);
                statTaxPower.baseValue = baseData.taxPower * gradeModify * System.Math.Pow(1.05, level);
            }
        }
    }

    public void InitPvPData(HeroData hero)
    {
        id = hero.id;
        heroID = hero.heroID;
        enhance = hero.enhance;
        rebirth = hero.rebirth;
        limitBreak = hero.limitBreak;
        training1 = hero.training1;
        training2 = hero.training2;
        training3 = hero.training3;

        InitTrainingData();
    }
    public void InitTestData(string _heroID,double hp = 100000, double ap = 1,double df = 1)
    {
        id = "test";
        heroID = _heroID;
        enhance = 0;
        rebirth = 0;
        limitBreak = 0;
        training1 = 0;
        training2 = 0;
        training3 = 0;

        
        var statHP = stats.CreateOrGetStat<ModifiableStat>(StatType.MaxHP);
        statHP.baseValue = hp;

        var statAttackPower = stats.CreateOrGetStat<ModifiableStat>(StatType.AttackPower);
        statAttackPower.baseValue = ap;

        var statDefensePower = stats.CreateOrGetStat<ModifiableStat>(StatType.DefensePower);
        statDefensePower.baseValue = df;
    }
    public void InitServerData(JsonData serverJsonData)
    {
        if (serverJsonData.ContainsKey("id"))
            id = serverJsonData["id"].ToString();

        if (serverJsonData.ContainsKey("heroID"))
            heroID = serverJsonData["heroID"].ToString();

        if (serverJsonData.ContainsKey("enhance"))
            enhance = serverJsonData["enhance"].ToInt();

        if (serverJsonData.ContainsKey("proficiency"))
            proficiencyTime = serverJsonData["proficiency"].ToFloat();

        if (serverJsonData.ContainsKey("isGetProficiencyReward"))
            isGetProficiencyReward = serverJsonData["isGetProficiencyReward"].ToBool();

        if (serverJsonData.ContainsKey("rebirth"))
            rebirth = serverJsonData["rebirth"].ToInt();

        if (serverJsonData.ContainsKey("limitBreak"))
            limitBreak = serverJsonData["limitBreak"].ToInt();

        if (serverJsonData.ContainsKey("training1"))
            training1 = serverJsonData["training1"].ToInt();

        if (serverJsonData.ContainsKey("training2"))
            training2 = serverJsonData["training2"].ToInt();

        if (serverJsonData.ContainsKey("training3"))
            training3 = serverJsonData["training3"].ToInt();


        //if (heroType == HeroType.NonBattle)
        //{
        //    int level = rebirth * 100 + rebirth + (enhance + 1);
        //    productivePower = (baseData.productionPower * 10) * System.Math.Pow(1.1, level);
        //}

        InitTrainingData();
    }
         

    void InitTrainingData()
    {
        StatType stat1, stat2, stat3;
        if(heroType == HeroType.NonBattle)
        {
            stat1 = StatType.ProductionPower;
            stat2 = StatType.CollectPower;
            stat3 = StatType.TaxPower;
        }
        else
        {
            stat1 = StatType.MaxHP;
            stat2 = StatType.AttackPower;
            stat3 = StatType.DefensePower;
        }

        if (trainingDataList.Count > 0)
        {
            trainingDataList[0].training = training1;
            trainingDataList[1].training = training2;
            trainingDataList[2].training = training3;
        }
        else
        {
            string key = "training1";

            TrainingData trainingData1 = new TrainingData();
            trainingData1.key = key;
            trainingData1.paramName = stats.GetParam(stat1).baseData.name;
            trainingData1.training = training1;
            trainingDataList.Add(trainingData1);

            key = "training2";

            TrainingData trainingData2 = new TrainingData();
            trainingData2.key = key;
            trainingData2.paramName = stats.GetParam(stat2).baseData.name;
            trainingData2.training = training2;
            trainingDataList.Add(trainingData2);

            key = "training3";

            TrainingData trainingData3 = new TrainingData();
            trainingData3.key = key;
            trainingData3.paramName = stats.GetParam(stat3).baseData.name;
            trainingData3.training = training3;
            trainingDataList.Add(trainingData3);
        }

    }

    public string trainingTypeID { get; private set; }
    public string typeName { get; private set; }

    /// <summary> HeroDB 고유 값 </summary>
    public ObscuredString id { get; private set; }

    /// <summary> 영웅 ID </summary>
    public ObscuredString heroID { get; private set; }
    

    public List<TrainingData> trainingDataList = new List<TrainingData>();


    //public List<string> trainingParamList = new List<string>();
    //public List<string> trainingParamNameList = new List<string>();
    //public List<string> trainingMaxList = new List<string>();


    ObscuredInt training1, training2, training3;

    public ObscuredInt trainingMax
    {
        get
        {
            return limitBreak + 1;
        }
    }

    public bool isTraining = false;

    
    bool _isChecked = true;
    /// <summary> 새로운 영웅인지 아닌지 </summary>
    public bool isChecked
    {
        get { return _isChecked; }
        set
        {
            if (_isChecked == value)
                return;

            _isChecked = value;

            if(onChangedValue != null)
            {
                PropertyInfo propertyInfo = GetType().GetProperty("isChecked");
                onChangedValue(propertyInfo);
            }
        }
    }



    ObscuredInt _enhance = 0;
    /// <summary> 강화수치 </summary>
    public ObscuredInt enhance
    {
        get { return _enhance; }
        set
        {
            if (_enhance == value)
                return;

            _enhance = value;

            RecalculateStats();

            if (onChangedValue != null)
            {
                PropertyInfo propertyInfo = GetType().GetProperty("enhance");
                onChangedValue(propertyInfo);
            }
        }
    }

    ObscuredInt _rebirth = 0;
    /// <summary> 환생 횟수 </summary>
    public ObscuredInt rebirth
    {
        get { return _rebirth; }
        set
        {
            bool isChanged = _rebirth != value;
            //if (_rebirth == value)
            //    return;

            _rebirth = value;

            RecalculateStats();

            if (isChanged && onChangedValue != null)
            {
                PropertyInfo propertyInfo = GetType().GetProperty("rebirth");
                onChangedValue(propertyInfo);
            }
        }
    }

    double _productivePower;
    /// <summary> 생산량 </summary>
    public double productivePower
    {
        get
        {
            return stats.GetValueOf(StatType.ProductionPower);

            return _productivePower;
        }
        private set
        {
            //if (_productivePower == value)
            //    return;

            Stat stat = stats.GetParam(StatType.ProductionPower);

            bool isChanged = stat.baseValue != value;

            stat.baseValue = value;

            if(isChanged && onChangedValue != null)
            {
                PropertyInfo propertyInfo = GetType().GetProperty("productivePower");
                onChangedValue(propertyInfo);
            }

            //코드 여기 저기 건드리기 귀찮아서 매핑만시켜둠
            return;

            _productivePower = value;
            if(onChangedValue != null)
            {
                PropertyInfo propertyInfo = GetType().GetProperty("productivePower");
                onChangedValue(propertyInfo);
            }
        }
    }

    ObscuredInt _limitBreak = 0;
    /// <summary> 한계돌파 횟수 </summary>
    public ObscuredInt limitBreak
    {
        get { return _limitBreak; }
        set
        {
            if (_limitBreak == value)
                return;

            

            _limitBreak = value;
            
        }
    }

   

    /// <summary> 영웅 이름 </summary>
    public string heroName { get; private set; }

    ObscuredInt _heroGrade = 1;
    /// <summary> 등급 </summary>
    public ObscuredInt heroGrade
    {
        get { return _heroGrade; }
        private set { _heroGrade = value; }
    }

    /// <summary> 영웅이미지 이름 </summary>
    public string heroImageName { get; private set; }


    /// <summary> 번들 주소 </summary>
    public string assetBundle;

    /// <summary> 프리펩 이름 </summary>
    public string prefab;

 
    /// <summary>
    /// 배치된 영지
    /// </summary>
    public ObscuredString placeID
    {
        get { return _placeID; }
        set
        {
            if (_placeID == value)
                return;

            _placeID = value;

            if (onChangedValue != null)
            {
                PropertyInfo p = GetType().GetProperty("placeID");
                onChangedValue(p);
            }

        }
    }
    ObscuredString _placeID = string.Empty;

    public List<SkillData> skillDataList { get { return baseData.skillDataList; } }
    

   
}
