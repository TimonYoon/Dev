using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using LitJson;

public class NicknameManager : MonoBehaviour {

    public static NicknameManager Instance;
    
    [HideInInspector]
    public List<string> slangList
    {
        get { return GameDataManager.slangList; }
    }
    
    public ShopData tempShopData;

    private void Awake()
    {
        Instance = this;
    }

    void Start ()
    {
        UIShopProductSlot.onClickItemButtonCallback += ShowNicknamePanel;
    }

    private void OnDisable()
    {
        UIShopProductSlot.onClickItemButtonCallback -= ShowNicknamePanel;
    }

    void ShowNicknamePanel(ShopData shopData)
    {
        if (shopData.category != "nickname")
            return;

        if (MoneyManager.GetMoney(MoneyType.ruby).value - int.Parse(shopData.price) < 0 && User.Instance.changeNickname != 0)
        {
            UIPopupManager.ShowOKPopup("구매 실패", shopData.costType + "가 부족합니다", null);
            return;
        }

        tempShopData = shopData;

        StartCoroutine(ShowNicknameChangeScene());
    }

    IEnumerator ShowNicknameChangeScene()
    {
        string assetBundle = "scene/nicknamechange";
        string sceneName = "NicknameChange";
        bool isAdditive = true;

        yield return StartCoroutine(ShowScene(assetBundle, sceneName, isAdditive));
    }

    IEnumerator ShowScene(string assetBundle, string sceneName, bool isAdditive = true)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.isLoaded)
        {
            while (!AssetLoader.Instance)
                yield return null;

            yield return StartCoroutine(AssetLoader.Instance.LoadLevelAsync(assetBundle, sceneName, isAdditive));

        }
        yield break;
    }

}
