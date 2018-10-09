using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
/// <summary> 모든 배틀 그룹에서 공용으로 사용하는 몬스터 풀을 관리하는 클래스 </summary>
public class actorPool : MonoBehaviour {

    public static actorPool Instance;
   
    /// <summary> 몬스터 딕셔너리 키값 모은 리스트 </summary>
    public List<string> monsterKeyList { get; private set; }
    /// <summary> 보스 딕셔너리 키값 모은 리스트 </summary>
    public List<string> bosskeyList { get; private set; }

    static public List<string> monsterIDListGrade1 = new List<string>();

    static public List<string> monsterIDListGrade2 = new List<string>();

    static public List<string> monsterIDListGrade3 = new List<string>();

    static public List<string> monsterIDList = new List<string>();


    public List<HeroBaseData> heroBaseDataList { get; set; }

    void Awake()
    {
        Instance = this;    
    }

    public bool isInitialization { get; private set; }
    IEnumerator Start ()
    {
        isInitialization = false;
        while (!HeroManager.Instance)
            yield return null;

        heroBaseDataList = HeroManager.heroBaseDataDic.Values.ToList().FindAll(x=>x.useForMonster);

        monsterKeyList = new List<string>();
        bosskeyList = new List<string>();
        for (int i = 0; i < heroBaseDataList.Count; i++)
        {
            HeroBaseData data = heroBaseDataList[i];

            if (data.grade == 1)
                monsterIDListGrade1.Add(data.id);
            else if (data.grade == 2)
                monsterIDListGrade2.Add(data.id);
            else if (data.grade == 3)
                monsterIDListGrade3.Add(data.id);

            //if (!data.useForMonster)
            //    continue;
            //if(keyList[i].EndsWith("_Mon"))
            {
                if (data.grade < 3)
                {
                    monsterKeyList.Add(data.id);
                    // 잡몹
                }
                else
                {
                    bosskeyList.Add(data.id);
                    // 보스
                }
            }            
        }

        isInitialization = true;
    }

    /// <summary> 몬스터 세팅부 </summary>
    public IEnumerator GetActor(string monsterID, System.Action<BattleHero> result)
    {
        HeroBaseData baseData = HeroManager.heroBaseDataDic[monsterID];

        string monsterObjName = baseData.prefab;

        //풀링 안되어 있으면 함.
        GameObject obj = Battle.GetObjectInPool(monsterObjName);
        if (!obj)
        {
            yield return AssetLoader.Instance.InstantiateGameObjectAsync(baseData.assetBundle, baseData.prefab, x => obj = x);

            //obj = AssetLoader.InstantiateGameObject(baseData.assetBundle, baseData.prefab);
            
            obj.name = monsterObjName;

            Battle.AddObjectToPool(obj);

        }

        if (!obj)
            result(null);
            
            //return null;

        obj.transform.SetParent(actorPool.Instance.transform);

        BattleHero battleHero = obj.GetComponent<BattleHero>();

        if(!battleHero)
            Debug.LogWarning("Cannot get hero from pool. id: " + monsterID);

        result(battleHero);

        //return battleHero;
    }
}
