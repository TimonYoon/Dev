using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleQuestSaveDataInfo
{
    public BattleQuestSaveDataInfo(BattleGroup battleGroup)
    {
        totalIncome = battleGroup.battleQuestController.totalIncome;

        unlockableIndex = battleGroup.battleQuestController.unlockableIndex;
    }

    public int unlockableIndex;

    public double totalIncome;
}

[System.Serializable]
public class BattleQuestSaveDataQuest
{
    public BattleQuestSaveDataQuest() { }
    public BattleQuestSaveDataQuest(BattleQuest battleQuest)
    {
        InitData(battleQuest);
    }

    public void InitData(BattleQuest battleQuest)
    {
        saveForm.Clear();
        saveForm.Add("id", battleQuest.baseData.id);
        saveForm.Add("level", battleQuest.level.ToString());
        //saveForm.Add("startTime", battleQuest.startTime.ToString());
        saveForm.Add("progress", battleQuest.progress.ToString());
        saveForm.Add("isAutoRepeat", battleQuest.isAutoRepeat.ToString());
    }

    public Dictionary<string, string> saveForm = new Dictionary<string, string>();
}