using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

public class BattleLogData
{
    public BattleLogData(JsonData jsondata)
    {
        isWin = jsondata["isWin"].ToBool();
        nickName = jsondata["opponentNickname"].ToString();
    }

    public bool isWin { get; private set; }
    public string nickName { get; private set; }
}

public enum BattlePvPServerConnectType
{
    None,
    SelectBattlePvPInfo,
    BattleStart,
    PvPResult,
    SelectBattleLog,
    BuyTicket,

}

public class BattlePvPManager : MonoBehaviour
{

    public static BattlePvPManager Instance;

    public static int userPvPScore;
    public static int userPvPRank;

    public static string opponentPvPUserID;
    public static string opponentPvPNickname;
    public static int opponentPvPScore;
    public static int opponentPvPRank;

    public static int userPvPResultScore;
    public static int userPvPResultRank;

    public static int userPvPWinCount;
    public static int userPvPLossCount;
    public static List<BattleLogData> battleLogList = new List<BattleLogData>();

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
        if (MoneyManager.GetMoney(MoneyType.pvpTicket).value < 5)
        {
            getTime = startTime - Time.unscaledTime;
            if (getTime <= 0)
            {
                MoneyManager.GetMoney(MoneyType.pvpTicket).value++;
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
        if (resultDataDic.ContainsKey("battlePvP"))
        {
            JsonReader json = new JsonReader(JsonMapper.ToJson(resultDataDic["battlePvP"]));
            JsonData jsonData = JsonMapper.ToObject(json);

            userPvPScore = jsonData["pvpScore"].ToInt();
            userPvPRank = jsonData["rank"].ToInt();

            float reminingTime = 0;
            if (jsonData.ContainsKey("remainingTime"))
                reminingTime = jsonData["remainingTime"].ToFloat();

            startTime = Time.unscaledTime + reminingTime;
        }

        if (resultDataDic.ContainsKey("battlePvPOpponent"))
        {
            JsonReader json = new JsonReader(JsonMapper.ToJson(resultDataDic["battlePvPOpponent"]));
            JsonData jsonData = JsonMapper.ToObject(json);

            opponentPvPUserID = jsonData["userID"].ToString();
            opponentPvPNickname = jsonData["nickname"].ToString();
            opponentPvPScore = jsonData["pvpScore"].ToInt();
            opponentPvPRank = jsonData["rank"].ToInt();

            blueTeamDataList.Clear();

            if (jsonData["heroList"] == null)
            {
                UIPopupManager.ShowInstantPopup("상대방의 영웅 정보가 존재하지 않음");
                return;
            }
            for (int i = 0; i < jsonData["heroList"].Count; i++)
            {
                HeroData hero = InitHeroData(jsonData["heroList"][i]);
                if (hero != null)
                    blueTeamDataList.Add(hero);

            }
        }

        if (resultDataDic.ContainsKey("battlePvPResult"))
        {
            JsonReader json = new JsonReader(JsonMapper.ToJson(resultDataDic["battlePvPResult"]));
            JsonData jsonData = JsonMapper.ToObject(json);

            userPvPResultScore = jsonData["pvpScore"].ToInt();
            userPvPResultRank = jsonData["rank"].ToInt();
        }

        if(resultDataDic.ContainsKey("battleLog"))
        {
            JsonReader json = new JsonReader(JsonMapper.ToJson(resultDataDic["battleLog"]));
            JsonData jsonData = JsonMapper.ToObject(json);

            userPvPWinCount = 0;
            userPvPLossCount = 0;

            if (jsonData.ContainsKey("winCount"))
                userPvPWinCount = jsonData["winCount"].ToInt();
            if (jsonData.ContainsKey("lossCount"))
                userPvPLossCount = jsonData["lossCount"].ToInt();

            battleLogList.Clear();

            if (jsonData.ContainsKey("battleLogList"))
            {
                for (int i = 0; i < jsonData["battleLogList"].Count; i++)
                {
                    BattleLogData battleLogData = new BattleLogData(jsonData["battleLogList"][i]);
                    battleLogList.Add(battleLogData);
                }
            }

           
        }
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

    public static void BattlePVPServerConnect(BattlePvPServerConnectType type, System.Action<bool> result)
    {
        if (battlePVPServerConnectCoroutine != null)
        {
            return;
        }

        battlePVPServerConnectCoroutine = Instance.StartCoroutine(Instance.BattlePVPServerConnectCoroutine(type, result));

    }

    static Coroutine battlePVPServerConnectCoroutine;
    IEnumerator BattlePVPServerConnectCoroutine(BattlePvPServerConnectType type, System.Action<bool> result)
    {
        string php = "BattlePvP.php";
        WWWForm form = new WWWForm();
        form.AddField("type", (int)type);
        form.AddField("userID", User.Instance.userID);

        if(string.IsNullOrEmpty(opponentPvPUserID) == false)
            form.AddField("opponentID", opponentPvPUserID);

        if (Battle.currentBattleGroup.redTeamList.Count > 0)
        {
            List<string> heroIDList = new List<string>();
            for (int i = 0; i < Battle.currentBattleGroup.redTeamList.Count; i++)
            {
                string heroID = Battle.currentBattleGroup.redTeamList[i].heroData.id;
                heroIDList.Add(heroID);
            }
            string json = JsonMapper.ToJson(heroIDList);
            form.AddField("heroIDList", json);
        }
        if(type == BattlePvPServerConnectType.PvPResult)
        {
            form.AddField("isWin", BattlePvP.Instance.isWin.ToString());
        }

        string data = "";
        string error = "";

        yield return StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(php, form, x => data = x, x => error = x));

        battlePVPServerConnectCoroutine = null;

        bool isResult = false;

        isResult = string.IsNullOrEmpty(data) == false;

        if (string.IsNullOrEmpty(error) == false)
        {
            isResult = false;
            Debug.Log(error);
        }

        // 서버 연결 결과
        if (result != null)
            result(isResult);
    }


    HeroData InitHeroData(JsonData serverJsonData)
    {
        string heroID = serverJsonData["heroID"].ToStringJ();
        HeroBaseData baseData = null;
        if (HeroManager.heroBaseDataDic.ContainsKey(heroID))
            baseData = HeroManager.heroBaseDataDic[heroID];

        //이런 히어로 우리게임에 없음.
        if (baseData == null)
        {
            Debug.LogError(heroID + " 없음");
            return null;
        }

        HeroData heroData = new HeroData(baseData);

        heroData.InitServerData(serverJsonData);

        return heroData;
    }
}
