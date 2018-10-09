using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System;
using CodeStage.AntiCheat.ObscuredTypes;

public class UIAdAlarmController : MonoBehaviour
{
    public static UIAdAlarmController Instance;

    public Canvas canvas;

    [Header("2배속 버튼")]
    public GameObject objBoostSpeed;

    [Header("공짜 영웅")]
    public GameObject objFreeHero;

    [Header("공짜 루비")]
    public GameObject objFreeRuby;

    //공짜영웅 1~2성 1마리
    ShopData freeHeroData;
    //공짜루비 10개
    ShopData freeRuby;

    const float constFreeHeroCoolTime = 43200f;
    const float constFreeRubyCoolTime = 3600f;
    const float constFreeBoostCoolTime = 900f;

    static public SimpleDelegate onGetFreeHero;

    DateTime _freeHeroStartTime = DateTime.MinValue;
    static DateTime freeHeroStartTime
    {
        get { return Instance._freeHeroStartTime; }
        set { Instance._freeHeroStartTime = value; }
    }

    ObscuredFloat _freeHeroRemainTime = 0f;
    /// <summary> 부스트 효과 남은 시간 </summary>
    static public ObscuredFloat freeHeroRemainTime
    {
        get { return Instance._freeHeroRemainTime; }
        private set
        {
            Instance._freeHeroRemainTime = value;

        }
    }

    static public SimpleDelegate onGetFreeRuby;

    DateTime _freeRubyStartTime = DateTime.MinValue;
    static DateTime freeRubyStartTime
    {
        get { return Instance._freeRubyStartTime; }
        set { Instance._freeRubyStartTime = value; }
    }

    ObscuredFloat _freeRubyRemainTime = 0f;
    /// <summary> 부스트 효과 남은 시간 </summary>
    static public ObscuredFloat freeRubyRemainTime
    {
        get { return Instance._freeRubyRemainTime; }
        private set
        {
            Instance._freeRubyRemainTime = value;

        }
    }

    static public SimpleDelegate onGetFreeBoost;

    DateTime _freeBoostStartTime = DateTime.MinValue;
    static DateTime freeBoostStartTime
    {
        get { return Instance._freeBoostStartTime; }
        set { Instance._freeBoostStartTime = value; }
    }

    ObscuredFloat _freeBoostRemainTime = 0f;
    /// <summary> 부스트 효과 남은 시간 </summary>
    static public ObscuredFloat freeBoostRemainTime
    {
        get { return Instance._freeBoostRemainTime; }
        private set
        {
            Instance._freeBoostRemainTime = value;

        }
    }
    public delegate void ShowHeroAdFinishCallback(ShopData shopData);
    public static ShowHeroAdFinishCallback onHeroAdfinish;
    //###################################################################
    void Awake()
    {
        if (!OptionManager.Instance)
            return;

        Instance = this;
        
        SceneLobby.Instance.OnChangedMenu += OnChangedLobbyMenu;

        canvas.gameObject.SetActive(false);
    }
    IEnumerator Start()
    {
        yield return (StartCoroutine(InitShopDataCoroutine()));

        canvas.gameObject.SetActive(true);

        InitCoolTime();

    }

