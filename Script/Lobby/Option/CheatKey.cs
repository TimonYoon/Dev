using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CheatKey : MonoBehaviour {

//#if UNITY_EDITOR
    public void OnClickShowCheatPanel()
    {

        cheatPanel.SetActive(true);

    }

    public GameObject cheatPanel;
    public InputField inputField;

    public void OnClickGetHero()
    {

        string heroID = inputField.text;
        if (string.IsNullOrEmpty(heroID))
        {
            UIPopupManager.ShowInstantPopup("영웅 아이디 입력하시오");
            return;
        }

        if(GameDataManager.heroBaseDataDic.ContainsKey(heroID) == false)
        {
            UIPopupManager.ShowInstantPopup("존재하지 않는 영웅 아이디 입니다");
            return;
        }

        if (coroutine != null)
            return;

        coroutine = StartCoroutine(GetCoroutine(1, heroID));


        Close();
    }
    Coroutine coroutine;

    public void OnClickGetAllHero()
    {
        if (coroutine != null)
            return;

        coroutine = StartCoroutine(GetCoroutine(2));
        Close();
    }

    IEnumerator GetCoroutine(int type, string heroID = "")
    {
        WWWForm form = new WWWForm();
        form.AddField("type", type);
        form.AddField("userID", User.Instance.userID);

        if (string.IsNullOrEmpty(heroID) == false)
            form.AddField("heroID", heroID);

        yield return StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine("Cheat.php", form));
        coroutine = null;

        yield return null;
    }

    void Close()
    {
        cheatPanel.SetActive(false);
    }
//#endif
}
