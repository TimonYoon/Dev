using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KingdomManagement;
using LitJson;
using System.Linq;
using System.Reflection;
using CodeStage.AntiCheat.ObscuredTypes;

public class PlaceData
{
    public PlaceData(string _placeID)
    {
        skillCT = new TerritorySkillController(this);

        placeID = _placeID;
        placeBaseData = GameDataManager.placeBaseDataDic[placeID];
        product = GameDataManager.itemDic[placeBaseData.productID];

        heroList.onAdd += OnAddHero;
        heroList.onRemovePost += OnRemoveHero;

        TerritoryManager.onAddPlace += CalculatePlaceBuffValue;
        TerritoryManager.onChangedPlaceData += CalculatePlaceBuffValue;

        CalculatePlaceBuffValue();

        float formula = 0;
        float.TryParse(placeBaseData.formula, out formula);
        power = formula * placeLevel;

        if (ObscuredPrefs.HasKey(placeID + saveKey))
        {
            string _data = ObscuredPrefs.GetString(placeID + saveKey);
            if (string.IsNullOrEmpty(_data))
            {
                return;
            }
            List<string> deployHeroIDList = JsonMapper.ToObject<List<string>>(new JsonReader(_data));

            for (int i = 0; i < deployHeroIDList.Count; i++)
            {
                HeroData heroData = HeroManager.heroDataDic[deployHeroIDList[i]];
                heroData.onChangedValue += OnChangedHeroData;
                heroData.placeID = placeID;
                heroList.Add(heroData);
            }
        }
       
    }
    void OnChangedHeroData(PropertyInfo p)
    {
        if (p.Name == "enhance" || p.Name == "rebirth" || p.Name == "level")
        {
            CalculateProductinAmount();
            CalculatePlaceBuffValue();
            //CalculateHeroSkillCollectPower();
        }
    }

    void CalculatePlaceBuffValue()
    {
        placeBuffValue = 0;
        for (int i = 0; i < TerritoryManager.Instance.myPlaceList.Count; i++)
        {
            PlaceData placData = TerritoryManager.Instance.myPlaceList[i];

            if (placData.placeBaseData.type == "Collect")
            {
                if (placData.placeBaseData.fillter == product.id)
                {
                    placeBuffValue += placData.power;
                }
                else if (placData.placeBaseData.fillter == "all")
                {
                    placeBuffValue += placData.power;
                }
            }

            if (placData.placeBaseData.type == "CategoryCollect")
            {
                if (placData.placeBaseData.fillter == product.category)
                {
                    placeBuffValue += placData.power;
                }
                else if (placData.placeBaseData.fillter == "all")
                {
                    placeBuffValue += placData.power;
                }
            }
        }
        placeBuffValue = finalProductionAmount * (placeBuffValue * 0.01);
    }

    /// <summary> 영웅 스킬중 생산력 올려주는 스킬 모음 </summary>
    public double heroSkillCollectPower { get { return skillCT.heroSkillCollectPower; } }

    /// <summary> 영웅 스킬중 시민이 주는 경험치 증가량 </summary>
    public double heroSkillCitizenExpValue { get { return skillCT.heroSkillCitizenExpValue; } }
    /// <summary> 영웅 스킬중 필터별 시민이 주는 경험치 증가량 </summary>
    public Dictionary<string, double> heroSkillCitizenExpValueDic { get { return skillCT.heroSkillCitizenExpValueDic; } }

    /// <summary> 영웅 스킬중 시민이 주는 세금 증가량 </summary>
    public double heroSkillCitizenTaxValue { get { return skillCT.heroSkillCitizenTaxValue; } }
    /// <summary> 영웅 스킬중 필터별 시민이 주는 세금 증가량 </summary>
    public Dictionary<string, double> heroSkillCitizenTaxValueDic { get { return skillCT.heroSkillCitizenTaxValueDic; } }

    public TerritorySkillController skillCT;

    
    bool fillterChecker(TerritorySkillData skill)
    {
        bool result = true;
        if (product == null)
            return false;

        if (string.IsNullOrEmpty(skill.fillterCategory) == false)
            result = skill.fillterCategory == product.category;

        if (string.IsNullOrEmpty(skill.fillterItem) == false)
            result = skill.fillterItem == product.id;

        return result;

    }

  


