using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class UIRank : MonoBehaviour {

    [Header("유저 랭킹")]
    public Text textLastWeekRank;
    public Text textLastWeekStage;
    public Text textThisWeekRank;
    public Text textThisWeekStage;
    public Button buttonReward;

    [Header("랭킹 리스트")]
    public GameObject rankPanel;
    public GameObject rankSlotPrefab;
    public GridLayoutGroup rankScrollViewContent;

    public GameObject tipPanel;

    List<UIRankSlot> rankSlotPool = new List<UIRankSlot>();

    RankType currentRankType = RankType.DoungenClearStage;

    IEnumerator Start()
    {
        while (!SceneLobby.Instance)
            yield return null;

        SceneLobby.Instance.OnChangedMenu += OnChangedMenu;

        ServerConnect(currentRankType, RankServerConnectType.Select);        
    }

    void OnChangedMenu(LobbyState state)
    {
        if (state != LobbyState.SubMenu && SceneLobby.currentSubMenuState != SubMenuState.Rank)
            Close();
    }
    public void OnClickRankType(string rankType)
    {
        if (rankType == "doungen")
        {
            if (currentRankType == RankType.DoungenClearStage)
                return;
            ChangeRankType(RankType.DoungenClearStage);
        }
        else if(rankType == "pvp")
        {
            if (currentRankType == RankType.Pvp)
                return;
            ChangeRankType(RankType.Pvp);
        }

    }

    void ChangeRankType(RankType rankType)
    {
        ServerConnect(rankType, RankServerConnectType.Select);
    }

    /// <summary> 랭킹 서버 연결 </summary>
    void ServerConnect(RankType rankType,RankServerConnectType serverConnectType)
    {
        currentRankType = rankType;

        for (int i = 0; i < rankSlotPool.Count; i++)
        {
            rankSlotPool[i].gameObject.SetActive(false);
        }

        rankPanel.SetActive(false);
        RankManager.RankServerConnect(rankType, serverConnectType, ServerConnectResult);
    }

    /// <summary> 랭킹 서버 연결 결과 </summary>
    void ServerConnectResult(bool isResult)
    {
        if(isResult)
            Show();
        else
            UIPopupManager.ShowInstantPopup("서버 연결 실패");
    }

    void Show()
    {
        rankPanel.SetActive(true);

        textLastWeekRank.text = RankManager.lastWeekRank + " 위";

        string text = "";
        if (currentRankType == RankType.DoungenClearStage)
        {
            text = RankManager.lastWeekStage + " stage";
        }
        else if(currentRankType == RankType.Pvp)
        {
            text = RankManager.lastWeekPvPScore + " Score";
        }

        textLastWeekStage.text = text;
       

        textThisWeekRank.text = RankManager.thisWeekRank + " 위";

        text = "";
        if (currentRankType == RankType.DoungenClearStage)
        {
            text = RankManager.thisWeekStage + " stage";
        }
        else if (currentRankType == RankType.Pvp)
        {
            text = RankManager.thisWeekPvPScore + " Score";
        }

        textThisWeekStage.text = text;

        buttonReward.gameObject.SetActive(!RankManager.isReward);

        for (int i = 0; i < RankManager.rankListDoungenClearStageThisWeek.Count; i++)
        {
            UIRankSlot slot = CreateSlot();
            slot.gameObject.SetActive(true);
            slot.InitSlot(RankManager.rankListDoungenClearStageThisWeek[i]);
        }
        SizeControl(RankManager.rankListDoungenClearStageThisWeek.Count);
    }
    public void OnShowTipPanel()
    {
        tipPanel.SetActive(true);
    }

    public void OnHideTipPanel()
    {
        tipPanel.SetActive(false);
    }


    UIRankSlot CreateSlot()
    {
        UIRankSlot slot = null;
        for (int i = 0; i < rankSlotPool.Count; i++)
        {
            if(rankSlotPool[i].gameObject.activeSelf == false)
            {
                slot = rankSlotPool[i];
                break;
            }
        }

        if(slot == null)
        {
            GameObject go = Instantiate(rankSlotPrefab);
            go.transform.SetParent(rankScrollViewContent.transform, false);
            slot = go.GetComponent<UIRankSlot>();
            rankSlotPool.Add(slot);
        }

        return slot;
    }

    public void OnClickRankRewardButton()
    {
        RankManager.RankServerConnect(currentRankType, RankServerConnectType.GetReward, ServerConnectResultGetReward);
    }

    /// <summary> 랭킹보상에 대한 서버 연결 결과 </summary>
    void ServerConnectResultGetReward(bool isResult)
    {
        if (isResult)
        {
            UIPopupManager.ShowInstantPopup("보상 획득");
            buttonReward.gameObject.SetActive(false);
        }
        else
            UIPopupManager.ShowInstantPopup("서버 연결 실패");
    }

    public void OnClickCloseButton()
    {
        SceneLobby.Instance.SceneChange(LobbyState.Lobby);
    }
    void Close()
    {
        SceneLobby.Instance.OnChangedMenu -= OnChangedMenu;
        SceneManager.UnloadSceneAsync("Rank");
    }


    /// <summary> Scroll content size conrtrol </summary>
    void SizeControl(int count)
    {

        RectTransform rect = rankScrollViewContent.GetComponent<RectTransform>();

        float sizeDeltaY = (rankScrollViewContent.cellSize.y + rankScrollViewContent.spacing.y) * count;

        rect.sizeDelta = new Vector2(rect.sizeDelta.x, sizeDeltaY);
    }
}
