using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface IHeroSlotUpdatable  {

    void UpdateContents();
}

public class HeroSlotUpdatableDefault : IHeroSlotUpdatable
{
    UIHeroSlot heroSlot;
    public HeroSlotUpdatableDefault(UIHeroSlot _heroSlot)
    {
        heroSlot = _heroSlot;
    }

    public void UpdateContents()
    {
        if (heroSlot.heroData.heroType == HeroData.HeroType.Battle)
        {
            HeroData heroDataBattle = heroSlot.heroData;
            if (heroDataBattle != null && string.IsNullOrEmpty(heroDataBattle.battleGroupID) == false)
            {
                Text text = heroSlot.BattlePanel.GetComponentInChildren<Text>();
                text.text = "전투중";
                heroSlot.BattlePanel.SetActive(true);
            }
            else
                heroSlot.BattlePanel.SetActive(false);
        }
        else
        {
            if (string.IsNullOrEmpty(heroSlot.heroData.placeID) == false)
            {
                string jobPlace = string.Empty;
                if(GameDataManager.placeBaseDataDic.ContainsKey(heroSlot.heroData.placeID))
                {
                    jobPlace = GameDataManager.placeBaseDataDic[heroSlot.heroData.placeID].name;
                }
                else
                {
                    jobPlace = GameDataManager.productionLineBaseDataDic[heroSlot.heroData.placeID].name;
                }

                Text text = heroSlot.BattlePanel.GetComponentInChildren<Text>();
                text.text = jobPlace;
                heroSlot.BattlePanel.SetActive(true);
            }
            else
                heroSlot.BattlePanel.SetActive(false);
        }
    }
}

public class HeroSlotUpdatableInventory : IHeroSlotUpdatable
{
    UIHeroSlot heroSlot;
    public HeroSlotUpdatableInventory(UIHeroSlot _heroSlot)
    {
        heroSlot = _heroSlot;
        if (heroSlot.AddPanel.activeSelf)
            heroSlot.AddPanel.SetActive(false);
    }

    public void UpdateContents()
    {
        if (heroSlot.heroData.heroType == HeroData.HeroType.Battle)
        {
            HeroData heroDataBattle = heroSlot.heroData;
            if (heroDataBattle != null && string.IsNullOrEmpty(heroDataBattle.battleGroupID) == false)
            {
                Text text = heroSlot.BattlePanel.GetComponentInChildren<Text>();
                text.text = "전투중";
                heroSlot.BattlePanel.SetActive(true);
            }
            else
                heroSlot.BattlePanel.SetActive(false);
        }
        else
        {
            if (string.IsNullOrEmpty(heroSlot.heroData.placeID) == false)
            {
                Text text = heroSlot.BattlePanel.GetComponentInChildren<Text>();
                text.text = "일하는중";
                heroSlot.BattlePanel.SetActive(true);
            }
            else
                heroSlot.BattlePanel.SetActive(false);
        }
        
    }
}

public class HeroSlotUpdatableLimitBreak : IHeroSlotUpdatable
{
    public void UpdateContents()
    {
    }
}

public class HeroSlotUpdatableTraining : IHeroSlotUpdatable
{
    UIHeroSlot heroSlot;
    public HeroSlotUpdatableTraining(UIHeroSlot _heroSlot)
    {
        heroSlot = _heroSlot;
    }

    public void UpdateContents()
    {
        if (HeroTrainingManager.Instance != null)
        {
            heroSlot.BattlePanel.SetActive(false);

            if (HeroManager.heroDataDic[heroSlot.id].isTraining)
            {
                Text text = heroSlot.BattlePanel.GetComponentInChildren<Text>();
                text.text = "훈련중";
                heroSlot.BattlePanel.SetActive(true);
            }

        }
    }
}

public class HeroSlotUpdatableTerritory : IHeroSlotUpdatable
{
    public void UpdateContents()
    {
    }
}

public class HeroSlotUpdatableBattle : IHeroSlotUpdatable
{
    public void UpdateContents()
    {
    }
}