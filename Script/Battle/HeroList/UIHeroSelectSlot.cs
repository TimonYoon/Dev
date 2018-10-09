using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

/// <summary> 전투 준비 상황에서 영웅 선택시 선택된 영웅을 보여주는 슬롯 </summary>
public class UIHeroSelectSlot : MonoBehaviour
{

    /// <summary> 영웅 고유값 </summary>
    public string id { get; private set; }

    public int index
    {
        get
        {
            return UIBattlePreparation.heroSelectSlotList.IndexOf(this);
        }
    }

    public Transform heroPivot;

    public BattleHero battleHero { get; private set; }

    public GameObject heroObj;

    public void AssignHero(string id, GameObject go)
    {
        //if (heroObj != null)
        //{
        //    heroObj.SetActive(false);
        //    heroObj.transform.SetParent(CharacterEmptyPool.Instance.transform);
        //    id = string.Empty;
        //    heroObj = null;
        //}

        this.id = id;
        heroObj = go;

        if (heroObj != null)
        {
            heroObj.transform.position = heroPivot.position;
            heroObj.gameObject.SetActive(true);

            heroObj.transform.SetParent(heroPivot);
            heroObj.transform.localScale = Vector3.one;
            
        }

    }


    public void OnClickHeroSlotButton()
    {
        if (string.IsNullOrEmpty(id))
            return;

        HeroData data = HeroManager.heroDataDic[id];
        if (data == null)
            return;
        
        UIBattlePreparation.RemoveHero(data);
    }

    public void Dispose()
    {
        if(heroObj != null)
        { 
            heroObj.SetActive(false);
            heroObj.transform.SetParent(CharacterEmptyPool.Instance.transform);
            id = string.Empty;
            heroObj = null;
        }                 
    }
}
