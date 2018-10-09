using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIAttendanceBoard : MonoBehaviour {

    public string boardID;

    [SerializeField]
    GameObject attendanceBoardPanel;

    [SerializeField]
    List<UIAttendanceSlot> attendanceSlotList = new List<UIAttendanceSlot>();


    //전체 출석 완료시 띄우는 텍스트 교체를 위한 레퍼런스
    [SerializeField]
    GameObject textDiscription;

    [SerializeField]
    GameObject textComplete;

    [SerializeField]
    GridLayoutGroup attendanceScrollViewContent;

    [SerializeField]
    ScrollRect attendanceScrollRect;

    [SerializeField]
    RectTransform attendanceViewPortRect;

    RectTransform attendanceContentRect;

    AttendanceData attendanceData;

    public bool isOpen = false;

    //슬롯의 보상 체커가 켜져있다면 꺼준다
    private void OnEnable()
    {
        isOpen = true;

        if (attendanceScrollViewContent != null)
            SizeControl();

        if(boardID.Contains("attendanceBoard4") == false)
        {
            attendanceData = AttendanceManager.Instance.attendanceData;
        }
        else
        {
            attendanceData = AttendanceManager.Instance.contAttendanceData;
        }
        

        for (int i = 0; i < attendanceSlotList.Count; i++)
        {
            attendanceSlotList[i].InitSlot(false);
        }
    }

    //출석한 만큼 켜주기 위한 메소드
    public void AttendanceSlotInit()
    {
        for (int i = 0; i < attendanceData.attendanceCount; i++)
        {
            attendanceSlotList[i].InitSlot(true);
        }

        if (textDiscription != null && attendanceSlotList.Count == attendanceData.attendanceCount)
        {
            ActiveCompleteText();
        }
    }

    //가장 마지막 출석 체커의 애니메이션을 보여준다
    public void Show()
    {
        
        attendanceSlotList[attendanceData.attendanceCount - 1].ShowComplete();
        
    }

    //모든 출석이 완료됐다면 완료 텍스트를 켜준다(설명 텍스트는 꺼준다)
    void ActiveCompleteText()
    {
        textDiscription.SetActive(false);
        textComplete.SetActive(true);
    }

    public void OnClickHideButton()
    {
        isOpen = false;
        UIAttendance.Instance.CloseOrHide(boardID);
    }

    public void OnClickSkipButton()
    {
        PlayerPrefs.SetString("attendanceSkip", "skip");
        isOpen = false;
        UIAttendance.Instance.Close();
    }


    void SizeControl()
    {
        if (attendanceContentRect == null)
            attendanceContentRect = attendanceScrollViewContent.GetComponent<RectTransform>();
        
        // 전투영웅
        double count = attendanceScrollViewContent.transform.childCount;

        if (count > 7)
            count /= 5;
        
        int quotient = (int)System.Math.Ceiling(count);

        float sizeDeltaY = (attendanceScrollViewContent.cellSize.y + attendanceScrollViewContent.spacing.y) * (quotient);

        attendanceContentRect.sizeDelta = new Vector2(attendanceContentRect.sizeDelta.x, sizeDeltaY);

        if (boardID == "attendanceBoard2" && AttendanceManager.Instance.attendanceData.attendanceCount > 4)
            attendanceScrollRect.normalizedPosition = new Vector2(attendanceScrollRect.normalizedPosition.x, -1f);
    }
    
}
