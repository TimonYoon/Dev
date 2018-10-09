using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class BattlePvPHeroSlot : MonoBehaviour
{

    public GameObject heroSlotPanel;

    public Image imageHero;

    public GameObject enhancePanel;

    public Text textEnhance;

    public GameObject rebirthPanel;

    public Text textRebirth;

    public Image gradeFrame;

    public List<Sprite> gradeSpriteList = new List<Sprite>();

    public void InitSlot(HeroData heroData = null)
    {
        heroSlotPanel.SetActive(heroData != null);

        if (heroData != null)
        {
            AssetLoader.AssignImage(imageHero, "sprite/hero", "Atlas_HeroImage", heroData.baseData.image);

            enhancePanel.SetActive(false);
            rebirthPanel.SetActive(false);
            if (heroData.enhance > 0)
            {
                enhancePanel.SetActive(true);
                textEnhance.text = heroData.enhance.ToString();
            }
            if (heroData.rebirth > 0)
            {
                rebirthPanel.SetActive(true);
                textRebirth.text = heroData.rebirth.ToString();
            }

            Sprite frame = null;
            for (int i = 0; i < gradeSpriteList.Count; i++)
            {
                frame = gradeSpriteList[i];
                if (i == (heroData.heroGrade - 1))
                    break;
                
            }
            if(frame != null)
                gradeFrame.sprite = frame;
        }

    }
}
