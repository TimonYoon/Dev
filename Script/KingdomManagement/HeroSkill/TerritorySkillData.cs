using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum TerritoryHeroDeployState
{
    All,
    Collect,
    Production,
}


public class TerritorySkillData
{
    public string id { get; private set; }

    public string name { get; private set; }

    /// <summary> 배치 상태 </summary>
    public TerritoryHeroDeployState deployState { get; private set; }

    /// <summary> 어디에 적용될 스킬인지 </summary>
    public string applyType { get; private set; }

    /// <summary> 적용 조건 </summary>
    public string fillterCategory { get; private set; }

    public string stat { get; private set; }

    public string statModifyType { get; private set; }

    public string fillterItem { get; private set; }

    public string formula { get; private set; }

  

    public string description { get; private set; }

    


    public TerritorySkillData(LitJson.JsonData json)
    {
        //string text = string.Empty;
        id = json["id"].ToStringJ();

        name = json["name"].ToStringJ();

        string _deployState = json["deployState"].ToStringJ();
        deployState = (TerritoryHeroDeployState)(System.Enum.Parse(typeof(TerritoryHeroDeployState), _deployState));

        applyType = json["applyType"].ToStringJ();

        fillterCategory = json["fillterCategory"].ToStringJ();

        fillterItem = json["fillterItem"].ToStringJ();

        stat = json["stat"].ToStringJ();

        statModifyType = json["statModifyType"].ToStringJ();

        formula = json["formula"].ToStringJ();

        description = json["description"].ToStringJ();

    }

}
