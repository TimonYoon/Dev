using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using LitJson;

/// <summary> 뽑기 관련 컨트롤러</summary>
public class DrawManager : MonoBehaviour {

    // TO DO : 다른 곳에서 뽑기를 하면 현재 있는 곳에서 뽑기를 실행한다. 
    public static DrawManager Instance;

    public delegate void DrawCallback();

    /// <summary> 뽑기로 영웅 추가됬을 때 </summary>
    public static DrawCallback onDrawHeroAdd;

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        //MoneyManager.onMoneyManagerPreparedForDraw += DrawStart;
        UIShopProductSlot.onClickItemButtonCallback += DrawStart;
        UIAdAlarmController.onHeroAdfinish += FreeDrawStart;
        WebServerConnectManager.onWebServerResult += OnWebServerResult;
    }
    void OnDisable()
    {
        //MoneyManager.onMoneyManagerPreparedForDraw -= DrawStart;
        UIShopProductSlot.onClickItemButtonCallback -= DrawStart;
        UIAdAlarmController.onHeroAdfinish -= FreeDrawStart;
        WebServerConnectManager.onWebServerResult -= OnWebServerResult;
    }
    public Coroutine drawCoroutine { get; private set; }

   
    void DrawStart(ShopData _shopData)
    {
        if (_shopData.category != "item")
            return;

        if (drawCoroutine != null)
            return;

        drawCoroutine = StartCoroutine(InitDraw(_shopData));
    }

    void FreeDrawStart(ShopData _shopData)
    {
        if (_shopData.category != "item")
            return;

        if (drawCoroutine != null)
            return;

        drawCoroutine = StartCoroutine(InitFreeDraw(_shopData));
    }

    void OnWebServerResult(Dictionary<string, object> resultDataDic)
    {
        if (resultDataDic.ContainsKey("drawHero"))
        {
            JsonReader json = new JsonReader(JsonMapper.ToJson(resultDataDic["drawHero"]));
            JsonData jsonData = JsonMapper.ToObject(json);
            
            if (jsonData.Count > 1)
            {
                if (drowHeroIDList == null)
                    drowHeroIDList = new List<string>();

                drowHeroIDList.Clear();

                if (jsonData.GetJsonType().ToString()  == "Array")
                {
                    isSingular = false;

                    for (int i = 0; i < jsonData.Count; i++)
                    {
                        drowHeroIDList.Add(JsonParser.ToString(jsonData[i]["id"]));
                    }

                    //char[] chars = { ',' };
                    //drowHeroIDList = new List<string>(_drawResult.Split(chars));
                    // 10개 뽑기
                }
                else
                {
                    isSingular = true;

                    drowHeroIDList.Add(JsonParser.ToString(jsonData["id"]));
                    
                    //drowHeroIDList = new List<string>();
                    //drowHeroIDList.Add(_drawResult);
                    // 단일 뽑기
                }
                //Debug.Log("뽑앗다 : " + _drawResult.ToString());
                //StartCoroutine(ShowDrawScene());
            }
        }

        if (resultDataDic.ContainsKey("specialDrawHero"))
        {
            JsonReader json = new JsonReader(JsonMapper.ToJson(resultDataDic["specialDrawHero"]));
            JsonData jsonData = JsonMapper.ToObject(json);

            if (jsonData.Count > 1)
            {
                if (drowHeroIDList == null)
                    drowHeroIDList = new List<string>();

                drowHeroIDList.Clear();

                
                isSingular = true;

                drowHeroIDList.Add(jsonData["id"].ToString());
                
            }
            isSpecialDraw = true;
            StartCoroutine(ShowDrawScene());
        }
    }
    public static bool isSpecialDraw = false;

    IEnumerator ShowDrawScene()
    {
        string assetBundle = "scene/draw";
        string sceneName = "Draw";
        bool isAdditive = true;

        yield return StartCoroutine(ShowScene(assetBundle, sceneName, isAdditive));
        
    }
 

    public bool isSingular { get; private set; }
    public List<string>  drowHeroIDList { get; private set; }
    IEnumerator InitDraw(ShopData _shopData)
    {
        
           
        LoadingManager.Show();
        string _drawResult = "";
        string _php = "Draw.php";

        WWWForm _form = new WWWForm();
        _form.AddField("userID", PlayerPrefs.GetString("userID"));
        _form.AddField("shopID", _shopData.id);
        _form.AddField("type", "1");

        yield return StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(_php,_form,x=>_drawResult =x));
        
        drawCoroutine = null;
        if (!string.IsNullOrEmpty(_drawResult))
        {
            // 1  = 재화 부족
            if(_drawResult == "1")
            {
                LoadingManager.Close();
                UIPopupManager.ShowOKPopup("알림", GameDataManager.moneyBaseDataDic[_shopData.costType].name + "이(가) 부족합니다.", null);
                if (UIShop.Instance != null && UIShop.Instance.loadingPanel.activeSelf)
                    UIShop.Instance.loadingPanel.SetActive(false);
                yield break;
            }
        }
        else
        {
            StartCoroutine(ShowDrawScene());
        }

        if (_shopData.price == "광고보기")
            UIAdAlarmController.SaveFreeHeroCoolTime();
        
    }

    IEnumerator InitFreeDraw(ShopData _shopData)
    {
        LoadingManager.Show();
        string _drawResult = "";
        string _php = "Draw.php";

        WWWForm _form = new WWWForm();
        _form.AddField("userID", PlayerPrefs.GetString("userID"));
        _form.AddField("shopID", _shopData.id);
        _form.AddField("type", "2");

        yield return StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(_php, _form, x => _drawResult = x));
        
        drawCoroutine = null;
        StartCoroutine(ShowDrawScene());
        UIAdAlarmController.SaveFreeHeroCoolTime();
    }

    IEnumerator ShowScene(string assetBundle, string sceneName, bool isAdditive = true)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.isLoaded)
        {
            while (!AssetLoader.Instance)
                yield return null;

            yield return StartCoroutine(AssetLoader.Instance.LoadLevelAsync(assetBundle, sceneName, isAdditive));

        }
        LoadingManager.Close();
        yield break;
    }


}
