using UnityEngine;
using System.Collections;
using System;

public class LootObjectArtifact : LootObjectBase
{
    public override void InitMoveDest()
    {
        moveDest = Camera.main.WorldToScreenPoint(UIArtifact.Instance.artifactPointText.transform.position);
    }

    public override void DoFinalJob()
    {
        battleGroup.artifactController.artifactPoint += value;

        //UIBattleLevelUp.ScaleText();
    }
}
