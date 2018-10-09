using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KingdomManagement;

public class TerritorySkillController {

    public TerritoryHeroDeployState deployState { get; private set; }
    
    public TerritorySkillController(ProductionData data)
    {
        deployState = TerritoryHeroDeployState.Production;
        productionData = data;
    }
    public TerritorySkillController(PlaceData data)
    {
        deployState = TerritoryHeroDeployState.Collect;
        placeData = data;
    }

    public ProductionData productionData { get; private set; }

    public PlaceData placeData { get; private set; }



    /// <summary> 영웅 스킬중 생산력 증가량 </summary>
    public double heroSkillProductionPower { get; private set; }
    /// <summary> 영웅 스킬중 채집력 증가량 </summary>
    public double heroSkillCollectPower { get; private set; }

    /// <summary> 영웅 스킬중 시민이 주는 경험치 증가량 </summary>
    public double heroSkillCitizenExpValue { get; private set; }
    /// <summary> 영웅 스킬중 필터별 시민이 주는 경험치 증가량 </summary>
    public Dictionary<string, double> heroSkillCitizenExpValueDic = new Dictionary<string, double>();

    /// <summary> 영웅 스킬중 시민이 주는 세금 증가량 </summary>
    public double heroSkillCitizenTaxValue { get; private set; }
    /// <summary> 영웅 스킬중 필터별 시민이 주는 세금 증가량 </summary>
    public Dictionary<string, double> heroSkillCitizenTaxValueDic = new Dictionary<string, double>();

    /// <summary> 시민이 주는 세금 2배 획득 확률 </summary>
    public float probabilityCitizenTaxDouble { get; private set; }


    public void CalculateHeroSkillPower()
    {
        if(deployState == TerritoryHeroDeployState.Production)
        {
            Item product = productionData.product;
            List<HeroData> heroList = productionData.heroList;
            CalculateHeroSkillProductionPower(heroList, product);
        }
        else if(deployState == TerritoryHeroDeployState.Collect)
        {
            Item product = placeData.product;
            List<HeroData> heroList = placeData.heroList;
            CalculateHeroSkillCollectPower(heroList, product);
        }

    }

    void CalculateHeroSkillProductionPower(List<HeroData> heroList, Item product)
    {
        double _heroSkillProductionPower = 0;
        double _heroSkillExpValue = 0;
        double _heroSkillTaxValue = 0;
        double _probabilityCitizenTaxDouble = 0;
        heroSkillCitizenExpValueDic.Clear();
        heroSkillCitizenTaxValueDic.Clear();
        for (int i = 0; i < heroList.Count; i++)
        {
            HeroData hero = heroList[i];
            List<TerritorySkillData> skilList = heroList[i].baseData.territorySkillDataList;
            for (int j = 0; j < skilList.Count; j++)
            {
                TerritorySkillData skill = skilList[j];
                if (skill.deployState != TerritoryHeroDeployState.Production && skill.deployState != TerritoryHeroDeployState.All)
                    continue;

                if (skill.applyType == "Production")
                {
                    if (fillterChecker(skill, product) == false)
                        continue;
                    _heroSkillProductionPower += CalculateHeroSkillPower(hero, skill);
                }
                else if (skill.applyType == "ProductionForProductLevel")
                {
                    if (fillterChecker(skill, product) == false)
                        continue;

                    _heroSkillProductionPower += CalculateHeroSkillPower(hero, skill, product.level);
                }
                else if (skill.applyType == "ProductionForSameGradeHero")
                {
                    _heroSkillProductionPower += CalculateProductionPowerForSameGradeHero(hero, skill);
                }
                else if (skill.applyType == "CitizenExp")
                {
                    _heroSkillExpValue += CalculateCitizenExp(hero, skill);
                }
                else if (skill.applyType == "CitizenTax")
                {
                    _heroSkillTaxValue += CalculateCitizenTax(hero, skill);
                }
                else if(skill.applyType == "CitizenDoubleTaxProbability")
                {
                    _probabilityCitizenTaxDouble += CalculateHeroSkillPower(hero, skill);
                }
            }
        }
        probabilityCitizenTaxDouble = (float)_probabilityCitizenTaxDouble;
        heroSkillCitizenExpValue = _heroSkillExpValue;
        heroSkillCitizenTaxValue = _heroSkillTaxValue;
        heroSkillProductionPower = _heroSkillProductionPower;

    }
    int maxTier = 5;
    public void CalculateHeroSkillCollectPower(List<HeroData> heroList, Item product)
    {
        double _heroSkillCollectPower = 0;
        double _heroSkillExpValue = 0;
        double _heroSkillTaxValue = 0;
        double _probabilityCitizenTaxDouble = 0;
        heroSkillCitizenExpValueDic.Clear();
        heroSkillCitizenTaxValueDic.Clear();
        for (int i = 0; i < heroList.Count; i++)
        {
            HeroData hero = heroList[i];
            List<TerritorySkillData> skilList = heroList[i].baseData.territorySkillDataList;
            for (int j = 0; j < skilList.Count; j++)
            {
                TerritorySkillData skill = skilList[j];
                if (skill.deployState != TerritoryHeroDeployState.Collect && skill.deployState != TerritoryHeroDeployState.All)
                    continue;

                if (skill.applyType == "Collect")
                {
                    if (fillterChecker(skill,product) == false)
                        continue;
                    _heroSkillCollectPower += CalculateHeroSkillPower(hero, skilList[j]);
                }
                else if (skill.applyType == "CollectForPlaceTierLong")
                {
                    if (fillterChecker(skill,product) == false)
                        continue;
                    int placeTier = placeData.placeBaseData.placeTier;

                    _heroSkillCollectPower += CalculateHeroSkillPower(hero, skilList[j], placeTier);
                }
                else if (skill.applyType == "CollectForPlaceTierShort")
                {
                    if (fillterChecker(skill,product) == false)
                        continue;
                    int placeTier = (maxTier + 1) - placeData.placeBaseData.placeTier;

                    _heroSkillCollectPower += CalculateHeroSkillPower(hero, skilList[j], placeTier);
                }
                else if (skill.applyType == "CitizenExp")
                {
                    _heroSkillExpValue += CalculateCitizenExp(hero, skill);
                }
                else if (skill.applyType == "CitizenTax")
                {
                    _heroSkillTaxValue += CalculateCitizenTax(hero, skill);
                }
                else if (skill.applyType == "CitizenDoubleTaxProbability")
                {
                    _probabilityCitizenTaxDouble += CalculateHeroSkillPower(hero, skill);
                }
            }
        }
        probabilityCitizenTaxDouble = (float)_probabilityCitizenTaxDouble;
        heroSkillCitizenExpValue = _heroSkillExpValue;
        heroSkillCitizenTaxValue = _heroSkillTaxValue;
        heroSkillCollectPower = _heroSkillCollectPower;

    }

