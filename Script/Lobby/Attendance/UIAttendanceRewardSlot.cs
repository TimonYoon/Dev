using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using AssetBundles;
public class UIAttendanceRewardSlot : MonoBehaviour {

    // TO DO : 사장님 말씀 하신 효과 넣는 방법 - 많아 질 시에 사용한다.
    public string attendanceRewardID;
    public List<GameObject> toggledObjectRewarded = new List<GameObject>();


    // 보상 슬롯 애들의 이미지가 모두 다르다.
    [SerializeField]
    Image AttendanceRewardBackGroundImage;
    
    // 1일차.가 들어있는 속에 이미지. 나 색 조절가능함.
    [SerializeField]
    Image AttendanceRewardSlotUpImage;

    // 몇 일 차
    [SerializeField]
    Text AttendanceRewardSlotDayCountText;

    // 출석 보상 아이템 텍스트
    [SerializeField]
    Text AttendanceRewardSlotItemNameText;

    // 출석 보상 아이템 이미지
    [SerializeField]
    Image AttendanceRewardSlotItemImage;

    // 출석 체크 이미지  - 나중에 토글에 모두 넣을 예정 
    [SerializeField]    
    GameObject AttendanceCheckDozangGameObject;

    Texture testTexture;

    /// <summary> 출석 보상 슬롯의 필터 데이터</summary>
    public AttendanceFilterData attendanceFilterData { get; private set; }

    /// <summary> 출석 보상 필터 데이터를 받아오는 함수</summary>    
    public void InitAttendanceRewardSlotData(AttendanceFilterData _value,int _currentAttendanceCount)
    {

        AttendanceFilterData _attendanceFilterData = new AttendanceFilterData();
        _attendanceFilterData.filterID = _value.filterID;
        _attendanceFilterData.filterCategory = _value.filterCategory;
        _attendanceFilterData.filterAttendanceCount = _value.filterAttendanceCount;
        _attendanceFilterData.rewardType = _value.rewardType;
        _attendanceFilterData.rewardImageName = _value.rewardImageName;
        _attendanceFilterData.rewardAmount = _value.rewardAmount;
        attendanceFilterData = _attendanceFilterData;

        // TO DO :카운트 체크 해줘서 킬지 끌지를 정해준다.- 서버 현재 카운트. 출석 카운트
        if (_value.filterAttendanceCount <= _currentAttendanceCount)
        {
            AttendanceCheckDozangGameObject.SetActive(true);
        }
        else
        {
            AttendanceCheckDozangGameObject.SetActive(false);
        }

        InitAttendanceRewardSlotUI();
    }

    /// <summary> 출석 보상 슬롯의 UI를 표현한다. </summary>
    public void InitAttendanceRewardSlotUI()
    {
        StartCoroutine(test());
    }

    IEnumerator test()
    {
        // TO DO : 백그라운드 이미지 - 넣어줘야함. 
        // AttendanceRewardBackGroundImage = 
        // TO DO : 보상 슬롯 위쪽 이미지 - 넣어줘야함.

        //AttendanceRewardSlotUpImage.overrideSprite = newSprite;
        // AttendanceRewardSlotUpSprite = Resources.Load<Sprite>("Resources/ShopUI/Shop/diamondImage");

        AttendanceRewardSlotDayCountText.text = attendanceFilterData.filterAttendanceCount + "일차";
        AttendanceRewardSlotItemNameText.text = attendanceFilterData.rewardType;
        // 에셋 번들 - 받아오는데 시간 걸려서 코루틴 사용. 
        yield return StartCoroutine(AssetLoader.Instance.LoadTexture("sprite", attendanceFilterData.rewardImageName, x => testTexture = x));
        
        Rect rect = new Rect(0, 0, testTexture.width, testTexture.height);
        AttendanceRewardSlotItemImage.sprite = Sprite.Create((Texture2D)testTexture,rect,new Vector2(0,0));

    }

    //bool isRewarded
    //{
    //    get
    //    {
    //        //id에 해당하는 데이타 검색
    //        //a 몇일자 보상임?
    //        //b 지금 몇일자 출석했음?
    //        //b>=a true , 아니면 false


    //        return false;
    //    }
    //}

    //AttendanceDataController attenanceDataController;

    //private void Awake()
    //{
    //    attenanceDataController = FindObjectOfType<AttendanceDataController>();

    //}

    //private void Start()
    //{
    //    //초기화
    //    //toggledObjectRewarded 일단 다 끄기

    //    //보상 받았는지 여부
    //    if (isRewarded)
    //    {
    //        //toggledObjectRewarded 다 켜기
    //    }

    //}


}
