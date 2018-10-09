using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



/// <summary> 출석 일부별 슬롯 </summary>
public class UIAttendanceSlot : MonoBehaviour {

    //[SerializeField]
    //Text rewardText;

    //[SerializeField]
    //Image rewardImage;

    [SerializeField]
    UIComplete completeScript;

    [SerializeField]
    RectTransform checker;


    /// <summary> 슬롯 초기화 </summary>
    public void InitSlot(bool value)
    {

        // 우선 false 차후 서버에서 오는 데이터에 따라 지정할 예정
        completeScript.InitComplete(value);
    }

    public void ShowComplete()
    {
        completeScript.Show();
    }


	
}
