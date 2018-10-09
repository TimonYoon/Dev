using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LitJson;

namespace UserData
{
    //public class AchievementBaseData
    //{
    //    public AchievementBaseData(JsonData jsonData)
    //    {

    //    }
    //}


    /// <summary> 업적을 리스트 생성과 관리를 하는곳 </summary>
    public class AchievementManager : MonoBehaviour
    {
        //static AchievementManager Instance;

        //public delegate void AchievementManagerCallback(List<AchievementData> achievementsDataList);
        ///// <summary> 업적 데이터 초기화 완료 콜백 </summary>
        //public static AchievementManagerCallback onAchievementManagerCallback;

        //public delegate void ChangedAchievementDataCallback(string achievementID, AchievementData achievementsData);
        ///// <summary> 변경된 업적 데이터가 존재할 때 콜백 </summary>
        //public static ChangedAchievementDataCallback onChangedAchievementDataCallback;

        //public delegate void SendAchievementDataCallback();
        ///// <summary> 신규 메일 알림용 콜백 </summary>
        //public static SendAchievementDataCallback onSendAchievementDataCallback;

        ////List<UIAchievementSlot> achievementsSlotList; // 생성된 업적 슬롯들에 접근할 수 있는 부분

        ////데이타 확인용
        //List<AchievementData> _achievementsDataList;

        ///// <summary> 사용자가 수행중인 업적데이타. 처음 서버에서 받아온 Data를 다시 부를때 서버에 접속하지 않고 출력하기 위해 따로 저장 </summary>
        //static public List<AchievementData> achievementsDataList
        //{
        //    get
        //    {
        //        if (!Instance)
        //            return null;

        //        return Instance._achievementsDataList;
        //    }
        //    set
        //    {
        //        if (!Instance)
        //            return;

        //        Instance._achievementsDataList = value;
        //    }
        //}

        //string achievemanetsListText;
        
        //void Awake()
        //{
        //    Instance = this;
        //}

        //public bool isInitialized = false;
        //void Start()
        //{
        //    StartCoroutine(AchievementsDataInitializeCoroutine());
        //    isInitialized = true;
        //}

        ///// <summary>업적 data 초기화 ex) 로비화면에서 업적을 눌렀을 때 작동하면 좋을것 같음  </summary>
        //public static IEnumerator AchievementsDataInitializeCoroutine()
        //{
        //    WWWForm form = new WWWForm();
        //    form.AddField("userID", User.Instance.userID);
        //    form.AddField("type", 1);

        //    string result = "";
        //    string php = "Achievement.php";
        //    yield return Instance.StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(php, form, x => result = x));

        //    // 만일 해당 유저 업적이 서버에 저장된것이 없다면 result가 null일때의 처리를 해주는곳
        //    int jsonCount = 0;
        //    JsonData jData = null;
        //    if (!string.IsNullOrEmpty(result))
        //    {
        //        jData = Instance.ParseCheckDodge(result);
        //        jsonCount = jData.Count;
        //    }

        //    achievementsDataList = new List<AchievementData>();
           

        //    for (int i = 0; i<Instance.achievementData.Count; i++)
        //    {
        //        bool isCheck = false;
        //        //
        //        for (int j = 0; j<jsonCount; j++)
        //        {

        //            if (JsonParser.ToString(achievementData[i]["achievementID"]) == JsonParser.ToString(jData[j]["achievementID"])
        //                && JsonParser.ToString(achievementData[i]["achievementLevel"]) == JsonParser.ToString(jData[j]["achievementLevel"]))
        //            {
        //                AchievementData data = new AchievementData();
        //                data.achievementID = JsonParser.ToString(jData[j]["achievementID"]);
        //                data.achievementLevel = JsonParser.ToString(jData[j]["achievementLevel"]);
        //                data.category = JsonParser.ToString(achievementData[i]["category"]);
        //                data.title = JsonParser.ToString(achievementData[i]["achievementTitle"]);
        //                data.goalSummary = JsonParser.ToString(achievementData[i]["achievementDiscription"]);
        //                data.nowAmount = JsonParser.ToString(jData[j]["nowAmount"]);
        //                data.goalAmount = JsonParser.ToString(achievementData[i]["goalAmount"]);
        //                //data.achievementIcon = JsonParser.ToString(achievementData[i]["achievementIcon"]);
        //                data.rewardID = JsonParser.ToString(achievementData[i]["rewardID"]);
        //                data.rewardAmount = JsonParser.ToString(achievementData[i]["rewardAmount"]);
        //                data.rewardIcon = JsonParser.ToString(achievementData[i]["rewardIcon"]);
        //                data.isDone = JsonParser.ToString(jData[j]["isDone"]);
        //                data.isRewarded = JsonParser.ToString(jData[j]["isRewarded"]);
        //                isCheck = true;
        //                achievementsDataList.Add(data);
        //            }

