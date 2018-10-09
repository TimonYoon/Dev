using UnityEngine;
using System.Collections;

/// <summary> 업적 데이터 자료형클래스 </summary>
[System.Serializable]
public class AchievementData {

    
        /// <summary> 업적고유 ID </summary>
        public string achievementID;
        /// <summary> 업적현재 레벨(등급) </summary>
        public string achievementLevel;
        /// <summary> 업적 카테고리 </summary>
        public string category;
        /// <summary> 업적 제목 </summary>
        public string title;
        /// <summary> 업적 요약내용 </summary>
        public string goalSummary;
        /// <summary> 업적 현재 달성 수치 </summary>
        public string nowAmount;
        /// <summary> 업적 목표수치 </summary>
        public string goalAmount;
        /// <summary> 업적 아이콘 이미지 이름 </summary>
        public string achievementIcon;
        /// <summary> 업적 보상고유 ID </summary>
        public string rewardID;
        /// <summary> 업적 보상 수치 </summary>
        public string rewardAmount;
        /// <summary> 보상 아이콘 이미지 이름 </summary>
        public string rewardIcon;
        /// <summary> 업적을 완료했는지 체크 </summary>
        public string isDone;
        /// <summary> 보상을 받았는지 체크 </summary>
        public string isRewarded;

    

}
