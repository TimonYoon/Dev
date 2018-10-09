using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILanguageSlot : MonoBehaviour {

    //[SerializeField]
    public LanguageType languageType { get; private set; }

    [SerializeField]
    Text languageName;

    [SerializeField]
    Toggle toggle; // 


    public delegate void OnClickLanguage(LanguageType type);
    public OnClickLanguage onClickLanguage;


    /// <summary> 슬롯 초기화 </summary>
    public void InitSlot(LanguageData data,ToggleGroup group)
    {
        languageType = data.languageType;
        languageName.text = data.languageName;
        toggle.group = group;
        if (languageType == LocalizationManager.language)
            toggle.isOn = true;
    }

    /// <summary> 해당 언어 클릭했을 때 </summary>
    public void OnClickLanguageButton()
    {
        if (OptionManager.Instance.language == (int)languageType)
            return;

        if (toggle.isOn)
        {
            if (onClickLanguage != null)
                onClickLanguage(languageType);
        }
    }

    /// <summary> 토글 변경 </summary>
    public void IsOn()
    {
        if (toggle.isOn)
            toggle.isOn = false;
        else if (!toggle.isOn)
            toggle.isOn = true;
    }
}
