using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using CodeStage.AntiCheat.ObscuredTypes;

public class HeroBaseData
{
    public HeroBaseData(JsonData jsonData)
    {
        id = jsonData["id"].ToString();
        grade = jsonData["grade"].ToInt();
        name = jsonData["name"].ToString();


        string _type = jsonData["type"].ToStringJ();
        if (!string.IsNullOrEmpty(_type))
            type = (HeroData.HeroBattleType)System.Enum.Parse(typeof(HeroData.HeroBattleType), _type, true);
        else
            elementalType = ElementalType.NotDefined;

        image = jsonData["image"].ToString();
        assetBundle = jsonData["assetBundle"].ToString();
        prefab = jsonData["prefab"].ToString();
        promoteID = jsonData["promoteID"].ToString();
        limitbreakFlag = jsonData["limitbreakFlag"].ToString();
        showInDic = jsonData["showInDic"].ToString();


        string elemental = jsonData["elemental"].ToString();
        if (!string.IsNullOrEmpty(elemental))
            elementalType = (ElementalType)System.Enum.Parse(typeof(ElementalType), elemental, true);
        else
            elementalType = ElementalType.NotDefined;

        useForMonster = jsonData["useForMonster"].ToBool();
        maxHP = jsonData["maxHp"].ToInt();
        attackPower = jsonData["attackPower"].ToInt();
        defensePower = jsonData["defensePower"].ToInt();




        string skillID_1 = jsonData["skillID_1"].ToString();
        string skillID_2 = jsonData["skillID_2"].ToString();
        string skillID_3 = jsonData["skillID_3"].ToString();
        string skillID_4 = jsonData["skillID_4"].ToString();

        AddToSkillList(skillID_1);
        AddToSkillList(skillID_2);
        AddToSkillList(skillID_3);
        AddToSkillList(skillID_4);

        trainingTypeID = jsonData["trainingTypeID"].ToString();



        productionPower = jsonData["productionPower"].ToInt();
        collectPower = jsonData["collectPower"].ToInt();
        taxPower = jsonData["taxPower"].ToInt();


        string townBuffID_1 = jsonData["territorySkillID_1"].ToString();
        string townBuffID_2 = jsonData["territorySkillID_2"].ToString();
        string townBuffID_3 = jsonData["territorySkillID_3"].ToString();
        string townBuffID_4 = jsonData["territorySkillID_4"].ToString();

        AddToTerritorySkillList(townBuffID_1);
        AddToTerritorySkillList(townBuffID_2);
        AddToTerritorySkillList(townBuffID_3);
        AddToTerritorySkillList(townBuffID_4);
    }

    public ObscuredString id { get; private set; }
    public ObscuredInt grade { get; private set; }

    public HeroData.HeroBattleType type { get; private set; }
    public string name { get; private set; }
    public string image { get; private set; }
    public string assetBundle { get; private set; }
    public string prefab { get; private set; }

    /// <summary> 승급될 영웅의 ID </summary>
    public ObscuredString promoteID { get; private set; }
    /// <summary> 한계돌파 가능한 여부 구분용 ID </summary>
    public string limitbreakFlag { get; private set; }
    /// <summary> 도감용 플래그 </summary>
    public string showInDic { get; private set; }

    public string trainingTypeID { get; private set; }

    public bool useForMonster { get; private set; }

    public ElementalType elementalType { get; private set; }
    public ObscuredInt maxHP { get; set; }
    public ObscuredInt attackPower { get; set; }
    //public int attackSpeed { get; private set; }
    public ObscuredInt defensePower { get; set; }


    /// <summary> 생산력 </summary>
    public ObscuredInt productionPower { get; private set; }

    public ObscuredInt collectPower { get; private set; }

    public ObscuredInt taxPower { get; private set; }
    /// <summary> 보유한 스킬 목록. 액티브 스킬의 경우 UI에 보여주기 위한 용도로만 사용됨. 패시브는.. 아직 미정 </summary>
    public List<SkillData> skillDataList = new List<SkillData>();


    void AddToSkillList(string skillID)
    {
        if (string.IsNullOrEmpty(skillID) || !GameDataManager.skillDataDic.ContainsKey(skillID))
            return;

        SkillData skill = GameDataManager.skillDataDic[skillID];        
        if (skill != null)
            skillDataList.Add(skill);
    }

    /// <summary> 보유한 내정 스킬 목록.  </summary>
    public List<TerritorySkillData> territorySkillDataList = new List<TerritorySkillData>();

    void AddToTerritorySkillList(string skillID)
    {
        if (string.IsNullOrEmpty(skillID))
            return;

        if (GameDataManager.territorySkillDataDic.ContainsKey(skillID))
        {
            TerritorySkillData skillData = GameDataManager.territorySkillDataDic[skillID];
            territorySkillDataList.Add(skillData);
        }
    }
}

public enum ElementalType
{
    NotDefined,
    Fire,
    Water,
    Earth,
    Light,
    Dark
}

