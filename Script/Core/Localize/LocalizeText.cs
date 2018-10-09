using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class LocalizeText : MonoBehaviour {

    /// <summary> 로컬라이징 데이타에 있는 id </summary>
    public string id;

    //LanguageType currentLanguage;

    Text localizedText;
    
    bool isSetText = false;

    bool isInitialized = false;

    //void Start ()
    //   {
    //       Init();
    //   }

    void Init()
    {
        if (!GameDataManager.Instance)
            return;

        localizedText = GetComponent<Text>();

        //currentLanguage = LocalizationManager.language;        

        if (LocalizationManager.Instance == false)
            return;

        LocalizationManager.Instance.onChangeLocalizationData += OnChangeLocalizationData;

        isInitialized = true;
    }

    void OnDestroy()
    {
        if (LocalizationManager.Instance == false)
            return;

        LocalizationManager.Instance.onChangeLocalizationData -= OnChangeLocalizationData;
    }

    void OnEnable()
    {
        if (!isInitialized)
            Init();

        if (!isSetText)
            SetText();
    }

    void OnChangeLocalizationData()
    {
        //Debug.LogWarning("OnChangeLocalizationData");

        SetText();

        //currentLanguage = LocalizationManager.language;
    }

    void SetText()
    {
        if (LocalizationManager.Instance == false)
            return;

        if (string.IsNullOrEmpty(id))
        {
            //#if UNITY_EDITOR
            ShowDebugMessage("id 없음");
            //#endif
            return;
        }

        //텍스트 얻어오기
        string text = "";
        

        text = LocalizationManager.GetText(id);

        if (string.IsNullOrEmpty(text))
            //#if UNITY_EDITOR
            ShowDebugMessage("설정된 id로 문자를 찾을 수 없거나 공백임. id가 정확한지 확인할 것");
        //#endif


        //UIInput이 있을 경우
        if (text != null)
        {
            localizedText.text = text;
        }

        isSetText = true;
    }

    void ShowDebugMessage(string message)
    {

        string path = transform.name;
        Transform t = transform;
        while (t.parent != null)
        {
            path = t.parent.name + "/" + path;
            t = t.parent;
        }

        Debug.LogWarning(path + " : " + message);
    }

}

