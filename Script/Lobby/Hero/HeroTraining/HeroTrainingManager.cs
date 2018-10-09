using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using KingdomManagement;
using CodeStage.AntiCheat.ObscuredTypes;


public class HeroTrainingData
{
    public void Init(JsonData json)
    {
        heroID = JsonParser.ToString(json["heroID"]);       

        trainingStat = JsonParser.ToInt(json["trainingStat"]);

        slotNumber = JsonParser.ToInt(json["slotNumber"]);

        diffTime = JsonParser.ToFloat(json["diffTime"]);
        
    }
    //영웅ID
    ObscuredString _heroID = string.Empty;
    public ObscuredString heroID
    {
        get { return _heroID; }
        set { _heroID = value; }
    }

    public ObscuredFloat startTime = 0f;

    //남은시간
    public ObscuredFloat remainTime = 0f;

    //경과 시간
    ObscuredFloat _diffTime = 0f;
    public ObscuredFloat diffTime
    {
        get { return _diffTime; }
        set
        {
            _diffTime = value;
            trainingTime = CheckTrainingTime();
            if (value > trainingTime)
            {
                isTrainingMax = true;
                isTrainingStart = false;
                remainTime = 0f;
            }
            else
            {
                isTrainingStart = true;
                startTime = trainingTime - diffTime + Time.unscaledTime;
                remainTime = startTime - Time.unscaledTime;
            }
        }
    }
    //몇번째 스탯 훈련인지
    ObscuredInt _trainingStat = 0;
    public ObscuredInt trainingStat
    {
        get { return _trainingStat; }
        set
        {
            _trainingStat = Mathf.Clamp(value, 0, 2);
        }
    }

    //몇번 슬롯에 있는지
    ObscuredInt _slotNumber = 0;
    public ObscuredInt slotNumber
    {
        get { return _slotNumber; }
        set
        {
            _slotNumber = Mathf.Clamp(value, 0, 4);
        }
    }

    public ObscuredBool isTrainingMax = false;

    public ObscuredBool isTrainingStart = false;
    //총 훈련시간
    public ObscuredFloat trainingTime;

    ObscuredFloat CheckTrainingTime()
    {
        string id = heroID;
        HeroData heroData = HeroManager.heroDataDic[id];
        int trainingCount = 0;
        for (int i = 0; i < heroData.trainingDataList.Count; i++)
        {
            trainingCount += heroData.trainingDataList[i].training;
        }
        
        trainingCount += 1;
        //int heroGrade = heroData.heroGrade;
        float trainingTime = trainingCount * 3600f;

        return trainingTime;
    }
}

public class HeroTrainingManager : MonoBehaviour {
    

    public static HeroTrainingManager Instance;
    

    public List<HeroTrainingData> heroTrainingDataList = new List<HeroTrainingData>();
    

    public delegate void OnChangedTrainingHero(string heroID);
    public OnChangedTrainingHero onChangedTrainingHero;
    

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

    public static bool isInitialized = false;

    public static IEnumerator Init()
    {
        yield return Instance.StartCoroutine(Instance.GetHeroTrainingData());

        isInitialized = true;
    }

    void OnWebServerResult(Dictionary<string, object> resultDic)
    {

        if (resultDic.ContainsKey("heroTraining"))
        {
            string text = JsonParser.Decode(JsonMapper.ToJson(resultDic["heroTraining"]));
            JsonReader json = new JsonReader(text);
            JsonData jsonData = JsonMapper.ToObject(json);

            int slotNum = 0;
            for (int i = 0; i < jsonData.Count; i++)
            {
                slotNum = jsonData[i]["slotNumber"].ToInt();
                HeroTrainingData data = new HeroTrainingData();
                data.Init(jsonData[i]);
                heroTrainingDataList.Add(data);
                //UIHeroTraining.Instance.heroTrainingSlotList[slotNum].heroTrainingData = data;
                //UIHeroTraining.Instance.heroTrainingSlotList[slotNum].InitTrainingSlot();
            }
        }
    }

    IEnumerator GetHeroTrainingData()
    {
        string php = "HeroTraining.php";
        WWWForm form = new WWWForm();
        form.AddField("userID", User.Instance.userID);
        form.AddField("type", 1);
        string result = string.Empty;
        yield return StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(php, form, x => result = x));
    }

    public void Training(int slotNum)
    {
        HeroTrainingData data = heroTrainingDataList.Find(x => x.slotNumber == slotNum);

        if (HeroManager.heroDataDic[data.heroID].trainingDataList[data.trainingStat].training < HeroManager.heroDataDic[data.heroID].trainingMax)
        {
            HeroManager.heroDataDic[data.heroID].trainingDataList[data.trainingStat].training++;
            StartCoroutine(HeroTrainingDataServerInput(HeroManager.heroDataDic[data.heroID].trainingDataList[data.trainingStat].key, data.heroID));
            Debug.Log("수련 완료");
            if (UIHeroTraining.Instance != null)
                UIHeroTraining.Instance.FinishTraining(slotNum);
            
        }
    }
    IEnumerator HeroTrainingDataServerInput(string trainingColumn,string heroID)
    {
        yield return null;
        string php = "Hero.php";
        WWWForm form = new WWWForm();
        form.AddField("userID", User.Instance.userID);
        form.AddField("type", 7);
        form.AddField("heroID", heroID);
        form.AddField("trainingColumn", trainingColumn);

        yield return StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(php, form));

        
        Debug.Log("수련데이터 전송 끝남");
    }

    
    private void Update()
    {
        if (heroTrainingDataList.Count < 1)
            return;

        for (int i = 0; i < heroTrainingDataList.Count; i++)
        {
            if (string.IsNullOrEmpty(heroTrainingDataList[i].heroID) || heroTrainingDataList[i].isTrainingMax == true)
                continue;

            if(heroTrainingDataList[i].isTrainingStart == true)
            {
                if(heroTrainingDataList[i].remainTime > 0f)
                {
                    heroTrainingDataList[i].remainTime = heroTrainingDataList[i].startTime - Time.unscaledTime;
                }
                if (heroTrainingDataList[i].remainTime <= 0)
                {
                    heroTrainingDataList[i].remainTime = 0f;
                    heroTrainingDataList[i].isTrainingMax = true;
                    heroTrainingDataList[i].isTrainingStart = false;
                    continue;
                }
            }
        }
    }

    
}
