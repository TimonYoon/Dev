using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;

public enum webServerResultDataType
{
    Hero,
    Money,
    Building
}

public class WebServerConnectManager : MonoBehaviour {

    static WebServerConnectManager instance;
    public static WebServerConnectManager Instance
    {
        get { return instance; }
    }
    void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this);
    }

    public delegate void OnWebServerResult(Dictionary<string,object> resultDataDic);
    public static OnWebServerResult onWebServerResult;
    

    public static string clientVersion ="";


    // web server address 
    readonly string url = "test/Project_L/Dev_";

    /// <summary> web server connect coroutine </summary>
    public IEnumerator WWWCoroutine(string connectPHP, WWWForm form = null, System.Action<string> resultCallBack = null, System.Action<string> error = null)
    {      
        WWW www = null;
        if (form == null)
        {
            www = new WWW("http://" + url + clientVersion + "/" + connectPHP);
        }
        else
        {
            www = new WWW("http://" + url + clientVersion + "/" + connectPHP,form);
        }
       

        yield return www;

        if (!string.IsNullOrEmpty(www.error))
        {
            Debug.Log("에러 발생 : " + www.error);
            UIPopupManager.ShowOKPopup("에러 발생", "인터넷 연결을 확인해주세요.", null);
            yield break;
        }            

        string wwwText = www.text;

        //// 서버 DB 접속체크
        if (wwwText.ToUpper().Contains("FAIL") || wwwText == "")
        {
            Debug.Log("서버에 접속할 수 없습니다. Config.php를 확인해주세요.");
            UIPopupManager.ShowOKPopup("에러 발생", "서버 연결부를 확인해주세요.", null);
            yield break;
        }

        // 유니코드 체크후 한글로 변경
        string resultData = JsonParser.Decode(wwwText);
       
        
        if (string.IsNullOrEmpty(resultData))
        {
            Debug.Log(connectPHP + "파일을 찾을 수 없습니다.");
            UIPopupManager.ShowOKPopup("에러 발생", "PHP를 찾을 수 없습니다.", null);
            yield break;
        }


        // 딕셔너리로 변경
        Dictionary<string, object> resultDictionary = Mini.MiniJSON.jsonDecode(resultData) as Dictionary<string, object>;


        if (resultDictionary == null || !resultDictionary.ContainsKey("message"))
            Debug.LogError(connectPHP + " error");

        string resultMessage = resultDictionary["message"].ToString();

        /*
         * resultMessage = 0
         * 연결 성공 메시지
         */

        if (resultMessage != "0")
        {
            Debug.Log(connectPHP + "에 접속하였으나 에러가 발생했습니다. #error message : " + resultMessage);
            if (error != null)
                error(resultMessage);
        }
        else
        {

            if (!resultDictionary.ContainsKey("data"))
            {
                string result = null;
                if (resultCallBack != null)
                    resultCallBack(result);
            }
            else
            {
                string result = resultDictionary["data"].GetType().ToString();
                if (result.Contains("Dic") || result.Contains("List"))
                {
                    result = JsonMapper.ToJson(resultDictionary["data"]);
                }
                else
                {
                    result = resultDictionary["data"].ToString();
                }
                if (resultCallBack != null)
                    resultCallBack(result);
            }
            if (onWebServerResult != null)
                onWebServerResult(resultDictionary);
        }
    }
}
