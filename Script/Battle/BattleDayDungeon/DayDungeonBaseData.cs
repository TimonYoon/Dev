using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

public class DayDungeonMonsterData
{
    public string id;
    public int amount = 1;
    public double maxHp = 100;
    public double attackPower = 100;
    public double defensePower = 100;
}

public class DayDungeonBaseData
{
    
    public DayDungeonBaseData(JsonData jsonData)
    {
        id = jsonData["id"].ToString();

        name = jsonData["name"].ToString();

        buffID = jsonData["buffID"].ToStringJ();

        string key = "";
        for (int i = 1; i < 6; i++)
        {
            key = "monsterID_" + i;
            string monsterID = jsonData[key].ToStringJ();
            if (string.IsNullOrEmpty(monsterID))
                continue;
            else
            {
                DayDungeonMonsterData data = new DayDungeonMonsterData();
                data.id = monsterID;
                key = "amount_" + i;
                data.amount = jsonData[key].ToInt();

                key = "maxHp_" + i;
                data.maxHp = jsonData[key].ToDouble();

                key = "attackPower_" + i;
                data.attackPower = jsonData[key].ToDouble();

                key = "defensePower_" + i;
                data.defensePower = jsonData[key].ToDouble();

                monsterList.Add(data);
            }    
        }

        key = "bossID";
        string bossID = jsonData[key].ToStringJ();
        if(string.IsNullOrEmpty(bossID) == false)
        {
            bossData = new DayDungeonMonsterData();

            bossData.id = bossID;
           
            key = "maxHp";
            bossData.maxHp = jsonData[key].ToDouble();

            key = "attackPower";
            bossData.attackPower = jsonData[key].ToDouble();

            key = "defensePower";
            bossData.defensePower = jsonData[key].ToDouble();
        }

        rewardID = jsonData["rewardID"].ToStringJ();
        rewardAmont = jsonData["rewardAmount"].ToDouble();
    }


    public string id { get; private set; }
    
    public string name { get; private set; }

    public string buffID { get; private set; }

    public List<DayDungeonMonsterData> monsterList = new List<DayDungeonMonsterData>();

    public DayDungeonMonsterData bossData { get; private set; }

    public string rewardID { get; private set; }

    public double rewardAmont { get; private set; }

}
