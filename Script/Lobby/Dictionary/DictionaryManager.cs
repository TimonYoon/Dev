using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AssetBundles;
using LitJson;
using System.Linq;
using System.Reflection;
using CodeStage.AntiCheat.ObscuredTypes;

public class DictionaryManager : MonoBehaviour {

    public class HeroDictionaryData
    {
        public delegate void DictionaryDataCallback(PropertyInfo p);

        /// <summary> 무언가 바뀌었을 때... </summary>
        public DictionaryDataCallback onChangedValue;
        
        public HeroData heroData;

        ObscuredInt _dictionaryLevel = 0;
        /// <summary> 도감의 획득 상태 </summary>
        public ObscuredInt dictionaryLevel
        {
            get { return _dictionaryLevel; }
            set
            {
                bool isChanged = _dictionaryLevel != value;
                //if (_rebirth == value)
                //    return;

                _dictionaryLevel = value;

                if (isChanged && onChangedValue != null)
                {
                    PropertyInfo propertyInfo = GetType().GetProperty("dictionaryLevel");
                    onChangedValue(propertyInfo);
                }
            }
        }
        ObscuredInt _rewardStep = 0;
        /// <summary> 도감의 보상 상태 </summary>
        public ObscuredInt rewardStep
        {
            get { return _rewardStep; }
            set
            {
                bool isChanged = _rewardStep != value;
                //if (_rebirth == value)
                //    return;

                _rewardStep = Mathf.Clamp(value, 0, 3);

                if (isChanged && onChangedValue != null)
                {
                    PropertyInfo propertyInfo = GetType().GetProperty("rewardStep");
                    onChangedValue(propertyInfo);
                }
            }
        }
    }

    static public List<HeroDictionaryData> heroDictionaryDataList = new List<HeroDictionaryData>();
    static public CustomDictionary<string, HeroDictionaryData> heroDictionaryDataDic = new CustomDictionary<string, HeroDictionaryData>();

    public static DictionaryManager Instance;
    
    
    public List<string> listHeroID = new List<string>();

  

    private void OnEnable()
    {
        
        HeroManager.heroDataDic.onAdd += OnAddHero;
    }

    private void OnDisable()
    {
        HeroManager.heroDataDic.onAdd -= OnAddHero;
    }

    private void Awake()
    {
        Instance = this;
       
    }

    public static Dictionary<string, HeroBaseData> heroBaseDataDic = new Dictionary<string, HeroBaseData>();

    public static bool isInitialized = false;

   
    public static IEnumerator Init()
    {
        while (!HeroManager.isInitialized)
            yield return null;

        //heroBaseDataDic = HeroManager.heroBaseDataDic;
        

        Instance.listHeroID = GameDataManager.heroBaseDataDic.Keys.ToList();

        for (int i = 0; i < Instance.listHeroID.Count; i++)
        {
            if(!string.IsNullOrEmpty(GameDataManager.heroBaseDataDic[Instance.listHeroID[i]].showInDic))
            {
                heroBaseDataDic.Add(Instance.listHeroID[i], GameDataManager.heroBaseDataDic[Instance.listHeroID[i]]);
            }
        }

        Instance.listHeroID = heroBaseDataDic.Keys.ToList();

        yield return Instance.StartCoroutine(Instance.HeroAdd());

        yield return Instance.StartCoroutine(Instance.InitDictionaryLevelData());

        isInitialized = true;
    }



    public static List<HeroData> heroDataList = new List<HeroData>();

    IEnumerator HeroAdd()
    {
        for (int i = 0; i < heroBaseDataDic.Count; i++)
        {
            HeroBaseData baseData = null;
            baseData = heroBaseDataDic[listHeroID[i]];

            //이런 히어로 우리게임에 없음.
            if (baseData == null)
            {
                Debug.LogError(listHeroID[i] + " 없음");

            }

            HeroData heroData = new HeroData(baseData);
            if (listHeroID[i].Contains("Territory"))
            {                
                heroData.heroType = HeroData.HeroType.NonBattle;
            }
            else
            {
                heroData.heroType = HeroData.HeroType.Battle;
            }
            HeroDictionaryData heroDictionaryData = new HeroDictionaryData();
            heroDictionaryData.heroData = heroData;
            heroDictionaryDataDic.Add(listHeroID[i], heroDictionaryData);

            //heroDataDic.Add(listHeroID[i], heroData);
            yield return null;
        }
    }


    public delegate void DictionaryCheckerCallback(AlarmType type, bool check);
    /// <summary> 도감 보상을 받을 수 있을때 알림용 콜백 </summary>
    public DictionaryCheckerCallback onDictionaryCheckerCallback;

    public IEnumerator InitDictionaryLevelData()
    {
        string php = "Dictionary.php";
        WWWForm form = new WWWForm();
        form.AddField("userID", User.Instance.userID);
        form.AddField("type", 1);
        string result = "";
        yield return StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(php, form, x => result = x));
        if(!string.IsNullOrEmpty(result))
        {
            JsonData jData = ParseCheckDodge(result);

            for (int i = 0; i < listHeroID.Count; i++)
            {
                for (int j = 0; j < jData.Count; j++)
                {
                    if(jData[j]["heroID"].ToString() == listHeroID[i])
                    {
                        heroDictionaryDataDic[listHeroID[i]].dictionaryLevel =  int.Parse(jData[j]["dictionaryLevel"].ToString());
                        heroDictionaryDataDic[listHeroID[i]].rewardStep = int.Parse(jData[j]["achievementLevel"].ToString());

                        
                    }

                    if (isInitialized == false && heroDictionaryDataDic[listHeroID[i]].dictionaryLevel > heroDictionaryDataDic[listHeroID[i]].rewardStep)
                        UpdateAlarm.updateDic = true;
                }
            }
        }
    }

    public void DictionaryRewardRecieveOrNot()
    {
        int count = 0;
        for (int i = 0; i < listHeroID.Count; i++)
        {
            if (heroDictionaryDataDic[listHeroID[i]].dictionaryLevel > heroDictionaryDataDic[listHeroID[i]].rewardStep)
            {
                count += 1;
            }
        }

        if(count > 0)
        {
            if (onDictionaryCheckerCallback != null)
                onDictionaryCheckerCallback(AlarmType.Dictionary, true);
        }
        else
        {
            if (onDictionaryCheckerCallback != null)
                onDictionaryCheckerCallback(AlarmType.Dictionary, false);
        }
        

    }

    void OnAddHero(string id)
    {
        if (isInitialized == false)
            return;

        if(heroDictionaryDataDic[HeroManager.heroDataDic[id].heroID].dictionaryLevel < 1)
        {
            heroDictionaryDataDic[HeroManager.heroDataDic[id].heroID].dictionaryLevel = 1;

            if (onDictionaryCheckerCallback != null)
                onDictionaryCheckerCallback(AlarmType.Dictionary, true);
        }
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
