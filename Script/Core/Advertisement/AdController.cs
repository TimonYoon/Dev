using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AdController : MonoBehaviour {

    enum AdType { Ads = 0, Admob, Vungle, };

    public AdsManager adsManager;
    public AdmobManager admobManager;
    public VungleAdManager vungleManager;

    public static AdController Instance;

    AdType adType = AdType.Ads;

    public bool isShow { get;  set; }
    public bool isSuccess { get;  set; }
    public bool isFailed;
#if UNITY_EDITOR
    public bool isPayedUser;
#endif
#if !UNITY_EDITOR
    public bool isPayedUser { get; set; }
#endif

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    

    //결제시 카메라 리사이즈를 위한 콜백
    public delegate void DeleteBannerCallback();
    public DeleteBannerCallback onResizeCamera;

    //광고 보기가 끝났을때를 알리는 콜백
    public delegate void AdShowEndCallback();
    public AdShowEndCallback onAdShowEnd;

    public void ShowRewardAD()
    {
        if (isShow)
            return;

        if (isSuccess)
            isSuccess = false;

        if (isFailed)
            isFailed = false;

        isShow = true;

        if(adType == AdType.Ads)
        {
            adType = AdType.Admob;
            adsManager.ShowAdsRewardAd();
        }
        else if (adType == AdType.Admob)
        {
            adType = AdType.Vungle;
            admobManager.ShowAdmobRewardAD();
        }
        else if(adType == AdType.Vungle)
        {
            adType = AdType.Ads;
            vungleManager.ShowVungleAD();
        }
    }

    

    public void DeleteBanner()
    {
        admobManager.DestroyBannerAd();
        onResizeCamera();
    }

    public IEnumerator CheckPayedPlayer()
    {
        string php = "Receipt.php";
        WWWForm form = new WWWForm();
        form.AddField("type", 5);
        form.AddField("userID", User.Instance.userID, System.Text.Encoding.UTF8);
        form.AddField("platformID", Social.localUser.id, System.Text.Encoding.UTF8);
        string result = "";
        yield return StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(php, form, x => result = x));
        if (!string.IsNullOrEmpty(result) && result == "1")
        {
            isPayedUser = true;
        }
        else
        {
            isPayedUser = false;
        }
    }

}
