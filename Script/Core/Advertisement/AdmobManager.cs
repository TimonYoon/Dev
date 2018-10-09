using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;
using System;

public class AdmobManager : MonoBehaviour
{

    string androidBannerID = "ca-app-pub-4214862165057951/9205418400";
    string iosBannerID = "ca-app-pub-4214862165057951/9205418400";

    string androidRewardAdID = "ca-app-pub-4214862165057951/9416337248";
    string iosRewardAdID = "ca-app-pub-4214862165057951/9416337248";

    private BannerView bannerView;
    private InterstitialAd rewardView;

 
    IEnumerator Start()
    {
        yield return (StartCoroutine(AdController.Instance.CheckPayedPlayer()));
        

        if (!AdController.Instance.isPayedUser)
        {
            RequestBannerAd();
        }
        else
        {
            while (AdController.Instance.onResizeCamera == null)
                yield return null;
            AdController.Instance.onResizeCamera();
        }

        RequestRewardAd();
    }

    public void RequestBannerAd()
    {
        string bannerAdUnitId = string.Empty;

#if UNITY_ANDROID
        bannerAdUnitId = androidBannerID;
#elif UNITY_IOS
        bannerAdUnitId = iosBannerID;
#endif
        bannerView = new BannerView(bannerAdUnitId, AdSize.SmartBanner, AdPosition.Bottom);
        
        //bannerView.LoadAd(new AdRequest.Builder().AddTestDevice(AdRequest.TestDeviceSimulator).AddTestDevice("6057E38E2E571864").Build());
        //bannerView.LoadAd(new AdRequest.Builder().Build());
        AdRequest request = new AdRequest.Builder().Build();
        bannerView.LoadAd(request);
        bannerView.Show();

        
    }
    AdRequest rewardRequest;
    private void RequestRewardAd()
    {
        string rewardAdUnitId = string.Empty;
#if UNITY_ANDROID
        rewardAdUnitId = androidRewardAdID;
#elif UNITY_IOS
        rewardAdUnitId = iosRewardAdID;
#endif
        rewardView = new InterstitialAd(rewardAdUnitId);

        rewardRequest = new AdRequest.Builder().Build();

        rewardView.OnAdClosed += HandleRewardBasedVideoClosed;
        rewardView.OnAdFailedToLoad += HandlerRewardFailedToLoad;

        //rewardView.LoadAd(rewardRequest);
        
    }

    public void ShowAdmobRewardAD()
    {
#if UNITY_EDITOR
        AdController.Instance.isShow = false;
        AdController.Instance.isSuccess = true;

        if (AdController.Instance.onAdShowEnd != null)
            AdController.Instance.onAdShowEnd();
#endif
#if !UNITY_EDITOR
        if (rewardView.IsLoaded())
        {
            rewardView.Show();
        }
        else
        {
            AdController.Instance.isShow = false;
            AdController.Instance.isFailed = true;
        }
#endif

    }

    public void HandleRewardBasedVideoClosed(object sender, EventArgs args)
    {
        AdController.Instance.isShow = false;
        AdController.Instance.isSuccess = true;
        
        if (AdController.Instance.onAdShowEnd != null)
            AdController.Instance.onAdShowEnd();


        rewardView.LoadAd(rewardRequest);
    }

    void HandlerRewardFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        Debug.Log(args.Message);
        UIPopupManager.ShowOKPopup("광고", "광고 불러오기를 실패하였습니다\n다시 시도해주세요", null);
    }

    public void ShowBannerAd()
    {
        bannerView.Show();
    }

    public void DestroyBannerAd()
    {
        Debug.Log("BANNER DESTROY!!!!!!!!!!!");
        bannerView.Destroy();        
    }

    
}
