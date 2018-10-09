using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using CodeStage.AntiCheat.ObscuredTypes;

public enum UserQuestType
{
   TaxGet,
   Retreat,
   HeroEnhance,
   ColaTraining,
   ColaLimitbreak,
   ColaPromote,
   DungeonArrival,
   HeroRebirth,
   PlatinumGet
}

public class UserQuestManager : MonoBehaviour
{

    public static UserQuestManager Instance;
    
    ObscuredInt _taxGetCount = 0;
    /// <summary> 회군 횟수 </summary>
    public ObscuredInt taxGetCount
    {
        get { return _taxGetCount; }
        set
        {
            if (_taxGetCount >= 1)
                return;

            if (value > 1)
                _taxGetCount = 1;
            else
                _taxGetCount = value;


            if (_taxGetCount == 1)
                isMissionComplete[0] = true;
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
            if (_retreatCount >= 1)
                return;

            if (value > 1)
            {
                _retreatCount = 1;
            }
            else
            {
                _retreatCount = value;
            }


            if (_retreatCount == 1)
                isMissionComplete[1] = true;
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
            if (_heroEnhanceCount >= 10)
                return;

            _heroEnhanceCount = Mathf.Clamp(value, 0, 10);


            if (_heroEnhanceCount == 10)
                isMissionComplete[2] = true;
        }
    }

    [HideInInspector]
    public ObscuredBool isMissionComplete3 = false;
    [HideInInspector]
    public ObscuredBool isMissionRewarded3 = false;

    ObscuredInt _colaTrainingCount = 0;
    /// <summary> 회군 횟수 </summary>
    public ObscuredInt colaTrainingCount
    {
        get { return _colaTrainingCount; }
        set
        {
            if (_colaTrainingCount >= 1)
                return;

            if (value > 1)
                _colaTrainingCount = 1;
            else
                _colaTrainingCount = value;


            if (_colaTrainingCount == 1)
                isMissionComplete[3] = true;
        }
    }

    [HideInInspector]
    public ObscuredBool isMissionComplete4 = false;
    [HideInInspector]
    public ObscuredBool isMissionRewarded4 = false;

    ObscuredInt _colaLimitbreakCount = 0;
    /// <summary> 회군 횟수 </summary>
    public ObscuredInt colaLimitbreakCount
    {
        get { return _colaLimitbreakCount; }
        set
        {
            if (_colaLimitbreakCount >= 3)
                return;

            if (value > 3)
                _colaLimitbreakCount = 3;
            else
                _colaLimitbreakCount = value;


            if (_colaLimitbreakCount == 3)
                isMissionComplete[4] = true;
        }
    }

    [HideInInspector]
    public ObscuredBool isMissionComplete5 = false;
    [HideInInspector]
    public ObscuredBool isMissionRewarded5 = false;

    ObscuredInt _colaPromoteCount = 0;
    /// <summary> 회군 횟수 </summary>
    public ObscuredInt colaPromoteCount
    {
        get { return _colaPromoteCount; }
        set
        {
            if (_colaPromoteCount >= 1)
                return;

            if (value > 1)
                _colaPromoteCount = 1;
            else
                _colaPromoteCount = value;


            if (_colaPromoteCount == 1)
                isMissionComplete[5] = true;
        }
    }

    [HideInInspector]
    public ObscuredBool isMissionComplete6 = false;
    [HideInInspector]
    public ObscuredBool isMissionRewarded6 = false;

    ObscuredInt _dungeonArrivalCount = 0;
    /// <summary> 회군 횟수 </summary>
    public ObscuredInt dungeonArrivalCount
    {
        get { return _dungeonArrivalCount; }
        set
        {
            if (_dungeonArrivalCount >= 30)
                return;

            if (value > 30)
                _dungeonArrivalCount = 30;
            else
                _dungeonArrivalCount = value;


            if (_dungeonArrivalCount == 30)
                isMissionComplete[6] = true;
        }
    }

    [HideInInspector]
    public ObscuredBool isMissionComplete7 = false;
    [HideInInspector]
    public ObscuredBool isMissionRewarded7 = false;

    ObscuredInt _heroRebirthCount = 0;
    /// <summary> 회군 횟수 </summary>
    public ObscuredInt heroRebirthCount
    {
        get { return _heroRebirthCount; }
        set
        {
            if (_heroRebirthCount >= 20)
                return;

            if (value > 20)
                _heroRebirthCount = 20;
            else
                _heroRebirthCount = value;


            if (_heroRebirthCount == 20)
                isMissionComplete[7] = true;
        }
    }

