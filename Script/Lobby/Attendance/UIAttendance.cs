using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;



public class UIAttendance : MonoBehaviour
{
    static public UIAttendance Instance;

    [SerializeField]
    Canvas canvas;

    [SerializeField]
    public List<UIAttendanceBoard> attendanceBoardList = new List<UIAttendanceBoard>();

    public static bool isLogin;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        SceneLobby.Instance.OnChangedMenu += OnChagedMenu;
    }


    IEnumerator Start()
    {
        canvas.enabled = false;
        
        if (!isLogin)
        {
            yield return StartCoroutine(AttendanceManager.Instance.ServerConnect());
        }
        else
        {
            isLogin = false;
            if (AttendanceManager.Instance.isAttendance == false)
            {
                Close();
                yield break;
            }
        }

        UIAttendanceBoard attendanceBoard = new UIAttendanceBoard();
        
        for (int i = 0; i < AttendanceManager.Instance.attendanceDataList.Count; i++)
        {

            attendanceBoard = attendanceBoardList.Find(x => x.boardID == AttendanceManager.Instance.attendanceDataList[i].attendanceBoardID);
            if (attendanceBoard != null)
            {
                    
                if (attendanceBoard.boardID.Contains("attendanceBoard4"))
                {
                    if (PlayerPrefs.HasKey("attendanceSkip") && isLogin == true)
                        continue;
                }

                attendanceBoard.gameObject.SetActive(true);
                attendanceBoard.AttendanceSlotInit();
                canvas.enabled = true;
                    
            }
        }


        if (AttendanceManager.Instance.isAttendance == true)
        {
            // 출석 했다면 출석 연출 보여주기

            for (int i = 0; i < AttendanceManager.Instance.attendanceDataList.Count; i++)
            {
                attendanceBoard = attendanceBoardList.Find(x => x.boardID == AttendanceManager.Instance.attendanceDataList[i].attendanceBoardID);
                if (attendanceBoard != null)
                {
                    if (attendanceBoard.boardID.Contains("attendanceBoard4") && PlayerPrefs.HasKey("attendanceSkip"))
                    {
                        continue;
                    }

                    attendanceBoard.Show();
                }

            }
        }
    }
    

    void OnChagedMenu(LobbyState state)
    {
        if (SceneLobby.currentSubMenuState != SubMenuState.Attendance)
            Close();
    }

    public void OnClickCloseButton()
    {
        Close();
        //if (isLogin)
        //    Close();    
        //else
        //    SceneLobby.Instance.SceneChange(LobbyState.Lobby);
    }

    public void CloseOrHide(string boardID)
    {
        int count = 0;
        for (int i = 0; i < attendanceBoardList.Count; i++)
        {
            if (attendanceBoardList[i].isOpen)
            {
                count += 1;
            }
        }

        if (count == 0)
        {
            Close();
        }
        else
        {
            attendanceBoardList.Find(x => x.boardID == boardID).gameObject.SetActive(false);
        }
    }


   
    public void Close()
    {
        SceneLobby.Instance.OnChangedMenu -= OnChagedMenu;
        SceneManager.UnloadSceneAsync("Attendance");
    }
    
}

