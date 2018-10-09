using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//함수를 호출할 매니저들을 구분하기 쉽게 하기 위한 enum
public enum AlarmType
{
    Mail,
    Dictionary,
    Hero,
    DailyMission,
    UserQuest,
};

public class UpdateAlarm : MonoBehaviour {

    public static UpdateAlarm Instance;

    public static bool updateMail = false;
    public static bool updateDic = false;
    public static bool updateDaily = false;
    public static bool updateUserQuest = false;
    
    //각 버튼에 달린 체커들을 활성화시키기 위한 레퍼런스
    public GameObject[] checkers;

    public GameObject heroMenuChecker;
    public GameObject subMenuChecker;
    

    //콜백삽입

    void Awake()
    {
        Instance = this;
    }

    private IEnumerator Start()
    {
        while (!SceneLobby.Instance)
            yield return null;
        
        HeroManager.Instance.onNewHeroCheckerCallback += AlarmCheck;
        MailManager.Instance.onNewMailCheckerCallback += AlarmCheck;
        DictionaryManager.Instance.onDictionaryCheckerCallback += AlarmCheck;
        DailyMissionManager.Instance.onDailyMissionCheckerCallback += AlarmCheck;
        UserQuestManager.Instance.onUserQuestCheckerCallback += AlarmCheck;

        if(updateMail == true)
        {
            AlarmCheck(AlarmType.Mail, updateMail);
        }
        if(updateDic == true)
        {
            AlarmCheck(AlarmType.Dictionary, updateDic);
        }
        if (updateDaily == true)
            AlarmCheck(AlarmType.DailyMission, updateDaily);
        if (updateUserQuest == true)
            AlarmCheck(AlarmType.UserQuest, updateUserQuest);
            
    }
    //테스트 메일 체커
    public void AlarmCheck(AlarmType type, bool check)
    {
        StartCoroutine(CheckerUp(type, check));
    }

    IEnumerator CheckerUp(AlarmType alarmType, bool check)
    {
        ////영웅 획득의 경우 상점에서 진행되므로 서브 메뉴와 상관없음
        //if(SceneLobby.currentState != LobbyState.Shop || SceneLobby.currentState != LobbyState.Hero)
        //{
        //    //서브메뉴가 꺼지기 때문에 기다린다
        //    while (SceneLobby.currentState != LobbyState.Lobby)
        //        yield return null;

        //    //SceneLobby에서 SetActive하는 것을 기다린다
        //    while (!subMenu.gameObject.activeSelf)
        //        yield return null;
        ////}
        //switch (SceneLobby.currentState)
        //{
        //    case LobbyState.Shop:
        //    case LobbyState.Hero:
        //    case LobbyState.SubMenu:
        //        break;
        //    default:
        //        //while (SceneLobby.currentState != LobbyState.Lobby)
        //        //    yield return null;
        //        //SceneLobby에서 SetActive하는 것을 기다린다
        //        while (!subMenu.gameObject.activeSelf)
        //            yield return null;
        //        break;
        //}
        
        if (checkers[(int)alarmType].activeSelf == check)
            yield break;
        else
            checkers[(int)alarmType].SetActive(check);

        if (checkers[(int)AlarmType.Hero].activeSelf)
        {
            heroMenuChecker.SetActive(true);
        }
        else
        {
            heroMenuChecker.SetActive(false);
        }

        if (checkers[(int)AlarmType.Mail].activeSelf || checkers[(int)AlarmType.Dictionary].activeSelf || checkers[(int)AlarmType.DailyMission].activeSelf || checkers[(int)AlarmType.UserQuest].activeSelf)
        {
            subMenuChecker.SetActive(true);
        }
        else
        {
            subMenuChecker.SetActive(false);
        }
        
    }
}