        //            if (JsonParser.ToString(achievementData[i]["achievementID"]) == JsonParser.ToString(jData[j]["achievementID"]))
        //            {
        //                isCheck = true;
        //            }
        //        }

        //        // 해당 유저가 서버에 저장된 업적 데이터가 없다면 client의 json파일을 읽어 업적 목록을 생성한다.
        //        if (JsonParser.ToString(achievementData[i]["achievementLevel"]) == "1" && !isCheck)
        //        {
        //            AchievementData data = new AchievementData();
        //            data.achievementID = JsonParser.ToString(achievementData[i]["achievementID"]);
        //            data.achievementLevel = JsonParser.ToString(achievementData[i]["achievementLevel"]);
        //            data.category = JsonParser.ToString(achievementData[i]["category"]);
        //            data.title = JsonParser.ToString(achievementData[i]["achievementTitle"]);
        //            data.goalSummary = JsonParser.ToString(achievementData[i]["achievementDiscription"]);
        //            data.nowAmount = "0";
        //            data.goalAmount = JsonParser.ToString(achievementData[i]["goalAmount"]);
        //            //data.achievementIcon = JsonParser.ToString(achievementData[i]["achievementIcon"]);
        //            data.rewardID = JsonParser.ToString(achievementData[i]["rewardID"]);
        //            data.rewardAmount = JsonParser.ToString(achievementData[i]["rewardAmount"]);
        //            data.rewardIcon = JsonParser.ToString(achievementData[i]["rewardIcon"]);
        //            data.isDone = "0";
        //            data.isRewarded = "0";
        //            achievementsDataList.Add(data);
        //        }
        //    }
        //    if (onAchievementManagerCallback != null)
        //        onAchievementManagerCallback(achievementsDataList);
        //}


        //void OnEnable()
        //{
        //    UIAchievementSlot.onRewardButtonClickCallback += AchievementReward;

        //    UIAchievement.onTESTCallback += OnClickAchievementButton;
        //}
        //void OnDisable()
        //{
        //    UIAchievementSlot.onRewardButtonClickCallback -= AchievementReward;

        //    UIAchievement.onTESTCallback -= OnClickAchievementButton;
        //}


        ///// <summary> 업적 증가 테스트용 </summary>
        //public void OnClickAchievementButton(string achievementID, int amonut)
        //{
        //    StartCoroutine(WWWConnectCoroutine(achievementID, amonut));
        //}

        ///// <summary> 업적 DB에 변경된 수치를 Update시키는 코루틴 </summary>
        //IEnumerator WWWConnectCoroutine(string achievementID, int amount)
        //{
        //    WWWForm form = new WWWForm();
        //    form.AddField("userID", PlayerPrefs.GetString("userID"));
        //    form.AddField("achievementID", achievementID);
        //    form.AddField("amount", amount); //증가량 예시임 바꿔야함
        //    form.AddField("type", 2); 
        //    string php = "Achievement.php";
        //    string result = "";
        //    yield return StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(php, form, x => result = x));

        //    JsonData jsonData = null;
        //    if (!string.IsNullOrEmpty(result))
        //    {
        //        jsonData = ParseCheckDodge(result);

        //        // 업적 데이터가 존재하면 해당 업적ID와 업적데이터를 콜백을 보낸다.
        //        if (onChangedAchievementDataCallback != null)
        //            onChangedAchievementDataCallback(achievementID, ConvertToAchievementData(achievementID, jsonData));
        //    }
        //    else
        //    {
        //        Debug.LogError("서버에서 업적올리기 결과값 null입니다. wwwform과 서버DB를 확인해주세요.");
        //    }
        //}

        ///// <summary> 서버에서 온 JsonData를 AchievementData로 변경한다. </summary>
        //AchievementData ConvertToAchievementData(string achievementsID, JsonData jsonData)
        //{
        //    JsonParser JsonParser = new JsonParser();
        //    for (int i = 0; i < achievementData.Count; i++)
        //    {
                
