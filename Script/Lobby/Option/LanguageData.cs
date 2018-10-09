using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary> 언어 데이터 </summary>
public class LanguageData {

    /// <summary> 차후 json 데이터 받아서 초기화 할 예정</summary>
    public LanguageData(LanguageType type, string _languageName)
    {
        languageType = type;
        languageName = _languageName;
    }

    public LanguageType languageType { get; private set; }
    public string languageName { get; private set; }
	
}
