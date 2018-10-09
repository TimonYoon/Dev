using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class UIAchievementSlot : MonoBehaviour
{
    public Image achievementsIconImage;
    public Text titleText;
    public Text goalSummaryText;
    public Image rewardIconImage;
    public Text rewardAmountText;
    public Text goalText;
    public Button rewardButton;

    /// <summary> 업적고유 ID </summary>
    public string achievementID { get; private set; }
    /// <summary> 업적현재 레벨(등급) </summary>
    public string achievementLevel { get; private set; }
    /// <summary> 업적 카테고리 </summary>
    public string category { get; private set; }
    /// <summary> 업적 제목 </summary>
    public string title { get; private set; }
    /// <summary> 업적 요약내용 </summary>
    public string goalSummary { get; private set; }
    /// <summary> 업적 현재 달성 수치 </summary>
    public string nowAmount { get; private set; }
    /// <summary> 업적 목표수치 </summary>
    public string goalAmount { get; private set; }
    /// <summary> 업적 아이콘 이미지 이름 </summary>
    public string achievementIcon { get; private set; }
    /// <summary> 업적 보상고유 ID </summary>
    public string rewardID { get; private set; }
    /// <summary> 업적 보상 수치 </summary>
    public string rewardAmount { get; private set; }
    /// <summary> 보상 아이콘 이미지 이름 </summary>
    public string rewardIcon { get; private set; }
    /// <summary> 업적을 완료했는지 체크 </summary>
    public string isDone { get; private set; }
    /// <summary> 보상을 받았는지 체크 </summary>
    public string isRewarded { get; private set; }

    

    /// <summary> 업적 데이터를 매개변수로 받아서 슬롯의 변수를 채운다. </summary>
    public void SlotDataInit(AchievementData data)
    {
         
        achievementID = data.achievementID;
        achievementLevel = data.achievementLevel;
        category = data.category;
        title = data.title;
        goalSummary = data.goalSummary;
        nowAmount = data.nowAmount;
        goalAmount = data.goalAmount;
        achievementIcon = data.achievementIcon;
        rewardID = data.rewardID;
        rewardAmount = data.rewardAmount;
        rewardIcon = data.rewardIcon;
        isDone = data.isDone;
        isRewarded = data.isRewarded;
        SlotUIInit();
    }
    public delegate void UIAchievementSlotRewardButtonClickCallback(UIAchievementSlot dd);
    public static UIAchievementSlotRewardButtonClickCallback onRewardButtonClickCallback;
    /// <summary> 슬롯의 UI를 표현한다. </summary>
    void SlotUIInit()
    {
        // 해당 슬롯에 UI에 업적정보를 적용시킨다.
        titleText.text = title;
        goalSummaryText.text = goalSummary + " (" + nowAmount + "/" + goalAmount + ")";
        rewardAmountText.text = rewardAmount;
        goalText.text = "(" + nowAmount + "/" + goalAmount + ")";

        // 버튼 활성화 조작 부분. to do : 무언가 좀더 효율적이고 좋은 형태로 활성화 비활성화를 넣어야 한다.
        if (isDone == "0" || isDone == "0" && isRewarded == "1")
        {
            rewardButton.interactable = false;
        }
        else if (isDone == "1" && isRewarded == "0")
        {
            rewardButton.interactable = true;
        }
    }

    public void OnClickRewardButton()
    {
        onRewardButtonClickCallback(this);
    }

}
