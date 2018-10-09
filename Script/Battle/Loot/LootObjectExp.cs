using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootObjectExp : LootObjectBase
{
    public override void InitMoveDest()
    {
        moveDest = Camera.main.WorldToScreenPoint(UIBattleLevelUp.pivotTotalExp.position);
    }

    public override void DoFinalJob()
    {
        battleGroup.battleLevelUpController.totalExp += value;

        UIBattleLevelUp.ScaleText();
    }
}
