using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleTestScene : MonoBehaviour {


    public static BattleTestScene Instance;
    void Awake()
    {
        Instance = this;    
    }

    IEnumerator Start()
    {
        yield return StartCoroutine(AssetLoader.Instance.Initialize());
        yield return StartCoroutine(GameDataManager.Init());
    }

    Coroutine coroutine = null;
    void Update ()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            if (coroutine != null)
                return;
            coroutine = StartCoroutine(CreateBattleHero());
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            battleHeroList[heroIndex].skillList[0].Execute();
        }
	}
    public int heroIndex = 0;
    List<BattleHero> battleHeroList = new List<BattleHero>();


    public Transform parent;


    IEnumerator CreateBattleHero()
    {

        BattleHero battleHero = null;
        yield return StartCoroutine(GetHero(heroID, x => battleHero = x));
        battleHero.gameObject.SetActive(true);

        HeroData hero = CreateHeroData();
        battleHero.team = BattleUnit.Team.Red;
        battleHero.Init(null, hero);
        battleHero.ReGen();
        battleHeroList.Add(battleHero);

        coroutine = null;
    }

    public string heroID = "AncientWeapon_01_Hero";
    HeroData CreateHeroData()
    {
        HeroBaseData baseData = GameDataManager.heroBaseDataDic[heroID];
        HeroData data = new HeroData(baseData);

        return data;
    }

    /// <summary> 캐릭터 생성 </summary>
    public IEnumerator GetHero(string monsterID, System.Action<BattleHero> result)
    {
        HeroBaseData baseData = HeroManager.heroBaseDataDic[monsterID];

        string monsterObjName = baseData.prefab;

        //풀링 안되어 있으면 함.
        GameObject obj = Battle.GetObjectInPool(monsterObjName);
        if (!obj)
        {
            yield return AssetLoader.Instance.InstantiateGameObjectAsync(baseData.assetBundle, baseData.prefab, x => obj = x);

            obj.name = monsterObjName;
        }

        if (!obj)
            result(null);

        //return null;

        obj.transform.SetParent(parent);

        BattleHero battleHero = obj.GetComponent<BattleHero>();

        if (!battleHero)
            Debug.LogWarning("Cannot get hero from pool. id: " + monsterID);

        result(battleHero);

        //return battleHero;
    }
}
