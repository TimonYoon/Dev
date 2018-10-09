using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AssetBundles;
using LitJson;
using System.Linq;

public class HeroManager : MonoBehaviour
{

    public static HeroManager Instance;
    

    //서버에서 내 계정이 가지고 있는 영웅인벤토리 정보

    /// <summary> 커스텀 딕셔너리로 만든 영웅 데이터 딕셔너리 </summary>
    public static CustomDictionary<string, HeroData> heroDataDic = new CustomDictionary<string, HeroData>();


    //정적데이타
    public static List<HeroData> heroDataList
    {
        get
        {
            if (heroDataDic == null)
                return null;

            return heroDataDic.Values.ToList();
        }
    }
    
    
    public delegate void HeroManagerInitCallback(List<HeroData> heroDataList);
    public static HeroManagerInitCallback onHeroManagerInitCallback;


    public delegate void OnPromotedHeroData(string id);
    public static OnPromotedHeroData onPromotedHeroData;

    private void OnEnable()
    {
        WebServerConnectManager.onWebServerResult += OnWebServerResult;
        
        UIHeroInventory.onCheckNewHeroEarned += CheckNewHero;
    }

    private void OnDisable()
    {
        UIHeroInventory.onCheckNewHeroEarned -= CheckNewHero;
    }

    public class HeroProductionAbilityData
    {
        public HeroProductionAbilityData(JsonData jsonData)
        {
            id = JsonParser.ToString(jsonData["id"]);
            heroID = JsonParser.ToString(jsonData["heroID"]);
            productID = JsonParser.ToString(jsonData["productID"]);
            abilityFormula = JsonParser.ToString(jsonData["abilityFormula"]);
        }

        public string id { get; private set; }
        public string heroID { get; private set; }
        public string productID { get; private set; }
        public string abilityFormula { get; private set; }
    }

    void Awake()
    {
        Instance = this;
    }


    static public bool isInitialized = false;

    public static Dictionary<string, HeroBaseData> heroBaseDataDic
    {
        get
        {
            return GameDataManager.heroBaseDataDic;
        }
    }

    

    void OnWebServerResult(Dictionary<string,object> resultDataDic)
    {
        object promoteData = null;
        object data = null;

        


        if (resultDataDic.ContainsKey("hero"))
        {
            data = resultDataDic["hero"];
        }
        else if(resultDataDic.ContainsKey("drawHero"))
        {
            data = resultDataDic["drawHero"];
        }
        else if (resultDataDic.ContainsKey("specialDrawHero"))
        {
            data = resultDataDic["specialDrawHero"];
        }
        else if(resultDataDic.ContainsKey("promoteHero"))
        {
            promoteData = resultDataDic["promoteHero"];
        }

        if (data != null)
        {
            JsonReader json = new JsonReader(JsonMapper.ToJson(data));
            JsonData jsonData = JsonMapper.ToObject(json);
            AddHero(jsonData);
        }

        if(promoteData != null)
        {
            JsonReader json = new JsonReader(JsonMapper.ToJson(promoteData));
            JsonData jsonData = JsonMapper.ToObject(json);
            PromoteHero(jsonData);
        }
    }
    

    public static bool isDelete { get; private set; }

    /// <summary> 영웅 삭제 (영웅 ID 리스트) </summary>
    public static void HeroDelete(List<string> heroIDList)
    {
        isDelete = false;
        Instance.StartCoroutine(Instance.HeroDeleteCoroutine(heroIDList));
    }

    IEnumerator HeroDeleteCoroutine(List<string> heroIDList)
    {

        Dictionary<string, string> dic = new Dictionary<string, string>();
        for (int i = 0; i < heroIDList.Count; i++)
        {
            dic.Add(i.ToString(), heroIDList[i]);
        }
        for (int i = 0; i < dic.Count; i++)
        {
            heroDataDic.Remove(dic.Values.ToList()[i]);
        }


        string json = JsonMapper.ToJson(dic);
        //Debug.Log(json);

        //yield break; // 서버에서 삭제할때 풀어야함;
        WWWForm form = new WWWForm();
        form.AddField("userID", PlayerPrefs.GetString("userID"));
        form.AddField("type", 2);
        form.AddField("heroID", json);
        string php = "Hero.php";
        string result = "";

        yield return StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(php, form, x => result = x));

