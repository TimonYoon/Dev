using UnityEngine;
using System.Collections;
using LitJson;
using CodeStage.AntiCheat.ObscuredTypes;

public class MailData{

    /*
     * 메일이 생성 되면 해당 메일의 id 는 db에서 지정한 고유값을 저장한다.
     * 
     * 해당 고유값을 다시 서버에 보내서 해당 유저가 해당 고유값과 일치한 메일을 소지하고 있다면
     * 
     * 아이템을 지급 아니라면 오류 메시지를 보낸다.
     * 
     * $array = array(0 => "a", 1 => "b", 2 => "c");
     * array_splice($array, 1, 1);
     * 
     * 위와 같은 형식으로 삭제된 메일의 고유값을 배열에서 제거할 수 있다.
     * 
     * 
     */


    public MailData(JsonData jsonData)
    {
        mailID = JsonParser.ToString(jsonData["id"]);
        sender = JsonParser.ToString(jsonData["sender"]);
        title = JsonParser.ToString(jsonData["title"]);
        message = JsonParser.ToString(jsonData["message"]);
        itemType = JsonParser.ToString(jsonData["itemType"]);
        itemID = JsonParser.ToString(jsonData["itemID"]);
        itemAmount = JsonParser.ToString(jsonData["itemAmount"]);
        recievedTime = JsonParser.ToString(jsonData["recievedTime"]);
        lifeTime = JsonParser.ToString(jsonData["lifeTime"]);
    }
    /// <summary> 메일고유 ID </summary>
    public ObscuredString mailID { get; private set; }
    /// <summary> 보낸사람 </summary>
    public string sender { get; private set; }
    /// <summary> 메일 제목 </summary>
    public string title { get; private set; }
    /// <summary> 메일 내용 </summary>
    public string message { get; private set; }
    /// <summary> 첨부 아이템 Type </summary>
    public ObscuredString itemType { get; private set; }
    /// <summary> 첨부 아이템 ID </summary>
    public ObscuredString itemID { get; private set; }
    /// <summary> 첨부 아이템 수량 </summary>
    public ObscuredString itemAmount { get; private set; }
    /// <summary> 메일 수신 시간 </summary>
    public string recievedTime { get; private set; }
    /// <summary> 메일 보관 시간 </summary>
    public string lifeTime { get; private set; }
}

