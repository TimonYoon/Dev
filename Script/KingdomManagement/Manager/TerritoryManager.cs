using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using LitJson;
using System;
using UnityEngine.UI;
using KingdomManagement;



/// <summary> 월드맵의 모든 건설물을 관리하는 클래스 </summary>
public class TerritoryManager : MonoBehaviour {
       
    public static TerritoryManager Instance;

    //[SerializeField]
    //Canvas worldMapCanvas;

    /// <summary> 현재 화면에 보고 있는 장소 아이디 </summary>
    [Obsolete("[사용 안함] 본인 코드에서 발견시 제거해주세요.")]
    public string currentPlaceID
    {
        get
        {
            if (UIDeployHeroInfo.Instance == null)
                return "";
            return UIDeployHeroInfo.Instance.currentPlaceID; } }



    /// <summary> 내 소유의 영지 리스트 </summary>
    public List<PlaceData> myPlaceList = new List<PlaceData>();

    /// <summary> 빌딩 매니저 초기화 여부 </summary>
    public bool isInitialized { get; private set; }

    /// <summary> 왕국 레벨업 비용 </summary>
    public string territoryLevelUpCostForGold
    {
        get
        {
            if (!User.Instance)
                return "";
            return (User.Instance.userLevel * 1000).ToString();
        }
    }

    public enum territoryPHPConnectType
    {
        None = 0,
        Reading = 1,
        PlaceAdd = 2,
        PlaceUpgrade = 3,
        TerritoryLevelUp =4,
    }

    private void Awake()
    {
        Instance = this;
        myPlaceList = new List<PlaceData>();
    }



    ///// <summary> 저장공간 </summary>
    //[Obsolete("[사용 안함] 본인 코드에서 발견시 제거해주세요. 대신 TerritoryStorage 를 사용해주세요.")]
    //public BuildingStorage buildingStorage { get; private set; }


    //전체 영지 데이터 리스트
    public List<PlaceData> placeDataList = new List<PlaceData>();

    IEnumerator Start ()
    {
        WebServerConnectManager.onWebServerResult += OnWebServerResult;
        

        while (!UnityEngine.SceneManagement.SceneManager.GetSceneByName("Territory").isLoaded)
            yield return null;

        while (!SceneLobby.Instance)
            yield return null;

        SceneLobby.Instance.OnChangedMenu += OnChangedMenu;

        List<string> keys = GameDataManager.placeBaseDataDic.Keys.ToList();
        for (int i = 0; i < keys.Count; i++)
        {
            PlaceData data = new PlaceData(keys[i]);
            placeDataList.Add(data);

        }
        
        // 서버에서 해당 유저의 영지 소유 data 읽어오기
        yield return StartCoroutine(TerritoryServerConnect(territoryPHPConnectType.Reading,"test"));

        isInitialized = true;
    }

    //[SerializeField]
    //GameObject buttonPanel;

    void OnChangedMenu(LobbyState state)
    {
        //RendererSwitch(state != LobbyState.Battle);

        //buttonPanel.SetActive(state == LobbyState.Territory);
    }    

    void OnWebServerResult(Dictionary<string,object> resultDataDic)
    {
        if (resultDataDic.ContainsKey("territory"))
        {            
            JsonReader json = new JsonReader(JsonMapper.ToJson(resultDataDic["territory"]));

            JsonData jsonData = JsonMapper.ToObject(json);
            if(jsonData.GetJsonType() == JsonType.Array)
            {
                for (int i = 0; i < jsonData.Count; i++)
                {
                    string placeID = jsonData[i]["placeID"].ToString();
                    int level = jsonData[i]["level"].ToInt();
                    InitPlaceData(placeID, level);                    
                }
            }
            else
            {
                string placeID = jsonData["placeID"].ToString();
                int level = jsonData["level"].ToInt();
                InitPlaceData(placeID, level);               
            }
        }
    }

    /// <summary> Atlas_Material 에서 재료 이미지를 찾아준다. </summary>
    public void ChangeMaterialImage(UnityEngine.UI.Image image, string spriteName)
    {
        AssetLoader.AssignImage(image, "sprite/material", "Atlas_Material", spriteName);
    }

    public void ChangeProductImage(UnityEngine.UI.Image image, string spriteName)
    {
        AssetLoader.AssignImage(image, "sprite/product", "Atlas_Product", spriteName);
    }

    public static float ReturnHeroSkillCitizenDoubleTaxProbability
    {
        get
        {
            float result = 0;
            for (int i = 0; i < Instance.myPlaceList.Count; i++)
            {

                result += Instance.myPlaceList[i].skillCT.probabilityCitizenTaxDouble;
                //Debug.Log("영지" + result);
            }
           
            return result;
        }
    }

    /// <summary> 왕국 경험치 획득량 증가량 반환 </summary>
    public static double ReturnHeroSkillCitizenExpValue
    {
        get
        {
            double result = 0;
            for (int i = 0; i < Instance.myPlaceList.Count; i++)
            {

                result += Instance.myPlaceList[i].heroSkillCitizenExpValue;
            }

            return result;
        }
    }



    /// <summary> 필터로 분류된 왕국 경험치 획득량 증가량 반환 </summary>
    public static double ReturnHeroSkillCitizenExpValueByFillter(Item product)
    {
        double result = 0;

        if (product == null)
            return result;

        for (int i = 0; i < Instance.myPlaceList.Count; i++)
        {
            Dictionary<string, double> heroSkillValueDic = Instance.myPlaceList[i].heroSkillCitizenExpValueDic;
            if (heroSkillValueDic.ContainsKey(product.category))
            {
                result += heroSkillValueDic[product.category];
            }

            if (heroSkillValueDic.ContainsKey(product.id))
            {
                result += heroSkillValueDic[product.id];
            }
        }

        return result;
    }

