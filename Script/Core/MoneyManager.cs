using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using LitJson;
using System.Reflection;
using CodeStage.AntiCheat.ObscuredTypes;

public enum MoneyType
{
    none,
    gold,
    ruby,
    drawTicketA,
    drawTicketB,
    drawTicketC,
    buildingSearchTicket,   //?
    placeTicket,
    mileage,
    enhancePointA,
    enhancePointB,
    enhancePointC,
    enhancePointD,
    enhancePointE,
    limitBreakTicket,
    upgradePointA,
    upgradePointB,
    upgradePointC,
    upgradePointD,
    upgradePointE,
    upgradePointF,
    upgradePointG,
    pvpTicket,
    dayDungeonTicket,


}

public enum MoneyPHPtype
{
    Reading = 1,
    Writing = 2
}

/// <summary> 사용자의 재화 정보를 들고 있고, 갱신도 해주는 클래스(얘만 싱글턴) </summary>
public class MoneyManager : MonoBehaviour
{
    [System.Serializable]
    public class Money
    {
        public SimpleDelegate onChangedValue;

        public MoneyType type = MoneyType.none;

        /// <summary> 재화 id. gold, ruby, enhance_a 등등.. </summary>
        public ObscuredString id;

        /// <summary> 재화 이름. 아마도 로컬라이징 id가 여기 박힐 듯? </summary>
        public ObscuredString name;

        double _value;
        /// <summary> 재화 값 </summary>
        public ObscuredDouble value
        {
            get
            {
                return _value;
            }
            set
            {
                bool isChanged = _value != value;

                _value = value;

                if (isChanged && onChangedValue != null)
                    onChangedValue();
            }
        }
    }

    static public MoneyManager Instance;

    void Awake()
    {
        Instance = this;

        Init();
    }

    void Init()
    {
        string[] moneyTypes = Enum.GetNames(typeof(MoneyType));
        for (int i = 0; i < moneyTypes.Length; i++)
        {
            MoneyType mType = (MoneyType)System.Enum.Parse(typeof(MoneyType), moneyTypes[i], true);

            if (mType == MoneyType.none)
                continue;

            Money m = new Money();
            m.id = moneyTypes[i];
            m.type = mType;
            m.value = 0;

            moneyDic.Add(mType, m);
        }

        isInitialized = true;
    }

    public static bool isInitialized = false;

    static Dictionary<MoneyType, Money> moneyDic = new Dictionary<MoneyType, Money>();
    
    void OnWebServerResult(Dictionary<string, object> resultDataDic)
    {
        if (resultDataDic.ContainsKey("money"))
        {
            JsonReader json = new JsonReader(JsonMapper.ToJson(resultDataDic["money"]));
            JsonData jsonData = JsonMapper.ToObject(json);

            List<string> keys = jsonData.keys() as List<string>;
            for(int i = 0; i < jsonData.Count; i++)
            {
                string key = keys[i];
                if (key == "userID")
                    continue;

                //값 갱신
                MoneyType mType = GetMoneyType(key);
                switch (mType)
                {
                    case MoneyType.gold:
                    case MoneyType.enhancePointA:
                    case MoneyType.enhancePointB:
                    case MoneyType.enhancePointC:
                    case MoneyType.enhancePointD:
                    case MoneyType.enhancePointE:
                        moneyDic[mType].value = jsonData[i].ToDouble();
                        break;
                    default:
                        moneyDic[mType].value = JsonParser.ToInt(jsonData[i].ToString());
                        break;

                }
                //moneyDic[mType].value = JsonParser.ToDouble(jsonData[i].ToString());
            }
        }
    }

    static MoneyType GetMoneyType(string type)
    {        
        MoneyType mType = (MoneyType)System.Enum.Parse(typeof(MoneyType), type, true);

        return mType;
    }

    /// <summary> 모든 재화에 전부 다 콜백 검 </summary>
    static public void RegisterOnChangedValueCallback(SimpleDelegate reciever)
    {
        foreach(Money m in moneyDic.Values)
        {
            m.onChangedValue += reciever;
        }
    }

