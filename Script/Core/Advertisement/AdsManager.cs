using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if !UNITY_EDITOR
using UnityEngine.Advertisements;
#endif

public class AdsManager : MonoBehaviour {


#if UNITY_IOS
private const string gameId = "1595839";
#elif UNITY_ANDROID
    private const string gameId = "1595838";
#endif
    private const string rewardVideo = "rewardedVideo";

    private void Start()
    {
#if !UNITY_EDITOR
        if (Advertisement.isSupported)
        {
            Advertisement.Initialize(gameId, true);
        }
#endif
    }

    public void ShowDefaultAd()
    {
        
#if !UNITY_EDITOR
        if (Advertisement.IsReady())
        {
            Advertisement.Show("video");
        }
#endif
    }
    /// <summary> 버프 획득용 광고 </summary>
    public void ShowAdsRewardAd()
    {
#if UNITY_EDITOR
        Debug.Log("ADS START!!!!!!!!!!!!");
        //테스트용
        AdController.Instance.isShow = false;

#endif
#if !UNITY_EDITOR
        if (Advertisement.IsReady())
        {
            ShowOptions options = new ShowOptions();
            options.resultCallback = HandleShowResult;
            Advertisement.Show(rewardVideo, options);
        }
#endif
        //테스트용
#if UNITY_EDITOR
        AdController.Instance.isSuccess = true;

        if (AdController.Instance.onAdShowEnd != null)
            AdController.Instance.onAdShowEnd();
#endif
    }


#if !UNITY_EDITOR
    void HandleShowResult(ShowResult result)
    {
        AdController.Instance.isShow = false;


        switch (result)
        {
            case ShowResult.Finished:
                Debug.Log("Ads Success!!!!!!!!!!!!!!!!!!");
                AdController.Instance.isSuccess = true;
                if (AdController.Instance.onAdShowEnd != null)
                    AdController.Instance.onAdShowEnd();
                break;
            case ShowResult.Skipped:
                Debug.Log("Ads Skip!!!!!!!!!!!!!!");
                AdController.Instance.isFailed = true;
                break;
            case ShowResult.Failed:
                Debug.Log("Ads Failed!!!!!!!!!!!!!!");
                AdController.Instance.isFailed = true;
                break;
        }
 
    }
#endif
}
