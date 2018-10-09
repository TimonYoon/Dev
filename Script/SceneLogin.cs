using UnityEngine;
using System.Collections;
using System;
using LitJson;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;
using AssetBundles;
using UnityEngine.UI;
using Facebook.Unity;
using UnityEngine.SocialPlatforms;


public enum LoginType 
{
    GuestLogin = 1,
    GoogleLogin = 2,
    FacebookLogin= 3,
    GoogleConnect = 4,
    FacebookConnect =5
}

/// <summary> 로그인을 기능을 하는 곳 </summary>
public class SceneLogin : MonoBehaviour {

    public static SceneLogin Instance;

    public GameObject loginPanel;    
    public Text versionText;
    public GameObject googleButton;

    [Header("번들 다운 관련")]
    public GameObject assetBundleDownLoadBar;
    public Text percentText;
    public GameObject messagePanel;
    public Text messageText;
    public List<string> assetBundleDownMessage;
    public RectTransform downloadProgressBar;

    [SerializeField]
    GameObject tipMessagePanel;
    [SerializeField]
    public Text tipMessageText;

    bool canDownload;
    float downloadProgressBarOrigalWidth;
    string storeURL = "";
    string versionName;

    /// <summary> 해당 빌드와 호환되는 번들 버전 </summary>
    public static string clientVersion { get; private set; }

    private AndroidJavaObject javaObject;
    int versionCode;
    string manufacturer;
    string model;
    string deviceID;

    [Header("이용약관 관련")]
    public GameObject agreementPanel;
    public Toggle agreeNecessaryToggle;
    public Toggle agreeSelectionToggle;
    bool isAgreeForUseNecessary = false;
    bool isAgreeFroUseSelection = false;
    bool agreeAllForUse = false;
    /* 
* to do : 
* 1. 중복로그인 체크 기능 만들기
* 2. 디바이스 정보 저장하기.
* 
*/

    public bool isFinishDownloadAssetBundles { get; private set; }


    void Awake()
    {
        Instance = this;
    }
    IEnumerator Start()
    {
        yield return null;

        Screen.fullScreen = true;

        InitClientVersion();

        yield return StartCoroutine(AssetLoader.Instance.Initialize());
        
        downloadProgressBarOrigalWidth = downloadProgressBar.sizeDelta.x;
        
        assetBundleDownLoadBar.SetActive(false);
        messagePanel.SetActive(true);

        StartCoroutine(messageCoroutine());
        //서버 활성화 유무 체크
        yield return StartCoroutine(ServerEnableCheck());
       
        //번들 체크
        yield return StartCoroutine(UpdateAssetBundles());        

        isFinishDownloadAssetBundles = true;

        // 게임 데이터 초기화

        tipMessagePanel.SetActive(true);
        yield return StartCoroutine(GameDataManager.Init());
        // 이용 약관 체크
        yield return StartCoroutine(coroutineAgree());

        // IOS 는 구글 로그인 버튼 비활성화
#if UNITY_IOS
        googleButton.SetActive(false);
#endif

        tipMessageText.text = "로그인 중..";
        if (PlayerPrefs.HasKey("userID"))
        {
            string userID = PlayerPrefs.GetString("userID");
            if (string.IsNullOrEmpty(userID))
            {
                loginPanel.SetActive(true);
                yield break;
            }
            else
            {
                if (GoogleManager.Instance && !string.IsNullOrEmpty(PlayerPrefs.GetString("google")))
                {
                    GoogleManager.Instance.LogIn();
                }

                if(FacebookManager.Instance && !string.IsNullOrEmpty(PlayerPrefs.GetString("facebook")))
                {
                    FacebookManager.Instance.LogIn();
                }

                StartCoroutine(Login(userID)); //클라이언트에 저장된 아이디로 바로 로그인
            }
        }
        else
        {
            loginPanel.SetActive(true);
        }
    }

   


    public delegate void FadeOutStartCallback();
    public FadeOutStartCallback onFadeOutStart;



