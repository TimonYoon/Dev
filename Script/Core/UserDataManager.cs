using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using UserData;

/// <summary> 유저 정보 관련 매니저용 오브젝트를 생성시켜 주는 클래스. 실질직인 구현은 각각의 매니저 클래스들이 하는걸로 </summary>
public partial class UserDataManager : MonoBehaviour
{
    public static UserDataManager Instance;

    public static bool isInitialized = false;

    void Awake()
    {
        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    public IEnumerator Init()
    {
        Debug.Log("서버 데이터 초기화 시작");
        SceneLogin.Instance.tipMessageText.text = "왕국 예산을 파악 중..";
        //재화
        if (MoneyManager.Instance)
        {
            yield return StartCoroutine(MoneyManager.InitMoneyDataCoroutine());
        }

        //보유 영웅 정보 초기화
        SceneLogin.Instance.tipMessageText.text = "영웅들이 왕국으로 향하는 중..";
        yield return StartCoroutine(HeroManager.Init());
        

        if (DictionaryManager.Instance)
        {
            yield return StartCoroutine(DictionaryManager.Init());
        }

        // 패키지
        if (PackageManager.Instance)
        {
            yield return StartCoroutine(PackageManager.Init());
        }

        //출석
        if (AttendanceManager.Instance)
        {
            yield return StartCoroutine(AttendanceManager.Init());
        }

        //메일
        if (MailManager.Instance)
        {
            yield return StartCoroutine(MailManager.MailDataInitCoroutine());
        }

        //훈련소
        if(HeroTrainingManager.Instance)
        {
            yield return StartCoroutine(HeroTrainingManager.Init());
        }

        //데일리미션
        if (DailyMissionManager.Instance)
        {
            yield return StartCoroutine(DailyMissionManager.Init());
        }

        //신규유저미션
        if(UserQuestManager.Instance)
        {
            yield return StartCoroutine(UserQuestManager.Init());
        }

        while (!MoneyManager.isInitialized || !MailManager.isInitialized || !HeroTrainingManager.isInitialized || !DailyMissionManager.isInitialized || !UserQuestManager.isInitialized ||
           !HeroManager.isInitialized || !DictionaryManager.isInitialized || !AttendanceManager.isInitialized)
            yield return null;

        //등등...
        Debug.Log("서버 데이터 매니저 초기화 완료");
        isInitialized = true;

        yield break;
    }
}