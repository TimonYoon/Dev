using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;


public enum DayDungeonServerConnectType
{
    None,
    SelectDay,
    BattleStart,
    BattleResult,
    BuyTicket,

}

public enum Day
{
    Sun,
    Mon,
    Tue,
    Wed,
    Thu,
    Fri,
    Sat,

}

public class BattleDayDungeonManager : MonoBehaviour {


    public static BattleDayDungeonManager Instance;

    public static Day today { get; private set; }

    public static int sunTopLevel { get; private set; }
    public static int monTopLevel { get; private set; }
    public static int tueTopLevel { get; private set; }
    public static int wedTopLevel { get; private set; }
    public static int thuTopLevel { get; private set; }
    public static int friTopLevel { get; private set; }
    public static int satTopLevel { get; private set; }

    public static List<HeroData> redTeamDataList = new List<HeroData>();
    public static List<HeroData> blueTeamDataList = new List<HeroData>();

    

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        WebServerConnectManager.onWebServerResult += OnWebServerResult;
    }

    float startTime;
    public static float getTime;
    private void Update()
    {   
        if(MoneyManager.GetMoney(MoneyType.dayDungeonTicket).value < 5)
        {
            getTime = startTime - Time.unscaledTime;
            if (getTime <= 0)
            {
                MoneyManager.GetMoney(MoneyType.dayDungeonTicket).value++;
                startTime = Time.unscaledTime + 900;
            }               
        }
        else
        {
            getTime = 0;
        }       
    }

    void OnWebServerResult(Dictionary<string, object> resultDataDic)
    {
        if (resultDataDic.ContainsKey("dayDungeon"))
        {
            JsonReader json = new JsonReader(JsonMapper.ToJson(resultDataDic["dayDungeon"]));
            JsonData jsonData = JsonMapper.ToObject(json);

            int day = jsonData["day"].ToInt();
           
            sunTopLevel = jsonData["sunTopLevel"].ToInt();
            monTopLevel = jsonData["monTopLevel"].ToInt();
            tueTopLevel = jsonData["tueTopLevel"].ToInt();
            wedTopLevel = jsonData["wedTopLevel"].ToInt();
            thuTopLevel = jsonData["thuTopLevel"].ToInt();
            friTopLevel = jsonData["friTopLevel"].ToInt();
            satTopLevel = jsonData["satTopLevel"].ToInt();

            today = (Day)day;


            float reminingTime = 0;
            if (jsonData.ContainsKey("remainingTime"))
                reminingTime = jsonData["remainingTime"].ToFloat();


            startTime = Time.unscaledTime + reminingTime; 

            //Debug.Log("reminingTime : " + reminingTime.ToString());
        }
    }
    Day lastDay;
    int lastDungeonLevel;

    public static void BattleStart(Day day,int dungeonLevel)
    {
        if (today != day)
            return;

        string key = day.ToString() + "_SaveDungeonLevel_" + User.Instance.userID;
        PlayerPrefs.SetInt(key, dungeonLevel);

        Instance.lastDay = day;
        Instance.lastDungeonLevel = dungeonLevel;
        InitRedTeam();
        InitBlueTeam();
    }

    public static void InitRedTeam()
    {
        redTeamDataList.Clear();
        for (int i = 0; i < Battle.currentBattleGroup.redTeamList.Count; i++)
        {
            string heroID = Battle.currentBattleGroup.redTeamList[i].heroData.id;
            if (string.IsNullOrEmpty(heroID))
            {
                // 소환수 제외
                continue;
            }
            if (HeroManager.heroDataDic.ContainsKey(heroID))
            {
                HeroData h = HeroManager.heroDataDic[heroID];
                HeroData hero = new HeroData(h.baseData);
                hero.InitPvPData(h);

                redTeamDataList.Add(hero);
            }
        }
    }
    public static void InitBlueTeam()
    {
        blueTeamDataList.Clear();
        string key = UIDayDungeonLobby.Instance.currentTapDay.ToString() + "_" + UIDayDungeonLobby.Instance.dungeonLevel;
        if (GameDataManager.dayDungeonBaseDataDic.ContainsKey(key))
        {
            DayDungeonBaseData data = GameDataManager.dayDungeonBaseDataDic[key];
            for (int i = 0; i < data.monsterList.Count; i++)
            {
                DayDungeonMonsterData monster = data.monsterList[i];
                for (int j = 0; j < monster.amount; j++)
                {
                    if (GameDataManager.heroBaseDataDic.ContainsKey(monster.id))
                    {
                        HeroBaseData heroBaseData = GameDataManager.heroBaseDataDic[monster.id];
                        HeroData hero = new HeroData(heroBaseData);
                        hero.InitTestData(heroBaseData.id, monster.maxHp, monster.attackPower,monster.defensePower);

                        blueTeamDataList.Add(hero);
                    }
                    else
                    {
                        Debug.LogError(monster.id + " - 이런 아이디 없음");
                    }
                }
            }
            if(data.bossData != null)
            {
                DayDungeonMonsterData boss = data.bossData;
                if (GameDataManager.heroBaseDataDic.ContainsKey(boss.id))
                {
                    HeroBaseData heroBaseData = GameDataManager.heroBaseDataDic[boss.id];
                    HeroData hero = new HeroData(heroBaseData);
                    hero.InitTestData(heroBaseData.id, boss.maxHp, boss.attackPower,boss.defensePower);

                    blueTeamDataList.Add(hero);
                }
                else
                {
                    Debug.LogError(boss.id + " - 이런 아이디 없음");
                }
            }
            
        }
    }

    public static void DayDungeonServerConnect(DayDungeonServerConnectType type, System.Action<bool> result)
    {
        if (dayDungeonServerConnectCoroutine != null)
        {
            return;
        }

        dayDungeonServerConnectCoroutine = Instance.StartCoroutine(Instance.DayDungeonServerConnectCoroutine(type, result));
    }
    static Coroutine dayDungeonServerConnectCoroutine;
    IEnumerator DayDungeonServerConnectCoroutine(DayDungeonServerConnectType type, System.Action<bool> result)
    {
        string php = "DayDungeon.php";
        WWWForm form = new WWWForm();
        form.AddField("type", (int)type);
        form.AddField("userID", User.Instance.userID);

        if (type == DayDungeonServerConnectType.BattleResult)
        {
            form.AddField("day", (int)lastDay);
            form.AddField("dungeonLevel", lastDungeonLevel);
            form.AddField("isWin",BattleDayDoungen.Instance.isWin.ToString());
        }

        string data = "";
        string error = "";

        yield return StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(php, form, x => data = x, x => error = x));

        dayDungeonServerConnectCoroutine = null;

        bool isResult = false;

        if (string.IsNullOrEmpty(data) == false)
        {
            isResult = true;
        }

        if (string.IsNullOrEmpty(error) == false)
        {
            isResult = false;
            Debug.LogError(error);
        }

        // 서버 연결 결과
        if (result != null)
            result(isResult);
    }

}
