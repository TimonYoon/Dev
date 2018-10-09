using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using System;

public class MailDataController : MonoBehaviour
{

    /// <summary> 해당 유저의 메일 데이터 리스트 </summary>
    public static List<MailData> mailDataList { get; private set; }

    public delegate void MailDataInitCallback(List<MailData> mailDataList);
    /// <summary> 메일 데이터 초기화 완료 콜백 </summary>
    public static MailDataInitCallback onMailDataInitCallback;

    private void OnEnable()
    {
        //UIMail.onResetMailListCallback += MailDataInit;
        //UIMailSlot.onClickReceiveButton += ReceiveItme;
    }
    private void OnDisable()
    {
        //UIMail.onResetMailListCallback -= MailDataInit;
        //UIMailSlot.onClickReceiveButton -= ReceiveItme;
    }

    public bool isInitialized = false;
    private void Start()
    {
        MailDataInit();
        isInitialized = true;
    }

    void MailDataInit()
    {
        StartCoroutine(MailDataInitCoroutine());
    }

    /// <summary> 메일 데이터 초기화 </summary>
    IEnumerator MailDataInitCoroutine()
    {
        WWWForm form = new WWWForm();
        form.AddField("userID", PlayerPrefs.GetString("userID"), System.Text.Encoding.UTF8);
        form.AddField("type", 1);
        string php = "Mail.php";
        string result = "";
        yield return StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(php, form, x => result = x));
        //JsonData jsonData = null;
        ////mailDataList = new List<MailData>();
        //if (!string.IsNullOrEmpty(result))
        //{
        //    jsonData = ParseCheckDodge(result);
        //}
        //else
        //{
        //    // 읽을 메일이 없음
        //    if (onMailDataInitCallback != null)
        //        onMailDataInitCallback(mailDataList);
        //    yield break;
        //}

        //yield break;

        //for (int i = 0; i < jsonData.Count; i++)
        //{
        //    MailData data = new MailData();
        //    data.mailID = JsonParser.ToString(jsonData[i]["id"]);
        //    data.sender = JsonParser.ToString(jsonData[i]["sender"]);
        //    data.title = JsonParser.ToString(jsonData[i]["title"]);
        //    data.message = JsonParser.ToString(jsonData[i]["message"]);
        //    data.itemID = JsonParser.ToString(jsonData[i]["itemID"]);
        //    data.itemAmount = JsonParser.ToString(jsonData[i]["itemAmount"]);
        //    data.recievedTime = JsonParser.ToString(jsonData[i]["recievedTime"]);
        //    data.lifeTime = JsonParser.ToString(jsonData[i]["lifeTime"]);
        //    /*
        //     * 남은 시간계산 부분 남음 넣어야함
        //     */

        //    mailDataList.Add(data);
        //}
        //if (onMailDataInitCallback != null)
        //    onMailDataInitCallback(mailDataList);
    }

    /// <summary> 메일 첨부 아이템 수령 버튼을 눌렀을 때 </summary>
    void ReceiveItme(UIMailSlot mailSlot)//, Action<bool> isResult)
    { 
        bool result = false;
        // mailDataList.FindAll(x => x.mailID == mailSlot.mailID);
        StartCoroutine(MailReceiveItemCoroutine(mailSlot.mailID, x => result = x));
        //isResult(result);
        Debug.Log(result);
    }

    IEnumerator MailReceiveItemCoroutine(string mailID, Action<bool> isResult)
    {
        WWWForm form = new WWWForm();
        form.AddField("userID", PlayerPrefs.GetString("userID"), System.Text.Encoding.UTF8);
        form.AddField("type", 3);
        form.AddField("id", mailID);

        string php = "Mail.php";
        string result = "";
        yield return StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(php, form, x => result = x));
        JsonData jsonData = null;
        bool isReceiveItemSuccess = false;
        if (!string.IsNullOrEmpty(result))
        {
            isReceiveItemSuccess = true;
            jsonData = ParseCheckDodge(result);
            MailDataInit();
        }
        if(JsonParser.ToString(jsonData["itemType"]) != "draw")
        {
            UIPopupManager.ShowOKPopup("획득아이템", "받은 첨부 아이템 : " + JsonParser.ToString(jsonData["itemID"]) + "/" + JsonParser.ToString(jsonData["itemAmount"]), null);
        }
        
        //Debug.Log("받은 첨부 아이템 : " + JsonParser.ToString(jsonData["itemID"]) + "/" + JsonParser.ToString(jsonData["itemAmount"]));
        isResult(isReceiveItemSuccess);

    }

    //IEnumerator AllMailReceiveItemCoroutine()
    //{

    //}



    JsonData ParseCheckDodge(string wwwString)
    {
        if (string.IsNullOrEmpty(wwwString))
            return null;

        //JsonParser jsonParser = new JsonParser();
        wwwString = JsonParser.Decode(wwwString);

        JsonReader jReader = new JsonReader(wwwString);
        JsonData jData = JsonMapper.ToObject(jReader);
        return jData;
    }


}
