using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using LitJson;
using UnityEngine.Advertisements;
using CodeStage.AntiCheat.ObscuredTypes;


// 상점 관련 정적 데이터
public class ShopGameData
{
    public ShopGameData(JsonData data)
    {
        goodsID = data["goodsID"].ToStringJ();
        goodsName = data["goodsName_1"].ToStringJ();
        if (data.ContainsKey("goodsName_2"))
            goodsName2 = JsonParser.ToString(data["goodsName_2"]);
        goodsDescription = JsonParser.ToString(data["goodsDescription_1"]);
        if (data.ContainsKey("goodsDescription_2"))
            goodsDescription2 = JsonParser.ToString(data["goodsDescription_2"]);
        goodsSpriteName = JsonParser.ToString(data["goodsSpriteName"]);
        paySpriteName = JsonParser.ToString(data["paySpriteName"]);
        doubleFlag = data["doubleFlag"].ToStringJ();
        bonusAmount = data["bonusAmount"].ToStringJ();
    }
    /// <summary> 상품 고유 ID </summary>
    public ObscuredString goodsID;
    /// <summary> 상품 이름 </summary>
    public string goodsName;
    /// <summary> 상품 이름2(특정 상황에 교체될 이름) </summary>
    public string goodsName2;
    /// <summary> 상품 내용 </summary>
    public string goodsDescription;
    /// <summary> 상품 내용2(특정 상황에 교체될 설명) </summary>
    public string goodsDescription2;
    /// <summary> 상품 이미지 이름 </summary>
    public string goodsSpriteName;
    /// <summary> 상품 구매시 소비자원 이미지 이름 </summary>
    public string paySpriteName;
    /// <summary> 1+1 상품 여부 </summary>
    public string doubleFlag;
    /// <summary> bonus태그에 들어갈 문구 및 수량 </summary>
    public string bonusAmount;

}

/// <summary>
/// 상점 관련 데이터를 서버에서 받아 저장하고 관리하는 곳
/// </summary>
public class ShopDataController : MonoBehaviour
{
    public static ShopDataController Instance;
    // public List<ShopData> ShopDataList { get; private set; }

    public static List<ShopData> shopPackageDataList { get; private set; }
    /// <summary> 서버에서 받아오는 아이템 리스트</summary>
    public static List<ShopData> shopHeroDataList { get; private set; }
    /// <summary> 서버에서 받아오는 재화 리스트</summary>
    public static List<ShopData> shopGoldDataList { get; private set; }

    public static List<ShopData> shopRubyDataList { get; private set; }

    /// <summary> 서버에서 받아오는 버프 리스트 </summary>
    public static List<ShopData> shopBuffDataList { get; private set; }

    public List<UIShopProductSlot> shopSlotList = new List<UIShopProductSlot>();

    public List<UIPackageSlot> packageSlotList = new List<UIPackageSlot>();

    public bool isInitialized { get; private set; }

    void OnEnable()
    {
        Instance = this;
        StartCoroutine(InitShopDataCoroutine());
        SceneLobby.Instance.OnChangedMenu += OnChangedMenu;
        UIShopProductSlot.onClickItemButtonCallback += UpdateMoneyDataByShop;
        UIPackageSlot.onClickItemButtonCallback += UpdateMoneyDataByShop;
    }

    void OnDisable()
    {
        UIShopProductSlot.onClickItemButtonCallback -= UpdateMoneyDataByShop;
        UIPackageSlot.onClickItemButtonCallback -= UpdateMoneyDataByShop;
    }

    public delegate void ShopDataControllerCallBack(ShopDataController value);
    /// <summary> 서버에서 받은 상점 data가 준비되면 발동 </summary>
    public ShopDataControllerCallBack onShopDataPrepared;

    public delegate void OnChangedShowShop(ShopType type);
    public OnChangedShowShop onChangedShowShop;

    void OnChangedMenu(LobbyState state)
    {
        if (state != LobbyState.Shop)
            return;


        ShowShop(ShopType.Hero);
    }

