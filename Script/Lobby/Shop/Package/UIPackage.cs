using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIPackage : MonoBehaviour {


    [SerializeField]
    GameObject packageSlotPrefab;

    [SerializeField]
    GridLayoutGroup packageScrollViewContect;
    [SerializeField]
    GameObject scrollView;
    

    ShopData packageShopData;

    private void OnEnable()
    {
        SceneLobby.Instance.OnChangedMenu += OnChangedMenu;
        ShopDataController.Instance.onChangedShowShop += ShowPackage;
        ShopDataController.Instance.OnRemove += RemoveSlot;

        ShowProductPanel(ShopDataController.shopPackageDataList);
    }

    private void OnDisable()
    {
        ShopDataController.Instance.onChangedShowShop -= ShowPackage;
        ShopDataController.Instance.OnRemove -= RemoveSlot;
        SceneLobby.Instance.OnChangedMenu -= OnChangedMenu;
    }

    void OnChangedMenu(LobbyState state)
    {
        if (state != LobbyState.Shop || SceneLobby.currentSubMenuState != SubMenuState.PackageBox)
            Close();
    }

    void RemoveSlot(string id)
    {
        for (int i = 0; i < objectList.Count; i++)
        {
            UIPackageSlot slot = objectList[i].GetComponent<UIPackageSlot>();
            if (slot.shopProductSlotData.id == id)
            {
                Destroy(objectList[i].gameObject);
                objectList.Remove(objectList[i]);
            }

        }
    }

    public void OnClickCloseButton()
    {
        //Debug.Log("여기!!");
        //SceneLobby.Instance.SceneChange(LobbyState.Shop);

        Close();
    }
    void Close()
    {
        SceneManager.UnloadSceneAsync("Package");
    }

    void ShowPackage(ShopType type)
    {
        ShowProductPanel(ShopDataController.shopPackageDataList);
    }

    void ShowProductPanel(List<ShopData> _test = null)
    {
        scrollView.SetActive(false);
        int count = 0;
        if (_test != null)
        {
            count = _test.Count;
        }
        ObjectPool(count, packageSlotPrefab);

        for (int i = 0; i < count; i++)
        {
            objectList[i].GetComponent<UIPackageSlot>().InitProductSlotData(_test[i]);
            objectList[i].gameObject.SetActive(true);
        }
        SizeControl(count);

        scrollView.SetActive(true);
    }


    List<Transform> objectList = new List<Transform>();
    void ObjectPool(int count, GameObject prefab)
    {
        RectTransform content = packageScrollViewContect.GetComponent<RectTransform>();
        int poolCount = 0;
        if (objectList.Count > 0)
        {
            for (int i = 0; i < objectList.Count; i++)
            {
                objectList[i].gameObject.SetActive(false);
            }
            poolCount = objectList.Count;
        }


        if (count > poolCount)
        {
            for (int i = 0; i < (count - poolCount); i++)
            {
                GameObject slot = Instantiate(prefab);
                slot.transform.SetParent(content, false);
                slot.SetActive(false);
                objectList.Add(slot.transform);
                ShopDataController.Instance.packageSlotList.Add(slot.GetComponent<UIPackageSlot>());
            }
        }
    }

    void SizeControl(float count)
    {
        packageScrollViewContect.GetComponent<RectTransform>().sizeDelta = new Vector2(packageScrollViewContect.GetComponent<RectTransform>().sizeDelta.x, ((packageScrollViewContect.spacing.y + packageScrollViewContect.cellSize.y) * (count)));

    }
}
