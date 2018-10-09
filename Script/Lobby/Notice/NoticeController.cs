using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

public class NoticeController : MonoBehaviour {

    public static NoticeController Instance;

    public DateTime today;
    public DateTime midNight;

    private void Awake()
    {
        Instance = this;
    }

    public IEnumerator InitNotice()
    {
        today = DateTime.Now;

        midNight = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day + 1);
        if (PlayerPrefs.HasKey("NoticeDontSeeOneDay"))
        {
            if(today > DateTime.Parse(PlayerPrefs.GetString("NoticeDontSeeOneDay")))
            {
                PlayerPrefs.DeleteKey("NoticeDontSeeOneDay");
                yield return StartCoroutine(AssetLoader.Instance.LoadLevelAsync("scene/notice", "Notice", true));
            }
            else
            {
                yield break;
            }
        }
        else if(PlayerPrefs.HasKey("NoticeDontSeeUpdate"))
        {

            string clientVersion = WebServerConnectManager.clientVersion;


            if (clientVersion.Equals(PlayerPrefs.GetString("NoticeDontSeeUpdate")) == false)
            {
                PlayerPrefs.DeleteKey("NoticeDontSeeUpdate");
                yield return StartCoroutine(AssetLoader.Instance.LoadLevelAsync("scene/notice", "Notice", true));
            }
            else
            {
                yield break;
            }
        }
        else
        {
            yield return StartCoroutine(AssetLoader.Instance.LoadLevelAsync("scene/notice", "Notice", true));
        }
    }

   
}
