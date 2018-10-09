using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPlaceAlarmIcon : MonoBehaviour {

    [SerializeField]
    Image alarmIconImage;

    [SerializeField]
    Image exclamationIcon;

    [SerializeField]
    List<Sprite> alarmSpriteList;

    public string alarmImageName { get; private set; }

    public PlaceAlarmType placeAlarmType { get; private set; }

    public void InitSlot(string _alarmType,PlaceAlarmType type)
    {
        alarmImageName = _alarmType;
        placeAlarmType = type;
        bool isSreachSptite = false;
        for (int i = 0; i < alarmSpriteList.Count; i++)
        {
            if (alarmSpriteList[i].name == alarmImageName)
            {
                alarmIconImage.sprite = alarmSpriteList[i];
                isSreachSptite = true;
            }
        }

        if(isSreachSptite == false)
        {
            TerritoryManager.Instance.ChangeMaterialImage(alarmIconImage, alarmImageName);
        }
    }

    public void ShowIcon(bool isActive)
    {
        //Debug.Log(placeAlarmType.ToString() + " / " + alarmImageName + " / " + isActive.ToString());
        //Debug.Log(alarmImageName + "/" + isActive);
        alarmIconImage.gameObject.SetActive(isActive);

        // 느낌표 표시 잠시 꺼둠
        exclamationIcon.gameObject.SetActive(false);
    }

}