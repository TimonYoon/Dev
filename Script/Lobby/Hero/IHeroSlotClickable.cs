using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public interface IHeroSlotClickable {

    void OnClick();
}

public class HeroSlotClickableInventory : IHeroSlotClickable
{
    HeroData heroData;
    public delegate void OpenHeroInfoCallback(HeroData heroData);
    OpenHeroInfoCallback OnOpenHeroInfo;
    public HeroSlotClickableInventory(HeroData _heroData, UIHeroSlotContainer _heroSlotContainer)
    {
        heroData = _heroData;
        OnOpenHeroInfo += _heroSlotContainer.ShowHeroInfo;
    }
    
    public void OnClick()
    {
        OnOpenHeroInfo(heroData);
    }
}

public class HeroSlotClickableLimitBreak : IHeroSlotClickable
{
    string id;
    UIHeroSlotContainer heroSlotContainer;
    public HeroSlotClickableLimitBreak(string _id, UIHeroSlotContainer _heroSlotContainer)
    {
        id = _id;
        heroSlotContainer = _heroSlotContainer;
    }
    public void OnClick()
    {
        if (!UILimitBreak.Instance.sacrificeHeroList.Contains(id))
        {
            for (int i = 0; i < HeroManager.heroDataDic[id].trainingDataList.Count; i++)
            {
                if(HeroManager.heroDataDic[id].trainingDataList[i].training > 0)
                {
                    UIPopupManager.ShowYesNoPopup("경고", "훈련되어 있는 영웅을 선택하셨습니다\n재료로 사용될 시 훈련 수치는 계승되지 않습니다\n계속 하시겠습니까", ResultLimitBreakChoice);
                    break;
                }
            }

            if (heroSlotContainer)
                heroSlotContainer.isSelectedToLimitBreak = true;
            
            UILimitBreak.Instance.sacrificeHeroList.Add(id);
            UILimitBreak.Instance.OnChangedTrainingMaxValue();

        }
        else
        {
            if (heroSlotContainer)
                heroSlotContainer.isSelectedToLimitBreak = false;
            
            UILimitBreak.Instance.sacrificeHeroList.Remove(id);
            UILimitBreak.Instance.OnChangedTrainingMaxValue();

        }
    }

    void ResultLimitBreakChoice(string result)
    {
        if(result == "no")
        {
            if (heroSlotContainer)
                heroSlotContainer.isSelectedToLimitBreak = false;

            UILimitBreak.Instance.sacrificeHeroList.Remove(id);
            UILimitBreak.Instance.OnChangedTrainingMaxValue();
            return;
        }
    }
}

public class HeroSlotClickableTraining : IHeroSlotClickable
{

    UIHeroSlotContainer heroSlotContainer;

    public HeroSlotClickableTraining(UIHeroSlotContainer _heroSlotContainer)
    {
        heroSlotContainer = _heroSlotContainer;
    }
    public void OnClick()
    {
        if (UIHeroTraining.Instance == null)
            return;

        for (int i = 0; i < UIHeroInventory.heroSlotContainerList.Count; i++)
        {
            if (UIHeroInventory.heroSlotContainerList[i].heroInvenID == heroSlotContainer.heroInvenID)
                continue;
            UIHeroInventory.heroSlotContainerList[i].isSelectedToHero = false;
        }

        //HeroData heroDataBattle = heroSlotContainer.heroData;
        //if (heroDataBattle.heroType == HeroData.HeroType.Battle)
        //{
        //    if (string.IsNullOrEmpty(heroDataBattle.battleGroupID) == false)
        //        return;
        //}

        //if (string.IsNullOrEmpty(heroSlotContainer.heroData.placeID) == false)
        //    return;

        
        if (!heroSlotContainer.isSelectedToHero)
        {
            //UIBattlePreparation.AddHero(heroDataBattle);

            if (heroSlotContainer)
                heroSlotContainer.isSelectedToHero = true;
            UIHeroTraining.Instance.trainingStat = 100;
            UIHeroTraining.Instance.SelectTrainingHero(heroSlotContainer.heroInvenID);
        }
        else
        {
            //UIBattlePreparation.RemoveHero(heroDataBattle);

            if (heroSlotContainer)
                heroSlotContainer.isSelectedToHero = false;
            UIHeroTraining.Instance.trainingStat = 100;
            UIHeroTraining.Instance.SelectTrainingHero("");
        }
    }
}

public class HeroSlotClickableTerritory : IHeroSlotClickable
{
    UIHeroSlotContainer heroSlotContainer;
    string id;

    public HeroSlotClickableTerritory(UIHeroSlotContainer _heroSlotContainer)
    {
        heroSlotContainer = _heroSlotContainer;
        id = heroSlotContainer.heroInvenID;
    }

    public void OnClick()
    {

        if (!heroSlotContainer.isSelectedToTerritory)
        {
            if (string.IsNullOrEmpty(HeroManager.heroDataDic[id].placeID) == false && UIDeployHeroInfo.Instance.currentPlaceID != HeroManager.heroDataDic[id].placeID)
            {
                string placeID = HeroManager.heroDataDic[id].placeID;

                string name = placeID;
                if (GameDataManager.productionLineBaseDataDic.ContainsKey(placeID))
                    name = GameDataManager.productionLineBaseDataDic[placeID].name;
                else if (GameDataManager.placeBaseDataDic.ContainsKey(placeID))
                    name = GameDataManager.placeBaseDataDic[placeID].name;

                UIPopupManager.ShowYesNoPopup("경고", "해당영웅은 " + name + "에서 활동중입니다. 배치하시겠습니까?", Result);
                return;
            }

            heroSlotContainer.isSelectedToTerritory = true;

            UIDeployHeroInfo.Instance.AddHero(id);
        }
        else
        {
            heroSlotContainer.isSelectedToTerritory = false;

            UIDeployHeroInfo.Instance.RemoveHero(id);
        }
    }

    void Result(string result)
    {
        if (result == "yes")
        {

            heroSlotContainer.isSelectedToTerritory = true;

            UIDeployHeroInfo.Instance.AddHero(id);

        }
    }
}

public class HeroSlotClickableBattle : IHeroSlotClickable
{
    UIHeroSlotContainer heroSlotContainer;
    HeroData heroData;

    public HeroSlotClickableBattle(UIHeroSlotContainer _heroSlotContainer, HeroData _heroData)
    {
        heroSlotContainer = _heroSlotContainer;
        heroData = _heroData;

        heroSlotContainer.isSelectedToBattle = false;
    }

    public void OnClick()
    {
        if (heroData.heroType != HeroData.HeroType.Battle)
            return;

        string battleGroupID = heroData.battleGroupID;

        if (!string.IsNullOrEmpty(battleGroupID))
        {
            //새로운 전투 그룹 만들 때에는 다른 전투 그룹에 있는 애들 출전 제외 불가
            if (UIBattlePreparation.battleGroupID != Battle.currentBattleGroup.battleType.ToString())
                return;

            //기존 전투 그룹 편집 중에는 기존 전투 그룹에 있는 애들 이외에는 출전 제외 불가
            if (Battle.currentBattleGroup && battleGroupID != Battle.currentBattleGroup.battleType.ToString())
                return;
        }
        

        if (!heroSlotContainer.isSelectedToBattle)
        {
            UIBattlePreparation.AddHero(heroData);
        }
        else
        {
            UIBattlePreparation.RemoveHero(heroData);
        }
    }
}