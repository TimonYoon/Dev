using UnityEngine;
using System.Collections;
/// <summary> 진동관련한 매서드 관리하는 매니저</summary>
public class VibrationManager : MonoBehaviour
{
    bool isActive;

    //private void OnEnable()
    //{
    //    UIOption.onUIOptionVibrationCallback += SetVibration;
    //}

    //private void OnDisable()
    //{
    //    UIOption.onUIOptionVibrationCallback -= SetVibration;
    //}

    // TO DO : 1. 진동 설정(On/Off) , 2. 진동 만들기 
    // 1. 진동설정(On/Off)
    void SetVibration(bool _isOnVibration)
    {
        // 진동 꺼주는 상태 
        isActive = _isOnVibration;
    }

    // 2.진동 만들기 
    // 진동 관련 매서드 =>      
    void CreateVibration()
    {
        if (isActive==true)
        {
            Handheld.Vibrate();
        }
        else
        {
            Debug.Log("진동 Off 상태입니다. ");
        }

    }

}
