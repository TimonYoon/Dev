using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIMarket : MonoBehaviour {

    [SerializeField]
    Text textRemainingAddTime;

    [SerializeField]
    GameObject marketSlotPrefab;

    [SerializeField]
    GridLayoutGroup content;

    [SerializeField]
    Text textCurrentSlotCount;

    RectTransform rectContent;


    //################################################

    /// <summary> 슬롯 풀  </summary>
    List<UIMarketSlot> marketSlotList = new List<UIMarketSlot>();


    void OnEnable()
    {
        SceneLobby.Instance.OnChangedMenu += OnChangedMenu;
        Show();
    }

    void OnDisable()
    {
        MarketManager.Instance.onChangedTrade -= OnChangedTrade;
        MarketManager.Instance.onRemoveTrade -= OnRemoveTrade;
    }

    void OnChangedMenu(LobbyState state)
    {
        if (state != LobbyState.SubMenu && SceneLobby.currentSubMenuState != SubMenuState.Market)
            Close();
    }


    void Show()
    {
        if (rectContent == null)
            rectContent = content.GetComponent<RectTransform>();

        //if (TerritoryManager.Instance == null)
        //    return;
        //Building building = null;

        //if (TerritoryManager.Instance.placeDic[TerritoryManager.Instance.currentPlaceID].building != null)
        //    building = TerritoryManager.Instance.placeDic[TerritoryManager.Instance.currentPlaceID].building;


        //if (building is BuildingHeroGuild)
        //{
        //    currentHeroGuild = building as BuildingHeroGuild;
        //    currentHeroGuild.onAddVisitedHero += OnAddVisitedHero;
        //    currentHeroGuild.onRemoveVisitedHero += OnRemoveVisitedHero;
        //    //Debug.Log("길드 데이터 존재");

        //    InittradeDataList();
        //}
        MarketManager.Instance.onChangedTrade += OnChangedTrade;
        MarketManager.Instance.onRemoveTrade += OnRemoveTrade;
        InitTradeDataList();
    }
    /// <summary> 슬롯 리스트 초기화 </summary>
    void InitTradeDataList()
    {
        if (MarketManager.Instance == null)
            return;


        for (int i = 0; i < MarketManager.Instance.tradeDataList.Count; i++)
        {
            // slot 생성 & 초기화
            UIMarketSlot slot = CreateMarketSlot(MarketManager.Instance.tradeDataList[i]);
            slot.gameObject.SetActive(true);
        }
        textCurrentSlotCount.text = "(" + MarketManager.Instance.tradeDataList.Count + "/" + MarketManager.Instance.defaultDataCount + ")";

        SizeControl(MarketManager.Instance.tradeDataList.Count);
    }

    /// <summary> 슬롯 생성 </summary>
    UIMarketSlot CreateMarketSlot(TradeData data)
    {
        UIMarketSlot slot = null;
        //for (int i = 0; i < marketSlotPool.Count; i++)
        //{
        //    if (marketSlotPool[i].gameObject.activeSelf == false && marketSlotPool[i].tradeData == null)
        //    {
        //        slot = marketSlotPool[i];
        //        break;

        //    }
        //}
        for (int j = 0; j < marketSlotList.Count; j++)
        {
            if (data== marketSlotList[j].tradeData)
            {
                slot = marketSlotList[j];
            }
        }
        if (slot == null)
        {
            GameObject go = Instantiate(marketSlotPrefab);
            go.transform.SetParent(content.transform, false);
            slot = go.GetComponent<UIMarketSlot>();
            slot.onClickRemove = MarketManager.Instance.CancleTrade;
            marketSlotList.Add(slot);
        }

        slot.InitMarketSlotData(data);

        return slot;
    }

    /// <summary> 거래 갱신 됐을때 </summary>
    void OnChangedTrade()
    {
        InitTradeDataList();
    }

    /// <summary> 거래 제거 됐을때 </summary>
    void OnRemoveTrade(TradeData trade)
    {
        UIMarketSlot slot = marketSlotList.Find(x => x.tradeData == trade);
        marketSlotList.Remove(slot);
        Destroy(slot.gameObject);
        textCurrentSlotCount.text = "(" + marketSlotList.Count + "/" + MarketManager.Instance.defaultDataCount + ")";
    }

    /// <summary> 교역권 사용 눌렸을 때 </summary>
    public void OnClickRenewalTradeButton()
    {
        //교역권 소모 구현 필요
        MarketManager.Instance.RenewalTrade();
    }
    

    /// <summary> mail content size 조절 </summary>
    void SizeControl(float count)
    {
        double quotient = System.Math.Ceiling((double)count);

        rectContent.sizeDelta = new Vector2(0, (content.spacing.y + content.cellSize.y) * (int)quotient);
    }

    private void Update()
    {
        if (MarketManager.Instance == null)
            return;

        textRemainingAddTime.text = RemainTime();
    }

    string RemainTime()
    {
        string result = "";


        if (MarketManager.Instance == null)
            return result;

        float totalSecond = MarketManager.Instance.remainingRenewalTime;

        if (totalSecond > 3600f)
        {
            int a = (int)totalSecond % 3600;

            int hour = ((int)totalSecond - a) / 3600;

            int b = (int)a % 60;

            int minute = ((int)a - b) / 60;

            result = hour + "h " + minute + "m";
        }
        else if (totalSecond > 60f)
        {
            int a = (int)totalSecond % 60;

            int minute = ((int)totalSecond - a) / 60;

            result = minute + "m " + a + "s";
        }
        else
        {
            result = totalSecond.ToString("N0") + "s";
        }


        return result;
    }

    public void OnClickCloseButton()
    {
        SceneLobby.Instance.SceneChange(LobbyState.Lobby);
    }

    public void Close()
    {
        SceneLobby.Instance.OnChangedMenu -= OnChangedMenu;
        SceneManager.UnloadSceneAsync("Market");
    }
}
