using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIRankSlot : MonoBehaviour
{
    public Text textRank;
    public Text textStage;
    public Text textName;
    public Image slotPanel;

    Color originalColor;
    private void Awake()
    {
        originalColor = slotPanel.color;
    }

    private void OnDisable()
    {
        textRank.text = "";
        textStage.text = "";
        textName.text = "";
    }

    public void InitSlot(RankData rankData)
    {
        if (rankData.userID == User.Instance.userID)
            slotPanel.color = Color.yellow;
        else
            slotPanel.color = originalColor;

        textRank.text = rankData.rank + " 위";

        string text = "";
        if(rankData.stage > 0)
            text = rankData.stage + " stage";
        else if(rankData.pvpScore > 0)
            text = rankData.pvpScore + " Score";

        textStage.text = text;

        textName.text = rankData.nickname;
    }
}
