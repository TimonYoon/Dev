using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

//public enum LanguageType
//{
//    Korean = 0,
//    English =1,
//    Chinese=2
//}

public class UIOption : MonoBehaviour
{
    [Header("속도관련")]
    [SerializeField]
    Slider worldmapMoveSpeed;

    [SerializeField]
    Slider zoomInOutSpeed;

    [Header("게임설정 패널 관련")]

    [SerializeField]
    GameObject gameOptionScroll;

    [SerializeField]
    Toggle gameSettingToggle;

    [SerializeField]
    Toggle bgmToggle;

    [SerializeField]
    Toggle seToggle;

    [SerializeField]
    Toggle vibrationToggle;

    [SerializeField]
    Toggle damageEffectToggle;

    [SerializeField]
    Toggle hpBarToggle;

    [SerializeField]
    Text userIDText;

    [Header("로그인 관려")]
    [SerializeField]
    Text facebookButtonText;
    [SerializeField]
    Text googleButtonText;

    [Header("언어 패널 관련")]

    [SerializeField]
    GameObject languageOptionScroll;

    [SerializeField]
    Toggle languageToggle;

    [SerializeField]
    ToggleGroup toggleGroup;

    [SerializeField]
    Transform content;

    [SerializeField]
    GameObject languageSlotPrefab;

    [Header("이용약관 관련")]
    [SerializeField]
    GameObject agreementPanel;

    [Header("스탭롤 관련")]
    [SerializeField]
    GameObject staffRolePanel;

    [Header("쿠폰입력 관련")]
    [SerializeField]
    GameObject couponPanel;

    IEnumerator Start()
    {
        while (!SceneLobby.Instance)
            yield return null;

        SceneLobby.Instance.OnChangedMenu += OnChangedMenu;

        StartCoroutine(Show());
    }

    void OnChangedMenu(LobbyState state)
    {
        if (SceneLobby.currentSubMenuState != SubMenuState.Option)
            Close();
    }

    /// <summary> 옵션 상단 탭 클릭했을 때 </summary>
    public void OnClickTabButton(string value)
    {
        switch(value)
        {
            case "Game":
                {
                    gameOptionScroll.SetActive(true);
                    languageOptionScroll.SetActive(false);
                    break;
                }
            case "Language":
                {
                    gameOptionScroll.SetActive(false);
                    languageOptionScroll.SetActive(true);
                    break;
                }
            default:
                break;
        }
    }





    /// <summary> 옵션창 보여주기  </summary>
    public IEnumerator Show()
    {
        while (!OptionManager.Instance)
            yield return null;
        bgmToggle.isOn = OptionManager.Instance.isOnBGM;
        seToggle.isOn = OptionManager.Instance.isOnSE;
        vibrationToggle.isOn = OptionManager.Instance.isOnVibration;
        damageEffectToggle.isOn = OptionManager.Instance.isOnDamageEffect;
        hpBarToggle.isOn = OptionManager.Instance.isOnHPBar;
        //데미지이펙트 추가

        if (User.Instance)
        {
            userIDText.text = User.Instance.userID;
            if (string.IsNullOrEmpty(User.Instance.facebookID))
                facebookButtonText.text = "페이스북 연동";
            else
                facebookButtonText.text = "페이스북 연동해제";

            if (string.IsNullOrEmpty(User.Instance.googleID))
                googleButtonText.text = "구글 연동";
            else
                googleButtonText.text = "구글 연동해제";
            
        }
            



        InitLanguageSlot();
        //languageSlotList.Find(x => x.languageType == (LanguageType)OptionManager.Instance.language).IsOn();
        

        //worldmapMoveSpeed.value = WorldmapCamera.Instance.scrollSpeed;
        //zoomInOutSpeed.value = ZoomInOut.Instance.ZoomSpeedTouch;
    }

    public void OnClickCloseButton()
    {
        //SceneLobby.Instance.SceneChange(LobbyState.Lobby);
        Close();
    }
    void Close()
    {
        SceneLobby.Instance.OnChangedMenu -= OnChangedMenu;
        SceneManager.UnloadSceneAsync("Option");
    }


    List<UILanguageSlot> languageSlotList = new List<UILanguageSlot>();
     

    void InitLanguageSlot()
    {
        // 지원 언어 체크해서 언어 슬롯 세팅
        List<LanguageType> typeList = new List<LanguageType>();
        typeList.Add(LanguageType.korean);
        typeList.Add(LanguageType.english);
        typeList.Add(LanguageType.chinese);

        // to do : 언어 데이터가 구성되면 다시 고쳐야할 부분
        for (int i = 0; i < 3; i++)
        {
            LanguageData data = new LanguageData(typeList[i], typeList[i].ToString());

            GameObject go = Instantiate(languageSlotPrefab, content, false);
            UILanguageSlot slot = go.GetComponent<UILanguageSlot>();
            slot.InitSlot(data, toggleGroup);
            slot.onClickLanguage += OnClickLanguage;
            languageSlotList.Add(slot);
        }

    }
    //월드맵이 항상 켜있지 않은 현상태에서는 작동 못함
    //private void Update()
    //{
    //    WorldmapCamera.Instance.scrollSpeed = worldmapMoveSpeed.value;
    //    ZoomInOut.Instance.ZoomSpeedTouch = zoomInOutSpeed.value;
    //}

    public void OnClickLanguage(LanguageType type)
    {
        OptionManager.Instance.ChangeLanguage(type);
    }

    public void OnClickBGM()
    {
        OptionManager.Instance.ChangeBGM(bgmToggle.isOn);
    }

    public void OnClickSE()
    {
        OptionManager.Instance.ChangeSE(seToggle.isOn);
    }

    public void OnClickVibration()
    {
        OptionManager.Instance.ChangeVibration(vibrationToggle.isOn);
    }

    public void OnClickDamageEffect()
    {
        OptionManager.Instance.ChangeDamageEffect(damageEffectToggle.isOn);
    }

    public void OnClickHPBar()
    {
        OptionManager.Instance.ChangeHPBar(hpBarToggle.isOn);
    }
    //public void OnClickTermsOfUse()
    //{
    //    OptionManager.Instance.OnClickTermsOfUse();
    //}

    public void OnClickCloudSave()
    {
        OptionManager.Instance.OnClickCloudSave();
    }

    public void OnClickCloudLoad()
    {
        OptionManager.Instance.OnClickCloudLoad();
    }

    public void OnClickGoogleLogOut()
    {
        OptionManager.Instance.OnClickGoogleLogOut();
    }

    public void OnClickFaceBookLogOut()
    {
        OptionManager.Instance.OnClickFaceBookLogOut();
    }

    public void OnClickDeleteUserID()
    {        
        OptionManager.Instance.OnClickDeleteUserID();
    }

    public void OnClickOpenAgreement()
    {
        agreementPanel.SetActive(true);
    }

    public void OnClickCloseAgreement()
    {
        agreementPanel.SetActive(false);
    }

    public void OnClickOpenStaffRole()
    {
        staffRolePanel.SetActive(true);
    }

    public void OnClickCloseStaffRole()
    {
        staffRolePanel.SetActive(false);
    }

    public void OnClickOnOffCouponPanel()
    {
        CouponManager.Instance.InitCouponUI();
        couponPanel.SetActive(!couponPanel.activeSelf);
    }
}
