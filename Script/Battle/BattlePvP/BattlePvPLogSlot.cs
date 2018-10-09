using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class BattlePvPLogSlot : MonoBehaviour {

    public Text textResult;
    public Text textOpponentNickname;

    public void InitSlot(BattleLogData data)
    {
        string result = data.isWin == true ? "승리" : "패배";
        textResult.text = result;

        textOpponentNickname.text = data.nickName;
    }
}
