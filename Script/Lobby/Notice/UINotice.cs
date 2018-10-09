using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class UINotice : MonoBehaviour {

    [SerializeField]
    Toggle oneDayToggle;

    [SerializeField]
    Toggle updateToggle;

    [SerializeField]
    Text textNotice;



    private void OnEnable()
    {
        SceneLobby.Instance.OnChangedMenu += OnChangedMenu;
    }

    private void OnDisable()
    {
        SceneLobby.Instance.OnChangedMenu -= OnChangedMenu;
    }

    private void Start()
    {
        if (PlayerPrefs.HasKey("NoticeDontSeeOneDay"))
        {
            if (NoticeController.Instance.today < DateTime.Parse(PlayerPrefs.GetString("NoticeDontSeeOneDay")))
            {
                oneDayToggle.isOn = true;
            }
        }
        else if (PlayerPrefs.HasKey("NoticeDontSeeUpdate"))
        {

            string clientVersion = WebServerConnectManager.clientVersion;


            if (clientVersion.Equals(PlayerPrefs.GetString("NoticeDontSeeUpdate")) == true)
            {
                updateToggle.isOn = true;
            }
        }
    }

    void OnChangedMenu(LobbyState state)
    {
        if(state != LobbyState.SubMenu && SceneLobby.currentSubMenuState != SubMenuState.Notice)
        {
            Close();
        }
    }

    public void OnClickOneDayToggleButton()
    {
        if (oneDayToggle.isOn)
        {
            PlayerPrefs.SetString("NoticeDontSeeOneDay", NoticeController.Instance.midNight.ToLongDateString());
        }
        else
        {
            if (PlayerPrefs.HasKey("NoticeDontSeeOneDay"))
            {
                PlayerPrefs.DeleteKey("NoticeDontSeeOneDay");
            }
        }
    }

    public void OnClickUntilUpdateToggleButton()
    {
        if (updateToggle.isOn)
        {

            string clientVersion = WebServerConnectManager.clientVersion;
            PlayerPrefs.SetString("NoticeDontSeeUpdate", clientVersion);

        }
        else
        {
            if (PlayerPrefs.HasKey("NoticeDontSeeUpdate"))
            {
                PlayerPrefs.DeleteKey("NoticeDontSeeUpdate");
            }
        }
    }

    public void OnClickCloseButton()
    {
        Close();
    }

    void Close()
    {
        SceneManager.UnloadSceneAsync("Notice");
    }
}
