using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using LitJson;
/// <summary> 내정에서 영웅 배치 부분 </summary>
public class TerritoryDeployHero : MonoBehaviour {

    public static TerritoryDeployHero Instance;

    private void Awake()
    {
        Instance = this;
    }

    private IEnumerator Start()
    {
        saveKey = "_DeployHero_" + User.Instance.userID;

        while (!TerritoryManager.Instance)
            yield return null;

        InitTerritoryDeployHero();
    }

    /// <summary> 배치 부서 리스트 </summary>
    public List<string> keys { get; private set; }

    public string saveKey;

  
    /// <summary> 각 부서에 배치된 영웅 ID </summary>
    public Dictionary<string, List<string>> deployHeroListDic { get; private set; }
    void InitTerritoryDeployHero()
    {
       
        keys = GameDataManager.itemDic.Keys.ToList();
        deployHeroListDic = new Dictionary<string, List<string>>();
        for (int i = 0; i < keys.Count; i++)
        {
            if(PlayerPrefs.HasKey(keys[i] + saveKey))
            {

                string data = PlayerPrefs.GetString(keys[i] + saveKey);
                List<string> deployHeroIDList = JsonMapper.ToObject<List<string>>(new JsonReader(data));
                for (int j = 0; j < deployHeroIDList.Count; j++)
                {
                    HeroManager.heroDataDic[deployHeroIDList[j]].placeID = keys[i];
                }
                
                deployHeroListDic.Add(keys[i], deployHeroIDList);
            }
        }
    }

    public delegate void OnChangedDeployHeroData();
    /// <summary> 영웅 배치 변경 </summary>
    public OnChangedDeployHeroData onChangedDeployHeroData;

    /// <summary> 배치 영웅 확정 </summary>
    public void DeployHero(string departmentID ,List<string> heroIDList)
    {
        if(heroIDList.Count == 0)
        {
            if (!deployHeroListDic.ContainsKey(departmentID) || deployHeroListDic[departmentID].Count ==0)
                return;
            
        }
        if(deployHeroListDic.ContainsKey(departmentID))
        {
            for (int i = 0; i < deployHeroListDic[departmentID].Count; i++)
            {
                HeroManager.heroDataDic[deployHeroListDic[departmentID][i]].placeID = string.Empty;
            }
        }
        

        for (int i = 0; i < heroIDList.Count; i++)
        {
            if(!string.IsNullOrEmpty(HeroManager.heroDataDic[heroIDList[i]].placeID))
            {
                string key = HeroManager.heroDataDic[heroIDList[i]].placeID;
                deployHeroListDic[HeroManager.heroDataDic[heroIDList[i]].placeID].Remove(heroIDList[i]);
                string _json = JsonMapper.ToJson(deployHeroListDic[HeroManager.heroDataDic[heroIDList[i]].placeID]);
                
                PlayerPrefs.SetString(key + saveKey, _json);
            }

            HeroManager.heroDataDic[heroIDList[i]].placeID = departmentID;
        }

        if(deployHeroListDic.ContainsKey(departmentID))
        {
            deployHeroListDic[departmentID] = heroIDList;
        }
        else
        {
            deployHeroListDic.Add(departmentID, heroIDList);
        }
        string json = JsonMapper.ToJson(deployHeroListDic[departmentID]);
        PlayerPrefs.SetString(departmentID + saveKey, json);

        if (onChangedDeployHeroData != null)
            onChangedDeployHeroData();
    }
    




}