    /// <summary> 주민 세금 획득량 증가량 반환 </summary>
    public static double ReturnHeroSkillCitizenTaxValue
    {
        get
        {
            double result = 0;
            for (int i = 0; i < Instance.myPlaceList.Count; i++)
            {
                result += Instance.myPlaceList[i].heroSkillCitizenTaxValue;
            }

            return result;
        }
    }



    /// <summary> 필터로 분류된 주민 세금 획득량 증가량 반환 </summary>
    public static double ReturnHeroSkillCitizenTaxValueByFillter(Item product)
    {
        double result = 0;

        if (product == null)
            return result;

        for (int i = 0; i < Instance.myPlaceList.Count; i++)
        {
            Dictionary<string, double> heroSkillValueDic = Instance.myPlaceList[i].heroSkillCitizenTaxValueDic;
            if (heroSkillValueDic.ContainsKey(product.category))
            {
                result += heroSkillValueDic[product.category];
            }

            if (heroSkillValueDic.ContainsKey(product.id))
            {
                result += heroSkillValueDic[product.id];
            }
        }

        return result;
    }

    private void Update()
    {
        for (int i = 0; i < myPlaceList.Count; i++)
        {
            PlaceData placeData = myPlaceList[i];
            if (placeData == null)
                return;

            if (placeData.placeState == PlaceState.MyPlace)
            {
                if (placeData.coroutineProduce == null)
                    placeData.coroutineProduce = StartCoroutine(placeData.ProduceCoroutine());
            }
        }
    }
   

  
    /// <summary> 영지 관련 서버 통신 부분 </summary>
    public IEnumerator TerritoryServerConnect(territoryPHPConnectType type, string placeID = "", Action<string> _result = null/*,double cost, int upgradeLevel = 1*/)
    {
        string php = "Territory.php";
        WWWForm form = new WWWForm();
        form.AddField("userID", User.Instance.userID, System.Text.Encoding.UTF8);
        form.AddField("type", (int)type);

        //form.AddField("upgradeLevel", upgradeLevel);
        //form.AddField("cost", cost.ToString());

        if (!string.IsNullOrEmpty(placeID))
            form.AddField("placeID", placeID);

        string result = null;
        string error = null;
        yield return StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(php, form, x => result = x,x=> error= x));

        if(error != null)
        {
            Debug.Log("territory.php error : " + error);
        }


        if (_result != null)
            _result(result);
    }
    public static double placeCost
    {
        get
        {
            double baseValue = 400d;
            float percent = 40f;
            int level = Instance.myPlaceList.Sum(x => x.placeLevel);
            double finalCostValue = baseValue * (1d + percent * 0.01f * (level -1)) ;

            return finalCostValue;
        }
    }
   

    public static SimpleDelegate onAddPlace;
    public static SimpleDelegate onChangedPlaceData;
    /// <summary> 지역을 추가!!! </summary>
    void InitPlaceData(string placeID,int level)
    {
        for (int i = 0; i < placeDataList.Count; i++)
        {
            if(placeDataList[i].placeID == placeID)
            {                
                placeDataList[i].placeLevel = level;
                placeDataList[i].placeState = PlaceState.MyPlace;

                
                AddMyPlace(placeDataList[i]);

                Calculate();
                if (onChangedPlaceData != null)
                    onChangedPlaceData();
            }
        }        
    }
    void Calculate()
    {
        List<string> keys = GameDataManager.itemDic.Keys.ToList();
        for (int i = 0; i < keys.Count; i++)
        {
            Item product = GameDataManager.itemDic[keys[i]];
            product.placeBuffValue = 0;
            for (int h = 0; h < myPlaceList.Count; h++)
            {
                PlaceData placData = myPlaceList[h];
                
                //PlaceBaseData data = TerritoryManager.Instance.myPlaceList[i].placeBaseData;
                if (placData.placeBaseData.type == product.itemType.ToString())
                {
                    if (placData.placeBaseData.fillter == product.id)
                    {                       
                        product.placeBuffValue += placData.power;
                    }

                    if (placData.placeBaseData.fillter == "all")
                    {
                        product.placeBuffValue += placData.power;                        
                    }
                }

                if (placData.placeBaseData.type == "Category" + product.itemType.ToString())
                {
                    if (placData.placeBaseData.fillter == product.category)
                    {
                        product.placeBuffValue += placData.power;                        
                    }

                    if (placData.placeBaseData.fillter == "all")
                    {
                        product.placeBuffValue += placData.power;                    
                    }
                }
            }
        }
    }
    void AddMyPlace(PlaceData data)
    {

        for (int i = 0; i < myPlaceList.Count; i++)
        {
            if (myPlaceList[i].placeID == data.placeID)
                return;
        }

        //Debug.Log("소유 영지 추가 : " + data.placeID);
        myPlaceList.Add(data);
        if (onAddPlace != null)
            onAddPlace();
    }



    //public Dictionary<PassiveType, List<Passive>> passiveListDic = new Dictionary<PassiveType, List<Passive>>();

    //public void Test()
    //{
    //    Debug.Log("안녕");
    //}

    //public List<Passive> passiveList = new List<Passive>();


    //void ApplyTerritoryPassive(PlaceData data)
    //{
    //    passiveList.Find(x=>x.passiveType == PassiveType.Tex).
    //    if (data.placeBaseData.type == "Collect")

    //    for (int i = 0; i < myPlaceList.Count; i++)
    //    {
    //        myPlaceList[i].placeBaseData.
    //    }
    //}
}


