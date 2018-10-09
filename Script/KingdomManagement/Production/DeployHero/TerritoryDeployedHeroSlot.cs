using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary> (생산라인)내정에 배치된 영웅슬롯 </summary>
public class TerritoryDeployedHeroSlot : MonoBehaviour {

    public GameObject heroSlotPanel;

    public Image imageHero;

    public GameObject enhancePanel;

    public Text textEnhance;

    public GameObject rebirthPanel;

    public Text textRebirth;

    public void InitSlot(HeroData heroData = null)
    {
        heroSlotPanel.SetActive(heroData != null);

        if(heroData != null)
        {
            AssetLoader.AssignImage(imageHero, "sprite/hero", "Atlas_HeroImage", heroData.baseData.image);

            enhancePanel.SetActive(false);
            rebirthPanel.SetActive(false);
            if (heroData.enhance > 0)
            {
                enhancePanel.SetActive(true);
                textEnhance.text = heroData.enhance.ToString();
            }
            if(heroData.rebirth > 0)
            {
                rebirthPanel.SetActive(true);
                textRebirth.text = heroData.rebirth.ToString();
            }
        }
    }
}
