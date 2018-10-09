using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary> 획득한 유물 정보를 담고 있는 슬롯 </summary>
public class UIArtifactSlot : MonoBehaviour/*,IPointerDownHandler,IPointerUpHandler*/
{
    public Artifact artifact;

    public Image relicsImage;
    public Text stackCount;
    public Text textName;
    public Text textDescription;
    string title = "";
    string message = "";

    public void SlotInit(Artifact artifact)
    {
        this.artifact = artifact;

        title = artifact.baseData.name;
        message = artifact.baseData.message;
        stackCount.text = artifact.stack.ToString();
        textName.text = artifact.baseData.name;

        AssetLoader.AssignImage(relicsImage, "sprite/artifact", "Atlas_Artifact", artifact.baseData.icon, null);

        ArtifactController artifactController = Battle.currentBattleGroup.artifactController;

        //설명에 있는 능력치는 현재 스택되어 적용되고 있는 능력치로 보여줌
        float power = 0;
        float.TryParse(artifact.baseData.formula, out power);
        string description = artifact.baseData.message.Replace("[formula]", "<color=#00ff00ff>" + (power * artifact.stack).ToString() + "</color>");
        textDescription.text = description;
    }
}
