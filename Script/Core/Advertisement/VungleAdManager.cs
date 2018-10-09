using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VungleAdManager : MonoBehaviour
{

    string appID = "";
    string iosAppID = "ios_app_id";
    string androidAppID = "5a053bcc1de694af74002ad8";
    public string[] placementsArray;

    private void Start()
    {
#if UNITY_IPHONE
    Dictionary<string, bool> placements = new Dictionary<string, bool>
    {
        { "ios_placement_id_1", false },
        { "ios_placement_id_2", false },
        { "ios_placement_id_3", false }
    };
#elif UNITY_ANDROID
        Dictionary<string, bool> placements = new Dictionary<string, bool>
        {
            { "DEFAULT51387", true },
            { "REWARDA83428", true }
        };
#endif
        placementsArray = new string[placements.Keys.Count];
        placements.Keys.CopyTo(placementsArray, 0);

        Vungle.init(androidAppID, iosAppID);
        Debug.Log("VUNGLE INIT");
        InitEventHandler();
    }

    void InitEventHandler()
    {
        Vungle.onAdStartedEvent += onAdStartedEvent;
        Vungle.onAdFinishedEvent += onAdFinishedEvent;
        Vungle.adPlayableEvent += adPlayableEvent;
    }


    void OnDisable()
    {
        Vungle.onAdStartedEvent -= onAdStartedEvent;
        Vungle.onAdFinishedEvent -= onAdFinishedEvent;
        Vungle.adPlayableEvent -= adPlayableEvent;
    }


    void onAdStartedEvent()
    {
        Debug.Log("onAdStartedEvent");
    }


    void onAdFinishedEvent(AdFinishedEventArgs arg)
    {
        AdController.Instance.isShow = false;
        Debug.Log("onAdFinishedEvent. watched: " + arg.TimeWatched + ", length: " + arg.TotalDuration + ", isCompletedView: " + arg.IsCompletedView);
        if (arg.IsCompletedView == true || arg.WasCallToActionClicked == true)
        {
            AdController.Instance.isSuccess = true;

            if (AdController.Instance.onAdShowEnd != null)
                AdController.Instance.onAdShowEnd();
        }
        else
        {
            AdController.Instance.isFailed = true;
        }
    }


    void adPlayableEvent(bool playable)
    {
        Debug.Log("adPlayableEvent: " + playable);
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            Vungle.onPause();
        }
        else
        {
            Vungle.onResume();
        }
    }

    public void ShowVungleAD()
    {
#if UNITY_EDITOR
        AdController.Instance.isShow = false;
        AdController.Instance.isSuccess = true;

        if (AdController.Instance.onAdShowEnd != null)
            AdController.Instance.onAdShowEnd();
#endif
#if !UNITY_EDITOR
        if (Vungle.isAdvertAvailable())
        {
            Vungle.playAd();
        }
        else
        {
            UIPopupManager.ShowOKPopup("광고", "광고 불러오기를 실패하였습니다\n다시 시도해주세요", null);
        }
#endif
    }

}