    void InitCoolTime()
    {
        string key = "";
        //무스트 시작 시간
        key = "FreeHeroStartTime";
        if (ObscuredPrefs.HasKey(key))
        {
            string s = ObscuredPrefs.GetString(key);
            if (!string.IsNullOrEmpty(s))
            {
                DateTime.TryParse(s, out _freeHeroStartTime);
                Debug.Log("공짜 영웅 획득 시간 : " + freeHeroStartTime);
            }
        }

        key = "FreeHeroRemainTime";
        if (ObscuredPrefs.HasKey(key))
            freeHeroRemainTime = ObscuredPrefs.GetFloat(key);


        float c = constFreeHeroCoolTime - (float)(DateTime.Now - freeHeroStartTime).TotalSeconds;

        if (c < 0f)
            c = 0f;

        Instance.StartCoroutine(ApplyFreeHeroCoolTime(c));

        //무스트 시작 시간
        key = "FreeRubyStartTime";
        if (ObscuredPrefs.HasKey(key))
        {
            string s = ObscuredPrefs.GetString(key);
            if (!string.IsNullOrEmpty(s))
            {
                DateTime.TryParse(s, out _freeRubyStartTime);
                Debug.Log("공짜 루비 획득 시간 : " + freeRubyStartTime);
            }
        }

        key = "FreeRubyRemainTime";
        if (ObscuredPrefs.HasKey(key))
            freeRubyRemainTime = ObscuredPrefs.GetFloat(key);

        c = constFreeRubyCoolTime - (float)(DateTime.Now - freeRubyStartTime).TotalSeconds;

        if (c < 0f)
            c = 0f;

        Instance.StartCoroutine(ApplyFreeRubyCoolTime(c));


        //무스트 시작 시간
        key = "FreeBoostStartTime";
        if (ObscuredPrefs.HasKey(key))
        {
            string s = ObscuredPrefs.GetString(key);
            if (!string.IsNullOrEmpty(s))
            {
                DateTime.TryParse(s, out _freeBoostStartTime);
                Debug.Log("공짜 2배부스트 획득 시간 : " + freeBoostStartTime);
            }
        }

        key = "FreeBoostRemainTime";
        if (ObscuredPrefs.HasKey(key))
            freeBoostRemainTime = ObscuredPrefs.GetFloat(key);

        c = constFreeBoostCoolTime - (float)(DateTime.Now - freeBoostStartTime).TotalSeconds;

        if (c < 0f)
            c = 0f;

        Instance.StartCoroutine(ApplyFreeBoostCoolTime(c));
    }

    /// <summary> 공짜 영웅 광고 보기 종료되면 보상지급후 쿨타임 계산시작 </summary> 
    static public void SaveFreeHeroCoolTime(float coolTime = constFreeHeroCoolTime)
    {
        Instance.objFreeHero.SetActive(false);

        freeHeroStartTime = DateTime.Now;
        freeHeroRemainTime = coolTime;

        if (onGetFreeHero != null)
            onGetFreeHero();

        ObscuredPrefs.SetString("FreeHeroStartTime", freeHeroStartTime.ToString());
        ObscuredPrefs.SetFloat("FreeHeroRemainTime", freeHeroRemainTime);
        ObscuredPrefs.Save();

        coroutineFreeHeroCoolTime = Instance.StartCoroutine(ApplyFreeHeroCoolTime(coolTime));

    }
    static Coroutine coroutineFreeHeroCoolTime = null;
    static IEnumerator ApplyFreeHeroCoolTime(float coolTime = constFreeHeroCoolTime)
    {
        //남은 시간 갱신
        freeHeroRemainTime = coolTime;

        if (freeHeroRemainTime > 0f)
            Instance.objFreeHero.SetActive(false);


        float boostCool = coolTime + Time.unscaledTime;
        while (freeHeroRemainTime > 0f)
        {
            freeHeroRemainTime = boostCool - Time.unscaledTime;

            yield return null;

        }


        ObscuredPrefs.DeleteKey("FreeHeroStartTime");
        ObscuredPrefs.DeleteKey("FreeHeroRemainTime");
        ObscuredPrefs.Save();


        freeHeroStartTime = DateTime.MinValue;
        freeHeroRemainTime = 0f;

        if (onGetFreeHero != null)
            onGetFreeHero();

        Instance.objFreeHero.SetActive(true);

        yield break;
    }

    /// <summary> 공짜 루비 광고 보기 후 쿨타임 계산시작 , 영웅은 5분, 루비는 6시간 </summary>
    static public void SaveFreeRubyCoolTime(float coolTime = constFreeRubyCoolTime)
    {
        Instance.objFreeRuby.SetActive(false);

        freeRubyStartTime = DateTime.Now;
        freeRubyRemainTime = coolTime;

        if (onGetFreeRuby != null)
            onGetFreeRuby();


        ObscuredPrefs.SetString("FreeRubyStartTime", freeRubyStartTime.ToString());
        ObscuredPrefs.SetFloat("FreeRubyRemainTime", freeRubyRemainTime);
        ObscuredPrefs.Save();

        coroutineFreeRubyCoolTime = Instance.StartCoroutine(ApplyFreeRubyCoolTime(coolTime));

    }
    static Coroutine coroutineFreeRubyCoolTime = null;
    static IEnumerator ApplyFreeRubyCoolTime(float coolTime = constFreeRubyCoolTime)
    {
        //남은 시간 갱신
        freeRubyRemainTime = coolTime;

        if (freeRubyRemainTime > 0f)
            Instance.objFreeRuby.SetActive(false);

        float coolTimeRemain = coolTime + Time.unscaledTime;

        while (freeRubyRemainTime > 0f)
        {
            //freeRubyRemainTime -= Time.unscaledDeltaTime;
            freeRubyRemainTime = coolTimeRemain - Time.unscaledTime;
            yield return null;

        }


        ObscuredPrefs.DeleteKey("FreeRubyStartTime");
        ObscuredPrefs.DeleteKey("FreeRubyRemainTime");
        ObscuredPrefs.Save();

        if (onGetFreeRuby != null)
            onGetFreeRuby();

        freeRubyStartTime = DateTime.MinValue;
        freeRubyRemainTime = 0f;

        Instance.objFreeRuby.SetActive(true);

        yield break;
    }

