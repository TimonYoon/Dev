using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CoolTimeTestManager : MonoBehaviour
{
    [SerializeField]
    Text CoolTimeHourText;
    [SerializeField]
    Text CoolTimeMinuteText;

    float timeCheck = 2456;
    bool isReadyForFree=false; 

    private void Awake()
    {
    }

    private void Update()
    {
        if (!(timeCheck==0))
        {
            timeCheck -= Time.deltaTime;
            CoolTimeHourText.text = (System.Math.Truncate(timeCheck / 60)).ToString() + "분";
            CoolTimeMinuteText.text = ((int)(timeCheck % 60)).ToString() + "초 남음";
        }
        else
        {
            //if timeCheck == 0 이면
            isReadyForFree = true;
        }

        if(isReadyForFree)
        {
            // TO DO : 여기서 무료 구매 열어준다. 
            // => 열어 주는 방식을 위에 같은 

        }

    }

    // TO DO  : 임시로 테스트 용으로 그냥 다 박아줌.

    public void OnClickFirstButton()
    {

    }


}
