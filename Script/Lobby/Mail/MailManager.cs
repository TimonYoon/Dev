using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using UnityEngine.SceneManagement;
using System;
using UserData;
using UnityEngine.UI;
using System.Linq;
public class MailManager : MonoBehaviour {



    /// <summary> 해당 유저의 메일 데이터 리스트 </summary>
    public List<MailData> mailDataList = new List<MailData>();

    ///// <summary> 해당 유저의 메일 데이터 딕셔너리 </summary>
    //public Dictionary<string, MailData> mailDataDic { get; private set; }


    public delegate void MailDataInitCallback(List<MailData> mailDataList);
    public static MailDataInitCallback onInitMailData;

    
    public delegate void NewMailCheckerCallback(AlarmType type, bool check);
    /// <summary> 새로온 메일 알림,해제용 콜백 </summary>
    public NewMailCheckerCallback onNewMailCheckerCallback;
    
    public static MailManager Instance;

    private void Awake()
    {
        Instance = this;
        
    }

    //private void OnEnable()
    //{
    //    WebServerConnectManager.onWebServerResult += OnWebServerResult;

    //}
    //private void OnDisable()
    //{
    //    WebServerConnectManager.onWebServerResult -= OnWebServerResult;
    //}


    void OnWebServerResult(Dictionary<string,object> resultDic)
    {

        if(resultDic.ContainsKey("mail"))
        {
            string text = JsonParser.Decode(JsonMapper.ToJson(resultDic["mail"]));
            JsonReader json = new JsonReader(text);
            JsonData jsonData = JsonMapper.ToObject(json);
            

            for (int i = 0; i < jsonData.Count; i++)
            {
                MailData data = null;
                for (int j = 0; j < mailDataList.Count; j++)
                {
                    if (mailDataList[j].mailID == JsonParser.ToString(jsonData[i]["id"]))
                        data = mailDataList[j];
                }  
                
                if(data == null)
                {
                    data = new MailData(jsonData[i]);

                    mailDataList.Add(data);
                    //mailDataDic.Add(data.mailID, data);
                    

                    //Debug.Log("mail ID : " + data.mailID);
                }                
            }

            if (UpdateAlarm.Instance == null && mailDataList.Count > 0)
                UpdateAlarm.updateMail = true;

            MailReceiveOrNot();

            if (onInitMailData != null)
                onInitMailData(mailDataList);
            

        }
    }

    /// <summary> 우편함 초기화 됬는가 </summary>
    public static bool isInitialized = false;
   

    /// <summary> 메일 데이터 초기화 </summary>
    public static IEnumerator MailDataInitCoroutine()
    {
        if(isInitialized == false)
        {
            while (!WebServerConnectManager.Instance)
                yield return null;
            WebServerConnectManager.onWebServerResult += Instance.OnWebServerResult;
        }

        WWWForm form = new WWWForm();
        form.AddField("userID", PlayerPrefs.GetString("userID"), System.Text.Encoding.UTF8);
        form.AddField("type", 1);
        string php = "Mail.php";
        string result = "";
        yield return Instance.StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(php, form, x => result = x));

