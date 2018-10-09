using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using CodeStage.AntiCheat.ObscuredTypes;


/// <summary> 환경설정 옵션에 관련된 매니져</summary>
public class OptionManager : MonoBehaviour
{
    public static OptionManager Instance;

    /// <summary> boostSpeed 가 변경될 때 발생 </summary>
    static public SimpleDelegate onChangedBoostSpeed;

    /// <summary> 전투 중 음식 자동 먹기 여부 </summary>
    static public bool isAutoEatBattleFood
    {
        get { return _isAutoEatBattleFood; }
        set
        {
            _isAutoEatBattleFood = value;

            PlayerPrefs.SetString("isAutoEatBattleFood", value.ToString());
        }
    }
    static bool _isAutoEatBattleFood = false;

    void Awake()
    {
        Instance = this;

        if (PlayerPrefs.HasKey("isAutoEatBattleFood"))
            bool.TryParse(PlayerPrefs.GetString("isAutoEatBattleFood"), out _isAutoEatBattleFood);
    }

    private void Start()
    {
        InitOption();
    }
    

    const float constBoostDoubleSpeedCoolTime = 900f;
    const float constBoostTripleSpeedCoolTime = 1800f;
    

    public static bool isInitialized = false;

    void InitOption()
    {
        //string key = "Language";
        if (LocalizationManager.Instance)
            language = (int)LocalizationManager.language;

        string key = "BGM";
        if (PlayerPrefs.HasKey(key))
            bool.TryParse(PlayerPrefs.GetString(key), out _isOnBGM);

        key = "SE";
        if (PlayerPrefs.HasKey(key))
            bool.TryParse(PlayerPrefs.GetString(key), out _isOnSE);

        key = "Vibration";
        if (PlayerPrefs.HasKey(key))
            bool.TryParse(PlayerPrefs.GetString(key), out _isOnVibration);

        key = "DE";
        if (PlayerPrefs.HasKey(key))
            bool.TryParse(PlayerPrefs.GetString(key), out _isOnDamageEffect);

        key = "HPBar";
        if (PlayerPrefs.HasKey(key))
            bool.TryParse(PlayerPrefs.GetString(key), out _isOnHPBar);

        key = "boostSpeed";
        if (ObscuredPrefs.HasKey(key))
            boostSpeed = ObscuredPrefs.GetFloat(key);

        //무스트 시작 시간
        key = "boostStartTime";
        if (ObscuredPrefs.HasKey(key))
        {
            string s = ObscuredPrefs.GetString(key);
            if (!string.IsNullOrEmpty(s))
            {
                DateTime.TryParse(s, out _boostStartTime);
                Debug.Log("부스트 적용 시작 시간 : " + boostStartTime);
            }
        }
        
        key = "boostRemainTime";
        if (ObscuredPrefs.HasKey(key))
            boostRemainTime = ObscuredPrefs.GetFloat(key);
        

        //2배속의 경우 버프 시작한 시점으로 부터 15분 유지
        if (boostSpeed == 2f)
        {
            float c = boostRemainTime - (float)(DateTime.Now - boostStartTime).TotalSeconds;
            if (c < 0f)
                c = 0f;
            
            Instance.StartCoroutine(ApplyBoostCoolTime(c));
            
        }
        else if(boostSpeed >= 3f)
        {
            //3배속(루비쓴부스터)의 경우 저장된 남은 시간으로 계산
            if (boostRemainTime > 0f)
                Instance.StartCoroutine(ApplyBoostCoolTime(boostRemainTime));
        }

        key = "lightEffect";
        if (PlayerPrefs.HasKey(key))
            bool.TryParse(PlayerPrefs.GetString(key), out _lightEffect);

        //쉐이더지원 여부
        vertectLitShader = Shader.Find("Spine/Sprite/Vertex Lit");
        //defaultShader = Shader.Find("Sprites/Default");
        defaultShader = Shader.Find("Spine/Sprite/Unlit");

        if (!vertectLitShader.isSupported)
            lightEffect = false;

        string gpuName = SystemInfo.graphicsDeviceName.ToLower();
        if (gpuName.Contains("adreno") && gpuName.Contains("418"))
            lightEffect = false;

        isInitialized = true;
    }

    static public Shader vertectLitShader;
    static public Shader defaultShader;

    static bool _lightEffect = true;
    static public bool lightEffect
    {
        get { return _lightEffect; }
        set
        {
            bool isChanged = _lightEffect != value;

            _lightEffect = value;

            PlayerPrefs.SetString("lightEffect", value.ToString());
            PlayerPrefs.Save();

            if (isChanged && onChangedLightEffect != null)
            {
                onChangedLightEffect();
            }
        }
    }