        //        for (int j = 0; j < achievementsDataList.Count; j++)
        //        {

        //            if (achievementsDataList[j].achievementID == achievementsID
        //                && JsonParser.ToString(achievementData[i]["achievementID"]) == JsonParser.ToString(jsonData["achievementID"])
        //                && JsonParser.ToString(achievementData[i]["achievementLevel"]) == JsonParser.ToString(jsonData["achievementLevel"]))
        //            {

        //                achievementsDataList[j].achievementID = JsonParser.ToString(jsonData["achievementID"]);
        //                achievementsDataList[j].achievementLevel = JsonParser.ToString(jsonData["achievementLevel"]);
        //                achievementsDataList[j].category = JsonParser.ToString(achievementData[i]["category"]);
        //                achievementsDataList[j].title = JsonParser.ToString(achievementData[i]["title"]);
        //                achievementsDataList[j].goalSummary = JsonParser.ToString(achievementData[i]["goalSummary"]);
        //                achievementsDataList[j].nowAmount = JsonParser.ToString(jsonData["nowAmount"]);
        //                achievementsDataList[j].goalAmount = JsonParser.ToString(achievementData[i]["goalAmount"]);
        //                achievementsDataList[j].achievementIcon = JsonParser.ToString(achievementData[i]["achievementIcon"]);
        //                achievementsDataList[j].rewardID = JsonParser.ToString(achievementData[i]["rewardID"]);
        //                achievementsDataList[j].rewardAmount = JsonParser.ToString(achievementData[i]["rewardAmount"]);
        //                achievementsDataList[j].rewardIcon = JsonParser.ToString(achievementData[i]["rewardIcon"]);
        //                achievementsDataList[j].isDone = JsonParser.ToString(jsonData["isDone"]);
        //                achievementsDataList[j].isRewarded = JsonParser.ToString(jsonData["isRewarded"]);
        //                return achievementsDataList[j];
        //            }
        //        }
        //    }


        //    return null;
        //}
        

        ///// <summary> 업적 보상 버튼 클릭에 따른 콜백을 받는 부분 </summary>
        //void AchievementReward(UIAchievementSlot achievementSlot)
        //{
        //    StartCoroutine(WWWConnectCoroutine(achievementSlot.achievementID));
        //}
        ///// <summary> 업적이 완료되고 보상버튼을 눌렀을 때 해당 슬롯의 업적DB에서 확인후 보상 데이터를 받아온다. </summary>
        //IEnumerator WWWConnectCoroutine(string achievementID)
        //{
        //    WWWForm form = new WWWForm();
        //    form.AddField("userID", PlayerPrefs.GetString("userID"));
        //    form.AddField("achievementID", achievementID);
        //    form.AddField("type", 3);
        //    string php = "Achievement.php";
        //    string result = "";
        //    yield return StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(php, form, x => result = x));
        //    JsonData jsonData = null;
        //    if (!string.IsNullOrEmpty(result))
        //    {
        //        jsonData = ParseCheckDodge(result);
        //        // 업적 데이터가 존재하면 해당 업적ID와 업적데이터를 콜백을 보낸다.
        //        if (onChangedAchievementDataCallback != null)
        //            onChangedAchievementDataCallback(achievementID, ConvertToAchievementData(achievementID, jsonData));

        //        //AchievementReading(achievementsID);
        //        UIPopupManager.ShowOKPopup("보상완료", "우편으로 보상지급 완료", null);

        //        // 보상 데이터를 다른 곳에서 사용가능하게 콜백을 만들어야함(추후 수정)
        //        if(onSendAchievementDataCallback != null)
        //           onSendAchievementDataCallback();

        //    }
        //    else
        //    {
        //        Debug.LogError("서버에서 업적보상받기 결과값 null입니다. wwwform과 서버DB를 확인해주세요.");
        //    }            
        //}

        //JsonData ParseCheckDodge(string wwwString)
        //{
        //    if (string.IsNullOrEmpty(wwwString))
        //        return null;

            
        //    wwwString = JsonParser.Decode(wwwString);

        //    JsonReader jReader = new JsonReader(wwwString);
        //    JsonData jData = JsonMapper.ToObject(jReader);
        //    return jData;
        //}

    }
}


