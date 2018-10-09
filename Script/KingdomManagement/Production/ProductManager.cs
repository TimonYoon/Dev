using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.Linq;
using KingdomManagement;

public class ProductManager : MonoBehaviour
{
    public static ProductManager Instance;

    /// <summary> 이 게임에 존재하는 모든 "생산품" 리스트. 아이템 목록을 의미하지 않음 </summary>
    public List<Item> productList { get; private set; }


    void Awake()
    {
        Instance = this;
        //productList = new List<Item>();
    }

    public bool isInitialized { get; private set; }

    IEnumerator Start()
    {
        while (!WebServerConnectManager.Instance)
            yield return null;

        WebServerConnectManager.onWebServerResult += OnWebServerResult;

        while (!TerritoryManager.Instance)
            yield return null;

        while (!Storage.isInitialized)
            yield return null;


        productList = GameDataManager.itemDic.Values.ToList().FindAll(x => x.itemType == ItemType.Production);

        List<ProductionLineBaseData> productionLineBaseData = GameDataManager.productionLineBaseDataDic.Values.ToList();
        for (int i = 0; i < productionLineBaseData.Count; i++)
        {
            ProductionData data = new ProductionData(productionLineBaseData[i]);
            productionLineDataList.Add(data);


        }

        while (true)
        {
            bool isEnd = true;
            for (int i = 0; i < productionLineDataList.Count; i++)
            {
                if (productionLineDataList[i].isInitialized == false)
                {
                    Debug.Log("없음!!!");
                    isEnd = false;
                }

            }

            if (isEnd)
                break;

            yield return null;
        }

        isInitialized = true;

        if (onProductManagerInitialized != null)
            onProductManagerInitialized();
        //TerritoryManager.Instance.onAddPlace += OnAddPlace;
    }

    /// <summary> 왕국 경험치 획득량 증가량 반환 </summary>
    public static double ReturnHeroSkillCitizenExpValue
    {
        get
        {
            double result = 0;
            for (int i = 0; i < Instance.productionLineDataList.Count; i++)
            {

                result += Instance.productionLineDataList[i].heroSkillCitizenExpValue;
            }

            return result;
        }
    }

    public static float ReturnHeroSkillCitizenDoubleTaxProbability
    {
        get
        {
            float result = 0;
            for (int i = 0; i < Instance.productionLineDataList.Count; i++)
            {

                result += Instance.productionLineDataList[i].skillCT.probabilityCitizenTaxDouble;
                //Debug.Log("왕국" + result);
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

        for (int i = 0; i < Instance.productionLineDataList.Count; i++)
        {
            Dictionary<string, double> heroSkillValueDic = Instance.productionLineDataList[i].heroSkillCitizenExpValueDic;
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
            for (int i = 0; i < Instance.productionLineDataList.Count; i++)
            {
                result += Instance.productionLineDataList[i].heroSkillCitizenTaxValue;
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

        for (int i = 0; i < Instance.productionLineDataList.Count; i++)
        {
            Dictionary<string, double> heroSkillValueDic = Instance.productionLineDataList[i].heroSkillCitizenTaxValueDic;
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


    public static SimpleDelegate onProductManagerInitialized;

    public static SimpleDelegate onChangedDeployHeroList;

    static public void DeployHero(string productionLineID,List<string> deployHeroIdList)
    {
        for (int i = 0; i < Instance.productionLineDataList.Count; i++)
        {
            Instance.productionLineDataList[i].ChangeDeployHeroList(productionLineID, deployHeroIdList);
        }
        for (int i = 0; i < TerritoryManager.Instance.myPlaceList.Count; i++)
        {
            TerritoryManager.Instance.myPlaceList[i].ChangeDeployHeroList(productionLineID, deployHeroIdList);
        }

        if (onChangedDeployHeroList != null)
            onChangedDeployHeroList();
    }

    /// <summary> 생산 라인 데이터 리스트 </summary>
    public CustomList<ProductionData> productionLineDataList = new CustomList<ProductionData>();// { get; private set; }

    void OnWebServerResult(Dictionary<string,object> resultDataDic)
    {

        if(resultDataDic.ContainsKey("kingdomProduct"))
        {
            JsonReader json = new JsonReader(JsonMapper.ToJson(resultDataDic["kingdomProduct"]));

            JsonData jsonData = JsonMapper.ToObject(json);
            if (jsonData.GetJsonType() == JsonType.Array)
            {
                for (int i = 0; i < jsonData.Count; i++)
                {
                    string productID = jsonData[i]["productID"].ToString();
                    int level = jsonData[i]["level"].ToInt();
                    productList.Find(x => x.id == productID).level = level;

                }
            }
            else
            {
                string productID = jsonData["productID"].ToString();
                int level = jsonData["level"].ToInt();
                productList.Find(x => x.id == productID).level = level;

            }
        }
    }
    Coroutine coroutine = null;
    public void UpgradeProduct(string productID, int level, double cost)
    {
        if (coroutine != null)
            return;

        //Item productData = productList.Find(x => x.id == productID);
        //productData.level += level;

        //MoneyManager.Instance.moneyData.gold -= cost;

        coroutine = StartCoroutine(UpgradeCoroutine(5, productID,level,cost));
    }

    IEnumerator UpgradeCoroutine(int type,string productID, int level, double cost)
    {
        string php = "Territory.php";
        WWWForm form = new WWWForm();
        form.AddField("userID", User.Instance.userID, System.Text.Encoding.UTF8);
        form.AddField("type", type);
        form.AddField("placeID", productID);
        form.AddField("upgradeLevel", level.ToString());
        form.AddField("cost", (-cost).ToString());
        string result = null;
        string error = null;
        yield return StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(php, form, x => result = x, x => error = x));
        
        if (error != null)
        {
            Debug.Log("territory.php error : " + error);
        }

        //if (_result != null)
        //    _result(string.IsNullOrEmpty(result));

        coroutine = null;
    }

    public void SaveProduction()
    {
        //for (int i = 0; i < productionDataList.Count; i++)
        //{
        //    if(PlayerPrefs.HasKey(productionDataList[i].saveKey))
        //        PlayerPrefs.DeleteKey(productionDataList[i].saveKey);

        //    if (productionDataList[i].isProduction)
        //        PlayerPrefs.SetFloat(productionDataList[i].saveKey, Time.time - productionDataList[i].startTime);
        //}
    }

}
