using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.Linq;
using KingdomManagement;
using CodeStage.AntiCheat.ObscuredTypes;

public class TradeData
{
    public TradeData()
    {
        //다 더미
        sellAmount = 3;//더미 - 차후 공식에 따라 결정
        buyAmount = 3;//더미 - 차후 공식에 따라 결정
        tradeTime = 10f;//더미 - 차후 공식에 따라 결정  
        remainingTime = 0f;
        startTime = 0;
    }

    public ObscuredString sellMaterialID;

    public ObscuredInt sellAmount;

    public ObscuredString buyMaterialID;

    public ObscuredInt buyAmount;

    public ObscuredFloat tradeTime;

    public ObscuredBool isTrade = false;

    public ObscuredFloat startTime { get; set; }

    /// <summary> 남은 시간 </summary>
    public ObscuredFloat remainingTime { get; set; }

    ObscuredBool _isFinished = false;
    public ObscuredBool isFinished
    {
        get
        {
            return _isFinished;
        }
        set
        {
            if(value == true)
            {
                _isFinished = false;
                TradeStart();
            }
            else
            {
                _isFinished = value;
            }
        }
    }


    /// <summary> 교역을 시작 </summary>
    public void TradeStart()
    {
        remainingTime = tradeTime;
        if (Storage.GetItemStoredAmount(sellMaterialID) < sellAmount)
        {
            Debug.Log("재고부족");
            isTrade = false;
            return;
        }

        Storage.OutItem(sellMaterialID, sellAmount);
        
        startTime = tradeTime + Time.unscaledTime;
        isTrade = true;
        Debug.Log("거래 시작 : " + isTrade);
    }

    //교역량 및 교역시간에 대한 공식 추가
}

public class MarketManager : MonoBehaviour {

    public static MarketManager Instance;
    
    ObscuredFloat _renewalTimeDefault = 120f;
    /// <summary> 거래 갱신시간 기본설정 값 </summary>
    ObscuredFloat renewalTimeDefault { get { return _renewalTimeDefault; } }


    ObscuredFloat _remainingRenewalTime;
    /// <summary> 다음 방문까지 진행시간 </summary>
    public ObscuredFloat remainingRenewalTime
    {
        get
        {
            return _remainingRenewalTime;
        }
        private set
        {
            _remainingRenewalTime = value;
            if (_remainingRenewalTime <= 0)
            {
                _remainingRenewalTime = 0;
                //Debug.Log("남은 시간 끝");
            }

        }
    }


    ObscuredFloat startTime = 0;

    /// <summary> 현재 거래 리스트 </summary>
    public CustomList<TradeData> tradeDataList = new CustomList<TradeData>();

    /// <summary> 현재 저장된 아이템 리스트 </summary>
    List<Storage.StoredItemInfo> currentStorageList = new List<Storage.StoredItemInfo>(); 

    /// <summary> 수출품목 아이디 리스트 </summary>
    List<string> exportIDList = new List<string>();

    /// <summary> 수입품목 아이디 리스트 </summary>
    List<string> importIDList = new List<string>();

    //TEST용 더미 리스트 - 주민이 원하는 물품 리스트(차후 삭제)
    public List<string> dummy = new List<string>();

    //데이터 기본/최대 생성 수
    [HideInInspector]
    public ObscuredInt defaultDataCount = 5;

    public bool isInitialized = false;
    

    private void Awake()
    {
        Instance = this;
    }

    private IEnumerator Start()
    {
        while (!Storage.isInitialized)
            yield return null;
                
        tradeDataList.onRemove += RemoveTrade;
        remainingRenewalTime = renewalTimeDefault;
        LoadMarketIDList();

        isInitialized = true;

        while (TerritoryManager.Instance == false)
            yield return null;

        TerritoryManager.onAddPlace += UpdatePlaceModify;
        TerritoryManager.onChangedPlaceData += UpdatePlaceModify;
        UpdatePlaceModify();
    }

    /// <summary> 영지 특성으로 인해 증가하는 교역량 증가치 % </summary>
    public double placeBuffIncreaseTradingValuePercnet { get; private set; }
    void UpdatePlaceModify()
    {
        double _placeBuffIncreaseTradingValue = 0;
        for (int i = 0; i < TerritoryManager.Instance.myPlaceList.Count; i++)
        {
            PlaceData data = TerritoryManager.Instance.myPlaceList[i];
            if (data.placeBaseData.type == "Trading")
            {
                double d = 0;
                double.TryParse(data.placeBaseData.formula, out d);
                d *= data.placeLevel;
                _placeBuffIncreaseTradingValue += d;
            }
        }

        placeBuffIncreaseTradingValuePercnet = _placeBuffIncreaseTradingValue * 0.01;
    }
    private void OnDestroy()
    {
        tradeDataList.onRemove -= RemoveTrade;
    }

