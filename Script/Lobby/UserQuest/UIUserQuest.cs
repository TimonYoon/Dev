using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIUserQuest : MonoBehaviour {

    public static UIUserQuest Instance;
    

    [SerializeField]
    List<Image> rewardGetImage = new List<Image>();

    [SerializeField]
    List<UIUserQuestSlot> userQuestSlotList = new List<UIUserQuestSlot>();

    [SerializeField]
    GameObject allClearPanel;

    public static bool isAllClearActive = false;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        SceneLobby.Instance.OnChangedMenu += OnChangedMenu;
    }

    private void OnDisable()
    {
        SceneLobby.Instance.OnChangedMenu -= OnChangedMenu;
    }

    void OnChangedMenu(LobbyState state)
    {
        if(SceneLobby.currentSubMenuState != SubMenuState.UserQuest)
        {
            Close();
        }
    }

    public void OnClickCloseButton()
    {
        Close();
    }

    void Close()
    {
        SceneManager.UnloadSceneAsync("UserQuest");
    }

    private IEnumerator Start()
    {
        while (UserQuestManager.isInitialized == false)
            yield return null;

       
        InitUI();
    }

    public void InitUI()
    {
        allClearPanel.SetActive(UserQuestManager.Instance.isAllClear);
        
        InitRewardImage();

        for (int i = 0; i < userQuestSlotList.Count; i++)
        {
            userQuestSlotList[i].InitUI();
        }
    }

    void InitRewardImage()
    {

        for (int i = 0; i < rewardGetImage.Count; i++)
        {
            rewardGetImage[i].gameObject.SetActive(UserQuestManager.Instance.isMissionRewarded[i] == true);
        }
    }



    

    //public void OnClickAllClearRewardButton()
    //{
    //    if (allClearCoroutine != null)
    //        return;

    //    allClearCoroutine = StartCoroutine(GetRewardAllUserQuestComplete());
    //}

   

    //Coroutine allClearCoroutine = null;
    //IEnumerator GetRewardAllUserQuestComplete()
    //{
    //    string php = "UserQuest.php";
    //    WWWForm form = new WWWForm();
    //    form.AddField("userID", User.Instance.userID);
    //    form.AddField("type", 4);
    //    string result = string.Empty;
    //    yield return StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(php, form, x => result = x));
    //    if (!string.IsNullOrEmpty(result))
    //    {
    //        Debug.LogError("전체 데일리미션 완료되면 안되는데 완료됨");
    //    }
    //    else
    //    {

    //        UIPopupManager.ShowOKPopup("미션 완료", "전체 데일리 미션완료 보상으로 루비 100개를 획득하였습니다", null);
    //    }

    //    UserQuestManager.Instance.isAllClear = false;
    //    isAllClearActive = true;

    //    InitUI();

    //    allClearCoroutine = null;
    //    yield break;
    //}
}
