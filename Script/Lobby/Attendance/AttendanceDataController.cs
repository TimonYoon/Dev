//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using LitJson;
//using System.Linq;


///// <summary> 출석 관련 데이터를 서버에서 받아 저장하고 관리하는 클래스</summary>
//public class AttendanceDataController : MonoBehaviour
//{
//    public static AttendanceDataController Instance;
//    /// <summary> 출석 관련 데이터 서버에 받아 저장하는 리스트 (서버 + 클라) </summary>
//    //public List<AttendanceData> attendanceDataList { get; private set; }    

//    /// <summary> 출석 - 보상 1번 필터 데이터 리스트</summary>
//    public List<AttendanceFilterData> attendanceFilterOneDataList { get; private set; }
//    /// <summary> 출석 - 보상 2번 필터 데이터 리스트</summary>
//    public List<AttendanceFilterData> attendanceFilterTwoDataList { get; private set; }


//    /// <summary> 출석 보드 첫 번째 (7일 - 일일출석) </summary>
//    public List<AttendanceData> attendanceBoardOneDataList { get; private set; }

//    /// <summary> 출석 보드 네 번째 (3일- 연속출석) </summary>
//    public List<AttendanceData> attendanceBoardTwoDataList { get; private set; }


//    public void Awake()
//    {
//        Instance = this;
//        WebServerConnectManager.onWebServerResult += OnWebServerResult;

//    }

//    private void Start()
//    {
//        // 서버 통신함 
//        StartCoroutine(InitAttendanceDataCoroutine());
//    }
//    public delegate void OnAttendance();
//    /// <summary> 출석했을 때 </summary>
//    public OnAttendance onAttendance;

//    /// <summary> 출석했는가? </summary>
//    public bool isAttendance { get; private set; }

//    JsonData attendanceBoardGameDataJson = null;
//    IEnumerator InitAttendanceDataCoroutine()
//    {
//        string bundle = "json";
//        string assetName = "AttendanceBoardTable";

//        yield return StartCoroutine(AssetLoader.LoadJsonData(bundle, assetName, x => attendanceBoardGameDataJson = x));

//        string result = "";
//        string php = "Attendance.php";
//        WWWForm form = new WWWForm();

//        form.AddField("userID", User.Instance.userID);
//        form.AddField("type", (int)AttendancePHPtype.Reading);

//        yield return StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(php, form, x => result = x));

//        if (string.IsNullOrEmpty(result))
//        {
//            yield break;
//        }

//        if (result == "add")
//        {
//            Debug.Log("출석했다~~~");

//            if (onAttendance != null)
//                onAttendance();//어떤 출석인지? 출석 보드 아이디.

//        }
//    }

//    public delegate void OnInitData();
//    /// <summary> 데이터 준비 됨 콜백 </summary>
//    public OnInitData onInitData;

//    /// <summary> 출석체크 정보 리스트 </summary>
//    public List<AttendanceData> attendanceDataList { get; private set; }
    
    

//    /// <summary> 데이터 초기화 여부 </summary>
//    public bool isInitialized { get; private set; }
//    /// <summary> 웹서버에서 콜백받음 </summary>
//    void OnWebServerResult(Dictionary<string, object> resultDataDic)
//    {
//        if (resultDataDic.ContainsKey("attendance"))
//        {
//            attendanceDataList = new List<AttendanceData>();
//            JsonReader jsonReader = new JsonReader(JsonMapper.ToJson(resultDataDic["attendance"]));
//            JsonData jsonData = JsonMapper.ToObject(jsonReader);

//            for (int i = 0; i < jsonData.Count; i++)
//            {
//                string serverAttendanceBoardID = JsonParser.ToString(jsonData[i]["attendanceBoardID"]);
//                for (int j = 0; j < attendanceBoardGameDataJson.Count; j++)
//                {
//                    string clientAttendanceBoardID = JsonParser.ToString(attendanceBoardGameDataJson[j]["id"]);
//                    if (clientAttendanceBoardID == serverAttendanceBoardID)
//                    {
//                        AttendanceData attendanceData = new AttendanceData(jsonData[i], attendanceBoardGameDataJson[j]);
//                        attendanceDataList.Add(attendanceData);
//                    }
//                }
//            }
//            isInitialized = true;
//        }
//    }

    
    
//}
