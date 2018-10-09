using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using CodeStage.AntiCheat.ObscuredTypes;

public enum DailyMissionType
{
    HeroTraining,
    Retreat,
    HeroEnhance,
    TaxGet,
}

public class DailyMissionManager : MonoBehaviour {

    public static DailyMissionManager Instance;

    ObscuredInt _heroTrainingCount = 0;
    /// <summary> 훈련 횟수 </summary>
    public ObscuredInt heroTrainingCount
    {
        get { return _heroTrainingCount; }
        set
        {
            if (_heroTrainingCount >= 1)
                return;

            if (value > 1)
                _heroTrainingCount = 1;
            else
                _heroTrainingCount = value;
            

            if (_heroTrainingCount == 1)
                isMissionComplete1 = true;
        }
    }

    [HideInInspector]
    public ObscuredBool isMissionComplete1 = false;
    [HideInInspector]
    public ObscuredBool isMissionRewarded1 = false;

    ObscuredInt _retreatCount = 0;
    /// <summary> 회군 횟수 </summary>
    public ObscuredInt retreatCount
    {
        get { return _retreatCount; }
        set
        {
            if (_retreatCount >= 3)
                return;

            if (value > 3)
            {
                _retreatCount = 3;
            }
            else
            {
                _retreatCount = value;
            }
            

            if (_retreatCount == 3)
                isMissionComplete2 = true;
        }
    }

    [HideInInspector]
    public ObscuredBool isMissionComplete2 = false;
    [HideInInspector]
    public ObscuredBool isMissionRewarded2 = false;

    ObscuredInt _heroEnhanceCount = 0;
    /// <summary> 영웅 강화 횟수 </summary>
    public ObscuredInt heroEnhanceCount
    {
        get { return _heroEnhanceCount; }
        set
        {
            if (_heroEnhanceCount >= 5)
                return;

            _heroEnhanceCount = Mathf.Clamp(value, 0, 5);
            

            if (_heroEnhanceCount == 5)
                isMissionComplete3 = true;
        }
    }

    [HideInInspector]
    public ObscuredBool isMissionComplete3 = false;
    [HideInInspector]
    public ObscuredBool isMissionRewarded3 = false;

    ObscuredInt _taxGetCount = 0;
    /// <summary> 회군 횟수 </summary>
    public ObscuredInt taxGetCount
    {
        get { return _taxGetCount; }
        set
        {
            if (_taxGetCount >= 10)
                return;

            if (value > 10)
                _taxGetCount = 10;
            else
                _taxGetCount = value;
            

            if (_taxGetCount == 10)
                isMissionComplete4 = true;
        }
    }

    [HideInInspector]
    public ObscuredBool isMissionComplete4 = false;
    [HideInInspector]
    public ObscuredBool isMissionRewarded4 = false;

    public ObscuredBool isAllClear = false;
    //{
    //    get
    //    {
    //        if (isMissionRewarded1 && isMissionRewarded2 && isMissionRewarded3 && isMissionRewarded4)
    //        {
    //            return true;
    //        }
    //        else
    //        {
    //            return false;
    //        }
    //    }
    //}

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        WebServerConnectManager.onWebServerResult += OnWebServerResult;
    }

    private void OnDisable()
    {
        WebServerConnectManager.onWebServerResult -= OnWebServerResult;
    }

    static public bool isInitialized = false;
    static public IEnumerator Init()
    {
        yield return Instance.StartCoroutine(Instance.InitDailyMissionData());
    }

    public IEnumerator SetDailyMission(DailyMissionType type)
    {
        while(setDataCoroutine != null)
        {
            yield return null;
        }

        switch (type)
        {
            case DailyMissionType.HeroTraining:
                setDataCoroutine = StartCoroutine(SetDailyMissionData(1, heroTrainingCount));
                break;
            case DailyMissionType.Retreat:
                setDataCoroutine = StartCoroutine(SetDailyMissionData(2, retreatCount));
                break;
            case DailyMissionType.HeroEnhance:
                setDataCoroutine = StartCoroutine(SetDailyMissionData(3, heroEnhanceCount));
                break;
            case DailyMissionType.TaxGet:
                setDataCoroutine = StartCoroutine(SetDailyMissionData(4, taxGetCount));
                break;

        }
    }

    void OnWebServerResult(Dictionary<string, object> resultDataDic)
    {
        if(resultDataDic.ContainsKey("dailyMission"))
        {
            LoadingManager.Show();

            JsonReader json = new JsonReader(JsonMapper.ToJson(resultDataDic["dailyMission"]));
            JsonData jsonData = JsonMapper.ToObject(json);
            
            heroTrainingCount = jsonData["missionCount1"].ToInt();
            if(jsonData["missionComplete1"].ToInt() != 0)
            {
                isMissionComplete1 = false;
                isMissionRewarded1 = true;
            }

            retreatCount = jsonData["missionCount2"].ToInt();
            if (jsonData["missionComplete2"].ToInt() != 0)
            {
                isMissionComplete2 = false;
                isMissionRewarded2 = true;
            }

            heroEnhanceCount = jsonData["missionCount3"].ToInt();
            if (jsonData["missionComplete3"].ToInt() != 0)
            {
                isMissionComplete3 = false;
                isMissionRewarded3 = true;
            }

            taxGetCount = jsonData["missionCount4"].ToInt();
            if (jsonData["missionComplete4"].ToInt() != 0)
            {
                isMissionComplete4 = false;
                isMissionRewarded4 = true;
            }

            if (isMissionRewarded1 && isMissionRewarded2 && isMissionRewarded3 && isMissionRewarded4)
                isAllClear = true;

            if (jsonData["allClear"].ToInt() != 0)
            {
                UIDailyMission.isAllClearActive = true;
                isAllClear = false;
            }
            
            if(isMissionComplete1 || isMissionComplete2 || isMissionComplete3 || isMissionComplete4)
            {
                if (onDailyMissionCheckerCallback != null)
                    onDailyMissionCheckerCallback(AlarmType.DailyMission, true);
                else
                    UpdateAlarm.updateDaily = true;
            }
            else
            {
                if (onDailyMissionCheckerCallback != null)
                    onDailyMissionCheckerCallback(AlarmType.DailyMission, false);
            }

            

            if (UIDailyMission.Instance)
                UIDailyMission.Instance.InitUI();
            if (isInitialized == false)
                isInitialized = true;

            LoadingManager.Close();
        }
    }

    public delegate void DailyMissionCheckerCallback(AlarmType type, bool check);
    /// <summary> 도감 보상을 받을 수 있을때 알림용 콜백 </summary>
    public DailyMissionCheckerCallback onDailyMissionCheckerCallback;

    IEnumerator InitDailyMissionData()
    {
        string php = "DailyMission.php";
        WWWForm form = new WWWForm();
        form.AddField("userID", User.Instance.userID);
        form.AddField("type", 1);
        yield return StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(php, form));
    }

    public IEnumerator GetDailyMissionData()
    {
        string php = "DailyMission.php";
        WWWForm form = new WWWForm();
        form.AddField("userID", User.Instance.userID);
        form.AddField("type", 5);
        yield return StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(php, form));
        
    }

    Coroutine setDataCoroutine = null;
    IEnumerator SetDailyMissionData(int missionNum, int missionCount)
    {
        string php = "DailyMission.php";
        WWWForm form = new WWWForm();
        form.AddField("userID", User.Instance.userID);
        form.AddField("missionNum", missionNum);
        form.AddField("missionCount", missionCount);
        form.AddField("type", 2);
        yield return StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(php, form));
        
        setDataCoroutine = null;
        yield break;
    }
    
}