    /// <summary> 공짜 부스트 광고 보기 후 쿨타임 계산시작 , 영웅은 5분, 루비는 6시간 </summary>
    static public void SaveFreeBoostCoolTime(float coolTime = constFreeBoostCoolTime)
    {
        Instance.objBoostSpeed.SetActive(false);

        freeBoostStartTime = DateTime.Now;
        freeBoostRemainTime = coolTime;

        if (onGetFreeBoost != null)
            onGetFreeBoost();

        ObscuredPrefs.SetString("FreeBoostStartTime", freeBoostStartTime.ToString());
        ObscuredPrefs.SetFloat("FreeBoostRemainTime", freeBoostRemainTime);
        ObscuredPrefs.Save();

        coroutineFreeBoostCoolTime = Instance.StartCoroutine(ApplyFreeBoostCoolTime(coolTime));

    }
    static Coroutine coroutineFreeBoostCoolTime = null;
    static IEnumerator ApplyFreeBoostCoolTime(float coolTime = constFreeBoostCoolTime)
    {
        //남은 시간 갱신
        freeBoostRemainTime = coolTime;

        if (freeBoostRemainTime > 0f)
            Instance.objBoostSpeed.SetActive(false);

        float coolTimeRemain = coolTime + Time.unscaledTime;
        while (freeBoostRemainTime > 0f)
        {
            //freeBoostRemainTime -= Time.unscaledDeltaTime;
            freeBoostRemainTime = coolTimeRemain - Time.unscaledTime;

            yield return null;

        }


        ObscuredPrefs.DeleteKey("FreeBoostStartTime");
        ObscuredPrefs.DeleteKey("FreeBoostRemainTime");
        ObscuredPrefs.Save();

        if (onGetFreeBoost != null)
            onGetFreeBoost();

        freeBoostStartTime = DateTime.MinValue;
        freeBoostRemainTime = 0f;

        Instance.objBoostSpeed.SetActive(true);

        yield break;
    }

    void OnChangedLobbyMenu(LobbyState menu)
    {
        //상점화면에서는 팝업 띄우지 말자
        canvas.gameObject.SetActive(menu != LobbyState.Shop);

        
    }
    /// <summary> 2배속 버튼 클릭 시 </summary>
    public void OnClickBoostSpeed()
    {
        //활성 불가능하면 버튼 안 눌림
        UIPopupManager.ShowYesNoPopup("2배속", "광고를 시청하시겠습니까?", PopupResultBoost);
    }

    /// <summary> 무료영웅 버튼 클릭 시 </summary>
    public void OnClickFreeHero()
    {
        //활성 불가능하면 버튼 안 눌림
        UIPopupManager.ShowYesNoPopup("무료 영웅 획득", "광고를 시청하시겠습니까?", PopupResultHero);
    }

    /// <summary> 무료루비 버튼 클릭 시 </summary>
    public void OnClickFreeRuby()
    {
        //활성 불가능하면 버튼 안 눌림
        UIPopupManager.ShowYesNoPopup("무료 루비 획득", "광고를 시청하시겠습니까?", PopupResultRuby);
    }

