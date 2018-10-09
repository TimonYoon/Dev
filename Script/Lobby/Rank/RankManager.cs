using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using LitJson;

public enum RankType
{
    None,
    /// <summary> 던전 최고층 클리어 랭킹 </summary>
    DoungenClearStage,

    /// <summary> PVP 랭킹 </summary>
    Pvp,

    /// <summary> 레이드 랭킹 </summary>
    Raid,

}
public enum RankServerConnectType
{
    None,
    /// <summary> 랭킹 받기(1~200위까지) </summary>
    Select,

    /// <summary> 본인 랭킹만 받기 </summary>
    UserSelect,
    
    /// <summary> 보상 획득 </summary>
    GetReward,
}

public class RankData
{
    public RankData(JsonData jsonData)
    {
        userID = jsonData["userID"].ToString();
        nickname = jsonData["nickname"].ToString();
        rank = jsonData["rank"].ToInt();

        if (jsonData.ContainsKey("stage"))
            stage = jsonData["stage"].ToInt();

        if (jsonData.ContainsKey("pvpScore"))
            pvpScore = jsonData["pvpScore"].ToInt();
    }

    public string userID { get; private set; }

    public string nickname { get; private set; }

    public int rank { get; private set; }

    public int stage { get; private set; }

    public int pvpScore { get; private set; }
}

public class RankManager : MonoBehaviour {

    static RankManager Instance;


    //public List<RankData> rankListDoungenClearStageLastWeek = new List<RankData>();
    
    /// <summary> 현재 랭킹  </summary>
    public static List<RankData> rankListDoungenClearStageThisWeek = new List<RankData>();

    static public int thisWeekRank = 0;

    static public int thisWeekStage = 0;
    static public int thisWeekPvPScore = 0;

    static public int lastWeekRank = 0;

    static public int lastWeekStage = 0;
    static public int lastWeekPvPScore = 0;

    static public bool isReward = true;

    void Awake()
    {
        Instance = this;
    }

    public void Start()
    {
        WebServerConnectManager.onWebServerResult += OnWebServerResult;
    }
    void OnWebServerResult(Dictionary<string,object> resultDataDic)
    {
        if(resultDataDic.ContainsKey("Rank"))
        {
            rankListDoungenClearStageThisWeek.Clear();

            JsonReader json = new JsonReader(JsonMapper.ToJson(resultDataDic["Rank"]));
            JsonData jsonData = JsonMapper.ToObject(json);

            if (jsonData.Count > 0)
            {
                for (int i = 0; i < jsonData.Count; i++)
                {
                    RankData rankData = new RankData(jsonData[i]);
                    rankListDoungenClearStageThisWeek.Add(rankData);
                }
            }
            //Debug.Log("랭킹 데이터 갯수  : " + jsonData.Count);
        }

        if(resultDataDic.ContainsKey("UserRank"))
        {
            JsonReader json = new JsonReader(JsonMapper.ToJson(resultDataDic["UserRank"]));
            JsonData jsonData = JsonMapper.ToObject(json);

            if (jsonData.ContainsKey("rank"))
                thisWeekRank = jsonData["rank"].ToInt();

            if (jsonData.ContainsKey("stage"))
                thisWeekStage = jsonData["stage"].ToInt();

            if(jsonData.ContainsKey("pvpScore"))
                thisWeekPvPScore = jsonData["pvpScore"].ToInt();
        }

        if (resultDataDic.ContainsKey("UserRankLastWeek"))
        {
            JsonReader json = new JsonReader(JsonMapper.ToJson(resultDataDic["UserRankLastWeek"]));
            JsonData jsonData = JsonMapper.ToObject(json);

            if (jsonData.ContainsKey("rank"))
                lastWeekRank = jsonData["rank"].ToInt();

            if (jsonData.ContainsKey("stage"))
                lastWeekStage = jsonData["stage"].ToInt();

            if (jsonData.ContainsKey("pvpScore"))
                lastWeekPvPScore = jsonData["pvpScore"].ToInt();

            if (jsonData.ContainsKey("isReward"))
                isReward = jsonData["isReward"].ToBool();
        }
    }

    /// <summary> 랭킹 서버 연결 </summary>
    public static void RankServerConnect(RankType rankType, RankServerConnectType serverConnectType,Action<bool> result)
    {
        if (rankServerCoroutine != null)
            return;

        rankServerCoroutine = Instance.StartCoroutine(Instance.RankServerConnectCoroutine(rankType, serverConnectType, result));
    }
    static Coroutine rankServerCoroutine;
    public IEnumerator RankServerConnectCoroutine(RankType rankType,RankServerConnectType serverConnectType, Action<bool> result)
    {
        if(serverConnectType == RankServerConnectType.Select)
        {
            thisWeekRank = 0;
            thisWeekStage = 0;
            thisWeekPvPScore = 0;

            lastWeekRank = 0;
            lastWeekStage = 0;
            lastWeekPvPScore = 0;

            isReward = true;
        }

        string php = "Rank.php";
        WWWForm form = new WWWForm();
        form.AddField("userID", User.Instance.userID);
        form.AddField("type", (int)serverConnectType);
        form.AddField("rankType", rankType.ToString());

        string data = "";
        string error = "";
        yield return StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(php, form, x => data = x, x => error = x));

        rankServerCoroutine = null;


        bool isResult = false;

        if (string.IsNullOrEmpty(data) == false)
        {
            isResult = true;
            Debug.Log(result);
        }

        if (string.IsNullOrEmpty(error) == false)
        {
            isResult = false;
            Debug.Log(error);
        }

        // 서버 연결 결과
        if (result != null)
            result(isResult);
    }
}
