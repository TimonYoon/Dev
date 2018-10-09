using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UIDayDungeonLobby : MonoBehaviour
{
    public static UIDayDungeonLobby Instance;
    public GameObject lightPanel;
    public Canvas dayDungeonCanvas;
    public GameObject dayDungeonPanel;
    public Text textDungeonName;
    public Button battleStartButton;

    int _dungeonLevel = 1;
    /// <summary> 현재 선택한 난이도 </summary>
    public int dungeonLevel
    {
        get { return _dungeonLevel; }
        private set
        {
            if (_dungeonLevel == value)
                return;
            _dungeonLevel = value;

            _dungeonLevel = _dungeonLevel > maxLevel ? maxLevel : _dungeonLevel;

            _dungeonLevel = _dungeonLevel < 1 ? 1 : _dungeonLevel;

            
            Show();
        }
    }
    int topLevel = 1;
    Day _currentTapDay = Day.Sun;
    /// <summary> 현재 탭한 요일 </summary>
    public Day currentTapDay
    {
        get { return _currentTapDay; }
        private set
        {
            bool isChack = _currentTapDay != value;
            _currentTapDay = value;

            switch (_currentTapDay)
            {
                case Day.Sun:
                    topLevel = BattleDayDungeonManager.sunTopLevel;
                    break;
                case Day.Mon:
                    topLevel = BattleDayDungeonManager.monTopLevel;
                    break;
                case Day.Tue:
                    topLevel = BattleDayDungeonManager.tueTopLevel;
                    break;
                case Day.Wed:
                    topLevel = BattleDayDungeonManager.wedTopLevel;
                    break;
                case Day.Thu:
                    topLevel = BattleDayDungeonManager.thuTopLevel;
                    break;
                case Day.Fri:
                    topLevel = BattleDayDungeonManager.friTopLevel;
                    break;
                case Day.Sat:
                    topLevel = BattleDayDungeonManager.satTopLevel;
                    break;
                default:
                    break;
            }            

            if(isChack)
                Show();
        }
    }


    [Header("던전 몬스터")]
    public Transform pivot;    
    GameObject bossObj;
    
    public GameObject monsterListParent;
    List<DayDungeonMonsterSlot> monsterSlotList;

   

    [Header("보상관련")]
    public Image imageReward;
    public Text textRewardName;
    public Text textRewardAmount;

    [Header("입장권 관련")]
    public Text textTicket;
    public Text textRemainingTime;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        monsterSlotList = new List<DayDungeonMonsterSlot>(monsterListParent.GetComponentsInChildren<DayDungeonMonsterSlot>());

        SceneLobby.Instance.OnChangedMenu += OnChangedMenu;
        SelectDay();
    }
    private void Update()
    {
        textTicket.text = MoneyManager.GetMoney(MoneyType.dayDungeonTicket).value + "/" + 5;

        textRemainingTime.text = BattleDayDungeonManager.getTime > 0 ? BattleDayDungeonManager.getTime.ToStringTime() : "";
    }
    void OnChangedMenu(LobbyState state)
    {
        if (state != LobbyState.DayDungeonLobby)
            Close();

        if (state == LobbyState.DayDungeonLobby)
        {
            SelectDay();
        }
    }

    public void SelectDay()
    {
        BattleDayDungeonManager.DayDungeonServerConnect(DayDungeonServerConnectType.SelectDay, ServerResult);
    }
    void ServerResult(bool isResult)
    {
        //Debug.Log("pvp 서버 접속 결과 : " + isResult);
        if (isResult)
        {
            currentTapDay = BattleDayDungeonManager.today;
            string key = currentTapDay.ToString() + "_SaveDungeonLevel_" + User.Instance.userID;
            
            if (PlayerPrefs.HasKey(key))
            {
                // 해당 요일에 가장 마지막에 도전했던 난이도 불러옴
                dungeonLevel = PlayerPrefs.GetInt(key);
            }
            else
            {
                dungeonLevel = topLevel;
            }

            
            Show();
        }
        else
        {
            UIPopupManager.ShowInstantPopup("서버연결이 고르지 않습니다. 다시 시도해 주세요");
        }
    }
    int maxLevel = 5;

    public void OnClickDayButton(string day)
    {
        if (string.IsNullOrEmpty(day) == false)
        {
            if (System.Enum.IsDefined(typeof(Day), day))
            {
                Day type = (Day)System.Enum.Parse(typeof(Day), day);
                currentTapDay = type;
            }
        }
    }

    public void OnClickDungeonLevelUpButton()
    {
        int level = dungeonLevel + 1;
        dungeonLevel = level > maxLevel ? 1 : level;
    }

    public void OnClickDungeonLevelDownButton()
    {
        int level = dungeonLevel - 1;
        dungeonLevel = level < 1 ? maxLevel : level;
    }

    void Show()
    {
        if(showCoroutine != null)
        {
            StopCoroutine(showCoroutine);
            showCoroutine = null;
        }

        dayDungeonPanel.SetActive(false);
        showCoroutine = StartCoroutine(ShowCoroutine());
    }

    Coroutine showCoroutine;

    IEnumerator ShowCoroutine()
    {
        string key = currentTapDay.ToString() + "_" + dungeonLevel;
        //Debug.Log(key);
       
        if (GameDataManager.dayDungeonBaseDataDic.ContainsKey(key))
        {
            DayDungeonBaseData data = GameDataManager.dayDungeonBaseDataDic[key];

            textDungeonName.text = data.name;

            if (string.IsNullOrEmpty(data.bossData.id) == false)
            {
                if(bossObj != null)
                {
                    bossObj.SetActive(false);
                    bossObj.transform.SetParent(CharacterEmptyPool.Instance.transform);
                }
               

                GameObject go = null;
                yield return StartCoroutine(CharacterEmptyPool.Instance.GetHero(data.bossData.id, x => go = x));
                bossObj = go;
                bossObj.transform.position = pivot.position;
               
                bossObj.transform.SetParent(pivot);

                bossObj.transform.localScale = Vector3.one;
                //bossObj.SetActive(true);
            }

            for (int i = 0; i < monsterSlotList.Count; i++)
            {
                monsterSlotList[i].InitSlot();
            }

            for (int i = 0; i < data.monsterList.Count; i++)
            {
                string monsterID = data.monsterList[i].id;
                int amount = data.monsterList[i].amount;
                if (GameDataManager.heroBaseDataDic.ContainsKey(monsterID))
                {
                    HeroBaseData heroBaseData = GameDataManager.heroBaseDataDic[monsterID];
                    monsterSlotList[i].InitSlot(heroBaseData, amount);
                }
               
            }

            
            battleStartButton.interactable = currentTapDay == BattleDayDungeonManager.today && dungeonLevel <= topLevel;

            if(GameDataManager.moneyBaseDataDic.ContainsKey(data.rewardID))
            {
                MoneyBaseData moneyBase = GameDataManager.moneyBaseDataDic[data.rewardID];
                AssetLoader.AssignImage(imageReward, "sptire/material", "Atlas_Material", moneyBase.spriteName);
                textRewardName.text = moneyBase.name;
                textRewardAmount.text = data.rewardAmont.ToStringABC();
            }

    
        }
        if (bossObj != null)
            bossObj.SetActive(true);
        dayDungeonCanvas.enabled = true;
        lightPanel.SetActive(true);

        dayDungeonPanel.SetActive(true);
        showCoroutine = null;
    }

    void Close()
    {
        if (bossObj != null)
            bossObj.SetActive(false);
        dayDungeonCanvas.enabled = false;
        lightPanel.SetActive(false);
    }
    bool isLoading = false;
    Coroutine coroutine = null;
    public void OnClickBattleStart()
    {
        BattleStart();
        
    }

    void BattleStart()
    {
        BattleDayDungeonManager.DayDungeonServerConnect(DayDungeonServerConnectType.BattleStart, BattleStartResult);
    }

    void BattleStartResult(bool result)
    {
        if(result)
        {
            if (coroutine != null)
                return;
            BattleDayDungeonManager.BattleStart(currentTapDay, dungeonLevel);
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
            if (t <= 0)
            {
                isLoading = false;
                dayDungeonCanvas.enabled = false;
                break;
            }
            yield return null;
        }
        yield return StartCoroutine(SceneLobby.Instance.ShowScene("scene/battledaydungeon", "BattleDayDungeon", true));
        SceneLobby.Instance.SceneChange(LobbyState.DayDungeon);

        coroutine = null;
    }
    
    public void OnClickBuyTicket()
    {
        if(MoneyManager.GetMoney(MoneyType.ruby).value < 500)
        {
            BuyTicketResult(false);
            return;
        }

        UIPopupManager.ShowYesNoPopup("구매", "500루비에 입장권5개 구매하시겠습니까?", PopupResult);

    }
    void PopupResult(string result)
    {
        if(result == "yes")
        {
            BattleDayDungeonManager.DayDungeonServerConnect(DayDungeonServerConnectType.BuyTicket, BuyTicketResult);
        }
    }

    void BuyTicketResult(bool result)
    {
        if(result == false)
        {
            UIPopupManager.ShowInstantPopup("루비가 부족합니다.");
        }
    }
}
