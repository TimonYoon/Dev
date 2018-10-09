using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.Linq;
using System;
using CodeStage.AntiCheat.ObscuredTypes;


public class HeroGuildManager : MonoBehaviour {

    public static HeroGuildManager Instance;


    ObscuredFloat _remainingVisitTime = 100f;
    /// <summary> 다음 방문까지 진행시간 </summary>
    public ObscuredFloat remainingVisitTime
    {
        get
        {
            return _remainingVisitTime;
        }
        private set
        {
            _remainingVisitTime = value;
            //if (_remainingVisitTime <= 0)
            //{
            //    _remainingVisitTime = 0;
            //    //Debug.Log("남은 시간 끝");
            //}

        }
    }

    ObscuredFloat startTime = 0;

    /// <summary> 방문한 영웅 리스트 </summary>
    public CustomList<VisitedHeroData> visitedHeroList = new CustomList<VisitedHeroData>();

    bool isInitialized = false;

    private void Awake()
    {
        Instance = this;
    }

    private IEnumerator Start()
    {

        yield return StartCoroutine(GetHeroGuildData());
        
    }

    private void OnEnable()
    {
        WebServerConnectManager.onWebServerResult += OnWebServerResilt;
    }

    private void OnDisable()
    {
        WebServerConnectManager.onWebServerResult -= OnWebServerResilt;
    }

    void OnWebServerResilt(Dictionary<string,object> result)
    {
        if(result.ContainsKey("heroGuild"))
        {
            JsonReader jsonReader = new JsonReader(JsonMapper.ToJson(result["heroGuild"]));
            JsonData jsonData = JsonMapper.ToObject(jsonReader);
            for (int i = 0; i < jsonData.Count; i++)
            {

                string heroID = jsonData[i]["heroID"].ToStringJ();
                int purchased = jsonData[i]["purchase"].ToInt();

                if (purchased == 0)
                    continue;

                //if (visitedHeroList.Count > 0 && visitedHeroList.Find(x => x.heroID == heroID) != null)
                //    continue;

                float remainingTime = jsonData[i]["remainingTime"].ToFloat();
                int visitOrder = jsonData[i]["visitOrder"].ToInt();

                VisitedHeroData visitedHero = new VisitedHeroData(heroID);

                visitedHero.remainingTime = remainingTime;
                visitedHero.visitOrder = visitOrder;
                visitedHero.startTime = Time.unscaledTime + visitedHero.remainingTime;

                if(visitedHeroList.Find(x=>x.visitOrder == visitedHero.visitOrder) != null)
                {
                    visitedHeroList[visitedHeroList.FindIndex(x => x.visitOrder == visitedHero.visitOrder)] = visitedHero;
                }
                else
                {
                    visitedHeroList.Add(visitedHero);
                }
            }

            if (onAddVisitedHero != null)
                onAddVisitedHero();
        }
    }

    public const string saveKeyRemainingVisitTime = "remainingVisitTime";
    public const string saveKeyVisitedHeroList = "visitedHeroList";



    void Visited()
    {
        if (coroutine != null)
            return;

        isInitialized = false;

        coroutine = StartCoroutine(RenewalGuildData());
    }

    public void Employ(string heroID)
    {
        VisitedHeroData data = visitedHeroList.Find(x => x.heroID == heroID);
        StartCoroutine(EmployServerConntect(data));
    }

    IEnumerator EmployServerConntect(VisitedHeroData data)
    {
        string heroID = data.heroID;
        string php = "Hero.php";
        WWWForm form = new WWWForm();
        form.AddField("userID", User.Instance.userID);
        form.AddField("type", 8);
        form.AddField("heroID", data.heroID);
        form.AddField("visitOrder", data.visitOrder);
        string result = "";
        yield return StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(php, form, x => result = x));

        if (string.IsNullOrEmpty(result))
        {
            //Debug.Log("이상 없음 !!!");
            visitedHeroList.Remove(data);
            if (onRemoveVisitedHero != null)
                onRemoveVisitedHero();
            UIPopupManager.ShowInstantPopup(HeroManager.heroBaseDataDic[heroID].name + "를 고용하였습니다.");
        }
        else
        {
            UIPopupManager.ShowInstantPopup("골드가 부족합니다.");
            //Debug.Log("이상 있음 : " + result);
        }
    }

    //void AddHero(VisitedHeroData visitedHeroData)
    //{
    //    if (onAddVisitedHero != null)
    //        onAddVisitedHero();
    //}


    Coroutine coroutine = null;
    IEnumerator GetHeroGuildData()
    {
        
        WWWForm form = new WWWForm();
        string php = "HeroGuild.php";
        string result = "";
        
        form.AddField("userID", User.Instance.userID);
        form.AddField("type", 1);
        yield return StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(php, form, x => result = x));
        if(!string.IsNullOrEmpty(result))
        {
            float remainTime = float.Parse(result);
            

            remainingVisitTime = remainTime;
            startTime = remainingVisitTime + Time.unscaledTime;
        }

        isInitialized = true;

        if (coroutine != null)
            coroutine = null;
    }

    IEnumerator RenewalGuildData()
    {
        LoadingManager.Show();

        WWWForm form = new WWWForm();
        string php = "HeroGuild.php";
        string result = "";

        form.AddField("userID", User.Instance.userID);
        form.AddField("renewal", "yes");
        form.AddField("type", 1);
        yield return StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(php, form, x => result = x));
        if (!string.IsNullOrEmpty(result))
        {
            float remainTime = float.Parse(result);


            remainingVisitTime = remainTime;
            startTime = remainingVisitTime + Time.unscaledTime;
        }

        isInitialized = true;
        LoadingManager.Close();
        if (coroutine != null)
            coroutine = null;
    }

    public delegate void OnChangedVisitedHero();

    /// <summary> 방문 영웅 추가 됬을 때 </summary>
    public static OnChangedVisitedHero onAddVisitedHero;

    /// <summary> 방문 영웅 제거 됬을 때 </summary>
    public static OnChangedVisitedHero onRemoveVisitedHero;

    ObscuredBool _isVisitTime = false;
    ObscuredBool isVisitTime
    {
        get { return _isVisitTime; }
        set
        {
            if (_isVisitTime == value)
                return;

            _isVisitTime = value;

            if (value == true)
            {
                Visited();
            }
        }
    }


    private void Update()
    {

        if (visitedHeroList.Count > 0)
        {
            for (int i = 0; i < visitedHeroList.Count; i++)
            {
                if (visitedHeroList[i].remainingTime <= 0)
                {
                    // 삭제
                    visitedHeroList.Remove(visitedHeroList[i]);
                    if (onRemoveVisitedHero != null)
                        onRemoveVisitedHero();
                }
                else
                {
                    visitedHeroList[i].remainingTime = visitedHeroList[i].startTime - Time.unscaledTime;
                }

            }
        }

        if (isInitialized == false)
            return;


        if (remainingVisitTime < 0f)
        {
            //startTime = Time.unscaledTime + 1200f;
            //remainingVisitTime = startTime - Time.unscaledTime;

            isVisitTime = true;
            
        }
        else
        {
            isVisitTime = false;
            remainingVisitTime = startTime - Time.unscaledTime;
        }
    }

}
