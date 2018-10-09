using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using LitJson;

public class LocalizationManager : MonoBehaviour
{

    public static LocalizationManager Instance;
    
    private string missingTextString = "Localized text not found";

    static public LanguageType language;
    
    
    public static Dictionary<string, string> localizingData = new Dictionary<string, string>();
    
    // Use this for initialization
    void Awake()
    {
        Instance = this;

        onChangeLanguage += OnChangeLanguage;

        DontDestroyOnLoad(gameObject);
    }

    /// <summary> 환경 설정에서 언어 설정 바꿨을 때 </summary>
    public SimpleDelegate onChangeLanguage;

    /// <summary> 로컬라이징 데이타 갱신이 완료 되었을 때. 환경설정에서 다른 언어로 선택한 후 확인을 눌러서 언어테이블 초기화가 끝나면 발생함 </summary>
    public SimpleDelegate onChangeLocalizationData;

    static public bool isInitializedPreLocalizingData = false;
    static public bool isLocalizeDataSet = false;

    //IEnumerator Start()
    //{
    //    yield return (StartCoroutine(GetDeviceLanguage()));

    //    yield return (StartCoroutine(coParsingPreLanguageDate(language)));



    //    //어셋 번들 전부 다 다운 로드 되었는지 체크

    //    //yield return (StartCoroutine(coParsingLanguageData(language)));


    //    //로비로 넘어감
    //}

    public static IEnumerator Init()
    {
        yield return (Instance.StartCoroutine(GetDeviceLanguage()));

        yield return (Instance.StartCoroutine(coParsingPreLanguageDate(language)));
    }
    

    static IEnumerator GetDeviceLanguage()
    {

        switch (Application.systemLanguage)
        {
            case SystemLanguage.Korean:
                language = LanguageType.korean;
                break;
            case SystemLanguage.English:
                language = LanguageType.english;
                break; 
            case SystemLanguage.Arabic:
                language = LanguageType.arabic;
                break;
            case SystemLanguage.Japanese:
                language = LanguageType.japanese;
                break;
            case SystemLanguage.Spanish:
                language = LanguageType.spanish;
                break;
            case SystemLanguage.German:
                language = LanguageType.german;
                break;
            case SystemLanguage.Chinese:
                language = LanguageType.chinese;
                break;
            case SystemLanguage.Russian:
                language = LanguageType.russian;
                break;
            case SystemLanguage.French:
                language = LanguageType.french;
                break;
            case SystemLanguage.Dutch:
                language = LanguageType.dutch;
                break;
            case SystemLanguage.Portuguese:
                language = LanguageType.portuguese;
                break;
            case SystemLanguage.Greek:
                language = LanguageType.greek;
                break;
            case SystemLanguage.Turkish:
                language = LanguageType.turkish;
                break;
            case SystemLanguage.Thai:
                language = LanguageType.thai;
                break;
            case SystemLanguage.Vietnamese:
                language = LanguageType.vietnamese;
                break;
            case SystemLanguage.Indonesian:
                language = LanguageType.indonesia;
                break;
        }

#if UNITY_EDITOR
        //유니티에서 맨처음 실행시 기본값
        language = LanguageType.korean;
#endif

        if (PlayerPrefs.HasKey("Language"))
        {
            language = (LanguageType)PlayerPrefs.GetInt("Language");
            if(language == 0)
            {
                language = LanguageType.korean;
                PlayerPrefs.SetInt("Language", (int)language);
            }
        }
        else
        {
            PlayerPrefs.SetInt("Language", (int)language);
        }

        yield break;
    }
    public static IEnumerator coParsingPreLanguageDate(LanguageType selectLanguage)
    {
        TextAsset txt = Resources.Load("PreLocalizingData") as TextAsset;

        if (!txt)
            yield break;

        JsonReader jReader = new JsonReader(txt.text);
        JsonData jData = JsonMapper.ToObject(jReader);

        if (jData == null)
        {
            Debug.LogError("Item base data has not initialized");
            yield break;
        }

        //파싱
        JsonParser jsonParser = new JsonParser();

        string language = selectLanguage.ToString();

        for (int i = 0; i < jData.Count; i++)
        {
            localizingData.Add(JsonParser.ToString(jData[i]["id"]), JsonParser.ToString(jData[i][language]));
        }
        

        isInitializedPreLocalizingData = true;

        //등록된 콜백들 전부 실행. 아마도 대부분 레이블들
        if (Instance.onChangeLocalizationData != null)
            Instance.onChangeLocalizationData();

        yield break;
    }

    public static IEnumerator coParsingLanguageData(LanguageType selectLanguage)
    {
        if(isLocalizeDataSet == true)
        {
            localizingData.Clear();
        }

        while (!AssetLoader.Instance.isInitialized)
            yield return null;

        string bundle = "data/localizing";// "json";
        JsonData json = null;
        string fileName = "LocalizingData";
        yield return (Instance.StartCoroutine(AssetLoader.LoadJsonData(bundle, fileName, x => json = x)));
        
        string language =  selectLanguage.ToString();

        if (json != null)
        {
            for (int i = 0; i < json.Count; i++)
            {
                localizingData.Add(JsonParser.ToString(json[i]["id"]), JsonParser.ToString(json[i][language]));
            }
        }
        else
        {
            Debug.Log("json file is missing!!!");
        }


        isLocalizeDataSet = true;
        

        //등록된 콜백들 전부 실행. 아마도 대부분 레이블들
        if (Instance.onChangeLocalizationData != null)
            Instance.onChangeLocalizationData();

#if UNITY_EDITOR
        Instance.co = null;
#endif

        yield break;
    }

    /// <summary> 환경 설정에서 언어 설정이 바뀌었을 때 </summary>
    void OnChangeLanguage()
    {
        Debug.Log("OnChangeLanguage");

        StartCoroutine(coParsingLanguageData(language));
    }


    /// <summary> 로컬라이징 데이타에서 키값에 해당하는 문자를 반환함 </summary>
    static public string GetText(string key)
    {
        if (localizingData.ContainsKey(key))
            return localizingData[key];
        else
        {

#if UNITY_EDITOR
            Debug.LogError("JsonData의 문자열에 해당 키가 없습니다. 키 : " + key);
#endif
            return key;
        }

    }

    public bool GetIsReady()
    {
        return isLocalizeDataSet;
    }

    Coroutine co;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && co == null)
        {
            Debug.Log("한국어");
            co = StartCoroutine(coParsingLanguageData(LanguageType.korean));
        }
            

        if (Input.GetKeyDown(KeyCode.Alpha2) && co == null)
        {
            Debug.Log("영어");
            co = StartCoroutine(coParsingLanguageData(LanguageType.english));

        }

    }
}