        isInitialized = true;
    }



    /// <summary> 메일 첨부 아이템 수령 버튼을 눌렀을 때 </summary>
    public void ReceiveItme(string mailID)//, Action<bool> isResult)
    {
        bool result = false;
        StartCoroutine(MailReceiveItemCoroutine(mailID, x => result = x));

        //Debug.Log(result);
    }
    IEnumerator MailReceiveItemCoroutine(string mailID, Action<bool> isResult)
    {
        WWWForm form = new WWWForm();
        form.AddField("userID", User.Instance.userID);
        form.AddField("type", 3);
        form.AddField("id", mailID);

        string php = "Mail.php";
        string result = "";
        yield return StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(php, form, x => result = x));

        mailDataList.RemoveAt(mailDataList.FindIndex(x => x.mailID == mailID));

        JsonData jsonData = null;
        bool isReceiveItemSuccess = false;
        if (!string.IsNullOrEmpty(result))
        {
            isReceiveItemSuccess = true;
            //jsonData = ParseCheckDodge(result);

            JsonReader jReader = new JsonReader(result);
            JsonData jData = JsonMapper.ToObject(jReader);

            List<string> keys = GameDataManager.moneyBaseDataDic.Keys.ToList();
            bool isMoneyData = false;
            for (int i = 0; i < keys.Count; i++)
            {
                string key = keys[i];
                if (jData.ContainsKey(key))
                {
                    isMoneyData = true;
                }
            }

            if (isMoneyData)
            {
                UIPopupManager.ShowOKPopup("우편 첨부 아이템 받기", "받은 첨부 아이템 : " + MailReceiveMessage(result), null);
            }
            else
            {
                UIPopupManager.ShowOKPopup("우편 첨부 아이템 받기", "아이템 수령 완료", null);
            }
            //Debug.Log("받은 첨부 아이템 : " + JsonParser.ToString(jsonData["itemID"]) + "/" + JsonParser.ToString(jsonData["itemAmount"]));
            isResult(isReceiveItemSuccess);

           
        }
        MailReceiveOrNot();
    }

    /// <summary> 메일 모두 받기 버튼 눌렀을 때 </summary>
    public void AllReceiveItem()
    {
        bool result = false;
        StartCoroutine(AllMailReceiveItemCoroutine(x => result = x));
        Debug.Log("AllmailReceive");
        Debug.Log(result);
    }
    IEnumerator AllMailReceiveItemCoroutine(Action<bool> isResult)
    {
        WWWForm form = new WWWForm();
        form.AddField("userID", User.Instance.userID);
        form.AddField("type", 4);
        

        string php = "Mail.php";
        string result = "";
        yield return StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(php, form, x => result = x));

        mailDataList.RemoveAll(x=> x.itemType == "money");


        JsonData jsonData = null;
        bool isReceiveItemSuccess = false;
        if (!string.IsNullOrEmpty(result))
        {
            isReceiveItemSuccess = true;
            //jsonData = ParseCheckDodge(result);
            //Debug.Log(result);
            UIPopupManager.ShowOKPopup("우편 첨부 아이템 받기", "받은 첨부 아이템 : " + MailReceiveMessage(result), null);
            isResult(isReceiveItemSuccess);
        }

        MailReceiveOrNot();
    }
    string MailReceiveMessage(string result)
    {

        string text = string.Empty;

        if (string.IsNullOrEmpty(result))
            return text;

        JsonReader jReader = new JsonReader(result);
        JsonData jData = JsonMapper.ToObject(jReader);

        List<string> keys = GameDataManager.moneyBaseDataDic.Keys.ToList();

        for (int i = 0; i < keys.Count; i++)
        {
            string key = keys[i];
            if (jData.ContainsKey(key))
            {
                text += "\n";
                text += GameDataManager.moneyBaseDataDic[key].name + " : " + string.Format("{0:#,###}", Convert.ToInt32(JsonParser.ToString(jData[key])));
            }
        }
        
        return text;
    }

    //보상관련 메일수신 확인 테스트용
    public void MailReceiveOrNot()
    {
        if(onNewMailCheckerCallback != null)
        {
            if (mailDataList.Count < 1)
                onNewMailCheckerCallback(AlarmType.Mail, false);
            else
                onNewMailCheckerCallback(AlarmType.Mail, true);

        }
    }


    JsonData ParseCheckDodge(string wwwString)
    {
        if (string.IsNullOrEmpty(wwwString))
            return null;

        wwwString = JsonParser.Decode(wwwString);

        JsonReader jReader = new JsonReader(wwwString);
        JsonData jData = JsonMapper.ToObject(jReader);
        return jData;
    }

}
