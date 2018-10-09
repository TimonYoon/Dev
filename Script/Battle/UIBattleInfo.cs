using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBattleInfo : MonoBehaviour {
    static UIBattleInfo Instance;

    public Button buttonRestart;
    public Button buttonStopBattle;

    public Text textDungeonName;
    public Text textStage;
    public Text textEnhanceStoneCount;
    public Text textReturnGuide;
    public Text textPlayTimeTotal;
    public Text textPlayTimeCurrent;
    public Text textTotalQuestIcome;  //퀘스트 누적 수익
    public Text textQuestIcomePerSec; //퀘스트 초당 수익
    public Text textQuestIcomeAutoPerSec; //퀘스트 초당 수익 (자동으로 얻는 것들만)
    public Text textDieCount;   //사망횟수

    public GameObject objEnhanceStoneFire;
    public GameObject objEnhanceStoneWater;
    public GameObject objEnhanceStoneEarth;    
    public GameObject objEnhanceStoneLight;
    public GameObject objEnhanceStoneDark;

    public Canvas canvasReturnConfirmPopup;
    public Canvas canvasResult;

    BattleGroup battleGroup;

    //########################################################################
    void Awake ()
    {
        Instance = this;

        Battle.onChangedBattleGroup += OnChangedBattleGroup;
	}

    void OnChangedBattleGroup(BattleGroup b)
    {
        if (battleGroup)
        {
            battleGroup.onChangedStage -= OnChangedStage;
        }
        

        battleGroup = b;

        if (battleGroup)
        {
            battleGroup.onChangedStage += OnChangedStage;
        }


        InitDungeonInfo();

        buttonStopBattle.gameObject.SetActive(battleGroup.battleType != "Battle_1");

        UpdateStage();
    }

    static public void UpdateQuestInfo()
    {
        double questIncome = 0d;
        double questIncomeAuto = 0d;
        double questIncomeTotal = 0d;

        questIncomeTotal = Instance.battleGroup.battleQuestController.totalIncome;

        for(int i = 0; i < Instance.battleGroup.battleQuestController.battleQuestList.Count; i++)
        {
            BattleQuest quest = Instance.battleGroup.battleQuestController.battleQuestList[i];
            if (quest.level == 0)
                break;

            if (quest.isAutoRepeat)
                questIncomeAuto += quest.baseIncome * quest.level;
            else
                questIncome += quest.baseIncome * quest.level;
        }

        Instance.textTotalQuestIcome.text = questIncomeTotal.ToStringABC();
        Instance.textQuestIcomePerSec.text = questIncome.ToStringABC();
        Instance.textQuestIcomeAutoPerSec.text = questIncomeAuto.ToStringABC();
    }

    void OnChangedStage(BattleGroup b)
    {
        UpdateStage();
    }

    void UpdateStage()
    {
        //층수 갱신
        textStage.text = battleGroup.stage.ToString();

        //회군 시 획득할 강화석 수량 갱신
        textEnhanceStoneCount.text = battleGroup.totalEnhanceStoneCount.ToStringABC();

        //현재 층 수에 따라 회군 버튼 활성/비활성. 30층 이상이어야 회군
        //buttonRestart.interactable = battleGroup.stage >= 30;
        //buttonStopBattle.interactable = battleGroup.stage >= 30;

        //회군 불가일 경우 가이드 문구 표시
        textReturnGuide.gameObject.SetActive(battleGroup.stage < 30);

    }

    void OnChangedEnhanceStone()
    {

    }

    /// <summary> 재시작 버튼 눌렀을 때 </summary>
    public void OnClickRestart()
    {
        UIBattle.ShowRestartConfirmPopup();
    }

    /// <summary> 전투 종료 버튼 눌렀을 때 </summary>
    public void OnClickStopBattle()
    {
        return;
        //현재 사용 안 함
    }
    
    void ResultStopBattle(string result)
    {
        if (result == "yes")
        {
            //전투 종료
            Battle.RemoveBattle();
        }
    }

    static public void InitDungeonInfo()
    {
        DungeonBaseData dungeonBaseData;
        dungeonBaseData = GameDataManager.dungeonBaseDataDic[Instance.battleGroup.dungeonID];

        //던전 이름 갱신
        Instance.textDungeonName.text = dungeonBaseData.dungeonName;

        //드랍되는 강화석 이미지 오브젝트 토글
        Instance.objEnhanceStoneFire.SetActive(dungeonBaseData.dropItemID == "enhancePointA");
        Instance.objEnhanceStoneWater.SetActive(dungeonBaseData.dropItemID == "enhancePointB");
        Instance.objEnhanceStoneEarth.SetActive(dungeonBaseData.dropItemID == "enhancePointC");
        Instance.objEnhanceStoneLight.SetActive(dungeonBaseData.dropItemID == "enhancePointD");
        Instance.objEnhanceStoneDark.SetActive(dungeonBaseData.dropItemID == "enhancePointE");
    }    
	
	void Update ()
    {
        if (!battleGroup)
            return;

        //플레이 타임 표시
        textPlayTimeTotal.text = SecChangeToDateTime(battleGroup.battleTime);
		
	}


    /// <summary> 초를 날/시/분/초 로 바꿈 </summary>
    string SecChangeToDateTime(float time)
    {
        int second = (int)time;
        int hour = 0;
        int minute = 0;

        bool isChack = false;
        while (true)
        {
            if (second > 59)
            {
                minute++;
                second -= 60;
                if (minute > 59)
                {
                    isChack = true;
                    hour++;
                    minute = 0;
                }
            }
            else
            {
                if (!isChack)
                {
                    string result = string.Format("{0:00} : {1:00}", minute, second);
                    return result;
                }
                else
                {
                    string result = string.Format("{0:00} : {1:00} : {2:00}", hour, minute, second);
                    return result;
                }

            }
        }
    }
}