    ObscuredFloat _boostSpeed = 1f;
    /// <summary> 광고 보기, 버프 상품 구매 등에 의해 증가한 게임 속도. </summary>
    static public ObscuredFloat boostSpeed
    {
        get { return Instance._boostSpeed; }
        set
        {
            bool isChange = Instance._boostSpeed != value;

            Instance._boostSpeed = value;

            Time.timeScale = boostSpeed;

            //부스트 속도 변경 됨 콜백 날림
            if (onChangedBoostSpeed != null)
                onChangedBoostSpeed();
        }
    }
    
    DateTime _boostStartTime = DateTime.MinValue;
    static DateTime boostStartTime
    {
        get { return Instance._boostStartTime; }
        set { Instance._boostStartTime = value; }
    }

    ObscuredFloat _boostRemainTime = 0f;
    /// <summary> 부스트 효과 남은 시간 </summary>
    static public ObscuredFloat boostRemainTime
    {
        get { return Instance._boostRemainTime; }
        private set
        {
            Instance._boostRemainTime = value;

            if(value <= 0f)
                Time.timeScale = 1f;
        }
    }

 
    /// <summary> 광고 보기, 버프 구매 할 때 증가. 광고보기 부스트는 15분, 루비 상품은 30분 </summary> <param name="boostMode"></param> <param name="coolTime"></param>
    static public void ApplyBoost(float speed, float coolTime = constBoostDoubleSpeedCoolTime)
    {
        

        if(speed == 3f)
        {
            coolTime = constBoostTripleSpeedCoolTime;
        }

        //속도에 따라서
        if (boostSpeed == speed)
        {
            boostRemainTime = boostRemainTime + coolTime;
            boostStartTime = DateTime.Now;
        }
        else if(boostSpeed < speed && boostSpeed > 1f)
        {
            ObscuredPrefs.SetFloat("afterBoostSpeed", boostSpeed);
            ObscuredPrefs.SetFloat("afterBoostRemainTime", boostRemainTime);

            boostStartTime = DateTime.Now;
            boostRemainTime = coolTime;
            boostSpeed = speed;
        }
        else if(boostSpeed > speed && speed > 1f)
        {
            UIPopupManager.ShowOKPopup("부스트 저장", "현재보다 낮은 부스트는 현재 부스트가 끝나고 적용됩니다\n(이미 저장된 부스트가 있다면 추가로 저장됩니다)", null);

            if (ObscuredPrefs.HasKey("afterBoostSpeed"))
            {
                float tempRemainTime = ObscuredPrefs.GetFloat("afterBoostRemainTime");
                tempRemainTime += coolTime;
                ObscuredPrefs.SetFloat("afterBoostRemainTime", tempRemainTime);
            }
            else
            {
                ObscuredPrefs.SetFloat("afterBoostSpeed", speed);
                ObscuredPrefs.SetFloat("afterBoostRemainTime", coolTime);
            }

            return;
        }
        else
        {
            boostStartTime = DateTime.Now;
            boostRemainTime = coolTime;
            boostSpeed = speed;
        }


        ObscuredPrefs.SetFloat("boostSpeed", boostSpeed);
        ObscuredPrefs.SetString("boostStartTime", boostStartTime.ToString());
        ObscuredPrefs.SetFloat("boostRemainTime", boostRemainTime);
        ObscuredPrefs.Save();

        //남은 시간 갱신 로직
        if (coroutineBoostCoolTime != null)
        {
            Instance.StopCoroutine(coroutineBoostCoolTime);
            coroutineBoostCoolTime = null;
        }

        coroutineBoostCoolTime = Instance.StartCoroutine(ApplyBoostCoolTime(boostRemainTime));

        if (UIBoostTimer.Instance)
            UIBoostTimer.Instance.StartBoostTimer();

    }