    IEnumerator Login(string userID, string pletformID = "", LoginType loginType = LoginType.GuestLogin)
    {
        Debug.Log("로그인 시작");
        loginPanel.SetActive(false);


        //로그인 시 유저 데이터 구성부분
        WWWForm form = new WWWForm();
        form.AddField("userID", userID, System.Text.Encoding.UTF8); //테스트 아이디로 로그인한다.

        string googleID = "";
        if (PlayerPrefs.HasKey("google"))
            googleID = PlayerPrefs.GetString("google");
        else
        {
            if (loginType == LoginType.GoogleLogin)
                googleID = pletformID;
        }

        string facebookID = "";
        if (PlayerPrefs.HasKey("facebook"))
            facebookID = PlayerPrefs.GetString("facebook");
        else
        {
            if (loginType == LoginType.FacebookLogin)
                facebookID = pletformID;
        }

        tipMessageText.text = "로그인 정보를 내려받는 중..";
        form.AddField("google", googleID, System.Text.Encoding.UTF8);
        form.AddField("facebook", facebookID, System.Text.Encoding.UTF8);
        form.AddField("deviceModel", SystemInfo.deviceModel);
        form.AddField("deviceID", SystemInfo.deviceUniqueIdentifier);

        form.AddField("type", (int)loginType);

        string result = "";
        string php = "Login.php";
        yield return StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(php, form, x => result = x));
        JsonData jsonData = ParseCheckDodge(result);


        PlayerPrefs.SetString("userID", JsonParser.ToString(jsonData["id"].ToString()));

        if (loginType == LoginType.GoogleLogin)
            PlayerPrefs.SetString("google", JsonParser.ToString(jsonData["google"].ToString()));

        if (loginType == LoginType.FacebookLogin)
            PlayerPrefs.SetString("facebook", JsonParser.ToString(jsonData["facebook"].ToString()));
        
        // 아틀라스 캐싱
        string atlasName = "Atlas_Product";
        AssetBundleLoadAssetOperation r = AssetBundleManager.LoadAssetAsync("sprite/product", atlasName, typeof(UnityEngine.U2D.SpriteAtlas));
        yield return StartCoroutine(r);

        UnityEngine.U2D.SpriteAtlas atlas = r.GetAsset<UnityEngine.U2D.SpriteAtlas>();

        if (atlas != null)
        {
            if (!AssetLoader.cachedAtlasDic.ContainsKey(atlasName))
                AssetLoader.cachedAtlasDic.Add(atlasName, atlas);

        }
        string atlasName2 = "Atlas_Material";
        AssetBundleLoadAssetOperation r2 = AssetBundleManager.LoadAssetAsync("sprite/material", atlasName2, typeof(UnityEngine.U2D.SpriteAtlas));
        yield return StartCoroutine(r2);

        UnityEngine.U2D.SpriteAtlas atlas2 = r2.GetAsset<UnityEngine.U2D.SpriteAtlas>();

        if (atlas2 != null)
        {
            if (!AssetLoader.cachedAtlasDic.ContainsKey(atlasName2))
                AssetLoader.cachedAtlasDic.Add(atlasName2, atlas2);

        }


        //// 게임 데이타 초기화 끝날 때 까지 대기
        //while (!GameDataManager.isInitialized)
        //    yield return null;


        if (User.Instance)
            User.Instance.InitUserData(jsonData);

        Debug.Log("유저 데이터 초기화");
        
        while (!User.isInitialized)
            yield return null;
        
        Debug.Log("완료");
        // 유저 데이터 초기화 시작
        StartCoroutine(UserDataManager.Instance.Init());

        

        // 유저 데이터 초기화 완료 했는가 체크
        while (!UserDataManager.isInitialized)
            yield return null;
        tipMessageText.text = "왕국으로 진입중..";
        Debug.Log("Login UserID : " + JsonParser.ToString(jsonData["id"]));

        //enterButton.SetActive(true);



        //if (onFadeOutStart != null)
        //    onFadeOutStart();

        ////페이드아웃 기다리는 시간
        //yield return new WaitForSeconds(1.5f);

