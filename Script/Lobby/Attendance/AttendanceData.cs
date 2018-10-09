using UnityEngine;
using System.Collections;
using LitJson;
using CodeStage.AntiCheat.ObscuredTypes;
/// <summary> 출석 관련 데이터 1. 유저데이터/ 2.보드 테이블</summary>
public class AttendanceData
{
    public AttendanceData(JsonData serverJsonData)//, JsonData clientJsonData)
    {
        id = JsonParser.ToString(serverJsonData["id"]);
        attendanceBoardID = JsonParser.ToString(serverJsonData["attendanceBoardID"]);
        attendanceCount = JsonParser.ToInt(serverJsonData["attendanceCount"]);

    }

    // 유저 데이터 - 출석활동
    /// <summary> 고유 값</summary>
    public ObscuredString id { get; private set; }
    /// <summary> 출석 보드 ID </summary>
    public ObscuredString attendanceBoardID { get; private set; }
    /// <summary> 출석 수</summary>
    public ObscuredInt attendanceCount { get; private set; }

}
/// <summary> 출석 관련 필터 데이터 </summary>
public class AttendanceFilterData
{
    // 보상 (필터) 테이블 cf) id는 클라에서 필요 없음 - 일단 넣어줌으로 변경
    /// <summary> 필터 고유 ID</summary>
    public string filterID;
    /// <summary> 필터 종류</summary>
    public string filterCategory;
    /// <summary> 필터 출석 일수 </summary>
    public int filterAttendanceCount;
    /// <summary> 필터 보상 타입</summary>
    public string rewardType;
    /// <summary> 필터 보상 이미지 이름</summary>
    public string rewardImageName;
    /// <summary> 필터 보상 수량 </summary>
    public int rewardAmount;
}