    static Coroutine coroutineBoostCoolTime = null;
    static IEnumerator ApplyBoostCoolTime(float coolTime = constBoostDoubleSpeedCoolTime)
    {
        
        //남은 시간 갱신
        boostRemainTime = coolTime;
        

        float startTime = Time.unscaledTime;
        float lastSaveTime = Time.unscaledTime + 60f;

        float boostTime = coolTime + Time.unscaledTime;
        while (boostRemainTime > 0f)
        {
            boostRemainTime = boostTime - Time.unscaledTime;

            yield return null;

            //5분 간격으로 부스트 남은 시간 저장
            if(Time.unscaledTime > lastSaveTime)
            {
                lastSaveTime = lastSaveTime + 60f;
                ObscuredPrefs.SetFloat("boostSpeed", boostSpeed);
                if (boostSpeed >= 3f)
                    ObscuredPrefs.SetFloat("boostRemainTime", boostRemainTime);
                ObscuredPrefs.Save();
            }
        }

        ObscuredPrefs.DeleteKey("boostStartTime");
        ObscuredPrefs.DeleteKey("boostSpeed");
        ObscuredPrefs.DeleteKey("boostRemainTime");
        ObscuredPrefs.Save();

        
        boostStartTime = DateTime.MinValue;
        boostRemainTime = 0f;
        boostSpeed = 1f;

        if (ObscuredPrefs.HasKey("afterBoostSpeed"))
        {
            boostSpeed = ObscuredPrefs.GetFloat("afterBoostSpeed");
            boostStartTime = DateTime.Now;
            boostRemainTime = ObscuredPrefs.GetFloat("afterBoostRemainTime");

            ObscuredPrefs.DeleteKey("afterBoostSpeed");
            ObscuredPrefs.DeleteKey("afterBoostRemainTime");

            ObscuredPrefs.SetFloat("boostSpeed", boostSpeed);
            ObscuredPrefs.SetString("boostStartTime", boostStartTime.ToString());
            ObscuredPrefs.SetFloat("boostRemainTime", boostRemainTime);
            ObscuredPrefs.Save();

            coroutineBoostCoolTime = Instance.StartCoroutine(ApplyBoostCoolTime(boostRemainTime));

            if (UIBoostTimer.Instance)
                UIBoostTimer.Instance.StartBoostTimer();

            yield break;
        }
        else
        {
            coroutineBoostCoolTime = null;

            yield break;
        }

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            ApplyBoost(2f);
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            ApplyBoost(3f, constBoostTripleSpeedCoolTime);
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            lightEffect = !lightEffect;
        }
    }

    /// <summary> 언어 타입 </summary>
    int _language = 1;
    public int language
    {
        get
        {
            return _language;
        }
        set
        {
            if (value != _language)
            {
                _language = value;

                if (LocalizationManager.Instance)
                    LocalizationManager.language = (LanguageType)value;
            }
        }
    }
    /// <summary> BGM 켜져있는가? </summary>
    public bool isOnBGM { get { return _isOnBGM; } }
    bool _isOnBGM = true;

    /// <summary> 사운드 이펙트 켜져있는가? </summary>
    public bool isOnSE { get { return _isOnSE; } }
    bool _isOnSE = true;

    /// <summary> 진동 켜져있는가? </summary>
    public bool isOnVibration { get { return _isOnVibration; } }
    bool _isOnVibration = true;

    /// <summary> 데미지 이펙트 켜져있는가? </summary>
    public bool isOnDamageEffect { get { return _isOnDamageEffect; } }
    bool _isOnDamageEffect = true;

    /// <summary> 데미지 이펙트 켜져있는가? </summary>
    public bool isOnHPBar { get { return _isOnHPBar; } }
    bool _isOnHPBar = true;


    //public delegate void OnChangedLanguage(LanguageType type);
    //public OnChangedLanguage onChnagedLanguage;

    public delegate void OnChangedBGM();
    public OnChangedBGM onChangedBGM;

    public delegate void OnChangedSE();
    public OnChangedSE onChangedSE;

    public delegate void OnChangedVibration();
    public OnChangedVibration onChangedVibration;

    public delegate void OnChangedDamageEffect();
    public OnChangedDamageEffect onChangedDamageEffect;

    public delegate void OnChangedHPBar();
    public OnChangedHPBar onChangedHPBar;

    static public SimpleDelegate onChangedLightEffect;

    /// <summary> 언어 변경 </summary>
    public void ChangeLanguage(LanguageType type)
    {
        language = (int)type;
        string key = "Language";
        PlayerPrefs.SetInt(key, language);

        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.onChangeLanguage();
    }

    /// <summary> BGM ON/OFF </summary>
    public void ChangeBGM(bool valeu)
    {
        if (_isOnBGM == valeu)
            return;

        _isOnBGM = valeu;

        string key = "BGM";
        PlayerPrefs.SetString(key, isOnBGM.ToString());

        if (onChangedBGM != null)
            onChangedBGM();
    }

    /// <summary> SoundEffect ON/OFF </summary>
    public void ChangeSE(bool valeu)
    {
        if (_isOnSE == valeu)
            return;

        _isOnSE = valeu;
        
        string key = "SE";
        PlayerPrefs.SetString(key, isOnSE.ToString());

        if (onChangedSE != null)
            onChangedSE();
    }

    /// <summary> Vibration ON/OFF </summary>
    public void ChangeVibration(bool value)
    {
        if (_isOnVibration == value)
            return;

        _isOnVibration = value;
        
        string key = "Vibration";
        PlayerPrefs.SetString(key, isOnVibration.ToString());

        if (onChangedVibration != null)
            onChangedVibration();
    }

    /// <summary> DamageEffect ON/OFF </summary>
    public void ChangeDamageEffect(bool value)
    {
        if (_isOnDamageEffect == value)
            return;

        _isOnDamageEffect = value;

        string key = "DE";
        PlayerPrefs.SetString(key, isOnDamageEffect.ToString());

        if (onChangedDamageEffect != null)
            onChangedDamageEffect();
    }

    /// <summary> HP Bar ON/OFF </summary>
    public void ChangeHPBar(bool value)
    {
        if (_isOnHPBar == value)
            return;

        _isOnHPBar = value;

        string key = "HPBar";
        PlayerPrefs.SetString(key, isOnHPBar.ToString());

        if (onChangedHPBar != null)
            onChangedHPBar();
    }

    /// <summary> 메일로 부스트버프 받았을때 </summary>
    public void ApplyBoostByMail(string itemID, float itemAmount)
    {
        if(itemID == "doubleBoost")
        {
            ApplyBoost(2f, itemAmount);
        }
        else if(itemID == "tripleBoost")
        {
            ApplyBoost(3f, itemAmount);
        }
    }


    /// <summary> 클라우드 저장 </summary>
    public void OnClickCloudSave()
    {
        Debug.Log("클라우드 저장...(미구현)");
    }


    /// <summary> 클라우드 불러오기 </summary>
    public void OnClickCloudLoad()
    {
        Debug.Log("클라우드 불러오기...(미구현)");
    }

    /// <summary> 구글 계정 로그아웃 </summary>
    public void OnClickGoogleLogOut()
    {
#if !UNITY_EDITOR
        if(User.Instance)
        {
            if (string.IsNullOrEmpty(User.Instance.googleID))
            {
                GoogleManager.Instance.LogIn();
                StartCoroutine(PlatformConnect(true, LoginType.GoogleConnect));
            }
            else
            {
                GoogleManager.Instance.LogOut();
                StartCoroutine(PlatformConnect(false, LoginType.GoogleConnect));
            }
                
        }
#endif
#if UNITY_EDITOR
        Debug.Log("에디터 에서는 사용 불가");
#endif
    }


    /// <summary> 페이스북 계정 로그아웃 </summary>
    public void OnClickFaceBookLogOut()
    {
#if !UNITY_EDITOR
        if (User.Instance)
        {
            if (string.IsNullOrEmpty(User.Instance.facebookID))
            {
                FacebookManager.Instance.LogIn();
                StartCoroutine(PlatformConnect(true, LoginType.FacebookConnect));
            }
                
            else
            {
                FacebookManager.Instance.LogOut();
                StartCoroutine(PlatformConnect(false, LoginType.FacebookConnect));
            }
                
        }   
#endif
#if UNITY_EDITOR
        Debug.Log("에디터 에서는 사용 불가");
#endif
    }

    IEnumerator PlatformConnect(bool isLogin, LoginType loginType)
    {
        if (isLogin && loginType == LoginType.GoogleConnect)
        {
            while (!GoogleManager.Instance.isInitialized)
                yield return null;
        }

        if (isLogin && loginType == LoginType.FacebookConnect)
        {
            while (!FacebookManager.Instance.isInitialized)
                yield return null;
        }


        yield return StartCoroutine(User.Instance.WWWUserPHPConnect(loginType));
    }


    /// <summary> 완전 계정 소멸!!! </summary>
    public void OnClickDeleteUserID()
    {
        UIPopupManager.ShowYesNoPopup("계정삭제", "게스트는 모든데이터가 삭제됩니다.", OnDeleteUserPopupResult);
        
    }
    void OnDeleteUserPopupResult(string result)
    {
        
        if(result == "yes")
        {
            PlayerPrefs.DeleteAll();
            for (int i = 0; i < Battle.battleGroupList.Count; i++)
            {
                SaveLoadManager.Clear(SaveType.Battle, Battle.battleGroupList[i].battleType);
                
            }
            
            for (int i = 0; i < TerritoryManager.Instance.myPlaceList.Count; i++)
            {
                SaveLoadManager.Clear(SaveType.Place, TerritoryManager.Instance.myPlaceList[i].placeID + User.Instance.userID);
            }

            Caching.ClearCache();

#if UNITY_EDITOR
            Debug.Log("계정 삭제 재시작해주세요.");
#endif

#if !UNITY_EDITOR
            Application.Quit();
#endif
        }
    }





}