    double CalculateHeroSkillPower(HeroData hero, TerritorySkillData skill, int placeTier = 1)
    {
        string s = skill.formula;
        int formula = 0;
        int.TryParse(s, out formula);
        int level = (1 + (hero.rebirth * 100) + hero.enhance);

        double collectPower = formula * level * placeTier;
        return collectPower;
    }

    //double CalculateCollectPowerForPlaceTier(HeroData hero, TerritorySkillData skill, int placeTier = 0)
    //{
    //    string s = skill.formula;
    //    int formula = 0;
    //    int.TryParse(s, out formula);
    //    int level = (1 + (hero.rebirth * 100) + hero.enhance);

    //    double collectPower = formula * level * placeTier;
    //    return collectPower;
    //}

    double _placeBuffValue = 0d;
    public double placeBuffValue
    {
        get
        {
            return _placeBuffValue;//product.placeBuffValue * placeLevel;
        }
        set
        {
            _placeBuffValue = value;
        }
    }


    public SimpleDelegate onChangedState;

    PlaceState _placeState;
    public PlaceState placeState
    {
        get { return _placeState; }
        set
        {
            _placeState = value;
            if (onChangedState != null)
                onChangedState();
        }
    }

    string saveKey = "_DeployHero_" + User.Instance.userID;

    public string placeID { get; private set; }

    public SimpleDelegate onChangedPlaceLevel;

    int _placeLevel = 1;
    public int placeLevel
    {
        get { return _placeLevel; }
        set
        {
            bool isChack = _placeLevel != value;
            _placeLevel = value;

            if (isChack && onChangedPlaceLevel != null)
                onChangedPlaceLevel();
            float formula = 0;
            float.TryParse(placeBaseData.formula, out formula);
            power = formula * placeLevel;
            CalculateProductinAmount();
        }
    }

    /// <summary> 영지 정적 데이터 </summary>
    public PlaceBaseData placeBaseData { get; private set; }


    


    /// <summary>  </summary>
    public double placeBuyCost
    {
        get
        {
            double d = 1000d * placeBaseData.placeTier;
            return d;
        }
    }

    public double placeUpgradeCost
    {
        get
        {
            double d = 1000d * System.Math.Pow((1 + placeBaseData.placeTier), placeLevel);
            return d;
        }
    }

    /// <summary> 배치된 영웅 </summary>
    public HeroData deployedHero = null;


    void OnAddHero(HeroData hero)
    {
        CalculateProductinAmount();

        //저장
    }

    void OnRemoveHero(HeroData hero)
    {
        CalculateProductinAmount();

        //저장
    }

    /// <summary> 진행률 </summary>
    public float progressValue { get; private set; }

    double _productionAmount = 0d;
    /// <summary> 제품의 기본 생산량과 영웅의 생산력에 의해 보정 받은 최종 생산량 </summary>
    public double finalProductionAmount
    {
        get
        {
            return _productionAmount;


        }
        set
        {
            bool isChanged = _productionAmount != value;

            _productionAmount = value;

            if (isChanged && onChangedProductionAmount != null)
            {
                //Debug.Log("final productionPower : " + finalProductionAmount);
                onChangedProductionAmount();
            }

        }
    }

   
    public SimpleDelegate onChangedProductionAmount;

    void CalculateProductinAmount()
    {
        //ApplyHeroSkill();
        
        double collectPower = 0f;
        if (heroList != null)
            collectPower = heroList.Sum(x => x.stats.GetValueOf(StatType.CollectPower));

        //Debug.Log(placeID + ", hero Count : " + heroList.Count + " , hero Power : " + collectPower);

        //Debug.Log("productionPower : " + productionPower);
        double baseValue = 0;

        if (product != null)
            baseValue = product.productionAmount;

        finalProductionAmount = baseValue * (1d + collectPower * 0.01d) * placeLevel;



        skillCT.CalculateHeroSkillPower();

        if(onChangedProductionAmount != null)
            onChangedProductionAmount();
        //finalProductionAmount = placeBuffValue;
    }

