using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System;
using CodeStage.AntiCheat.ObscuredTypes;

public class BackKeyController :MonoBehaviour
{
    Queue<BackKey> backKeyQueue = new Queue<BackKey>();

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            backKeyQueue.Dequeue().InputBackKey();
        }
    }
}

public class BackKey
{
    public void Init()
    {

    }

    public void InputBackKey()
    {

    }
}

/// <summary> (단일 클래스) 유저의 정보를 담고 있음 </summary>
public class User : MonoBehaviour {

    public static User Instance;

    private void Awake()
    {
        Instance = this;
        isInitialized = false;
    }

    private IEnumerator Start()
    {
        while (!WebServerConnectManager.Instance)
            yield return null;
        LocalSave.RegisterSaveCallBack(OnSave);
        
        WebServerConnectManager.onWebServerResult += OnWebServerResult;
        
    }

    void OnWebServerResult(Dictionary<string, object> resultDataDic)
    {
        object data = null;
        if (resultDataDic.ContainsKey("user"))
        {
            data = resultDataDic["user"];
            JsonReader json = new JsonReader(JsonMapper.ToJson(data));
            JsonData jsonData = JsonMapper.ToObject(json);
            InitUserData(jsonData);
        }

       //if(resultDataDic.ContainsKey("kingdom"))
       // {
       //     data = resultDataDic["kingdom"];
       //     JsonReader json = new JsonReader(JsonMapper.ToJson(data));
       //     JsonData jsonData = JsonMapper.ToObject(json);
       //     currentExp = jsonData["exp"].ToDouble();
            
       // }

    }

    void OnLoad()
    {
        if(ObscuredPrefs.HasKey("currentExp"))
        {
            string data = ObscuredPrefs.GetString("currentExp");
            double exp = 0;
            double.TryParse(data, out exp);

            if (exp > currentExp)
                currentExp = exp;
        }
            
    }
    void OnSave()
    {
        ObscuredPrefs.SetString("currentExp", currentExp.ToString());
    }

    /// <summary> 초기화 완료 여부 체크 </summary>
    public static bool isInitialized { get; private set; }

    /// <summary> 유저 고유 아이디 (게스트일때 해당 아이디만 존재) </summary>
    public string userID { get; private set; }

    /// <summary> 구글 아이디 </summary>
    public string googleID { get; private set; }

    /// <summary> 페이스북 아이디 </summary>
    public string facebookID { get; private set; }

    ///<summary> 닉네임 </summary>
    public string nickname { get; private set; }

    /// <summary> 최초가입 시간 </summary>
    public string signUpTime { get; private set; }

    int _userLevel;
    /// <summary> 유저 레벨 </summary>
    public int userLevel
    {
        get { return _userLevel; }
        private set
        {
            bool isChange = _userLevel != value;

            _userLevel = value;

            if (isChange && onChangedLevel != null)
            {
                onChangedLevel();                
            }                
        }
    }

    static public SimpleDelegate onChangedLevel;
    static public SimpleDelegate onChangedExp;


    double _currentExp;
    /// <summary> 현재 경험치 </summary>
    static public double currentExp
    {
        get { return Instance._currentExp; }
        set
        {
            //0 ~ 필요 경험치만큼 절삭
            value = value < 0d ? 0d : value;
            value = value > requiredExp ? requiredExp : value;

            bool isChanged = Instance._currentExp != value;

            Instance._currentExp = value;

            if (isChanged && onChangedExp != null)
                onChangedExp();
        }
    }

    /// <summary> 다음 레벨업까지 필요 경험치 </summary>
    static public double requiredExp
    {
        get
        {
            return 400 * System.Math.Pow(1.4d, Instance.userLevel - 1);
        }
    }
    Coroutine levelUpCoroutine = null;
    Action<bool> levelUpCallback = null;
    static public void LevelUp(Action<bool> result)
    {
        if (Instance.levelUpCoroutine != null)
            return;
        Instance.levelUpCallback = result;
        Instance.levelUpCoroutine = Instance.StartCoroutine(KingDomServerConnect(3,0, Instance.LevelUpCheck));
        
        //Instance.userLevel++;

        //Todo: 서버 저장
    }
    void LevelUpCheck(bool result)
    {
        if(result)
        {
            currentExp = 0;
            OnSave();
            if (levelUpCallback != null)
                levelUpCallback(result);            
        }

        levelUpCallback = null;
    }

    Coroutine taxCoroutine = null;
    /// <summary> 세금 획득 </summary>
    static public void Tax(double tax)
    {
        
        if (Instance.taxCoroutine != null)
            return;

        Instance.taxCoroutine = Instance.StartCoroutine(KingDomServerConnect(2, tax));
    }


    public static IEnumerator KingDomServerConnect(int type, double tax = 0,Action<bool> resultCallback = null)
    {
        WWWForm form = new WWWForm();
        form.AddField("type", type);
        form.AddField("userID", Instance.userID);
        form.AddField("tax", tax.ToString());
        form.AddField("exp", currentExp.ToString());
        string result = "";
        yield return Instance.StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine("Kingdom.php", form,x => result = x));

        if (string.IsNullOrEmpty(result) ==false)
        {
            if (resultCallback != null)
                resultCallback(false);
        }
        else
        {
            if (resultCallback != null)
                resultCallback(true);
        }

        if(type == 2 && DailyMissionManager.Instance && DailyMissionManager.Instance.taxGetCount < 10)
        {
            DailyMissionManager.Instance.taxGetCount += 1;
            Instance.StartCoroutine(DailyMissionManager.Instance.SetDailyMission(DailyMissionType.TaxGet));
        }
        if(type == 2 && UserQuestManager.Instance && UserQuestManager.Instance.taxGetCount < 1)
        {
            UserQuestManager.Instance.taxGetCount += 1;
            Instance.StartCoroutine(UserQuestManager.Instance.SetUserQuest(UserQuestType.TaxGet));
        }


        if (type == 3)
            Instance.levelUpCoroutine = null;
        else if (type == 2)
            Instance.taxCoroutine = null;

    }

    /// <summary> 닉네임 변경 여부 </summary>
    public int changeNickname { get; private set; }

    public delegate void OnChangedUserData();
    public OnChangedUserData onChangedUserData;

    /// <summary> 최초 로그인시 초기화 </summary>
    public void InitUserData(JsonData jsonData)
    {
        currentExp = jsonData["exp"].ToDouble();
        OnLoad();

        if (jsonData.ContainsKey("level"))
        {
            userLevel = jsonData["level"].ToInt();
        }
        else
            userLevel = 1;

        userID = jsonData["id"].ToString();
        googleID = jsonData["google"].ToString();
        facebookID = jsonData["facebook"].ToString();
       
        nickname = JsonParser.ToString(jsonData["nickname"]);
        signUpTime = jsonData["signUpTime"].ToString();
        changeNickname = jsonData["changeNickname"].ToInt();

        if (onChangedUserData != null)
            onChangedUserData();

        isInitialized = true;
    }

    /// <summary> 플렛폼 로그인 연동 or 해제에 사용됨</summary>
    public IEnumerator WWWUserPHPConnect(LoginType loginTyep)
    {
        
        string php = "Login.php";
        WWWForm form = new WWWForm();
        form.AddField("userID", userID);
        form.AddField("google", GoogleManager.Instance.googleID);
        form.AddField("facebook", FacebookManager.Instance.UserID);
        form.AddField("type", (int)loginTyep);
        yield return StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(php, form));
    }

}