    private void Save()
    {
        SaveMarketIDList();
    }


    public const string saveKeyRemainingTime = "remainingTradeTime";
    public const string saveKeytradeDataList = "addedtradeDataList";


    ///// <summary> 거래 개수 추가 </summary>
    //public void AddTradeData()
    //{

    //    if (defaultDataCount <= tradeDataList.Count)
    //    {
    //        UIPopupManager.ShowInstantPopup("최대 거래 개수를 초과하셨습니다");
    //        return;
    //    }


    //    TradeData trade = new TradeData();
    //    tradeDataList.Add(trade);

    //    Save();
    //}

    //규칙에 따른 거래 데이터 구성
    void TradeDataSet()
    {
        int count = defaultDataCount;
        for (int i = 0; i < tradeDataList.Count; i++)
        {
            if (tradeDataList[i].isTrade)
                count -= 1;
        }

        if (count == 0)
            return;

        if (currentStorageList != null && currentStorageList.Count > 0)
        {
            currentStorageList.Clear();
        }

        if (Storage.storedItemDic.Count < 1)
            return;
        

        //현재 저장된 아이템 데이터 리스트를 가지고 새 리스트를 구성
        //currentStorageList = new List<ItemData>(TerritoryStorage.Instance.storageList);
        //currentStorageList = TerritoryStorage.Instance.storageList;
        
        
        currentStorageList = Storage.storedItemDic.Values.ToList();

        //저장량이 적은 순으로 정렬
        currentStorageList.Sort(delegate (Storage.StoredItemInfo dataA, Storage.StoredItemInfo dataB)
        {
            if (dataA.amount > dataB.amount) return 1;
            else if (dataA.amount < dataB.amount) return -1;
            else return 0;
        });

        //산출 로직에 따른 아이디값 불러오기
        InitExportIDList();
        InitImportIDList();

        int listCount = defaultDataCount - tradeDataList.Count;
        for (int i = 0; i < listCount; i++)
        {
            int exportNum = Random.Range(0, exportIDList.Count);
            int importNum = Random.Range(0, importIDList.Count);
            
            if (exportIDList[exportNum] == importIDList[importNum])
            {
                i -= 1;
                continue;
            }
            else
            {
                TradeData trade = new TradeData();
                trade.sellMaterialID = exportIDList[exportNum];
                trade.buyMaterialID = importIDList[importNum];
                tradeDataList.Add(trade);
            }
        }

        OnChangedTradeList();
    }
    /// <summary> 수출품목 리스트 Initialize </summary>
    void InitExportIDList()
    {
        if (exportIDList.Count > 0)
            exportIDList.Clear();


        double[] weight = new double[currentStorageList.Count];
        double total = 0;

        for (int i = 0; i < currentStorageList.Count; i++)
        {
            weight[i] = currentStorageList[i].amount;
            total += currentStorageList[i].amount;
        }

        for (int i = 0; i < defaultDataCount; i++)
        {
            int num = WeightRandom(weight, total);
            exportIDList.Add(currentStorageList[num].itemID);
        }
    }

