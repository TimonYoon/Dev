using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using UnityEngine.SceneManagement;
using CodeStage.AntiCheat.ObscuredTypes;


public enum AttendancePHPtype
{
    Reading = 1,
    // 나중에 없어진다. ** 서버에서 주는 거 읽기만 하면된다.
    Testing = 2,
    // 임의로 만들어 놓은 출석 리셋 버튼 
    Reset = 3,

}
public class AttendanceManager : MonoBehaviour {

    public static AttendanceManager Instance;

    /// <summary>출석체크 판 클라이언트 데이터 json </summary>
    JsonData attendanceBoardClientDataJson = null;

    /// <summary> 출석체크 정보 리스트 </summary>
    //public List<AttendanceData> attendanceDataList { get; private set; }

    public AttendanceData attendanceData { get; private set; }
    public AttendanceData contAttendanceData { get; private set; }
    public List<AttendanceData> attendanceDataList = new List<AttendanceData>();

    /// <summary> 출석했는가? </summary>
    public ObscuredBool isAttendance { get; private set; }

    /// <summary> 데이터 초기화 여부 </summary>
    public static bool isInitialized { get; private set; }

    void Awake()
    {
        Instance = this;

    }
  
    public static IEnumerator Init()
    {
        while (!WebServerConnectManager.Instance)
            yield return null;
        WebServerConnectManager.onWebServerResult += Instance.OnWebServerResult;

        yield return Instance.StartCoroutine(Instance.ServerConnect());

        isInitialized = true;
    }

    /// <summary> 출석 서버 통신 </summary>
    public IEnumerator ServerConnect()
    {


        Instance.isAttendance = false;

        string result = "";
        string php = "Attendance.php";
        WWWForm form = new WWWForm();

        form.AddField("userID", User.Instance.userID);
        form.AddField("type", (int)AttendancePHPtype.Reading);

        yield return Instance.StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(php, form, x => result = x));

        if (string.IsNullOrEmpty(result))
        {
            yield break;
        }

        if (result == "add")
        {

            if(isInitialized)
            {
                yield return StartCoroutine(MailManager.MailDataInitCoroutine());
            }
            else
            {
                UIAttendance.isLogin = true;
            }

            Instance.isAttendance = true;
        }
        yield return null;

       
    }   



    /// <summary> 웹서버에서 콜백받음 </summary>
    void OnWebServerResult(Dictionary<string, object> resultDataDic)
    {
        //if (isInitialized)
        //    return;
        

        if (resultDataDic.ContainsKey("attendance"))
        {
            attendanceDataList.Clear();
            JsonReader jsonReader = new JsonReader(JsonMapper.ToJson(resultDataDic["attendance"]));
            JsonData jsonData = JsonMapper.ToObject(jsonReader);
            
            attendanceData = new AttendanceData(jsonData);
            attendanceDataList.Add(attendanceData);
        }
        if (resultDataDic.ContainsKey("continuousAttendance"))
        {
            JsonReader jsonReader = new JsonReader(JsonMapper.ToJson(resultDataDic["continuousAttendance"]));
            JsonData jsonData = JsonMapper.ToObject(jsonReader);

            
            contAttendanceData = new AttendanceData(jsonData);
            attendanceDataList.Add(contAttendanceData);
        }
    }    

    /// <summary> 씬 띄우기 </summary>
    IEnumerator ShowScene(string assetBundle, string sceneName, bool isAdditive = true)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.isLoaded)
        {
            while (!AssetLoader.Instance)
                yield return null;

            yield return StartCoroutine(AssetLoader.Instance.LoadLevelAsync(assetBundle, sceneName, isAdditive));

        }
        yield break;
    }
    
}
