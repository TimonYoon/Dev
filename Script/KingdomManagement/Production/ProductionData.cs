using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using LitJson;
using KingdomManagement;
using System.Reflection;
using CodeStage.AntiCheat.ObscuredTypes;

/// <summary> 생산 라인 데이타 </summary>
public class ProductionData
{
    public bool isInitialized;
    
    public ProductionData(ProductionLineBaseData data)
    {
        skillCT = new TerritorySkillController(this);

        baseData = data;
        productionLineID = baseData.id;

        
        heroList.onAdd += OnAddHero;
        heroList.onRemovePost += OnRemoveHero;

        ProductManager.onChangedDeployHeroList += OnChangedDeployHeroList;
        ProductManager.onProductManagerInitialized += OnProductManagerInitialized;
        if (ObscuredPrefs.HasKey(productionLineID + saveKey))
        {
            string _data = ObscuredPrefs.GetString(productionLineID + saveKey);
            if(string.IsNullOrEmpty(_data))
            {
                return;
            }
            List<string> deployHeroIDList = JsonMapper.ToObject<List<string>>(new JsonReader(_data));
            
            for (int i = 0; i < deployHeroIDList.Count; i++)
            {
                HeroData heroData = HeroManager.heroDataDic[deployHeroIDList[i]];
                heroData.onChangedValue += OnChangedHeroData;
                heroData.placeID = productionLineID;
                heroList.Add(heroData);                
            }
        }

        saveKeyApllyProduct = productionLineID + saveKeyApllyProduct + User.Instance.userID;

        if (ObscuredPrefs.HasKey(saveKeyApllyProduct))
        {
            string productID = ObscuredPrefs.GetString(saveKeyApllyProduct);
            product = ProductManager.Instance.productList.Find(x => x.id == productID);
        }

        Storage.storedItemDic.onAdd += OnAddItemData;

        isInitialized = true;
    }

    string saveKey = "_DeployHero_" + User.Instance.userID;

    public string productionLineID { get; private set; }

    public ProductionLineBaseData baseData { get; private set; }
    
    /// <summary> 생산에 필요한 소비 완료했는가? </summary>
    public bool isMaterialConsumption { get; private set; }

    public SimpleDelegate onChangedProduct;

    void OnProductManagerInitialized()
    {
        CalculateProductinAmount();
    }

    void OnChangedDeployHeroList()
    {
        CalculateProductinAmount();
    }

    void OnAddHero(HeroData hero)
    {
        CalculateProductinAmount();
    }

