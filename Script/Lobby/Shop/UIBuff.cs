using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary> 버프 관련 UI </summary>
public class UIBuff : MonoBehaviour {

    [Header("버프 관련 이미지와 텍스트")]
    [SerializeField]
    Image BuffImage;
    [SerializeField]
    Text BuffTimeDay;
    [SerializeField]
    Text BuffTimeHour;
    [SerializeField]
    Text BuffTimeMinute;

    Text BuffProuctName;

    private void OnEnable()
    {

    }

    private void OnDisable()
    {

    }


}