    /// <summary> 수입품목 리스트 Initialize </summary>
    void InitImportIDList()
    {
        if (importIDList.Count > 0)
            importIDList.Clear();

        List<Storage.StoredItemInfo> currentDemandList = new List<Storage.StoredItemInfo>();
        Dictionary<string, double> weightDic = new Dictionary<string, double>();

        //double total = Storage.storedItemDic.Sum(x => x.Value.amount);
        double total = 0;
        for (int i = 0; i < currentStorageList.Count; i++)
        {
            total += currentStorageList[i].amount;
        }
        double average = total / currentStorageList.Count;

        //적은 것에 더 가중치를 주기위한 변수(최저값 + 최고값)
        double reverse = currentStorageList[0].amount + currentStorageList[currentStorageList.Count - 1].amount;
        //주민들이 원하는 것중 가장 저장량이 적은 순으로 리스트에 담음
        for (int i = 0; i < currentStorageList.Count; i++)
        {
            for (int j = 0; j < dummy.Count; j++)
            {
                if (currentStorageList[i].itemID == dummy[j])
                {
                    currentDemandList.Add(currentStorageList[i]);
                    weightDic.Add(currentStorageList[i].itemID, (reverse - currentStorageList[i].amount) / total); //((최저값+최고값) - 현재값 )/총합
                }
            }
        }


        //Todo: 물품을 재료단위까지 분해하여 가중치에 합산
        for (int i = 0; i < currentDemandList.Count; i++)
        {
            if (GameDataManager.itemDic.ContainsKey(currentDemandList[i].itemID) == false)
                continue;

            Item item = GameDataManager.itemDic[currentDemandList[i].itemID];
            for (int j = 0; j < item.ingredientList.Count; j++)
            {
                if (currentDemandList[i].itemID == "training_101")
                    continue;
                
                if (weightDic.Keys.Contains(item.ingredientList[j].itemID))
                {
                    weightDic[item.ingredientList[j].itemID] *= 2;
                }
                else
                {
                    weightDic.Add(item.ingredientList[j].itemID, (reverse - currentDemandList[i].amount) / total);
                }
            }
        }

        //주민들이 원하지도 않고 보유하고 있지도 않은 마테리얼은 평균 가중치의 1% 적용(더미)
        List<string> materialIDList = GameDataManager.itemDic.Keys.ToList();
        for (int i = 0; i < materialIDList.Count; i++)
        {
            if (materialIDList[i].Contains("training_101"))
                continue;

            if (weightDic.ContainsKey(materialIDList[i]) == false)
            {
                weightDic.Add(materialIDList[i], average / total * 0.01f);
            }
        }

        //모아진 메테리얼 딕셔너리를 리스트로 만듬
        List<string> weightIDList = weightDic.Keys.ToList();
        List<double> weightValueList = weightDic.Values.ToList();

        currentDemandList.Clear();

        //정리된 데이터를 바탕으로 다시 리스트를 꾸림
        for (int i = 0; i < weightIDList.Count; i++)
        {
            Storage.StoredItemInfo item = new Storage.StoredItemInfo();
            item.itemID = weightIDList[i];
            item.amount = weightValueList[i] * average;
            currentDemandList.Add(item);
        }

        //리스트를 가중치가 적은 순으로 정렬
        currentDemandList.Sort(delegate (Storage.StoredItemInfo dataA, Storage.StoredItemInfo dataB)
        {
            if (dataA.amount > dataB.amount) return 1;
            else if (dataA.amount < dataB.amount) return -1;
            else return 0;
        });

        double[] weight = new double[currentDemandList.Count];

        total = 0;
        //리스트 값을 통해 가중치를 부여 - 
        for (int i = 0; i < currentDemandList.Count; i++)
        {
            weight[i] = currentDemandList[i].amount;
            total += currentDemandList[i].amount;
        }

        for (int i = 0; i < defaultDataCount; i++)
        {
            int num = WeightRandom(weight, total);

            importIDList.Add(currentDemandList[num].itemID);
        }
    }

    //가중치 랜덤
    int WeightRandom(double[] weight, double total)
    {
        double r = Random.Range(0f, 1f); //(0.0 ~ 1.0)
        double dr = r * total;

        double cumulative = 0.0f;

        int index = 0;

        for (int i = 0; i < weight.Length; i++)
        {
            cumulative += weight[i];
            if (dr <= cumulative)
            {
                index = i;
                break;
            }
            
        }

        return index;
    }

    void OnChangedTradeList()
    {
        if (onChangedTrade != null)
            onChangedTrade();
    }
    //거래가 새롭게 갱신되며 삭제될때
    void RemoveTrade(TradeData data)
    {
        Save();

        if (onRemoveTrade != null)
            onRemoveTrade(data);
    }
    //거래를 중단하고 삭제시킬때
    public void CancleTrade(TradeData data)
    {
        tradeDataList.Remove(data);
        Save();
    }