    void OnRemoveHero(HeroData hero)
    {
        CalculateProductinAmount();
    }
    
    
    void MaterialConsumption()
    {
        if (product == null)
            return;

        if (isMaterialConsumption)
            return;
        
        if (product.ingredientList.Count > 0)
        {
            isMaterialConsumption = true;
            for (int i = 0; i < product.ingredientList.Count; i++)
            {
                double dd = Storage.OutItem(product.ingredientList[i].item, product.ingredientList[i].count * totalValue);
            }
            
            
        }
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
        }
    }
    
    public double placeBuffValue
    {
        get
        {
            if (product == null)
                return 0;
            
            return finalProductionAmount *( product.placeBuffValue/100);
        }
    }

    public SimpleDelegate onChangedProductionAmount;

    void OnChangedHeroData(PropertyInfo p)
    {
        if (p.Name == "enhance" || p.Name == "rebirth" || p.Name == "level")
        {
            CalculateProductinAmount();
            //CalculatePlaceBuffValue();
        }
    }

    public List<TerritorySkillData> deploedHeroSkillList = new List<TerritorySkillData>();

    void ApplyHeroSkill()
    {
        deploedHeroSkillList.Clear();
        if (heroList == null)
            return;
        
        for (int i = 0; i < heroList.Count; i++)
        {
            List<TerritorySkillData> skilList = heroList[i].baseData.territorySkillDataList;
            ApplySkill(skilList);
        }
    }
    void ApplySkill(List<TerritorySkillData> skilList)
    {
        for (int i = 0; i < skilList.Count; i++)
        {
            if (skilList[i].deployState == TerritoryHeroDeployState.Production)
            {
                deploedHeroSkillList.Add(skilList[i]);
            }
            else if(skilList[i].deployState == TerritoryHeroDeployState.All)
            {
                deploedHeroSkillList.Add(skilList[i]);
            }
        }
    }
   

    public double heroSkillCollectPower { get { return finalProductionAmount * (skillCT.heroSkillProductionPower * 0.01f); } }

    /// <summary> 영웅 스킬중 시민이 주는 경험치 증가량 </summary>
    public double heroSkillCitizenExpValue { get { return skillCT.heroSkillCitizenExpValue; } }
    /// <summary> 영웅 스킬중 필터별 시민이 주는 경험치 증가량 </summary>
    public Dictionary<string, double> heroSkillCitizenExpValueDic { get { return skillCT.heroSkillCitizenExpValueDic; } }

    /// <summary> 영웅 스킬중 시민이 주는 세금 증가량 </summary>
    public double heroSkillCitizenTaxValue { get { return skillCT.heroSkillCitizenTaxValue; } }
    /// <summary> 영웅 스킬중 필터별 시민이 주는 세금 증가량 </summary>
    public Dictionary<string, double> heroSkillCitizenTaxValueDic { get { return skillCT.heroSkillCitizenTaxValueDic; } }

    public TerritorySkillController skillCT;

    void CalculateProductinAmount()
    {
        //ApplyHeroSkill();

        productionPower = 0f;
        if (heroList != null)
            productionPower = heroList.Sum(x => x.productivePower);

        double baseValue = 0;

        if (product != null)
            baseValue = product.productionAmount;

        finalProductionAmount = baseValue * (1d + productionPower * 0.01d);

        skillCT.CalculateHeroSkillPower();

        totalValue = finalProductionAmount + placeBuffValue + heroSkillCollectPower;

        
    }

    public double productionPower = 0d;

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

            if(isChanged && onChangedProduceStatus != null)
                    onChangedProduceStatus();

            //Enqueue, Dequeue 할 때 걸어주는게 안전하긴 함..
        }
    }

    /// <summary> 생산될 때 콜백 </summary>
    public SimpleDelegate onProduce;
    /// <summary> 생산 </summary>
    public IEnumerator Produce()
    {
        progressValue = 0;
        while (isMaterialConsumption == false)
            yield return null;

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
            Produce(product);
           
        }


        coroutineProduce = null;
        progressValue = 0f;

    }

    void Produce(Item product)
    {
        Storage.InItem(product.id, totalValue);
        if (onProduce != null)
            onProduce();
        isMaterialConsumption = false;

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

    void LocalSave()
    {
        if (product == null)
            ObscuredPrefs.DeleteKey(saveKeyApllyProduct);
        else
            ObscuredPrefs.SetString(saveKeyApllyProduct, product.id);
    }
    string saveKeyApllyProduct = "_ApplyProduct_";

    Item _product;
    public Item product
    {
        get
        {
            return _product;
        }
        set
        {
            bool isChanged = _product != value;

            if (!isChanged)
                return;
            
            if(_product != null)
            {
                isMaterialConsumption = false;
                _product.onChangedLevel -= CalculateProductinAmount;
                for (int i = 0; i < _product.ingredientList.Count; i++)
                {
                    Storage.UnregisterOnChangedStoredAmountCallback(_product.ingredientList[i].itemID, OnChangedIngredientAmount);
                    //Debug.Log("해제" + _product.ingredientList[i].itemID);

                }
            }
                

            _product = value;

            if (_product != null)
            {
                _product.onChangedLevel += CalculateProductinAmount;
                for (int i = 0; i < _product.ingredientList.Count; i++)
                {
                    Storage.RegisterOnChangedStoredAmountCallback(_product.ingredientList[i].itemID, OnChangedIngredientAmount);
                    //Debug.Log("등록" + _product.ingredientList[i].itemID);
                }

            }
                

            if (isChanged)
            {
                CalculateProductinAmount();
                LocalSave();
                if (onChangedProduct != null)
                    onChangedProduct();
            }                
        }
    }

    void OnAddItemData(string itemID)
    {
        if (product == null)
            return;
        for (int i = 0; i < product.ingredientList.Count; i++)
        {
            if(itemID == product.ingredientList[i].itemID)
                Storage.RegisterOnChangedStoredAmountCallback(_product.ingredientList[i].itemID, OnChangedIngredientAmount);
            //Debug.Log("등록" + _product.ingredientList[i].itemID);
        }
    }

    void OnChangedIngredientAmount()
    {
        bool isFill = true;
        for (int i = 0; i < product.ingredientList.Count; i++)
        {
            if(Storage.GetItemStoredAmount(product.ingredientList[i].item) < product.ingredientList[i].count * totalValue)
            {
                isFill = false;
            }
        }

        if (isFill)
            MaterialConsumption();
    }

    double _totalValue = 0;
    public double totalValue
    {
        get { return _totalValue; }
        private set
        {
            bool isChanged = _totalValue != value;

           _totalValue = value;

            if(isChanged && onChangedProductionAmount != null)
                onChangedProductionAmount();
        }
    }

    /// <summary> 영웅 교체됬을 때 발생 </summary>
    public SimpleDelegate onChangedHeroList;

    /// <summary> 배치 된 영웅 리스트 </summary>
    public CustomList<HeroData> heroList = new CustomList<HeroData>();



    /// <summary> 영웅 교체 </summary>
    public void ChangeDeployHeroList(string LineID, List<string> deployHeroIDList)
    {
        if (LineID == productionLineID)
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
        }
        else
        {
            // 동일 ID 체크 후 제거 
            for (int i = 0; i < deployHeroIDList.Count; i++)
            {
                HeroData hero = heroList.Find(x => x.id == deployHeroIDList[i]);
                if(hero != null)
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

        //CalculateProductinAmount();

        string saveData = JsonMapper.ToJson(heroIDList);
        ObscuredPrefs.SetString(productionLineID + saveKey, saveData);

        if (onChangedHeroList != null)
            onChangedHeroList();
    }

}