    /// <summary> 특정 재화에 콜백 검 </summary>
    static public void RegisterOnChangedValueCallback(MoneyType moneyType, SimpleDelegate reciever)
    {
        moneyDic[moneyType].onChangedValue += reciever;
    }

    static public Money GetMoney(MoneyType moneyType)
    {
        if (moneyType == MoneyType.none)
            return null;

        if (!moneyDic.ContainsKey(moneyType))
            return null;

        return moneyDic[moneyType];
    }

    static public Money GetMoney(string moneyType)
    {
        MoneyType mType = GetMoneyType(moneyType);

        return GetMoney(mType);
    }    

    public static IEnumerator InitMoneyDataCoroutine()
    {
        WebServerConnectManager.onWebServerResult += Instance.OnWebServerResult;

        string result = "";
        string php = "Money.php";
        WWWForm form = new WWWForm();
        form.AddField("type", (int)MoneyPHPtype.Reading);
        form.AddField("userID", PlayerPrefs.GetString("userID"), System.Text.Encoding.UTF8);

        yield return Instance.StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(php, form, x => result = x));
        
        isInitialized = true;        
    }

    Coroutine coroutine;

    static public void SendMoneyToServer(string moneyType, string value, Action<bool> result = null)
    {
        if (GetMoneyType(moneyType) == MoneyType.none)
            return;

        if (Instance.coroutine != null)
            return;

        Instance.coroutine = Instance.StartCoroutine(Instance.SendMoneyToServerA(moneyType, value));
    }        

    string currentMoneyType;
    IEnumerator SendMoneyToServerA(string moneyType,string value, Action<bool> resultCallBack = null)
    {
        currentMoneyType = moneyType;

        string result = "";
        string php = "Money.php";
        WWWForm form = new WWWForm();
        form.AddField("type", (int)MoneyPHPtype.Writing);
        form.AddField("userID", User.Instance.userID);
        form.AddField("moneyType", moneyType, System.Text.Encoding.UTF8);
        form.AddField("value", value, System.Text.Encoding.UTF8);

        yield return StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(php, form, x => result = x));

        if (!string.IsNullOrEmpty(result))
            UIPopupManager.ShowYesNoPopup("돈없음","구입하러 갈래?", Popup);

        if (resultCallBack != null)
            resultCallBack(string.IsNullOrEmpty(result));

        coroutine = null;
    }
    void Popup(string reuslt)
    {
        if(reuslt == "yes")
        {
            if (currentMoneyType == "ruby")
                SceneLobby.Instance.ShowShop(ShopType.Ruby);
            else
                SceneLobby.Instance.ShowShop(ShopType.Gold);
        }
            
    }
    

    IEnumerator test()
    {
        // 재화 치트키 
        //yield break;
        // 딕셔너리 만듬 
        Dictionary<string, int> testDic = new Dictionary<string, int>();
        testDic.Add("gold", 999999);
        testDic.Add("ruby", 999999);
        testDic.Add("buildingSearchTicket", 10);
        testDic.Add("enhancePointA", 999999);
        testDic.Add("enhancePointB", 999999);
        testDic.Add("enhancePointC", 999999);
        testDic.Add("enhancePointD", 999999);
        testDic.Add("enhancePointE", 999999);

        string packet = JsonMapper.ToJson(testDic);

        Debug.Log("json : " + packet);
        string result = null;
        string php = "Money.php";
        WWWForm form = new WWWForm();
        form.AddField("type", 99);
        form.AddField("userID", User.Instance.userID);
        form.AddField("packet", packet, System.Text.Encoding.UTF8);
        //form.AddField("value", "-" + shopData.price, System.Text.Encoding.UTF8);
        yield return StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(php, form, x => result = x));

        if (string.IsNullOrEmpty(result))
            Debug.Log("결과값 없음");
        else
            Debug.Log("결과값 : " + result);

    }


}