    void PopupResultBoost(string result)
    {
        if (result == "yes")
        {
#if !UNITY_EDITOR
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                UIPopupManager.ShowOKPopup("네트워크 연결 불량", "네트워크 연결 상태를 확인해주세요", null);
                return;
            }
#endif
            StartCoroutine(ShowAdForBoost());
        }
    }

    void PopupResultHero(string result)
    {
        if (result == "yes")
        {
#if !UNITY_EDITOR
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                UIPopupManager.ShowOKPopup("네트워크 연결 불량", "네트워크 연결 상태를 확인해주세요", null);
                return;
            }
#endif

            StartCoroutine(ShowAdForHero());
        }
    }

    void PopupResultRuby(string result)
    {
        if (result == "yes")
        {
#if !UNITY_EDITOR
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                UIPopupManager.ShowOKPopup("네트워크 연결 불량", "네트워크 연결 상태를 확인해주세요", null);
                return;
            }
#endif

            StartCoroutine(ShowAdForRuby());
        }
    }

    public IEnumerator ShowAdForBoost()
    {
        AdController.Instance.ShowRewardAD();

        while (AdController.Instance.isShow)
            yield return null;

        yield return null;

        if (AdController.Instance.isFailed)
        {
            UIPopupManager.ShowOKPopup("광고 시청 취소", "광고 시청이 취소되어 2배속이 적용되지 않았습니다.", null);
            yield break;
        }

        if (AdController.Instance.isSuccess)
        {

            OptionManager.ApplyBoost(2f);
            SaveFreeBoostCoolTime();
            UIPopupManager.ShowOKPopup("광고 시청 완료", "2배속이 적용됐습니다.", null);
        }
    }

    public IEnumerator ShowAdForHero()
    {
        AdController.Instance.ShowRewardAD();

        while (AdController.Instance.isShow)
            yield return null;

        yield return null;

        if (AdController.Instance.isFailed)
        {
            UIPopupManager.ShowOKPopup("광고 시청 취소", "광고 시청이 취소되어 무료 영웅을 획득하지 못했습니다.", null);
            yield break;
        }

        if (AdController.Instance.isSuccess)
        {
            if (onHeroAdfinish != null)
                onHeroAdfinish(freeHeroData);

        }
    }

    public IEnumerator ShowAdForRuby()
    {
        AdController.Instance.ShowRewardAD();

        while (AdController.Instance.isShow)
            yield return null;

        yield return null;

        if (AdController.Instance.isFailed)
        {
            UIPopupManager.ShowOKPopup("광고 시청 취소", "광고 시청이 취소되어 무료 영웅을 획득하지 못했습니다.", null);
            yield break;
        }

        if (AdController.Instance.isSuccess)
        {
            GetFreeRuby(freeRuby);
            UIPopupManager.ShowOKPopup("광고 시청 완료", GameDataManager.shopGameDataDic[freeRuby.id].goodsName +" 획득했습니다.", null);

            SaveFreeRubyCoolTime();
        }
    }
    /// <summary> 상점 Data 초기화 </summary>
    IEnumerator InitShopDataCoroutine()
    {

        WWWForm form = new WWWForm();
        string result = "";
        string php = "ShopInfo.php";
        form.AddField("userID", User.Instance.userID);
        form.AddField("type", 4);
        yield return StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(php, form, x => result = x));


        JsonData jData = ParseCheckDodge(result);
        for (int i = 0; i < jData.Count; i++)
        {
            ShopData shopData = new ShopData();
            if (jData[i]["id"].ToString() == "goods_item_005")
            {
                shopData.id = jData[i]["id"].ToString();
                shopData.category = jData[i]["category"].ToString();
                shopData.costType = jData[i]["costType"].ToString();
                freeHeroData = shopData;
            }
            else if (jData[i]["id"].ToString() == "goods_diamond_000")
            {
                shopData.id = jData[i]["id"].ToString();
                shopData.category = jData[i]["category"].ToString();
                shopData.costType = jData[i]["costType"].ToString();
                freeRuby = shopData;
            }
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

    void GetFreeRuby(ShopData _value)
    {
        StartCoroutine(ServerShopDataCheck(_value));
    }

    IEnumerator ServerShopDataCheck(ShopData shopData)
    {
        WWWForm form = new WWWForm();
        form.AddField("userID", User.Instance.userID, System.Text.Encoding.UTF8);
        form.AddField("type", 3);
        form.AddField("shopID", shopData.id, System.Text.Encoding.UTF8);
        string php = "ShopInfo.php";
        string result = "";
        yield return StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(php, form, x => result = x));

       
    }

   
}