    void SaveMarketIDList()
    {
        List<Dictionary<string, string>> saveList = new List<Dictionary<string, string>>();
        for (int i = 0; i < tradeDataList.Count; i++)
        {
            Dictionary<string, string> saveDic = new Dictionary<string, string>();
            saveDic.Add("sellMaterialID", tradeDataList[i].sellMaterialID);
            saveDic.Add("sellAmount", tradeDataList[i].sellAmount.ToString());
            saveDic.Add("buyMaterialID", tradeDataList[i].buyMaterialID);
            saveDic.Add("buyAmount", tradeDataList[i].buyAmount.ToString());
            saveDic.Add("remainingTime", tradeDataList[i].remainingTime.ToString());
            saveDic.Add("isTrade", tradeDataList[i].isTrade.ToString());

            saveList.Add(saveDic);
        }

        string saveListJson = JsonMapper.ToJson(saveList);

        //Debug.Log("저장함 jsonB : " + saveListJson);
        ObscuredPrefs.SetString("Market_" + User.Instance.userID + "_" + saveKeytradeDataList, saveListJson);
        ObscuredPrefs.SetFloat("Market_" + User.Instance.userID + "_" + saveKeyRemainingTime, remainingRenewalTime);
    }

    void LoadMarketIDList()
    {
        string key = "Market_" + User.Instance.userID + "_" + saveKeytradeDataList;
        if (ObscuredPrefs.HasKey(key))
        {

            string json = ObscuredPrefs.GetString(key);

            //Debug.Log(saveKeyVisitedHeroList + "저장데이터 있음 : " + json);
            List<Dictionary<string, string>> data = JsonMapper.ToObject<List<Dictionary<string, string>>>(new JsonReader(json));
            for (int i = 0; i < data.Count; i++)
            {
                string sellMaterialID = data[i]["sellMaterialID"];
                int sellAmount = int.Parse(data[i]["sellAmount"]);
                string buyMaterialID = data[i]["buyMaterialID"];
                int buyAmount = int.Parse(data[i]["buyAmount"]);
                float remainingTime = float.Parse(data[i]["remainingTime"]);
                bool isTrade = bool.Parse(data[i]["isTrade"]);

                TradeData tradeData = new TradeData();

                tradeData.sellMaterialID = sellMaterialID;
                tradeData.sellAmount = sellAmount;
                tradeData.buyMaterialID = buyMaterialID;
                tradeData.buyAmount = buyAmount;
                tradeData.remainingTime = remainingTime;
                tradeData.startTime = tradeData.remainingTime + Time.unscaledTime;
                tradeData.isTrade = isTrade;
                tradeDataList.Add(tradeData);
            }
            
        }

        key = "Market_" + User.Instance.userID + "_" + saveKeyRemainingTime;
        if(ObscuredPrefs.HasKey(key))
        {
            remainingRenewalTime = ObscuredPrefs.GetFloat(key);
            startTime = remainingRenewalTime + Time.unscaledTime;
        }

        if (tradeDataList.Count < defaultDataCount)
        {
            RenewalTrade();
        }
    }

    public delegate void OnChangedTrade();

    /// <summary> 거래 추가 됐을 때 </summary>
    public OnChangedTrade onChangedTrade;

    /// <summary> 거래 제거 됐을 때 </summary>
    public delegate void OnRemoveTrade(TradeData tradeData);
    public OnRemoveTrade onRemoveTrade;

    public void RenewalTrade()
    {
        if (tradeDataList.Count > 0)
        {
            for (int i = 0; i < tradeDataList.Count; i++)
            {
                if (tradeDataList[i].isTrade)
                    continue;

                tradeDataList.RemoveAt(i);
            }
        }

        TradeDataSet();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
            SaveMarketIDList();


        if (tradeDataList.Count > 0)
        {
            for (int i = 0; i < tradeDataList.Count; i++)
            {
                if (tradeDataList[i].isTrade == false)
                {
                    continue;
                }

                if (tradeDataList[i].remainingTime <= 0)
                {
                    Debug.Log("교역 완료");
                    
                    Storage.InItem(tradeDataList[i].buyMaterialID, tradeDataList[i].buyAmount);

                    tradeDataList[i].isFinished = true;
                }
                else
                {
                    tradeDataList[i].remainingTime = tradeDataList[i].startTime - Time.unscaledTime;
                }

            }
        }
        

        if (isInitialized)
        {
            if (startTime == 0)
            {
                startTime = remainingRenewalTime + Time.unscaledTime;
            }

            if (remainingRenewalTime <= 0)
            {
                remainingRenewalTime = renewalTimeDefault;
                startTime = remainingRenewalTime + Time.unscaledTime;
                RenewalTrade();
            }
            else
            {
                remainingRenewalTime = startTime - Time.unscaledTime;
            }
        }
    }
}