    [HideInInspector]
    public ObscuredBool isMissionComplete8 = false;
    [HideInInspector]
    public ObscuredBool isMissionRewarded8 = false;

    ObscuredInt _platinumGetCount = 0;
    /// <summary> 회군 횟수 </summary>
    public ObscuredInt platinumGetCount
    {
        get { return _platinumGetCount; }
        set
        {
            if (_platinumGetCount >= 3)
                return;

            if (value > 3)
                _platinumGetCount = 3;
            else
                _platinumGetCount = value;


            if (_platinumGetCount == 3)
                isMissionComplete[8] = true;
        }
    }

    [HideInInspector]
    public ObscuredBool isMissionComplete9 = false;
    [HideInInspector]
    public ObscuredBool isMissionRewarded9 = false;

    //[HideInInspector]
    public List<bool> isMissionComplete = new List<bool>(new bool[9]);
    //[HideInInspector]
    public List<bool> isMissionRewarded = new List<bool>(new bool[9]);
    [HideInInspector]
    public ObscuredBool isAllClear = false;
 

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
        yield return Instance.StartCoroutine(Instance.InitUserQuestData());
    }

    public IEnumerator SetUserQuest(UserQuestType type)
    {
        while (setDataCoroutine != null)
        {
            yield return null;
        }

        switch (type)
        {
            case UserQuestType.TaxGet:
                setDataCoroutine = StartCoroutine(SetUserQuestData(1, taxGetCount));
                break;
            case UserQuestType.Retreat:
                setDataCoroutine = StartCoroutine(SetUserQuestData(2, retreatCount));
                break;
            case UserQuestType.HeroEnhance:
                setDataCoroutine = StartCoroutine(SetUserQuestData(3, heroEnhanceCount));
                break;
            case UserQuestType.ColaTraining:
                setDataCoroutine = StartCoroutine(SetUserQuestData(4, colaTrainingCount));
                break;
            case UserQuestType.ColaLimitbreak:
                setDataCoroutine = StartCoroutine(SetUserQuestData(5, colaLimitbreakCount));
                break;
            case UserQuestType.ColaPromote:
                setDataCoroutine = StartCoroutine(SetUserQuestData(6, colaPromoteCount));
                break;
            case UserQuestType.DungeonArrival:
                setDataCoroutine = StartCoroutine(SetUserQuestData(7, dungeonArrivalCount));
                break;
            case UserQuestType.HeroRebirth:
                setDataCoroutine = StartCoroutine(SetUserQuestData(8, heroRebirthCount));
                break;
            case UserQuestType.PlatinumGet:
                setDataCoroutine = StartCoroutine(SetUserQuestData(9, platinumGetCount));
                break;
        }
    }

    public int GetValue(UserQuestType type)
    {
        switch (type)
        {
            case UserQuestType.TaxGet:
                return taxGetCount;
            case UserQuestType.Retreat:
                return retreatCount;
            case UserQuestType.HeroEnhance:
                return heroEnhanceCount;
            case UserQuestType.ColaTraining:
                return colaTrainingCount;
            case UserQuestType.ColaLimitbreak:
                return colaLimitbreakCount;
            case UserQuestType.ColaPromote:
                return colaPromoteCount;
            case UserQuestType.DungeonArrival:
                return dungeonArrivalCount;
            case UserQuestType.HeroRebirth:
                return heroRebirthCount;
            case UserQuestType.PlatinumGet:
                return platinumGetCount;
            default:
                return -1;
        }
    }

    void OnWebServerResult(Dictionary<string, object> resultDataDic)
    {
        if (resultDataDic.ContainsKey("userQuest"))
        {
            LoadingManager.Show();

            JsonReader json = new JsonReader(JsonMapper.ToJson(resultDataDic["userQuest"]));
            JsonData jsonData = JsonMapper.ToObject(json);

            taxGetCount = jsonData["missionCount1"].ToInt();
            if (jsonData["missionComplete1"].ToInt() != 0)
            {
                isMissionComplete[0] = false;
                isMissionRewarded[0] = true;
            }

            retreatCount = jsonData["missionCount2"].ToInt();
            if (jsonData["missionComplete2"].ToInt() != 0)
            {
                isMissionComplete[1] = false;
                isMissionRewarded[1] = true;
            }

            heroEnhanceCount = jsonData["missionCount3"].ToInt();
            if (jsonData["missionComplete3"].ToInt() != 0)
            {
                isMissionComplete[2] = false;
                isMissionRewarded[2] = true;
            }

            colaTrainingCount = jsonData["missionCount4"].ToInt();
            if (jsonData["missionComplete4"].ToInt() != 0)
            {
                isMissionComplete[3] = false;
                isMissionRewarded[3] = true;
            }

            colaLimitbreakCount = jsonData["missionCount5"].ToInt();
            if (jsonData["missionComplete5"].ToInt() != 0)
            {
                isMissionComplete[4] = false;
                isMissionRewarded[4] = true;
            }

            colaPromoteCount = jsonData["missionCount6"].ToInt();
            if (jsonData["missionComplete6"].ToInt() != 0)
            {
                isMissionComplete[5] = false;
                isMissionRewarded[5] = true;
            }

            dungeonArrivalCount = jsonData["missionCount7"].ToInt();
            if (jsonData["missionComplete7"].ToInt() != 0)
            {
                isMissionComplete[6] = false;
                isMissionRewarded[6] = true;
            }

            heroRebirthCount = jsonData["missionCount8"].ToInt();
            if (jsonData["missionComplete8"].ToInt() != 0)
            {
                isMissionComplete[7] = false;
                isMissionRewarded[7] = true;
            }

            platinumGetCount = jsonData["missionCount9"].ToInt();
            if (jsonData["missionComplete9"].ToInt() != 0)
            {
                isMissionComplete[8] = false;
                isMissionRewarded[8] = true;
            }

            //if (isMissionRewarded1 && isMissionRewarded2 && isMissionRewarded3 && isMissionRewarded4 && isMissionRewarded5 && isMissionRewarded6 && isMissionRewarded7 && isMissionRewarded8 && isMissionRewarded9)
            //    isAllClear = true;

            int count = 0;
            for (int i = 0; i < isMissionRewarded.Count; i++)
            {
                if (isMissionRewarded[i] == false)
                    count += 1;
            }
            if (count == 0)
                isAllClear = true;

            count = 0;
            for (int i = 0; i < isMissionComplete.Count; i++)
            {
                if (isMissionComplete[i] == true)
                    count += 1;
            }
            if(count > 0)
            {
                if (onUserQuestCheckerCallback != null)
                    onUserQuestCheckerCallback(AlarmType.UserQuest, true);
                else
                    UpdateAlarm.updateUserQuest = true;
            }
            else
            {
                if (onUserQuestCheckerCallback != null)
                    onUserQuestCheckerCallback(AlarmType.UserQuest, false);
            }


            //if (isMissionComplete1 || isMissionComplete2 || isMissionComplete3 || isMissionComplete4 || isMissionComplete5 || isMissionComplete6 || isMissionComplete7 || isMissionComplete8 || isMissionComplete9)
            //{
            //    if (onUserQuestCheckerCallback != null)
            //        onUserQuestCheckerCallback(AlarmType.UserQuest, true);
            //}
            //else
            //{
            //    if (onUserQuestCheckerCallback != null)
            //        onUserQuestCheckerCallback(AlarmType.UserQuest, false);
            //}



            if (UIUserQuest.Instance)
                UIUserQuest.Instance.InitUI();
            if (isInitialized == false)
                isInitialized = true;

            LoadingManager.Close();
        }
    }

    public delegate void UserQuestCheckerCallback(AlarmType type, bool check);
    /// <summary> 도감 보상을 받을 수 있을때 알림용 콜백 </summary>
    public UserQuestCheckerCallback onUserQuestCheckerCallback;

    IEnumerator InitUserQuestData()
    {
        string php = "UserQuest.php";
        WWWForm form = new WWWForm();
        form.AddField("userID", User.Instance.userID);
        form.AddField("type", 1);
        yield return StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(php, form));
    }

    public IEnumerator GetUserQuestData()
    {
        string php = "UserQuest.php";
        WWWForm form = new WWWForm();
        form.AddField("userID", User.Instance.userID);
        form.AddField("type", 5);
        yield return StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(php, form));

    }

    Coroutine setDataCoroutine = null;
    IEnumerator SetUserQuestData(int missionNum, int missionCount)
    {
        string php = "UserQuest.php";
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
