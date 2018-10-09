using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LitJson;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;

public class UINickname : MonoBehaviour {

    //public static UINickname Instance;

    [SerializeField]
    GameObject nicknamePanel;

    public InputField nicknameInput;
    [SerializeField]
    Button enterButton;

    bool isOK = false;

    string restrictChar = "!@#$%^&*()-_=+|\'\"?/>.<,:;{[}]`~";

    //private void Awake()
    //{
    //    Instance = this;
    //}

    private void OnEnable()
    {
        enterButton.interactable = false;

        SceneLobby.Instance.OnChangedMenu += OnChangedMenu;

        nicknameInput.onValueChanged.AddListener(delegate { OnValueChanged(); });
        

        FocusInputField();
        
    }

    private void OnDisable()
    {
        SceneLobby.Instance.OnChangedMenu -= OnChangedMenu;
    }

    void OnChangedMenu(LobbyState state)
    {
        if (state != LobbyState.Shop)
            Close();
    }

    void OnValueChanged()
    {
        if (nicknameInput.text.Length > 0)
            enterButton.interactable = true;
        else
            enterButton.interactable = false;
    }

    public void OnClickChangeNickname()
    {
        
        if (coroutine != null)
            return;

        if(MoneyManager.GetMoney(MoneyType.ruby).value - int.Parse(NicknameManager.Instance.tempShopData.price) < 0 && User.Instance.changeNickname > 0)
        {
            UIPopupManager.ShowOKPopup("구매 실패", NicknameManager.Instance.tempShopData.costType + "가 부족합니다", null);
            return;
        }

        string temp = "";
        temp = nicknameInput.text;

        if(System.Text.Encoding.UTF8.GetByteCount(temp) > 36 || temp.Length > 24)
        {
            UIPopupManager.ShowOKPopup("변경 실패", "제한된 글자수를 초과하셨습니다", null);
            return;
        }

        isOK = CheckSlangWord(temp);

        if(isOK == true)
        {
            isOK = CheckRestrictChar(temp);
        }

        if (isOK == true)
        {
            //ChangeNickname(temp, tempShopData);
            UIPopupManager.ShowYesNoPopup("닉네임 변경", "변경시 즉시 적용되며 환불이 불가합니다\n적용하시겠습니까?", PopupResultChangeNickname);
        }
        else
        {
            return;
        }
    }

    void PopupResultChangeNickname(string result)
    {
        if (result == "yes")
        {
            ChangeNickname(nicknameInput.text, NicknameManager.Instance.tempShopData);
        }
        else
        {
            FocusInputField();
        }
    }

    bool CheckRestrictChar(string nick)
    {
        bool contain = false;
        foreach (char c in restrictChar)
        {
            string word = c.ToString();

            if (nick.Contains(word))
            {
                contain = false;
                break;
            }
            else
            {
                contain = true;
            }
        }

        if (contain == false)
            UIPopupManager.ShowOKPopup("변경 실패", "특수문자를 입력하셨습니다", FocusInputField);

        return contain;
    }

    bool CheckSlangWord(string nick)
    {
        if (string.IsNullOrEmpty(nick) || nick.Contains(" "))
        {
            UIPopupManager.ShowOKPopup("변경 실패", "입력이 없거나 공백을 입력하셨습니다", FocusInputField);
            return false;
        }

        string temp = Regex.Replace(nick, @"[\d-]", string.Empty);
        
       
        for (int i = 0; i < NicknameManager.Instance.slangList.Count; i++)
        {
            if (temp.Contains(NicknameManager.Instance.slangList[i]))
            {
                UIPopupManager.ShowOKPopup("변경 실패", "비속어를 포함하실 수 없습니다", FocusInputField);
                return false;
            }
        }

        return true;
    }

    //연속입력 방지를 위한 메소드
    Coroutine coroutine;
    void ChangeNickname(string nick, ShopData shopData)
    {
        coroutine = StartCoroutine(ChangeNicknameCoroutine(nick, shopData));
    }

    IEnumerator ChangeNicknameCoroutine(string nick, ShopData shopData)
    {
        WWWForm form = new WWWForm();
        string php = "Nickname.php";
        string result = "";
        form.AddField("userID", User.Instance.userID);
        form.AddField("nickname", nick);
        form.AddField("changeNickname", User.Instance.changeNickname);
        form.AddField("type", 1);
        form.AddField("shopID", shopData.id);
        
        yield return (StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(php, form, x => result = x)));
        if(!string.IsNullOrEmpty(result) && result == "1")
        {
            UIPopupManager.ShowOKPopup("닉네임 중복", "중복된 닉네임입니다\n다시 입력해주세요", FocusInputField);
        }
        else if(!string.IsNullOrEmpty(result) && result == "2")
        {
            UIPopupManager.ShowOKPopup("변경 실패", "입력이 없거나 공백을 입력하셨습니다", FocusInputField);
        }
        else if (!string.IsNullOrEmpty(result) && result == "3")
        {
            UIPopupManager.ShowOKPopup("금액 부족", "루비가 부족합니다", FocusInputField);
        }
        else
        {
            isOK = false;
            UIPopupManager.ShowOKPopup("변경 완료", "닉네임이 성공적으로 변경되었습니다", Close);
            ShopDataController.Instance.UpdateShopSlot(shopData);
        }

        coroutine = null;
    }

    void Close()
    {
        if (UIShop.Instance && UIShop.Instance.loadingPanel.activeSelf)
            UIShop.Instance.loadingPanel.SetActive(false);

        Scene nickname = SceneManager.GetSceneByName("NicknameChange");
        SceneManager.UnloadSceneAsync(nickname);
    }

    public void OnClickCloseButton()
    {
        Close();
    }

    void FocusInputField()
    {
        nicknameInput.ActivateInputField();
        nicknameInput.Select();
    }

    
}
