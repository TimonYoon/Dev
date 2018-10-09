using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

//언어. 주의: json데이타에서 값찾을 때 사용되기 때문에 json데이타에서 사용된 이름과 같아야 함
public enum LanguageType
{
    //Empty,
    korean = 1,     //한국
    english = 2,    //영어
    arabic = 3,     //아랍
    japanese = 4,   //일본
    spanish = 5,    //스페인
    german = 6,     //독일
    chinese = 7,    //중국
    russian = 8,    //러시아
    french = 9,     //프랑스
    dutch = 10,     //네덜란드
    portuguese = 11, //포르투갈
    greek = 12,     //그리스
    turkish = 13,   //터키
    thai = 14,      //태국
    vietnamese = 15, //베트남
    indonesia = 16 // 인도네시아
}


//유니크 패러미터 테이블
[System.Serializable]
public class LocalizationData
{
    public string id;              // 언어 식별자
    public string localizingText;    // 텍스트에 표시할 언어
    
    public string languageName;            // 언어이름(텍스트)
}

//아이템 정보
[System.Serializable]
public class DataLanguage
{
    public List<LocalizationData> localizationDataList;      //국가데이터 테이블 정보

    //테이블 파싱
    public void ParseTable(JsonData jData)
    {
        //초기화
        if (localizationDataList != null)
        {
            localizationDataList.Clear();
            localizationDataList = null;
        }

        //생성
        localizationDataList = new List<LocalizationData>();

        //파싱
        for (int a = 0; a < jData.Count; a++)
        {

            LocalizationData data = new LocalizationData();

            // 언어 이름
            data.id = JsonParser.ToString(jData[a]["id"]);

            // 언어로 표현할 텍스트
            data.localizingText = JsonParser.ToString(jData[a]["localizingText"]);
            

            // 언어 이름(텍스트)
            data.languageName = JsonParser.ToString(jData[a]["languageName"]);

            //입력
            localizationDataList.Add(data);
        }

        //Debug.Log("ParseTable Ok");
    }
}
