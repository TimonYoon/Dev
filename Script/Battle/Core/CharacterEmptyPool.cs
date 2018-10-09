using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterEmptyPool : MonoBehaviour {

    public static CharacterEmptyPool Instance;

    void Awake()
    {
        Instance = this;    
    }

    /// <summary> 세팅부 </summary>
    public IEnumerator GetHero(string heroID, System.Action<GameObject> result)
    {
        HeroBaseData baseData = HeroManager.heroBaseDataDic[heroID];

        string prefabName = baseData.prefab;


        GameObject go = SelectHero(prefabName);
        if (!go)
        {
            yield return AssetLoader.Instance.InstantiateGameObjectAsync(baseData.assetBundle, baseData.prefab + "_Empty", x => go = x);

            go.transform.SetParent(transform);
            go.name = prefabName;

            UpdateHeroPool(prefabName,go);
        }

        if (!go)
        {
            result(null);
        }
        else
        {
            result(go);
        }

    }


    public Dictionary<string, List<GameObject>> heroPoolDic = new Dictionary<string, List<GameObject>>();
    GameObject SelectHero(string prefabName)
    {
        GameObject go = null;
        if (heroPoolDic.ContainsKey(prefabName))
        {
            List<GameObject> list = heroPoolDic[prefabName];
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].activeSelf == false)
                {
                    go = list[i];
                    break;
                }
            }
        }
        return go;
    }
    void UpdateHeroPool(string prefabName, GameObject go)
    {
        if(heroPoolDic.ContainsKey(prefabName))
        {
            heroPoolDic[prefabName].Add(go);
        }
        else
        {
            List<GameObject> list = new List<GameObject>();
            list.Add(go);
            heroPoolDic.Add(prefabName, list);
        }
    }
}
