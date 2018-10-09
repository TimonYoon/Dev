using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LitJson;

public class CouponManager : MonoBehaviour {

    public static CouponManager Instance;

    [SerializeField]
    List<InputField> couponInputList = new List<InputField>();

    List<string> couponNum = new List<string>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    private void OnEnable()
    {
        WebServerConnectManager.onWebServerResult += OnWebServerResult;
    }

    private void OnDisable()
    {
        WebServerConnectManager.onWebServerResult -= OnWebServerResult;
    }

    public void InitCouponUI()
    {
        for (int i = 0; i < couponInputList.Count; i++)
        {
            couponInputList[i].text = string.Empty;
        }
    }

    public void OnClickApplyCouponButton()
    {
        couponNum.Clear();

        for (int i = 0; i < couponInputList.Count; i++)
        {
            if(couponInputList[i].text.Length < 4)
            {
                UIPopupManager.ShowOKPopup("쿠폰 입력 오류", "잘못된 쿠폰코드를 입력하셨습니다", null);
                return;
            }

            couponNum.Add(couponInputList[i].text);
        }

        StartCoroutine(ApplyCoupon());
    }

    void OnWebServerResult(Dictionary<string, object> resultDataDic)
    {
        if (resultDataDic.ContainsKey("coupon"))
        {
            LoadingManager.Show();

            JsonReader json = new JsonReader(JsonMapper.ToJson(resultDataDic["coupon"]));
            JsonData jsonData = JsonMapper.ToObject(json);

            string itemID = jsonData["itemID"].ToStringJ();

            UIPopupManager.ShowOKPopup("쿠폰 사용 완료", GameDataManager.moneyBaseDataDic[itemID].name + "이(가) 우편으로 지급되었습니다", null);
        }
    }


    IEnumerator ApplyCoupon()
    {
        LoadingManager.Show();

        string php = "Coupon.php";
        WWWForm form = new WWWForm();
        form.AddField("userID", User.Instance.userID);
        form.AddField("couponNum1", couponNum[0]);
        form.AddField("couponNum2", couponNum[1]);
        form.AddField("couponNum3", couponNum[2]);
        form.AddField("couponNum4", couponNum[3]);
        form.AddField("type", 1);
        string result = string.Empty;
        yield return StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(php, form, x => result = x));

        LoadingManager.Close();

        if(!string.IsNullOrEmpty(result))
        {
            if(result == "1")
            {
                UIPopupManager.ShowOKPopup("쿠폰입력 오류", "잘못된 쿠폰코드입니다", null);
                yield break;
            }
            else if(result == "2")
            {
                UIPopupManager.ShowOKPopup("쿠폰입력 오류", "이미 사용된 쿠폰코드입니다", null);
                yield break;
            }
        }
        else
        {
            yield return StartCoroutine(MailManager.MailDataInitCoroutine());
        }
    }
}