    bool fillterChecker(TerritorySkillData skill,Item product)
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

    /// <summary> 영웅 스킬 파워 계산 </summary>
    double CalculateHeroSkillPower(HeroData hero, TerritorySkillData skill, int productLevel = 1)
    {
        string s = skill.formula;
        int formula = 0;
        int.TryParse(s, out formula);
        int level = (1 + (hero.rebirth * 100) + hero.enhance);

        double collectPower = formula * level * productLevel;
        return collectPower;
    }
    /// <summary> 같은 카테고리에 배치된 영웅중 같은 등급인 영웅이 존재할 때 생산력 증가 </summary>
    double CalculateProductionPowerForSameGradeHero(HeroData hero, TerritorySkillData skill)
    {
        int count = 0;
        for (int i = 0; i < ProductManager.Instance.productionLineDataList.Count; i++)
        {
            ProductionData line = ProductManager.Instance.productionLineDataList[i];
            if (line.product != null && line.product.category == skill.fillterCategory)
            {
                for (int j = 0; j < line.heroList.Count; j++)
                {
                    if (hero.id == line.heroList[j].id)
                        continue;

                    if (hero.heroGrade == line.heroList[j].heroGrade)
                        count++;
                }
            }
        }
        if (count > 0)
            count = 1;

        string s = skill.formula;
        int formula = 0;
        int.TryParse(s, out formula);
        int level = (1 + (hero.rebirth * 100) + hero.enhance);

        double power = formula * level * count;
        return power;
    }
    /// <summary> 시민에게 획득하는 왕국 경험치량 계산 </summary>
    double CalculateCitizenExp(HeroData hero, TerritorySkillData skill)
    {
        double result = 0;

        if (string.IsNullOrEmpty(skill.fillterCategory) == false)
        {
            string key = skill.fillterCategory;
            if (heroSkillCitizenExpValueDic.ContainsKey(key))
                heroSkillCitizenExpValueDic[key] += CalculateHeroSkillPower(hero, skill);
            else
                heroSkillCitizenExpValueDic.Add(key, CalculateHeroSkillPower(hero, skill));
        }
        else if (string.IsNullOrEmpty(skill.fillterItem) == false)
        {
            string key = skill.fillterItem;
            if (heroSkillCitizenExpValueDic.ContainsKey(key))
                heroSkillCitizenExpValueDic[key] += CalculateHeroSkillPower(hero, skill);
            else
                heroSkillCitizenExpValueDic.Add(key, CalculateHeroSkillPower(hero, skill));

        }
        else
        {
            result = CalculateHeroSkillPower(hero, skill);
        }

        return result;
    }

    /// <summary> 시민에게 획득하는 세금량 계산 </summary>
    double CalculateCitizenTax(HeroData hero, TerritorySkillData skill)
    {
        double result = 0;

        if (string.IsNullOrEmpty(skill.fillterCategory) == false)
        {
            string key = skill.fillterCategory;
            if (heroSkillCitizenTaxValueDic.ContainsKey(key))
                heroSkillCitizenTaxValueDic[key] += CalculateHeroSkillPower(hero, skill);
            else
                heroSkillCitizenTaxValueDic.Add(key, CalculateHeroSkillPower(hero, skill));

        }
        else if (string.IsNullOrEmpty(skill.fillterItem) == false)
        {
            string key = skill.fillterItem;
            if (heroSkillCitizenTaxValueDic.ContainsKey(key))
                heroSkillCitizenTaxValueDic[key] += CalculateHeroSkillPower(hero, skill);
            else
                heroSkillCitizenTaxValueDic.Add(key, CalculateHeroSkillPower(hero, skill));
        }
        else
        {
            result = CalculateHeroSkillPower(hero, skill);
        }

        return result;
    }
}
