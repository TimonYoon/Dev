using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIBattlePvPLobby : MonoBehaviour
{
    public static UIBattlePvPLobby Instance;

    private void Awake()
    {
        Instance = this;
    }
    public Canvas pvpCanvas;
    public GameObject pvpPanel;
    public Camera pvpCamera;
    
    [Header("유저 정보")]
    public Text userName;
    public Text userRank;
    public Text userScore;
    public GameObject userHeroList;
    List<BattlePvPHeroSlot> userBattlePvPHeroSlotList = new List<BattlePvPHeroSlot>();

    [Header("상대편 정보")]
    public Text opponentName;
    public Text opponentRank;
    public Text opponentScore;
    public GameObject opponentHeroList;
    List<BattlePvPHeroSlot> opponentBattlePvPHeroSlotList = new List<BattlePvPHeroSlot>();


    [Header("전적 정보")]
    public GameObject pvpLogPanel;
    public Text textBattleLog;
    public Text textBattleLogUserScore;
    public GridLayoutGroup battleLogListContent;
    public GameObject battleLogSlotPrefab;
    List<BattlePvPLogSlot> battlePvPLogSlotList = new List<BattlePvPLogSlot>();

    [Header("pvp티켓 획득까지 남은시간")]
    public Text textRemainingTime;

    private void Start()
    {
        userBattlePvPHeroSlotList = new List<BattlePvPHeroSlot>(userHeroList.GetComponentsInChildren<BattlePvPHeroSlot>());
        opponentBattlePvPHeroSlotList = new List<BattlePvPHeroSlot>(opponentHeroList.GetComponentsInChildren<BattlePvPHeroSlot>());
        pvpLogPanel.SetActive(false);
        ShowRedTeam();
        SceneLobby.Instance.OnChangedMenu += OnChangedMenu;
        OnClickSearchButton();
        
    }
    void OnChangedMenu(LobbyState state)
    {
        if (state != LobbyState.PvPBattleLobby)
            Close();
        
        if(state == LobbyState.PvPBattleLobby)
        {
            ShowRedTeam();
            pvpCanvas.enabled = true;
            if(pvpCamera)
                pvpCamera.enabled = true;
        }            
    }
    void ShowRedTeam()
    {
        BattlePvPManager.InitRedTeam();
        for (int i = 0; i < userBattlePvPHeroSlotList.Count; i++)
        {
            userBattlePvPHeroSlotList[i].InitSlot();
        }
        for (int i = 0; i < BattlePvPManager.redTeamDataList.Count; i++)
        {
            HeroData hero = BattlePvPManager.redTeamDataList[i];
            userBattlePvPHeroSlotList[i].InitSlot(hero);
        }
    }
    
    public void OnClickSearchButton()
    {
        pvpPanel.SetActive(false);
        BattlePvPManager.BattlePVPServerConnect(BattlePvPServerConnectType.SelectBattlePvPInfo, ServerResult);
    }
    void ShowLog()
    {
        pvpLogPanel.SetActive(true);
        textBattleLog.text = BattlePvPManager.userPvPWinCount + "승" + BattlePvPManager.userPvPLossCount + "패";
        textBattleLogUserScore.text = BattlePvPManager.userPvPScore + "점";
        for (int i = 0; i < battlePvPLogSlotList.Count; i++)
        {
            battlePvPLogSlotList[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < BattlePvPManager.battleLogList.Count; i++)
        {
            BattlePvPLogSlot slot = CreateBattlePvPLogSlot();
            slot.InitSlot(BattlePvPManager.battleLogList[i]);
            slot.gameObject.SetActive(true);
        }
        SizeControl(BattlePvPManager.battleLogList.Count);
    }

    void Show()
    {
        userName.text = User.Instance.nickname;
        userRank.text = BattlePvPManager.userPvPRank + "위";
        userScore.text = BattlePvPManager.userPvPScore + "점";

        opponentName.text = BattlePvPManager.opponentPvPNickname;
        opponentRank.text = BattlePvPManager.opponentPvPRank + "위";
        opponentScore.text = BattlePvPManager.opponentPvPScore + "점";


        for (int i = 0; i < opponentBattlePvPHeroSlotList.Count; i++)
        {
            opponentBattlePvPHeroSlotList[i].InitSlot();
        }
        for (int i = 0; i < BattlePvPManager.blueTeamDataList.Count; i++)
        {
            HeroData hero = BattlePvPManager.blueTeamDataList[i];
            opponentBattlePvPHeroSlotList[i].InitSlot(hero);
        }
    }
    bool isLoading = false;
    Coroutine coroutine = null;

    public void OnClickBattleStart()
    {
        BattleStart();

    }

    void BattleStart()
    {
        BattlePvPManager.BattlePVPServerConnect(BattlePvPServerConnectType.BattleStart, BattleStartResult);
    }

    void BattleStartResult(bool result)
    {
        if (result)
        {
            if (coroutine != null)
                return;
            coroutine = StartCoroutine(ShowBattleScene());
        }
        else
        {
            UIPopupManager.ShowInstantPopup("입장권이 부족합니다.");
        }
    }

    IEnumerator ShowBattleScene()
    {
        isLoading = true;
        LoadingManager.ShowFullSceneLoading();       
        float startTime = Time.unscaledTime + 2f;
        while (true)
        {
            float t = startTime - Time.unscaledTime;
            if(t <= 0)
            {
                isLoading = false;
                pvpCanvas.enabled = false;
                if (pvpCamera)
                    pvpCamera.enabled = false;
                break;
            }
            yield return null;
        }
        yield return StartCoroutine(SceneLobby.Instance.ShowScene("scene/battlepvp", "BattlePvP", true));
        SceneLobby.Instance.SceneChange(LobbyState.PVPBattle);

        coroutine = null;
    }
   
    void ServerResult(bool isResult)
    {
        if(isResult)
        {
            pvpPanel.SetActive(true);
            Show();
        }
        else
        {
            UIPopupManager.ShowInstantPopup("서버연결이 고르지 않습니다. 다시 시도해 주세요");
        }
    }
    void BattleLogServerResult(bool isResult)
    {
        if(isResult)
        {
            ShowLog();
        }
        else
        {
            UIPopupManager.ShowInstantPopup("서버연결이 고르지 않습니다. 다시 시도해 주세요");
        }
    }
    BattlePvPLogSlot CreateBattlePvPLogSlot()
    {
        BattlePvPLogSlot slot = null;

        for (int i = 0; i < battlePvPLogSlotList.Count; i++)
        {
            if(battlePvPLogSlotList[i].gameObject.activeSelf == false)
            {
                slot = battlePvPLogSlotList[i];
                break;
            }
        }

        if(slot == null)
        {
            GameObject go = Instantiate(battleLogSlotPrefab, battleLogListContent.transform, false);
            slot = go.GetComponent<BattlePvPLogSlot>();
            battlePvPLogSlotList.Add(slot);
        }
        return slot;
    }

    /// <summary> 전투기록 리스트 슬롯 수에 맞게 스크롤 사이즈 조절 </summary>
    void SizeControl(int count)
    {

        RectTransform rect = battleLogListContent.GetComponent<RectTransform>();

        float sizeDeltaY = (battleLogListContent.cellSize.y + battleLogListContent.spacing.y) * count;

        rect.sizeDelta = new Vector2(rect.sizeDelta.x, sizeDeltaY);
    }
    public void OnClickShowBattleLogButton()
    {
        BattlePvPManager.BattlePVPServerConnect(BattlePvPServerConnectType.SelectBattleLog, BattleLogServerResult);
    }

    public void OnClickCloseBattleLogButton()
    {
        pvpLogPanel.SetActive(false);
    }
    public void OnClickCloseButton()
    {
        Close();
    }

    void Close()
    {        
        if(isLoading)
        {
            return;
        }
        pvpLogPanel.SetActive(false);
        pvpCanvas.enabled = false;
        if (pvpCamera)
            pvpCamera.enabled = false;
    }

    private void Update()
    {
        textRemainingTime.text = BattlePvPManager.getTime > 0 ? BattlePvPManager.getTime.ToStringTime() : "";
    }

    public void OnClickBuyTicket()
    {
        if (MoneyManager.GetMoney(MoneyType.ruby).value < 500)
        {
            BuyTicketResult(false);
            return;
        }

        UIPopupManager.ShowYesNoPopup("구매", "500루비에 입장권5개 구매하시겠습니까?", PopupResult);

    }
    void PopupResult(string result)
    {
        if (result == "yes")
        {
            BattlePvPManager.BattlePVPServerConnect(BattlePvPServerConnectType.BuyTicket, BuyTicketResult);
        }
    }

    void BuyTicketResult(bool result)
    {
        if (result == false)
        {
            UIPopupManager.ShowInstantPopup("루비가 부족합니다.");
        }
    }
}