    /// <summary> 영지 특성 능력치 </summary>
    public double power { get; private set; }
    
    
    /// <summary> 생산 예약열 </summary>
    public Queue<IEnumerator> productionQueue = new Queue<IEnumerator>();



    /// <summary> 생산이 시작 또는 종료 될 때 발생 </summary>
    public SimpleDelegate onChangedProduceStatus;

    Coroutine _coroutineProduce = null;
    /// <summary> 이거 null 아니면 생산 시작 안 함. 생산 끝날 때 null로 초기화 </summary>
    public Coroutine coroutineProduce
    {
        get { return _coroutineProduce; }
        set
        {
            bool isChanged = _coroutineProduce != value;

            _coroutineProduce = value;

            if (isChanged && onChangedProduceStatus != null)
                onChangedProduceStatus();

            //Enqueue, Dequeue 할 때 걸어주는게 안전하긴 함..
        }
    }

    /// <summary> 생산될 때 콜백 </summary>
    public SimpleDelegate onProduce;
    /// <summary> 생산 </summary>
    public IEnumerator ProduceCoroutine()
    {
        float startTime = Time.time;

        //생산 시간 만큼 대기
        while (Time.time - startTime < productionTime)
        {
            progressValue = (Time.time - startTime) / productionTime;

            yield return null;
        }
        if (product != null)
        {
            //제품 생산
            Produce();

        }

        coroutineProduce = null;
        progressValue = 0f;

    }

    void Produce()
    {        
        Storage.InItem(product.id, finalProductionAmount + placeBuffValue + heroSkillCollectPower);

        //Debug.Log(placeID + " 생산" + product.id + " , " + finalProductionAmount + "/" + Storage.GetItemStoredAmount(product).ToStringABC());
        if (onProduce != null)
            onProduce();
    }

    /// <summary> 생산 시간 </summary>
    public float productionTime
    {
        get
        {
            if (product == null)
                return 5f;

            return product.productionTime;
        }
    }
    

    Item _product;
    public Item product
    {
        get
        {
            return _product;
        }
        private set
        {
            _product = value;
        }
    }
    /// <summary> 영웅 교체됬을 때 발생 </summary>
    public SimpleDelegate onChangedHeroList;

    /// <summary> 배치 된 영웅 리스트 </summary>
    public CustomList<HeroData> heroList = new CustomList<HeroData>();

    /// <summary> 영웅 교체 </summary>
    public void ChangeDeployHeroList(string LineID, List<string> deployHeroIDList)
    {
        if (LineID == placeID)
        {
            for (int i = 0; i < heroList.Count; i++)
            {
                heroList[i].onChangedValue -= OnChangedHeroData;
                heroList[i].placeID = "";
            }
            // 초기화 후 새로 삽입
            heroList.Clear();

            for (int i = 0; i < deployHeroIDList.Count; i++)
            {
                HeroData hero = HeroManager.heroDataDic[deployHeroIDList[i]];
                hero.onChangedValue += OnChangedHeroData;
                hero.placeID = LineID;
                heroList.Add(hero);
            }
            CalculateProductinAmount();
        }
        else
        {
            // 동일 ID 체크 후 제거 
            for (int i = 0; i < deployHeroIDList.Count; i++)
            {
                HeroData hero = heroList.Find(x => x.id == deployHeroIDList[i]);
                if (hero != null)
                {
                    hero.onChangedValue -= OnChangedHeroData;
                    heroList.Remove(hero);
                }
            }
        }

        // 저장
        List<string> heroIDList = new List<string>();
        for (int i = 0; i < heroList.Count; i++)
        {

            heroIDList.Add(heroList[i].id);
        }

        //for (int i = 0; i < heroList.Count; i++)
        //{
        //    heroList[i].baseData.territorySkillDataList[0].
        //}

        string saveData = JsonMapper.ToJson(heroIDList);
        ObscuredPrefs.SetString(placeID + saveKey, saveData);

        if (onChangedHeroList != null)
            onChangedHeroList();
    }
}
