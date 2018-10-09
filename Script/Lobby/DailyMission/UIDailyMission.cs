using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIDailyMission : MonoBehaviour {

    static public UIDailyMission Instance;

    [SerializeField]
    Text textHeroTrainingCount;

    [SerializeField]
    Text textRetreatCount;

    [SerializeField]
    Text textHeroEnhanceCount;

    [SerializeField]
    Text textTaxGetCount;

    [SerializeField]
    List<Button> missionButtonList = new List<Button>();

    [SerializeField]
    List<Image> rewardGetImage = new List<Image>();

    [SerializeField]
    Button allClearButton;
    
    [SerializeField]
    GameObject allClearPanel;

    public static bool isAllClearActive = false;

    private void Awake()
    {
        Instance = this;
    }

    private IEnumerator Start()
    {
        while (DailyMissionManager.isInitialized == false)
            yield return null;

        for (int i = 0; i < missionButtonList.Count; i++)
        {
            missionButtonList[i].interactable = false;
        }

        InitUI();
    }

    public void InitUI()
    {
        textHeroTrainingCount.text = DailyMissionManager.Instance.heroTrainingCount.ToString();
        textRetreatCount.text = DailyMissionManager.Instance.retreatCount.ToString();
        textHeroEnhanceCount.text = DailyMissionManager.Instance.heroEnhanceCount.ToString();
        textTaxGetCount.text = DailyMissionManager.Instance.taxGetCount.ToString();

        allClearPanel.SetActive(isAllClearActive);

        InitButtonList();

        InitRewardImage();
    }

    void InitButtonList()
    {
        missionButtonList[0].interactable = DailyMissionManager.Instance.isMissionComplete1;
        missionButtonList[1].interactable = DailyMissionManager.Instance.isMissionComplete2;
        missionButtonList[2].interactable = DailyMissionManager.Instance.isMissionComplete3;
        missionButtonList[3].interactable = DailyMissionManager.Instance.isMissionComplete4;

        allClearButton.interactable = DailyMissionManager.Instance.isAllClear;
    }

    void InitRewardImage()
    {
        rewardGetImage[0].gameObject.SetActive(DailyMissionManager.Instance.isMissionRewarded1);
        rewardGetImage[1].gameObject.SetActive(DailyMissionManager.Instance.isMissionRewarded2);
        rewardGetImage[2].gameObject.SetActive(DailyMissionManager.Instance.isMissionRewarded3);
        rewardGetImage[3].gameObject.SetActive(DailyMissionManager.Instance.isMissionRewarded4);
    }

    public void OnClickRewardButton(int num)
    {
        if (getRewardCoroutine != null)
            return;

        getRewardCoroutine = StartCoroutine(GetRewardDailyMissionComplete(num));
    }

    public void OnClickAllClearRewardButton()
    {
        if (allClearCoroutine != null)
            return;

        allClearCoroutine = StartCoroutine(GetRewardAllDailyMissionComplete());
    }

    Coroutine getRewardCoroutine = null;
    IEnumerator GetRewardDailyMissionComplete(int missionNum)
    {
        string php = "DailyMission.php";
        WWWForm form = new WWWForm();
        form.AddField("userID", User.Instance.userID);
        form.AddField("missionNum", missionNum);
        form.AddField("type", 3);
        string result = string.Empty;
        yield return StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(php, form, x => result = x));
        if(!string.IsNullOrEmpty(result))
        {
            Debug.LogError("개별 데일리미션 완료되면 안되는데 완료됨");
        }
        else
        {
            switch (missionNum)
            {
                case 1:
                    DailyMissionManager.Instance.isMissionComplete1 = false;
                    DailyMissionManager.Instance.isMissionRewarded1 = true;
                    break;
                case 2:
                    DailyMissionManager.Instance.isMissionComplete2 = false;
                    DailyMissionManager.Instance.isMissionRewarded2 = true;
                    break;
                case 3:
                    DailyMissionManager.Instance.isMissionComplete3 = false;
                    DailyMissionManager.Instance.isMissionRewarded3 = true;
                    break;
                case 4:
                    DailyMissionManager.Instance.isMissionComplete4 = false;
                    DailyMissionManager.Instance.isMissionRewarded4 = true;
                    break;
            }
            UIPopupManager.ShowOKPopup("미션 완료", "데일리미션 미션완료 보상으로 루비 50개를 획득하였습니다", null);
        }

        if (DailyMissionManager.Instance.isMissionRewarded1 && DailyMissionManager.Instance.isMissionRewarded2 && DailyMissionManager.Instance.isMissionRewarded3 && DailyMissionManager.Instance.isMissionRewarded4)
            DailyMissionManager.Instance.isAllClear = true;

        if (!DailyMissionManager.Instance.isMissionComplete1 && !DailyMissionManager.Instance.isMissionComplete2 && !DailyMissionManager.Instance.isMissionComplete3 && !DailyMissionManager.Instance.isMissionComplete4)
        {
            if (DailyMissionManager.Instance.onDailyMissionCheckerCallback != null)
                DailyMissionManager.Instance.onDailyMissionCheckerCallback(AlarmType.DailyMission, false);
        }

        InitUI();
        getRewardCoroutine = null;
        yield break;
    }

    Coroutine allClearCoroutine = null;
    IEnumerator GetRewardAllDailyMissionComplete()
    {
        string php = "DailyMission.php";
        WWWForm form = new WWWForm();
        form.AddField("userID", User.Instance.userID);
        form.AddField("type", 4);
        string result = string.Empty;
        yield return StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(php, form, x => result = x));
        if(!string.IsNullOrEmpty(result))
        {
            Debug.LogError("전체 데일리미션 완료되면 안되는데 완료됨");
        }
        else
        {
            
            UIPopupManager.ShowOKPopup("미션 완료", "전체 데일리 미션완료 보상으로 루비 100개를 획득하였습니다", null);
        }

        DailyMissionManager.Instance.isAllClear = false;
        isAllClearActive = true;

        InitUI();

        allClearCoroutine = null;
        yield break;
    }

    //private void Update()
    //{
    //    if (DailyMissionManager.Instance == null)
    //        return;

    //    InitUI();
    //}
}
