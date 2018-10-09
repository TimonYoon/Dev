using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using UnityEngine.SceneManagement;
using UserData;

/// <summary> 업적 UI 생성 / 업적 Button 기능 구현 </summary>
public class UIAchievement : MonoBehaviour {

    public GameObject achievementSlotPrefab;// 업적 프리팹
    public Transform achievementScrollViewContent; //업적 프리팹 생성위치 

    List<UIAchievementSlot> achievementSlotList; // 생성된 업적 슬롯들에 접근할 수 있는 부분
    

    private void OnEnable()
    {
        //AchievementManager.onAchievementManagerCallback += AchievementListCreate;
        //AchievementManager.onChangedAchievementDataCallback += ChangedUIAchievementSlotData;
        init();
        SceneLobby.Instance.OnChangedMenu += OnChangedMenu;
    }
    private void OnDisable()
    {
        //AchievementManager.onAchievementManagerCallback -= AchievementListCreate;
        //AchievementManager.onChangedAchievementDataCallback -= ChangedUIAchievementSlotData;
        SceneLobby.Instance.OnChangedMenu -= OnChangedMenu;
    }
    void OnChangedMenu(LobbyState state)
    {
        if (SceneLobby.currentSubMenuState != SubMenuState.Achievement)
            Close();
    }

    public void init()
    {
        //AchievementListCreate(AchievementManager.achievementsDataList);

    }

    /// <summary> 업적 데이터가 준비되면 업적 UI를 생성한다.</summary>
    void AchievementListCreate(List<AchievementData> data)
    {
        // 검수 부분...
        if(achievementSlotPrefab == null || achievementScrollViewContent == null)
        {
            Debug.Log("위의 Object가 존재하지 않습니다.");
            return;
        }
        achievementSlotList = new List<UIAchievementSlot>();
       
        for (int i = 0; i < data.Count; i++)
        {
            GameObject achievementSlot = Instantiate(achievementSlotPrefab);
            achievementSlot.transform.SetParent(achievementScrollViewContent, false);
            achievementSlot.GetComponent<UIAchievementSlot>().SlotDataInit(data[i]);
            achievementSlotList.Add(achievementSlot.GetComponent<UIAchievementSlot>());
        }
    }

    /// <summary> 하나의 업적 초기화 하는 부분 </summary>
    void ChangedUIAchievementSlotData(string achievementID, AchievementData data)
    {
        SelectAchievementsSlot(achievementID).SlotDataInit(data);
    }
    


    /// <summary> 업적ID로 해당 업적의 슬롯에 접근할 수 있다. </summary>
    UIAchievementSlot SelectAchievementsSlot(string achievementID)
    {
        foreach (var achievementSlot in achievementSlotList)
        {
            if(achievementSlot.achievementID == achievementID)
            {
                return achievementSlot;
            }
        }
        return null;
    }

    public void OnClickCloseButton()
    {
        //SceneLobby.Instance.SceneChange(LobbyState.Lobby);
        Close();
    }
    void Close()
    {
        SceneManager.UnloadSceneAsync("Achievements");
    }
    /*
     * (START) 업적 추가 테스트
     */
    public delegate void TESTCallback(string achievementsID,int amount);
    public static TESTCallback onTESTCallback;
    public void OnClickAchievementCountAdd(string achievementsID)
    {
        if (onTESTCallback != null)
            onTESTCallback(achievementsID, SelectAchievementsSlotAmountData(achievementsID));
    }

    /// <summary> 생성된 업적 리스트에 접근해서 원하는 업적의 현재 달성수치를 리턴한다. </summary>
    int SelectAchievementsSlotAmountData(string achievementsID)
    {
        int amount = 0;
        foreach (var item in achievementSlotList)
        {
            if (item.achievementID == achievementsID)
            {
                amount = int.Parse(item.nowAmount);
                break;
            }
        }
        return ++amount;
    }
    /*
     * (END) 여기까지 업적 추가 테스트
     */ 


}