    public delegate void OnRemoveShopData(string id);
    public OnRemoveShopData OnRemove;
    void RemoveShopData(ShopData shop)
    {
        ShopData shopData = shop;
        string id = shop.id;
        ShopType type = ShopType.Ruby;
        if (shopPackageDataList.Find(x => x.id == shopData.id) != null)
        {
            shopPackageDataList.Remove(shopPackageDataList.Find(x=>x.id == shopData.id));
            packageSlotList.Remove(packageSlotList.Find(x => x.shopProductSlotData.id == shopData.id));
            type = ShopType.Package;
        }
        else if(shopRubyDataList.Find(x => x.id == shopData.id) != null)
        {
            shopRubyDataList.Remove(shopRubyDataList.Find(x => x.id == shopData.id));
            shopSlotList.Remove(shopSlotList.Find(x => x.shopProductSlotData.id == shopData.id));
            type = ShopType.Ruby;
        }
        else if(shopHeroDataList.Find(x => x.id == shopData.id) != null)
        {
            shopHeroDataList.Remove(shopRubyDataList.Find(x => x.id == shopData.id));
            shopSlotList.Remove(shopSlotList.Find(x => x.shopProductSlotData.id == shopData.id));
            ShowShop(ShopType.Hero);
        }
        else if(shopBuffDataList.Find(x => x.id == shopData.id) != null)
        {
            shopBuffDataList.Remove(shopRubyDataList.Find(x => x.id == shopData.id));
            shopSlotList.Remove(shopSlotList.Find(x => x.shopProductSlotData.id == shopData.id));
            ShowShop(ShopType.Buff);
        }
        else if(shopGoldDataList.Find(x => x.id == shopData.id) != null)
        {
            shopGoldDataList.Remove(shopRubyDataList.Find(x => x.id == shopData.id));
            shopSlotList.Remove(shopSlotList.Find(x => x.shopProductSlotData.id == shopData.id));
            ShowShop(ShopType.Gold);
        }

        if (OnRemove != null)
            OnRemove(id);

        ShowShop(type);
    }

    /// <summary> 상점 보기 </summary>
    public void ShowShop(ShopType type)
    {
        //Debug.Log("들어옴" + type.ToString());
        if (onChangedShowShop != null)
            onChangedShowShop(type);
    }

    /// <summary> 상점 Data 초기화 </summary>
    IEnumerator InitShopDataCoroutine()
    {
        // 상점 정적 데이터를 넣어줌.
        //yield return StartCoroutine(InitGoodsStaicDataList());

        /// TO DO : 구현해 줘야 한다.
        shopPackageDataList = new List<ShopData>();

        shopHeroDataList = new List<ShopData>();
        shopGoldDataList = new List<ShopData>();
        shopRubyDataList = new List<ShopData>();
        shopBuffDataList = new List<ShopData>();

        WWWForm form = new WWWForm();
        string result = "";
        string php = "ShopInfo.php";
        form.AddField("userID", User.Instance.userID);
        form.AddField("type", 4);
        yield return StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(php, form, x => result = x));

        //result = result.Replace("Success/", "");

