using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using System.Linq;
//using GameData;

/// <summary> 클라이언트용 정적데이타들 초기화 담당 </summary>
public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance;

    static public bool isInitialized = false;
    
    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public static IEnumerator Init()
    {
        Debug.Log("게임 데이타 초기화 시작");
        //퀘스트 데이타 초기화
        if(SceneLogin.Instance)
            SceneLogin.Instance.tipMessageText.text = "세상을 구성하는 중..";
        yield return Instance.StartCoroutine(Instance.InitStatBaseData());

        yield return Instance.StartCoroutine(Instance.InitBattleQuestBaseData());

        yield return Instance.StartCoroutine(Instance.InitBuffBaseData());

        yield return Instance.StartCoroutine(Instance.InitDungeonBaseData());

        yield return Instance.StartCoroutine(Instance.InitDayDungeonBaseData());

        yield return Instance.StartCoroutine(Instance.InitMoneyBaseData());

        yield return Instance.StartCoroutine(Instance.InitShopGameData());


        // =======

        yield return Instance.StartCoroutine(Instance.LoadSlangWordList());
        if(LocalizationManager.Instance)
            yield return Instance.StartCoroutine(LocalizationManager.coParsingLanguageData(LocalizationManager.language));

        // =======
        if (SceneLogin.Instance)
            SceneLogin.Instance.tipMessageText.text = "왕국을 설정 중..";
        yield return Instance.StartCoroutine(Instance.InitItems());

        yield return Instance.StartCoroutine(Instance.InitProductionLineBaseData());

        //내정 스킬
        yield return Instance.StartCoroutine(Instance.InitTerritorySkillData());

        //스킬 데이타 초기화
        yield return Instance.StartCoroutine(Instance.InitSkillData());

        //영웅 데이타
        yield return Instance.StartCoroutine(Instance.InitHeroBaseData());


        yield return Instance.StartCoroutine(Instance.InitPlaceBaseData());
        
        //yield return Instance.StartCoroutine(Instance.InitTrainingBaseData());

        


        //Todo: 그 외 이것 저것들
        Debug.Log("게임데이터 매니저 초기화완료");

        isInitialized = true;
    }

    /// <summary> 생산 라인 베이스 데이타 모임 </summary>
    public static Dictionary<string,ProductionLineBaseData> productionLineBaseDataDic { get; private set; }

    /// <summary> 내정 스킬 모음 딕셔너리 </summary>
    public static Dictionary<string,TerritorySkillData> territorySkillDataDic { get; private set; }

    /// <summary> 전체 던전 정적 데이터 모음 딕셔너리</summary>
    public static Dictionary<string, DungeonBaseData> dungeonBaseDataDic { get; private set; }

    /// <summary> 요일 던전 데이타 모음 딕셔너리 </summary>
    public static Dictionary<string,DayDungeonBaseData> dayDungeonBaseDataDic { get; private set; }

    static public Dictionary<string, SkillData> skillDataDic = new Dictionary<string, SkillData>();

    /// <summary> 버프 관련 정적 데이터 모음 딕셔너리 </summary>
    public static Dictionary<string, BuffBaseData> buffBaseDataDic { get; private set; }

    /// <summary> 버프 관련 정적 데이터 모음 딕셔너리 </summary>
    public static Dictionary<string, ArtifactBaseData> ArtifactBaseDataDic { get; private set; }

    /// <summary> 전투 퀘스트 정적 데이터 모음 딕셔너리 </summary>
    public static Dictionary<string, BattleQuestBaseData> battleQuestBaseDataDic { get; private set; }

    //#######################################################

    /// <summary> 지역(영지) base Data</summary>
    public static Dictionary<string, PlaceBaseData> placeBaseDataDic { get; private set; }
    
    /// <summary> 자원 base Data</summary>
    public static Dictionary<string, KingdomManagement.Item> itemDic = new Dictionary<string, KingdomManagement.Item>();
    
    /// <summary> 수련 관련 수치 모음 딕셔너리 (key : HeroData.trainingTypeID) </summary>
    //public static Dictionary<string, TrainingTypeBaseData> trainingTypeBaseDataDic { get; private set; }

    /// <summary> 상점 데이터 모음 딕셔너리 (key : ShopData.id) </summary>
    public static Dictionary<string, ShopGameData> shopGameDataDic { get; private set; }

    /// <summary> 히어로 데이타 </summary>
    public static Dictionary<string, HeroBaseData> heroBaseDataDic { get; private set; }

    /// <summary> 스탯 데이타 </summary>
    public static Dictionary<string, StatBaseData> statBaseDataDic = new Dictionary<string, StatBaseData>();

    public static List<string> slangList = new List<string>();

    IEnumerator InitHeroBaseData()
    {
        JsonData clientJsonData = null;
        string bundle = "data/hero";
        string assetName = "Hero";
        yield return StartCoroutine(AssetLoader.LoadJsonData(bundle, assetName, x => clientJsonData = x));
        heroBaseDataDic = new Dictionary<string, HeroBaseData>();
       
        for (int i = 0; i < clientJsonData.Count; i++)
        {
            HeroBaseData heroBaseData = new HeroBaseData(clientJsonData[i]);
            heroBaseDataDic.Add(heroBaseData.id, heroBaseData);
        }
    }

    
    IEnumerator LoadSlangWordList()
    {
        JsonData json = null;

        yield return StartCoroutine(AssetLoader.LoadJsonData("data/slang", "SlangWordList", x => json = x));
        for (int i = 0; i < json.Count; i++)
        {
            slangList.Add(JsonParser.ToString(json[i]["word"].ToString()));
        }
    }

    IEnumerator InitProductionLineBaseData()
    {
        JsonData json = null;
        yield return StartCoroutine(AssetLoader.LoadJsonData("json", "ProductionLineBaseData", x => json = x));
        productionLineBaseDataDic= new Dictionary<string, ProductionLineBaseData>();
        for (int i = 0; i < json.Count; i++)
        {
            ProductionLineBaseData data = new ProductionLineBaseData(json[i]);
            productionLineBaseDataDic.Add(data.id, data);
        }
    }

    /// <summary> 던전 정보 초기화 </summary>
    IEnumerator InitDungeonBaseData()
    {
        JsonData json = null;

        // 던전 데이터 불러온다.
        yield return StartCoroutine(AssetLoader.LoadJsonData("data/dungeonlist", "DungeonList", x => json = x));
        dungeonBaseDataDic = new Dictionary<string, DungeonBaseData>();
        for (int i = 0; i < json.Count; i++)
        {
            DungeonBaseData dungeon = new DungeonBaseData(json[i]);
            dungeonBaseDataDic.Add(dungeon.dungeonID, dungeon);
        }

    }

    /// <summary> 던전 정보 초기화 </summary>
    IEnumerator InitDayDungeonBaseData()
    {
        JsonData json = null;

        // 던전 데이터 불러온다.
        yield return StartCoroutine(AssetLoader.LoadJsonData("data/daydungeon", "DayDungeon", x => json = x));
        dayDungeonBaseDataDic = new Dictionary<string, DayDungeonBaseData>();
        for (int i = 0; i < json.Count; i++)
        {
            DayDungeonBaseData dungeon = new DayDungeonBaseData(json[i]);
            dayDungeonBaseDataDic.Add(dungeon.id, dungeon);
        }

    }


    IEnumerator InitBuffBaseData()
    {
        JsonData json = null;

        if(buffBaseDataDic == null)
            buffBaseDataDic = new Dictionary<string, BuffBaseData>();

        buffBaseDataDic.Clear();

        yield return StartCoroutine(AssetLoader.LoadJsonData("data/buff", "Buff", x => json = x));
        for (int i = 0; i < json.Count; i++)
        {
            BuffBaseData buffBaseData = new BuffBaseData(json[i]);
            buffBaseDataDic.Add(buffBaseData.id, buffBaseData);
        }


        ArtifactBaseDataDic = new Dictionary<string, ArtifactBaseData>();
        yield return StartCoroutine(AssetLoader.LoadJsonData("json", "Artifact", x => json = x));
        for (int i = 0; i < json.Count; i++)
        {
            ArtifactBaseData data = new ArtifactBaseData(json[i]);
            ArtifactBaseDataDic.Add(data.id, data);
        }
    }

    IEnumerator InitStatBaseData()
    {
        JsonData json = null;

        statBaseDataDic.Clear();

        yield return StartCoroutine(AssetLoader.LoadJsonData("data/stat", "Stat", x => json = x));
        for (int i = 0; i < json.Count; i++)
        {
            StatBaseData baseData = new StatBaseData(json[i]);
            statBaseDataDic.Add(baseData.id, baseData);
        }
    }

    IEnumerator InitBattleQuestBaseData()
    {
        JsonData json = null;

        if (battleQuestBaseDataDic == null)
            battleQuestBaseDataDic = new Dictionary<string, BattleQuestBaseData>();

        battleQuestBaseDataDic.Clear();

        yield return StartCoroutine(AssetLoader.LoadJsonData("data/battlequest", "BattleQuest", x => json = x));
        for (int i = 0; i < json.Count; i++)
        {
            BattleQuestBaseData baseData = new BattleQuestBaseData(json[i]);
            battleQuestBaseDataDic.Add(baseData.id, baseData);
        }
    }

    IEnumerator InitTerritorySkillData()
    {
        JsonData jData = null;
        territorySkillDataDic = new Dictionary<string, TerritorySkillData>();

        yield return StartCoroutine(AssetLoader.LoadJsonData("data/territoryskill", "TerritorySkill", x => jData = x));
        if (jData == null)
        {
            Debug.LogWarning("Failed to load TerritorySkill json data");
            yield break;
        }

        for (int i = 0; i < jData.Count; i++)
        {
            TerritorySkillData skillData = new TerritorySkillData(jData[i]);

            territorySkillDataDic.Add(skillData.id,skillData);
        }
    }


    IEnumerator InitSkillData()
    {
        JsonData jData = null;
        yield return StartCoroutine(AssetLoader.LoadJsonData("data/skill"/* "json"*/, "Skill", x => jData = x));
        if (jData == null)
        {
            Debug.LogWarning("Failed to load skill json data");
            yield break;
        }

        for (int i = 0; i < jData.Count; i++)
        {
            SkillData skillData = new SkillData(jData[i]);

            skillDataDic.Add(skillData.id, skillData);
        }
    }

    IEnumerator InitItems()
    {
        JsonData json = null;
        yield return StartCoroutine(AssetLoader.LoadJsonData("data/item", "Item", x => json = x));
        for (int i = 0; i < json.Count; i++)
        {
            KingdomManagement.Item item = new KingdomManagement.Item(json[i]);
            itemDic.Add(item.id, item);
        }
    }
    
    IEnumerator InitPlaceBaseData()
    {
        JsonData json = null;
        placeBaseDataDic = new Dictionary<string, PlaceBaseData>();
        yield return StartCoroutine(AssetLoader.LoadJsonData("data/place", "PlaceBaseData", x => json = x));
        for (int i = 0; i < json.Count; i++)
        {
            PlaceBaseData data = new PlaceBaseData(json[i]);
            placeBaseDataDic.Add(data.id, data);
        }
    }

    ///// <summary> 수련 타입 정보 초기화 </summary>
    //IEnumerator InitTrainingBaseData()
    //{
    //    JsonData json = null;
    //    trainingTypeBaseDataDic = new Dictionary<string, TrainingTypeBaseData>();
    //    yield return StartCoroutine(AssetLoader.LoadJsonData("json", "TrainingTypeBaseData", x => json = x));
    //    for (int i = 0; i < json.Count; i++)
    //    {
    //        TrainingTypeBaseData trainingTypeBaseData = new TrainingTypeBaseData(json[i]);
    //        trainingTypeBaseDataDic.Add(trainingTypeBaseData.trainingTypeID, trainingTypeBaseData);
    //    }
    //}
    

    /// <summary> 재화 관려 베이스 데이터 </summary>
    public static Dictionary<string, MoneyBaseData> moneyBaseDataDic { get; private set; }

    IEnumerator InitMoneyBaseData()
    {
        JsonData json = null;
        moneyBaseDataDic = new Dictionary<string, MoneyBaseData>();
        yield return StartCoroutine(AssetLoader.LoadJsonData("data/money", "MoneyBaseData", x => json = x));
        for (int i = 0; i < json.Count; i++)
        {
            MoneyBaseData data = new MoneyBaseData(json[i]);
            moneyBaseDataDic.Add(data.id, data);
        }

    }

    IEnumerator InitShopGameData()
    {
        JsonData json = null;
        shopGameDataDic = new Dictionary<string, ShopGameData>();
        yield return StartCoroutine(AssetLoader.LoadJsonData("data/shop", "Shop", x => json = x));
        for (int i = 0; i < json.Count; i++)
        {
            ShopGameData data = new ShopGameData(json[i]);
            shopGameDataDic.Add(data.goodsID, data);
        }

    }
}
