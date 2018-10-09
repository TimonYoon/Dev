using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PackageManager : MonoBehaviour {

    public static PackageManager Instance;

    void Awake()
    {
        Instance = this;
    }

    // 패키지 슬롯 UI변경시 추가 수정
    public static IEnumerator Init ()
    {

        while (!User.Instance)
            yield return null;
        
        yield return Instance.StartCoroutine(Instance.TakeFixedChargeItem());

	}
    
	

    IEnumerator TakeFixedChargeItem()
    {
        WWWForm form = new WWWForm();
        form.AddField("userID", User.Instance.userID, System.Text.Encoding.UTF8);
        form.AddField("type", 1);
        string php = "Package.php";
        string result = "";
        yield return StartCoroutine(WebServerConnectManager.Instance.WWWCoroutine(php, form, x => result = x));
    }

}