        JsonData jData = ParseCheckDodge(result);
        for (int i = 0; i < jData.Count; i++)
        {
            // 유효성 검사
            if (jData[i]["category"] == null)
                continue;

            ShopData shopData = new ShopData();
            if (jData[i]["category"].ToString() == "package")
            {
                // 서버 데이터 
                shopData.id = jData[i]["id"].ToString();
                shopData.Index = jData[i]["index"].ToString();

                shopData.category = jData[i]["category"].ToString();
                shopData.costType = jData[i]["costType"].ToString();
                shopData.price = jData[i]["price"].ToString();
                shopData.enable = jData[i]["enable"].ToString();

                shopData.productType = jData[i]["productType"].ToString();
                shopData.productAmount = jData[i]["productAmount"].ToString();
                if (jData[i]["maxCount"] != null)
                    shopData.maxCount = jData[i]["maxCount"].ToString();

                //shopData.maxCount = jData[i]["maxCount"].ToString();

                //클락 정적 데이터

                if (GameDataManager.shopGameDataDic.ContainsKey(shopData.id) == true)
                {
                    shopData.goodsName = GameDataManager.shopGameDataDic[shopData.id].goodsName;
                    shopData.goodsDescription = GameDataManager.shopGameDataDic[shopData.id].goodsDescription;
                    shopData.goodsSpriteName = GameDataManager.shopGameDataDic[shopData.id].goodsSpriteName;
                    shopData.paySpriteName = GameDataManager.shopGameDataDic[shopData.id].paySpriteName;
                    shopData.doubleFlag = GameDataManager.shopGameDataDic[shopData.id].doubleFlag;
                    shopData.bonusAmount = GameDataManager.shopGameDataDic[shopData.id].bonusAmount;
                }
                else
                {
                    // TO DO : 에외처리
                }
                

                shopPackageDataList.Add(shopData);
                shopPackageDataList.Sort(delegate (ShopData A, ShopData B)
                {
                    if (int.Parse(A.Index) > int.Parse(B.Index)) return 1;
                    else if (int.Parse(A.Index) < int.Parse(B.Index)) return -1;
                    return 0;
                });
            }
            else if (jData[i]["category"].ToString() == "item")
            {
                // 서버 데이터 
                shopData.id = jData[i]["id"].ToString();
                shopData.Index = jData[i]["index"].ToString();

                shopData.category = jData[i]["category"].ToString();
                shopData.costType = jData[i]["costType"].ToString();
                shopData.price = jData[i]["price"].ToString();
                shopData.enable = jData[i]["enable"].ToString();

                shopData.productType = jData[i]["productType"].ToString();
                shopData.productAmount = jData[i]["productAmount"].ToString();

                // TODO :아직 서버에서 아무 값도 안넣어줌 - null
                // shopData.maxCount = jData[i]["maxCount"].ToString();

                // 클라 데이터

                if (GameDataManager.shopGameDataDic.ContainsKey(shopData.id) == true)
                {
                    shopData.goodsName = GameDataManager.shopGameDataDic[shopData.id].goodsName;
                    shopData.goodsDescription = GameDataManager.shopGameDataDic[shopData.id].goodsDescription;
                    shopData.goodsSpriteName = GameDataManager.shopGameDataDic[shopData.id].goodsSpriteName;
                    shopData.paySpriteName = GameDataManager.shopGameDataDic[shopData.id].paySpriteName;
                    shopData.doubleFlag = GameDataManager.shopGameDataDic[shopData.id].doubleFlag;
                    shopData.bonusAmount = GameDataManager.shopGameDataDic[shopData.id].bonusAmount;
                }
                else
                {
                    // TO DO : 에외처리
                }
                  
                

                shopHeroDataList.Add(shopData);
                shopHeroDataList.Sort(delegate (ShopData A, ShopData B)
                {
                    if (int.Parse(A.Index) > int.Parse(B.Index)) return 1;
                    else if (int.Parse(A.Index) < int.Parse(B.Index)) return -1;
                    return 0;
                });

            }
            else if (jData[i]["category"].ToString() == "gold")
            {
                // 서버 데이터 
                shopData.id = jData[i]["id"].ToString();
                shopData.Index = jData[i]["index"].ToString();

                shopData.category = jData[i]["category"].ToString();
                shopData.costType = jData[i]["costType"].ToString();
                shopData.price = jData[i]["price"].ToString();
                shopData.enable = jData[i]["enable"].ToString();

                shopData.productType = jData[i]["productType"].ToString();
                shopData.productAmount = jData[i]["productAmount"].ToString();

                // TODO :아직 서버에서 아무 값도 안넣어줌 - null
                //shopData.maxCount = jData[i]["maxCount"].ToString();

                // 클라 데이터
               
                if (GameDataManager.shopGameDataDic.ContainsKey(shopData.id) == true)
                {
                    shopData.goodsName = GameDataManager.shopGameDataDic[shopData.id].goodsName;
                    shopData.goodsDescription = GameDataManager.shopGameDataDic[shopData.id].goodsDescription;
                    shopData.goodsSpriteName = GameDataManager.shopGameDataDic[shopData.id].goodsSpriteName;
                    shopData.paySpriteName = GameDataManager.shopGameDataDic[shopData.id].paySpriteName;
                    shopData.doubleFlag = GameDataManager.shopGameDataDic[shopData.id].doubleFlag;
                    shopData.bonusAmount = GameDataManager.shopGameDataDic[shopData.id].bonusAmount;
                }
                else
                {
                    // TO DO : 에외처리
                }
                

                shopGoldDataList.Add(shopData);
                
            }
            else if (jData[i]["category"].ToString() == "ruby")
            {
                // 서버 데이터 
                shopData.id = jData[i]["id"].ToString();
                shopData.Index = jData[i]["index"].ToString();

                shopData.category = jData[i]["category"].ToString();
                shopData.costType = jData[i]["costType"].ToString();
                shopData.price = jData[i]["price"].ToString();
                shopData.enable = jData[i]["enable"].ToString();

                shopData.productType = jData[i]["productType"].ToString();
                shopData.productAmount = jData[i]["productAmount"].ToString();

                // TODO :아직 서버에서 아무 값도 안넣어줌 - null
                //shopData.maxCount = jData[i]["maxCount"].ToString();

                // 클라 데이터

                if (GameDataManager.shopGameDataDic.ContainsKey(shopData.id) == true)
                {
                    shopData.goodsName = GameDataManager.shopGameDataDic[shopData.id].goodsName;
                    shopData.goodsDescription = GameDataManager.shopGameDataDic[shopData.id].goodsDescription;
                    shopData.goodsSpriteName = GameDataManager.shopGameDataDic[shopData.id].goodsSpriteName;
                    shopData.paySpriteName = GameDataManager.shopGameDataDic[shopData.id].paySpriteName;
                    shopData.doubleFlag = GameDataManager.shopGameDataDic[shopData.id].doubleFlag;
                    shopData.bonusAmount = GameDataManager.shopGameDataDic[shopData.id].bonusAmount;
                }
                else
                {
                    // TO DO : 에외처리
                }
                

                shopRubyDataList.Add(shopData);
            }
            else if (jData[i]["category"].ToString() == "buff" || jData[i]["category"].ToString() == "nickname")
            {
                // 서버 데이터 
                shopData.id = jData[i]["id"].ToString();
                shopData.Index = jData[i]["index"].ToString();

                shopData.category = jData[i]["category"].ToString();
                shopData.costType = jData[i]["costType"].ToString();
                shopData.price = jData[i]["price"].ToString();
                shopData.enable = jData[i]["enable"].ToString();

                shopData.productType = jData[i]["productType"].ToString();
                shopData.productAmount = jData[i]["productAmount"].ToString();

                // TODO :아직 서버에서 아무 값도 안넣어줌 - null
                // shopData.maxCount = jData[i]["maxCount"].ToString();

                // 클라 데이터

                if (GameDataManager.shopGameDataDic.ContainsKey(shopData.id) == true)
                {
                    shopData.goodsName = GameDataManager.shopGameDataDic[shopData.id].goodsName;
                    shopData.goodsDescription = GameDataManager.shopGameDataDic[shopData.id].goodsDescription;
                    shopData.goodsSpriteName = GameDataManager.shopGameDataDic[shopData.id].goodsSpriteName;
                    shopData.paySpriteName = GameDataManager.shopGameDataDic[shopData.id].paySpriteName;
                    shopData.doubleFlag = GameDataManager.shopGameDataDic[shopData.id].doubleFlag;
                    shopData.bonusAmount = GameDataManager.shopGameDataDic[shopData.id].bonusAmount;
                }


                shopBuffDataList.Add(shopData);
            }
        }

