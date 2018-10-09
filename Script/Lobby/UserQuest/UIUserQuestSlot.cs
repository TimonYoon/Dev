using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIUserQuestSlot : MonoBehaviour {

    public UserQuestType type;

    public int level;

    int currentValue = 0;

    [SerializeField]
    Text textCurrentCount;

    [SerializeField]
    Button button;

    private void Awake()
    {
        currentValue = UserQuestManager.Instance.GetValue(type);
    }

    void Start () {

        button.interactable = false;

        InitUI();
	}

    public void InitUI()
    {
        textCurrentCount.text = currentValue.ToString();
        if (type == UserQuestType.TaxGet)
            button.interactable = (UserQuestManager.Instance.isMissionComplete[(int)type] == true && User.Instance.userLevel >= level);
        else
            button.interactable = (UserQuestManager.Instance.isMissionRewarded[(int)type - 1] == true && UserQuestManager.Instance.isMissionComplete[(int)type] == true && User.Instance.userLevel >= level);
    }

    public void OnClickRewardButton()
    {
        if (getRewardCoroutine != null)
            return;

        getRewardCoroutine = StartCoroutine(GetRewardUserQuestComplete((int)type + 1));
    }

    Coroutine getRewardCoroutine = null;
    IEnumerator GetRewardUserQuestComplete(int missionNum)
    {
        string php = "UserQuest.php";
        WWWForm form = new WWWForm();
        form.AddField("userID", User.Instance.userID);
        form.AddField("missionNum", missionNum);
        form.AddField("type", 3);
        string result = string.Empty;
        yield return StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(php, form, x => result = x));
        if (!string.IsNullOrEmpty(result))
        {
            Debug.LogError("개별 유저미션 완료되면 안되는데 완료됨");
        }
        else
        {
            UserQuestManager.Instance.isMissionComplete[missionNum - 1] = false;
            UserQuestManager.Instance.isMissionRewarded[missionNum - 1] = true;
            UIPopupManager.ShowOKPopup("미션 완료", "신규 유저 미션완료 보상을 우편으로 전송했습니다.\n확인해주세요", null);

            yield return StartCoroutine(MailManager.MailDataInitCoroutine());
        }

        //InitUI();
        //UIUserQuest.Instance.InitUI();
        getRewardCoroutine = null;
        yield break;
    }
}