        yield return StartCoroutine(LoadingManager.FadeOutScreen());

        string nextSceneBundleName = "scene/lobby";
        string nextSceneName = "Lobby";
        AssetBundleLoadOperation operation = AssetBundleManager.LoadLevelAsync(nextSceneBundleName, nextSceneName, true);



        //// 몬스터 풀 초기화 끝났는가 체크
        //while (!MonsterPool.Instance)
        //    yield return null;

        //while (!MonsterPool.Instance.isInitialization)
        //    yield return null;

        //while (!Battle.Instance || !Battle.Instance.isInitialized)
        //    yield return null;


        while (!operation.IsDone())
            yield return null;


        versionText.gameObject.SetActive(false);
        messagePanel.SetActive(false);
        StopCoroutine(messageCoroutine());


        //while (!UILoginManager.isFinished)
        //    yield return null;


        Scene lobby = SceneManager.GetSceneByName("Lobby");
        Scene login = SceneManager.GetSceneByName("Login");
        Scene preLogin = SceneManager.GetSceneByName("PreLogin");

        SceneManager.SetActiveScene(lobby);
        SceneManager.UnloadSceneAsync(login);
        SceneManager.UnloadSceneAsync(preLogin);
    }



    /// <summary> 현재 클라이언트 버전 초기화 </summary>
    void InitClientVersion()
    {
        // todo : javaObject에서 데이터를 읽을수 없어서 우선 주석처리함 원인을 알아내야함
#if !UNITY_EDITOR && UNITY_ANDROID
        //서버 버전과 비교해서 업데이트 요청     
        
        clientVersion = Application.version;
        versionText.text = "v " + clientVersion;
#endif

#if !UNITY_EDITOR && UNITY_IOS
        versionName = Application.bundleIdentifier;
        int.TryParse(Application.version, out versionCode);        
        manufacturer = UnityEngine.iOS.Device.generation.ToString();
        model = SystemInfo.deviceModel
        deviceID = UnityEngine.iOS.Device.advertisingIdentifier;
        clientVersion = versionCode;
        versionText.text = "v " + clientVersion;
#endif
#if UNITY_EDITOR
        //versionName = "";
        //에디터의 경우
        //System.Guid g = System.Guid.NewGuid();
        int i = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        deviceID = "Editor_" + i;
        clientVersion = UnityEditor.PlayerSettings.bundleVersion;
        Debug.Log(clientVersion + " 버전 저장");
        versionText.text = "v " + clientVersion;
#endif
        WebServerConnectManager.clientVersion = clientVersion;
        //PlayerPrefs.SetString("version", clientVersion);
    }
    
    // to do : 모바일상에서는 해당 로그인 플랫폼의 아이디를 받아와서 사용할 예정
    public void OnClickGuestLoginButton()
    {
        if (coroutineLogin != null)
            return;

        coroutineLogin = StartCoroutine(PlatformLogin(LoginType.GuestLogin));
    }

    Coroutine coroutineLogin;
    public void OnClickGoogleLoginButton()
    {
        if (coroutineLogin != null)
            return;

        coroutineLogin = StartCoroutine(PlatformLogin(LoginType.GoogleLogin));
    }

    public void OnClickFaceBookLoginButton()
    {
        if (coroutineLogin != null)
            return;

        coroutineLogin = StartCoroutine(PlatformLogin(LoginType.GoogleLogin));
    }

    IEnumerator PlatformLogin(LoginType logintType)
    {
        string platformID = "";
        if(logintType == LoginType.GoogleLogin)
        {
            if (GoogleManager.Instance)
                GoogleManager.Instance.LogIn();

            while (GoogleManager.Instance.isInitialized)
                yield return null;

            while (string.IsNullOrEmpty(Social.localUser.id))
                yield return null;

            platformID = Social.localUser.id;
        }
        else if(logintType == LoginType.FacebookLogin)
        {
            //콜백으로 대체
#if UNITY_ANDROID
            if (FacebookManager.Instance)
                FacebookManager.Instance.LogIn();

            while (string.IsNullOrEmpty(FacebookManager.Instance.UserID))
                yield return null;

            platformID = FacebookManager.Instance.UserID;
#endif
        }
        else if(logintType == LoginType.GuestLogin)
        {
            UIPopupManager.ShowYesNoPopup("경고", "게스트 로그인시\n데이터 손실을\n책임지지 않습니다", AgreeGuestLogIn);
            coroutineLogin = null;
            yield break;
        }

        StartCoroutine(Login("", platformID, logintType));

        coroutineLogin = null;
    }
    void AgreeGuestLogIn(string result)
    {
        if (result == "yes")
        {
            StartCoroutine(Login("", "", LoginType.GuestLogin));
        }
    }

    JsonData ParseCheckDodge(string wwwString)
    {
        if (string.IsNullOrEmpty(wwwString))
            return null;

        wwwString = JsonParser.Decode(wwwString);

        //DB에 지정된 필드 이름 참조할 것.
        JsonReader jReader = new JsonReader(wwwString);
        JsonData jData = JsonMapper.ToObject(jReader);
        return jData;
    }


    /// <summary> 서버 활성화 체크 </summary>
    IEnumerator ServerEnableCheck()
    {
        
        string result = "";
        string php = "ServerStat.php";
        yield return /*StartCoroutine*/(WebServerConnectManager.Instance.WWWCoroutine(php, null, x => result = x));
        JsonData jData = ParseCheckDodge(result);
#if UNITY_ANDROID
        if(JsonParser.ToInt(jData["googleServerEnable"].ToString()) == 0)
            UIPopupManager.ShowOKPopup("서버점검", JsonParser.ToString(jData["noticeMessage"]), ClientExit);
        
        storeURL = JsonParser.ToString(jData["googleStoreURL"]);
#endif
#if UNITY_IOS
        if(JsonParser.ToInt(jData["appleServerEnable"].ToString()) == 0)
            UIPopupManager.ShowOKPopup("서버점검", JsonParser.ToString(jData["noticeMessage"]), ClientExit);
        
        storeURL = JsonParser.ToString(jData["appleStoreURL"]);
#endif

#if !UNITY_EDITOR
        if (VersionChack(JsonParser.ToString(jData["minClientVersion"]),clientVersion))
        {
            string title = "업데이트";
            string message = "신규버전이 업데이트 됬습니다. 다운받아주세요.";
            UIPopupManager.ShowYesNoPopup(title, message, UpdateCheck);
        }
#endif
        yield return null;
    }
    
    // 최소 클라이언트 버전 체크 (해당 버전보다 스토어에서 최신 버전을 받게 한다.)
    bool VersionChack(string minClientVersion,string currentClientVersion)
    {
        bool result = false;

        string[] min = minClientVersion.Split('.');
        string[] current = currentClientVersion.Split('.');

        for (int i = 0; i < min.Length; i++)
        {
            if (int.Parse(min[i]) > int.Parse(current[i]))
            {
                result = true;
                break;
            }
        }
        return result;
    }

   
    void UpdateCheck(string title)
    {
        if(title == "yes")
        {
            if(!string.IsNullOrEmpty(storeURL))
            {
                WWW www = new WWW(storeURL);// 구글 스토어로 이동
            }
        }
        else
        {
            ClientExit();
        }
    }

    void ClientExit()
    {
        Application.Quit();
        //Debug.Log("게임 종료");
    }

    IEnumerator UpdateAssetBundles()
    {
        yield return null;

        while (AssetLoader.Instance.isInitialized == false)
            yield return null;
        
        List<AssetLoader.AssetBundleHash> assetBundleHashes = AssetLoader.Instance.assetBundleHashes;

        List<AssetLoader.AssetBundleHash> assetBundlesToDownload = new List<AssetLoader.AssetBundleHash>();

        //다운로드 필요한 어셋만 따로 취합
        int count = assetBundleHashes.Count;
        for (int i = 0; i < count; i++)
        {
            // 캐싱 체크.. 
            bool isCached = Caching.IsVersionCached(assetBundleHashes[i].assetBundle, assetBundleHashes[i].hash);
            
            if (!isCached)
                assetBundlesToDownload.Add(assetBundleHashes[i]);
        }


        //다운로드 진행 상황 보여주기. 다운로드 할 것 없으면 그냥 패스
        if (assetBundlesToDownload.Count < 1)
            yield break;

        canDownload = false;
        
        string title = "업데이트";
        string message =assetBundlesToDownload.Count + "개의 추가 컨텐츠\n다운로드가 필요합니다.";
        UIPopupManager.ShowYesNoPopup(title, message, AssetDownLoadCheck);

        while (!canDownload)
            yield return null;


        assetBundleDownLoadBar.SetActive(true);
        messagePanel.SetActive(true);

        StartCoroutine("messageCoroutine");

        string msg = "";
        int cur = 1;
        int percent = 0;
        for (int i = 0; i < assetBundlesToDownload.Count; i++)
        {
            yield return StartCoroutine(AssetLoader.Instance.DownLoadAsset(assetBundlesToDownload[i].assetBundle, assetBundlesToDownload[i].hash));

            cur = i + 1;
            percent = (int)((cur * 100) / assetBundlesToDownload.Count);
            msg = "Download data " + percent + "% (" + cur + "/" + assetBundlesToDownload.Count + ")";
            Debug.Log(msg);
            percentText.text = msg;
            downloadProgressBar.sizeDelta = new Vector2(downloadProgressBarOrigalWidth * ((float)cur / (float)assetBundlesToDownload.Count), downloadProgressBar.sizeDelta.y);

            yield return null;
        }
        assetBundleDownLoadBar.SetActive(false);
        
    }
   

    void AssetDownLoadCheck(string result)
    {
        if(result == "yes")
        {
            //Debug.Log("에셋번들 다운받기로 함 (다운로드 진행)");
            canDownload = true;
        }
        else
        {
            //Debug.Log("에셋번들 다운받지 않음 밖으로 나감...");
            ClientExit();
        }
    }

    // =============== 약관 관련 ===================
    public void OnClickAgreeNecessary()
    {
        if (agreeNecessaryToggle.isOn)
            isAgreeForUseNecessary = true;
        else
            isAgreeForUseNecessary = false;

        //Debug.Log(isAgreeForUseNecessary + "필수");
    }

    public void OnClickAgreeSelection()
    {
        if (agreeSelectionToggle.isOn)
            isAgreeFroUseSelection = true;
        else
            isAgreeFroUseSelection = false;

        //Debug.Log(isAgreeFroUseSelection + "선택");
    }

    public void OnClickAgreeForUse()
    {
        if (isAgreeForUseNecessary && isAgreeFroUseSelection)
        {
            PlayerPrefs.SetString("agreement", "AgreeAll");

            agreementPanel.SetActive(false);
            agreeAllForUse = true;
        }
        else
        {
            UIPopupManager.ShowOKPopup("약관동의", "약관에 모두 동의하셔야 진행할 수 있습니다.", null);
        }
    }

    IEnumerator coroutineAgree()
    {
        bool isChack = false;
#if !UNITY_EDITOR
        if(Application.systemLanguage == SystemLanguage.Korean)
        {
            isChack = true;
        }
#endif
#if UNITY_EDITOR
        isChack = true;
#endif

        if (isChack)
        {
            // 약관 관련
            string key = "agreement";
            if (!PlayerPrefs.HasKey(key))
            {
                agreementPanel.SetActive(!agreeAllForUse);

                while (!agreeAllForUse)
                    yield return null;
            }
        }
        yield break;

    }

    // =========================================

    IEnumerator messageCoroutine()
    {
        WaitForSeconds time = new WaitForSeconds(2);
        while (messageText != null)
        {

            messageText.text = assetBundleDownMessage[UnityEngine.Random.Range(0, assetBundleDownMessage.Count)];
            yield return time;
            yield return null;
        }
    }

}