        if (string.IsNullOrEmpty(result))
        {
            isDelete = true;
            yield break;
        }
        else
        {
            Debug.Log(result);
        }

        
        yield return null;
    }


    public delegate void NewHeroCheckerCallback(AlarmType type, bool check);
    /// <summary> 새로운 영웅 획득 체커 정보 알림용 콜백 </summary>
    public NewHeroCheckerCallback onNewHeroCheckerCallback;

    //로그인시 첫번째 Init호출을 알기위한 변수
    bool isLogin = true;

    /// <summary> 영웅 인벤토리 정보 받아오기 </summary>
    public static IEnumerator Init()
    {
        WWWForm form = new WWWForm();
        form.AddField("userID", PlayerPrefs.GetString("userID"));
        form.AddField("type", 1);
        string php = "Hero.php";

        string result = "";
        
        yield return Instance.StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(php, form, x => result = x));

        if (string.IsNullOrEmpty(result))
        {
            yield break;
        }

        JsonData serverJsonData = Instance.ParseCheckDodge(result);

        Instance.AddHero(serverJsonData);

        isInitialized = true;
    }

    public static IEnumerator UpdateHeroProficiency()
    {
        WWWForm form = new WWWForm();
        form.AddField("userID", PlayerPrefs.GetString("userID"));
        form.AddField("type", 10);
        string php = "Hero.php";

        Dictionary<string, string> heroProficiencyDic = new Dictionary<string, string>();

        for (int i = 0; i < heroDataList.Count; i++)
        {
            if (heroDataList[i].proficiencyTime > 0 && heroDataList[i].isGetProficiencyReward == false)
            {
                if(heroDataList[i].proficiencyTime >= heroDataList[i].maxProficiencyTime)
                    heroDataList[i].isGetProficiencyReward = true; // 보상 받음 처리

                heroProficiencyDic.Add(heroDataList[i].id, heroDataList[i].proficiencyTime.ToString());
            }                
        }
        
        string packet = JsonMapper.ToJson(heroProficiencyDic);

        //Debug.Log("json : " + packet);

        form.AddField("packet", packet);

        string result = "";

        yield return Instance.StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(php, form, x => result = x));

        if (string.IsNullOrEmpty(result))
        {
            yield break;
        }
    }

    void AddHero(JsonData serverJsonData)
    {
        if (serverJsonData.GetJsonType() == JsonType.Array)
        {
            for (int j = 0; j < serverJsonData.Count; j++)
            {
                SetHeroData(serverJsonData[j]);
            }
        }
        else
        {
            SetHeroData(serverJsonData);
        }


        

        if (onHeroManagerInitCallback != null)
        {
            onHeroManagerInitCallback(heroDataList);
        }

        isLogin = false;
    }

    void PromoteHero(JsonData serverJsonData)
    {
        string invenID = JsonParser.ToString(serverJsonData["id"]);

        string heroID = JsonParser.ToString(serverJsonData["heroID"]);

        HeroData heroData = null;
        if (heroDataDic.ContainsKey(invenID))
        {
            heroData = heroDataDic[invenID];
            heroData.InitBaseData(GameDataManager.heroBaseDataDic[heroID]);
        }

        heroData.InitServerData(serverJsonData);

        if (heroDataDic.ContainsKey(heroData.id))
        {
            heroDataDic[heroData.id] = heroData;
        }

        if (onPromotedHeroData != null)
            onPromotedHeroData(heroData.id);
    }


    void SetHeroData(JsonData serverJsonData)
    {
        string invenID = JsonParser.ToString(serverJsonData["id"]);

        string heroID = JsonParser.ToString(serverJsonData["heroID"]);
        HeroBaseData baseData = null;
        if (heroBaseDataDic.ContainsKey(heroID))
            baseData = heroBaseDataDic[heroID];

        //이런 히어로 우리게임에 없음.
        if (baseData == null)
        {
            Debug.LogError(heroID + " 없음");

        }

        //아니면 인벤토리에 추가
        //--------------------------------------------------
        HeroData heroData = null;
        if (heroDataDic.ContainsKey(invenID))
        {
            heroData = heroDataDic[invenID];
        }

        if (heroData == null)
        {
            heroData = new HeroData(baseData);

        }
        heroData.InitServerData(serverJsonData);
        

        //인벤토리에 담음
        if (heroDataDic.ContainsKey(heroData.id))
        {
            heroDataDic[heroData.id] = heroData;
        }
        else
        {
            if (isInitialized)
                heroData.isChecked = false;

            heroDataDic.Add(heroData.id, heroData);
        }

       
    }
    
    /// <summary> UIHeroInventory에서 콜백을 받아 UpdateAlarm으로 넘겨줌 </summary>
    public void CheckNewHero()
    {

        //Debug.Log("uiHero에서 콜백");
        if (onNewHeroCheckerCallback != null)
            onNewHeroCheckerCallback(AlarmType.Hero, false);
    }
        

    JsonData ParseCheckDodge(string wwwString)
    {
        if (string.IsNullOrEmpty(wwwString))
            return null;

        //JsonParser jsonParser = new JsonParser();
        wwwString = JsonParser.Decode(wwwString);

        JsonReader jReader = new JsonReader(wwwString);
        JsonData jData = JsonMapper.ToObject(jReader);
        return jData;
    }
}