        yield return (StartCoroutine(SortShopDataList()));

        isInitialized = true;

        if (null != onShopDataPrepared)
            onShopDataPrepared(this);
    }

    IEnumerator SortShopDataList()
    {
        shopPackageDataList.Sort(delegate (ShopData A, ShopData B)
        {
            if (int.Parse(A.Index) > int.Parse(B.Index)) return 1;
            else if (int.Parse(A.Index) < int.Parse(B.Index)) return -1;
            return 0;
        });
        shopHeroDataList.Sort(delegate (ShopData A, ShopData B)
        {
            if (int.Parse(A.Index) > int.Parse(B.Index)) return 1;
            else if (int.Parse(A.Index) < int.Parse(B.Index)) return -1;
            return 0;
        });
        shopGoldDataList.Sort(delegate (ShopData A, ShopData B)
        {
            if (int.Parse(A.Index) > int.Parse(B.Index)) return 1;
            else if (int.Parse(A.Index) < int.Parse(B.Index)) return -1;
            return 0;
        });
        shopRubyDataList.Sort(delegate (ShopData A, ShopData B)
        {
            if (int.Parse(A.Index) > int.Parse(B.Index)) return 1;
            else if (int.Parse(A.Index) < int.Parse(B.Index)) return -1;
            return 0;
        });
        shopBuffDataList.Sort(delegate (ShopData A, ShopData B)
        {
            if (int.Parse(A.Index) > int.Parse(B.Index)) return 1;
            else if (int.Parse(A.Index) < int.Parse(B.Index)) return -1;
            return 0;
        });

        yield return null;
    }
    
    void UpdateShopData()
    {
        StartCoroutine("UpdateShopDataCouroutine");
    }

    /// <summary> 상점 Data Update시 </summary>
    IEnumerator UpdateShopDataCouroutine()
    {
        /*
         * 기존의 상점과 변동되는 사항이 있으면. 추가하거나 삭제
         * TODO: 서버와의 통신은 고민해봐야함
         * 
         */

        yield return null;
    }

    //TO DO : 상점 거래 MoneyManager에서 이전
    Coroutine coroutine;
    void UpdateMoneyDataByShop(ShopData _value)
    {

        if (coroutine != null)
            return;
        Debug.Log(_value.goodsName);
        coroutine = StartCoroutine(UpdateMoneyDataByShopCoroutine(_value));
    }

    IEnumerator ServerShopDataCheck(int type, ShopData shopData, Action<string> resultData = null, string transactionID = null, string check = null)
    {
        WWWForm form = new WWWForm();
        form.AddField("userID", User.Instance.userID, System.Text.Encoding.UTF8);
        form.AddField("type", type);
        form.AddField("shopID", shopData.id, System.Text.Encoding.UTF8);
        if (transactionID != null)
            form.AddField("transactionID", transactionID);
        if (check != null)
            form.AddField("check", check);
        string php = "ShopInfo.php";
        string result = "";
        yield return StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(php, form, x => result = x));

        if(resultData != null)
            resultData(result);
        if (type != 5 && !string.IsNullOrEmpty(result) && result == "0")
        {
            UIPopupManager.ShowOKPopup("구매 실패", GameDataManager.moneyBaseDataDic[shopData.costType].name + "이(가) 부족합니다", null);
        }
        else if((type == 5 || type == 6) && !string.IsNullOrEmpty(result) && result == "2")
        {
            UIPopupManager.ShowOKPopup("구매 실패", "구매가 정상처리 되지 않았습니다", null);
        }
        else
        {
            UIPopupManager.ShowOKPopup("구매 완료", shopData.goodsName + "을(를) 구매하셨습니다", null);
            if(shopData.category == "package")
            {
                yield return StartCoroutine(MailManager.MailDataInitCoroutine());
            }
            
#if !UNITY_EDITOR
            yield return (StartCoroutine(AdController.Instance.CheckPayedPlayer()));
#endif
        }

        LoadingManager.Close();
        UIShop.Instance.loadingPanel.SetActive(false);
    }

    

    public IEnumerator UpdateMoneyDataByShopCoroutine(ShopData shopData)
    {
        LoadingManager.Show();
        if (shopData.category == "item")
        {
            //Debug.Log("아이템 카테고리는 서버에서 자체적으로 재화를 변동시킨다.");
            coroutine = null;
            yield break;
        }

        if(shopData.category == "nickname")
        {
            coroutine = null;
            LoadingManager.Close();
            yield break;
        }

        if (shopData.costType == "gold")
        {
            if (MoneyManager.GetMoney(MoneyType.gold).value - int.Parse(shopData.price) < 0)
            {
                UIPopupManager.ShowOKPopup("구매 실패", GameDataManager.moneyBaseDataDic[shopData.costType].name + "이(가) 부족합니다", null);
                UIShop.Instance.loadingPanel.SetActive(false);
            }
            
        }
        else if (shopData.costType == "ruby")
        {
            if (shopData.price != "광고보기" && MoneyManager.GetMoney(MoneyType.ruby).value - int.Parse(shopData.price) < 0 )
            {
                UIPopupManager.ShowOKPopup("구매 실패", GameDataManager.moneyBaseDataDic[shopData.costType].name + "이(가) 부족합니다", null);
                UIShop.Instance.loadingPanel.SetActive(false);
            }
            else
            {

                if (shopData.category == "gold")
                {
                    //moneyData.gold += int.Parse(shopData.productAmount);
                    yield return StartCoroutine(ServerShopDataCheck(1, shopData));
                }

                else if (shopData.category == "buff" && shopData.price == "광고보기")
                {
                    yield return StartCoroutine(UIAdAlarmController.Instance.ShowAdForBoost());

                    //AdController.Instance.ShowRewardAD();

                    //while (AdController.Instance.isShow)
                    //    yield return null;

                    //yield return null;

                    //if (AdController.Instance.isFailed)
                    //{
                    //    FailedShowAD();
                    //    tempAdFailData = shopData;
                    //    UIShop.Instance.loadingPanel.SetActive(false);
                    //    coroutine = null;
                    //    yield break;
                    //}

                    //if (AdController.Instance.isSuccess)
                    //{
                        
                    //    OptionManager.ApplyBoost(float.Parse(shopData.productAmount));
                    //    UIAdAlarmController.SaveFreeBoostCoolTime();
                    //    UIPopupManager.ShowOKPopup("구매 완료", shopData.goodsName + "이(가) 적용됐습니다", null);
                    //}
                    ////TODO : 버프 아이템 구매에 따른 보상 처리 해줘야 함.               
                }
                else if (shopData.category == "ruby" && shopData.price == "광고보기")
                {
                    yield return StartCoroutine(UIAdAlarmController.Instance.ShowAdForRuby());

                    //AdController.Instance.ShowRewardAD();

                    //while (AdController.Instance.isShow)
                    //    yield return null;

                    //yield return null;

                    //if (AdController.Instance.isFailed)
                    //{
                    //    FailedShowAD();
                    //    tempAdFailData = shopData;
                    //    coroutine = null;
                    //    UIShop.Instance.loadingPanel.SetActive(false);
                    //    yield break;
                    //}

                    //if (AdController.Instance.isSuccess)
                    //{
                    //    yield return StartCoroutine(ServerShopDataCheck(3, shopData));
                    //    UIAdAlarmController.SaveFreeRubyCoolTime();
                    //}
                    ////TODO : 버프 아이템 구매에 따른 보상 처리 해줘야 함.               
                }
                else if(shopData.category == "buff" && shopData.price != "광고보기")
                {
                    if(GameDataManager.moneyBaseDataDic.ContainsKey(shopData.productType))
                    {
                        string result = string.Empty;
                        yield return StartCoroutine(ServerShopDataCheck(7, shopData, x => result = x));
                    }
                    else
                    {
                        string result = string.Empty;
                        yield return StartCoroutine(ServerShopDataCheck(2, shopData, x => result = x));
                        if (!string.IsNullOrEmpty(result) && result != "0")
                        {
                            OptionManager.ApplyBoost(float.Parse(shopData.productAmount));
                        }
                        ////TODO : 버프 아이템 구매에 따른 보상 처리 해줘야 함. php파일 수정 포함
                    }
                }
            }
        }
        else if (shopData.costType == "cash")
        {
            // TO DO : 바로 현금 결제로 이어져야 한다.- 무조건이다. 현재 보유 금액 체크 필요 없음

            if (shopData.productType == "ruby")
            {
                //현금결제완료되면 삭제 -> IAPManager로 옮김
                //AdController.Instance.DeleteBanner();
                IAPManager.Instance.BuyInAppProduct(shopData.id);

                if (IAPManager.Instance.isFailed)
                {
                    UIPopupManager.ShowOKPopup("구매실패", "유효하지 않은 거래입니다", null);
                    UIShop.Instance.loadingPanel.SetActive(false);
                    coroutine = null;
                    yield break;
                }
                while (!IAPManager.Instance.isSuccess)
                    yield return null;

                
#if UNITY_EDITOR
                yield return StartCoroutine(ServerShopDataCheck(3, shopData));
#endif
#if !UNITY_EDITOR
                yield return StartCoroutine(ServerShopDataCheck(5, shopData, null, IAPManager.Instance.transactionID));
#endif
                yield return StartCoroutine(CheckPackageBuyable(shopData));
                if(buyable == Buyable.Limited)
                {
                    RemoveShopData(shopData);
                }
            }
            else if(shopData.productType == "package")
            {
                yield return StartCoroutine(CheckPackageBuyable(shopData));

                if (buyable == Buyable.Overlap)
                {
                    UIPopupManager.ShowYesNoPopup("구매 중지", "아직 사용기간이 남은 월정액 상품입니다\n재구매시 기간이 연장됩니다 계속하시겠습니까?", RetryPackageBuy);
                    tempPackageData = shopData;
                    UIShop.Instance.loadingPanel.SetActive(false);
                    coroutine = null;
                    yield break;
                }
                else if(buyable == Buyable.Limited)
                {
                    UIPopupManager.ShowOKPopup("구매 실패", "구매 가능 횟수를 초과하셨습니다", null);
                    UIShop.Instance.loadingPanel.SetActive(false);
                    coroutine = null;
                    yield break;
                }

                IAPManager.Instance.BuyInAppProduct(shopData.id);

                if (IAPManager.Instance.isFailed)
                {
                    UIPopupManager.ShowOKPopup("구매실패", "유효하지 않은 거래입니다", null);
                    UIShop.Instance.loadingPanel.SetActive(false);
                    coroutine = null;
                    yield break;
                }
                while (!IAPManager.Instance.isSuccess)
                    yield return null;

#if UNITY_EDITOR
                yield return StartCoroutine(ServerShopDataCheck(99, shopData));
#endif
#if !UNITY_EDITOR
                yield return StartCoroutine(ServerShopDataCheck(6, shopData, null, IAPManager.Instance.transactionID));
#endif
                yield return StartCoroutine(CheckPackageBuyable(shopData));
                if (buyable == Buyable.Limited)
                {
                    RemoveShopData(shopData);
                }

            }
        }


        //TODO : 유효성 검사

        // TODO :  클라에서 체크/ 서버에서 체크 => 해줘야 한다.  2개의 값이 같은지 같지 않으면 오류 메세지 보내준다. 
        // 같으면 다음 줄 실행한다.
        UIShop.Instance.loadingPanel.SetActive(false);
        coroutine = null;
    }

    ShopData tempPackageData;
    void RetryPackageBuy(string result)
    {
        if(result == "yes")
        {
            StartCoroutine(OverlapFixedCharge(tempPackageData));
        }
        else
        {
            tempPackageData = null;
            return;
        }
    }

    IEnumerator OverlapFixedCharge(ShopData shopData)
    {
        IAPManager.Instance.BuyInAppProduct(shopData.id);

        if (IAPManager.Instance.isFailed)
        {
            UIPopupManager.ShowOKPopup("구매실패", "유효하지 않은 거래입니다", null);
            coroutine = null;
            yield break;
        }
        while (!IAPManager.Instance.isSuccess)
            yield return null;

#if UNITY_EDITOR
        yield return StartCoroutine(ServerShopDataCheck(99, shopData, null, null, "check"));
#endif
#if !UNITY_EDITOR
                yield return StartCoroutine(ServerShopDataCheck(6, shopData, null, IAPManager.Instance.transactionID, "check"));
#endif
    }


    ShopData tempAdFailData;
    public void FailedShowAD()
    {
        UIPopupManager.ShowYesNoPopup("광고 시청 취소", "광고 시청이 취소되어 보상이 지급되지 않습니다\n다시 시청하시겠습니까", RetryShowAD);
    }

    void RetryShowAD(string result)
    {
        if (result == "yes")
        {
            UpdateMoneyDataByShop(tempAdFailData);
        }
        else
        {
            tempAdFailData = null;
            return;
        }
    }

    public void UpdateShopSlot(ShopData shopData)
    {

        if(shopData.category == "nickname" && User.Instance.changeNickname == 1)
        {
            for (int i = 0; i < shopSlotList.Count; i++)
            {
                if (shopSlotList[i].shopProductSlotData.id == shopData.id)
                    shopSlotList[i].InitItemSlotUI();
            }
        }
    }

    enum Buyable { Buy, Overlap, Limited};
    Buyable buyable;
    IEnumerator CheckPackageBuyable(ShopData shopData)
    {
        buyable = Buyable.Buy;
        WWWForm form = new WWWForm();
        form.AddField("userID", User.Instance.userID, System.Text.Encoding.UTF8);
        form.AddField("type", 100);
        form.AddField("shopID", shopData.id, System.Text.Encoding.UTF8);
        string php = "ShopInfo.php";
        string result = "";
        yield return StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(php, form, x => result = x));

        if(!string.IsNullOrEmpty(result))
        {
            if (result == "1")
                buyable = Buyable.Overlap;
            else if (result == "2")
                buyable = Buyable.Limited;
        }
    }

    JsonData ParseCheckDodge(string wwwString)
    {
        if (string.IsNullOrEmpty(wwwString))
            return null;

        // 중요.. 유니코드를 한글로 변환 시켜주는 함수.. 
        //JsonParser jsonParser = new JsonParser();
        //wwwString = jsonParser.Decoder(wwwString);

        //DB에 지정된 필드 이름 참조할 것.
        JsonReader jReader = new JsonReader(wwwString);
        JsonData jData = JsonMapper.ToObject(jReader);
        return jData;
    }
}
