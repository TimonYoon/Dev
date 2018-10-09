using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> 클릭시 원하는 상점으로 바로 갈수 있게 하는 클래스 </summary>
public class UIOnClickDirectShop : MonoBehaviour {


    [SerializeField]
    ShopType ShopType;

    public void OnClickDirectShop()
    {
        SceneLobby.Instance.ShowShop(ShopType);
    }
}
